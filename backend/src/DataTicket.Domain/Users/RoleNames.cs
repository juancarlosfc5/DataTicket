namespace DataTicket.Domain.Users;

/// <summary>
/// Nombres de los roles (glosario; <c>db.sql</c> §12). Los equipos <see cref="Development"/> y
/// <see cref="Production"/> se modelan como roles (decisión provisional de <c>db.sql</c> §3).
/// </summary>
public static class RoleNames
{
    public const string ProductManager = "ProductManager";
    public const string Administrator = "Administrator";
    public const string Development = "Development";
    public const string Production = "Production";
    public const string Requester = "Requester";
    public const string CompanyCoordinator = "CompanyCoordinator";

    /// <summary>Roles de las cuentas cliente.</summary>
    public static IReadOnlyList<string> Client { get; } = [Requester, CompanyCoordinator];

    /// <summary>Roles y equipos de las cuentas internas de Data Global.</summary>
    public static IReadOnlyList<string> Internal { get; } = [ProductManager, Administrator, Development, Production];

    public static IReadOnlyList<string> All { get; } = [.. Internal, .. Client];
}
