using DataTicket.Application.Ports.In.Authentication;
using DataTicket.Domain.Users;

namespace DataTicket.Api.Endpoints.Auth;

/// <summary>
/// Respuesta de <c>GET /api/auth/me</c> (contrato T-01 de HU-004). <see cref="Company"/> solo tiene valor si
/// <see cref="Surface"/> es <c>client</c>; para internos viaja como <c>null</c>.
/// </summary>
public sealed record CurrentUserResponse(
    Guid Id,
    string DisplayName,
    string Email,
    string Surface,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Teams,
    CurrentUserCompanyResponse? Company)
{
    public const string ClientSurface = "client";
    public const string InternalSurface = "internal";

    public static CurrentUserResponse From(CurrentUserProfile profile) =>
        new(
            profile.Id,
            profile.DisplayName,
            profile.Email,
            SurfaceLiteral(profile.Surface),
            profile.Roles,
            profile.Teams,
            profile.Company is { } company ? new CurrentUserCompanyResponse(company.Id, company.Name) : null);

    /// <summary>
    /// Literal del contrato para cada superficie. Exhaustivo: una superficie que el Api no conoce falla en
    /// lugar de presentarse como <c>internal</c>.
    /// </summary>
    private static string SurfaceLiteral(Domain.Users.Surface surface) => surface switch
    {
        Domain.Users.Surface.Client => ClientSurface,
        Domain.Users.Surface.Internal => InternalSurface,
        _ => throw new ArgumentOutOfRangeException(nameof(surface), surface, "Superficie sin literal en el contrato de /api/auth/me."),
    };
}

/// <summary>Empresa del usuario cliente.</summary>
public sealed record CurrentUserCompanyResponse(Guid Id, string Name);
