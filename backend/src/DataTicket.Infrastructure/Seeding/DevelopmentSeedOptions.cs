namespace DataTicket.Infrastructure.Seeding;

/// <summary>
/// Configuración del sembrador de Development (sección <c>Seed</c>; claves propuestas en HU-010 y
/// <c>db.sql</c> §13). Solo <c>appsettings.Development.json</c> la define; fuera de Development se ignora.
/// </summary>
public sealed class DevelopmentSeedOptions
{
    public const string SectionName = "Seed";

    public const string EnabledKey = SectionName + ":" + nameof(Enabled);

    public const string DefaultPasswordKey = SectionName + ":" + nameof(DefaultPassword);

    /// <summary>Activa la siembra al arrancar. Segunda guarda: además el entorno debe ser Development.</summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Contraseña común SINTÉTICA de las cuentas sembradas con contraseña (<c>db.sql</c> §13): un valor local de
    /// ejemplo, nunca un secreto real. Solo se usa para calcular el hash con Identity; no se registra en el log.
    /// </summary>
    public string? DefaultPassword { get; init; }
}
