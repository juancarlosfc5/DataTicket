using System.Net;
using System.Security.Claims;
using DataTicket.Infrastructure.Identity;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>
/// HU-004 CHU-05 a CHU-08 con sesiones reales: la cookie que emite el login lleva los claims que las
/// políticas y <c>ICurrentUser</c> necesitan, y un perfil sin empresa falla cerrado.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class SessionAuthorizationTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private const string ClientUser = "ClientUser";
    private const string InternalUser = "InternalUser";
    private const string Administrator = "Administrator";
    private const string ProductManagerOrAdministrator = "ProductManagerOrAdministrator";
    private const string ForbiddenTitle = "No tienes permiso para realizar esta acción.";

    private static readonly string[] AllPolicies = [ClientUser, InternalUser, Administrator, ProductManagerOrAdministrator];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Politicas_con_sesion_real_cumplen_la_matriz()
    {
        var matrix = new (TestAccount Account, string[] Granted)[]
        {
            (TestAccounts.AnaPerez, [ClientUser]),
            (TestAccounts.Company02Coordinator, [ClientUser]),
            (TestAccounts.Elizabeth, [InternalUser, ProductManagerOrAdministrator]),
            (TestAccounts.Admin, [InternalUser, Administrator, ProductManagerOrAdministrator]),
            (TestAccounts.Laura, [InternalUser]),
            (TestAccounts.Julian, [InternalUser]),
        };
        var mismatches = new List<string>();

        foreach (var (account, granted) in matrix)
        {
            using var browser = await SignedInAsync(api.Factory, account);
            foreach (var policy in AllPolicies)
            {
                using var response = await browser.GetAsync(TestEndpoints.PolicyProbe(policy), Ct);
                var expected = granted.Contains(policy) ? HttpStatusCode.NoContent : HttpStatusCode.Forbidden;
                if (response.StatusCode != expected)
                {
                    mismatches.Add($"{account.Email} → {policy}: esperado {(int)expected}, obtuvo {(int)response.StatusCode}");
                }
            }
        }

        Assert.True(mismatches.Count == 0, string.Join(Environment.NewLine, mismatches));
    }

    /// <summary>CHU-07: un caso de uso que lee <c>ICurrentUser.CompanyId</c> obtiene la empresa de la sesión.</summary>
    [Fact]
    public async Task ICurrentUser_entrega_la_empresa_de_la_sesion_y_no_la_de_la_peticion()
    {
        var companyB = TestCompanies.Company02.Id;
        using var browser = await SignedInAsync(api.Factory, TestAccounts.AnaPerez);
        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"{TestEndpoints.ClientContext}?status=abierto&companyId={companyB}&company_id={companyB}");
        request.Headers.Add("X-Company-Id", companyB.ToString());

        using var response = await browser.SendAsync(request, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ProblemResponse.ReadAsync(response, Ct);
        Assert.Equal(TestCompanies.Company01.Id.ToString(), body["companyId"]!.GetValue<string>());
        Assert.Equal("abierto", body["status"]!.GetValue<string>());
    }

    /// <summary>
    /// CHU-07: una sesión de cliente cuya cookie no trae <c>company_id</c> (aquí, emitida por una fábrica de
    /// claims defectuosa) sigue autenticada pero no está autorizada: 403, nunca una empresa inventada.
    /// </summary>
    [Fact]
    public async Task Cliente_sin_company_id_falla_cerrado()
    {
        using var faulty = api.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            var original = services.Last(descriptor => descriptor.ServiceType == typeof(IUserClaimsPrincipalFactory<ApplicationUser>));
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>>(provider => new CompanyStrippingClaimsFactory(
                (IUserClaimsPrincipalFactory<ApplicationUser>)ActivatorUtilities.CreateInstance(provider, original.ImplementationType!)));
        }));
        using var browser = await SignedInAsync(faulty, TestAccounts.AnaPerez);

        // Autenticada (no es un 401)…
        using var authenticated = await browser.GetAsync(TestEndpoints.Authenticated, Ct);
        Assert.Equal(HttpStatusCode.OK, authenticated.StatusCode);

        // …pero sin empresa no pasa ClientUser, ni obtiene contexto, ni ninguna otra política.
        foreach (var path in new[] { TestEndpoints.PolicyProbe(ClientUser), TestEndpoints.ClientContext, "/api/auth/me" })
        {
            using var response = await browser.GetAsync(path, Ct);
            var problem = await ProblemResponse.AssertAsync(response, HttpStatusCode.Forbidden, ForbiddenTitle, Ct);
            Assert.False(problem.ContainsKey("company"));
            Assert.False(problem.ContainsKey("companyId"));
        }

        foreach (var policy in new[] { InternalUser, Administrator, ProductManagerOrAdministrator })
        {
            using var response = await browser.GetAsync(TestEndpoints.PolicyProbe(policy), Ct);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Control: en el host normal la misma cuenta sí pasa ClientUser.
        using var healthy = await SignedInAsync(api.Factory, TestAccounts.AnaPerez);
        using var control = await healthy.GetAsync(TestEndpoints.PolicyProbe(ClientUser), Ct);
        Assert.Equal(HttpStatusCode.NoContent, control.StatusCode);
    }

    /// <summary>
    /// CHU-06, segunda mitad del control: el endpoint sin metadatos existe y responde a un usuario
    /// autenticado; lo único que lo protege de los anónimos es la política de reserva.
    /// </summary>
    [Fact]
    public async Task Endpoint_sin_metadatos_admite_a_cualquier_usuario_autenticado()
    {
        using var anonymous = new TestBrowser(api.Factory);
        using var withoutSession = await anonymous.GetAsync(TestEndpoints.NoMetadata, Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, withoutSession.StatusCode);

        foreach (var account in new[] { TestAccounts.Company01Requester2, TestAccounts.Kevin })
        {
            using var browser = await SignedInAsync(api.Factory, account);
            using var response = await browser.GetAsync(TestEndpoints.NoMetadata, Ct);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }

    /// <summary>Con sesión, una ruta que no existe también responde 404: la sesión no revela ni crea rutas.</summary>
    [Fact]
    public async Task Ruta_inexistente_con_sesion_responde_404()
    {
        using var browser = await SignedInAsync(api.Factory, TestAccounts.Brayan);

        foreach (var path in new[] { "/api/no-existe", "/api/auth/no-existe", "/no-existe" })
        {
            using var response = await browser.GetAsync(path, Ct);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    private static async Task<TestBrowser> SignedInAsync(WebApplicationFactory<Program> factory, TestAccount account)
    {
        var browser = new TestBrowser(factory);
        try
        {
            using var login = await browser.LoginAsync(account.Email, TestAccounts.SyntheticPassword, Ct);
            Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
            return browser;
        }
        catch
        {
            browser.Dispose();
            throw;
        }
    }

    /// <summary>Envuelve la fábrica real y descarta <c>company_id</c> del principal que produce.</summary>
    private sealed class CompanyStrippingClaimsFactory(IUserClaimsPrincipalFactory<ApplicationUser> inner)
        : IUserClaimsPrincipalFactory<ApplicationUser>
    {
        public async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            var principal = await inner.CreateAsync(user);
            foreach (var identity in principal.Identities)
            {
                foreach (var claim in identity.FindAll(DataTicketClaimTypes.CompanyId).ToList())
                {
                    identity.RemoveClaim(claim);
                }
            }

            return principal;
        }
    }
}
