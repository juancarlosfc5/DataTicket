using DataTicket.Infrastructure.Persistence;
using DataTicket.Infrastructure.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

    /// <summary>
    /// Si el host aplica las migraciones al arrancar (solo surte efecto en Development). Por defecto no:
    /// las fixtures migran de forma explícita para controlar los fallos.
    /// </summary>
    public bool ApplyMigrationsOnStartup { get; init; }

    /// <summary>
    /// Valor de <c>Seed:Enabled</c>. Por defecto <c>false</c>, aunque <c>appsettings.Development.json</c> lo
    /// active: las pruebas siembran su propio catálogo (<c>TestDataSeeder</c>), con los mismos IDs que el
    /// sembrador de Development. Solo las pruebas del sembrador lo activan.
    /// </summary>
    public bool SeedEnabled { get; init; }

    /// <summary>
    /// Valor de <c>Seed:DefaultPassword</c>. <c>null</c> deja el de la configuración del Api (el de
    /// <c>appsettings.Development.json</c> en Development; ninguno en los demás entornos).
    /// </summary>
    public string? SeedDefaultPassword { get; init; }

    /// <summary>Proveedor de log adicional, para que una prueba lea lo que registra el host.</summary>
    public ILoggerProvider? LoggerProvider { get; init; }
}

/// <summary>
/// Host en memoria del Api (<see cref="WebApplicationFactory{TEntryPoint}"/>) apuntado a una base aislada,
/// con los <see cref="TestEndpoints"/>. Para simular un reinicio se crea otra instancia con la misma cadena.
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
        builder.UseSetting(DatabaseMigrations.ApplyOnStartupKey, Options.ApplyMigrationsOnStartup ? "true" : "false");
        builder.UseSetting(DevelopmentSeedOptions.EnabledKey, Options.SeedEnabled ? "true" : "false");
        if (Options.SeedDefaultPassword is { } seedPassword)
        {
            builder.UseSetting(DevelopmentSeedOptions.DefaultPasswordKey, seedPassword);
        }

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IStartupFilter, TestEndpoints>();

            if (Options.LoggerProvider is { } loggerProvider)
            {
                services.AddSingleton(loggerProvider);
            }

            if (Options.TimeProvider is { } fakeTime)
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(fakeTime);
            }
        });
    }
}
