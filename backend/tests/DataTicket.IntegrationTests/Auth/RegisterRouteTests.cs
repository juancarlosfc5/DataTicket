using System.Net;
using DataTicket.IntegrationTests.Infrastructure;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>HU-003 CHU-09: no hay registro público ni rutas de <c>MapIdentityApi</c> (PRD §5).</summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class RegisterRouteTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task No_existe_ruta_register()
    {
        using var browser = new TestBrowser(api.Factory);
        var token = await browser.FetchAntiforgeryTokenAsync(Ct);
        var body = new { email = "nuevo@empresa-sintetica-01.test", password = "Clave-Sintetica-2026" };

        foreach (var path in new[] { "/api/auth/register", "/register", "/api/register" })
        {
            using var response = await browser.PostJsonAsync(path, body, token, Ct);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        using var openApi = await browser.GetAsync("/openapi/v1.json", Ct);
        Assert.Equal(HttpStatusCode.OK, openApi.StatusCode);
        var document = await openApi.Content.ReadAsStringAsync(Ct);
        foreach (var forbidden in new[] { "register", "confirmEmail", "manage", "refresh" })
        {
            Assert.DoesNotContain(forbidden, document, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("/api/auth/login", document, StringComparison.Ordinal);
    }
}
