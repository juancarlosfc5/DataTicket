namespace DataTicket.Domain.Common;

/// <summary>
/// Utilidades de fechas persistidas (UTC en <c>timestamptz</c>, resolución de microsegundos).
/// </summary>
public static class Timestamps
{
    private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

    /// <summary>
    /// Convierte a UTC y trunca a microsegundos, la resolución de PostgreSQL, para que el valor en memoria
    /// coincida con el leído de la base.
    /// </summary>
    public static DateTimeOffset ToStoredPrecision(DateTimeOffset value)
    {
        var utc = value.ToUniversalTime();
        return new DateTimeOffset(utc.Ticks - (utc.Ticks % TicksPerMicrosecond), TimeSpan.Zero);
    }
}
