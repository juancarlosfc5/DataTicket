using System.Security.Claims;
using DataTicket.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DataTicket.Infrastructure.Identity;

/// <summary>
/// Fábrica de claims de la cookie de sesión (contrato T-01 de HU-004). Emite una lista cerrada: id, nombre de
/// usuario, correo, sello de seguridad, nombre visible, un <c>Role</c> por rol o equipo y, <b>solo para
/// cuentas cliente</b>, <c>company_id</c>.
/// </summary>
/// <remarks>
/// <para>
/// No hereda de <c>UserClaimsPrincipalFactory</c>, a propósito: la fábrica estándar copia a la cookie todas
/// las filas de <c>identity.user_claims</c> e <c>identity.role_claims</c>, y una fila de tipo rol o
/// <c>company_id</c> escrita a mano elevaría a un usuario, o a un equipo entero, o le cambiaría la empresa
/// (PRD §5 reglas 1 y 5). La aplicación no escribe en esas tablas, así que no se lee nada de ellas: cada
/// claim sale de la cuenta (<see cref="ApplicationUser"/>) o de <c>identity.user_roles</c>.
/// </para>
/// <para>
/// Los roles son los de <c>UserManager.GetRolesAsync</c> filtrados por los roles conocidos
/// (<see cref="RoleNames.All"/>). Falla cerrado: si el perfil no es uno de los «Perfiles válidos»
/// (<see cref="AccountAccess.ResolveSurface"/>), el principal identifica a la cuenta pero no lleva roles ni
/// empresa, de modo que no satisface ninguna política con nombre.
/// </para>
/// <para>
/// Se ejecuta al iniciar sesión y en cada revalidación del sello de seguridad, así que los cambios de rol,
/// equipo o empresa llegan a la cookie en el siguiente intervalo.
/// </para>
/// </remarks>
public sealed class DataTicketClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    IOptions<IdentityOptions> options)
    : IUserClaimsPrincipalFactory<ApplicationUser>
{
    private const string GuidFormat = "D";

    public async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claimTypes = options.Value.ClaimsIdentity;
        var identity = new ClaimsIdentity(
            IdentityConstants.ApplicationScheme, claimTypes.UserNameClaimType, claimTypes.RoleClaimType);

        identity.AddClaim(new Claim(claimTypes.UserIdClaimType, await userManager.GetUserIdAsync(user)));
        AddIfPresent(identity, claimTypes.UserNameClaimType, await userManager.GetUserNameAsync(user));
        AddIfPresent(identity, claimTypes.EmailClaimType, await userManager.GetEmailAsync(user));
        // Lo necesita la revalidación periódica de la sesión (SecurityStampValidator).
        identity.AddClaim(new Claim(claimTypes.SecurityStampClaimType, await userManager.GetSecurityStampAsync(user)));
        AddIfPresent(identity, DataTicketClaimTypes.DisplayName, user.DisplayName);

        var roles = await GetKnownRolesAsync(user);
        if (AccountAccess.ResolveSurface(roles, user.CompanyId) is { } surface)
        {
            identity.AddClaims(roles.Select(role => new Claim(claimTypes.RoleClaimType, role)));
            if (surface == Surface.Client && user.CompanyId is { } companyId)
            {
                identity.AddClaim(new Claim(DataTicketClaimTypes.CompanyId, companyId.ToString(GuidFormat)));
            }
        }

        return new ClaimsPrincipal(identity);
    }

    /// <summary>Roles y equipos de <c>identity.user_roles</c> que el producto conoce, sin repetir.</summary>
    private async Task<IReadOnlyList<string>> GetKnownRolesAsync(ApplicationUser user)
    {
        var assigned = await userManager.GetRolesAsync(user);
        return [.. assigned.Where(RoleNames.All.Contains).Distinct(StringComparer.Ordinal)];
    }

    private static void AddIfPresent(ClaimsIdentity identity, string claimType, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            identity.AddClaim(new Claim(claimType, value));
        }
    }
}
