using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.Infrastructure.Persistence;

/// <summary>
/// Aplicación de migraciones de EF Core. Solo se invoca al arrancar en Development (ver <c>Program.cs</c>);
/// en otros entornos las migraciones se aplican por comando o en el despliegue.
/// </summary>
public static class DatabaseMigrations
{
    /// <summary>Clave de configuración que activa la migración al arrancar (solo se honra en Development).</summary>
    public const string ApplyOnStartupKey = "Persistence:ApplyMigrationsOnStartup";

    public static async Task ApplyAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
