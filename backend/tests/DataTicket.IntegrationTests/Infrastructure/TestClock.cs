using Microsoft.Extensions.Time.Testing;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Reloj controlable de las pruebas. Arranca en un instante fijo (UTC) para que las fechas sean reproducibles.
/// </summary>
public static class TestClock
{
    /// <summary>2026-10-09 13:00:00 UTC (08:00 en Bogotá).</summary>
    public static readonly DateTimeOffset Start = new(2026, 10, 9, 13, 0, 0, TimeSpan.Zero);

    public static FakeTimeProvider Create() => new(Start);
}
