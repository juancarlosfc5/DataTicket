namespace DataTicket.Domain.Tickets;

/// <summary>
/// Origen de una participación en el ticket (<c>db.sql</c> §6, <c>ck_ticket_participants_origin</c>; nombre
/// propuesto en el núcleo N1, a ratificar en la T-01 de HU-018).
/// </summary>
public enum ParticipationOrigin
{
    /// <summary>La persona fue asignada como responsable (asignar implica participar, propuesta V-10).</summary>
    Assignment,

    /// <summary>La PM o un administrador la agregó como participante sin hacerla responsable (HU-018).</summary>
    Participant,

    /// <summary>Un administrador tomó el ticket desde la cola de cobertura (HU-017).</summary>
    CoverageTake,
}
