using DataTicket.Application.Ports.Out;

namespace DataTicket.Application.Ports.In.Tickets;

/// <summary>
/// Regla de acceso interno a un ticket (núcleo N1), reutilizable por el detalle interno (HU-020), la bitácora
/// (HU-012), el historial y el hub del chat (HU-026 a HU-032): «participante vigente del ticket o
/// <c>ProductManager</c>» (PRD §5, §6.2.5, §7; invariantes 3 y 5).
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>La PM (rol <c>ProductManager</c>) accede a cualquier ticket existente (PRD §5.3).</item>
/// <item>Cualquier otra persona interna, solo con una participación vigente; también el administrador, que no
/// accede por serlo (invariante 5), y los equipos <c>Development</c>/<c>Production</c>, que no dan acceso.</item>
/// <item>Una persona retirada pierde el acceso (PRD §5.4, §7.5).</item>
/// <item>Un usuario cliente, un perfil no válido o una petición sin sesión nunca acceden.</item>
/// </list>
/// Un ticket inexistente y uno no visible dan el mismo <c>false</c>: el adaptador responde 404 idéntico en
/// ambos casos (P-01). Se consulta el repositorio en cada llamada, sin caché.
/// </remarks>
public interface ITicketAccessPolicy
{
    /// <summary>
    /// <paramref name="user"/> puede ver el contenido interno del ticket (detalle, bitácora y chat).
    /// </summary>
    Task<bool> CanAccessInternalAsync(Guid ticketId, ICurrentUser user, CancellationToken cancellationToken);
}
