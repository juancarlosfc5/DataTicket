using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Auditing;
using DataTicket.Domain.Companies;
using DataTicket.Infrastructure.Persistence;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using DataTicket.Testing.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Npgsql;

namespace DataTicket.IntegrationTests.Auditing;

/// <summary>
/// HU-011 contra PostgreSQL real: la entrada de auditoría se confirma o se revierte junto con la acción
/// (CHU-01 a CHU-03), la tabla rechaza UPDATE, DELETE y TRUNCATE (CHU-05) y la bitácora de un ticket tiene
/// orden determinista (CHU-08).
/// </summary>
/// <remarks>
/// La «acción» del caso de uso de ejemplo es crear una empresa (<c>dataticket.companies</c>), la única tabla de
/// dominio que existía en HU-011; la entrada describe un cambio de estado de ticket. El mecanismo no depende de la
/// acción: ambas comparten el <see cref="DataTicketDbContext"/> del ámbito y un único <c>SaveChangesAsync</c>.
/// Desde el núcleo N1 existe la FK <c>fk_audit_entries_ticket</c>: cada prueba radica su propio ticket
/// (<see cref="TestTickets.SubmitAsync"/>) para que sus entradas no se mezclen con las de otra.
/// </remarks>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class AuditLogTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    /// <summary>Instante fijo de CHU-01: 2026-10-07T15:00:00Z.</summary>
    private static readonly DateTimeOffset FixedNow = new(2026, 10, 7, 15, 0, 0, TimeSpan.Zero);

    private static readonly Guid ActorId = TestAccounts.Elizabeth.Id;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Append_IsCommittedWithTheActionInSameTransaction()
    {
        var ticketId = await NewTicketAsync();
        var companyName = UniqueCompanyName();

        await using (var scope = api.Factory.Services.CreateAsyncScope())
        {
            await SampleAuditedActionAsync(scope.ServiceProvider, companyName, ticketId, ActorId);
        }

        // Una sola fila, con cada columna como la fija el caso de uso (actor, instante UTC, acción, objeto,
        // valores jsonb y resultado).
        var rows = await QueryAsync(
            """
            SELECT actor_id::text, to_char(occurred_at AT TIME ZONE 'UTC', 'YYYY-MM-DD HH24:MI:SS'),
                   action, object_type, object_id::text, ticket_id::text,
                   old_value = '{"status":"New"}'::jsonb, new_value = '{"status":"InProgress"}'::jsonb, result
            FROM dataticket.audit_entries WHERE object_id = @id
            """,
            ticketId);
        var row = Assert.Single(rows);
        Assert.Equal(
            [ActorId.ToString(), "2026-10-07 15:00:00", "StatusChanged", "Ticket", ticketId.ToString(), ticketId.ToString(), "True", "True", "Succeeded"],
            row);
        Assert.Equal(1, await CountCompaniesAsync(companyName));

        // Lo mismo, leído por EF Core con la aserción que reutilizan las HU emisoras.
        var entries = await ReadEntriesAsync(ticketId);
        var entry = AuditAssert.AssertAuditEntry(
            entries, AuditAction.StatusChanged, ActorId, "Ticket", ticketId,
            oldValue: new { status = "New" }, newValue: new { status = "InProgress" });
        Assert.Equal(FixedNow.UtcDateTime, entry.OccurredAt);
        Assert.Equal(ticketId, entry.TicketId);
    }

    [Fact]
    public async Task Append_DoesNotSaveChanges()
    {
        var ticketId = await NewTicketAsync();

        await using var scope = api.Factory.Services.CreateAsyncScope();
        var auditLog = scope.ServiceProvider.GetRequiredService<IAuditLog>();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();
        var entry = NewEntry(ticketId, ActorId);

        auditLog.Append(entry);

        // Queda en la unidad de trabajo del ámbito, pendiente de que el caso de uso confirme.
        Assert.Equal(EntityState.Added, dbContext.Entry(entry).State);
        Assert.Equal(0, await CountEntriesAsync(ticketId));
    }

    [Fact]
    public async Task Append_IsRolledBackWhenActionFails()
    {
        var companyName = UniqueCompanyName();
        await using (var scope = api.Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();
            dbContext.Companies.Add(new Company(Guid.CreateVersion7(), companyName, FixedNow));
            await dbContext.SaveChangesAsync(Ct);
        }

        var ticketId = await NewTicketAsync();
        DbUpdateException failure;
        await using (var scope = api.Factory.Services.CreateAsyncScope())
        {
            // La acción viola ux_companies_name_ci (mismo nombre sin distinguir mayúsculas).
            failure = await Assert.ThrowsAsync<DbUpdateException>(() =>
                SampleAuditedActionAsync(scope.ServiceProvider, companyName.ToUpperInvariant(), ticketId, ActorId));
        }

        var postgres = Assert.IsType<PostgresException>(failure.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgres.SqlState);
        Assert.Equal("ux_companies_name_ci", postgres.ConstraintName);
        Assert.Equal(0, await CountEntriesAsync(ticketId));
        Assert.Equal(1, await CountCompaniesAsync(companyName));
    }

    [Fact]
    public async Task ActionIsRolledBackWhenAuditInsertFails()
    {
        var companyName = UniqueCompanyName();
        var ticketId = await NewTicketAsync();
        // Un actor que no existe en identity.users: la inserción de auditoría viola fk_audit_entries_actor.
        var unknownActor = Guid.CreateVersion7();

        DbUpdateException failure;
        await using (var scope = api.Factory.Services.CreateAsyncScope())
        {
            failure = await Assert.ThrowsAsync<DbUpdateException>(() =>
                SampleAuditedActionAsync(scope.ServiceProvider, companyName, ticketId, unknownActor));
        }

        var postgres = Assert.IsType<PostgresException>(failure.InnerException);
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, postgres.SqlState);
        Assert.Equal("fk_audit_entries_actor", postgres.ConstraintName);
        // Nunca queda una acción auditable confirmada sin su entrada.
        Assert.Equal(0, await CountCompaniesAsync(companyName));
        Assert.Equal(0, await CountEntriesAsync(ticketId));
    }

    [Fact]
    public async Task UpdateOrDeleteOnAuditTable_IsRejectedByDatabase()
    {
        var ticketId = await NewTicketAsync();
        await using (var scope = api.Factory.Services.CreateAsyncScope())
        {
            await SampleAuditedActionAsync(scope.ServiceProvider, UniqueCompanyName(), ticketId, ActorId);
        }

        const string Snapshot = "SELECT row_to_json(a)::text FROM dataticket.audit_entries a WHERE object_id = @id";
        var before = Assert.Single(await QueryAsync(Snapshot, ticketId));

        // Con la misma cadena de conexión que usa la aplicación.
        foreach (var sql in new[]
        {
            "UPDATE dataticket.audit_entries SET action = 'TicketClosed', new_value = '{\"status\":\"Closed\"}' WHERE object_id = @id",
            "DELETE FROM dataticket.audit_entries WHERE object_id = @id",
            "TRUNCATE dataticket.audit_entries",
        })
        {
            var rejected = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(sql, ticketId));
            Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, rejected.SqlState);
            Assert.Contains("audit_entries es inmutable", rejected.MessageText, StringComparison.Ordinal);
        }

        Assert.Equal(before, Assert.Single(await QueryAsync(Snapshot, ticketId)));
    }

    [Fact]
    public async Task EntriesOfSameTicketAndInstant_AreOrderedDeterministicallyById()
    {
        var ticketId = await NewTicketAsync();
        var occurredAt = FixedNow.UtcDateTime;
        var first = AuditEntry.Create(AuditAction.TicketSubmitted, ActorId, occurredAt, "Ticket", ticketId, ticketId);
        var second = AuditEntry.Create(AuditAction.ParticipantAdded, ActorId, occurredAt, "TicketParticipant", Guid.CreateVersion7(), ticketId);
        var third = AuditEntry.Create(AuditAction.TicketAssigned, ActorId, occurredAt, "Assignment", Guid.CreateVersion7(), ticketId);
        // Orden esperado: el de PostgreSQL para uuid (bytes en orden, igual que el texto en minúsculas).
        var expected = new[] { first, second, third }
            .Select(entry => entry.Id)
            .OrderBy(id => id.ToString("D"), StringComparer.Ordinal)
            .ToList();

        // Se insertan al revés del orden esperado para que el resultado no dependa del orden de inserción.
        foreach (var id in Enumerable.Reverse(expected))
        {
            await using var scope = api.Factory.Services.CreateAsyncScope();
            scope.ServiceProvider.GetRequiredService<IAuditLog>().Append(new[] { first, second, third }.Single(entry => entry.Id == id));
            await scope.ServiceProvider.GetRequiredService<DataTicketDbContext>().SaveChangesAsync(Ct);
        }

        for (var run = 0; run < 3; run++)
        {
            Assert.Equal(expected, (await ReadEntriesAsync(ticketId)).Select(entry => entry.Id));
            var bySql = await QueryAsync(
                "SELECT id::text FROM dataticket.audit_entries WHERE ticket_id = @id ORDER BY occurred_at, id", ticketId);
            Assert.Equal(expected.Select(id => id.ToString()), bySql.Select(row => row[0]));
        }

        var index = Assert.Single(await QueryAsync(
            "SELECT indexdef FROM pg_indexes WHERE schemaname = 'dataticket' AND indexname = 'ix_audit_entries_ticket_cursor'",
            Guid.Empty));
        Assert.EndsWith("(ticket_id, occurred_at, id)", index[0], StringComparison.Ordinal);
    }

    /// <summary>
    /// Caso de uso de ejemplo: ejecuta una acción (crear una empresa) y registra su entrada de auditoría con el
    /// actor y el reloj recibidos; confirma ambas con un único <c>SaveChangesAsync</c>, como harán los casos de
    /// uso emisores a través de su unidad de trabajo.
    /// </summary>
    private static async Task SampleAuditedActionAsync(IServiceProvider services, string companyName, Guid ticketId, Guid actorId)
    {
        var clock = new FakeTimeProvider(FixedNow);
        var dbContext = services.GetRequiredService<DataTicketDbContext>();
        var auditLog = services.GetRequiredService<IAuditLog>();

        dbContext.Companies.Add(new Company(Guid.CreateVersion7(clock.GetUtcNow()), companyName, clock.GetUtcNow()));
        auditLog.Append(AuditEntry.Create(
            AuditAction.StatusChanged,
            actorId,
            clock.GetUtcNow().UtcDateTime,
            "Ticket",
            ticketId,
            ticketId,
            oldValue: new { status = "New" },
            newValue: new { status = "InProgress" }));

        await dbContext.SaveChangesAsync(Ct);
    }

    private Task<Guid> NewTicketAsync() => TestTickets.SubmitAsync(api.Factory.Services, configure: null, Ct);

    private static AuditEntry NewEntry(Guid ticketId, Guid actorId) =>
        AuditEntry.Create(AuditAction.StatusChanged, actorId, FixedNow.UtcDateTime, "Ticket", ticketId, ticketId);

    private static string UniqueCompanyName() => $"Empresa Auditada {Guid.NewGuid():N}";

    private async Task<List<AuditEntry>> ReadEntriesAsync(Guid ticketId)
    {
        await using var scope = api.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<DataTicketDbContext>().AuditEntries
            .AsNoTracking()
            .Where(entry => entry.TicketId == ticketId)
            .OrderBy(entry => entry.OccurredAt)
            .ThenBy(entry => entry.Id)
            .ToListAsync(Ct);
    }

    private async Task<long> CountEntriesAsync(Guid ticketId) =>
        long.Parse(Assert.Single(await QueryAsync(
            "SELECT count(*)::text FROM dataticket.audit_entries WHERE object_id = @id OR ticket_id = @id", ticketId))[0],
            System.Globalization.CultureInfo.InvariantCulture);

    private async Task<long> CountCompaniesAsync(string name)
    {
        await using var connection = new NpgsqlConnection(api.Database.ConnectionString);
        await connection.OpenAsync(Ct);
        await using var command = new NpgsqlCommand(
            "SELECT count(*) FROM dataticket.companies WHERE lower(name) = lower(@name)", connection);
        command.Parameters.AddWithValue("name", name);
        return (long)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task ExecuteAsync(string sql, Guid id)
    {
        await using var connection = new NpgsqlConnection(api.Database.ConnectionString);
        await connection.OpenAsync(Ct);
        await using var command = new NpgsqlCommand(sql, connection);
        if (sql.Contains("@id", StringComparison.Ordinal))
        {
            command.Parameters.AddWithValue("id", id);
        }

        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<List<string[]>> QueryAsync(string sql, Guid id)
    {
        await using var connection = new NpgsqlConnection(api.Database.ConnectionString);
        await connection.OpenAsync(Ct);
        await using var command = new NpgsqlCommand(sql, connection);
        if (sql.Contains("@id", StringComparison.Ordinal))
        {
            command.Parameters.AddWithValue("id", id);
        }

        await using var reader = await command.ExecuteReaderAsync(Ct);
        var rows = new List<string[]>();
        while (await reader.ReadAsync(Ct))
        {
            var row = new string[reader.FieldCount];
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[i] = Convert.ToString(reader.GetValue(i), System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            }

            rows.Add(row);
        }

        return rows;
    }
}
