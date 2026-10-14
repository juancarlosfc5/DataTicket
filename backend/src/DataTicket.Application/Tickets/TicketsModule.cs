using DataTicket.Application.Ports.In.Tickets;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.Application.Tickets;

/// <summary>Registro del módulo de tickets en la capa de aplicación (núcleo N1; lo amplían HU-013 a HU-022).</summary>
public static class TicketsModule
{
    /// <summary>Regla de acceso interno al ticket. Se registra por ámbito, igual que el repositorio que usa.</summary>
    public static IServiceCollection AddTicketsModule(this IServiceCollection services)
    {
        services.AddScoped<ITicketAccessPolicy, TicketAccessPolicy>();
        return services;
    }
}
