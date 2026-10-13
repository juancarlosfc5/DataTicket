using System.Net;
using DataTicket.IntegrationTests.Fixtures;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// El navegador de pruebas comparte el reloj del host: decide si una cookie llegó expirada con el
/// <see cref="TimeProvider"/> de la aplicación, no con el reloj real de la máquina.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class TestBrowserTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Navegador_conserva_una_cookie_que_expira_en_el_futuro_del_reloj_del_host()
    {
        // El reloj del host arranca en TestClock.Start (pasado respecto al reloj real): una cookie que vence
        // una hora después sigue vigente para el host, aunque para el reloj real ya habría expirado.
        using var browser = new TestBrowser(api.Factory);
        using var login = await browser.LoginAsync(TestAccounts.Kevin.Email, TestAccounts.SyntheticPassword, Ct);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        using var response = await browser.GetAsync(TestEndpoints.PersistentCookie, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains(TestEndpoints.PersistentCookieName, browser.Cookies.Keys);
    }
}
