using DataTicket.Infrastructure.Persistence;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DataTicket.IntegrationTests.Persistence;

/// <summary>
/// Núcleo N1: <c>HuN1_NucleoTickets</c> crea <c>categories</c>, <c>tickets</c>, <c>ticket_participants</c> y
/// <c>assignments</c> como en <c>db.sql</c> §1, §4, §6 y §12 (funciones, secuencia, restricciones, índices, valores
/// por defecto y semilla de categorías), añade la FK <c>fk_audit_entries_ticket</c> que dejó pendiente HU-011, y su
/// <c>Down</c> lo elimina todo sin tocar la auditoría.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class TicketsMigrationTests(PostgreSqlContainerFixture postgres)
{
    private const string MigrationSuffix = "_HuN1_NucleoTickets";

    private const string TicketTables = "'categories', 'tickets', 'ticket_participants', 'assignments'";

    private const string TicketObjectsSql =
        $"""
        SELECT 'table:' || table_name FROM information_schema.tables
            WHERE table_schema = 'dataticket' AND table_name IN ({TicketTables})
        UNION ALL
        SELECT 'function:' || p.proname FROM pg_proc p JOIN pg_namespace n ON n.oid = p.pronamespace
            WHERE n.nspname = 'dataticket' AND p.proname IN ('priority_matrix', 'next_ticket_number')
        UNION ALL
        SELECT 'sequence:' || sequence_name FROM information_schema.sequences WHERE sequence_schema = 'dataticket'
        UNION ALL
        SELECT 'constraint:' || c.conname || ' ' || pg_get_constraintdef(c.oid) FROM pg_constraint c
            JOIN pg_class t ON t.oid = c.conrelid JOIN pg_namespace n ON n.oid = t.relnamespace
            WHERE n.nspname = 'dataticket' AND (t.relname IN ({TicketTables}) OR c.conname = 'fk_audit_entries_ticket')
        UNION ALL
        SELECT 'index:' || indexname || ' ' || indexdef FROM pg_indexes
            WHERE schemaname = 'dataticket' AND tablename IN ({TicketTables})
        UNION ALL
        SELECT 'default:' || table_name || '.' || column_name || '=' || column_default FROM information_schema.columns
            WHERE table_schema = 'dataticket' AND table_name IN ({TicketTables}) AND column_default IS NOT NULL
        UNION ALL
        SELECT 'column:' || table_name || '.' || column_name || ' ' || data_type || ' ' || is_nullable
            || coalesce(' ' || character_maximum_length::text, '') FROM information_schema.columns
            WHERE table_schema = 'dataticket' AND table_name IN ({TicketTables})
        """;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task HuN1_migration_creates_ticket_core_as_in_db_sql_and_Down_removes_it()
    {
        await using var database = await postgres.CreateDatabaseAsync(Ct);
        await using var factory = new DataTicketApiFactory(new ApiFactoryOptions
        {
            ConnectionString = database.ConnectionString,
            ApplyMigrationsOnStartup = true,
        });
        _ = factory.Services; // Arranca el host y migra (Development).

        var created = await QueryListAsync(database.ConnectionString, TicketObjectsSql);

        foreach (var expected in new[]
        {
            "table:categories", "table:tickets", "table:ticket_participants", "table:assignments",
            "function:priority_matrix", "function:next_ticket_number", "sequence:ticket_number_seq",
            // tickets (db.sql §6)
            "column:tickets.id uuid NO", "column:tickets.number character varying NO 24",
            "column:tickets.company_id uuid NO", "column:tickets.requester_id uuid NO", "column:tickets.category_id uuid NO",
            "column:tickets.title character varying NO 120", "column:tickets.description text NO",
            "column:tickets.urgency character varying NO 10", "column:tickets.impact character varying NO 10",
            "column:tickets.calculated_priority character varying NO 10", "column:tickets.current_priority character varying NO 10",
            "column:tickets.status character varying NO 20", "column:tickets.pull_request_url character varying YES 2048",
            "column:tickets.created_at timestamp with time zone NO", "column:tickets.updated_at timestamp with time zone NO",
            "column:tickets.closed_at timestamp with time zone YES", "column:tickets.closed_by uuid YES",
            "default:tickets.id=uuidv7()", "default:tickets.number=dataticket.next_ticket_number()",
            "default:tickets.status='New'::character varying", "default:tickets.created_at=now()", "default:tickets.updated_at=now()",
            // ticket_participants
            "column:ticket_participants.id uuid NO", "column:ticket_participants.ticket_id uuid NO",
            "column:ticket_participants.user_id uuid NO", "column:ticket_participants.origin character varying NO 20",
            "column:ticket_participants.added_by uuid NO", "column:ticket_participants.added_at timestamp with time zone NO",
            "column:ticket_participants.removed_by uuid YES", "column:ticket_participants.removed_at timestamp with time zone YES",
            "default:ticket_participants.id=uuidv7()", "default:ticket_participants.added_at=now()",
            // assignments
            "column:assignments.id uuid NO", "column:assignments.ticket_id uuid NO", "column:assignments.assignee_id uuid NO",
            "column:assignments.assigned_by uuid NO", "column:assignments.assigned_at timestamp with time zone NO",
            "column:assignments.unassigned_by uuid YES", "column:assignments.unassigned_at timestamp with time zone YES",
            "default:assignments.id=uuidv7()", "default:assignments.assigned_at=now()",
            // categories (db.sql §4)
            "column:categories.id uuid NO", "column:categories.name character varying NO 80",
            "column:categories.sort_order integer NO", "column:categories.is_active boolean NO",
            "column:categories.created_at timestamp with time zone NO",
            "default:categories.id=uuidv7()", "default:categories.sort_order=0", "default:categories.is_active=true",
            "default:categories.created_at=now()",
        })
        {
            Assert.Contains(expected, created);
        }

        // Exactamente las columnas de db.sql, salvo form_template_version_id y custom_field_values (Fase 4).
        Assert.Equal(17, created.Count(item => item.StartsWith("column:tickets.", StringComparison.Ordinal)));
        Assert.Equal(8, created.Count(item => item.StartsWith("column:ticket_participants.", StringComparison.Ordinal)));
        Assert.Equal(7, created.Count(item => item.StartsWith("column:assignments.", StringComparison.Ordinal)));
        Assert.Equal(5, created.Count(item => item.StartsWith("column:categories.", StringComparison.Ordinal)));

        AssertConstraint(created, "pk_tickets", "PRIMARY KEY (id)");
        AssertConstraint(created, "ux_tickets_number", "UNIQUE (number)");
        AssertConstraint(created, "fk_tickets_company", "FOREIGN KEY (company_id) REFERENCES dataticket.companies(id) ON DELETE RESTRICT");
        AssertConstraint(
            created, "fk_tickets_requester_company",
            "FOREIGN KEY (requester_id, company_id) REFERENCES identity.users(id, company_id) ON UPDATE RESTRICT ON DELETE RESTRICT");
        AssertConstraint(created, "fk_tickets_category", "FOREIGN KEY (category_id) REFERENCES dataticket.categories(id) ON DELETE RESTRICT");
        AssertConstraint(created, "fk_tickets_closed_by", "FOREIGN KEY (closed_by) REFERENCES identity.users(id) ON DELETE RESTRICT");
        AssertConstraint(created, "ck_tickets_title", "CHECK ((((title)::text = btrim((title)::text)) AND ((char_length((title)::text) >= 1) AND (char_length((title)::text) <= 120))))");
        AssertConstraint(created, "ck_tickets_description", "CHECK (((char_length(btrim(description)) >= 1) AND (char_length(btrim(description)) <= 10000)))");
        AssertConstraint(created, "ck_tickets_calculated_priority", "CHECK (((calculated_priority)::text = (dataticket.priority_matrix(urgency, impact))::text))");
        AssertConstraint(created, "ck_tickets_pull_request_url", "CHECK (((pull_request_url IS NULL) OR ((pull_request_url)::text ~ '^https://'::text)))");
        AssertConstraint(created, "ck_tickets_closure", "CHECK (((((status)::text = 'Closed'::text) = (closed_at IS NOT NULL)) AND ((closed_at IS NULL) = (closed_by IS NULL))))");
        foreach (var name in new[] { "ck_tickets_urgency", "ck_tickets_impact", "ck_tickets_current_priority", "ck_tickets_status" })
        {
            Assert.Contains(created, item => item.StartsWith($"constraint:{name} CHECK", StringComparison.Ordinal));
        }

        AssertConstraint(created, "pk_ticket_participants", "PRIMARY KEY (id)");
        AssertConstraint(created, "fk_ticket_participants_ticket", "FOREIGN KEY (ticket_id) REFERENCES dataticket.tickets(id) ON DELETE RESTRICT");
        AssertConstraint(created, "fk_ticket_participants_user", "FOREIGN KEY (user_id) REFERENCES identity.users(id) ON DELETE RESTRICT");
        AssertConstraint(created, "fk_ticket_participants_added_by", "FOREIGN KEY (added_by) REFERENCES identity.users(id) ON DELETE RESTRICT");
        AssertConstraint(created, "fk_ticket_participants_removed_by", "FOREIGN KEY (removed_by) REFERENCES identity.users(id) ON DELETE RESTRICT");
        AssertConstraint(created, "ck_ticket_participants_origin", "CHECK (((origin)::text = ANY ((ARRAY['Assignment'::character varying, 'Participant'::character varying, 'CoverageTake'::character varying])::text[])))");
        AssertConstraint(created, "ck_ticket_participants_removal", "CHECK ((((removed_at IS NULL) = (removed_by IS NULL)) AND ((removed_at IS NULL) OR (removed_at >= added_at))))");

        AssertConstraint(created, "pk_assignments", "PRIMARY KEY (id)");
        AssertConstraint(created, "fk_assignments_ticket", "FOREIGN KEY (ticket_id) REFERENCES dataticket.tickets(id) ON DELETE RESTRICT");
        AssertConstraint(created, "fk_assignments_assignee", "FOREIGN KEY (assignee_id) REFERENCES identity.users(id) ON DELETE RESTRICT");
        AssertConstraint(created, "fk_assignments_assigned_by", "FOREIGN KEY (assigned_by) REFERENCES identity.users(id) ON DELETE RESTRICT");
        AssertConstraint(created, "fk_assignments_unassigned_by", "FOREIGN KEY (unassigned_by) REFERENCES identity.users(id) ON DELETE RESTRICT");
        AssertConstraint(created, "ck_assignments_unassignment", "CHECK ((((unassigned_at IS NULL) = (unassigned_by IS NULL)) AND ((unassigned_at IS NULL) OR (unassigned_at >= assigned_at))))");

        AssertConstraint(created, "pk_categories", "PRIMARY KEY (id)");
        AssertConstraint(created, "ck_categories_name", "CHECK ((((name)::text = btrim((name)::text)) AND (char_length((name)::text) >= 1)))");

        // La FK que HU-011 dejó pendiente.
        AssertConstraint(created, "fk_audit_entries_ticket", "FOREIGN KEY (ticket_id) REFERENCES dataticket.tickets(id) ON DELETE RESTRICT");

        foreach (var (name, definition) in new[]
        {
            ("ix_tickets_company_requester_created", "(company_id, requester_id, created_at, id)"),
            ("ix_tickets_company_created", "(company_id, created_at, id)"),
            ("ix_tickets_status_created", "(status, created_at, id)"),
            ("ix_tickets_created_at", "(created_at)"),
            ("ix_tickets_category_id", "(category_id)"),
            ("ix_tickets_requester_id", "(requester_id)"),
            ("ux_ticket_participants_active", "(ticket_id, user_id) WHERE (removed_at IS NULL)"),
            ("ix_ticket_participants_user_removed", "(user_id, removed_at)"),
            ("ix_ticket_participants_ticket_id", "(ticket_id)"),
            ("ux_assignments_active", "(ticket_id, assignee_id) WHERE (unassigned_at IS NULL)"),
            ("ix_assignments_assignee_active", "(assignee_id) WHERE (unassigned_at IS NULL)"),
            ("ix_assignments_ticket_id", "(ticket_id)"),
            ("ux_categories_name_ci", "(lower((name)::text))"),
        })
        {
            Assert.Contains(created, item => item.StartsWith($"index:{name} ", StringComparison.Ordinal)
                && item.EndsWith(definition, StringComparison.Ordinal));
        }

        // Solo los índices de db.sql: EF Core no añade índices propios para las FK.
        Assert.Equal(
            ["ix_tickets_category_id", "ix_tickets_company_created", "ix_tickets_company_requester_created", "ix_tickets_created_at",
             "ix_tickets_requester_id", "ix_tickets_status_created", "pk_tickets", "ux_tickets_number"],
            IndexNames(created, "tickets"));
        Assert.Equal(
            ["ix_ticket_participants_ticket_id", "ix_ticket_participants_user_removed", "pk_ticket_participants", "ux_ticket_participants_active"],
            IndexNames(created, "ticket_participants"));
        Assert.Equal(
            ["ix_assignments_assignee_active", "ix_assignments_ticket_id", "pk_assignments", "ux_assignments_active"],
            IndexNames(created, "assignments"));
        Assert.Equal(["pk_categories", "ux_categories_name_ci"], IndexNames(created, "categories"));

        // Semilla de referencia de db.sql §12 (V-12) y la matriz de db.sql §1.
        Assert.Equal(
            ["Incidente|1|True", "Requerimiento|2|True", "Consulta|3|True"],
            await QueryListAsync(
                database.ConnectionString,
                "SELECT name || '|' || sort_order || '|' || CASE WHEN is_active THEN 'True' ELSE 'False' END FROM dataticket.categories ORDER BY sort_order"));
        Assert.Equal(
            ["Critical", "VeryLow", "High"],
            await QueryListAsync(
                database.ConnectionString,
                "SELECT dataticket.priority_matrix(u, i) FROM (VALUES (1, 'High', 'High'), (2, 'Low', 'Low'), (3, 'Medium', 'High')) AS v(n, u, i) ORDER BY n"));

        // Down: se vuelve a HU-011; no queda rastro del núcleo y la auditoría sigue ahí. Después se reaplica.
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();
            var migrations = dbContext.Database.GetMigrations().ToList();
            var index = migrations.FindIndex(id => id.EndsWith(MigrationSuffix, StringComparison.Ordinal));
            Assert.True(index > 0, "No se encontró la migración HuN1_NucleoTickets.");
            Assert.EndsWith("_Hu011_AddAuditEntries", migrations[index - 1], StringComparison.Ordinal);
            var migrator = dbContext.GetService<IMigrator>();

            await migrator.MigrateAsync(migrations[index - 1], Ct);
            Assert.Empty(await QueryListAsync(database.ConnectionString, TicketObjectsSql));
            Assert.Contains(
                "audit_entries",
                await QueryListAsync(database.ConnectionString, "SELECT table_name FROM information_schema.tables WHERE table_schema = 'dataticket'"));

            await migrator.MigrateAsync(cancellationToken: Ct);
        }

        Assert.Equal(
            created.Order(StringComparer.Ordinal),
            (await QueryListAsync(database.ConnectionString, TicketObjectsSql)).Order(StringComparer.Ordinal));
    }

    private static void AssertConstraint(List<string> created, string name, string definition) =>
        Assert.Contains($"constraint:{name} {definition}", created);

    private static List<string> IndexNames(List<string> created, string table) =>
        [.. created
            .Where(item => item.StartsWith("index:", StringComparison.Ordinal)
                && item.Contains($" ON dataticket.{table} USING ", StringComparison.Ordinal))
            .Select(item => item["index:".Length..item.IndexOf(' ', StringComparison.Ordinal)])
            .Order(StringComparer.Ordinal)];

    private static async Task<List<string>> QueryListAsync(string connectionString, string sql)
    {
        // search_path fijo: el catálogo muestra los objetos de otros esquemas calificados (p. ej.
        // dataticket.next_ticket_number()) aunque la sesión de la aplicación tenga dataticket en su search_path.
        await using var connection = new NpgsqlConnection(new NpgsqlConnectionStringBuilder(connectionString) { SearchPath = "public" }.ConnectionString);
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
