using DataTicket.Application.Ports.In.Tickets;
using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Users;

namespace DataTicket.Application.Tickets;

/// <summary>
/// Regla de acceso interno al ticket (núcleo N1): «participante vigente del ticket o <c>ProductManager</c>». Ver
/// <see cref="ITicketAccessPolicy"/>. Falla cerrado: todo lo que no es una persona interna con perfil válido se
/// deniega sin consultar la base.
/// </summary>
public sealed class TicketAccessPolicy(ITicketRepository tickets) : ITicketAccessPolicy
{
    public async Task<bool> CanAccessInternalAsync(Guid ticketId, ICurrentUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Sin sesión, perfil no válido (Surface nula) o cliente: nunca ven contenido interno (invariantes 1 y 2).
        if (ticketId == Guid.Empty
            || !user.IsAuthenticated
            || user.UserId is not { } userId
            || user.Surface != Surface.Internal)
        {
            return false;
        }

        // Elizabeth (PM) siempre puede, para triage y seguimiento (PRD §5.3), si el ticket existe.
        if (user.IsInRole(RoleNames.ProductManager))
        {
            return await tickets.ExistsAsync(ticketId, cancellationToken);
        }

        // Cualquier otra persona interna, administrador incluido (invariante 5), solo con participación vigente.
        // Pertenecer a un equipo no da acceso (PRD §6.2.5). Se consulta en cada llamada, sin caché (HU-032).
        return await tickets.IsActiveParticipantAsync(ticketId, userId, cancellationToken);
    }
}
