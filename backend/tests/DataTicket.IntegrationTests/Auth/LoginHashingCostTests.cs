using System.Net;
using DataTicket.Infrastructure.Identity;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>
/// HU-003 CHU-03/CHU-04/CHU-05 (PRD §5: no revelar si la cuenta existe): todo intento de login cuesta
/// exactamente UNA verificación de hash, exista o no la cuenta, tenga o no contraseña y esté o no bloqueada.
/// Se cuenta con un espía del hasher; no se miden tiempos. Clase propia porque bloquea una cuenta.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class LoginHashingCostTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private const string UnknownEmail = "nadie@empresa-sintetica-99.test";
    private const string WrongPassword = "Clave-Incorrecta-2026";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public Task Login_con_correo_inexistente_verifica_exactamente_un_hash() =>
        AssertSingleHashVerificationAsync(UnknownEmail, TestAccounts.SyntheticPassword, HttpStatusCode.Unauthorized);

    [Fact]
    public Task Login_de_cuenta_Invited_sin_clave_verifica_exactamente_un_hash() =>
        AssertSingleHashVerificationAsync(
            TestAccounts.Company01Invited.Email, TestAccounts.SyntheticPassword, HttpStatusCode.Unauthorized);

    [Fact]
    public async Task Login_de_cuenta_bloqueada_verifica_exactamente_un_hash()
    {
        var account = TestAccounts.Company02Requester2;
        var locked = await api.WithServicesAsync(async services =>
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(account.Id.ToString());
            return await userManager.SetLockoutEndDateAsync(user!, api.Time.GetUtcNow() + IdentitySettings.LockoutDuration);
        });
        Assert.True(locked.Succeeded, string.Join("; ", locked.Errors.Select(error => error.Description)));

        // Con la clave CORRECTA: la cuenta está bloqueada, así que se compara contra el señuelo.
        await AssertSingleHashVerificationAsync(account.Email, TestAccounts.SyntheticPassword, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public Task Login_con_clave_incorrecta_verifica_exactamente_un_hash() =>
        AssertSingleHashVerificationAsync(TestAccounts.Company01Requester2.Email, WrongPassword, HttpStatusCode.Unauthorized);

    [Fact]
    public Task Login_valido_verifica_exactamente_un_hash() =>
        AssertSingleHashVerificationAsync(TestAccounts.AnaPerez.Email, TestAccounts.SyntheticPassword, HttpStatusCode.NoContent);

    /// <summary>
    /// El hash señuelo debe salir del hasher de la aplicación (mismas <c>PasswordHasherOptions</c> que los
    /// hashes reales): si se generara con otro, su coste divergiría al cambiar las iteraciones.
    /// </summary>
    [Fact]
    public async Task Hash_senuelo_se_genera_con_el_hasher_de_la_aplicacion()
    {
        var spy = new PasswordHasherSpy();
        await using var host = spy.Attach(api.Factory);
        using var browser = new TestBrowser(host);
        var token = await browser.FetchAntiforgeryTokenAsync(Ct);

        using var response = await browser.PostJsonAsync(
            "/api/auth/login", new { email = UnknownEmail, password = WrongPassword }, token, Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var decoy = Assert.Single(spy.VerifiedHashes);
        Assert.True(spy.Produced(decoy), "El hash señuelo no lo generó el IPasswordHasher registrado en DI.");
    }

    private async Task AssertSingleHashVerificationAsync(string email, string password, HttpStatusCode expectedStatus)
    {
        var spy = new PasswordHasherSpy();
        await using var host = spy.Attach(api.Factory);
        using var browser = new TestBrowser(host);
        var token = await browser.FetchAntiforgeryTokenAsync(Ct);
        spy.ResetVerifications();

        using var response = await browser.PostJsonAsync("/api/auth/login", new { email, password }, token, Ct);

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Single(spy.VerifiedHashes);
    }
}
