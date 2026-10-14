using DataTicket.Application.Tickets;
using DataTicket.Domain.Tickets;
using DataTicket.Domain.Users;
using DataTicket.Testing.Tickets;
using DataTicket.UnitTests.Doubles;

namespace DataTicket.UnitTests.Application;

/// <summary>
/// Regla de acceso interno del núcleo N1 («participante vigente o <c>ProductManager</c>»), con la matriz completa
/// sobre T03 de <c>db.sql</c> §13: Laura y Juan David vigentes, Brayan retirado, Kevin interno no participante,
/// Elizabeth PM, administrador no asociado y clientes. Los nombres de HU-018 y HU-019 T-02 (<c>CanAccess_*</c>)
/// se conservan; la regla vive en Application porque depende del rol del usuario actual.
/// </summary>
public sealed class TicketAccessPolicyTests
{
    private static readonly Guid CategoryId = Guid.Parse("01920000-0000-7000-8000-00000000ca7e");
    private static readonly Guid UnknownTicketId = Guid.Parse("d0000000-0000-7000-8000-000000000999");

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static TheoryData<Guid> ActiveParticipants => [TicketSeed.Laura, TicketSeed.JuanDavid];

    [Theory]
    [MemberData(nameof(ActiveParticipants))]
    public async Task CanAccess_ActiveParticipant_IsTrue(Guid userId)
    {
        var (policy, _) = PolicyWithT03();

        Assert.True(await policy.CanAccessInternalAsync(
            TicketSeed.T03Id, FakeCurrentUser.Internal(userId, teams: [RoleNames.Development]), Ct));
    }

    [Fact]
    public async Task CanAccess_RemovedParticipant_IsFalse()
    {
        var (policy, _) = PolicyWithT03();

        Assert.False(await policy.CanAccessInternalAsync(
            TicketSeed.T03Id, FakeCurrentUser.Internal(TicketSeed.Brayan, teams: [RoleNames.Development]), Ct));
    }

    [Theory]
    [InlineData("a0000000-0000-7000-8000-000000000008", RoleNames.Development)] // Kevin
    [InlineData("a0000000-0000-7000-8000-000000000002", RoleNames.Production)] // Julián
    public async Task CanAccess_TeamMemberNotParticipant_IsFalse(string userId, string team)
    {
        var (policy, _) = PolicyWithT03();

        Assert.False(await policy.CanAccessInternalAsync(
            TicketSeed.T03Id, FakeCurrentUser.Internal(Guid.Parse(userId), teams: [team]), Ct));
    }

    [Fact]
    public async Task CanAccess_ProductManager_IsAlwaysTrue()
    {
        var (policy, repository) = PolicyWithT03();
        var elizabeth = FakeCurrentUser.Internal(TicketSeed.Elizabeth, [RoleNames.ProductManager], [RoleNames.Production]);

        // Elizabeth no participa en T03 y aun así accede (PRD §5.3).
        Assert.False(TicketSeed.BuildT03(CategoryId).HasActiveParticipant(TicketSeed.Elizabeth));
        Assert.True(await policy.CanAccessInternalAsync(TicketSeed.T03Id, elizabeth, Ct));
        Assert.Equal(["Exists"], repository.Calls);
    }

    [Fact]
    public async Task CanAccess_ProductManager_NonexistentTicket_IsFalse()
    {
        var (policy, _) = PolicyWithT03();
        var elizabeth = FakeCurrentUser.Internal(TicketSeed.Elizabeth, [RoleNames.ProductManager], [RoleNames.Production]);

        Assert.False(await policy.CanAccessInternalAsync(UnknownTicketId, elizabeth, Ct));
    }

    [Fact]
    public async Task CanAccess_AdministratorNotParticipant_IsFalse()
    {
        var (policy, _) = PolicyWithT03();

        // Invariante 5: el administrador no ve contenido por ser administrador.
        Assert.False(await policy.CanAccessInternalAsync(
            TicketSeed.T03Id, FakeCurrentUser.Internal(TicketSeed.Admin, [RoleNames.Administrator]), Ct));
    }

    [Fact]
    public async Task CanAccess_AdministratorParticipant_IsTrue()
    {
        var ticket = TicketSeed.BuildT03(CategoryId);
        // Tomó el ticket desde la cola de cobertura (HU-017): queda asociado antes de cargar el detalle.
        ticket.AddParticipant(TicketSeed.Admin, ParticipationOrigin.CoverageTake, TicketSeed.Admin, TicketSeed.T03UpdatedAt);
        var policy = new TicketAccessPolicy(new InMemoryTicketRepository(ticket));

        Assert.True(await policy.CanAccessInternalAsync(
            TicketSeed.T03Id, FakeCurrentUser.Internal(TicketSeed.Admin, [RoleNames.Administrator]), Ct));
    }

