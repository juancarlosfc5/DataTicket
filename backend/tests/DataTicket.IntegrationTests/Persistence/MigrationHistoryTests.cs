using DataTicket.Infrastructure.Persistence;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DataTicket.IntegrationTests.Persistence;

/// <summary>
/// Regresión de B7: el backend no arrancaba por segunda vez sobre una base ya migrada (42P07,
/// <c>relation "companies" already exists</c>).
/// </summary>
/// <remarks>
/// <para>
/// Causa: la tabla de historial de migraciones no tenía esquema explícito, así que EF la creaba y la buscaba
/// en el primer esquema EXISTENTE del <c>search_path</c>. En el Compose el usuario de PostgreSQL se llama
/// <c>dataticket</c>, igual que el esquema de la aplicación, y el <c>search_path</c> por defecto es
/// <c>"$user", public</c>: en el primer arranque el esquema <c>dataticket</c> aún no existe y el historial
/// queda en <c>public</c>; en el segundo ya existe, EF busca el historial allí, no lo encuentra y reaplica la
/// migración inicial.
/// </para>
/// <para>
/// Estas pruebas reproducen la condición sin depender del nombre del usuario del contenedor: fijan en la base
/// <c>search_path = dataticket, public</c>, que tiene exactamente el mismo efecto que <c>"$user", public</c>
/// con un usuario llamado <c>dataticket</c> (PostgreSQL ignora los esquemas del <c>search_path</c> que no existen).
/// </para>
/// </remarks>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class MigrationHistoryTests(PostgreSqlContainerFixture postgres)
{
    /// <summary>El esquema de la aplicación por delante de <c>public</c>, como en el Compose.</summary>
    private const string ComposeLikeSearchPath = "dataticket, public";

    private const string HistoryTable = "__EFMigrationsHistory";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Segundo_arranque_sobre_una_base_migrada_no_reaplica_las_migraciones()
    {
        await using var database = await postgres.CreateDatabaseAsync(ComposeLikeSearchPath, Ct);
        // Si la base no tuviera ese search_path, la prueba pasaría sin reproducir la condición.
        Assert.Equal(ComposeLikeSearchPath, await ScalarAsync(database, "SHOW search_path"));

        var migrations = await StartDevelopmentHostAsync(database);
        Assert.NotEmpty(migrations);
        var afterFirstStart = await HistoryAsync(database);

        // Segundo arranque (reinicio del contenedor del backend): el esquema dataticket ya existe.
        await StartDevelopmentHostAsync(database);

        // Una sola tabla de historial, en public, con una fila por migración: nada se reaplicó.
        var afterSecondStart = await HistoryAsync(database);
        Assert.Equal([$"public.{HistoryTable}"], afterSecondStart.Tables);
        Assert.Equal(migrations, afterSecondStart.AppliedMigrations);
        Assert.Equal(afterFirstStart.Tables, afterSecondStart.Tables);
        Assert.Equal(afterFirstStart.AppliedMigrations, afterSecondStart.AppliedMigrations);
        Assert.Equal("1", await ScalarAsync(database, "SELECT count(*)::text FROM information_schema.tables WHERE table_schema = 'dataticket' AND table_name = 'companies'"));
    }

    /// <summary>
    /// Estado en que quedó el <c>db</c> del Compose tras el arranque fallido: el historial bueno en
    /// <c>public</c> y una tabla de historial VACÍA sobrante en <c>dataticket</c>. El backend debe arrancar
    /// leyendo el de <c>public</c>, sin reaplicar nada y sin que la tabla sobrante estorbe.
    /// </summary>
    [Fact]
    public async Task Arranque_se_recupera_con_un_historial_vacio_sobrante_en_dataticket()
    {
        await using var database = await postgres.CreateDatabaseAsync(ComposeLikeSearchPath, Ct);
        var migrations = await StartDevelopmentHostAsync(database);
        await ExecuteAsync(
            database,
            $"""
            CREATE TABLE dataticket."{HistoryTable}" (
                migration_id character varying(150) NOT NULL PRIMARY KEY,
                product_version character varying(32) NOT NULL)
            """);

        await StartDevelopmentHostAsync(database);

        var history = await HistoryAsync(database);
        Assert.Equal([$"dataticket.{HistoryTable}", $"public.{HistoryTable}"], history.Tables);
        Assert.Equal(migrations, history.AppliedMigrations);
        Assert.Equal("0", await ScalarAsync(database, $"SELECT count(*)::text FROM dataticket.\"{HistoryTable}\""));
    }

    /// <summary>
    /// Arranca un host en Development (migra al arrancar), lo apaga y devuelve las migraciones del ensamblado.
    /// </summary>
    private static async Task<List<string>> StartDevelopmentHostAsync(TestDatabase database)
    {
        await using var host = new DataTicketApiFactory(new ApiFactoryOptions
        {
            ConnectionString = database.ConnectionString,
            TimeProvider = TestClock.Create(),
            ApplyMigrationsOnStartup = true,
        });

        await using var scope = host.Services.CreateAsyncScope(); // host.Services arranca el host.
        var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();
        Assert.Empty(await dbContext.Database.GetPendingMigrationsAsync(Ct));
        return [.. dbContext.Database.GetMigrations()];
    }

    private static async Task<(List<string> Tables, List<string> AppliedMigrations)> HistoryAsync(TestDatabase database)
    {
        var tables = await ListAsync(
            database,
            $"""
            SELECT table_schema || '.' || table_name FROM information_schema.tables
            WHERE table_name = '{HistoryTable}' ORDER BY table_schema
            """);
        var applied = await ListAsync(database, $"SELECT migration_id FROM public.\"{HistoryTable}\" ORDER BY migration_id");
        return (tables, applied);
    }

    private static async Task<string?> ScalarAsync(TestDatabase database, string sql) =>
        (await ListAsync(database, sql)).SingleOrDefault();

    private static async Task<List<string>> ListAsync(TestDatabase database, string sql)
    {
        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync(Ct);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(Ct);

        var values = new List<string>();
        while (await reader.ReadAsync(Ct))
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }

    private static async Task ExecuteAsync(TestDatabase database, string sql)
    {
        await using var connection = new NpgsqlConnection(database.ConnectionString);
        await connection.OpenAsync(Ct);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
