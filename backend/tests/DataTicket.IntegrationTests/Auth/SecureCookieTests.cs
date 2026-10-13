using System.Net;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>HU-003 CHU-02: fuera de Development las cookies llevan <c>Secure</c>.</summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class SecureCookieTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Login_fuera_de_Development_emite_cookie_Secure()
    {
        await using var staging = api.CreateFactory("Staging");
        using var browser = new TestBrowser(staging, TestBrowser.Https);

        var token = await browser.FetchAntiforgeryTokenAsync(Ct);
        using var response = await browser.PostJsonAsync(
            "/api/auth/login",
            new { email = TestAccounts.AnaPerez.Email, password = TestAccounts.SyntheticPassword },
            token,
            Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var authCookie = TestBrowser.FindSetCookie(response, TestBrowser.AuthCookie);
        Assert.NotNull(authCookie);
        Assert.True(authCookie.Secure);
        Assert.True(authCookie.HttpOnly);
        Assert.Contains(TestBrowser.AntiforgeryCookie, browser.Cookies.Keys);
    }

    [Fact]
    public async Task Cookie_antiforgery_fuera_de_Development_es_Secure_HttpOnly_y_Strict()
    {
        await using var staging = api.CreateFactory("Staging");
        using var browser = new TestBrowser(staging, TestBrowser.Https);

        using var response = await browser.GetAsync("/api/auth/antiforgery", Ct);

        var cookie = TestBrowser.FindSetCookie(response, TestBrowser.AntiforgeryCookie);
        Assert.NotNull(cookie);
        Assert.True(cookie.Secure);
        Assert.True(cookie.HttpOnly);
        Assert.Equal(Microsoft.Net.Http.Headers.SameSiteMode.Strict, cookie.SameSite);
    }

    /// <summary>
    /// Sobre HTTPS, <c>Always</c> y <c>SameAsRequest</c> emiten la misma cabecera: lo que las distingue es la
    /// política configurada. Fuera de Development debe ser <c>Always</c> (nunca depender del esquema recibido).
    /// </summary>
    [Fact]
    public async Task Fuera_de_Development_la_politica_Secure_de_las_cookies_es_Always()
    {
        await using var staging = api.CreateFactory("Staging");

        Assert.Equal(CookieSecurePolicy.Always, AuthCookieSecurePolicy(staging));
        Assert.Equal(CookieSecurePolicy.Always, AntiforgeryCookieSecurePolicy(staging));
    }

    /// <summary>En Development (<c>http://localhost</c>) la cookie sigue el esquema de la petición (T-01).</summary>
    [Fact]
    public void En_Development_la_politica_Secure_de_las_cookies_es_SameAsRequest()
    {
        Assert.Equal(CookieSecurePolicy.SameAsRequest, AuthCookieSecurePolicy(api.Factory));
        Assert.Equal(CookieSecurePolicy.SameAsRequest, AntiforgeryCookieSecurePolicy(api.Factory));
    }

    private static CookieSecurePolicy AuthCookieSecurePolicy(WebApplicationFactory<Program> factory) =>
        factory.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme).Cookie.SecurePolicy;

    private static CookieSecurePolicy AntiforgeryCookieSecurePolicy(WebApplicationFactory<Program> factory) =>
        factory.Services.GetRequiredService<IOptions<AntiforgeryOptions>>().Value.Cookie.SecurePolicy;
}
