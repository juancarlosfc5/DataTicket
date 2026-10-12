using DataTicket.Application.Ports.Out;
using Microsoft.AspNetCore.Identity;

namespace DataTicket.Infrastructure.Identity;

/// <summary>
/// Adaptador de <see cref="IUserSession"/>: emite y expira la cookie de Identity (<c>dataticket.auth</c>).
/// La cookie no es persistente: vive mientras el navegador esté abierto y su ticket expira a las 8 h
/// deslizantes (configuración de la cookie en el Api).
/// </summary>
public sealed class IdentityUserSession(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IUserSession
{
    public async Task StartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"No existe la cuenta {userId}.");

        await signInManager.SignInAsync(user, isPersistent: false);
    }

    public Task EndAsync(CancellationToken cancellationToken) => signInManager.SignOutAsync();
}
