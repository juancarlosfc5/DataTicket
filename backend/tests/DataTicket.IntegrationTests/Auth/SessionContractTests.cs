using System.Net;
using DataTicket.Infrastructure.Identity;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>
/// Parámetros de la sesión del contrato T-01 (HU-003): los valores se escriben aquí como literales, a
/// propósito, para que cambiar una constante del código obligue a revisar el contrato.
/// Clase propia porque avanza el reloj de su fixture.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class SessionContractTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void Parametros_de_Identity_y_de_la_cookie_son_los_del_contrato()
    {
        var services = api.Factory.Services;
        var identity = services.GetRequiredService<IOptions<IdentityOptions>>().Value;
        var securityStamp = services.GetRequiredService<IOptions<SecurityStampValidatorOptions>>().Value;
        var cookie = services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);

        // P-18: contraseña y bloqueo.
        Assert.Equal(12, identity.Password.RequiredLength);
        Assert.Equal(5, identity.Lockout.MaxFailedAccessAttempts);
        Assert.Equal(TimeSpan.FromMinutes(15), identity.Lockout.DefaultLockoutTimeSpan);
        Assert.True(identity.Lockout.AllowedForNewUsers);
        Assert.True(identity.User.RequireUniqueEmail);

        // P-13: revalidación del sello de seguridad.
        Assert.Equal(TimeSpan.FromMinutes(5), securityStamp.ValidationInterval);

        // P-22: cookie de sesión.
        Assert.Equal("dataticket.auth", cookie.Cookie.Name);
        Assert.Equal(TimeSpan.FromHours(8), cookie.ExpireTimeSpan);
        Assert.True(cookie.SlidingExpiration);
    }

    [Fact]
    public async Task Politica_de_contrasenas_exige_12_caracteres()
    {
        // Cumplen el resto de reglas de Identity (mayúscula, minúscula, dígito y símbolo): solo cambia la longitud.
        const string elevenCharacters = "Abcdefgh-12";
        const string twelveCharacters = "Abcdefghi-12";

        var (tooShort, longEnough) = await api.WithServicesAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            return (
                await ValidateAsync(userManager, elevenCharacters),
                await ValidateAsync(userManager, twelveCharacters));
        });

        Assert.Contains("PasswordTooShort", tooShort);
        Assert.Empty(longEnough);
    }

    [Fact]
    public async Task Sesion_expira_tras_8_horas_sin_actividad_y_la_actividad_la_renueva()
    {
        // Dos navegadores con la MISMA cookie recién emitida: uno queda inactivo y el otro sigue en uso.
        using var idle = new TestBrowser(api.Factory);
        using var login = await idle.LoginAsync(TestAccounts.Laura.Email, TestAccounts.SyntheticPassword, Ct);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        using var active = new TestBrowser(api.Factory);
        active.SetCookie(TestBrowser.AuthCookie, idle.Cookies[TestBrowser.AuthCookie]);

        // A las 7 h 59 min la sesión sigue viva y la petición la renueva (expiración deslizante).
        api.Time.Advance(TimeSpan.FromHours(8) - TimeSpan.FromMinutes(1));
        using var beforeExpiry = await active.GetAsync(TestEndpoints.Authenticated, Ct);
        Assert.Equal(HttpStatusCode.OK, beforeExpiry.StatusCode);
        Assert.NotNull(TestBrowser.FindSetCookie(beforeExpiry, TestBrowser.AuthCookie));

        // A las 8 h 1 min del login: la cookie sin actividad expiró; la renovada sigue valiendo.
        api.Time.Advance(TimeSpan.FromMinutes(2));
        using var idleResponse = await idle.GetAsync(TestEndpoints.Authenticated, Ct);
        await ProblemResponse.AssertAsync(idleResponse, HttpStatusCode.Unauthorized, "Se requiere iniciar sesión.", Ct);
        using var activeResponse = await active.GetAsync(TestEndpoints.Authenticated, Ct);
        Assert.Equal(HttpStatusCode.OK, activeResponse.StatusCode);
    }

    private static async Task<List<string>> ValidateAsync(UserManager<ApplicationUser> userManager, string password)
    {
        var codes = new List<string>();
        foreach (var validator in userManager.PasswordValidators)
        {
            var result = await validator.ValidateAsync(userManager, new ApplicationUser(), password);
            codes.AddRange(result.Errors.Select(error => error.Code));
        }

        return codes;
    }
}
