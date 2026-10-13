using DataTicket.Domain.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataTicket.Infrastructure.Identity;

/// <summary>
/// <see cref="SignInManager{TUser}"/> que, al revalidar el sello de seguridad (cada 5 minutos, T-01), exige
/// además que la cuenta siga <c>Active</c> y con perfil válido: una cuenta desactivada pierde la sesión
/// aunque su sello no haya cambiado.
/// </summary>
public sealed class DataTicketSignInManager(
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor contextAccessor,
    IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
    IOptions<IdentityOptions> optionsAccessor,
    ILogger<SignInManager<ApplicationUser>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<ApplicationUser> confirmation)
    : SignInManager<ApplicationUser>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
{
    public override async Task<bool> ValidateSecurityStampAsync(ApplicationUser? user, string? securityStamp)
    {
        if (!await base.ValidateSecurityStampAsync(user, securityStamp) || user is null)
        {
            return false;
        }

        var roles = await UserManager.GetRolesAsync(user);
        return AccountAccess.CanSignIn(user.AccountStatus, [.. roles], user.CompanyId);
    }
}
