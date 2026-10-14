using DataTicket.Domain.Common;

namespace DataTicket.Domain.Tickets;

/// <summary>
/// Participación de una persona interna en un ticket (glosario <c>TicketParticipant</c>, PRD §5, §6.2). Es lo
/// que concede acceso al detalle y al chat (junto con el rol <c>ProductManager</c>). Retirar no borra: marca
/// <see cref="RemovedAt"/> y <see cref="RemovedBy"/>, y la participación pasada se conserva (PRD §5.4, §12).
/// Vigente = <see cref="RemovedAt"/> nulo. Solo se crea y se retira a través de <see cref="Ticket"/>.
/// </summary>
public sealed class TicketParticipant
{
    internal TicketParticipant(
        Guid id, Guid ticketId, Guid userId, ParticipationOrigin origin, Guid addedBy, DateTimeOffset addedAt)
    {
        Id = id;
        TicketId = ticketId;
        UserId = userId;
        Origin = origin;
        AddedBy = addedBy;
        AddedAt = addedAt;
    }

    // Para EF Core.
    private TicketParticipant()
    {
    }

    public Guid Id { get; private set; }

    public Guid TicketId { get; private set; }

    /// <summary>Persona interna asociada (<c>identity.users</c>).</summary>
    public Guid UserId { get; private set; }

    public ParticipationOrigin Origin { get; private set; }

    /// <summary>Quién la asoció (la PM, un administrador o, en cobertura, el propio administrador).</summary>
    public Guid AddedBy { get; private set; }

    public DateTimeOffset AddedAt { get; private set; }

    public Guid? RemovedBy { get; private set; }

    public DateTimeOffset? RemovedAt { get; private set; }

    /// <summary>La participación está vigente: no se ha retirado.</summary>
    public bool IsActive => RemovedAt is null;

    internal void Remove(Guid removedBy, DateTimeOffset removedAt)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("La participación ya está retirada.");
        }

        if (removedBy == Guid.Empty)
        {
            throw new ArgumentException("Quién retira la participación es obligatorio.", nameof(removedBy));
        }

        var stored = Timestamps.ToStoredPrecision(removedAt);
        if (stored < AddedAt)
        {
            throw new ArgumentException("La fecha de retiro no puede ser anterior a la de incorporación.", nameof(removedAt));
        }

        RemovedBy = removedBy;
        RemovedAt = stored;
    }
}
