namespace DataTicket.Application.Ports.Out;

/// <summary>
/// Revoca en tiempo real el acceso de una persona al chat de un ticket (HU-032; PRD §7.5, CA-06). Lo invoca
/// <c>RemoveParticipant</c> (HU-019) DESPUÉS de persistir el retiro y su <c>AuditEntry</c>.
/// </summary>
/// <remarks>
/// El adaptador real (HU-032, <c>Api/Realtime</c>) saca del grupo <c>ticket:{ticketId}</c> todas las conexiones
/// de esa persona unidas al ticket y les envía <c>TicketAccessRevoked { ticketId, revokedAt }</c>, sin cerrar la
/// conexión ni tocar sus otros tickets. Mientras no exista, se registra un adaptador nulo
/// (<c>NullChatConnectionRevoker</c>). Si el adaptador falla, el retiro se mantiene: la revalidación por
/// operación del hub sigue bloqueando a la persona retirada.
/// </remarks>
public interface IChatConnectionRevoker
{
    /// <summary>Revoca el acceso de <paramref name="userId"/> al chat de <paramref name="ticketId"/>.</summary>
    Task RevokeTicketAccessAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken);
}