    [Theory]
    [InlineData(RoleNames.Requester)]
    [InlineData(RoleNames.CompanyCoordinator)]
    public async Task CanAccess_Client_IsFalse_WithoutQuerying(string role)
    {
        var (policy, repository) = PolicyWithT03();
        // Ana Pérez es la solicitante de T03: ni siquiera ella ve el contenido interno (invariante 2).
        var client = FakeCurrentUser.Client(TicketSeed.AnaPerez, TicketSeed.Company01, role);

        Assert.False(await policy.CanAccessInternalAsync(TicketSeed.T03Id, client, Ct));
        Assert.Empty(repository.Calls);
    }

    [Fact]
    public async Task CanAccess_ClientWithAnInternalRoleName_IsFalse()
    {
        var (policy, repository) = PolicyWithT03();
        // Un perfil de cliente nunca pasa por la rama interna, aunque un doble mal formado traiga ProductManager.
        var client = FakeCurrentUser.Client(TicketSeed.AnaPerez, TicketSeed.Company01) with
        {
            Roles = [RoleNames.Requester, RoleNames.ProductManager],
        };

        Assert.False(await policy.CanAccessInternalAsync(TicketSeed.T03Id, client, Ct));
        Assert.Empty(repository.Calls);
    }

    [Fact]
    public async Task CanAccess_Anonymous_IsFalse_WithoutQuerying()
    {
        var (policy, repository) = PolicyWithT03();

        Assert.False(await policy.CanAccessInternalAsync(TicketSeed.T03Id, FakeCurrentUser.Anonymous, Ct));
        Assert.Empty(repository.Calls);
    }

    [Fact]
    public async Task CanAccess_InvalidProfile_IsFalse_WithoutQuerying()
    {
        var (policy, repository) = PolicyWithT03();

        // Sesión de Laura con un perfil no válido: el adaptador falla cerrado (sin superficie ni roles).
        Assert.False(await policy.CanAccessInternalAsync(TicketSeed.T03Id, FakeCurrentUser.InvalidProfile(TicketSeed.Laura), Ct));
        Assert.Empty(repository.Calls);
    }

    [Fact]
    public async Task CanAccess_NonexistentTicket_IsFalse()
    {
        var (policy, _) = PolicyWithT03();

        Assert.False(await policy.CanAccessInternalAsync(UnknownTicketId, FakeCurrentUser.Internal(TicketSeed.Laura), Ct));
    }

    [Fact]
    public async Task CanAccess_EmptyTicketId_IsFalse_WithoutQuerying()
    {
        var (policy, repository) = PolicyWithT03();
        var elizabeth = FakeCurrentUser.Internal(TicketSeed.Elizabeth, [RoleNames.ProductManager]);

        Assert.False(await policy.CanAccessInternalAsync(Guid.Empty, elizabeth, Ct));
        Assert.Empty(repository.Calls);
    }

    [Fact]
    public async Task CanAccess_RevalidatesOnEveryCall()
    {
        var ticket = TicketSeed.BuildT03(CategoryId);
        var repository = new InMemoryTicketRepository(ticket);
        var policy = new TicketAccessPolicy(repository);
        var laura = FakeCurrentUser.Internal(TicketSeed.Laura, teams: [RoleNames.Development]);

        Assert.True(await policy.CanAccessInternalAsync(TicketSeed.T03Id, laura, Ct));

        // Se retira a Laura: la siguiente consulta ya lo refleja (sin caché; HU-032 depende de ello).
        ticket.RemoveParticipant(TicketSeed.Laura, TicketSeed.Elizabeth, TicketSeed.T03UpdatedAt);

        Assert.False(await policy.CanAccessInternalAsync(TicketSeed.T03Id, laura, Ct));
        Assert.Equal(["IsActiveParticipant", "IsActiveParticipant"], repository.Calls);
    }

    private static (TicketAccessPolicy Policy, InMemoryTicketRepository Repository) PolicyWithT03()
    {
        var repository = new InMemoryTicketRepository(TicketSeed.BuildT03(CategoryId));
        return (new TicketAccessPolicy(repository), repository);
    }
}
