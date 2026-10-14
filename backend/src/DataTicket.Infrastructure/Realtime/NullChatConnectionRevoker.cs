using DataTicket.Application.Ports.Out;

namespace DataTicket.Infrastructure.Realtime;

/// <summary>
/// Adaptador nulo de <see cref="IChatConnectionRevoker"/> (núcleo N1): no hay conexiones de chat que revocar
/// mientras no exista el hub. HU-032 lo sustituye por el adaptador de SignalR en <c>Api/Realtime</c>
/// (<c>services.Replace(...)</c> o retirando este registro de <c>AddTicketsAdapters</c>). Sin él, el retiro de un
/// participante (HU-019) ya bloquea al retirado: la regla de acceso se revalida en cada operación.
/// </summary>
public sealed class NullChatConnectionRevoker : IChatConnectionRevoker
{
    public Task RevokeTicketAccessAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken) => Task.CompletedTask;
}
