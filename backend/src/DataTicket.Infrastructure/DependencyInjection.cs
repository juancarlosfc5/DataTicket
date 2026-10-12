using DataTicket.Application.Ports.Out;
using DataTicket.Infrastructure.Identity;
using DataTicket.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        services.AddDbContext<DataTicketDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddHealthChecks()
            .AddDbContextCheck<DataTicketDbContext>("postgresql", tags: [ReadinessTag]);

        services.AddIdentityStores();

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
