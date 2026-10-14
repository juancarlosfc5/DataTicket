using DataTicket.Application.Ports.In.Tickets;
using DataTicket.Application.Ports.Out;
using DataTicket.Application.Tickets;
using DataTicket.Infrastructure.Persistence;
using DataTicket.Infrastructure.Realtime;
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

    /// <summary>
    /// Núcleo N1: la regla de acceso y el repositorio de tickets están registrados, y el puerto
    /// <see cref="IChatConnectionRevoker"/> tiene un adaptador nulo hasta que HU-032 registre el de SignalR.
    /// </summary>
    [Fact]
    public async Task Nucleo_de_tickets_registra_regla_de_acceso_repositorio_y_revocador_nulo()
    {
        await using var factory = new DataTicketApiFactory(new ApiFactoryOptions { ConnectionString = UnusedConnectionString });
        await using var scope = factory.Services.CreateAsyncScope();

        Assert.IsType<TicketAccessPolicy>(scope.ServiceProvider.GetRequiredService<ITicketAccessPolicy>());
        Assert.IsType<TicketRepository>(scope.ServiceProvider.GetRequiredService<ITicketRepository>());
        var revoker = Assert.Single(scope.ServiceProvider.GetServices<IChatConnectionRevoker>());
        Assert.IsType<NullChatConnectionRevoker>(revoker);

        // No hace nada ni falla: el retiro (HU-019) no depende de que el chat exista.
        await revoker.RevokeTicketAccessAsync(Guid.CreateVersion7(), Guid.CreateVersion7(), TestContext.Current.CancellationToken);
    }
}
