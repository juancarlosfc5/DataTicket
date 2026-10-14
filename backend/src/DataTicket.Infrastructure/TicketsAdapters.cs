using DataTicket.Application.Ports.Out;
using DataTicket.Infrastructure.Persistence;
using DataTicket.Infrastructure.Realtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DataTicket.Infrastructure;

/// <summary>Adaptadores secundarios del módulo de tickets (núcleo N1; lo amplían HU-013 a HU-022).</summary>
public static class TicketsAdapters
{
    public static IServiceCollection AddTicketsAdapters(this IServiceCollection services)
    {
        services.AddScoped<ITicketRepository, TicketRepository>();

        // Hasta HU-032 no hay conexiones de chat que revocar. El adaptador de SignalR lo sustituirá.
        services.TryAddSingleton<IChatConnectionRevoker, NullChatConnectionRevoker>();
        return services;
    }
}
