namespace DataTicket.Application.Ports.In.Authentication;

/// <summary>
/// Caso de uso: cerrar la sesión actual (HU-003). Solo expira la cookie del navegador (riesgo aceptado en T-01).
/// </summary>
public interface ISignOut
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
