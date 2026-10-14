using DataTicket.Domain.Common;

namespace DataTicket.Domain.Tickets;

/// <summary>
/// Asignación de una persona responsable del ticket (glosario <c>Assignment</c>, PRD §6.2). Asignar implica
/// participar (propuesta V-10): <see cref="Ticket.Assign"/> crea también la participación si no la hay. La
/// asignación vigente tiene <see cref="UnassignedAt"/> nulo; retirar a la persona o reasignar la cierra, sin
/// borrarla (columnas <c>unassigned_at</c> y <c>unassigned_by</c> de <c>db.sql</c> §6). Solo se crea y se cierra a
/// través de <see cref="Ticket"/>.
/// </summary>
public sealed class Assignment
{
    internal Assignment(Guid id, Guid ticketId, Guid assigneeId, Guid assignedBy, DateTimeOffset assignedAt)
    {
        Id = id;
        TicketId = ticketId;
        AssigneeId = assigneeId;
        AssignedBy = assignedBy;
        AssignedAt = assignedAt;
    }

    // Para EF Core.
    private Assignment()
    {
    }

    public Guid Id { get; private set; }

    public Guid TicketId { get; private set; }

    /// <summary>Persona interna responsable (<c>identity.users</c>).</summary>
    public Guid AssigneeId { get; private set; }

    public Guid AssignedBy { get; private set; }

    public DateTimeOffset AssignedAt { get; private set; }

    /// <summary>Quién cerró la asignación (P-17 la llama <c>EndedBy</c>; ver el informe del núcleo N1).</summary>
    public Guid? UnassignedBy { get; private set; }

    /// <summary>Cuándo se cerró la asignación (P-17 la llama <c>EndedAt</c>).</summary>
    public DateTimeOffset? UnassignedAt { get; private set; }

    /// <summary>La asignación está vigente: no se ha cerrado.</summary>
    public bool IsActive => UnassignedAt is null;

    internal void Unassign(Guid unassignedBy, DateTimeOffset unassignedAt)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("La asignación ya está cerrada.");
        }

        if (unassignedBy == Guid.Empty)
        {
            throw new ArgumentException("Quién cierra la asignación es obligatorio.", nameof(unassignedBy));
        }

        var stored = Timestamps.ToStoredPrecision(unassignedAt);
        if (stored < AssignedAt)
        {
            throw new ArgumentException("La asignación no puede cerrarse antes de crearse.", nameof(unassignedAt));
        }

        UnassignedBy = unassignedBy;
        UnassignedAt = stored;
    }
}
