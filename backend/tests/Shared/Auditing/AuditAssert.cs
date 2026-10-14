using System.Text.Json.Nodes;
using DataTicket.Domain.Auditing;

namespace DataTicket.Testing.Auditing;

/// <summary>
/// Aserción reutilizable de HU-011 (T-06) para las HU emisoras de eventos auditables. Se enlaza como código
/// fuente en <c>DataTicket.UnitTests</c> (entradas del doble <c>InMemoryAuditLog</c>) y en
/// <c>DataTicket.IntegrationTests</c> (entradas leídas de <c>dataticket.audit_entries</c>).
/// </summary>
internal static class AuditAssert
{
    /// <summary>
    /// Exige exactamente una entrada con <paramref name="action"/> sobre el objeto
    /// (<paramref name="objectType"/>, <paramref name="objectId"/>) y comprueba actor, resultado
    /// <see cref="AuditResult.Succeeded"/> y valores anterior/nuevo. Los valores esperados se escriben como en la
    /// factoría (objeto anónimo, texto JSON o <c>null</c>) y se comparan como JSON, sin depender del orden de
    /// las claves ni de los espacios (PostgreSQL normaliza <c>jsonb</c>). Devuelve la entrada encontrada.
    /// </summary>
    public static AuditEntry AssertAuditEntry(
        IEnumerable<AuditEntry> entries,
        AuditAction action,
        Guid actorId,
        string objectType,
        Guid objectId,
        object? oldValue,
        object? newValue)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var matching = entries
            .Where(entry => entry.Action == action && entry.ObjectType == objectType && entry.ObjectId == objectId)
            .ToList();
        Assert.True(
            matching.Count == 1,
            $"Se esperaba exactamente una entrada {action} sobre {objectType} {objectId}; hay {matching.Count}.");

        var entry = matching[0];
        AssertAuditEntry(entry, action, actorId, objectType, objectId, oldValue, newValue);
        return entry;
    }

    /// <summary>Como la sobrecarga de colección, sobre una entrada concreta.</summary>
    public static void AssertAuditEntry(
        AuditEntry entry,
        AuditAction action,
        Guid actorId,
        string objectType,
        Guid objectId,
        object? oldValue,
        object? newValue)
    {
        ArgumentNullException.ThrowIfNull(entry);

        Assert.Equal(action, entry.Action);
        Assert.Equal(actorId, entry.ActorId);
        Assert.Equal(objectType, entry.ObjectType);
        Assert.Equal(objectId, entry.ObjectId);
        Assert.Equal(AuditResult.Succeeded, entry.Result);
        Assert.Equal(DateTimeKind.Utc, entry.OccurredAt.Kind);
        AssertJsonEqual(nameof(AuditEntry.OldValue), AuditValue.ToJson(oldValue), entry.OldValue);
        AssertJsonEqual(nameof(AuditEntry.NewValue), AuditValue.ToJson(newValue), entry.NewValue);
    }

    private static void AssertJsonEqual(string field, string? expected, string? actual)
    {
        if (expected is null || actual is null)
        {
            Assert.True(expected is null && actual is null, $"{field}: se esperaba {expected ?? "null"} y es {actual ?? "null"}.");
            return;
        }

        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual)),
            $"{field}: se esperaba {expected} y es {actual}.");
    }
}
