using DataTicket.Domain.Common;

namespace DataTicket.Domain.Tickets;

/// <summary>
/// Raíz del agregado ticket (glosario <c>Ticket</c>; <c>db.sql</c> §6). Contiene sus participaciones
/// (<see cref="TicketParticipant"/>) y asignaciones (<see cref="Assignment"/>), que solo se modifican a través de
/// él.
/// </summary>
/// <remarks>
/// Núcleo N1 (adelanto de HU-013, HU-018 y HU-019): la factoría de radicación y las operaciones mínimas de
/// participación que necesitan la regla de acceso y los fixtures. No hay todavía casos de uso, autorización ni
/// auditoría de esas operaciones (los añaden sus HU), ni cambios de estado, ajuste de prioridad, URL de PR o
/// cierre (HU-015, HU-021, HU-022, HU-036): esas columnas existen y se leen, pero el dominio aún no las escribe.
/// <para>
/// <see cref="CompanyId"/> y <see cref="RequesterId"/> salen de la cuenta autenticada y no cambian (PRD §6.1).
/// Las fechas se guardan en UTC con resolución de microsegundos (<see cref="Timestamps"/>).
/// </para>
/// </remarks>
public sealed class Ticket
{
    /// <summary>Longitud de <c>number varchar(24)</c> (formato provisional <c>DT-000001</c>, V-16).</summary>
    public const int NumberMaxLength = 24;

    /// <summary>Título de 1 a 120 caracteres tras recortar (PRD §6.1).</summary>
    public const int TitleMaxLength = 120;

    /// <summary>Límite técnico de la descripción (propuesta P-19; <c>ck_tickets_description</c>).</summary>
    public const int DescriptionMaxLength = 10_000;

    public const int PullRequestUrlMaxLength = 2048;

    private readonly List<TicketParticipant> _participants = [];
    private readonly List<Assignment> _assignments = [];

