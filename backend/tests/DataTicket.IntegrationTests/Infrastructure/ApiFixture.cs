using DataTicket.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Fixture de clase (<c>IClassFixture&lt;ApiFixture&gt;</c>): una base aislada por clase de pruebas, migrada y
/// sembrada con el catálogo (<see cref="TestDataSeeder"/>), y un host del Api con reloj controlable.
/// Las clases que la usan deben llevar <c>[Trait(TestCategories.Name, TestCategories.RequiresDocker)]</c>.
/// </summary>
/// <remarks>
/// El host, la base y el reloj se COMPARTEN entre las pruebas de la clase (xUnit las ejecuta en serie dentro
/// de una clase). Una prueba que avance <see cref="Time"/> o cambie el estado de una cuenta debe vivir en su
/// propia clase, usar <see cref="CreateFactory"/> con un reloj propio o trabajar con tiempos relativos.
/// </remarks>
public sealed class ApiFixture(PostgreSqlContainerFixture postgres) : IAsyncLifetime
{
    private TestDatabase? _database;
    private DataTicketApiFactory? _factory;

    public TestDatabase Database => _database ?? throw NotInitialized();

    public DataTicketApiFactory Factory => _factory ?? throw NotInitialized();

    /// <summary>Reloj del host compartido; avánzalo con <see cref="FakeTimeProvider.Advance"/>.</summary>
    public FakeTimeProvider Time => Factory.Options.TimeProvider ?? throw NotInitialized();

    public PostgreSqlContainerFixture Postgres => postgres;

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        try
        {
            _database = await postgres.CreateDatabaseAsync(cancellationToken);
            // Development migra al arrancar, ANTES de que Data Protection lea su tabla de claves; la llamada
            // explícita cubre hosts de otros entornos y es idempotente.
            _factory = CreateFactory();
            await _factory.MigrateDatabaseAsync(cancellationToken);
            await TestDataSeeder.SeedAsync(_factory.Services, Time, cancellationToken);
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    /// <summary>
    /// Otro host sobre la MISMA base (p. ej. otro entorno, o un «reinicio» del backend), con su propio reloj.
    /// Quien lo crea lo desecha.
    /// </summary>
    public DataTicketApiFactory CreateFactory(string? environment = null) =>
        new(new ApiFactoryOptions
        {
            ConnectionString = Database.ConnectionString,
            Environment = environment ?? Environments.Development,
            TimeProvider = TestClock.Create(),
            ApplyMigrationsOnStartup = true,
        });

    /// <summary>Ejecuta código con los servicios de un ámbito del host (p. ej. <c>UserManager</c>).</summary>
    public async Task<T> WithServicesAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        return await action(scope.ServiceProvider);
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
            _factory = null;
        }

        if (_database is not null)
        {
            await _database.DisposeAsync();
            _database = null;
        }
    }

    private static InvalidOperationException NotInitialized() =>
        new("ApiFixture no está inicializada: úsala como IClassFixture<ApiFixture>.");
}
