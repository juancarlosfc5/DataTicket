using System.Net;
using DataTicket.Infrastructure.Identity;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>
/// HU-003 CHU-05: bloqueo tras 5 fallos durante 15 minutos, medido con el reloj de la aplicación.
/// Clase propia porque avanza el reloj compartido de su fixture.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class LockoutTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private const string GenericTitle = "Correo o contraseña incorrectos.";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Quinto_fallo_bloquea_y_no_lo_revela()
    {
        var account = TestAccounts.AnaPerez;
        using var browser = new TestBrowser(api.Factory);
        using var reference = await browser.LoginAsync("nadie@empresa-sintetica-99.test", "Clave-Incorrecta-2026", Ct);
        var genericBody = await ProblemResponse.ReadWithoutTraceIdAsync(reference, Ct);

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            using var failed = await browser.LoginAsync(account.Email, "Clave-Incorrecta-2026", Ct);
            await ProblemResponse.AssertAsync(failed, HttpStatusCode.Unauthorized, GenericTitle, Ct);
        }

        // Sexto intento con la clave CORRECTA: bloqueada, pero la respuesta es la genérica.
        using var locked = await browser.LoginAsync(account.Email, TestAccounts.SyntheticPassword, Ct);
        await ProblemResponse.AssertAsync(locked, HttpStatusCode.Unauthorized, GenericTitle, Ct);
        Assert.Equal(genericBody, await ProblemResponse.ReadWithoutTraceIdAsync(locked, Ct));
        Assert.Null(TestBrowser.FindSetCookie(locked, TestBrowser.AuthCookie));
        var lockoutEnd = await ReadUserAsync(account.Id, user => user.LockoutEnd);
        Assert.Equal(api.Time.GetUtcNow() + IdentitySettings.LockoutDuration, lockoutEnd);

        // 14 minutos después sigue bloqueada.
        api.Time.Advance(TimeSpan.FromMinutes(14));
        using var stillLocked = await browser.LoginAsync(account.Email, TestAccounts.SyntheticPassword, Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, stillLocked.StatusCode);

        // Pasados los 15 minutos, la clave correcta entra y el contador queda en cero.
        api.Time.Advance(TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1));
        using var unlocked = await browser.LoginAsync(account.Email, TestAccounts.SyntheticPassword, Ct);
        Assert.Equal(HttpStatusCode.NoContent, unlocked.StatusCode);
        Assert.Equal(0, await ReadUserAsync(account.Id, user => user.AccessFailedCount));
    }

    [Fact]
    public async Task Cuatro_fallos_no_bloquean_y_un_acierto_reinicia_el_contador()
    {
        var account = TestAccounts.Company01Requester2;
        using var browser = new TestBrowser(api.Factory);

        for (var attempt = 1; attempt <= 4; attempt++)
        {
            using var failed = await browser.LoginAsync(account.Email, "Clave-Incorrecta-2026", Ct);
            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }

        Assert.Equal(4, await ReadUserAsync(account.Id, user => user.AccessFailedCount));
        using var success = await browser.LoginAsync(account.Email, TestAccounts.SyntheticPassword, Ct);
        Assert.Equal(HttpStatusCode.NoContent, success.StatusCode);
        Assert.Equal(0, await ReadUserAsync(account.Id, user => user.AccessFailedCount));
    }

    private Task<T> ReadUserAsync<T>(Guid userId, Func<ApplicationUser, T> select) =>
        api.WithServicesAsync(async services =>
        {
            var user = await services.GetRequiredService<UserManager<ApplicationUser>>().FindByIdAsync(userId.ToString());
            return select(user!);
        });
}
