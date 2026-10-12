using DataTicket.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace DataTicket.Infrastructure.Identity;

/// <summary>
/// Cuenta de ASP.NET Core Identity (<c>identity.users</c>, <c>db.sql</c> §3). Vive en Infrastructure: el dominio
/// solo conoce el <c>UserId</c>. <see cref="IdentityUser{TKey}.UserName"/> es el correo.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public const int DisplayNameMaxLength = 100;

    public string DisplayName { get; set; } = string.Empty;

    public AccountStatus AccountStatus { get; set; } = AccountStatus.Invited;

    /// <summary>Empresa de la cuenta cliente; <c>null</c> en las internas. Origen del claim <c>company_id</c>.</summary>
    public Guid? CompanyId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
