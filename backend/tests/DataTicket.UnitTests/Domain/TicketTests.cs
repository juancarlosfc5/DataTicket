using DataTicket.Domain.Tickets;
using DataTicket.Testing.Tickets;

namespace DataTicket.UnitTests.Domain;

/// <summary>
/// Agregado <see cref="Ticket"/> del núcleo N1: factoría de radicación (adelanto de HU-013 T-03) y las operaciones
/// mínimas de participación y asignación (adelanto de HU-018 y HU-019 T-02) que necesitan los fixtures y la regla
/// de acceso. Los nombres de prueba son los de esas tareas.
/// </summary>
public sealed class TicketTests
{
    private static readonly Guid CategoryId = Guid.Parse("01920000-0000-7000-8000-00000000ca7e");
    private static readonly DateTimeOffset SubmittedAt = new(2026, 10, 7, 15, 0, 0, TimeSpan.Zero);
    private static readonly Guid ParticipationId = Guid.Parse("01920000-0000-7000-8000-0000000000a2");

    [Fact]
    public void Submit_WithValidData_StartsInNew()
    {
        var ticket = NewTicket();

        Assert.Equal(TicketStatus.New, ticket.Status);
        Assert.Equal(TicketSeed.T03Id, ticket.Id);
        Assert.Equal("DT-000123", ticket.Number);
        Assert.Equal(TicketSeed.Company01, ticket.CompanyId);
        Assert.Equal(TicketSeed.AnaPerez, ticket.RequesterId);
        Assert.Equal(CategoryId, ticket.CategoryId);
        Assert.Equal(Urgency.High, ticket.Urgency);
        Assert.Equal(Impact.Medium, ticket.Impact);
        Assert.Equal(SubmittedAt, ticket.CreatedAt);
        Assert.Equal(SubmittedAt, ticket.UpdatedAt);
        Assert.Null(ticket.PullRequestUrl);
        Assert.Null(ticket.ClosedAt);
        Assert.Null(ticket.ClosedBy);
        Assert.Empty(ticket.Participants);
        Assert.Empty(ticket.Assignments);
    }

    [Fact]
    public void Submit_SetsCalculatedPriorityFromMatrix()
    {
        var ticket = NewTicket(urgency: Urgency.High, impact: Impact.Medium);

        Assert.Equal(CalculatedPriority.High, ticket.CalculatedPriority);
        // Sin ajuste manual (HU-015), la prioridad vigente es la calculada.
        Assert.Equal(ticket.CalculatedPriority, ticket.CurrentPriority);
    }

    [Fact]
    public void Submit_WithTitleOf120Chars_Succeeds()
    {
        var title = new string('a', 120);

        Assert.Equal(title, NewTicket(title: title).Title);
    }

