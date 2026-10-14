using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Auditing;

namespace DataTicket.Infrastructure.Persistence;

/// <summary>
/// Adaptador de <see cref="IAuditLog"/> sobre EF Core: añade la entrada al <see cref="DataTicketDbContext"/> del
/// ámbito (el mismo que usa la acción) sin confirmar. El <c>SaveChangesAsync</c> del caso de uso inserta la
/// acción y la entrada en una sola transacción (HU-011 CHU-02 y CHU-03).
/// </summary>
public sealed class EfAuditLog(DataTicketDbContext dbContext) : IAuditLog
{
    public void Append(AuditEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        dbContext.AuditEntries.Add(entry);
    }
}
