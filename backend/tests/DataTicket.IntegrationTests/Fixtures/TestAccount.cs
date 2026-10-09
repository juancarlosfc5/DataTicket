namespace DataTicket.IntegrationTests.Fixtures;

/// <summary>
/// Estado de la cuenta en las pruebas. Refleja <c>AccountStatus</c> del contrato T-01 (HU-003); cuando
/// el tipo exista en el código (B3), el sembrador hace la conversión por nombre.
/// </summary>
public enum TestAccountStatus
{
    Invited,
    Active,
    Deactivated,
}

/// <summary>Empresa cliente sintética (<c>db.sql</c> §13.1).</summary>
public sealed record TestCompany(Guid Id, string Name, bool IsActive);

/// <summary>
/// Cuenta sintética (<c>db.sql</c> §13.2–13.4). Inmutable: los datos de una prueba se derivan con <c>with</c>.
/// </summary>
public sealed record TestAccount(
    Guid Id,
    string Email,
    string DisplayName,
    TestAccountStatus Status,
    IReadOnlyList<string> Roles,
    Guid? CompanyId)
{
    /// <summary>Las cuentas <c>Invited</c> no tienen contraseña hasta activarse (HU-005).</summary>
    public bool HasPassword => Status != TestAccountStatus.Invited;

    public bool IsClient => CompanyId is not null;
}
