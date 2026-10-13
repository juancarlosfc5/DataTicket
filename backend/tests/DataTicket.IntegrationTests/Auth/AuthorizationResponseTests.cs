using System.Net;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>HU-003 CHU-10: la API responde 401/403 en ProblemDetails, nunca 302 ni <c>Location</c>.</summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class AuthorizationResponseTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private const string UnauthenticatedTitle = "Se requiere iniciar sesión.";
    private const string ForbiddenTitle = "No tienes permiso para realizar esta acción.";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Api_sin_sesion_responde_401_sin_Location()
    {
        using var browser = new TestBrowser(api.Factory);
        var token = await browser.FetchAntiforgeryTokenAsync(Ct);

        using var get = await browser.GetAsync(TestEndpoints.Authenticated, Ct);
        await ProblemResponse.AssertAsync(get, HttpStatusCode.Unauthorized, UnauthenticatedTitle, Ct);

        // El logout exige sesión: sin ella, 401 (la autorización actúa antes que el filtro antiforgery).
        using var logout = await browser.PostJsonAsync("/api/auth/logout", null, token, Ct);
        await ProblemResponse.AssertAsync(logout, HttpStatusCode.Unauthorized, UnauthenticatedTitle, Ct);
    }

    /// <summary>
    /// El 401 no depende de <c>Accept</c>: una navegación (<c>text/html</c>) o un cliente de texto plano
    /// reciben el mismo ProblemDetails que el SPA, nunca un 500 ni un cuerpo vacío.
    /// </summary>
    [Theory]
    [InlineData("text/html")]
    [InlineData("text/plain")]
    [InlineData("text/html,application/xhtml+xml,application/xml;q=0.9")]
    public async Task Api_sin_sesion_con_Accept_text_html_responde_401(string accept)
    {
        using var browser = new TestBrowser(api.Factory);
        using var reference = await browser.GetAsync(TestEndpoints.Authenticated, Ct);
        var expected = await ProblemResponse.ReadWithoutTraceIdAsync(reference, Ct);

        using var response = await browser.GetAsync(TestEndpoints.Authenticated, accept, Ct);

        var body = await ProblemResponse.AssertAsync(response, HttpStatusCode.Unauthorized, UnauthenticatedTitle, Ct);
        Assert.False(string.IsNullOrEmpty(body["traceId"]?.GetValue<string>()), "Falta traceId.");
        Assert.Equal(expected, await ProblemResponse.ReadWithoutTraceIdAsync(response, Ct));
    }

    [Fact]
    public async Task Api_sin_rol_requerido_responde_403_sin_Location()
    {
        using var browser = new TestBrowser(api.Factory);
        using var login = await browser.LoginAsync(TestAccounts.AnaPerez.Email, TestAccounts.SyntheticPassword, Ct);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        using var response = await browser.GetAsync(TestEndpoints.AdministratorOnly, Ct);

        await ProblemResponse.AssertAsync(response, HttpStatusCode.Forbidden, ForbiddenTitle, Ct);
    }

    [Theory]
    [InlineData("text/html")]
    [InlineData("text/plain")]
    [InlineData("text/html,application/xhtml+xml,application/xml;q=0.9")]
    public async Task Api_sin_rol_requerido_con_Accept_text_html_responde_403(string accept)
    {
        using var browser = new TestBrowser(api.Factory);
        using var login = await browser.LoginAsync(TestAccounts.AnaPerez.Email, TestAccounts.SyntheticPassword, Ct);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        using var reference = await browser.GetAsync(TestEndpoints.AdministratorOnly, Ct);
        var expected = await ProblemResponse.ReadWithoutTraceIdAsync(reference, Ct);

        using var response = await browser.GetAsync(TestEndpoints.AdministratorOnly, accept, Ct);

        var body = await ProblemResponse.AssertAsync(response, HttpStatusCode.Forbidden, ForbiddenTitle, Ct);
        Assert.False(string.IsNullOrEmpty(body["traceId"]?.GetValue<string>()), "Falta traceId.");
        Assert.Equal(expected, await ProblemResponse.ReadWithoutTraceIdAsync(response, Ct));
    }

    [Fact]
    public async Task Api_con_rol_requerido_responde_204()
    {
        using var browser = new TestBrowser(api.Factory);
        using var login = await browser.LoginAsync(TestAccounts.Admin.Email, TestAccounts.SyntheticPassword, Ct);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        using var response = await browser.GetAsync(TestEndpoints.AdministratorOnly, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
