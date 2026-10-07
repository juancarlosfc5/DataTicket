using Microsoft.EntityFrameworkCore;

namespace DataTicket.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core sobre PostgreSQL. Al incorporar ASP.NET Core Identity pasará a
/// heredar de IdentityDbContext; las entidades de dominio se mapean con IEntityTypeConfiguration.
/// </summary>
public sealed class DataTicketDbContext(DbContextOptions<DataTicketDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataTicketDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
