using System.Net;
using DataTicket.Infrastructure.Identity;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>
/// HU-004 («Frescura de claims»): los roles viajan en la cookie, así que un cambio de rol llega a la sesión
/// cuando el sello de seguridad se revalida (cada 5 minutos, contrato T-01), sin cerrar la sesión.
/// Clase propia porque avanza el reloj de su fixture y cambia roles.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class ClaimsFreshnessTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private const string Me = "/api/auth/me";
    private const string Administrator = "Administrator";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Cambio_de_rol_llega_a_la_sesion_al_revalidar_el_sello()
    {
        var kevin = TestAccounts.Kevin;
        using var browser = new TestBrowser(api.Factory);
        using var login = await browser.LoginAsync(kevin.Email, TestAccounts.SyntheticPassword, Ct);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
        Assert.Empty(await RolesAsync(browser));
        using var deniedAtFirst = await browser.GetAsync(TestEndpoints.PolicyProbe(Administrator), Ct);
        Assert.Equal(HttpStatusCode.Forbidden, deniedAtFirst.StatusCode);

        // Un administrador le concede el rol Administrator (HU-009 lo hará desde la aplicación).
        var granted = await api.WithServicesAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(kevin.Id.ToString());
            return await userManager.AddToRoleAsync(user!, Administrator);
        });
        Assert.True(granted.Succeeded);

        // Antes del intervalo la cookie sigue con los claims anteriores (desfase acotado a 5 minutos).
        api.Time.Advance(TimeSpan.FromMinutes(4));
        Assert.Empty(await RolesAsync(browser));

        // Pasado el intervalo, la revalidación regenera los claims desde la cuenta.
        api.Time.Advance(TimeSpan.FromMinutes(2));
        Assert.Equal([Administrator], await RolesAsync(browser));
        using var allowedAfterwards = await browser.GetAsync(TestEndpoints.PolicyProbe(Administrator), Ct);
        Assert.Equal(HttpStatusCode.NoContent, allowedAfterwards.StatusCode);
    }

    private static async Task<List<string>> RolesAsync(TestBrowser browser)
    {
        using var response = await browser.GetAsync(Me, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ProblemResponse.ReadAsync(response, Ct);
        return [.. body["roles"]!.AsArray().Select(role => role!.GetValue<string>())];
    }
}
