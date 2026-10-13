using DataTicket.Domain.Users;

namespace DataTicket.UnitTests.Domain;

public sealed class AccountAccessTests
{
    private static readonly Guid CompanyId = Guid.Parse("c0000000-0000-7000-8000-000000000001");

    [Theory]
    [InlineData(RoleNames.Requester)]
    [InlineData(RoleNames.CompanyCoordinator)]
    public void Cliente_con_empresa_es_superficie_cliente(string role)
    {
        Assert.Equal(Surface.Client, AccountAccess.ResolveSurface([role], CompanyId));
    }

    [Theory]
    [InlineData(RoleNames.ProductManager)]
    [InlineData(RoleNames.Administrator)]
    [InlineData(RoleNames.Development)]
    [InlineData(RoleNames.Production)]
    public void Interno_sin_empresa_es_superficie_interna(string role)
    {
        Assert.Equal(Surface.Internal, AccountAccess.ResolveSurface([role], null));
    }

    [Fact]
    public void Elizabeth_ProductManager_y_Production_es_interna()
    {
        Assert.Equal(
            Surface.Internal,
            AccountAccess.ResolveSurface([RoleNames.ProductManager, RoleNames.Production], null));
    }

    [Fact]
    public void Cliente_sin_empresa_falla_cerrado()
    {
        Assert.Null(AccountAccess.ResolveSurface([RoleNames.Requester], null));
        Assert.Null(AccountAccess.ResolveSurface([RoleNames.Requester], Guid.Empty));
    }

    [Fact]
    public void Interno_con_empresa_no_es_valido()
    {
        Assert.Null(AccountAccess.ResolveSurface([RoleNames.Development], CompanyId));
    }

    [Fact]
    public void Perfil_mixto_no_es_valido()
    {
        Assert.Null(AccountAccess.ResolveSurface([RoleNames.Requester, RoleNames.Administrator], CompanyId));
        Assert.Null(AccountAccess.ResolveSurface([RoleNames.Requester, RoleNames.Administrator], null));
    }

    [Fact]
    public void Sin_roles_o_con_roles_desconocidos_no_es_valido()
    {
        Assert.Null(AccountAccess.ResolveSurface([], null));
        Assert.Null(AccountAccess.ResolveSurface(["Desconocido"], null));
    }

    [Theory]
    [InlineData(AccountStatus.Invited, false)]
    [InlineData(AccountStatus.Active, true)]
    [InlineData(AccountStatus.Deactivated, false)]
    public void Solo_una_cuenta_activa_con_perfil_valido_puede_tener_sesion(AccountStatus status, bool expected)
    {
        Assert.Equal(expected, AccountAccess.CanSignIn(status, [RoleNames.Requester], CompanyId));
    }

    [Fact]
    public void Cuenta_activa_con_perfil_invalido_no_puede_tener_sesion()
    {
        Assert.False(AccountAccess.CanSignIn(AccountStatus.Active, [RoleNames.Requester], null));
    }
}
