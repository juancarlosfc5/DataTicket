using System.Net;
using System.Text.Json.Nodes;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>
/// HU-004 CHU-01 a CHU-04: <c>GET /api/auth/me</c> según el contrato T-01. Los nombres de los campos y los
/// valores de <c>surface</c> se escriben como literales, a propósito: son los que consume el SPA.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class MeEndpointTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private const string Me = "/api/auth/me";
    private const string UnauthenticatedTitle = "Se requiere iniciar sesión.";

    private static readonly string[] ContractFields = ["id", "displayName", "email", "surface", "roles", "teams", "company"];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Me_de_cliente_incluye_solo_su_empresa()
    {
        var ana = TestAccounts.AnaPerez;
        using var browser = await SignedInAsync(ana);

        using var response = await browser.GetAsync(Me, Ct);

        var body = await ReadMeAsync(response);
        Assert.Equal("b0000000-0000-7000-8000-000001000001", body["id"]!.GetValue<string>());
        Assert.Equal("Ana Pérez", body["displayName"]!.GetValue<string>());
        Assert.Equal("ana.perez@empresa-sintetica-01.test", body["email"]!.GetValue<string>());
        Assert.Equal("client", body["surface"]!.GetValue<string>());
        Assert.Equal(["Requester"], Strings(body["roles"]));
        Assert.Empty(Strings(body["teams"]));

        var company = body["company"]!.AsObject();
        Assert.Equal(["id", "name"], company.Select(property => property.Key));
        Assert.Equal("c0000000-0000-7000-8000-000000000001", company["id"]!.GetValue<string>());
        Assert.Equal("Empresa Sintética 01", company["name"]!.GetValue<string>());

        // Nada de la empresa B en la respuesta (el objeto company solo tiene el id y el nombre de A).
        Assert.DoesNotContain(TestCompanies.Company02.Id.ToString(), body.ToJsonString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString() ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Me_de_interno_no_tiene_empresa()
    {
        using var browser = await SignedInAsync(TestAccounts.Elizabeth);

        using var response = await browser.GetAsync(Me, Ct);

        var body = await ReadMeAsync(response);
        Assert.Equal("a0000000-0000-7000-8000-000000000001", body["id"]!.GetValue<string>());
        Assert.Equal("Elizabeth", body["displayName"]!.GetValue<string>());
        Assert.Equal("elizabeth@dataticket.local", body["email"]!.GetValue<string>());
        Assert.Equal("internal", body["surface"]!.GetValue<string>());
        Assert.Equal(["ProductManager"], Strings(body["roles"]));
        Assert.Equal(["Production"], Strings(body["teams"]));
        // El campo existe y vale null (el SPA lo espera así), no se omite.
        Assert.True(body.ContainsKey("company"));
        Assert.Null(body["company"]);
    }

    /// <summary>
    /// Los demás perfiles del catálogo: roles y equipos van en campos distintos y solo los clientes tienen empresa.
    /// </summary>
    [Fact]
    public async Task Me_separa_roles_y_equipos_en_cada_perfil()
    {
        var expectations = new (TestAccount Account, string Surface, string[] Roles, string[] Teams, Guid? CompanyId)[]
        {
            (TestAccounts.Admin, "internal", ["Administrator"], [], null),
            (TestAccounts.Laura, "internal", [], ["Development"], null),
            (TestAccounts.Julian, "internal", [], ["Production"], null),
            (TestAccounts.Company01Coordinator, "client", ["CompanyCoordinator"], [], TestCompanies.Company01.Id),
            (TestAccounts.Company02Coordinator, "client", ["CompanyCoordinator"], [], TestCompanies.Company02.Id),
        };

        foreach (var (account, surface, roles, teams, companyId) in expectations)
        {
            using var browser = await SignedInAsync(account);
            using var response = await browser.GetAsync(Me, Ct);

            var body = await ReadMeAsync(response);
            Assert.Equal(account.Id.ToString(), body["id"]!.GetValue<string>());
            Assert.Equal(account.DisplayName, body["displayName"]!.GetValue<string>());
            Assert.Equal(account.Email, body["email"]!.GetValue<string>());
            Assert.Equal(surface, body["surface"]!.GetValue<string>());
            Assert.Equal(roles, Strings(body["roles"]));
            Assert.Equal(teams, Strings(body["teams"]));
            Assert.Equal(companyId?.ToString(), body["company"]?["id"]?.GetValue<string>());
        }
    }

    /// <summary>CHU-03: prueba negativa A↔B. <c>/me</c> no admite parámetros; describe siempre a quien pregunta.</summary>
    [Fact]
    public async Task Me_ignora_userId_y_cabeceras_de_empresa()
    {
        var ana = TestAccounts.AnaPerez;
        var luis = TestAccounts.Company02Requester1;
        Assert.NotEqual(ana.CompanyId, luis.CompanyId);

        // La Solicitante de A pide los datos de un usuario de B; y a la inversa.
        var anaView = await AskAboutAnotherUserAsync(asker: ana, target: luis);
        var luisView = await AskAboutAnotherUserAsync(asker: luis, target: ana);

        AssertOwnContextOnly(anaView, ana, TestCompanies.Company01, foreign: luis, foreignCompany: TestCompanies.Company02);
        AssertOwnContextOnly(luisView, luis, TestCompanies.Company02, foreign: ana, foreignCompany: TestCompanies.Company01);
    }

    [Fact]
    public async Task Me_sin_sesion_responde_401()
    {
        // Sin cookie.
        using var anonymous = new TestBrowser(api.Factory);
        using var withoutCookie = await anonymous.GetAsync(Me, Ct);
        await ProblemResponse.AssertAsync(withoutCookie, HttpStatusCode.Unauthorized, UnauthenticatedTitle, Ct);
        Assert.Null(TestBrowser.FindSetCookie(withoutCookie, TestBrowser.AuthCookie));

        // Con una cookie que no emitió el servidor.
        using var forged = new TestBrowser(api.Factory);
        forged.SetCookie(TestBrowser.AuthCookie, "CfDJ8-valor-inventado");
        using var withForgedCookie = await forged.GetAsync(Me, Ct);
        await ProblemResponse.AssertAsync(withForgedCookie, HttpStatusCode.Unauthorized, UnauthenticatedTitle, Ct);

        // Tras cerrar la sesión en el navegador.
        using var browser = await SignedInAsync(TestAccounts.Gerardo);
        using var whileSignedIn = await browser.GetAsync(Me, Ct);
        Assert.Equal(HttpStatusCode.OK, whileSignedIn.StatusCode);
        var token = await browser.FetchAntiforgeryTokenAsync(Ct);
        using var logout = await browser.PostJsonAsync("/api/auth/logout", null, token, Ct);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        using var afterLogout = await browser.GetAsync(Me, Ct);
        var problem = await ProblemResponse.AssertAsync(afterLogout, HttpStatusCode.Unauthorized, UnauthenticatedTitle, Ct);
        Assert.False(problem.ContainsKey("id"));
        Assert.False(problem.ContainsKey("email"));
    }

    private async Task<JsonObject> AskAboutAnotherUserAsync(TestAccount asker, TestAccount target)
    {
        using var browser = await SignedInAsync(asker);
        using var plain = await browser.GetAsync(Me, Ct);
        var expected = (await ReadMeAsync(plain)).ToJsonString();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{Me}?userId={target.Id}&id={target.Id}&companyId={target.CompanyId}&company_id={target.CompanyId}&email={target.Email}");
        request.Headers.Add("X-Company-Id", target.CompanyId.ToString());
        request.Headers.Add("X-User-Id", target.Id.ToString());
        request.Headers.Add("X-Forwarded-User", target.Email);
        using var response = await browser.SendAsync(request, Ct);

        var body = await ReadMeAsync(response);
        // Byte a byte la misma respuesta que sin parámetros.
        Assert.Equal(expected, body.ToJsonString());
        return body;
    }

    private static void AssertOwnContextOnly(
        JsonObject body, TestAccount own, TestCompany ownCompany, TestAccount foreign, TestCompany foreignCompany)
    {
        Assert.Equal(own.Id.ToString(), body["id"]!.GetValue<string>());
        Assert.Equal(own.Email, body["email"]!.GetValue<string>());
        Assert.Equal(ownCompany.Id.ToString(), body["company"]!["id"]!.GetValue<string>());
        Assert.Equal(ownCompany.Name, body["company"]!["name"]!.GetValue<string>());

        var raw = body.ToJsonString();
        Assert.DoesNotContain(foreign.Id.ToString(), raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(foreign.Email, raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(foreignCompany.Id.ToString(), raw, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(foreignCompany.Name, body["company"]!["name"]!.GetValue<string>());
    }

    private async Task<TestBrowser> SignedInAsync(TestAccount account)
    {
        var browser = new TestBrowser(api.Factory);
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

    /// <summary>200, JSON y exactamente los campos del contrato, en su orden.</summary>
    private static async Task<JsonObject> ReadMeAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var body = await ProblemResponse.ReadAsync(response, Ct);
        Assert.Equal(ContractFields, body.Select(property => property.Key));
        return body;
    }

    private static List<string> Strings(JsonNode? array) =>
        [.. array!.AsArray().Select(item => item!.GetValue<string>())];
}
