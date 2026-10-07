using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los casos de uso (implementaciones de los puertos de entrada).
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
