namespace DataTicket.Domain.Tickets;

/// <summary>
/// Matriz urgencia × impacto del PRD §6.1 (wiki <c>matriz-de-prioridad</c>). Es simétrica y coincide con la
/// función <c>dataticket.priority_matrix</c> que verifica la base (<c>ck_tickets_calculated_priority</c>).
/// </summary>
public static class PriorityMatrix
{
    public static CalculatedPriority Calculate(Urgency urgency, Impact impact)
    {
        if (!Enum.IsDefined(urgency))
        {
            throw new ArgumentOutOfRangeException(nameof(urgency), urgency, "La urgencia debe ser Low, Medium o High.");
        }

        if (!Enum.IsDefined(impact))
        {
            throw new ArgumentOutOfRangeException(nameof(impact), impact, "El impacto debe ser Low, Medium o High.");
        }

        // Ambas escalas valen 0 (Low), 1 (Medium) o 2 (High): la suma recorre la matriz simétrica del PRD.
        //   suma 0 → VeryLow; 1 → Low; 2 → Medium; 3 → High; 4 → Critical.
        return (CalculatedPriority)((int)urgency + (int)impact);
    }
}
