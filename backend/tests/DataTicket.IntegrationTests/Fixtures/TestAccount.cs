using DataTicket.Domain.Users;

namespace DataTicket.IntegrationTests.Fixtures;

/// <summary>Empresa cliente sintética (<c>db.sql</c> §13.1).</summary>
public sealed record TestCompany(Guid Id, string Name, bool IsActive);

/// <summary>
/// Cuenta sintética (<c>db.sql</c> §13.2–13.4). Inmutable: los datos de una prueba se derivan con <c>with</c>.
/// </summary>
public sealed record TestAccount(
    Guid Id,
    string Email,
    string DisplayName,
    AccountStatus Status,
    IReadOnlyList<string> Roles,
    Guid? CompanyId)
{
    /// <summary>Las cuentas <c>Invited</c> no tienen contraseña hasta activarse (HU-005).</summary>
    public bool HasPassword => Status != AccountStatus.Invited;

    public bool IsClient => CompanyId is not null;
}
