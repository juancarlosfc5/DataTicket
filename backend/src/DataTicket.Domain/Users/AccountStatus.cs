namespace DataTicket.Domain.Users;

/// <summary>
/// Estado de la cuenta (glosario). Solo <see cref="Active"/> inicia sesión (PRD §5).
/// Se persiste como texto con estos nombres exactos (<c>identity.users.account_status</c>).
/// </summary>
public enum AccountStatus
{
    /// <summary>Invitada por Data Global; aún no definió contraseña (HU-005).</summary>
    Invited,

    /// <summary>Activa: puede iniciar sesión.</summary>
    Active,

    /// <summary>Desactivada por Data Global; no se borra (HU-008).</summary>
    Deactivated,
}
