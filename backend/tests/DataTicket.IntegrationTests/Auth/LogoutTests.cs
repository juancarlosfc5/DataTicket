using System.Net;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>HU-003 CHU-08: el cierre de sesión expira la cookie.</summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class LogoutTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Logout_expira_la_cookie()
    {
        using var browser = new TestBrowser(api.Factory);
        using var login = await browser.LoginAsync(TestAccounts.Elizabeth.Email, TestAccounts.SyntheticPassword, Ct);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        var token = await browser.FetchAntiforgeryTokenAsync(Ct);
        using var logout = await browser.PostJsonAsync("/api/auth/logout", null, token, Ct);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        var expired = TestBrowser.FindSetCookie(logout, TestBrowser.AuthCookie);
        Assert.NotNull(expired);
        Assert.True(expired.Expires < api.Time.GetUtcNow(), "La cookie de sesión debe llegar expirada.");
        Assert.Equal("/", expired.Path.ToString());
        Assert.DoesNotContain(TestBrowser.AuthCookie, browser.Cookies.Keys);

        using var afterLogout = await browser.GetAsync(TestEndpoints.Authenticated, Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, afterLogout.StatusCode);
    }
}
