namespace DataTicket.Domain.Tickets;

/// <summary>
/// Estado interno del ticket (glosario <c>TicketStatus</c>, PRD §6.4). Solo lo ven los internos; el cliente ve
/// <c>ClientStatus</c>, que se deriva de este. Se persiste como texto (<c>db.sql</c> §6, <c>ck_tickets_status</c>).
/// </summary>
public enum TicketStatus
{
    New,
    InDevelopment,
    PullRequestReview,
    InProduction,
    SolutionDelivered,
    Closed,
}
