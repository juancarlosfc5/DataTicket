using Microsoft.Extensions.Time.Testing;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Fixture de clase (<c>IClassFixture&lt;ApiFixture&gt;</c>): una base aislada por clase de pruebas, migrada,
/// y un host del Api con reloj controlable. Las clases que la usan deben llevar
/// <c>[Trait(TestCategories.Name, TestCategories.RequiresDocker)]</c>.
/// </summary>
public sealed class ApiFixture(PostgreSqlContainerFixture postgres) : IAsyncLifetime
{
    private TestDatabase? _database;
    private DataTicketApiFactory? _factory;

    public TestDatabase Database => _database ?? throw NotInitialized();

    public DataTicketApiFactory Factory => _factory ?? throw NotInitialized();

    /// <summary>Reloj del host; avánzalo con <see cref="FakeTimeProvider.Advance"/>.</summary>
    public FakeTimeProvider Time => Factory.Options.TimeProvider ?? throw NotInitialized();

    public PostgreSqlContainerFixture Postgres => postgres;

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        _database = await postgres.CreateDatabaseAsync(cancellationToken);
        _factory = new DataTicketApiFactory(new ApiFactoryOptions
        {
            ConnectionString = _database.ConnectionString,
            TimeProvider = TestClock.Create(),
        });

        await _factory.MigrateDatabaseAsync(cancellationToken);
        await SeedAsync(_factory.Services, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }

    /// <summary>
    /// Punto de extensión de los datos de prueba. Hoy no hay tablas que sembrar; con Identity (B3, HU-003)
    /// aquí se crean las cuentas de <see cref="Fixtures.TestAccounts.All"/> con <c>UserManager</c>
    /// (contraseña <see cref="Fixtures.TestAccounts.SyntheticPassword"/>, salvo las <c>Invited</c>).
    /// </summary>
    private static Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        _ = services;
        _ = cancellationToken;
        return Task.CompletedTask;
    }

    private static InvalidOperationException NotInitialized() =>
        new("ApiFixture no está inicializada: úsala como IClassFixture<ApiFixture>.");
}
