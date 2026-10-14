using System.Globalization;
using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Auditing;
using DataTicket.Domain.Tickets;
using DataTicket.Infrastructure.Persistence;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using DataTicket.Testing.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DataTicket.IntegrationTests.Tickets;

/// <summary>
/// Núcleo N1 contra PostgreSQL real: el repositorio reconstruye el agregado T03 tal como lo siembra
/// <see cref="TestTickets"/> (igual que <c>db.sql</c> §13) y la base hace cumplir las reglas de <c>db.sql</c> §6
/// aunque alguien se salte el dominio (participación vigente única, matriz de prioridad, solicitante de la empresa
/// del ticket, sin borrado físico) y la FK pendiente de HU-011 (<c>fk_audit_entries_ticket</c>).
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class TicketPersistenceTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static readonly Guid UnknownTicketId = Guid.Parse("d0000000-0000-7000-8000-000000000999");

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Repositorio_carga_T03_con_sus_participaciones_y_asignaciones()
    {
        await using var scope = api.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();
        var ticket = await scope.ServiceProvider.GetRequiredService<ITicketRepository>().FindAsync(TestTickets.T03Id, Ct);

        Assert.NotNull(ticket);
        Assert.Equal("DT-000003", ticket.Number);
        Assert.Equal(TestCompanies.Company01.Id, ticket.CompanyId);
        Assert.Equal(TestAccounts.AnaPerez.Id, ticket.RequesterId);
        Assert.Equal(await TestTickets.CategoryIdAsync(dbContext, "Incidente", Ct), ticket.CategoryId);
        Assert.Equal(TicketSeed.T03Title, ticket.Title);
        Assert.Equal(TicketSeed.T03Description, ticket.Description);
        Assert.Equal(Urgency.Medium, ticket.Urgency);
        Assert.Equal(Impact.High, ticket.Impact);
        Assert.Equal(CalculatedPriority.High, ticket.CalculatedPriority);
        Assert.Equal(CalculatedPriority.High, ticket.CurrentPriority);
        Assert.Equal(TicketStatus.InDevelopment, ticket.Status);
        Assert.Equal(TicketSeed.T03CreatedAt, ticket.CreatedAt);
        Assert.Equal(TicketSeed.T03UpdatedAt, ticket.UpdatedAt);
        Assert.Null(ticket.PullRequestUrl);
        Assert.Null(ticket.ClosedAt);
        Assert.Null(ticket.ClosedBy);

        Assert.Equal(
            [
                (TicketSeed.T03LauraParticipationId, TestAccounts.Laura.Id, ParticipationOrigin.Assignment, (DateTimeOffset?)null),
                (TicketSeed.T03JuanDavidParticipationId, TestAccounts.JuanDavid.Id, ParticipationOrigin.Assignment, null),
                (TicketSeed.T03BrayanParticipationId, TestAccounts.Brayan.Id, ParticipationOrigin.Participant, TicketSeed.T03BrayanRemovedAt),
            ],
            ticket.Participants
                .OrderBy(participant => participant.Id)
                .Select(participant => (participant.Id, participant.UserId, participant.Origin, participant.RemovedAt)));
        Assert.All(ticket.Participants, participant => Assert.Equal(TestAccounts.Elizabeth.Id, participant.AddedBy));
        Assert.Equal(TestAccounts.Elizabeth.Id, ticket.Participants.Single(participant => !participant.IsActive).RemovedBy);

        Assert.Equal(
            [(TicketSeed.T03LauraAssignmentId, TestAccounts.Laura.Id), (TicketSeed.T03JuanDavidAssignmentId, TestAccounts.JuanDavid.Id)],
            ticket.Assignments.OrderBy(assignment => assignment.Id).Select(assignment => (assignment.Id, assignment.AssigneeId)));
        Assert.All(ticket.Assignments, assignment =>
        {
            Assert.True(assignment.IsActive);
            Assert.Equal(TestAccounts.Elizabeth.Id, assignment.AssignedBy);
            Assert.Equal(TicketSeed.T03AssignedAt, assignment.AssignedAt);
        });
    }

    [Fact]
    public async Task Repositorio_responde_las_consultas_de_acceso()
    {
        await using var scope = api.Factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();

        Assert.True(await repository.ExistsAsync(TestTickets.T03Id, Ct));
        Assert.False(await repository.ExistsAsync(UnknownTicketId, Ct));
        Assert.True(await repository.IsActiveParticipantAsync(TestTickets.T03Id, TestAccounts.Laura.Id, Ct));
        Assert.True(await repository.IsActiveParticipantAsync(TestTickets.T03Id, TestAccounts.JuanDavid.Id, Ct));
        Assert.False(await repository.IsActiveParticipantAsync(TestTickets.T03Id, TestAccounts.Brayan.Id, Ct));
        Assert.False(await repository.IsActiveParticipantAsync(TestTickets.T03Id, TestAccounts.Kevin.Id, Ct));
        Assert.False(await repository.IsActiveParticipantAsync(TestTickets.T03Id, TestAccounts.Elizabeth.Id, Ct));
        Assert.False(await repository.IsActiveParticipantAsync(UnknownTicketId, TestAccounts.Laura.Id, Ct));
        Assert.Null(await repository.FindAsync(UnknownTicketId, Ct));
    }

    [Fact]
    public async Task Base_rechaza_una_segunda_participacion_vigente()
    {
        var error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            $"""
            INSERT INTO dataticket.ticket_participants (ticket_id, user_id, origin, added_by)
            VALUES ('{TestTickets.T03Id}', '{TestAccounts.Laura.Id}', 'Participant', '{TestAccounts.Elizabeth.Id}')
            """));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, error.SqlState);
        Assert.Equal("ux_ticket_participants_active", error.ConstraintName);
    }

    [Fact]
    public async Task Base_permite_reincorporar_a_un_retirado_con_una_participacion_nueva()
    {
        var ticketId = await TestTickets.SubmitAsync(api.Factory.Services, ticket =>
        {
            var at = api.Time.GetUtcNow();
            ticket.AddParticipant(TestAccounts.Brayan.Id, ParticipationOrigin.Participant, TestAccounts.Elizabeth.Id, at);
            ticket.RemoveParticipant(TestAccounts.Brayan.Id, TestAccounts.Elizabeth.Id, at);
            ticket.AddParticipant(TestAccounts.Brayan.Id, ParticipationOrigin.Participant, TestAccounts.Elizabeth.Id, at);
        }, Ct);

        Assert.Equal(
            "2|1",
            await ScalarAsync(
                $"SELECT count(*) || '|' || count(*) FILTER (WHERE removed_at IS NULL) FROM dataticket.ticket_participants WHERE ticket_id = '{ticketId}'"));
    }

    [Fact]
    public async Task Base_rechaza_una_prioridad_calculada_que_no_sigue_la_matriz()
    {
        // T03: urgencia Medium e impacto High → High. Low viola ck_tickets_calculated_priority.
        var error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            $"UPDATE dataticket.tickets SET calculated_priority = 'Low' WHERE id = '{TestTickets.T03Id}'"));

        Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
        Assert.Equal("ck_tickets_calculated_priority", error.ConstraintName);
    }

    [Fact]
    public async Task Base_rechaza_un_solicitante_de_otra_empresa()
    {
        // Ana Pérez es de la Empresa 01: un ticket de la Empresa 02 a su nombre viola la FK compuesta (invariante 1).
        var error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            $"""
            INSERT INTO dataticket.tickets (company_id, requester_id, category_id, title, description, urgency, impact,
                                            calculated_priority, current_priority)
            SELECT '{TestCompanies.Company02.Id}', '{TestAccounts.AnaPerez.Id}', id, 'Título', 'Descripción', 'Low', 'Low',
                   'VeryLow', 'VeryLow'
            FROM dataticket.categories WHERE name = 'Incidente'
            """));

        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, error.SqlState);
        Assert.Equal("fk_tickets_requester_company", error.ConstraintName);
    }

    [Fact]
    public async Task Base_asigna_numero_estado_e_id_por_defecto_despues_de_la_semilla()
    {
        var row = await ScalarAsync(
            $"""
            INSERT INTO dataticket.tickets (company_id, requester_id, category_id, title, description, urgency, impact,
                                            calculated_priority, current_priority)
            SELECT '{TestCompanies.Company01.Id}', '{TestAccounts.AnaPerez.Id}', id, 'Título', 'Descripción', 'High', 'High',
                   'Critical', 'Critical'
            FROM dataticket.categories WHERE name = 'Incidente'
            RETURNING number || '|' || status || '|' || (uuid_extract_version(id))::text
            """);

        var parts = row.Split('|');
        // La semilla reserva DT-000001 a DT-000010 (db.sql §13.6): los números nuevos son mayores.
        Assert.Matches("^DT-[0-9]{6}$", parts[0]);
        Assert.True(int.Parse(parts[0][3..], CultureInfo.InvariantCulture) > 10, parts[0]);
        Assert.Equal("New", parts[1]);
        Assert.Equal("7", parts[2]);
    }

    [Fact]
    public async Task Base_no_permite_borrar_un_ticket()
    {
        var error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(
            $"DELETE FROM dataticket.tickets WHERE id = '{TestTickets.T03Id}'"));

        // Sin borrado físico: las participaciones y asignaciones lo impiden (ON DELETE RESTRICT → 23001).
        Assert.Equal(PostgresErrorCodes.RestrictViolation, error.SqlState);
        Assert.True(await RepositoryExistsAsync(TestTickets.T03Id));

        async Task<bool> RepositoryExistsAsync(Guid id)
        {
            await using var scope = api.Factory.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ITicketRepository>().ExistsAsync(id, Ct);
        }
    }

    [Fact]
    public async Task Auditoria_exige_que_el_ticket_exista()
    {
        await using var scope = api.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();
        var auditLog = scope.ServiceProvider.GetRequiredService<IAuditLog>();
        var now = api.Time.GetUtcNow().UtcDateTime;

        auditLog.Append(AuditEntry.Create(
            AuditAction.StatusChanged, TestAccounts.Elizabeth.Id, now, "Ticket", UnknownTicketId, UnknownTicketId));
        var failure = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync(Ct));

        var postgres = Assert.IsType<PostgresException>(failure.InnerException);
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, postgres.SqlState);
        Assert.Equal("fk_audit_entries_ticket", postgres.ConstraintName);
    }

    [Fact]
    public async Task Auditoria_de_T03_se_guarda_con_su_ticket()
    {
        await using var scope = api.Factory.Services.CreateAsyncScope();
        var now = api.Time.GetUtcNow().UtcDateTime;

        scope.ServiceProvider.GetRequiredService<IAuditLog>().Append(AuditEntry.Create(
            AuditAction.ParticipantRemoved, TestAccounts.Elizabeth.Id, now, "TicketParticipant",
            TicketSeed.T03BrayanParticipationId, TestTickets.T03Id));
        await scope.ServiceProvider.GetRequiredService<DataTicketDbContext>().SaveChangesAsync(Ct);

        Assert.Equal(
            "1",
            await ScalarAsync(
                $"SELECT count(*)::text FROM dataticket.audit_entries WHERE ticket_id = '{TestTickets.T03Id}' AND object_id = '{TicketSeed.T03BrayanParticipationId}'"));
    }

    private async Task ExecuteAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(api.Database.ConnectionString);
        await connection.OpenAsync(Ct);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<string> ScalarAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(api.Database.ConnectionString);
        await connection.OpenAsync(Ct);
        await using var command = new NpgsqlCommand(sql, connection);
        return Convert.ToString(await command.ExecuteScalarAsync(Ct), CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
