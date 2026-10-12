namespace DataTicket.Application.Ports.Out;

/// <summary>
/// Sesión del usuario en el canal actual (cookie de Identity en el adaptador HTTP).
/// </summary>
public interface IUserSession
{
    /// <summary>Emite la sesión de la cuenta indicada. Solo se llama tras validar estado y perfil.</summary>
    Task StartAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Cierra la sesión del canal actual (no revoca otras copias de la cookie).</summary>
    Task EndAsync(CancellationToken cancellationToken);
}
