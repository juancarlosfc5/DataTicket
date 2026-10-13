using System.Net;
using System.Net.Http.Json;
using System.Text;
using DataTicket.Domain.Users;
using DataTicket.Infrastructure.Identity;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>HU-003: CHU-01, CHU-02 (Development), CHU-03, CHU-04 y CHU-06.</summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class LoginTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private const string GenericTitle = "Correo o contraseña incorrectos.";
    private const string ValidationTitle = "Uno o más campos no son válidos.";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Login_valido_emite_cookie_HttpOnly_SameSiteLax()
    {
        // Development sobre http://localhost (CHU-02): funciona y la cookie no exige Secure.
        using var browser = new TestBrowser(api.Factory, TestBrowser.Http);

        using var response = await browser.LoginAsync(TestAccounts.AnaPerez.Email, TestAccounts.SyntheticPassword, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var cookie = TestBrowser.FindSetCookie(response, TestBrowser.AuthCookie);
        Assert.NotNull(cookie);
        Assert.True(cookie.HttpOnly);
        Assert.Equal(SameSiteMode.Lax, cookie.SameSite);
        Assert.Equal("/", cookie.Path.ToString());
        Assert.False(cookie.Secure);

        using var next = await browser.GetAsync(TestEndpoints.Authenticated, Ct);
        Assert.Equal(HttpStatusCode.OK, next.StatusCode);
        Assert.Contains(TestAccounts.AnaPerez.Id.ToString(), await next.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_con_correo_inexistente_y_clave_incorrecta_responde_igual()
    {
        using var browser = new TestBrowser(api.Factory);

        using var unknown = await browser.LoginAsync("nadie@empresa-sintetica-99.test", TestAccounts.SyntheticPassword, Ct);
        using var wrongPassword = await browser.LoginAsync(TestAccounts.Company01Requester2.Email, "Clave-Incorrecta-2026", Ct);

        await ProblemResponse.AssertAsync(unknown, HttpStatusCode.Unauthorized, GenericTitle, Ct);
        await ProblemResponse.AssertAsync(wrongPassword, HttpStatusCode.Unauthorized, GenericTitle, Ct);
        Assert.Equal(
            await ProblemResponse.ReadWithoutTraceIdAsync(unknown, Ct),
            await ProblemResponse.ReadWithoutTraceIdAsync(wrongPassword, Ct));
        Assert.Null(TestBrowser.FindSetCookie(unknown, TestBrowser.AuthCookie));
        Assert.Null(TestBrowser.FindSetCookie(wrongPassword, TestBrowser.AuthCookie));
    }

    [Fact]
    public async Task Login_de_cuenta_desactivada_o_pendiente_responde_401_generico()
    {
        // La cuenta Invited no tiene contraseña: la prueba le fija la sintética (CHU-04).
        var passwordAdded = await api.WithServicesAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var invited = await userManager.FindByIdAsync(TestAccounts.Company01Invited.Id.ToString());
            return await userManager.AddPasswordAsync(invited!, TestAccounts.SyntheticPassword);
        });
        Assert.True(passwordAdded.Succeeded, string.Join("; ", passwordAdded.Errors.Select(error => error.Description)));
        using var browser = new TestBrowser(api.Factory);
        using var reference = await browser.LoginAsync("nadie@empresa-sintetica-99.test", TestAccounts.SyntheticPassword, Ct);
        var expected = await ProblemResponse.ReadWithoutTraceIdAsync(reference, Ct);

        foreach (var account in new[] { TestAccounts.Company01Deactivated, TestAccounts.Company01Invited })
        {
            using var response = await browser.LoginAsync(account.Email, TestAccounts.SyntheticPassword, Ct);

            await ProblemResponse.AssertAsync(response, HttpStatusCode.Unauthorized, GenericTitle, Ct);
            Assert.Equal(expected, await ProblemResponse.ReadWithoutTraceIdAsync(response, Ct));
            Assert.Null(TestBrowser.FindSetCookie(response, TestBrowser.AuthCookie));
        }
    }

    [Fact]
    public async Task Login_con_perfil_invalido_responde_401_generico()
    {
        // Activa y con clave correcta, pero mezcla rol de cliente e interno: falla cerrado (T-01).
        var mixed = new TestAccount(
            Guid.Parse("b0000000-0000-7000-8000-000001000099"),
            "mixto@empresa-sintetica-01.test",
            "Perfil Mixto",
            AccountStatus.Active,
            [TestRoles.Requester, TestRoles.Administrator],
            TestCompanies.Company01.Id);
        await api.WithServicesAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var hash = services.GetRequiredService<IPasswordHasher<ApplicationUser>>()
                .HashPassword(new ApplicationUser(), TestAccounts.SyntheticPassword);
            return await TestDataSeeder.CreateAsync(userManager, mixed, hash, api.Time.GetUtcNow());
        });
        using var browser = new TestBrowser(api.Factory);

        using var response = await browser.LoginAsync(mixed.Email, TestAccounts.SyntheticPassword, Ct);

        await ProblemResponse.AssertAsync(response, HttpStatusCode.Unauthorized, GenericTitle, Ct);
        Assert.Null(TestBrowser.FindSetCookie(response, TestBrowser.AuthCookie));
    }

    [Fact]
    public async Task Login_valida_limites_de_email()
    {
        using var browser = new TestBrowser(api.Factory);

        await AssertFieldErrorAsync(browser, Email(257), TestAccounts.SyntheticPassword, "email");
        await AssertFieldErrorAsync(browser, "ana.perez", TestAccounts.SyntheticPassword, "email");
        await AssertFieldErrorAsync(browser, "", TestAccounts.SyntheticPassword, "email");
        await AssertFieldErrorAsync(browser, TestAccounts.AnaPerez.Email, "", "password");

        // 256 caracteres exactos con formato válido no es error de validación: no existe → 401 genérico.
        using var boundary = await browser.LoginAsync(Email(256), TestAccounts.SyntheticPassword, Ct);
        await ProblemResponse.AssertAsync(boundary, HttpStatusCode.Unauthorized, GenericTitle, Ct);
    }

    /// <summary>
    /// Un cuerpo que no es JSON válido es un error del cliente en CUALQUIER entorno: 400 en ProblemDetails,
    /// sin sesión y sin detalles internos (ni excepción ni traza).
    /// </summary>
    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    public async Task Login_con_JSON_mal_formado_responde_400(string environment)
    {
        await using var host = api.CreateFactory(environment);
        using var browser = new TestBrowser(host);
        var token = await browser.FetchAntiforgeryTokenAsync(Ct);
        using var malformed = new StringContent("{\"email\": ", Encoding.UTF8, "application/json");

        using var response = await browser.PostAsync("/api/auth/login", malformed, token, accept: null, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ProblemResponse.ContentType, response.Content.Headers.ContentType?.MediaType);
        var text = await response.Content.ReadAsStringAsync(Ct);
        var body = await ProblemResponse.ReadAsync(response, Ct);
        Assert.Equal(400, body["status"]?.GetValue<int>());
        Assert.DoesNotContain("Exception", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("   at ", text, StringComparison.Ordinal);
        Assert.Null(TestBrowser.FindSetCookie(response, TestBrowser.AuthCookie));
    }

    /// <summary>El 401 genérico del login tampoco depende de <c>Accept</c>.</summary>
    [Fact]
    public async Task Login_fallido_con_Accept_text_html_responde_401_generico()
    {
        using var browser = new TestBrowser(api.Factory);
        var token = await browser.FetchAntiforgeryTokenAsync(Ct);
        using var content = JsonContent.Create(new { email = "nadie@empresa-sintetica-99.test", password = "Clave-Incorrecta-2026" });

        using var response = await browser.PostAsync("/api/auth/login", content, token, accept: "text/html", Ct);

        await ProblemResponse.AssertAsync(response, HttpStatusCode.Unauthorized, GenericTitle, Ct);
        Assert.Null(TestBrowser.FindSetCookie(response, TestBrowser.AuthCookie));
    }

    private static async Task AssertFieldErrorAsync(TestBrowser browser, string email, string password, string field)
    {
        using var response = await browser.LoginAsync(email, password, Ct);

        var body = await ProblemResponse.AssertAsync(response, HttpStatusCode.BadRequest, ValidationTitle, Ct);
        var errors = body["errors"]?.AsObject();
        Assert.NotNull(errors);
        Assert.True(errors.ContainsKey(field), $"Se esperaba error en '{field}': {body.ToJsonString()}");
        Assert.Null(body["code"]);
        Assert.Null(TestBrowser.FindSetCookie(response, TestBrowser.AuthCookie));
    }

    // Correo sintáctico de la longitud pedida, con etiquetas de dominio de 63 caracteres como máximo.
    private static string Email(int length)
    {
        const string localPart = "usuario";
        const string topLevel = ".test";
        var domainLength = length - localPart.Length - 1 - topLevel.Length;
        var labels = new List<string>();
        while (domainLength > 0)
        {
            var size = Math.Min(63, domainLength);
            labels.Add(new string('d', size));
            domainLength -= size + 1;
        }

        var email = $"{localPart}@{string.Join('.', labels)}{topLevel}";
        return email.Length == length
            ? email
            : throw new InvalidOperationException($"Correo de prueba de {email.Length} caracteres; se pedían {length}.");
    }
}
