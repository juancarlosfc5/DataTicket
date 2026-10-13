using Npgsql;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Base de datos aislada dentro del contenedor de pruebas. Al desecharla se borra (con <c>FORCE</c>,
/// que cierra las conexiones que queden abiertas).
/// </summary>
public sealed class TestDatabase(string name, string connectionString, string adminConnectionString) : IAsyncDisposable
{
    public string Name { get; } = name;

    public string ConnectionString { get; } = connectionString;

    public async ValueTask DisposeAsync()
    {
        await using (var pooled = new NpgsqlConnection(ConnectionString))
        {
            NpgsqlConnection.ClearPool(pooled);
        }

        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{Name}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }
}
