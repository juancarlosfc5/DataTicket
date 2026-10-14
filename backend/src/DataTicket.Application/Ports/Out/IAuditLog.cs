using DataTicket.Domain.Auditing;

namespace DataTicket.Application.Ports.Out;

/// <summary>
/// Bitácora de auditoría append-only (PRD §12, HU-011). Única operación: añadir una entrada a la unidad de
/// trabajo en curso. No confirma: la entrada se guarda o se revierte junto con la acción cuando el caso de uso
/// confirma su propio cambio, así nunca queda una acción auditable sin su entrada ni una entrada sin su acción.
/// No hay operaciones de actualización ni de borrado (lo vigila <c>IAuditLog_ExposesOnlyAppend</c>).
/// </summary>
public interface IAuditLog
{
    /// <summary>Añade la entrada a la unidad de trabajo en curso; no hace <c>SaveChanges</c>.</summary>
    void Append(AuditEntry entry);
}
