using DataTicket.Application.Ports.In.Authentication;
using DataTicket.Application.Ports.Out;

namespace DataTicket.Application.Authentication;

/// <summary>
/// Cierra la sesión del canal actual. No revoca otras copias de la cookie (riesgo aceptado en T-01).
/// </summary>
public sealed class SignOut(IUserSession session) : ISignOut
{
    public Task ExecuteAsync(CancellationToken cancellationToken) => session.EndAsync(cancellationToken);
}
