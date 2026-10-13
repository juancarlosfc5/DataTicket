using DataTicket.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Conventions;

/// <summary>
/// Lectura de la tabla de endpoints del host (<see cref="EndpointDataSource"/>) para las pruebas de convención.
/// Una firma es <c>MÉTODO /ruta</c>; un endpoint sin restricción de método se describe como <c>* /ruta</c>.
/// </summary>
public static class EndpointCatalog
{
    public const string AnyMethod = "*";

    private static readonly string[] MutationMethods =
        [HttpMethods.Post, HttpMethods.Put, HttpMethods.Patch, HttpMethods.Delete];

    public static IReadOnlyList<RouteEndpoint> All(IServiceProvider services) =>
        [.. services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()];

    public static string Route(RouteEndpoint endpoint) => endpoint.RoutePattern.RawText ?? string.Empty;

    /// <summary>Métodos HTTP que admite el endpoint; vacío si no declara ninguno (los admite todos).</summary>
    public static IReadOnlyList<string> Methods(RouteEndpoint endpoint) =>
        endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? [];

    public static IReadOnlyList<string> Signatures(RouteEndpoint endpoint)
    {
        var methods = Methods(endpoint);
        return methods.Count == 0
            ? [$"{AnyMethod} {Route(endpoint)}"]
            : [.. methods.Select(method => $"{method.ToUpperInvariant()} {Route(endpoint)}")];
    }

    /// <summary>
    /// El endpoint responde a una petición sin sesión: lleva <see cref="IAllowAnonymous"/> o, si no hay
    /// política de reserva, no declara ningún requisito de autorización.
    /// </summary>
    public static bool AdmitsAnonymous(RouteEndpoint endpoint, AuthorizationPolicy? fallbackPolicy)
    {
        if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            return true;
        }

        var declaresAuthorization = endpoint.Metadata.GetMetadata<IAuthorizeData>() is not null
            || endpoint.Metadata.GetMetadata<AuthorizationPolicy>() is not null;
        return fallbackPolicy is null && !declaresAuthorization;
    }

    /// <summary>El endpoint puede recibir POST, PUT, PATCH o DELETE (o no restringe el método).</summary>
    public static bool AcceptsMutation(RouteEndpoint endpoint)
    {
        var methods = Methods(endpoint);
        return methods.Count == 0 || methods.Any(method => MutationMethods.Contains(method, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Firmas de las mutaciones que admite el endpoint: una por método que muta, o <c>* /ruta</c> si no
    /// restringe el método. Los métodos de solo lectura de un endpoint mixto no cuentan.
    /// </summary>
    public static IReadOnlyList<string> MutationSignatures(RouteEndpoint endpoint)
    {
        var methods = Methods(endpoint);
        return methods.Count == 0
            ? [$"{AnyMethod} {Route(endpoint)}"]
            :
            [
                .. methods
                    .Where(method => MutationMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
                    .Select(method => $"{method.ToUpperInvariant()} {Route(endpoint)}"),
            ];
    }

    /// <summary>
    /// Mutaciones de CUALQUIER ruta que no pasan por el filtro antiforgery, salvo las firmas de
    /// <paramref name="exemptions"/> (hallazgos BAJO-8 de B3 y BAJO-2 de B4). La regla no mira el prefijo:
    /// una mutación fuera de <c>/api</c> (un webhook, una ruta suelta) también debe llevar el marcador o
    /// estar exenta de forma explícita.
    /// </summary>
    public static IReadOnlyList<string> MutationsWithoutAntiforgery(
        IEnumerable<RouteEndpoint> endpoints, IReadOnlyCollection<string> exemptions) =>
    [
        .. endpoints
            .Where(AcceptsMutation)
            .Where(endpoint => endpoint.Metadata.GetMetadata<AntiforgeryValidationMetadata>() is null)
            .SelectMany(MutationSignatures)
            .Where(signature => !exemptions.Contains(signature, StringComparer.Ordinal)),
    ];

    /// <summary>
    /// El endpoint exige una política que solo satisface el personal interno: un usuario cliente nunca
    /// llega a él. Todos los demás cuentan como «endpoints de cliente».
    /// </summary>
    public static bool IsInternalOnly(RouteEndpoint endpoint) =>
        endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>()
            .Any(data => data.Policy is { } policy && AuthorizationPolicies.InternalOnly.Contains(policy));
}
