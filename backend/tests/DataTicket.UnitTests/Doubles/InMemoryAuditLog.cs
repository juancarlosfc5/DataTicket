using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Auditing;

namespace DataTicket.UnitTests.Doubles;

/// <summary>
/// Doble en memoria de <see cref="IAuditLog"/> para las pruebas unitarias de los casos de uso emisores
/// (HU-013, HU-015, HU-017, HU-018, HU-019, HU-021, HU-033, HU-036). Guarda las entradas en el orden de
/// <see cref="IAuditLog.Append"/>; se comprueban con <c>AuditAssert.AssertAuditEntry</c>.
/// </summary>
/// <remarks>
/// Como el adaptador real, solo añade: no confirma nada. Si la prueba necesita verificar el orden respecto
/// de otras llamadas (p. ej. confirmar la unidad de trabajo), pasa <paramref name="onAppend"/> y registra
/// la llamada en la misma secuencia que los demás dobles.
/// </remarks>
public sealed class InMemoryAuditLog(Action<AuditEntry>? onAppend = null) : IAuditLog
{
    private readonly List<AuditEntry> _entries = [];

    public IReadOnlyList<AuditEntry> Entries => _entries;

    public void Append(AuditEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries.Add(entry);
        onAppend?.Invoke(entry);
    }
}
