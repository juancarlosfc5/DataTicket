using DataTicket.Testing.Tickets;

namespace DataTicket.IntegrationTests.Fixtures;

/// <summary>
/// El escenario compartido <see cref="TicketSeed"/> usa los mismos identificadores que el catálogo de cuentas y
/// empresas (<c>db.sql</c> §13). No necesita Docker.
/// </summary>
public sealed class TestTicketsCatalogTests
{
    [Fact]
    public void TicketSeed_usa_los_ids_del_catalogo_de_cuentas()
    {
        Assert.Equal(TestAccounts.Elizabeth.Id, TicketSeed.Elizabeth);
        Assert.Equal(TestAccounts.Julian.Id, TicketSeed.Julian);
        Assert.Equal(TestAccounts.Gerardo.Id, TicketSeed.Gerardo);
        Assert.Equal(TestAccounts.JuanDavid.Id, TicketSeed.JuanDavid);
        Assert.Equal(TestAccounts.Laura.Id, TicketSeed.Laura);
        Assert.Equal(TestAccounts.Brayan.Id, TicketSeed.Brayan);
        Assert.Equal(TestAccounts.Cristian.Id, TicketSeed.Cristian);
        Assert.Equal(TestAccounts.Kevin.Id, TicketSeed.Kevin);
        Assert.Equal(TestAccounts.Admin.Id, TicketSeed.Admin);
        Assert.Equal(TestAccounts.AnaPerez.Id, TicketSeed.AnaPerez);
        Assert.Equal(TestCompanies.Company01.Id, TicketSeed.Company01);
        Assert.Equal(TestAccounts.AnaPerez.CompanyId, TicketSeed.Company01);
    }

    [Fact]
    public void T03_replica_los_ids_de_db_sql()
    {
        Assert.Equal(Guid.Parse("d0000000-0000-7000-8000-000000000003"), TestTickets.T03Id);
        Assert.Equal("DT-000003", TicketSeed.T03Number);
        Assert.Equal(Guid.Parse("a2000000-0000-7000-8000-000000000003"), TicketSeed.T03BrayanParticipationId);
        Assert.Equal(Guid.Parse("a3000000-0000-7000-8000-000000000001"), TicketSeed.T03LauraAssignmentId);
    }
}
