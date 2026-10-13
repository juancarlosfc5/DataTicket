using DataTicket.Domain.Companies;
using DataTicket.Infrastructure.Identity;
using DataTicket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataTicket.Infrastructure.Seeding;

/// <summary>
/// Sembrador SOLO de Development: crea las empresas y cuentas sintéticas de <see cref="DevelopmentSeedCatalog"/>
/// para poder entrar al stack local. Nunca corre en un entorno compartido ni en producción (<c>db.sql</c> §13).
/// </summary>
/// <remarks>
/// <para>
/// Guardas: <see cref="RunIfEnabledAsync"/> solo siembra si <c>Seed:Enabled</c> es <c>true</c> Y el entorno es
/// Development; fuera de Development la clave se ignora con una advertencia. Además, <see cref="SeedAsync"/>
/// se niega a ejecutarse fuera de Development aunque se invoque directamente.
/// </para>
/// <para>
/// Idempotente: una empresa o cuenta que ya existe (por id) se omite sin modificarla, así que un segundo
/// arranque no cambia hashes, sellos de seguridad ni datos editados a mano. Todo ocurre en una transacción:
/// o queda sembrado lo que faltaba, o no queda nada a medias.
/// </para>
/// <para>
/// Las cuentas se crean con <see cref="UserManager{TUser}"/> (validadores de Identity, hash con
/// <c>IPasswordHasher</c>, sello de seguridad aleatorio) y sus roles en <c>identity.user_roles</c>; los roles
/// los siembra la migración. La contraseña sale de <c>Seed:DefaultPassword</c> y nunca se registra en el log.
/// </para>
/// </remarks>
public sealed partial class DevelopmentSeeder(
    DataTicketDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IHostEnvironment environment,
    IOptions<DevelopmentSeedOptions> options,
    ILogger<DevelopmentSeeder> logger)
{
    /// <summary>
    /// Punto de entrada del arranque (tras las migraciones). No hace nada si la siembra no está activada o si
    /// el entorno no es Development.
    /// </summary>
    public static async Task RunIfEnabledAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        if (!provider.GetRequiredService<IOptions<DevelopmentSeedOptions>>().Value.Enabled)
        {
            return;
        }

        var environment = provider.GetRequiredService<IHostEnvironment>();
        if (!environment.IsDevelopment())
        {
            LogIgnoredOutsideDevelopment(
                provider.GetRequiredService<ILogger<DevelopmentSeeder>>(), DevelopmentSeedOptions.EnabledKey, environment.EnvironmentName);
            return;
        }

        await provider.GetRequiredService<DevelopmentSeeder>().SeedAsync(cancellationToken);
    }

    /// <summary>Crea lo que falte del catálogo. Lanza si el entorno no es Development o falta la contraseña.</summary>
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                $"El sembrador de Development solo se ejecuta en el entorno Development (actual: {environment.EnvironmentName}).");
        }

        var password = options.Value.DefaultPassword;
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"Falta '{DevelopmentSeedOptions.DefaultPasswordKey}': con '{DevelopmentSeedOptions.EnabledKey}' activado, el "
                + "sembrador de Development necesita la contraseña sintética de las cuentas (valor local de ejemplo en "
                + "appsettings.Development.json).");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var companies = await SeedCompaniesAsync(cancellationToken);
        var accounts = await SeedAccountsAsync(password, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        LogSeeded(
            logger,
            companies,
            DevelopmentSeedCatalog.Companies.Count - companies,
            accounts,
            DevelopmentSeedCatalog.Accounts.Count - accounts);
    }

    private async Task<int> SeedCompaniesAsync(CancellationToken cancellationToken)
    {
        var created = 0;
        foreach (var company in DevelopmentSeedCatalog.Companies)
        {
            if (await dbContext.Companies.AnyAsync(existing => existing.Id == company.Id, cancellationToken))
            {
                continue;
            }

            dbContext.Companies.Add(new Company(company.Id, company.Name, company.CreatedAt));
            created++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return created;
    }

    private async Task<int> SeedAccountsAsync(string password, CancellationToken cancellationToken)
    {
        var created = 0;
        foreach (var account in DevelopmentSeedCatalog.Accounts)
        {
            if (await dbContext.Users.AnyAsync(existing => existing.Id == account.Id, cancellationToken))
            {
                continue;
            }

            var user = new ApplicationUser
            {
                Id = account.Id,
                UserName = account.Email,
                Email = account.Email,
                EmailConfirmed = account.HasPassword,
                DisplayName = account.DisplayName,
                AccountStatus = account.Status,
                CompanyId = account.CompanyId,
                CreatedAt = account.CreatedAt,
            };

            // CreateAsync(user, password) valida la contraseña con la política de Identity y guarda solo su hash.
            EnsureSucceeded(
                account.HasPassword ? await userManager.CreateAsync(user, password) : await userManager.CreateAsync(user),
                account);
            EnsureSucceeded(await userManager.AddToRolesAsync(user, account.Roles), account);
            created++;
        }

        return created;
    }

    /// <summary>Los errores de Identity describen la regla incumplida; nunca incluyen la contraseña.</summary>
    private static void EnsureSucceeded(IdentityResult result, SeedAccount account)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"El sembrador de Development no pudo crear la cuenta {account.Email}: "
                + string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}")));
        }
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "{Key} está activado pero el entorno es {Environment}: el sembrador de Development solo corre en Development. No se sembró nada.")]
    private static partial void LogIgnoredOutsideDevelopment(ILogger logger, string key, string environment);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Sembrador de Development: {CompaniesCreated} empresas creadas ({CompaniesSkipped} ya existían) y {AccountsCreated} cuentas creadas ({AccountsSkipped} ya existían).")]
    private static partial void LogSeeded(
        ILogger logger, int companiesCreated, int companiesSkipped, int accountsCreated, int accountsSkipped);
}
