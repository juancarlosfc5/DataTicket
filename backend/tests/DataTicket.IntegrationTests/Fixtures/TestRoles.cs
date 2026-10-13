using DataTicket.Domain.Users;

namespace DataTicket.IntegrationTests.Fixtures;

/// <summary>
/// Atajos a los nombres de rol de producción (<see cref="RoleNames"/>, <c>db.sql</c> §12). <c>Development</c> y
/// <c>Production</c> son los equipos, modelados como roles.
/// </summary>
public static class TestRoles
{
    public const string ProductManager = RoleNames.ProductManager;
    public const string Administrator = RoleNames.Administrator;
    public const string Development = RoleNames.Development;
    public const string Production = RoleNames.Production;
    public const string Requester = RoleNames.Requester;
    public const string CompanyCoordinator = RoleNames.CompanyCoordinator;

    public static IReadOnlyList<string> All => RoleNames.All;
}
