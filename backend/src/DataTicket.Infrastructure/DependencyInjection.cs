using DataTicket.Application.Ports.Out;
using DataTicket.Infrastructure.Identity;
using DataTicket.Infrastructure.Persistence;
using DataTicket.Infrastructure.Seeding;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "DataTicket";
    public const string ReadinessTag = "ready";

    /// <summary>
    /// Registra los adaptadores secundarios (implementaciones de los puertos de salida).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Falta la cadena de conexión 'ConnectionStrings:{ConnectionStringName}'.");
        }

        // El historial de migraciones lleva esquema explícito: no puede depender del search_path de la sesión.
        services.AddDbContext<DataTicketDbContext>(options => options
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.MigrationsHistory))
            .UseSnakeCaseNamingConvention());

        services.AddHealthChecks()
            .AddDbContextCheck<DataTicketDbContext>("postgresql", tags: [ReadinessTag]);

        services.AddIdentityStores();

        services.AddScoped<ICompanyDirectory, CompanyDirectory>();

        // Sembrador de Development: se registra siempre, pero solo actúa en Development y con Seed:Enabled.
        services.AddOptions<DevelopmentSeedOptions>().Bind(configuration.GetSection(DevelopmentSeedOptions.SectionName));
        services.AddScoped<DevelopmentSeeder>();

        return services;
    }

    /// <summary>
    /// ASP.NET Core Identity sobre <see cref="DataTicketDbContext"/> (sin esquemas de autenticación: la cookie
    /// la configura el Api) y claves de Data Protection en PostgreSQL.
    /// </summary>
    private static void AddIdentityStores(this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Esquema de Identity v1: sin passkeys; longitudes de db.sql en las configuraciones.
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version1;
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = IdentitySettings.PasswordMinLength;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = IdentitySettings.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = IdentitySettings.LockoutDuration;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<DataTicketDbContext>()
            .AddUserManager<DataTicketUserManager>()
            .AddSignInManager<DataTicketSignInManager>()
            .AddClaimsPrincipalFactory<DataTicketClaimsPrincipalFactory>()
            .AddDefaultTokenProviders();

        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = IdentitySettings.SecurityStampValidationInterval);

        services.AddDataProtection()
            .PersistKeysToDbContext<DataTicketDbContext>()
            .SetApplicationName(IdentitySettings.DataProtectionApplicationName);

        services.AddSingleton<DecoyPasswordHash>();
        services.AddScoped<ICredentialVerifier, IdentityCredentialVerifier>();
        services.AddScoped<IUserSession, IdentityUserSession>();
    }
}
