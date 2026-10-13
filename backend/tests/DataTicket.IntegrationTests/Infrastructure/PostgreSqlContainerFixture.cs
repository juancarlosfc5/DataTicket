using Npgsql;
using Testcontainers.PostgreSql;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Fixture de ensamblado: un contenedor <c>postgres:18.6</c> (la misma imagen del Compose) por ejecución.
/// Cada consumidor pide una base propia con <see cref="CreateDatabaseAsync"/>; nunca se usa el servicio
/// <c>db</c> de Compose. El contenedor se arranca de forma perezosa en la primera petición, de modo que las
/// pruebas que no necesitan PostgreSQL corren aunque no haya Docker. Testcontainers (Ryuk) elimina el
/// contenedor aunque el proceso termine de forma abrupta.
/// </summary>
public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    public const string Image = "postgres:18.6";

    /// <summary>
    /// Usuario y base de mantenimiento del contenedor: los mismos nombres que el servicio <c>db</c> del Compose
    /// (<c>POSTGRES_USER</c> y <c>POSTGRES_DB</c>). No es un detalle estético: el usuario se llama igual que el
    /// esquema <c>dataticket</c> de la aplicación y el <c>search_path</c> por defecto es <c>"$user", public</c>,
    /// así que, en cuanto ese esquema existe, pasa a ser el esquema actual de cada sesión. Con el usuario
    /// <c>postgres</c> que Testcontainers usa por defecto, las pruebas no vieron el fallo de arranque de B7
    /// (historial de migraciones sin esquema explícito).
    /// </summary>
    public const string ComposeUser = "dataticket";

    private readonly SemaphoreSlim _startLock = new(1, 1);

    // Build() ya comprueba que Docker responda: se construye al arrancar, no en el constructor.
    private PostgreSqlContainer? _container;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Crea una base <c>dataticket_test_&lt;guid&gt;</c> vacía en el contenedor. Se borra al desechar el resultado.
    /// </summary>
    public Task<TestDatabase> CreateDatabaseAsync(CancellationToken cancellationToken) =>
        CreateDatabaseAsync(searchPath: null, cancellationToken);

    /// <summary>
    /// Como <see cref="CreateDatabaseAsync(CancellationToken)"/>, fijando además el <c>search_path</c> por defecto
    /// de las sesiones de esa base (<c>ALTER DATABASE … SET search_path</c>), p. ej. <c>dataticket, public</c>.
    /// Solo para pruebas que dependen de él; el valor es un literal de la prueba, nunca una entrada externa.
    /// </summary>
    public async Task<TestDatabase> CreateDatabaseAsync(string? searchPath, CancellationToken cancellationToken)
    {
        var container = await EnsureStartedAsync(cancellationToken);

        var adminConnectionString = container.GetConnectionString();
        var name = $"dataticket_test_{Guid.NewGuid():N}";

        // Se ejecuta por la conexión de administración (otra base, otro pool): el search_path de una base solo
        // rige para las sesiones abiertas DESPUÉS del ALTER, y aún no hay ninguna contra la base nueva.
        await using (var connection = new NpgsqlConnection(adminConnectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
            await command.ExecuteNonQueryAsync(cancellationToken);

            if (searchPath is not null)
            {
                await using var alter = new NpgsqlCommand($"ALTER DATABASE \"{name}\" SET search_path = {searchPath}", connection);
                await alter.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        var connectionString = new NpgsqlConnectionStringBuilder(adminConnectionString) { Database = name }.ConnectionString;
        return new TestDatabase(name, connectionString, adminConnectionString);
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }

        _startLock.Dispose();
    }

    private async Task<PostgreSqlContainer> EnsureStartedAsync(CancellationToken cancellationToken)
    {
        if (_container is { } started)
        {
            return started;
        }

        await _startLock.WaitAsync(cancellationToken);
        try
        {
            if (_container is null)
            {
                var container = new PostgreSqlBuilder(Image)
                    .WithUsername(ComposeUser)
                    .WithDatabase(ComposeUser)
                    .Build();
                try
                {
                    await container.StartAsync(cancellationToken);
                }
                catch
                {
                    await container.DisposeAsync();
                    throw;
                }

                _container = container;
            }

            return _container;
        }
        finally
        {
            _startLock.Release();
        }
    }
}
