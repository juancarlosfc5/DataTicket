namespace DataTicket.Domain.Tickets;

/// <summary>Impacto que indica el cliente al radicar (glosario <c>Impact</c>, PRD §6.1). Los valores numéricos
/// los usa <see cref="PriorityMatrix"/>; se persiste como texto.</summary>
public enum Impact
{
    Low = 0,
    Medium = 1,
    High = 2,
}
