using DataTicket.Domain.Tickets;

namespace DataTicket.Testing.Tickets;

/// <summary>
/// Escenario de tickets del núcleo N1 que replica <c>db.sql</c> §13 con los mismos identificadores. Se enlaza como
/// código fuente en <c>DataTicket.UnitTests</c> (agregados en memoria) y en <c>DataTicket.IntegrationTests</c>
/// (se persiste con EF Core en <c>TestTickets</c>), de modo que las pruebas unitarias y las de integración hablan
/// del mismo ticket.
/// </summary>
/// <remarks>
/// T03 (<c>DT-000003</c>, Empresa 01, radicado por Ana Pérez, <c>InDevelopment</c>) es el ticket principal del chat
/// (HU-026 a HU-032): Laura y Juan David son responsables y participantes vigentes desde el 2026-09-22; Brayan
/// participó desde el 2026-09-23 y fue retirado el 2026-09-25; Kevin es el «interno no participante»; Elizabeth
/// (PM) accede sin participar; el administrador no participa.
/// </remarks>
internal static class TicketSeed
{
    // db.sql §13.2: a0000000-0000-7000-8000-0000000000NN.
    public static readonly Guid Elizabeth = Internal(1);
    public static readonly Guid Julian = Internal(2);
    public static readonly Guid Gerardo = Internal(3);
    public static readonly Guid JuanDavid = Internal(4);
    public static readonly Guid Laura = Internal(5);
    public static readonly Guid Brayan = Internal(6);
    public static readonly Guid Cristian = Internal(7);
    public static readonly Guid Kevin = Internal(8);
    public static readonly Guid Admin = Internal(9);

    // db.sql §13.1 y §13.3.
    public static readonly Guid Company01 = Guid.Parse("c0000000-0000-7000-8000-000000000001");
    public static readonly Guid AnaPerez = Guid.Parse("b0000000-0000-7000-8000-000001000001");

    // db.sql §13.6 (T03).
    public static readonly Guid T03Id = Guid.Parse("d0000000-0000-7000-8000-000000000003");
    public const string T03Number = "DT-000003";
    public const string T03CategoryName = "Incidente";
    public const string T03Title = "Consecutivo de facturas duplicado";
    public const string T03Description = "Dos facturas del 21 de septiembre salieron con el mismo consecutivo.";
    public const TicketStatus T03Status = TicketStatus.InDevelopment;
    public static readonly DateTimeOffset T03CreatedAt = Utc(2026, 9, 22, 13, 30);
    public static readonly DateTimeOffset T03UpdatedAt = Utc(2026, 9, 26, 16, 30);

    // db.sql §13.8 y §13.9 (T03).
    public static readonly Guid T03LauraAssignmentId = Guid.Parse("a3000000-0000-7000-8000-000000000001");
    public static readonly Guid T03JuanDavidAssignmentId = Guid.Parse("a3000000-0000-7000-8000-000000000002");
    public static readonly Guid T03LauraParticipationId = Guid.Parse("a2000000-0000-7000-8000-000000000001");
    public static readonly Guid T03JuanDavidParticipationId = Guid.Parse("a2000000-0000-7000-8000-000000000002");
    public static readonly Guid T03BrayanParticipationId = Guid.Parse("a2000000-0000-7000-8000-000000000003");
    public static readonly DateTimeOffset T03AssignedAt = Utc(2026, 9, 22, 15, 0);
    public static readonly DateTimeOffset T03BrayanAddedAt = Utc(2026, 9, 23, 14, 0);
    public static readonly DateTimeOffset T03BrayanRemovedAt = Utc(2026, 9, 25, 20, 0);

    /// <summary>Participantes vigentes de T03.</summary>
    public static IReadOnlyList<Guid> T03ActiveParticipants { get; } = [Laura, JuanDavid];

    /// <summary>
    /// Construye T03 con el dominio: radicación, asignación de Laura y Juan David por Elizabeth, incorporación y
    /// retiro de Brayan. El estado <c>InDevelopment</c> y <c>updated_at</c> no tienen todavía operación de dominio
    /// (HU-021): quien persiste el escenario los fija aparte (ver <c>TestTickets</c>).
    /// </summary>
    public static Ticket BuildT03(Guid categoryId)
    {
        var ticket = Ticket.Submit(
            T03Id,
            T03Number,
            Company01,
            AnaPerez,
            categoryId,
            T03Title,
            T03Description,
            Urgency.Medium,
            Impact.High,
            T03CreatedAt);

        ticket.Assign(Laura, Elizabeth, T03AssignedAt, T03LauraAssignmentId, T03LauraParticipationId);
        ticket.Assign(JuanDavid, Elizabeth, T03AssignedAt, T03JuanDavidAssignmentId, T03JuanDavidParticipationId);
        ticket.AddParticipant(Brayan, ParticipationOrigin.Participant, Elizabeth, T03BrayanAddedAt, T03BrayanParticipationId);
        ticket.RemoveParticipant(Brayan, Elizabeth, T03BrayanRemovedAt);
        return ticket;
    }

    private static Guid Internal(int number) => Guid.Parse($"a0000000-0000-7000-8000-{number:D12}");

    private static DateTimeOffset Utc(int year, int month, int day, int hour, int minute) =>
        new(year, month, day, hour, minute, 0, TimeSpan.Zero);
}
