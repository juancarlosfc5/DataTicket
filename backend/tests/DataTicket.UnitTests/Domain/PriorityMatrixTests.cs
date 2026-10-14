using DataTicket.Domain.Tickets;

namespace DataTicket.UnitTests.Domain;

/// <summary>
/// Matriz urgencia × impacto del PRD §6.1 (adelanto de HU-013 T-02 en el núcleo N1: el agregado la necesita para
/// que <c>calculated_priority</c> cumpla <c>ck_tickets_calculated_priority</c>).
/// </summary>
public sealed class PriorityMatrixTests
{
    [Theory]
    [InlineData(Urgency.Low, Impact.Low, CalculatedPriority.VeryLow)]
    [InlineData(Urgency.Medium, Impact.Low, CalculatedPriority.Low)]
    [InlineData(Urgency.High, Impact.Low, CalculatedPriority.Medium)]
    [InlineData(Urgency.Low, Impact.Medium, CalculatedPriority.Low)]
    [InlineData(Urgency.Medium, Impact.Medium, CalculatedPriority.Medium)]
    [InlineData(Urgency.High, Impact.Medium, CalculatedPriority.High)]
    [InlineData(Urgency.Low, Impact.High, CalculatedPriority.Medium)]
    [InlineData(Urgency.Medium, Impact.High, CalculatedPriority.High)]
    [InlineData(Urgency.High, Impact.High, CalculatedPriority.Critical)]
    public void Calculate_ReturnsMatrixValue(Urgency urgency, Impact impact, CalculatedPriority expected)
    {
        Assert.Equal(expected, PriorityMatrix.Calculate(urgency, impact));
    }

    [Fact]
    public void Calculate_IsSymmetric()
    {
        foreach (var urgency in Enum.GetValues<Urgency>())
        {
            foreach (var impact in Enum.GetValues<Impact>())
            {
                // Urgency e Impact comparten niveles (Low, Medium, High): la matriz no distingue cuál es cuál.
                Assert.Equal(
                    PriorityMatrix.Calculate(urgency, impact),
                    PriorityMatrix.Calculate((Urgency)(int)impact, (Impact)(int)urgency));
            }
        }
    }

    [Fact]
    public void Calculate_WithUndefinedLevel_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PriorityMatrix.Calculate((Urgency)7, Impact.Low));
        Assert.Throws<ArgumentOutOfRangeException>(() => PriorityMatrix.Calculate(Urgency.Low, (Impact)7));
    }
}
