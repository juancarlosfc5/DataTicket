using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Tickets;
using Microsoft.EntityFrameworkCore;

namespace DataTicket.Infrastructure.Persistence;

/// <summary>
/// Adaptador de <see cref="ITicketRepository"/> sobre EF Core (<c>dataticket.tickets</c>,
/// <c>ticket_participants</c> y <c>assignments</c>).
/// </summary>
public sealed class TicketRepository(DataTicketDbContext dbContext) : ITicketRepository
{
    public Task<Ticket?> FindAsync(Guid ticketId, CancellationToken cancellationToken) =>
        dbContext.Tickets
            .Include(ticket => ticket.Participants)
            .Include(ticket => ticket.Assignments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(ticket => ticket.Id == ticketId, cancellationToken);

    public Task<bool> ExistsAsync(Guid ticketId, CancellationToken cancellationToken) =>
        dbContext.Tickets.AsNoTracking().AnyAsync(ticket => ticket.Id == ticketId, cancellationToken);

    // Una consulta por el índice parcial ux_ticket_participants_active (ticket_id, user_id) WHERE removed_at IS NULL.
    public Task<bool> IsActiveParticipantAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken) =>
        dbContext.TicketParticipants
            .AsNoTracking()
            .AnyAsync(
                participant => participant.TicketId == ticketId
                    && participant.UserId == userId
                    && participant.RemovedAt == null,
                cancellationToken);
}
