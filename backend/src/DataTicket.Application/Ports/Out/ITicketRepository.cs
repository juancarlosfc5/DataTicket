using DataTicket.Domain.Tickets;

namespace DataTicket.Application.Ports.Out;

/// <summary>
/// Repositorio del agregado <see cref="Ticket"/> (núcleo N1). Uno solo para el ticket, sus participaciones y
/// sus asignaciones: son partes del mismo agregado y se escriben siempre a través de la raíz. Las consultas de
/// acceso no cargan el agregado: responden con una sola consulta indexada.
/// </summary>
/// <remarks>
/// Lectura y carga del agregado. La inserción y la unidad de trabajo (confirmar con el <c>IAuditLog</c> en la
/// misma transacción) las define HU-013, que es la primera que escribe tickets desde un caso de uso.
/// </remarks>
public interface ITicketRepository
{
    /// <summary>
    /// Carga el agregado completo (con todas sus participaciones y asignaciones, vigentes e históricas) para
    /// modificarlo; <c>null</c> si no existe.
    /// </summary>
    Task<Ticket?> FindAsync(Guid ticketId, CancellationToken cancellationToken);

    /// <summary>El ticket existe.</summary>
    Task<bool> ExistsAsync(Guid ticketId, CancellationToken cancellationToken);

    /// <summary>
    /// La persona tiene una participación vigente (no retirada) en el ticket. <c>false</c> si el ticket no existe.
    /// </summary>
    Task<bool> IsActiveParticipantAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken);
}
