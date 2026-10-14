using DataTicket.Api.Auth;
using DataTicket.Application.Ports.In.Tickets;
using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Tickets;
using DataTicket.Infrastructure.Identity;
using DataTicket.Infrastructure.Persistence;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Tickets;

/// <summary>
/// Núcleo N1: la regla de acceso interno contra PostgreSQL real, con T03 de <c>db.sql</c> §13 y el principal que
/// emite la fábrica de claims real (el mismo que lleva la cookie) leído por el adaptador real de
/// <see cref="ICurrentUser"/>. La política y el repositorio salen del contenedor de DI del Api.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class TicketAccessPolicyIntegrationTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static readonly Guid UnknownTicketId = Guid.Parse("d0000000-0000-7000-8000-000000000999");

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("laura@dataticket.local", true)] // Participante vigente (responsable).
    [InlineData("juan.david@dataticket.local", true)] // Participante vigente (responsable).
    [InlineData("elizabeth@dataticket.local", true)] // PM: accede sin participar.
    [InlineData("brayan@dataticket.local", false)] // Participante retirado.
    [InlineData("kevin@dataticket.local", false)] // Interno de Desarrollo no participante.
    [InlineData("julian@dataticket.local", false)] // Interno de Producción no participante.
    [InlineData("admin@dataticket.local", false)] // Administrador no asociado (invariante 5).
    [InlineData("ana.perez@empresa-sintetica-01.test", false)] // Cliente: la solicitante de T03.
    [InlineData("coordinador@empresa-sintetica-01.test", false)] // Cliente: coordinador de la empresa de T03.
    [InlineData("coordinador@empresa-sintetica-02.test", false)] // Cliente de otra empresa.
    public async Task Regla_de_acceso_interno_sobre_T03_respeta_la_matriz(string email, bool expected)
    {
        Assert.Equal(expected, await CanAccessAsync(TestTickets.T03Id, Account(email)));
    }

    [Theory]
    [InlineData("elizabeth@dataticket.local")]
    [InlineData("laura@dataticket.local")]
    public async Task Ticket_inexistente_no_es_accesible_ni_para_la_PM(string email)
    {
        Assert.False(await CanAccessAsync(UnknownTicketId, Account(email)));
    }

    [Fact]
    public async Task Administrador_asociado_accede_y_al_retirarlo_pierde_el_acceso()
    {
        var admin = TestAccounts.Admin;
        var ticketId = await TestTickets.SubmitAsync(
            api.Factory.Services,
            ticket => ticket.AddParticipant(admin.Id, ParticipationOrigin.CoverageTake, admin.Id, api.Time.GetUtcNow()),
            Ct);

        Assert.True(await CanAccessAsync(ticketId, admin));

        // Retiro a través del agregado cargado por el repositorio (lo que hará RemoveParticipant de HU-019).
        await using (var scope = api.Factory.Services.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
            var ticket = await repository.FindAsync(ticketId, Ct);
            Assert.NotNull(ticket);
            ticket.RemoveParticipant(admin.Id, TestAccounts.Elizabeth.Id, api.Time.GetUtcNow());
            await scope.ServiceProvider.GetRequiredService<DataTicketDbContext>().SaveChangesAsync(Ct);
        }

        Assert.False(await CanAccessAsync(ticketId, admin));
        // La PM sigue accediendo al mismo ticket.
        Assert.True(await CanAccessAsync(ticketId, TestAccounts.Elizabeth));
    }

    [Fact]
    public async Task Participante_agregado_tarde_accede_al_ticket()
    {
        var kevin = TestAccounts.Kevin;
        var ticketId = await TestTickets.SubmitAsync(api.Factory.Services, configure: null, Ct);
        Assert.False(await CanAccessAsync(ticketId, kevin));

        await using (var scope = api.Factory.Services.CreateAsyncScope())
        {
            var ticket = await scope.ServiceProvider.GetRequiredService<ITicketRepository>().FindAsync(ticketId, Ct);
            Assert.NotNull(ticket);
            // La fila nueva se descubre por la navegación del agregado: debe insertarse, no actualizarse.
            ticket.AddParticipant(kevin.Id, ParticipationOrigin.Participant, TestAccounts.Elizabeth.Id, api.Time.GetUtcNow());
            await scope.ServiceProvider.GetRequiredService<DataTicketDbContext>().SaveChangesAsync(Ct);
        }

        Assert.True(await CanAccessAsync(ticketId, kevin));
    }

    private static TestAccount Account(string email) => TestAccounts.All.Single(account => account.Email == email);

    /// <summary>
    /// Evalúa la regla con el principal que emite la fábrica de claims para la cuenta, como si llegara en la
    /// cookie: lo lee <see cref="HttpContextCurrentUser"/> y la política del contenedor decide.
    /// </summary>
    private async Task<bool> CanAccessAsync(Guid ticketId, TestAccount account)
    {
        await using var scope = api.Factory.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var user = await services.GetRequiredService<UserManager<ApplicationUser>>().FindByIdAsync(account.Id.ToString());
        Assert.NotNull(user);
        var principal = await services.GetRequiredService<IUserClaimsPrincipalFactory<ApplicationUser>>().CreateAsync(user);
        var currentUser = new HttpContextCurrentUser(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        });

        return await services.GetRequiredService<ITicketAccessPolicy>().CanAccessInternalAsync(ticketId, currentUser, Ct);
    }
}
