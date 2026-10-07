using DataTicket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "DataTicket";
    public const string ReadinessTag = "ready";

    /// <summary>
    /// Registra los adaptadores secundarios (implementaciones de los puertos de salida).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Falta la cadena de conexión 'ConnectionStrings:{ConnectionStringName}'.");
        }

        services.AddDbContext<DataTicketDbContext>(options => options.UseNpgsql(connectionString));

        services.AddHealthChecks()
            .AddDbContextCheck<DataTicketDbContext>("postgresql", tags: [ReadinessTag]);

        return services;
    }
}
