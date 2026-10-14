using System.Reflection;
using System.Text.Json.Nodes;
using DataTicket.Domain.Auditing;
using DataTicket.Testing.Auditing;
using DataTicket.UnitTests.Doubles;

namespace DataTicket.UnitTests.Domain;

/// <summary>
/// HU-011: <see cref="AuditEntry"/> es inmutable y su factoría rechaza entradas incompletas (CHU-04, CHU-06);
/// <see cref="AuditAction"/> fija el catálogo de PRD §12 (CHU-07).
/// </summary>
public sealed class AuditEntryTests
{
    private static readonly Guid ActorId = Guid.Parse("b0000000-0000-7000-8000-000000000001");
    private static readonly Guid TicketId = Guid.Parse("d0000000-0000-7000-8000-000000000003");
    private static readonly DateTime OccurredAt = new(2026, 10, 7, 15, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithoutActor_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => Create(actorId: Guid.Empty));
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Create_WithNonUtcTimestamp_Throws(DateTimeKind kind)
    {
        Assert.ThrowsAny<ArgumentException>(() => Create(occurredAt: DateTime.SpecifyKind(OccurredAt, kind)));
    }

    [Fact]
    public void Create_WithEmptyObjectId_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => Create(objectId: Guid.Empty));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" Ticket")]
    [InlineData("Ticket ")]
    public void Create_WithoutObjectType_Throws(string objectType)
    {
        Assert.ThrowsAny<ArgumentException>(() => Create(objectType: objectType));
    }

    [Fact]
    public void Create_WithObjectTypeLongerThanColumn_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => Create(objectType: new string('T', AuditEntry.ObjectTypeMaxLength + 1)));
        Assert.Equal(AuditEntry.ObjectTypeMaxLength, Create(objectType: new string('T', AuditEntry.ObjectTypeMaxLength)).ObjectType.Length);
    }

    [Fact]
    public void Create_WithActionOutsideTheCatalog_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => Create(action: (AuditAction)999));
    }

    [Fact]
    public void Create_WithResultOutsideTheCatalog_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => AuditEntry.Create(
            AuditAction.StatusChanged, ActorId, OccurredAt, "Ticket", TicketId, TicketId, result: (AuditResult)999));
    }

    [Fact]
    public void Create_WithEmptyTicketId_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => AuditEntry.Create(
            AuditAction.StatusChanged, ActorId, OccurredAt, "Ticket", TicketId, ticketId: Guid.Empty));
    }

    [Theory]
    [InlineData("[1, 2]")]
    [InlineData("\"New\"")]
    [InlineData("42")]
    [InlineData("{ no es json")]
    public void Create_WithValueThatIsNotAJsonObject_Throws(string json)
    {
        Assert.ThrowsAny<ArgumentException>(() => Create(oldValue: json));
        Assert.ThrowsAny<ArgumentException>(() => Create(newValue: json));
    }

    [Fact]
    public void Create_WithValueThatSerializesToANonObject_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => Create(newValue: new[] { 1, 2 }));
    }

    [Fact]
    public void Create_KeepsAllFieldsWithUtcMicrosecondPrecisionAndVersion7Id()
    {
        var occurredAt = OccurredAt.AddTicks(1234567);

        var entry = AuditEntry.Create(
            AuditAction.StatusChanged,
            ActorId,
            occurredAt,
            "Ticket",
            TicketId,
            TicketId,
            oldValue: new { Status = SampleStatus.New },
            newValue: new { Status = SampleStatus.InProgress });

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(7, entry.Id.Version);
        Assert.Equal(TicketId, entry.TicketId);
        Assert.Equal(ActorId, entry.ActorId);
        Assert.Equal(DateTimeKind.Utc, entry.OccurredAt.Kind);
        Assert.Equal(OccurredAt.AddTicks(1234560), entry.OccurredAt);
        Assert.Equal(AuditAction.StatusChanged, entry.Action);
        Assert.Equal("Ticket", entry.ObjectType);
        Assert.Equal(TicketId, entry.ObjectId);
        Assert.Equal(AuditResult.Succeeded, entry.Result);
        // Claves en camelCase y valores de dominio en inglés (contrato de HU-011).
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("""{"status":"New"}"""), JsonNode.Parse(entry.OldValue!)));
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("""{"status":"InProgress"}"""), JsonNode.Parse(entry.NewValue!)));
    }

    [Fact]
    public void Create_WithoutTicketOrValues_LeavesThemNull()
    {
        var entry = Create(oldValue: null, newValue: null);

        Assert.Null(entry.OldValue);
        Assert.Null(entry.NewValue);
        Assert.Equal(TicketId, entry.TicketId);
        Assert.Null(AuditEntry.Create(AuditAction.TicketSubmitted, ActorId, OccurredAt, "Ticket", TicketId).TicketId);
    }

    [Fact]
    public void Create_AcceptsJsonTextAsIs()
    {
        var entry = Create(newValue: """{"assigneeIds":["b0000000-0000-7000-8000-000000000005"]}""");

        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse("""{"assigneeIds":["b0000000-0000-7000-8000-000000000005"]}"""),
            JsonNode.Parse(entry.NewValue!)));
    }

    [Fact]
    public void AuditEntry_HasNoPublicSetters()
    {
        var properties = typeof(AuditEntry).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var withPublicSetter = properties
            .Where(property => property.GetSetMethod(nonPublic: false) is not null)
            .Select(property => property.Name)
            .ToList();
        var publicConstructors = typeof(AuditEntry).GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        var publicFields = typeof(AuditEntry).GetFields(BindingFlags.Public | BindingFlags.Instance);

        // Si no hubiera propiedades, la regla pasaría sin mirar nada.
        Assert.NotEmpty(properties);
        Assert.True(withPublicSetter.Count == 0, $"AuditEntry no debe tener setters públicos: {string.Join(", ", withPublicSetter)}");
        Assert.Empty(publicConstructors);
        Assert.Empty(publicFields);
        Assert.True(typeof(AuditEntry).IsSealed);
    }

    [Fact]
    public void AuditAction_ContainsAllPrdEvents()
    {
        // PRD §12: radicación, prioridad, estado, asignación, reasignación, incorporación, retiro,
        // respuesta formal y cierre manual. Exactamente nueve; las HU emisoras no añaden valores sin T-01.
        string[] expected =
        [
            "TicketSubmitted",
            "PriorityOverridden",
            "StatusChanged",
            "TicketAssigned",
            "TicketReassigned",
            "ParticipantAdded",
            "ParticipantRemoved",
            "FormalResponseDelivered",
            "TicketClosed",
        ];

        Assert.Equal(expected.Order(StringComparer.Ordinal), Enum.GetNames<AuditAction>().Order(StringComparer.Ordinal));
    }

    [Fact]
    public void AuditResult_ContainsTheThreeValuesOfTheResultColumn()
    {
        Assert.Equal(["Denied", "Failed", "Succeeded"], Enum.GetNames<AuditResult>().Order(StringComparer.Ordinal));
        // El valor por defecto del enum coincide con el DEFAULT 'Succeeded' de la columna.
        Assert.Equal(AuditResult.Succeeded, default);
    }

    [Fact]
    public void AssertAuditEntry_ComparesValuesSemantically()
    {
        var log = new InMemoryAuditLog();
        log.Append(Create(oldValue: """{ "status" : "New" }""", newValue: new { Status = SampleStatus.InProgress }));
        log.Append(Create(action: AuditAction.TicketClosed, oldValue: new { status = "Resolved" }, newValue: new { status = "Closed" }));

        var entry = AuditAssert.AssertAuditEntry(
            log.Entries, AuditAction.StatusChanged, ActorId, "Ticket", TicketId,
            oldValue: new { status = "New" }, newValue: """{"status":"InProgress"}""");

        Assert.Same(log.Entries[0], entry);
        // Un valor distinto hace fallar la aserción.
        Assert.ThrowsAny<Exception>(() => AuditAssert.AssertAuditEntry(
            log.Entries, AuditAction.StatusChanged, ActorId, "Ticket", TicketId,
            oldValue: new { status = "Closed" }, newValue: new { status = "InProgress" }));
        // Otro actor también.
        Assert.ThrowsAny<Exception>(() => AuditAssert.AssertAuditEntry(
            log.Entries, AuditAction.StatusChanged, Guid.NewGuid(), "Ticket", TicketId,
            oldValue: new { status = "New" }, newValue: new { status = "InProgress" }));
    }

    private static AuditEntry Create(
        AuditAction action = AuditAction.StatusChanged,
        Guid? actorId = null,
        DateTime? occurredAt = null,
        string objectType = "Ticket",
        Guid? objectId = null,
        object? oldValue = null,
        object? newValue = null) =>
        AuditEntry.Create(
            action,
            actorId ?? ActorId,
            occurredAt ?? OccurredAt,
            objectType,
            objectId ?? TicketId,
            TicketId,
            oldValue,
            newValue);

    private enum SampleStatus
    {
        New,
        InProgress,
    }
}
