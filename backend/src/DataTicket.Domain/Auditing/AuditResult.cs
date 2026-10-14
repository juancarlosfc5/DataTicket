namespace DataTicket.Domain.Auditing;

/// <summary>
/// Resultado de la acción auditada (PRD §12). Se persiste por nombre en <c>dataticket.audit_entries.result</c>
/// (CHECK con estos tres valores). Por ahora solo se registran acciones confirmadas (<see cref="Succeeded"/>);
/// <see cref="Denied"/> y <see cref="Failed"/> existen para no migrar cuando se decida V-11 (auditar intentos
/// denegados).
/// </summary>
public enum AuditResult
{
    /// <summary>La acción se confirmó. Es el valor por defecto, igual que el DEFAULT de la columna.</summary>
    Succeeded = 0,

    /// <summary>Intento rechazado por autorización (pendiente de V-11).</summary>
    Denied = 1,

    /// <summary>Intento fallido por otra causa (pendiente de V-11).</summary>
    Failed = 2,
}
