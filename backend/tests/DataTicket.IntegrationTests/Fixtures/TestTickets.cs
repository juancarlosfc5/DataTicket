using DataTicket.Domain.Tickets;
using DataTicket.Infrastructure.Persistence;
using DataTicket.Testing.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Fixtures;

/// <summary>
/// Tickets de prueba del núcleo N1. <see cref="SeedAsync"/> persiste T03 de <c>db.sql</c> §13 (mismos IDs, con
/// Laura y Juan David vigentes, Brayan retirado y Kevin sin participar; ver <see cref="TicketSeed"/>) y lo llama
/// <see cref="TestDataSeeder"/>, así que toda clase con <c>ApiFixture</c> lo tiene. <see cref="SubmitAsync"/> crea un
/// ticket nuevo de la Empresa 01 para las pruebas que necesitan modificar uno sin tocar T03.
/// </summary>
public static class TestTickets
{
    /// <summary>T03 (<c>DT-000003</c>): ticket principal del chat (HU-026 a HU-032).</summary>
    public static Guid T03Id => TicketSeed.T03Id;

    /// <summary>Categoría de la semilla de referencia (<c>db.sql</c> §12) que usan los tickets de prueba.</summary>
    public const string DefaultCategoryName = TicketSeed.T03CategoryName;

    /// <summary>Requiere las cuentas y empresas de <see cref="TestDataSeeder"/>.</summary>
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();

        var t03 = TicketSeed.BuildT03(await CategoryIdAsync(dbContext, TicketSeed.T03CategoryName, cancellationToken));
        dbContext.Tickets.Add(t03);
        // El cambio de estado aún no tiene operación de dominio (HU-021): se fija como en db.sql §13.6.
        var entry = dbContext.Entry(t03);
        entry.Property(ticket => ticket.Status).CurrentValue = TicketSeed.T03Status;
        entry.Property(ticket => ticket.UpdatedAt).CurrentValue = TicketSeed.T03UpdatedAt;
        await dbContext.SaveChangesAsync(cancellationToken);

        // db.sql §13.6 reserva DT-000001 a DT-000010 para su semilla: la secuencia continúa en DT-000011.
        await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT setval('dataticket.ticket_number_seq', 10, true)", cancellationToken);
    }

    /// <summary>
    /// Radica un ticket nuevo (Empresa 01, solicitante Ana Pérez, número de <c>next_ticket_number()</c>, reloj del
    /// host) y deja que la prueba lo configure (p. ej. agregar participantes) antes de guardarlo.
    /// </summary>
    public static async Task<Guid> SubmitAsync(
        IServiceProvider services, Action<Ticket>? configure, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();
        var now = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow();

        var number = await NextNumberAsync(dbContext, cancellationToken);
        var ticket = Ticket.Submit(
            Guid.CreateVersion7(now),
            number,
            TestCompanies.Company01.Id,
            TestAccounts.AnaPerez.Id,
            await CategoryIdAsync(dbContext, DefaultCategoryName, cancellationToken),
            $"Ticket de prueba {number}",
            "Ticket sintético creado por una prueba de integración.",
            Urgency.Low,
            Impact.Low,
            now);
        configure?.Invoke(ticket);

        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ticket.Id;
    }

    public static Task<string> NextNumberAsync(DataTicketDbContext dbContext, CancellationToken cancellationToken) =>
        dbContext.Database
            .SqlQuery<string>($"SELECT dataticket.next_ticket_number() AS \"Value\"")
            .SingleAsync(cancellationToken);

    public static Task<Guid> CategoryIdAsync(DataTicketDbContext dbContext, string name, CancellationToken cancellationToken) =>
        dbContext.Categories
            .AsNoTracking()
            .Where(category => category.Name == name)
            .Select(category => category.Id)
            .SingleAsync(cancellationToken);
}
