using DataTicket.Domain.Users;

namespace DataTicket.Application.Ports.Out;

/// <summary>
/// Usuario de la sesión actual (glosario «Usuario actual»). Los casos de uso autorizan con él cada operación.
/// Todo sale de la sesión; nada de la petición (cuerpo, consulta, ruta o cabeceras).
/// </summary>
/// <remarks>
/// Falla cerrado: si el perfil de la sesión no es válido (p. ej. un cliente sin empresa o una mezcla de roles
/// de cliente e internos), <see cref="Surface"/> es <c>null</c>, no hay roles ni equipos y
/// <see cref="CompanyId"/> es <c>null</c>.
/// </remarks>
public interface ICurrentUser
{
    /// <summary>Hay una sesión con un identificador de usuario válido.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Identificador del usuario; <c>null</c> sin sesión.</summary>
    Guid? UserId { get; }

    /// <summary>Nombre visible; <c>null</c> sin sesión.</summary>
    string? DisplayName { get; }

    /// <summary>Correo de la cuenta; <c>null</c> sin sesión.</summary>
    string? Email { get; }

    /// <summary>Superficie del perfil; <c>null</c> sin sesión o con un perfil no válido.</summary>
    Surface? Surface { get; }

    /// <summary>Roles (<c>ProductManager</c>, <c>Administrator</c>, <c>Requester</c>, <c>CompanyCoordinator</c>).</summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>Equipos (<c>Development</c>, <c>Production</c>). No dan acceso a ningún ticket por sí solos.</summary>
    IReadOnlyList<string> Teams { get; }

    /// <summary>
    /// Empresa del usuario cliente, tomada de la sesión. <c>null</c> para internos, sin sesión o si la sesión
    /// de un cliente no la trae: nunca se inventa ni se lee de la petición (PRD §5 regla 1).
    /// </summary>
    Guid? CompanyId { get; }

    /// <summary>El usuario tiene el rol indicado (los equipos se consultan en <see cref="Teams"/>).</summary>
    bool IsInRole(string role);
}