    private Ticket(
        Guid id,
        string number,
        Guid companyId,
        Guid requesterId,
        Guid categoryId,
        string title,
        string description,
        Urgency urgency,
        Impact impact,
        DateTimeOffset createdAt)
    {
        Id = id;
        Number = number;
        CompanyId = companyId;
        RequesterId = requesterId;
        CategoryId = categoryId;
        Title = title;
        Description = description;
        Urgency = urgency;
        Impact = impact;
        CalculatedPriority = PriorityMatrix.Calculate(urgency, impact);
        CurrentPriority = CalculatedPriority;
        Status = TicketStatus.New;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    // Para EF Core.
    private Ticket()
    {
        Number = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
    }

    public Guid Id { get; private set; }

    /// <summary>Número visible, único, generado en servidor (formato provisional, V-16).</summary>
    public string Number { get; private set; }

    public Guid CompanyId { get; private set; }

    public Guid RequesterId { get; private set; }

    public Guid CategoryId { get; private set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public Urgency Urgency { get; private set; }

    public Impact Impact { get; private set; }

    /// <summary>Resultado de la matriz urgencia × impacto (<see cref="PriorityMatrix"/>).</summary>
    public CalculatedPriority CalculatedPriority { get; private set; }

    /// <summary>Prioridad vigente: la calculada salvo un ajuste manual (HU-015).</summary>
    public CalculatedPriority CurrentPriority { get; private set; }

    /// <summary>Estado interno; nunca se expone al cliente, que ve <c>ClientStatus</c> (PRD §6.4).</summary>
    public TicketStatus Status { get; private set; }

    /// <summary>URL del PR, manual e interna (PRD §6.3; HU-022).</summary>
    public string? PullRequestUrl { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Cuándo se cerró manualmente (P-17; HU-036). Nulo si no está cerrado.</summary>
    public DateTimeOffset? ClosedAt { get; private set; }

    /// <summary>Quién lo cerró (P-17; HU-036). Nulo si no está cerrado.</summary>
    public Guid? ClosedBy { get; private set; }

    /// <summary>Participaciones vigentes y retiradas, en orden de incorporación.</summary>
    public IReadOnlyCollection<TicketParticipant> Participants => _participants.AsReadOnly();

    /// <summary>Asignaciones vigentes y cerradas, en orden de creación.</summary>
    public IReadOnlyCollection<Assignment> Assignments => _assignments.AsReadOnly();

    /// <summary>
    /// Radica un ticket nuevo en estado <see cref="TicketStatus.New"/> con la prioridad de la matriz (PRD §6.1,
    /// §6.2). El número lo genera el servidor (HU-013, V-16); empresa y solicitante salen de la cuenta.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Falta un identificador o el número, o el título o la descripción están vacíos o superan su límite.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">Urgencia o impacto fuera de Low, Medium o High.</exception>
    public static Ticket Submit(
        Guid id,
        string number,
        Guid companyId,
        Guid requesterId,
        Guid categoryId,
        string title,
        string description,
        Urgency urgency,
        Impact impact,
        DateTimeOffset submittedAt)
    {
        RequireId(id, nameof(id));

        var trimmedNumber = number?.Trim() ?? string.Empty;
        if (trimmedNumber.Length is 0 or > NumberMaxLength || trimmedNumber.Length != number!.Length)
        {
            throw new ArgumentException(
                $"El número del ticket es obligatorio, sin espacios en los extremos y de hasta {NumberMaxLength} caracteres.",
                nameof(number));
        }

        RequireId(companyId, nameof(companyId));
        RequireId(requesterId, nameof(requesterId));
        RequireId(categoryId, nameof(categoryId));

        var trimmedTitle = title?.Trim() ?? string.Empty;
        if (trimmedTitle.Length is 0 or > TitleMaxLength)
        {
            throw new ArgumentException(
                $"El título debe tener entre 1 y {TitleMaxLength} caracteres.", nameof(title));
        }

        // El CHECK de db.sql mide la descripción recortada: se valida igual y se guarda tal cual la escribió el cliente.
        var trimmedDescriptionLength = description?.Trim().Length ?? 0;
        if (trimmedDescriptionLength is 0 or > DescriptionMaxLength)
        {
            throw new ArgumentException(
                $"La descripción debe tener entre 1 y {DescriptionMaxLength} caracteres.", nameof(description));
        }

        if (!Enum.IsDefined(urgency))
        {
            throw new ArgumentOutOfRangeException(nameof(urgency), urgency, "La urgencia debe ser Low, Medium o High.");
        }

        if (!Enum.IsDefined(impact))
        {
            throw new ArgumentOutOfRangeException(nameof(impact), impact, "El impacto debe ser Low, Medium o High.");
        }

        return new Ticket(
            id,
            trimmedNumber,
            companyId,
            requesterId,
            categoryId,
            trimmedTitle,
            description!,
            urgency,
            impact,
            Timestamps.ToStoredPrecision(submittedAt));
    }

    /// <summary>La persona tiene una participación vigente (no retirada) en el ticket.</summary>
    public bool HasActiveParticipant(Guid userId) => FindActiveParticipation(userId) is not null;

    /// <summary>
    /// Asocia a una persona interna al ticket (HU-017, HU-018). Agregar da acceso al historial completo
    /// (PRD §5.4). Una persona retirada puede volver: se crea una participación nueva y la anterior se conserva.
    /// </summary>
    /// <param name="participationId">Identificador de la participación; por defecto, un UUID v7 de
    /// <paramref name="addedAt"/>. Los fixtures lo fijan para replicar <c>db.sql</c> §13.</param>
    /// <exception cref="InvalidOperationException">La persona ya es participante vigente.</exception>
    public TicketParticipant AddParticipant(
        Guid userId, ParticipationOrigin origin, Guid addedBy, DateTimeOffset addedAt, Guid? participationId = null)
    {
        RequireId(userId, nameof(userId));
        if (!Enum.IsDefined(origin))
        {
            throw new ArgumentOutOfRangeException(nameof(origin), origin, "El origen de la participación no es válido.");
        }

        RequireId(addedBy, nameof(addedBy));
        if (HasActiveParticipant(userId))
        {
            throw new InvalidOperationException("La persona ya es participante vigente del ticket.");
        }

        var storedAt = Timestamps.ToStoredPrecision(addedAt);
        var participation = new TicketParticipant(
            NewId(participationId, storedAt, nameof(participationId)), Id, userId, origin, addedBy, storedAt);
        _participants.Add(participation);
        return participation;
    }

    /// <summary>
    /// Asigna a una persona interna como responsable (PRD §6.2). Asignar implica participar (propuesta V-10): si
    /// no tiene una participación vigente, se crea con origen <see cref="ParticipationOrigin.Assignment"/>. Repetir
    /// la asignación de un responsable vigente no cambia nada y devuelve la asignación existente.
    /// </summary>
    public Assignment Assign(
        Guid assigneeId, Guid assignedBy, DateTimeOffset assignedAt, Guid? assignmentId = null, Guid? participationId = null)
    {
        RequireId(assigneeId, nameof(assigneeId));
        RequireId(assignedBy, nameof(assignedBy));

        var current = _assignments.Find(assignment => assignment.IsActive && assignment.AssigneeId == assigneeId);
        if (current is not null)
        {
            return current;
        }

        var storedAt = Timestamps.ToStoredPrecision(assignedAt);
        var created = new Assignment(
            NewId(assignmentId, storedAt, nameof(assignmentId)), Id, assigneeId, assignedBy, storedAt);
        if (!HasActiveParticipant(assigneeId))
        {
            AddParticipant(assigneeId, ParticipationOrigin.Assignment, assignedBy, storedAt, participationId);
        }

        _assignments.Add(created);
        return created;
    }

    /// <summary>
    /// Retira a una persona del ticket (HU-019): marca su participación vigente como retirada y, si era
    /// responsable, cierra su asignación. No borra nada: la participación pasada queda en el historial (PRD §5.4,
    /// §12). Retirar revoca el acceso futuro; la revocación de las conexiones del chat la hace el caso de uso con
    /// <c>IChatConnectionRevoker</c> después de persistir (HU-032).
    /// </summary>
    /// <exception cref="InvalidOperationException">La persona no es participante vigente.</exception>
    public TicketParticipant RemoveParticipant(Guid userId, Guid removedBy, DateTimeOffset removedAt)
    {
        var participation = FindActiveParticipation(userId)
            ?? throw new InvalidOperationException("La persona no es participante vigente del ticket.");
        RequireId(removedBy, nameof(removedBy));

        // Se valida todo antes de cambiar nada: el retiro y el cierre de sus asignaciones van juntos.
        var storedAt = Timestamps.ToStoredPrecision(removedAt);
        var activeAssignments = _assignments
            .Where(assignment => assignment.IsActive && assignment.AssigneeId == userId)
            .ToList();
        if (storedAt < participation.AddedAt || activeAssignments.Exists(assignment => storedAt < assignment.AssignedAt))
        {
            throw new ArgumentException(
                "La fecha de retiro no puede ser anterior a la incorporación ni a la asignación.", nameof(removedAt));
        }

        participation.Remove(removedBy, storedAt);
        foreach (var assignment in activeAssignments)
        {
            assignment.Unassign(removedBy, storedAt);
        }

        return participation;
    }

    private TicketParticipant? FindActiveParticipation(Guid userId) =>
        _participants.Find(participant => participant.IsActive && participant.UserId == userId);

    private static Guid NewId(Guid? requested, DateTimeOffset at, string parameterName)
    {
        if (requested is { } id)
        {
            RequireId(id, parameterName);
            return id;
        }

        return Guid.CreateVersion7(at);
    }

    private static void RequireId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("El identificador es obligatorio.", parameterName);
        }
    }
}