    [Fact]
    public void Submit_WithTitleOf121Chars_Throws()
    {
        var error = Assert.Throws<ArgumentException>(() => NewTicket(title: new string('a', 121)));
        Assert.Equal("title", error.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Submit_WithBlankTitle_Throws(string title)
    {
        var error = Assert.Throws<ArgumentException>(() => NewTicket(title: title));
        Assert.Equal("title", error.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \n ")]
    public void Submit_WithBlankDescription_Throws(string description)
    {
        var error = Assert.Throws<ArgumentException>(() => NewTicket(description: description));
        Assert.Equal("description", error.ParamName);
    }

    [Fact]
    public void Submit_WithDescriptionOver10000Chars_Throws()
    {
        Assert.Equal(10_000, NewTicket(description: new string('d', 10_000)).Description.Length);
        var error = Assert.Throws<ArgumentException>(() => NewTicket(description: new string('d', 10_001)));
        Assert.Equal("description", error.ParamName);
    }

    [Fact]
    public void Submit_TrimsTitleAndStoresTimestampsInUtcMicroseconds()
    {
        // 10:00:00.1234567 en Bogotá (UTC−5) = 15:00:00.123456 UTC tras truncar a microsegundos (timestamptz).
        var local = new DateTimeOffset(2026, 10, 7, 10, 0, 0, TimeSpan.FromHours(-5)).AddTicks(1_234_567);

        var ticket = NewTicket(title: "  Error al facturar  ", submittedAt: local);

        Assert.Equal("Error al facturar", ticket.Title);
        Assert.Equal(TimeSpan.Zero, ticket.CreatedAt.Offset);
        Assert.Equal(new DateTimeOffset(2026, 10, 7, 15, 0, 0, TimeSpan.Zero).AddTicks(1_234_560), ticket.CreatedAt);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("companyId")]
    [InlineData("requesterId")]
    [InlineData("categoryId")]
    public void Submit_WithEmptyIdentifier_Throws(string parameter)
    {
        var error = Assert.Throws<ArgumentException>(() => Ticket.Submit(
            parameter == "id" ? Guid.Empty : TicketSeed.T03Id,
            "DT-000123",
            parameter == "companyId" ? Guid.Empty : TicketSeed.Company01,
            parameter == "requesterId" ? Guid.Empty : TicketSeed.AnaPerez,
            parameter == "categoryId" ? Guid.Empty : CategoryId,
            "Título",
            "Descripción",
            Urgency.Low,
            Impact.Low,
            SubmittedAt));
        Assert.Equal(parameter, error.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("DT-0000000000000000000001")] // 25 caracteres (varchar(24)).
    public void Submit_WithInvalidNumber_Throws(string number)
    {
        var error = Assert.Throws<ArgumentException>(() => NewTicket(number: number));
        Assert.Equal("number", error.ParamName);
    }

    [Fact]
    public void Submit_WithUndefinedUrgencyOrImpact_Throws()
    {
        Assert.Equal("urgency", Assert.Throws<ArgumentOutOfRangeException>(() => NewTicket(urgency: (Urgency)9)).ParamName);
        Assert.Equal("impact", Assert.Throws<ArgumentOutOfRangeException>(() => NewTicket(impact: (Impact)9)).ParamName);
    }

    [Fact]
    public void Assign_CreatesAssignmentAndActiveParticipation()
    {
        var ticket = NewTicket();
        var assignedAt = SubmittedAt.AddHours(1);

        var assignment = ticket.Assign(TicketSeed.Laura, TicketSeed.Elizabeth, assignedAt);

        Assert.Same(assignment, Assert.Single(ticket.Assignments));
        Assert.Equal(ticket.Id, assignment.TicketId);
        Assert.Equal(TicketSeed.Laura, assignment.AssigneeId);
        Assert.Equal(TicketSeed.Elizabeth, assignment.AssignedBy);
        Assert.Equal(assignedAt, assignment.AssignedAt);
        Assert.True(assignment.IsActive);
        Assert.NotEqual(Guid.Empty, assignment.Id);

        // Asignar implica participar (propuesta V-10).
        var participation = Assert.Single(ticket.Participants);
        Assert.Equal(TicketSeed.Laura, participation.UserId);
        Assert.Equal(ParticipationOrigin.Assignment, participation.Origin);
        Assert.Equal(TicketSeed.Elizabeth, participation.AddedBy);
        Assert.Equal(assignedAt, participation.AddedAt);
        Assert.True(ticket.HasActiveParticipant(TicketSeed.Laura));
    }

    [Fact]
    public void Assign_SeveralPeople_CreatesOneAssignmentEach()
    {
        var ticket = NewTicket();

        ticket.Assign(TicketSeed.Laura, TicketSeed.Elizabeth, SubmittedAt);
        ticket.Assign(TicketSeed.Brayan, TicketSeed.Elizabeth, SubmittedAt);

        Assert.Equal([TicketSeed.Laura, TicketSeed.Brayan], ticket.Assignments.Select(assignment => assignment.AssigneeId));
        Assert.Equal([TicketSeed.Laura, TicketSeed.Brayan], ticket.Participants.Select(participant => participant.UserId));
    }

    [Fact]
    public void Assign_AlreadyActiveAssignee_IsNoOp()
    {
        var ticket = NewTicket();
        var first = ticket.Assign(TicketSeed.Laura, TicketSeed.Elizabeth, SubmittedAt);

        var again = ticket.Assign(TicketSeed.Laura, TicketSeed.Admin, SubmittedAt.AddHours(1));

        Assert.Same(first, again);
        Assert.Single(ticket.Assignments);
        Assert.Single(ticket.Participants);
    }

    [Fact]
    public void Assign_ActiveParticipant_KeepsTheExistingParticipation()
    {
        var ticket = NewTicket();
        var participation = ticket.AddParticipant(TicketSeed.Kevin, ParticipationOrigin.Participant, TicketSeed.Elizabeth, SubmittedAt);

        ticket.Assign(TicketSeed.Kevin, TicketSeed.Elizabeth, SubmittedAt.AddHours(1));

        Assert.Same(participation, Assert.Single(ticket.Participants));
        Assert.Single(ticket.Assignments);
    }

    [Fact]
    public void Assign_UsesTheGivenIdentifiers()
    {
        var ticket = NewTicket();

        var assignment = ticket.Assign(
            TicketSeed.Laura, TicketSeed.Elizabeth, SubmittedAt, TicketSeed.T03LauraAssignmentId, TicketSeed.T03LauraParticipationId);

        Assert.Equal(TicketSeed.T03LauraAssignmentId, assignment.Id);
        Assert.Equal(TicketSeed.T03LauraParticipationId, Assert.Single(ticket.Participants).Id);
    }

    [Fact]
    public void AddParticipant_DoesNotCreateAssignment()
    {
        var ticket = NewTicket();

        var participation = ticket.AddParticipant(
            TicketSeed.Kevin, ParticipationOrigin.Participant, TicketSeed.Admin, SubmittedAt, ParticipationId);

        Assert.Empty(ticket.Assignments);
        Assert.Same(participation, Assert.Single(ticket.Participants));
        Assert.Equal(ParticipationId, participation.Id);
        Assert.Equal(ticket.Id, participation.TicketId);
        Assert.Equal(ParticipationOrigin.Participant, participation.Origin);
        Assert.Equal(TicketSeed.Admin, participation.AddedBy);
        Assert.True(participation.IsActive);
        Assert.Null(participation.RemovedAt);
        Assert.Null(participation.RemovedBy);
    }

    [Fact]
    public void AddParticipant_AlreadyActive_Throws()
    {
        var ticket = NewTicket();
        ticket.AddParticipant(TicketSeed.Kevin, ParticipationOrigin.Participant, TicketSeed.Admin, SubmittedAt);

        Assert.Throws<InvalidOperationException>(() =>
            ticket.AddParticipant(TicketSeed.Kevin, ParticipationOrigin.CoverageTake, TicketSeed.Admin, SubmittedAt));
        Assert.Single(ticket.Participants);
    }

    [Fact]
    public void AddParticipant_WithInvalidArguments_Throws()
    {
        var ticket = NewTicket();

        Assert.Equal("userId", Assert.Throws<ArgumentException>(() =>
            ticket.AddParticipant(Guid.Empty, ParticipationOrigin.Participant, TicketSeed.Admin, SubmittedAt)).ParamName);
        Assert.Equal("addedBy", Assert.Throws<ArgumentException>(() =>
            ticket.AddParticipant(TicketSeed.Kevin, ParticipationOrigin.Participant, Guid.Empty, SubmittedAt)).ParamName);
        Assert.Equal("origin", Assert.Throws<ArgumentOutOfRangeException>(() =>
            ticket.AddParticipant(TicketSeed.Kevin, (ParticipationOrigin)9, TicketSeed.Admin, SubmittedAt)).ParamName);
        Assert.Empty(ticket.Participants);
    }

    [Fact]
    public void RemoveParticipant_MarksRemovedAtAndRemovedBy_DoesNotDelete()
    {
        var ticket = NewTicket();
        var participation = ticket.AddParticipant(TicketSeed.Brayan, ParticipationOrigin.Participant, TicketSeed.Elizabeth, SubmittedAt);
        var removedAt = SubmittedAt.AddDays(2);

        var removed = ticket.RemoveParticipant(TicketSeed.Brayan, TicketSeed.Admin, removedAt);

        Assert.Same(participation, removed);
        Assert.Same(participation, Assert.Single(ticket.Participants));
        Assert.False(participation.IsActive);
        Assert.Equal(removedAt, participation.RemovedAt);
        Assert.Equal(TicketSeed.Admin, participation.RemovedBy);
        Assert.False(ticket.HasActiveParticipant(TicketSeed.Brayan));
    }

    [Fact]
    public void RemoveParticipant_Assignee_EndsAssignment()
    {
        var ticket = NewTicket();
        var assignment = ticket.Assign(TicketSeed.Laura, TicketSeed.Elizabeth, SubmittedAt);
        var removedAt = SubmittedAt.AddDays(1);

        ticket.RemoveParticipant(TicketSeed.Laura, TicketSeed.Elizabeth, removedAt);

        Assert.False(assignment.IsActive);
        Assert.Equal(removedAt, assignment.UnassignedAt);
        Assert.Equal(TicketSeed.Elizabeth, assignment.UnassignedBy);
        Assert.Same(assignment, Assert.Single(ticket.Assignments));
    }

    [Fact]
    public void RemoveParticipant_AlreadyRemoved_Throws()
    {
        var ticket = NewTicket();
        ticket.AddParticipant(TicketSeed.Brayan, ParticipationOrigin.Participant, TicketSeed.Elizabeth, SubmittedAt);
        ticket.RemoveParticipant(TicketSeed.Brayan, TicketSeed.Elizabeth, SubmittedAt.AddDays(1));

        Assert.Throws<InvalidOperationException>(() =>
            ticket.RemoveParticipant(TicketSeed.Brayan, TicketSeed.Elizabeth, SubmittedAt.AddDays(2)));
    }

    [Fact]
    public void RemoveParticipant_NeverParticipated_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            NewTicket().RemoveParticipant(TicketSeed.Kevin, TicketSeed.Elizabeth, SubmittedAt));
    }

    [Fact]
    public void RemoveParticipant_BeforeAddedAt_Throws()
    {
        var ticket = NewTicket();
        var participation = ticket.AddParticipant(TicketSeed.Brayan, ParticipationOrigin.Participant, TicketSeed.Elizabeth, SubmittedAt);

        var error = Assert.Throws<ArgumentException>(() =>
            ticket.RemoveParticipant(TicketSeed.Brayan, TicketSeed.Elizabeth, SubmittedAt.AddTicks(-10)));

        Assert.Equal("removedAt", error.ParamName);
        Assert.True(participation.IsActive);
    }

    [Fact]
    public void ReAdd_AfterRemoval_CreatesNewParticipation()
    {
        var ticket = NewTicket();
        var first = ticket.AddParticipant(TicketSeed.Brayan, ParticipationOrigin.Participant, TicketSeed.Elizabeth, SubmittedAt);
        ticket.RemoveParticipant(TicketSeed.Brayan, TicketSeed.Elizabeth, SubmittedAt.AddDays(1));

        var second = ticket.AddParticipant(TicketSeed.Brayan, ParticipationOrigin.Participant, TicketSeed.Elizabeth, SubmittedAt.AddDays(2));

        Assert.NotSame(first, second);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(2, ticket.Participants.Count);
        Assert.False(first.IsActive);
        Assert.True(second.IsActive);
        Assert.True(ticket.HasActiveParticipant(TicketSeed.Brayan));
    }

    [Fact]
    public void BuildT03_ReplicatesTheDbSqlScenario()
    {
        var ticket = TicketSeed.BuildT03(CategoryId);

        Assert.Equal(TicketSeed.T03Id, ticket.Id);
        Assert.Equal("DT-000003", ticket.Number);
        Assert.Equal(CalculatedPriority.High, ticket.CalculatedPriority);
        Assert.True(ticket.HasActiveParticipant(TicketSeed.Laura));
        Assert.True(ticket.HasActiveParticipant(TicketSeed.JuanDavid));
        Assert.False(ticket.HasActiveParticipant(TicketSeed.Brayan));
        Assert.False(ticket.HasActiveParticipant(TicketSeed.Kevin));
        Assert.False(ticket.HasActiveParticipant(TicketSeed.Elizabeth));
        Assert.False(ticket.HasActiveParticipant(TicketSeed.Admin));

        var brayan = Assert.Single(ticket.Participants, participant => participant.UserId == TicketSeed.Brayan);
        Assert.Equal(TicketSeed.T03BrayanRemovedAt, brayan.RemovedAt);
        Assert.Equal(TicketSeed.Elizabeth, brayan.RemovedBy);
        Assert.Equal(
            [TicketSeed.T03LauraAssignmentId, TicketSeed.T03JuanDavidAssignmentId],
            ticket.Assignments.Select(assignment => assignment.Id));
        Assert.All(ticket.Assignments, assignment => Assert.True(assignment.IsActive));
    }

    private static Ticket NewTicket(
        string number = "DT-000123",
        string title = "Error al generar la factura electrónica",
        string description = "Desde ayer el módulo de facturación devuelve un error 500 al emitir.",
        Urgency urgency = Urgency.High,
        Impact impact = Impact.Medium,
        DateTimeOffset? submittedAt = null) =>
        Ticket.Submit(
            TicketSeed.T03Id,
            number,
            TicketSeed.Company01,
            TicketSeed.AnaPerez,
            CategoryId,
            title,
            description,
            urgency,
            impact,
            submittedAt ?? SubmittedAt);
}
