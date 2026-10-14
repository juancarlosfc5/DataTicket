using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Users;

namespace DataTicket.UnitTests.Doubles;

/// <summary>
/// Doble de <see cref="ICurrentUser"/> con la misma forma que el adaptador real: los roles van en
/// <see cref="Roles"/>, los equipos en <see cref="Teams"/> y la empresa solo en clientes. Las fábricas cubren los
/// perfiles que distinguen las reglas de acceso (núcleo N1, HU-020, HU-026 a HU-032).
/// </summary>
public sealed record FakeCurrentUser : ICurrentUser
{
    public static FakeCurrentUser Anonymous { get; } = new();

    public bool IsAuthenticated => UserId is not null;

    public Guid? UserId { get; init; }

    public string? DisplayName { get; init; }

    public string? Email { get; init; }

    public Surface? Surface { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = [];

    public IReadOnlyList<string> Teams { get; init; } = [];

    public Guid? CompanyId { get; init; }

    public bool IsInRole(string role) => Roles.Contains(role);

    /// <summary>Persona interna con sus roles (<c>ProductManager</c>, <c>Administrator</c>) y equipos.</summary>
    public static FakeCurrentUser Internal(Guid userId, IReadOnlyList<string>? roles = null, IReadOnlyList<string>? teams = null) =>
        new()
        {
            UserId = userId,
            Surface = global::DataTicket.Domain.Users.Surface.Internal,
            Roles = roles ?? [],
            Teams = teams ?? [],
        };

    /// <summary>Usuario cliente (<c>Requester</c> o <c>CompanyCoordinator</c>) de una empresa.</summary>
    public static FakeCurrentUser Client(Guid userId, Guid companyId, string role = RoleNames.Requester) =>
        new()
        {
            UserId = userId,
            Surface = global::DataTicket.Domain.Users.Surface.Client,
            Roles = [role],
            CompanyId = companyId,
        };

    /// <summary>
    /// Sesión con un perfil no válido (p. ej. roles de cliente e internos mezclados): el adaptador real falla
    /// cerrado y no expone superficie, roles, equipos ni empresa.
    /// </summary>
    public static FakeCurrentUser InvalidProfile(Guid userId) => new() { UserId = userId };
}
