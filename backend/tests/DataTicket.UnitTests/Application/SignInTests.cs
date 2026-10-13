using DataTicket.Application.Authentication;
using DataTicket.Application.Ports.In.Authentication;
using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Users;

namespace DataTicket.UnitTests.Application;

public sealed class SignInTests
{
    private static readonly Guid UserId = Guid.Parse("b0000000-0000-7000-8000-000001000001");
    private static readonly Guid CompanyId = Guid.Parse("c0000000-0000-7000-8000-000000000001");
    private static readonly SignInCommand Command = new("ana.perez@empresa-sintetica-01.test", "clave");

    [Fact]
    public async Task Cuenta_activa_con_perfil_valido_inicia_sesion()
    {
        var verifier = new StubVerifier(new VerifiedAccount(UserId, AccountStatus.Active, CompanyId, [RoleNames.Requester]));
        var session = new SpySession();

        var outcome = await new SignIn(verifier, session).ExecuteAsync(Command, TestContext.Current.CancellationToken);

        Assert.Equal(SignInOutcome.Succeeded, outcome);
        Assert.Equal([UserId], session.Started);
        Assert.Equal((Command.Email, Command.Password), verifier.Received);
    }

    [Fact]
    public async Task Credenciales_no_verificadas_no_inician_sesion()
    {
        var session = new SpySession();

        var outcome = await new SignIn(new StubVerifier(null), session)
            .ExecuteAsync(Command, TestContext.Current.CancellationToken);

        Assert.Equal(SignInOutcome.InvalidCredentials, outcome);
        Assert.Empty(session.Started);
    }

    [Theory]
    [InlineData(AccountStatus.Invited)]
    [InlineData(AccountStatus.Deactivated)]
    public async Task Cuenta_no_activa_no_inicia_sesion_aunque_la_clave_sea_correcta(AccountStatus status)
    {
        var session = new SpySession();
        var verifier = new StubVerifier(new VerifiedAccount(UserId, status, CompanyId, [RoleNames.Requester]));

        var outcome = await new SignIn(verifier, session).ExecuteAsync(Command, TestContext.Current.CancellationToken);

        Assert.Equal(SignInOutcome.InvalidCredentials, outcome);
        Assert.Empty(session.Started);
    }

    [Fact]
    public async Task Perfil_invalido_no_inicia_sesion()
    {
        var session = new SpySession();
        var mixed = new VerifiedAccount(UserId, AccountStatus.Active, CompanyId, [RoleNames.Requester, RoleNames.Administrator]);

        var outcome = await new SignIn(new StubVerifier(mixed), session)
            .ExecuteAsync(Command, TestContext.Current.CancellationToken);

        Assert.Equal(SignInOutcome.InvalidCredentials, outcome);
        Assert.Empty(session.Started);
    }

    [Fact]
    public async Task Cerrar_sesion_termina_la_sesion_actual()
    {
        var session = new SpySession();

        await new SignOut(session).ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, session.Ended);
    }

    private sealed class StubVerifier(VerifiedAccount? account) : ICredentialVerifier
    {
        public (string Email, string Password)? Received { get; private set; }

        public Task<VerifiedAccount?> VerifyAsync(string email, string password, CancellationToken cancellationToken)
        {
            Received = (email, password);
            return Task.FromResult(account);
        }
    }

    private sealed class SpySession : IUserSession
    {
        private readonly List<Guid> _started = [];

        public IReadOnlyList<Guid> Started => _started;

        public int Ended { get; private set; }

        public Task StartAsync(Guid userId, CancellationToken cancellationToken)
        {
            _started.Add(userId);
            return Task.CompletedTask;
        }

        public Task EndAsync(CancellationToken cancellationToken)
        {
            Ended++;
            return Task.CompletedTask;
        }
    }
}
