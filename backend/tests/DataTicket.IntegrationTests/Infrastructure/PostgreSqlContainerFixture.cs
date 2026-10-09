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

    private readonly SemaphoreSlim _startLock = new(1, 1);

    // Build() ya comprueba que Docker responda: se construye al arrancar, no en el constructor.
    private PostgreSqlContainer? _container;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Crea una base <c>dataticket_test_&lt;guid&gt;</c> vacía en el contenedor. Se borra al desechar el resultado.
    /// </summary>
    public async Task<TestDatabase> CreateDatabaseAsync(CancellationToken cancellationToken)
    {
        var container = await EnsureStartedAsync(cancellationToken);

        var adminConnectionString = container.GetConnectionString();
        var name = $"dataticket_test_{Guid.NewGuid():N}";

        await using (var connection = new NpgsqlConnection(adminConnectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
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
                var container = new PostgreSqlBuilder(Image).Build();
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
