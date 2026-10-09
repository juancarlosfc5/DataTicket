using DataTicket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;
using InfrastructureRegistration = DataTicket.Infrastructure.DependencyInjection;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Opciones de un host de pruebas.
/// </summary>
public sealed record ApiFactoryOptions
{
    /// <summary>Cadena de conexión de la base aislada (nunca la del Compose).</summary>
    public required string ConnectionString { get; init; }

    /// <summary>Entorno del host. Por defecto <c>Development</c>, como el stack local.</summary>
    public string Environment { get; init; } = Environments.Development;

    /// <summary>
    /// Reloj de pruebas que sustituye al <see cref="TimeProvider"/> registrado por el Api.
    /// <c>null</c> deja el de producción (<see cref="TimeProvider.System"/>).
    /// </summary>
    public FakeTimeProvider? TimeProvider { get; init; }
}

/// <summary>
/// Host en memoria del Api (<see cref="WebApplicationFactory{TEntryPoint}"/>) apuntado a una base aislada.
/// Para simular un reinicio se crea otra instancia con la misma cadena de conexión.
/// </summary>
public sealed class DataTicketApiFactory(ApiFactoryOptions options) : WebApplicationFactory<Program>
{
    public ApiFactoryOptions Options { get; } = options;

    /// <summary>
    /// Aplica las migraciones de EF Core sobre la base aislada (nunca <c>EnsureCreated</c>).
    /// </summary>
    public async Task MigrateDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Options.Environment);

        // UseSetting entra en la configuración antes de que Program.cs la lea (AddInfrastructure),
        // y tiene prioridad sobre appsettings.{Environment}.json.
        builder.UseSetting($"ConnectionStrings:{InfrastructureRegistration.ConnectionStringName}", Options.ConnectionString);

        if (Options.TimeProvider is { } fakeTime)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(fakeTime);
            });
        }
    }
}
