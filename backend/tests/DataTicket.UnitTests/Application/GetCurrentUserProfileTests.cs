using DataTicket.Application.Authentication;
using DataTicket.Application.Ports.In.Authentication;
using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Users;

namespace DataTicket.UnitTests.Application;

/// <summary>HU-004 T-05: el perfil se compone solo desde <see cref="ICurrentUser"/> y el nombre de la empresa.</summary>
public sealed class GetCurrentUserProfileTests
{
    private static readonly Guid UserId = Guid.Parse("b0000000-0000-7000-8000-000001000001");
    private static readonly Guid CompanyA = Guid.Parse("c0000000-0000-7000-8000-000000000001");

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Cliente_recibe_su_perfil_con_el_nombre_de_su_empresa()
    {
        var companies = new StubCompanyDirectory((CompanyA, "Empresa Sintética 01"));
        var currentUser = Client(CompanyA);

        var profile = await new GetCurrentUserProfile(currentUser, companies).ExecuteAsync(Ct);

        Assert.NotNull(profile);
        Assert.Equal(UserId, profile.Id);
        Assert.Equal("Ana Pérez", profile.DisplayName);
        Assert.Equal("ana.perez@empresa-sintetica-01.test", profile.Email);
        Assert.Equal(Surface.Client, profile.Surface);
        Assert.Equal([RoleNames.Requester], profile.Roles);
        Assert.Empty(profile.Teams);
        Assert.Equal(new CurrentUserCompany(CompanyA, "Empresa Sintética 01"), profile.Company);
        Assert.Equal([CompanyA], companies.Requested);
    }

    [Fact]
    public async Task Interno_recibe_su_perfil_sin_empresa_y_sin_consultar_empresas()
    {
        var companies = new StubCompanyDirectory((CompanyA, "Empresa Sintética 01"));
        var currentUser = new StubCurrentUser
        {
            UserId = UserId,
            DisplayName = "Elizabeth",
            Email = "elizabeth@dataticket.local",
            Surface = Surface.Internal,
            Roles = [RoleNames.ProductManager],
            Teams = [RoleNames.Production],
        };

        var profile = await new GetCurrentUserProfile(currentUser, companies).ExecuteAsync(Ct);

        Assert.NotNull(profile);
        Assert.Equal(Surface.Internal, profile.Surface);
        Assert.Equal([RoleNames.ProductManager], profile.Roles);
        Assert.Equal([RoleNames.Production], profile.Teams);
        Assert.Null(profile.Company);
        Assert.Empty(companies.Requested);
    }

    [Fact]
    public async Task Sin_nombre_visible_se_usa_el_correo()
    {
        var currentUser = Client(CompanyA) with { DisplayName = "  " };

        var profile = await new GetCurrentUserProfile(currentUser, new StubCompanyDirectory((CompanyA, "Empresa"))).ExecuteAsync(Ct);

        Assert.Equal("ana.perez@empresa-sintetica-01.test", profile?.DisplayName);
    }

    [Fact]
    public async Task Sin_sesion_no_hay_perfil()
    {
        var profile = await new GetCurrentUserProfile(new StubCurrentUser(), new StubCompanyDirectory()).ExecuteAsync(Ct);

        Assert.Null(profile);
    }

    [Fact]
    public async Task Perfil_no_valido_falla_cerrado()
    {
        // Sesión autenticada cuya superficie no se pudo resolver (p. ej. cliente sin company_id).
        var withoutSurface = Client(CompanyA) with { Surface = null, CompanyId = null, Roles = [] };
        // Superficie de cliente sin empresa: no debería ocurrir, pero tampoco se inventa.
        var clientWithoutCompany = Client(CompanyA) with { CompanyId = null };
        // Interno con empresa: incoherente.
        var internalWithCompany = Client(CompanyA) with { Surface = Surface.Internal, Roles = [RoleNames.Administrator] };
        var withoutEmail = Client(CompanyA) with { Email = null };
        var companies = new StubCompanyDirectory((CompanyA, "Empresa Sintética 01"));

        foreach (var currentUser in new[] { withoutSurface, clientWithoutCompany, internalWithCompany, withoutEmail })
        {
            Assert.Null(await new GetCurrentUserProfile(currentUser, companies).ExecuteAsync(Ct));
        }
    }

    [Fact]
    public async Task Cliente_cuya_empresa_no_existe_falla_cerrado()
    {
        var profile = await new GetCurrentUserProfile(Client(CompanyA), new StubCompanyDirectory()).ExecuteAsync(Ct);

        Assert.Null(profile);
    }

    private static StubCurrentUser Client(Guid companyId) => new()
    {
        UserId = UserId,
        DisplayName = "Ana Pérez",
        Email = "ana.perez@empresa-sintetica-01.test",
        Surface = Surface.Client,
        Roles = [RoleNames.Requester],
        CompanyId = companyId,
    };

    private sealed record StubCurrentUser : ICurrentUser
    {
        public bool IsAuthenticated => UserId is not null;

        public Guid? UserId { get; init; }

        public string? DisplayName { get; init; }

        public string? Email { get; init; }

        public Surface? Surface { get; init; }

        public IReadOnlyList<string> Roles { get; init; } = [];

        public IReadOnlyList<string> Teams { get; init; } = [];

        public Guid? CompanyId { get; init; }

        public bool IsInRole(string role) => Roles.Contains(role);
    }

    private sealed class StubCompanyDirectory(params (Guid Id, string Name)[] companies) : ICompanyDirectory
    {
        private readonly List<Guid> _requested = [];

        public IReadOnlyList<Guid> Requested => _requested;

        public Task<string?> FindNameAsync(Guid companyId, CancellationToken cancellationToken)
        {
            _requested.Add(companyId);
            var match = companies.Where(company => company.Id == companyId).Select(company => company.Name).FirstOrDefault();
            return Task.FromResult(match);
        }
    }
}
