namespace DataTicket.Application.Ports.In.Authentication;

/// <summary>
/// Caso de uso: iniciar sesión con correo y contraseña (HU-003).
/// </summary>
public interface ISignIn
{
    Task<SignInOutcome> ExecuteAsync(SignInCommand command, CancellationToken cancellationToken);
}

public sealed record SignInCommand(string Email, string Password);

/// <summary>
/// Resultado sin detalle: el adaptador responde igual a todo fallo para no revelar si la cuenta existe,
/// está bloqueada, pendiente o desactivada (PRD §5).
/// </summary>
public enum SignInOutcome
{
    Succeeded,
    InvalidCredentials,
}
