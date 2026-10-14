using DataTicket.Domain.Common;

namespace DataTicket.Domain.Auditing;

/// <summary>
/// Entrada de la bitácora de auditoría (glosario <c>AuditEntry</c>, PRD §12, HU-011): quién (actor), cuándo
/// (UTC), qué (<see cref="AuditAction"/>), sobre qué objeto (tipo + id, y el ticket si lo hay), con qué valores
/// anterior/nuevo y con qué resultado. Es inmutable y append-only: solo se crea con <see cref="Create"/> y se
/// inserta por el puerto <c>IAuditLog</c>; la base rechaza además UPDATE, DELETE y TRUNCATE (trigger
/// <c>reject_mutation</c>).
/// </summary>
public sealed class AuditEntry
{
    /// <summary>Longitud de <c>object_type varchar(40)</c> en <c>db.sql</c> §11.</summary>
    public const int ObjectTypeMaxLength = 40;

    /// <summary>Longitud de <c>action varchar(60)</c> en <c>db.sql</c> §11.</summary>
    public const int ActionMaxLength = 60;

    /// <summary>Longitud de <c>result varchar(20)</c> en <c>db.sql</c> §11.</summary>
    public const int ResultMaxLength = 20;

    private AuditEntry(
        Guid id,
        Guid? ticketId,
        Guid actorId,
        DateTime occurredAt,
        AuditAction action,
        string objectType,
        Guid objectId,
        string? oldValue,
        string? newValue,
        AuditResult result)
    {
        Id = id;
        TicketId = ticketId;
        ActorId = actorId;
        OccurredAt = occurredAt;
        Action = action;
        ObjectType = objectType;
        ObjectId = objectId;
        OldValue = oldValue;
        NewValue = newValue;
        Result = result;
    }

    // Para EF Core.
    private AuditEntry()
    {
        ObjectType = string.Empty;
    }

    public Guid Id { get; private set; }

    /// <summary>
    /// Ticket al que pertenece la entrada, para consultar su bitácora (HU-012); null si el objeto no cuelga de
    /// un ticket.
    /// </summary>
    public Guid? TicketId { get; private set; }

    /// <summary>Usuario que ejecutó la acción (<c>identity.users</c>).</summary>
    public Guid ActorId { get; private set; }

    /// <summary>Instante UTC de la acción, truncado a microsegundos (resolución de <c>timestamptz</c>).</summary>
    public DateTime OccurredAt { get; private set; }

    public AuditAction Action { get; private set; }

    /// <summary>
    /// Tipo del objeto afectado según el catálogo de la wiki: <c>Ticket</c>, <c>Assignment</c>,
    /// <c>TicketParticipant</c> o <c>FormalResponse</c>.
    /// </summary>
    public string ObjectType { get; private set; }

    public Guid ObjectId { get; private set; }

    /// <summary>Valor anterior como objeto JSON en camelCase (<see cref="AuditValue"/>), o null.</summary>
    public string? OldValue { get; private set; }

    /// <summary>Valor nuevo como objeto JSON en camelCase (<see cref="AuditValue"/>), o null.</summary>
    public string? NewValue { get; private set; }

    public AuditResult Result { get; private set; }

    /// <summary>
    /// Crea una entrada validada. El identificador es un UUID v7 derivado de <paramref name="occurredAt"/>.
    /// <paramref name="oldValue"/> y <paramref name="newValue"/> siguen las reglas de <see cref="AuditValue.ToJson"/>.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Falta el actor o el objeto, la fecha no es UTC, la acción o el resultado están fuera del catálogo, el
    /// ticket es <see cref="Guid.Empty"/> o un valor no es un objeto JSON.
    /// </exception>
    public static AuditEntry Create(
        AuditAction action,
        Guid actorId,
        DateTime occurredAt,
        string objectType,
        Guid objectId,
        Guid? ticketId = null,
        object? oldValue = null,
        object? newValue = null,
        AuditResult result = AuditResult.Succeeded)
    {
        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(nameof(action), action, "La acción no pertenece al catálogo de auditoría.");
        }

        if (actorId == Guid.Empty)
        {
            throw new ArgumentException("El actor de la acción auditada es obligatorio.", nameof(actorId));
        }

        if (occurredAt.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                $"La fecha de la acción auditada debe estar en UTC (DateTimeKind.Utc), no en {occurredAt.Kind}.",
                nameof(occurredAt));
        }

        if (string.IsNullOrWhiteSpace(objectType)
            || objectType.Length > ObjectTypeMaxLength
            || !string.Equals(objectType, objectType.Trim(), StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"El tipo de objeto auditado es obligatorio, sin espacios en los extremos y de hasta {ObjectTypeMaxLength} caracteres.",
                nameof(objectType));
        }

        if (objectId == Guid.Empty)
        {
            throw new ArgumentException("El identificador del objeto auditado es obligatorio.", nameof(objectId));
        }

        if (ticketId == Guid.Empty)
        {
            throw new ArgumentException("El ticket, si se indica, no puede ser vacío.", nameof(ticketId));
        }

        if (!Enum.IsDefined(result))
        {
            throw new ArgumentOutOfRangeException(nameof(result), result, "El resultado no pertenece al catálogo de auditoría.");
        }

        var storedAt = Timestamps.ToStoredPrecision(new DateTimeOffset(occurredAt));

        return new AuditEntry(
            Guid.CreateVersion7(storedAt),
            ticketId,
            actorId,
            storedAt.UtcDateTime,
            action,
            objectType,
            objectId,
            AuditValue.ToJson(oldValue, nameof(oldValue)),
            AuditValue.ToJson(newValue, nameof(newValue)),
            result);
    }
}
