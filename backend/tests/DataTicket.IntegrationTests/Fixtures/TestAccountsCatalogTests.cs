namespace DataTicket.IntegrationTests.Fixtures;

/// <summary>
/// El catálogo de cuentas replica <c>db.sql</c> §13 y cubre los estados y roles que piden HU-003 y HU-004.
/// No necesita Docker.
/// </summary>
public sealed class TestAccountsCatalogTests
{
    [Fact]
    public void Catalogo_replica_ids_correos_y_nombres_de_db_sql()
    {
        Assert.Equal(Guid.Parse("a0000000-0000-7000-8000-000000000001"), TestAccounts.Elizabeth.Id);
        Assert.Equal("elizabeth@dataticket.local", TestAccounts.Elizabeth.Email);
        Assert.Equal(Guid.Parse("a0000000-0000-7000-8000-000000000009"), TestAccounts.Admin.Id);
        Assert.Equal("Administrador DataTicket", TestAccounts.Admin.DisplayName);

        Assert.Equal(Guid.Parse("b0000000-0000-7000-8000-000001000001"), TestAccounts.AnaPerez.Id);
        Assert.Equal("ana.perez@empresa-sintetica-01.test", TestAccounts.AnaPerez.Email);
        Assert.Equal("Ana Pérez", TestAccounts.AnaPerez.DisplayName);
        Assert.Equal(Guid.Parse("c0000000-0000-7000-8000-000000000001"), TestAccounts.AnaPerez.CompanyId);

        Assert.Equal(Guid.Parse("b0000000-0000-7000-8000-000002000003"), TestAccounts.Company02Coordinator.Id);
        Assert.Equal("coordinador@empresa-sintetica-02.test", TestAccounts.Company02Coordinator.Email);
        Assert.Equal("Coordinador Empresa 02", TestAccounts.Company02Coordinator.DisplayName);

        Assert.Equal("invitado@empresa-sintetica-01.test", TestAccounts.Company01Invited.Email);
        Assert.Equal("Desactivado Empresa 02", TestAccounts.Company02Deactivated.DisplayName);
        Assert.Equal("Empresa Sintética 02", TestCompanies.Company02.Name);
    }

    [Fact]
    public void Catalogo_cubre_los_tres_estados_y_los_seis_roles()
    {
        Assert.Equal(
            Enum.GetValues<TestAccountStatus>().Order(),
            TestAccounts.All.Select(account => account.Status).Distinct().Order());

        Assert.Equal(
            TestRoles.All.Order(StringComparer.Ordinal),
            TestAccounts.All.SelectMany(account => account.Roles).Distinct().Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Elizabeth_es_ProductManager_y_de_Produccion()
    {
        Assert.Equal([TestRoles.ProductManager, TestRoles.Production], TestAccounts.Elizabeth.Roles);
    }

    [Fact]
    public void Clientes_tienen_empresa_y_rol_de_cliente_e_internos_no()
    {
        string[] clientRoles = [TestRoles.Requester, TestRoles.CompanyCoordinator];

        Assert.All(TestAccounts.Clients, account =>
        {
            Assert.NotNull(account.CompanyId);
            Assert.All(account.Roles, role => Assert.Contains(role, clientRoles));
        });
        Assert.All(TestAccounts.Internals, account =>
        {
            Assert.Null(account.CompanyId);
            Assert.Equal(TestAccountStatus.Active, account.Status);
            Assert.DoesNotContain(account.Roles, role => clientRoles.Contains(role));
        });
    }

    [Fact]
    public void Empresas_A_y_B_tienen_cuentas_en_cada_estado()
    {
        foreach (var company in TestCompanies.All)
        {
            var statuses = TestAccounts.Clients
                .Where(account => account.CompanyId == company.Id)
                .Select(account => account.Status)
                .Distinct();

            Assert.Equal(Enum.GetValues<TestAccountStatus>().Order(), statuses.Order());
        }
    }

    [Fact]
    public void Ids_y_correos_son_unicos()
    {
        Assert.Equal(TestAccounts.All.Count, TestAccounts.All.Select(account => account.Id).Distinct().Count());
        Assert.Equal(
            TestAccounts.All.Count,
            TestAccounts.All.Select(account => account.Email).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Solo_las_cuentas_Invited_carecen_de_contrasena()
    {
        Assert.All(TestAccounts.All, account =>
            Assert.Equal(account.Status != TestAccountStatus.Invited, account.HasPassword));
    }
}
