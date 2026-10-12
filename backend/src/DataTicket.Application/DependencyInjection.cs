using DataTicket.Application.Authentication;
using DataTicket.Application.Ports.In.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los casos de uso (implementaciones de los puertos de entrada).
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISignIn, SignIn>();
        services.AddScoped<ISignOut, SignOut>();

        return services;
    }
}
