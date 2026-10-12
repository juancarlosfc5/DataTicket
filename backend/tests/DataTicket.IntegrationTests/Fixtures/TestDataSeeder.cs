using DataTicket.Domain.Companies;
using DataTicket.Infrastructure.Identity;
using DataTicket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Fixtures;

/// <summary>
/// Persiste el catálogo de pruebas (<see cref="TestCompanies"/> y <see cref="TestAccounts"/>) con EF Core y
/// <c>UserManager</c>, como hará el sembrador de Development. Las cuentas <c>Invited</c> quedan sin contraseña.
/// </summary>
public static class TestDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, TimeProvider clock, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();

        foreach (var company in TestCompanies.All)
        {
            dbContext.Companies.Add(new Company(company.Id, company.Name, clock.GetUtcNow()));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Un solo hash (PBKDF2 es caro a propósito); la sal aleatoria no importa para las pruebas.
        var passwordHash = passwordHasher.HashPassword(new ApplicationUser(), TestAccounts.SyntheticPassword);
        foreach (var account in TestAccounts.All)
        {
            await CreateAsync(userManager, account, account.HasPassword ? passwordHash : null, clock.GetUtcNow());
        }
    }

    /// <summary>
    /// Crea una cuenta y le asigna sus roles. Útil también para cuentas ad hoc de una prueba.
    /// </summary>
    public static async Task<ApplicationUser> CreateAsync(
        UserManager<ApplicationUser> userManager, TestAccount account, string? passwordHash, DateTimeOffset createdAt)
    {
        var user = new ApplicationUser
        {
            Id = account.Id,
            UserName = account.Email,
            Email = account.Email,
            EmailConfirmed = account.HasPassword,
            DisplayName = account.DisplayName,
            AccountStatus = account.Status,
            CompanyId = account.CompanyId,
            CreatedAt = createdAt,
            PasswordHash = passwordHash,
        };

        EnsureSucceeded(await userManager.CreateAsync(user), account.Email);
        EnsureSucceeded(await userManager.AddToRolesAsync(user, account.Roles), account.Email);
        return user;
    }

    private static void EnsureSucceeded(IdentityResult result, string email)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"No se pudo sembrar {email}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}
