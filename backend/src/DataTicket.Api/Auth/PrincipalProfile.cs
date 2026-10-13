using System.Security.Claims;
using DataTicket.Domain.Users;
using DataTicket.Infrastructure.Identity;

namespace DataTicket.Api.Auth;

/// <summary>
/// Lectura única de los claims de la sesión: la comparten el adaptador de <c>ICurrentUser</c> y las políticas
/// con nombre, para que ambos decidan con las mismas reglas. Solo lee el principal: nunca el cuerpo, la
/// consulta, la ruta ni las cabeceras de la petición.
/// </summary>
/// <remarks>
/// Falla cerrado. Si el perfil no es uno de los «Perfiles válidos» del contrato (cliente sin
/// <c>company_id</c>, interno con <c>company_id</c>, mezcla de roles de cliente e internos, <c>company_id</c>
/// repetido o mal formado), <see cref="Surface"/> es <c>null</c> y no se exponen roles, equipos ni empresa.
/// </remarks>
internal sealed record PrincipalProfile(
    Guid? UserId,
    string? DisplayName,
    string? Email,
    Surface? Surface,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Teams,
    Guid? CompanyId)
{
    public static PrincipalProfile Anonymous { get; } = new(null, null, null, null, [], [], null);

    public bool IsAuthenticated => UserId is not null;

    public bool IsInRole(string role) => Roles.Contains(role, StringComparer.Ordinal);

    public static PrincipalProfile From(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true
            || !Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            || userId == Guid.Empty)
        {
            return Anonymous;
        }

        var identified = Anonymous with
        {
            UserId = userId,
            DisplayName = principal.FindFirstValue(DataTicketClaimTypes.DisplayName),
            Email = principal.FindFirstValue(ClaimTypes.Email),
        };

        var granted = principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToHashSet(StringComparer.Ordinal);
        if (!TryReadCompanyId(principal, out var companyId)
            || AccountAccess.ResolveSurface(granted, companyId) is not { } surface)
        {
            return identified;
        }

        return identified with
        {
            Surface = surface,
            Roles = [.. RoleNames.Roles.Where(granted.Contains)],
            Teams = [.. RoleNames.Teams.Where(granted.Contains)],
            CompanyId = companyId,
        };
    }

    /// <summary>
    /// <c>false</c> si el claim <c>company_id</c> está repetido con valores distintos o no es un GUID válido;
    /// si no hay claim, <paramref name="companyId"/> es <c>null</c>.
    /// </summary>
    private static bool TryReadCompanyId(ClaimsPrincipal principal, out Guid? companyId)
    {
        companyId = null;
        var values = principal.FindAll(DataTicketClaimTypes.CompanyId).Select(claim => claim.Value).Distinct(StringComparer.Ordinal).ToList();
        if (values.Count == 0)
        {
            return true;
        }

        if (values.Count > 1 || !Guid.TryParse(values[0], out var parsed) || parsed == Guid.Empty)
        {
            return false;
        }

        companyId = parsed;
        return true;
    }
}
