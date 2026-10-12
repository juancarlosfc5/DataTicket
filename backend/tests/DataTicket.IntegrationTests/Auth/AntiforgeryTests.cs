using System.Net;
using System.Text.Json;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>HU-003 CHU-07: antiforgery obligatorio en las mutaciones de <c>/api</c>.</summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class AntiforgeryTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private const string InvalidRequestTitle = "La solicitud no es válida.";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static object Credentials => new { email = TestAccounts.AnaPerez.Email, password = TestAccounts.SyntheticPassword };

    [Fact]
    public async Task Mutacion_sin_antiforgery_responde_400()
    {
        using var browser = new TestBrowser(api.Factory);
        await browser.FetchAntiforgeryTokenAsync(Ct);

        // Login sin cabecera: 400 con code antiforgery_invalid, sin errors, y sin sesión.
        using var withoutHeader = await browser.PostJsonAsync("/api/auth/login", Credentials, antiforgeryToken: null, Ct);
        await AssertAntiforgeryRejectedAsync(withoutHeader);
        Assert.Null(TestBrowser.FindSetCookie(withoutHeader, TestBrowser.AuthCookie));

        // Login con el token de otra sesión (otro navegador): también 400.
        using var otherBrowser = new TestBrowser(api.Factory);
        var foreignToken = await otherBrowser.FetchAntiforgeryTokenAsync(Ct);
        using var withForeignToken = await browser.PostJsonAsync("/api/auth/login", Credentials, foreignToken, Ct);
        await AssertAntiforgeryRejectedAsync(withForeignToken);

        // Tras un login correcto, el token anterior ya no vale para el logout y la sesión sigue intacta.
        var tokenBeforeLogin = await browser.FetchAntiforgeryTokenAsync(Ct);
        using var login = await browser.PostJsonAsync("/api/auth/login", Credentials, tokenBeforeLogin, Ct);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        using var logoutWithOldToken = await browser.PostJsonAsync("/api/auth/logout", null, tokenBeforeLogin, Ct);
        await AssertAntiforgeryRejectedAsync(logoutWithOldToken);
        using var stillSignedIn = await browser.GetAsync(TestEndpoints.Authenticated, Ct);
        Assert.Equal(HttpStatusCode.OK, stillSignedIn.StatusCode);

        using var logoutWithoutHeader = await browser.PostJsonAsync("/api/auth/logout", null, antiforgeryToken: null, Ct);
        await AssertAntiforgeryRejectedAsync(logoutWithoutHeader);

        // Con el token pedido DESPUÉS del login, el logout funciona.
        var tokenAfterLogin = await browser.FetchAntiforgeryTokenAsync(Ct);
        using var logout = await browser.PostJsonAsync("/api/auth/logout", null, tokenAfterLogin, Ct);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
    }

    [Fact]
    public async Task Endpoint_antiforgery_entrega_token_sin_cache_y_cookie_estricta()
    {
        using var browser = new TestBrowser(api.Factory);

        using var response = await browser.GetAsync("/api/auth/antiforgery", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString() ?? string.Empty, StringComparison.Ordinal);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        Assert.Equal(TestBrowser.AntiforgeryHeader, json.RootElement.GetProperty("headerName").GetString());
        Assert.False(string.IsNullOrEmpty(json.RootElement.GetProperty("token").GetString()));

        var cookie = TestBrowser.FindSetCookie(response, TestBrowser.AntiforgeryCookie);
        Assert.NotNull(cookie);
        Assert.True(cookie.HttpOnly);
        Assert.Equal(Microsoft.Net.Http.Headers.SameSiteMode.Strict, cookie.SameSite);
    }

    private static async Task AssertAntiforgeryRejectedAsync(HttpResponseMessage response)
    {
        var body = await ProblemResponse.AssertAsync(response, HttpStatusCode.BadRequest, InvalidRequestTitle, Ct);
        Assert.Equal("antiforgery_invalid", body["code"]?.GetValue<string>());
        Assert.Null(body["errors"]);
    }
}
