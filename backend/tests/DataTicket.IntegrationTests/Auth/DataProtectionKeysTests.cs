using System.Net;
using DataTicket.Infrastructure.Persistence;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>HU-003 CHU-12: la cookie sobrevive al reinicio porque las claves están en PostgreSQL.</summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class DataProtectionKeysTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Cookie_sobrevive_reinicio_por_claves_en_PostgreSQL()
    {
        // Primera instancia del backend: emite la cookie y se apaga.
        string authCookie;
        await using (var first = api.CreateFactory())
        {
            using var browser = new TestBrowser(first);
            using var login = await browser.LoginAsync(TestAccounts.JuanDavid.Email, TestAccounts.SyntheticPassword, Ct);
            Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
            authCookie = browser.Cookies[TestBrowser.AuthCookie];
        }

        // Segunda instancia (memoria limpia) sobre la misma base: acepta la cookie.
        await using var second = api.CreateFactory();
        using var restarted = new TestBrowser(second);
        restarted.SetCookie(TestBrowser.AuthCookie, authCookie);

        using var response = await restarted.GetAsync(TestEndpoints.Authenticated, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(TestAccounts.JuanDavid.Id.ToString(), await response.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        var keys = await api.WithServicesAsync(services =>
            services.GetRequiredService<DataTicketDbContext>().DataProtectionKeys.CountAsync(Ct));
        Assert.True(keys >= 1, "Debe haber al menos una clave de Data Protection en identity.data_protection_keys.");
    }
}
