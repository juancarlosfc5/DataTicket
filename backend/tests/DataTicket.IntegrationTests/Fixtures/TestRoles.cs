namespace DataTicket.IntegrationTests.Fixtures;

/// <summary>
/// Nombres de los roles de Identity (<c>db.sql</c> §12; contrato T-01 de HU-004). <c>Development</c> y
/// <c>Production</c> son los equipos, modelados como roles.
/// </summary>
public static class TestRoles
{
    public const string ProductManager = "ProductManager";
    public const string Administrator = "Administrator";
    public const string Development = "Development";
    public const string Production = "Production";
    public const string Requester = "Requester";
    public const string CompanyCoordinator = "CompanyCoordinator";

    public static IReadOnlyList<string> All { get; } =
        [ProductManager, Administrator, Development, Production, Requester, CompanyCoordinator];
}
