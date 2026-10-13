using System.Security.Claims;
using DataTicket.Api.Auth;
using DataTicket.Domain.Users;
using DataTicket.Infrastructure.Identity;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>
/// HU-004 T-03 y CHU-07: el adaptador de <c>ICurrentUser</c> lee solo los claims del principal y falla
/// cerrado. No necesita host ni Docker.
/// </summary>
public sealed class CurrentUserAdapterTests
{
    private static readonly Guid CompanyA = TestCompanies.Company01.Id;
    private static readonly Guid CompanyB = TestCompanies.Company02.Id;

    [Fact]
    public void Principal_de_cliente_con_company_id_expone_su_empresa()
    {
        var currentUser = For(TestPrincipals.Create([RoleNames.Requester], CompanyA));

        Assert.True(currentUser.IsAuthenticated);
        Assert.Equal(TestPrincipals.UserId, currentUser.UserId);
        Assert.Equal("Usuario Sintético", currentUser.DisplayName);
        Assert.Equal("usuario@empresa-sintetica-01.test", currentUser.Email);
        Assert.Equal(Surface.Client, currentUser.Surface);
        Assert.Equal([RoleNames.Requester], currentUser.Roles);
        Assert.Empty(currentUser.Teams);
        Assert.Equal(CompanyA, currentUser.CompanyId);
        Assert.True(currentUser.IsInRole(RoleNames.Requester));
        Assert.False(currentUser.IsInRole(RoleNames.CompanyCoordinator));
    }

    [Fact]
    public void Principal_de_cliente_sin_company_id_no_inventa_empresa()
    {
        var currentUser = For(TestPrincipals.Create([RoleNames.Requester]));

        // Sigue autenticado (la respuesta es 403, no 401), pero sin superficie, roles ni empresa.
        Assert.True(currentUser.IsAuthenticated);
        Assert.Null(currentUser.CompanyId);
        Assert.Null(currentUser.Surface);
        Assert.Empty(currentUser.Roles);
        Assert.False(currentUser.IsInRole(RoleNames.Requester));
    }

    [Fact]
    public void Principal_interno_no_tiene_empresa_y_separa_roles_de_equipos()
    {
        var currentUser = For(TestPrincipals.Create([RoleNames.ProductManager, RoleNames.Production]));

        Assert.Equal(Surface.Internal, currentUser.Surface);
        Assert.Equal([RoleNames.ProductManager], currentUser.Roles);
        Assert.Equal([RoleNames.Production], currentUser.Teams);
        Assert.Null(currentUser.CompanyId);
        Assert.True(currentUser.IsInRole(RoleNames.ProductManager));
        // Un equipo no es un rol: no concede lo que concede un rol.
        Assert.False(currentUser.IsInRole(RoleNames.Production));
    }

    [Fact]
    public void Principal_interno_con_company_id_falla_cerrado()
    {
        var currentUser = For(TestPrincipals.Create([RoleNames.Administrator], CompanyA));

        Assert.Null(currentUser.Surface);
        Assert.Null(currentUser.CompanyId);
        Assert.Empty(currentUser.Roles);
        Assert.False(currentUser.IsInRole(RoleNames.Administrator));
    }

    [Fact]
    public void Perfil_mixto_de_cliente_e_interno_falla_cerrado()
    {
        var currentUser = For(TestPrincipals.Create([RoleNames.Requester, RoleNames.Administrator], CompanyA));

        Assert.Null(currentUser.Surface);
        Assert.Null(currentUser.CompanyId);
        Assert.Empty(currentUser.Roles);
        Assert.Empty(currentUser.Teams);
    }

    [Fact]
    public void Company_id_ambiguo_o_mal_formado_falla_cerrado()
    {
        var twoCompanies = For(TestPrincipals.Create(
            [RoleNames.Requester], CompanyA, new Claim(DataTicketClaimTypes.CompanyId, CompanyB.ToString())));
        var malformed = For(TestPrincipals.Create(
            [RoleNames.Requester], companyId: null, new Claim(DataTicketClaimTypes.CompanyId, "no-es-un-guid")));
        var empty = For(TestPrincipals.Create([RoleNames.Requester], Guid.Empty));

        foreach (var currentUser in new[] { twoCompanies, malformed, empty })
        {
            Assert.Null(currentUser.CompanyId);
            Assert.Null(currentUser.Surface);
            Assert.Empty(currentUser.Roles);
        }
    }

    [Fact]
    public void Roles_desconocidos_se_ignoran()
    {
        var currentUser = For(TestPrincipals.Create([RoleNames.Development, "Root"]));

        Assert.Equal(Surface.Internal, currentUser.Surface);
        Assert.Empty(currentUser.Roles);
        Assert.Equal([RoleNames.Development], currentUser.Teams);
        Assert.False(currentUser.IsInRole("Root"));
    }

    [Fact]
    public void Sin_sesion_no_hay_usuario()
    {
        var anonymous = For(TestPrincipals.Anonymous);
        var withoutHttpContext = new HttpContextCurrentUser(new HttpContextAccessor());

        foreach (var currentUser in new[] { anonymous, withoutHttpContext })
        {
            Assert.False(currentUser.IsAuthenticated);
            Assert.Null(currentUser.UserId);
            Assert.Null(currentUser.DisplayName);
            Assert.Null(currentUser.Email);
            Assert.Null(currentUser.Surface);
            Assert.Empty(currentUser.Roles);
            Assert.Empty(currentUser.Teams);
            Assert.Null(currentUser.CompanyId);
            Assert.False(currentUser.IsInRole(RoleNames.Administrator));
        }
    }

    [Fact]
    public void Sesion_sin_identificador_valido_no_cuenta_como_autenticada()
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "no-es-un-guid"), new Claim(ClaimTypes.Role, RoleNames.Administrator)],
            IdentityConstants.ApplicationScheme);

        var currentUser = For(new ClaimsPrincipal(identity));

        Assert.False(currentUser.IsAuthenticated);
        Assert.Null(currentUser.UserId);
        Assert.Empty(currentUser.Roles);
    }

    private static HttpContextCurrentUser For(ClaimsPrincipal principal) =>
        new(new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = principal } });
}
