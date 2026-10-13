using System.Security.Claims;
using DataTicket.Domain.Users;
using DataTicket.Infrastructure.Identity;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>
/// HU-004 CHU-05 y CHU-08: políticas con nombre evaluadas con el <see cref="IAuthorizationService"/> del
/// composition root real y principales construidos. Los nombres se escriben como literales, a propósito:
/// son los del contrato T-01. No necesita Docker.
/// </summary>
public sealed class AuthorizationPolicyTests(OfflineApiFixture api) : IClassFixture<OfflineApiFixture>
{
    private const string ClientUser = "ClientUser";
    private const string InternalUser = "InternalUser";
    private const string Administrator = "Administrator";
    private const string ProductManagerOrAdministrator = "ProductManagerOrAdministrator";

    private static readonly string[] AllPolicies = [ClientUser, InternalUser, Administrator, ProductManagerOrAdministrator];

    private static readonly Guid CompanyA = TestCompanies.Company01.Id;

    /// <summary>Perfil → políticas que debe satisfacer (todas las demás deben fallar).</summary>
    private static readonly (string Profile, ClaimsPrincipal Principal, string[] Granted)[] Matrix =
    [
        ("Requester", TestPrincipals.Create([RoleNames.Requester], CompanyA), [ClientUser]),
        ("CompanyCoordinator", TestPrincipals.Create([RoleNames.CompanyCoordinator], CompanyA), [ClientUser]),
        ("ProductManager", TestPrincipals.Create([RoleNames.ProductManager]), [InternalUser, ProductManagerOrAdministrator]),
        (
            "ProductManager + equipo Production",
            TestPrincipals.Create([RoleNames.ProductManager, RoleNames.Production]),
            [InternalUser, ProductManagerOrAdministrator]),
        (
            "Administrator",
            TestPrincipals.Create([RoleNames.Administrator]),
            [InternalUser, Administrator, ProductManagerOrAdministrator]),
        ("solo equipo Development", TestPrincipals.Create([RoleNames.Development]), [InternalUser]),
        ("solo equipo Production", TestPrincipals.Create([RoleNames.Production]), [InternalUser]),

        // Fuera de la matriz no hay nada: sin sesión, sin roles o con un perfil no válido.
        ("anónimo", TestPrincipals.Anonymous, []),
        ("autenticado sin roles", TestPrincipals.Create([]), []),
        ("rol desconocido", TestPrincipals.Create(["Root"]), []),
        ("Requester sin company_id", TestPrincipals.Create([RoleNames.Requester]), []),
        ("CompanyCoordinator sin company_id", TestPrincipals.Create([RoleNames.CompanyCoordinator]), []),
        (
            "Requester con company_id mal formado",
            TestPrincipals.Create([RoleNames.Requester], null, new Claim(DataTicketClaimTypes.CompanyId, "no-es-un-guid")),
            []),
        ("mixto Requester + Administrator", TestPrincipals.Create([RoleNames.Requester, RoleNames.Administrator], CompanyA), []),
        ("Administrator con company_id", TestPrincipals.Create([RoleNames.Administrator], CompanyA), []),
        ("equipo Development con company_id", TestPrincipals.Create([RoleNames.Development], CompanyA), []),
    ];

    [Fact]
    public async Task Politicas_por_rol_cumplen_la_matriz()
    {
        var authorization = api.Development.Services.GetRequiredService<IAuthorizationService>();
        var mismatches = new List<string>();

        foreach (var (profile, principal, granted) in Matrix)
        {
            foreach (var policy in AllPolicies)
            {
                var result = await authorization.AuthorizeAsync(principal, resource: null, policy);
                var expected = granted.Contains(policy);
                if (result.Succeeded != expected)
                {
                    mismatches.Add($"{profile} → {policy}: esperado {(expected ? "autoriza" : "deniega")}, obtuvo lo contrario");
                }
            }
        }

        Assert.True(mismatches.Count == 0, string.Join(Environment.NewLine, mismatches));
    }

    [Fact]
    public async Task Politica_de_reserva_exige_usuario_autenticado()
    {
        var services = api.Development.Services;
        var authorization = services.GetRequiredService<IAuthorizationService>();
        var fallback = await services.GetRequiredService<IAuthorizationPolicyProvider>().GetFallbackPolicyAsync();

        Assert.NotNull(fallback);
        Assert.Contains(fallback.Requirements, requirement => requirement is DenyAnonymousAuthorizationRequirement);
        Assert.False((await authorization.AuthorizeAsync(TestPrincipals.Anonymous, fallback)).Succeeded);
        // Cualquier usuario autenticado, también uno sin roles: la política de reserva no mira el perfil.
        Assert.True((await authorization.AuthorizeAsync(TestPrincipals.Create([]), fallback)).Succeeded);
    }

    /// <summary>
    /// CHU-08 (PRD §5 regla 5): ser administrador no convierte a nadie en usuario cliente, y esta HU no
    /// registra ninguna política de contenido de tickets (eso lo decide la participación, en HU posteriores).
    /// Se comprueba contra las políticas REALMENTE registradas en el host, no contra nombres supuestos: son
    /// exactamente las cuatro del contrato, así que no hay ninguna otra que un administrador pueda satisfacer.
    /// </summary>
    [Fact]
    public async Task Administrador_no_obtiene_politicas_de_cliente_ni_de_contenido()
    {
        var administrator = TestPrincipals.Create([RoleNames.Administrator]);
        string[] contract = [.. AllPolicies.Order(StringComparer.Ordinal)];

        foreach (var services in new[] { api.Development.Services, api.Production.Services })
        {
            var authorization = services.GetRequiredService<IAuthorizationService>();
            var registered = RegisteredPolicies.Names(services);

            // Ni una política de más (de contenido o de lo que sea) ni una de menos.
            Assert.Equal(contract, registered);
            // La lista que publica el Api sale de la misma tabla que el registro.
            Assert.Equal(registered, Api.Auth.AuthorizationPolicies.All.Order(StringComparer.Ordinal));

            var granted = new List<string>();
            foreach (var policy in registered)
            {
                if ((await authorization.AuthorizeAsync(administrator, resource: null, policy)).Succeeded)
                {
                    granted.Add(policy);
                }
            }

            // De todo lo registrado, el administrador solo obtiene las de personal interno: nunca ClientUser.
            Assert.Equal([Administrator, InternalUser, ProductManagerOrAdministrator], granted);
            Assert.DoesNotContain(ClientUser, Api.Auth.AuthorizationPolicies.InternalOnly);
        }
    }

    [Fact]
    public async Task Politicas_son_las_mismas_fuera_de_Development()
    {
        var authorization = api.Production.Services.GetRequiredService<IAuthorizationService>();
        var administrator = TestPrincipals.Create([RoleNames.Administrator]);
        var requester = TestPrincipals.Create([RoleNames.Requester], CompanyA);

        Assert.True((await authorization.AuthorizeAsync(administrator, resource: null, Administrator)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(requester, resource: null, Administrator)).Succeeded);
        Assert.True((await authorization.AuthorizeAsync(requester, resource: null, ClientUser)).Succeeded);
        Assert.NotNull(await api.Production.Services.GetRequiredService<IAuthorizationPolicyProvider>().GetFallbackPolicyAsync());
    }
}
