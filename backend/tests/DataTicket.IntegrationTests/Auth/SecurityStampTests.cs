using System.Net;
using DataTicket.Domain.Users;
using DataTicket.Infrastructure.Identity;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>
/// Contrato T-01 (P-13): la cookie se revalida cada 5 minutos y una cuenta que ya no está <c>Active</c>
/// pierde la sesión. Clase propia porque avanza el reloj de su fixture.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class SecurityStampTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Cuenta_desactivada_pierde_la_sesion_al_revalidar_el_sello()
    {
        var account = TestAccounts.Company02Requester1;
        using var browser = new TestBrowser(api.Factory);
        using var login = await browser.LoginAsync(account.Email, TestAccounts.SyntheticPassword, Ct);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        // Data Global desactiva la cuenta sin tocar el sello de seguridad.
        await api.WithServicesAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(account.Id.ToString());
            user!.AccountStatus = AccountStatus.Deactivated;
            return await userManager.UpdateAsync(user);
        });

        // Antes del intervalo de revalidación la cookie sigue valiendo (riesgo acotado a 5 minutos).
        api.Time.Advance(TimeSpan.FromMinutes(4));
        using var beforeInterval = await browser.GetAsync(TestEndpoints.Authenticated, Ct);
        Assert.Equal(HttpStatusCode.OK, beforeInterval.StatusCode);

        // Pasado el intervalo, la revalidación rechaza la cuenta y la sesión se pierde.
        api.Time.Advance(TimeSpan.FromMinutes(2));
        using var afterInterval = await browser.GetAsync(TestEndpoints.Authenticated, Ct);
        await ProblemResponse.AssertAsync(afterInterval, HttpStatusCode.Unauthorized, "Se requiere iniciar sesión.", Ct);
    }

    [Fact]
    public async Task Sesion_activa_sigue_valida_tras_la_revalidacion()
    {
        using var browser = new TestBrowser(api.Factory);
        using var login = await browser.LoginAsync(TestAccounts.Gerardo.Email, TestAccounts.SyntheticPassword, Ct);
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        api.Time.Advance(TimeSpan.FromMinutes(6));
        using var response = await browser.GetAsync(TestEndpoints.Authenticated, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
