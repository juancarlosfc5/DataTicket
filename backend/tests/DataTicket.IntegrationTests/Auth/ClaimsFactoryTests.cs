using System.Net;
using System.Security.Claims;
using DataTicket.Domain.Users;
using DataTicket.Infrastructure.Identity;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>
/// HU-004 T-03: la fábrica de claims emite id, correo, nombre visible, un <c>Role</c> por rol o equipo y
/// <c>company_id</c> solo para cuentas cliente, tomado siempre de la cuenta y de <c>user_roles</c>. Los tipos
/// de claim y los nombres de rol y de política se escriben como literales, a propósito: son los del contrato
/// T-01. Clase propia porque modifica <c>user_claims</c>, <c>role_claims</c> y <c>roles</c>.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class ClaimsFactoryTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private const string CompanyIdClaim = "company_id";
    private const string DisplayNameClaim = "display_name";
    private const string SecurityStampClaim = "AspNet.Identity.SecurityStamp";
    private const string StoredPermissionClaim = "permission";
    private const string UnknownRole = "Root";

    /// <summary>Tipos de claim de la cookie de un interno: los que emite la aplicación y ninguno más.</summary>
    private static readonly string[] InternalSessionClaimTypes =
    [
        .. new[] { ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Email, ClaimTypes.Role, DisplayNameClaim, SecurityStampClaim }
            .Order(StringComparer.Ordinal),
    ];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Cuenta_cliente_lleva_roles_nombre_visible_y_su_company_id()
    {
        var ana = TestAccounts.AnaPerez;

        var principal = await CreatePrincipalAsync(ana);

        Assert.Equal(ana.Id.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal(ana.Email, principal.FindFirstValue(ClaimTypes.Email));
        Assert.Equal(["Ana Pérez"], Values(principal, DisplayNameClaim));
        Assert.Equal(["Requester"], Values(principal, ClaimTypes.Role));
        Assert.Equal([TestCompanies.Company01.Id.ToString()], Values(principal, CompanyIdClaim));
    }

    [Fact]
    public async Task Cuenta_interna_lleva_roles_y_equipos_pero_no_company_id()
    {
        var principal = await CreatePrincipalAsync(TestAccounts.Elizabeth);

        Assert.Equal(["Elizabeth"], Values(principal, DisplayNameClaim));
        Assert.Equal(["ProductManager", "Production"], Values(principal, ClaimTypes.Role).Order(StringComparer.Ordinal));
        Assert.Empty(Values(principal, CompanyIdClaim));
    }

    [Fact]
    public async Task Company_id_sale_solo_de_la_cuenta_y_no_de_claims_almacenados()
    {
        var foreignCompany = TestCompanies.Company02.Id.ToString();
        var client = TestAccounts.Company01Requester2;
        var internalUser = TestAccounts.Laura;

        // Alguien escribe claims a mano en identity.user_claims: la empresa B y otro nombre visible.
        await api.WithServicesAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var account in new[] { client, internalUser })
            {
                var user = await userManager.FindByIdAsync(account.Id.ToString());
                var result = await userManager.AddClaimsAsync(
                    user!, [new Claim(CompanyIdClaim, foreignCompany), new Claim(DisplayNameClaim, "Nombre Suplantado")]);
                Assert.True(result.Succeeded);
            }

            return true;
        });

        var clientPrincipal = await CreatePrincipalAsync(client);
        var internalPrincipal = await CreatePrincipalAsync(internalUser);

        // El cliente conserva SU empresa (una sola) y el interno sigue sin empresa.
        Assert.Equal([TestCompanies.Company01.Id.ToString()], Values(clientPrincipal, CompanyIdClaim));
        Assert.Equal([client.DisplayName], Values(clientPrincipal, DisplayNameClaim));
        Assert.Empty(Values(internalPrincipal, CompanyIdClaim));
        Assert.Equal([internalUser.DisplayName], Values(internalPrincipal, DisplayNameClaim));
    }

    /// <summary>
    /// Hallazgo MEDIO de la revisión de B4: los roles de la sesión salen solo de <c>identity.user_roles</c>.
    /// Una fila de tipo rol escrita en <c>identity.user_claims</c> o en <c>identity.role_claims</c> no eleva a
    /// nadie, ni en el principal ni en una sesión real.
    /// </summary>
    [Fact]
    public async Task Roles_salen_solo_de_user_roles_y_no_de_claims_almacenados()
    {
        var developer = TestAccounts.JuanDavid;
        var producer = TestAccounts.Gerardo;
        Assert.Equal(["Development"], developer.Roles);
        Assert.Equal(["Production"], producer.Roles);

        await api.WithServicesAsync(async services =>
        {
            // identity.user_claims: a un integrante de Desarrollo le escriben roles y un permiso inventado.
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(developer.Id.ToString());
            var userClaims = await userManager.AddClaimsAsync(
                user!,
                [
                    new Claim(ClaimTypes.Role, "Administrator"),
                    new Claim(ClaimTypes.Role, "ProductManager"),
                    new Claim(StoredPermissionClaim, "tickets.read_all"),
                ]);
            Assert.True(userClaims.Succeeded);

            // identity.role_claims: al equipo Production entero le escriben el rol ProductManager.
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var production = await roleManager.FindByNameAsync("Production");
            foreach (var claim in new[] { new Claim(ClaimTypes.Role, "ProductManager"), new Claim(StoredPermissionClaim, "tickets.read_all") })
            {
                Assert.True((await roleManager.AddClaimAsync(production!, claim)).Succeeded);
            }

            return true;
        });

        var developerPrincipal = await CreatePrincipalAsync(developer);
        var producerPrincipal = await CreatePrincipalAsync(producer);

        Assert.Equal(["Development"], Values(developerPrincipal, ClaimTypes.Role));
        Assert.Equal(["Production"], Values(producerPrincipal, ClaimTypes.Role));
        foreach (var principal in new[] { developerPrincipal, producerPrincipal })
        {
            // Nada de lo almacenado llega a la cookie: solo los tipos que emite la aplicación.
            Assert.Equal(InternalSessionClaimTypes, principal.Claims.Select(claim => claim.Type).Distinct().Order(StringComparer.Ordinal));
            Assert.Equal(["InternalUser"], await SatisfiedPoliciesAsync(principal));
        }

        // Lo mismo con una sesión real: /me no muestra el rol y la política Administrator lo rechaza.
        foreach (var account in new[] { developer, producer })
        {
            using var browser = new TestBrowser(api.Factory);
            using var login = await browser.LoginAsync(account.Email, TestAccounts.SyntheticPassword, Ct);
            Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

            using var me = await browser.GetAsync("/api/auth/me", Ct);
            Assert.Equal(HttpStatusCode.OK, me.StatusCode);
            var body = await ProblemResponse.ReadAsync(me, Ct);
            Assert.Empty(body["roles"]!.AsArray());
            Assert.Equal(account.Roles, body["teams"]!.AsArray().Select(team => team!.GetValue<string>()));

            foreach (var policy in new[] { "Administrator", "ProductManagerOrAdministrator" })
            {
                using var probe = await browser.GetAsync(TestEndpoints.PolicyProbe(policy), Ct);
                Assert.Equal(HttpStatusCode.Forbidden, probe.StatusCode);
            }
        }
    }

    /// <summary>
    /// Los roles de la sesión se filtran por los roles conocidos: un rol ajeno al producto, aunque esté en
    /// <c>identity.roles</c> y asignado en <c>identity.user_roles</c>, no llega a la cookie.
    /// </summary>
    [Fact]
    public async Task Rol_desconocido_de_user_roles_no_llega_a_la_sesion()
    {
        var account = TestAccounts.Cristian;

        await api.WithServicesAsync(async services =>
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var unknown = new IdentityRole<Guid>(UnknownRole) { Id = Guid.CreateVersion7(api.Time.GetUtcNow()) };
            Assert.True((await roleManager.CreateAsync(unknown)).Succeeded);
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(account.Id.ToString());
            Assert.True((await userManager.AddToRoleAsync(user!, UnknownRole)).Succeeded);
            return true;
        });

        var principal = await CreatePrincipalAsync(account);

        Assert.Equal(["Development"], Values(principal, ClaimTypes.Role));
        Assert.Equal(["InternalUser"], await SatisfiedPoliciesAsync(principal));
    }

    /// <summary>
    /// Hallazgo BAJO-1 de la revisión de B4: la fábrica falla cerrado. Una cuenta cuyo perfil no es uno de los
    /// «Perfiles válidos» (hoy no puede tener sesión: la frenan el login y la revalidación del sello) produce
    /// un principal sin roles ni empresa, que no satisface ninguna política con nombre.
    /// </summary>
    [Fact]
    public async Task Perfil_no_valido_no_satisface_ninguna_politica_con_nombre()
    {
        var company = TestCompanies.Company01.Id;
        var invalidProfiles = new[]
        {
            InvalidProfile(1, ["Administrator"], company),
            InvalidProfile(2, ["Development"], company),
            InvalidProfile(3, ["ProductManager", "Production"], company),
            InvalidProfile(4, ["Requester", "Administrator"], company),
            InvalidProfile(5, ["Requester", "Administrator"], null),
            InvalidProfile(6, ["Requester"], null),
            InvalidProfile(7, ["CompanyCoordinator"], null),
            InvalidProfile(8, [], null),
            InvalidProfile(9, [], company),
        };
        await api.WithServicesAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            foreach (var account in invalidProfiles)
            {
                await TestDataSeeder.CreateAsync(userManager, account, passwordHash: null, api.Time.GetUtcNow());
            }

            return true;
        });
        var failures = new List<string>();

        foreach (var account in invalidProfiles)
        {
            var principal = await CreatePrincipalAsync(account);
            var description = $"[{string.Join(", ", account.Roles)}] con empresa {account.CompanyId?.ToString() ?? "(ninguna)"}";

            // Sigue identificando a la cuenta, pero sin nada que conceda acceso.
            Assert.Equal(account.Id.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
            var granted = Values(principal, ClaimTypes.Role);
            var satisfied = await SatisfiedPoliciesAsync(principal);
            if (granted.Count > 0 || satisfied.Count > 0 || Values(principal, CompanyIdClaim).Count > 0)
            {
                failures.Add(
                    $"{description} → roles [{string.Join(", ", granted)}], políticas [{string.Join(", ", satisfied)}], "
                    + $"company_id [{string.Join(", ", Values(principal, CompanyIdClaim))}]");
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private static TestAccount InvalidProfile(int number, string[] roles, Guid? companyId) =>
        new(
            Guid.Parse($"d0000000-0000-7000-8000-{number:D12}"),
            $"perfil-no-valido-{number:D2}@dataticket.local",
            $"Perfil No Válido {number:D2}",
            AccountStatus.Active,
            roles,
            companyId);

    /// <summary>Políticas con nombre REGISTRADAS en el host que el principal satisface.</summary>
    private async Task<List<string>> SatisfiedPoliciesAsync(ClaimsPrincipal principal)
    {
        var authorization = api.Factory.Services.GetRequiredService<IAuthorizationService>();
        var satisfied = new List<string>();
        foreach (var policy in RegisteredPolicies.Names(api.Factory.Services))
        {
            if ((await authorization.AuthorizeAsync(principal, resource: null, policy)).Succeeded)
            {
                satisfied.Add(policy);
            }
        }

        return satisfied;
    }

    private Task<ClaimsPrincipal> CreatePrincipalAsync(TestAccount account) =>
        api.WithServicesAsync(async services =>
        {
            var user = await services.GetRequiredService<UserManager<ApplicationUser>>().FindByIdAsync(account.Id.ToString());
            return await services.GetRequiredService<IUserClaimsPrincipalFactory<ApplicationUser>>().CreateAsync(user!);
        });

    private static List<string> Values(ClaimsPrincipal principal, string claimType) =>
        [.. principal.FindAll(claimType).Select(claim => claim.Value)];
}
