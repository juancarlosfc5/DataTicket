namespace DataTicket.Infrastructure.Identity;

/// <summary>
/// Claims propios de la cookie de sesión (contrato T-01 de HU-004). El id, el correo y los roles usan los
/// tipos estándar de Identity (<c>ClaimTypes.NameIdentifier</c>, <c>ClaimTypes.Email</c>, <c>ClaimTypes.Role</c>).
/// </summary>
public static class DataTicketClaimTypes
{
    /// <summary>Empresa del usuario cliente. Solo lo emite la fábrica de claims, desde la cuenta.</summary>
    public const string CompanyId = "company_id";

    /// <summary>Nombre visible de la cuenta.</summary>
    public const string DisplayName = "display_name";
}
