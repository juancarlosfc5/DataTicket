using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Smoke;

/// <summary>
/// Registros del composition root que no necesitan base de datos (no abren conexión). No necesita Docker.
/// </summary>
public sealed class CompositionRootTests
{
    // Cadena que nunca se usa para conectar: solo satisface la validación de AddInfrastructure.
    private const string UnusedConnectionString = "Host=postgres.invalid;Database=unused;Username=unused";

    [Fact]
    public async Task El_Api_registra_TimeProvider_System_por_defecto()
    {
        await using var factory = new DataTicketApiFactory(new ApiFactoryOptions
        {
            ConnectionString = UnusedConnectionString,
            TimeProvider = null,
        });

        var resolved = factory.Services.GetService<TimeProvider>();

        Assert.Same(TimeProvider.System, resolved);
    }
}
