using DataTicket.Application.Ports.Out;
using Microsoft.AspNetCore.Identity;

namespace DataTicket.Infrastructure.Identity;

/// <summary>
/// Adaptador de <see cref="ICredentialVerifier"/> sobre Identity: contraseña con <c>PasswordHasher</c> y bloqueo
/// (<c>lockoutOnFailure: true</c>). Cuando no hay contraseña que comparar (correo inexistente, cuenta sin
/// contraseña o ya bloqueada) verifica igualmente un hash señuelo: todo intento cuesta exactamente una
/// verificación, para que el tiempo de respuesta no delate el motivo (PRD §5).
/// </summary>
public sealed class IdentityCredentialVerifier(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IPasswordHasher<ApplicationUser> passwordHasher,
    DecoyPasswordHash decoyPasswordHash) : ICredentialVerifier
{
    public async Task<VerifiedAccount?> VerifyAsync(string email, string password, CancellationToken cancellationToken)
    {
        // Antes de ramificar: el primer intento del proceso paga la generación del señuelo sea cual sea su motivo.
        var decoyHash = decoyPasswordHash.GetOrCreate(passwordHasher);

        var user = await userManager.FindByEmailAsync(email);
        if (user?.PasswordHash is null || await userManager.IsLockedOutAsync(user))
        {
            SpendHashingTime(decoyHash, password);
            return null;
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return new VerifiedAccount(user.Id, user.AccountStatus, user.CompanyId, [.. roles]);
    }

    private void SpendHashingTime(string decoyHash, string password) =>
        _ = passwordHasher.VerifyHashedPassword(new ApplicationUser(), decoyHash, password);
}
