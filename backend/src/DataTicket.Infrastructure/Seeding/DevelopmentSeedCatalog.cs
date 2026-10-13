using DataTicket.Domain.Users;

namespace DataTicket.Infrastructure.Seeding;

/// <summary>
/// Subconjunto MÍNIMO de <c>db.sql</c> §13.1–13.4 que siembra Development para poder iniciar sesión en el
/// stack local: una empresa, la PM, un integrante de Desarrollo y tres cuentas cliente de la Empresa 01
/// (activa, invitada y desactivada). Mismos identificadores deterministas, correos, nombres, estados, roles y
/// fechas que <c>db.sql</c>. El juego completo de datos sintéticos es de HU-010.
/// </summary>
/// <remarks>
/// La fuente de verdad es <c>db.sql</c> §13. El catálogo completo de las pruebas (<c>TestAccounts</c>) vive en
/// el proyecto de pruebas, que depende de <c>src</c> y nunca al revés; una prueba comprueba que cada fila de
/// aquí coincide con la suya.
/// </remarks>
public static class DevelopmentSeedCatalog
{
    private static readonly DateTimeOffset CompaniesCreatedAt = new(2026, 9, 1, 13, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset InternalAccountsCreatedAt = new(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ClientAccountsCreatedAt = new(2026, 9, 1, 13, 30, 0, TimeSpan.Zero);

    /// <summary>Empresa 01 de <c>db.sql</c> §13.1 (empresa A de las pruebas de aislamiento).</summary>
    public static SeedCompany Company01 { get; } =
        new(Guid.Parse("c0000000-0000-7000-8000-000000000001"), "Empresa Sintética 01", CompaniesCreatedAt);

    public static IReadOnlyList<SeedCompany> Companies { get; } = [Company01];

    public static IReadOnlyList<SeedAccount> Accounts { get; } =
    [
        // §13.2 y §13.4: Elizabeth es ProductManager y además del equipo de Producción (PRD §5).
        Internal(
            "a0000000-0000-7000-8000-000000000001", "elizabeth@dataticket.local", "Elizabeth",
            RoleNames.ProductManager, RoleNames.Production),
        Internal("a0000000-0000-7000-8000-000000000005", "laura@dataticket.local", "Laura", RoleNames.Development),

        // §13.3: cuentas de la Empresa 01. La Invited no tiene contraseña hasta activarse (HU-005).
        Client("b0000000-0000-7000-8000-000001000001", "ana.perez@empresa-sintetica-01.test", "Ana Pérez", AccountStatus.Active),
        Client("b0000000-0000-7000-8000-000001000004", "invitado@empresa-sintetica-01.test", "Invitado Empresa 01", AccountStatus.Invited),
        Client(
            "b0000000-0000-7000-8000-000001000005", "desactivado@empresa-sintetica-01.test", "Desactivado Empresa 01",
            AccountStatus.Deactivated),
    ];

    private static SeedAccount Internal(string id, string email, string displayName, params string[] roles) =>
        new(Guid.Parse(id), email, displayName, AccountStatus.Active, roles, CompanyId: null, InternalAccountsCreatedAt);

    private static SeedAccount Client(string id, string email, string displayName, AccountStatus status) =>
        new(Guid.Parse(id), email, displayName, status, [RoleNames.Requester], Company01.Id, ClientAccountsCreatedAt);
}

/// <summary>Empresa cliente sintética a sembrar.</summary>
public sealed record SeedCompany(Guid Id, string Name, DateTimeOffset CreatedAt);

/// <summary>Cuenta sintética a sembrar. Los roles y equipos se asignan en <c>identity.user_roles</c>.</summary>
public sealed record SeedAccount(
    Guid Id,
    string Email,
    string DisplayName,
    AccountStatus Status,
    IReadOnlyList<string> Roles,
    Guid? CompanyId,
    DateTimeOffset CreatedAt)
{
    /// <summary>Las cuentas <c>Invited</c> no tienen contraseña ni correo confirmado (<c>db.sql</c> §13.3).</summary>
    public bool HasPassword => Status != AccountStatus.Invited;
}
