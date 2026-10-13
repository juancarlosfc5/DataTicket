using System.Text.Json;
using DataTicket.Domain.Users;
using DataTicket.Infrastructure.Seeding;
using DataTicket.IntegrationTests.Fixtures;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DataTicket.IntegrationTests.Seeding;

/// <summary>
/// Catálogo y configuración del sembrador de Development, sin base de datos. La fuente de verdad de los datos
/// es <c>db.sql</c> §13: el catálogo de producción (cinco cuentas) y el de las pruebas (<c>TestAccounts</c>,
/// completo) la replican por separado y aquí se comparan, sin que <c>src</c> dependa de <c>tests</c>.
/// No necesita Docker.
/// </summary>
public sealed class DevelopmentSeedCatalogTests(OfflineApiFixture api) : IClassFixture<OfflineApiFixture>
{
    [Fact]
    public void Catalogo_del_sembrador_coincide_con_el_de_db_sql()
    {
        foreach (var seeded in DevelopmentSeedCatalog.Accounts)
        {
            var reference = TestAccounts.All.SingleOrDefault(account => account.Id == seeded.Id);
            Assert.True(reference is not null, $"La cuenta {seeded.Id} del sembrador no existe en db.sql §13.");
            Assert.Equal(reference.Email, seeded.Email);
            Assert.Equal(reference.DisplayName, seeded.DisplayName);
            Assert.Equal(reference.Status, seeded.Status);
            Assert.Equal(reference.Roles, seeded.Roles);
            Assert.Equal(reference.CompanyId, seeded.CompanyId);
            Assert.Equal(reference.HasPassword, seeded.HasPassword);
            // Todo perfil sembrado es uno de los «Perfiles válidos» del contrato.
            Assert.NotNull(AccountAccess.ResolveSurface(seeded.Roles, seeded.CompanyId));
        }

        foreach (var seeded in DevelopmentSeedCatalog.Companies)
        {
            var reference = TestCompanies.All.SingleOrDefault(company => company.Id == seeded.Id);
            Assert.True(reference is not null, $"La empresa {seeded.Id} del sembrador no existe en db.sql §13.");
            Assert.Equal(reference.Name, seeded.Name);
            Assert.True(reference.IsActive);
        }
    }

    /// <summary>El subconjunto es el mínimo acordado para B7; el juego completo es de HU-010.</summary>
    [Fact]
    public void Catalogo_del_sembrador_es_el_subconjunto_minimo()
    {
        var accounts = DevelopmentSeedCatalog.Accounts;

        Assert.Equal(5, accounts.Count);
        Assert.Equal(accounts.Count, accounts.Select(account => account.Id).Distinct().Count());
        Assert.Equal(accounts.Count, accounts.Select(account => account.Email).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        Assert.Single(accounts, account => account.Roles.Contains(RoleNames.ProductManager));
        Assert.Single(accounts, account => account.CompanyId is null && !account.Roles.Contains(RoleNames.ProductManager));
        Assert.Single(accounts, account => account is { CompanyId: not null, Status: AccountStatus.Active });
        Assert.Single(accounts, account => account.Status == AccountStatus.Invited);
        Assert.Single(accounts, account => account.Status == AccountStatus.Deactivated);
        Assert.Contains(accounts, account => account.Email == "ana.perez@empresa-sintetica-01.test");
        Assert.DoesNotContain(accounts, account => account.Roles.Contains(RoleNames.Administrator));

        // Solo las empresas que necesitan esas cuentas.
        Assert.Equal(
            accounts.Where(account => account.CompanyId is not null).Select(account => account.CompanyId!.Value).Distinct(),
            DevelopmentSeedCatalog.Companies.Select(company => company.Id));
    }

    /// <summary>
    /// La siembra solo se activa en <c>appsettings.Development.json</c>: ningún otro archivo de configuración
    /// del Api define la sección <c>Seed</c>, y allí la contraseña es un valor local de ejemplo no vacío.
    /// </summary>
    [Fact]
    public void Seed_solo_se_configura_en_appsettings_Development()
    {
        var contentRoot = api.Production.Services.GetRequiredService<IHostEnvironment>().ContentRootPath;
        var files = Directory.GetFiles(contentRoot, "appsettings*.json").Select(Path.GetFileName).Order(StringComparer.Ordinal).ToList();
        Assert.Contains("appsettings.json", files);
        Assert.Contains("appsettings.Development.json", files);

        foreach (var file in files)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(contentRoot, file!)));
            var hasSeed = document.RootElement.TryGetProperty(DevelopmentSeedOptions.SectionName, out var seed);

            if (file != "appsettings.Development.json")
            {
                Assert.False(hasSeed, $"{file} no debe definir la sección Seed: la siembra es solo de Development.");
                continue;
            }

            Assert.True(hasSeed, "appsettings.Development.json debe definir la sección Seed.");
            Assert.True(seed.GetProperty(nameof(DevelopmentSeedOptions.Enabled)).GetBoolean());
            Assert.False(string.IsNullOrWhiteSpace(seed.GetProperty(nameof(DevelopmentSeedOptions.DefaultPassword)).GetString()));
        }
    }
}
