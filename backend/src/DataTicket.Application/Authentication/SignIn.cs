using DataTicket.Application.Ports.In.Authentication;
using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Users;

namespace DataTicket.Application.Authentication;

/// <summary>
/// Inicia sesión solo si la contraseña es correcta (con bloqueo), la cuenta está <c>Active</c> y su perfil es
/// válido. La comprobación ocurre ANTES de emitir la sesión y todo fallo es indistinguible (HU-003 CHU-03/04/05).
/// </summary>
public sealed class SignIn(ICredentialVerifier verifier, IUserSession session) : ISignIn
{
    public async Task<SignInOutcome> ExecuteAsync(SignInCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var account = await verifier.VerifyAsync(command.Email, command.Password, cancellationToken);
        if (account is null || !AccountAccess.CanSignIn(account.Status, account.Roles, account.CompanyId))
        {
            return SignInOutcome.InvalidCredentials;
        }

        await session.StartAsync(account.UserId, cancellationToken);
        return SignInOutcome.Succeeded;
    }
}
