namespace DataTicket.Domain.Auditing;

/// <summary>
/// Catálogo de acciones auditables de PRD §12 (HU-011, glosario <c>AuditAction</c>). Se persiste por nombre en
/// <c>dataticket.audit_entries.action</c>: renombrar un valor rompe la bitácora ya escrita. Añadir uno exige
/// ratificarlo en T-01 de la HU emisora y actualizar el catálogo de la wiki (<c>auditoria</c>).
/// </summary>
public enum AuditAction
{
    /// <summary>Radicación del ticket (HU-013).</summary>
    TicketSubmitted = 1,

    /// <summary>Ajuste manual de la prioridad calculada (HU-015).</summary>
    PriorityOverridden = 2,

    /// <summary>Cambio de estado interno (HU-021).</summary>
    StatusChanged = 3,

    /// <summary>Asignación de responsables (HU-018).</summary>
    TicketAssigned = 4,

    /// <summary>Reasignación del responsable (HU-019).</summary>
    TicketReassigned = 5,

    /// <summary>Incorporación de un participante por asignación, incorporación o toma en cobertura (HU-017, HU-018).</summary>
    ParticipantAdded = 6,

    /// <summary>Retiro de un participante; su participación histórica se conserva (HU-019).</summary>
    ParticipantRemoved = 7,

    /// <summary>Entrega de la respuesta formal al cliente (HU-033).</summary>
    FormalResponseDelivered = 8,

    /// <summary>Cierre manual del ticket (HU-036).</summary>
    TicketClosed = 9,
}
