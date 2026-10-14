namespace DataTicket.Domain.Tickets;

/// <summary>Urgencia que indica el cliente al radicar (glosario <c>Urgency</c>, PRD §6.1). Los valores numéricos
/// los usa <see cref="PriorityMatrix"/>; se persiste como texto.</summary>
public enum Urgency
{
    Low = 0,
    Medium = 1,
    High = 2,
}
