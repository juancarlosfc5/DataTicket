using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Users;
using Microsoft.AspNetCore.Authorization;

namespace DataTicket.Api.Auth;

/// <summary>
/// Políticas de autorización con nombre (contrato T-01 de HU-004) y política de reserva. Son la primera
/// barrera: cada caso de uso autoriza además contra el recurso concreto con <see cref="ICurrentUser"/>.
/// </summary>
/// <remarks>
/// Ninguna concede contenido de tickets: ser <c>Administrator</c> o pertenecer a un equipo no da acceso a un
/// ticket (PRD §5 regla 5, §6.2). Eso lo decide la participación, en los casos de uso de las HU de tickets.
/// </remarks>
public static class AuthorizationPolicies
{
    /// <summary><c>Requester</c> o <c>CompanyCoordinator</c> con claim <c>company_id</c>.</summary>
    public const string ClientUser = "ClientUser";

    /// <summary><c>ProductManager</c>, <c>Administrator</c> o integrante de un equipo.</summary>
    public const string InternalUser = "InternalUser";

    public const string Administrator = "Administrator";

    public const string ProductManagerOrAdministrator = "ProductManagerOrAdministrator";

    /// <summary>
    /// Única tabla de políticas con nombre: de aquí salen el registro en <see cref="AuthorizationOptions"/>,
    /// <see cref="All"/> e <see cref="InternalOnly"/>. Una política nueva se añade solo aquí; una prueba compara
    /// esta tabla con lo que queda registrado en el host.
    /// </summary>
    private static readonly PolicyDefinition[] Definitions =
    [
        new(ClientUser, IsInternalOnly: false, profile => profile.Surface == Surface.Client),
        new(InternalUser, IsInternalOnly: true, profile => profile.Surface == Surface.Internal),
        new(Administrator, IsInternalOnly: true, profile =>
            profile.Surface == Surface.Internal && profile.IsInRole(RoleNames.Administrator)),
        new(ProductManagerOrAdministrator, IsInternalOnly: true, profile =>
            profile.Surface == Surface.Internal
            && (profile.IsInRole(RoleNames.ProductManager) || profile.IsInRole(RoleNames.Administrator))),
    ];

    /// <summary>Nombres de todas las políticas registradas, en el orden de la tabla.</summary>
    public static IReadOnlyList<string> All { get; } = [.. Definitions.Select(definition => definition.Name)];

    /// <summary>Políticas que solo satisface el personal interno (un cliente nunca llega a sus endpoints).</summary>
    public static IReadOnlyList<string> InternalOnly { get; } =
        [.. Definitions.Where(definition => definition.IsInternalOnly).Select(definition => definition.Name)];

    /// <summary>
    /// Registra las políticas con nombre, la política de reserva (todo endpoint sin <c>AllowAnonymous</c>
    /// exige un usuario autenticado) y el adaptador de <see cref="ICurrentUser"/>.
    /// </summary>
    public static IServiceCollection AddDataTicketAuthorization(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        // La política de reserva protege endpoints; una ruta que no existe sigue respondiendo 404.
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, EndpointScopedAuthorizationResultHandler>();

        var authorization = services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        foreach (var definition in Definitions)
        {
            authorization.AddPolicy(definition.Name, policy => Require(policy, definition.Condition));
        }

        return services;
    }

    /// <summary>
    /// Exige sesión y que el perfil de la sesión cumpla la condición. Se evalúa sobre <see cref="PrincipalProfile"/>,
    /// la misma lectura que usa <see cref="ICurrentUser"/>: un perfil no válido no satisface ninguna política.
    /// </summary>
    private static void Require(AuthorizationPolicyBuilder policy, Func<PrincipalProfile, bool> condition) =>
        policy.RequireAuthenticatedUser().RequireAssertion(context => condition(PrincipalProfile.From(context.User)));

    /// <summary>Fila de la tabla de políticas: nombre, si es exclusiva del personal interno y condición sobre el perfil.</summary>
    private sealed record PolicyDefinition(string Name, bool IsInternalOnly, Func<PrincipalProfile, bool> Condition);
}
