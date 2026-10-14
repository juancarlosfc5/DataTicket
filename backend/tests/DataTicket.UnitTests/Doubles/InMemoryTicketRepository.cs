using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Tickets;

namespace DataTicket.UnitTests.Doubles;

/// <summary>
/// Doble en memoria de <see cref="ITicketRepository"/> sobre agregados reales (núcleo N1). Responde con el estado
/// actual de cada <see cref="Ticket"/> en cada llamada, sin copiar ni cachear: si una prueba retira a una persona
/// del agregado, la siguiente consulta ya lo refleja. Registra las consultas en <see cref="Calls"/> para afirmar,
/// por ejemplo, que la regla de acceso no consulta nada para un cliente.
/// </summary>
public sealed class InMemoryTicketRepository(params Ticket[] tickets) : ITicketRepository
{
    private readonly Dictionary<Guid, Ticket> _tickets = tickets.ToDictionary(ticket => ticket.Id);
    private readonly List<string> _calls = [];

    /// <summary>Consultas recibidas, en orden: <c>Find</c>, <c>Exists</c> o <c>IsActiveParticipant</c>.</summary>
    public IReadOnlyList<string> Calls => _calls;

    public void Add(Ticket ticket) => _tickets.Add(ticket.Id, ticket);

    public Task<Ticket?> FindAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        _calls.Add("Find");
        return Task.FromResult(_tickets.GetValueOrDefault(ticketId));
    }

    public Task<bool> ExistsAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        _calls.Add("Exists");
        return Task.FromResult(_tickets.ContainsKey(ticketId));
    }

    public Task<bool> IsActiveParticipantAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken)
    {
        _calls.Add("IsActiveParticipant");
        return Task.FromResult(_tickets.TryGetValue(ticketId, out var ticket) && ticket.HasActiveParticipant(userId));
    }
}
