namespace DataTicket.Infrastructure.Identity;

/// <summary>
/// Parámetros de Identity ratificados en T-01 de HU-003 (P-18, P-13, P-22: propuestas vigentes).
/// </summary>
public static class IdentitySettings
{
    public const int PasswordMinLength = 12;

    public const int MaxFailedAccessAttempts = 5;

    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    /// <summary>Cada cuánto la cookie revalida el sello de seguridad y el estado de la cuenta.</summary>
    public static readonly TimeSpan SecurityStampValidationInterval = TimeSpan.FromMinutes(5);

    public const string DataProtectionApplicationName = "DataTicket";
}
