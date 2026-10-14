using DataTicket.Infrastructure.Persistence;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DataTicket.IntegrationTests.Persistence;

/// <summary>
/// HU-011 (DoD de la migración): <c>Hu011_AddAuditEntries</c> crea la tabla de <c>db.sql</c> §11 con sus
/// restricciones, índices, triggers append-only y la función <c>reject_mutation</c>, y su <c>Down</c> lo
/// elimina todo.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class AuditMigrationTests(PostgreSqlContainerFixture postgres)
{
    private const string MigrationSuffix = "_Hu011_AddAuditEntries";

    private const string AuditObjectsSql =
        """
        SELECT 'table:' || table_name FROM information_schema.tables
            WHERE table_schema = 'dataticket' AND table_name = 'audit_entries'
        UNION ALL
        SELECT 'trigger:' || tgname FROM pg_trigger WHERE NOT tgisinternal AND tgname LIKE 'trg_audit_entries_%'
        UNION ALL
        SELECT 'function:' || p.proname FROM pg_proc p JOIN pg_namespace n ON n.oid = p.pronamespace
            WHERE n.nspname = 'dataticket' AND p.proname = 'reject_mutation'
        UNION ALL
        SELECT 'constraint:' || conname FROM pg_constraint WHERE conname LIKE '%audit_entries%'
        UNION ALL
        SELECT 'index:' || indexname || ' ' || indexdef FROM pg_indexes
            WHERE schemaname = 'dataticket' AND tablename = 'audit_entries'
        UNION ALL
        SELECT 'default:' || column_name || '=' || column_default FROM information_schema.columns
            WHERE table_schema = 'dataticket' AND table_name = 'audit_entries' AND column_default IS NOT NULL
        UNION ALL
        SELECT 'column:' || column_name || ' ' || data_type || ' ' || is_nullable
            || coalesce(' ' || character_maximum_length::text, '') FROM information_schema.columns
            WHERE table_schema = 'dataticket' AND table_name = 'audit_entries'
        """;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Hu011_migration_creates_audit_table_as_in_db_sql_and_Down_removes_it()
    {
        await using var database = await postgres.CreateDatabaseAsync(Ct);
        await using var factory = new DataTicketApiFactory(new ApiFactoryOptions
        {
            ConnectionString = database.ConnectionString,
            ApplyMigrationsOnStartup = true,
        });
        _ = factory.Services; // Arranca el host y migra (Development).

        var created = await QueryListAsync(database.ConnectionString, AuditObjectsSql);
        foreach (var expected in new[]
        {
            "table:audit_entries",
            "trigger:trg_audit_entries_no_update_delete",
            "trigger:trg_audit_entries_no_truncate",
            "function:reject_mutation",
            "constraint:pk_audit_entries",
            "constraint:fk_audit_entries_actor",
            "constraint:ck_audit_entries_action",
            "constraint:ck_audit_entries_object_type",
            "constraint:ck_audit_entries_result",
            "default:id=uuidv7()",
            "default:occurred_at=now()",
            "default:result='Succeeded'::character varying",
            "column:id uuid NO",
            "column:ticket_id uuid YES",
            "column:actor_id uuid NO",
            "column:occurred_at timestamp with time zone NO",
            "column:action character varying NO 60",
            "column:object_type character varying NO 40",
            "column:object_id uuid NO",
            "column:old_value jsonb YES",
            "column:new_value jsonb YES",
            "column:result character varying NO 20",
        })
        {
            Assert.Contains(expected, created);
        }

        Assert.Contains(created, item => item.StartsWith("index:ix_audit_entries_ticket_cursor ", StringComparison.Ordinal)
            && item.EndsWith("(ticket_id, occurred_at, id)", StringComparison.Ordinal));
        Assert.Contains(created, item => item.StartsWith("index:ix_audit_entries_actor_occurred ", StringComparison.Ordinal)
            && item.EndsWith("(actor_id, occurred_at)", StringComparison.Ordinal));
        Assert.Contains(created, item => item.StartsWith("index:ix_audit_entries_object ", StringComparison.Ordinal)
            && item.EndsWith("(object_type, object_id, occurred_at)", StringComparison.Ordinal));
        // Solo los tres índices de db.sql y la clave primaria: EF no añade un índice propio para la FK del actor.
        Assert.Equal(4, created.Count(item => item.StartsWith("index:", StringComparison.Ordinal)));
        // Exactamente las 10 columnas de db.sql.
        Assert.Equal(10, created.Count(item => item.StartsWith("column:", StringComparison.Ordinal)));

        // Down: se vuelve a la migración anterior y no queda rastro; después se puede volver a aplicar.
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();
            var migrations = dbContext.Database.GetMigrations().ToList();
            var index = migrations.FindIndex(id => id.EndsWith(MigrationSuffix, StringComparison.Ordinal));
            Assert.True(index > 0, "No se encontró la migración Hu011_AddAuditEntries.");
            var migrator = dbContext.GetService<IMigrator>();

            await migrator.MigrateAsync(migrations[index - 1], Ct);
            Assert.Empty(await QueryListAsync(database.ConnectionString, AuditObjectsSql));
            Assert.Contains(
                "companies",
                await QueryListAsync(database.ConnectionString, "SELECT table_name FROM information_schema.tables WHERE table_schema = 'dataticket'"));

            await migrator.MigrateAsync(cancellationToken: Ct);
        }

        Assert.Equal(created.Order(StringComparer.Ordinal), (await QueryListAsync(database.ConnectionString, AuditObjectsSql)).Order(StringComparer.Ordinal));
    }

    private static async Task<List<string>> QueryListAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
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
}
