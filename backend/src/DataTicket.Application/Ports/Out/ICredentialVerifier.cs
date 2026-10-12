using DataTicket.Domain.Users;

namespace DataTicket.Application.Ports.Out;

/// <summary>
/// Verifica correo y contraseña contra el almacén de cuentas, aplicando el bloqueo por intentos fallidos.
/// No emite sesión ni revela por qué falló.
/// </summary>
public interface ICredentialVerifier
{
    /// <summary>
    /// Devuelve la cuenta si la contraseña coincide y la cuenta no está bloqueada; <c>null</c> en cualquier
    /// otro caso (correo inexistente, sin contraseña, contraseña incorrecta o bloqueo).
    /// </summary>
    Task<VerifiedAccount?> VerifyAsync(string email, string password, CancellationToken cancellationToken);
}

/// <summary>
/// Datos de la cuenta necesarios para decidir si puede tener sesión.
/// </summary>
public sealed record VerifiedAccount(
    Guid UserId,
    AccountStatus Status,
    Guid? CompanyId,
    IReadOnlyCollection<string> Roles);
