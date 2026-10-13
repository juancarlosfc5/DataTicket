using DataTicket.Domain.Users;

namespace DataTicket.Application.Ports.In.Authentication;

/// <summary>
/// Contexto del usuario de la sesión (<c>GET /api/auth/me</c>, HU-004). No recibe parámetros: describe
/// siempre al usuario actual.
/// </summary>
public interface IGetCurrentUserProfile
{
    /// <summary>
    /// Perfil del usuario actual, o <c>null</c> si la sesión no tiene un perfil válido (falla cerrado).
    /// </summary>
    Task<CurrentUserProfile?> ExecuteAsync(CancellationToken cancellationToken);
}

/// <summary>Identidad, superficie, roles, equipos y empresa (solo cliente) del usuario actual.</summary>
public sealed record CurrentUserProfile(
    Guid Id,
    string DisplayName,
    string Email,
    Surface Surface,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Teams,
    CurrentUserCompany? Company);

/// <summary>Empresa del usuario cliente.</summary>
public sealed record CurrentUserCompany(Guid Id, string Name);
