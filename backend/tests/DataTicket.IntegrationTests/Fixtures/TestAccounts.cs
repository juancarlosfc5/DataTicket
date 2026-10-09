namespace DataTicket.IntegrationTests.Fixtures;

/// <summary>
/// Catálogo de cuentas sintéticas que replica <c>db.sql</c> §13.2–13.4 (mismos IDs, correos, nombres,
/// estados y roles). Es solo de pruebas: nunca se carga en un entorno compartido.
/// </summary>
public static class TestAccounts
{
    /// <summary>
    /// Contraseña común SINTÉTICA de las cuentas con contraseña (<c>db.sql</c> §13 y criterios de HU-003).
    /// Solo existe en este proyecto de pruebas; nunca sale de configuración real.
    /// </summary>
    public const string SyntheticPassword = "Clave-Sintetica-2026";

    // 13.2 Usuarios internos de Data Global (todos Active, sin empresa).
    public static TestAccount Elizabeth { get; } =
        Internal(1, "elizabeth@dataticket.local", "Elizabeth", TestRoles.ProductManager, TestRoles.Production);

    public static TestAccount Julian { get; } = Internal(2, "julian@dataticket.local", "Julián", TestRoles.Production);

    public static TestAccount Gerardo { get; } = Internal(3, "gerardo@dataticket.local", "Gerardo", TestRoles.Production);

    public static TestAccount JuanDavid { get; } =
        Internal(4, "juan.david@dataticket.local", "Juan David", TestRoles.Development);

    public static TestAccount Laura { get; } = Internal(5, "laura@dataticket.local", "Laura", TestRoles.Development);

    public static TestAccount Brayan { get; } = Internal(6, "brayan@dataticket.local", "Brayan", TestRoles.Development);

    public static TestAccount Cristian { get; } =
        Internal(7, "cristian@dataticket.local", "Cristian", TestRoles.Development);

    public static TestAccount Kevin { get; } = Internal(8, "kevin@dataticket.local", "Kevin", TestRoles.Development);

    public static TestAccount Admin { get; } =
        Internal(9, "admin@dataticket.local", "Administrador DataTicket", TestRoles.Administrator);

    // 13.3 Usuarios cliente de la Empresa 01 (empresa A).
    /// <summary>Cuenta de ejemplo de HU-003: Solicitante 1 de la Empresa 01.</summary>
    public static TestAccount AnaPerez { get; } = Client(1, ClientKind.Requester1);

    public static TestAccount Company01Requester2 { get; } = Client(1, ClientKind.Requester2);

    public static TestAccount Company01Coordinator { get; } = Client(1, ClientKind.Coordinator);

    public static TestAccount Company01Invited { get; } = Client(1, ClientKind.Invited);

    public static TestAccount Company01Deactivated { get; } = Client(1, ClientKind.Deactivated);

    // 13.3 Usuarios cliente de la Empresa 02 (empresa B).
    public static TestAccount Company02Requester1 { get; } = Client(2, ClientKind.Requester1);

    public static TestAccount Company02Requester2 { get; } = Client(2, ClientKind.Requester2);

    public static TestAccount Company02Coordinator { get; } = Client(2, ClientKind.Coordinator);

    public static TestAccount Company02Invited { get; } = Client(2, ClientKind.Invited);

    public static TestAccount Company02Deactivated { get; } = Client(2, ClientKind.Deactivated);

    public static IReadOnlyList<TestAccount> Internals { get; } =
        [Elizabeth, Julian, Gerardo, JuanDavid, Laura, Brayan, Cristian, Kevin, Admin];

    public static IReadOnlyList<TestAccount> Clients { get; } =
    [
        AnaPerez, Company01Requester2, Company01Coordinator, Company01Invited, Company01Deactivated,
        Company02Requester1, Company02Requester2, Company02Coordinator, Company02Invited, Company02Deactivated,
    ];

    public static IReadOnlyList<TestAccount> All { get; } = [.. Internals, .. Clients];

    private enum ClientKind
    {
        Requester1 = 1,
        Requester2 = 2,
        Coordinator = 3,
        Invited = 4,
        Deactivated = 5,
    }

    private static TestAccount Internal(int number, string email, string displayName, params string[] roles) =>
        new(
            Guid.Parse($"a0000000-0000-7000-8000-{number:D12}"),
            email,
            displayName,
            TestAccountStatus.Active,
            [.. roles],
            CompanyId: null);

    // Misma fórmula que db.sql §13.3: b0000000-0000-7000-8000-EEEEEEKKKKKK (E = empresa, K = tipo).
    private static TestAccount Client(int companyNumber, ClientKind kind)
    {
        var isAnaPerez = companyNumber == 1 && kind == ClientKind.Requester1;
        var (localPart, label) = kind switch
        {
            ClientKind.Requester1 => ("solicitante1", "Solicitante 1"),
            ClientKind.Requester2 => ("solicitante2", "Solicitante 2"),
            ClientKind.Coordinator => ("coordinador", "Coordinador"),
            ClientKind.Invited => ("invitado", "Invitado"),
            _ => ("desactivado", "Desactivado"),
        };
        var status = kind switch
        {
            ClientKind.Invited => TestAccountStatus.Invited,
            ClientKind.Deactivated => TestAccountStatus.Deactivated,
            _ => TestAccountStatus.Active,
        };
        var role = kind == ClientKind.Coordinator ? TestRoles.CompanyCoordinator : TestRoles.Requester;

        return new TestAccount(
            Guid.Parse($"b0000000-0000-7000-8000-{companyNumber:D6}{(int)kind:D6}"),
            isAnaPerez ? "ana.perez@empresa-sintetica-01.test" : $"{localPart}@empresa-sintetica-{companyNumber:D2}.test",
            isAnaPerez ? "Ana Pérez" : $"{label} Empresa {companyNumber:D2}",
            status,
            [role],
            TestCompanies.IdOf(companyNumber));
    }
}
