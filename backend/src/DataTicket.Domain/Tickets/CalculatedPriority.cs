namespace DataTicket.Domain.Tickets;

/// <summary>
/// Niveles de prioridad (glosario <c>CalculatedPriority</c>, PRD §6.1). Los usan la prioridad calculada por la
/// matriz (<see cref="PriorityMatrix"/>) y la prioridad vigente del ticket, que solo difiere tras un ajuste
/// manual (HU-015).
/// </summary>
public enum CalculatedPriority
{
    VeryLow = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4,
}
