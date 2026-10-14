using DataTicket.Domain.Auditing;
using DataTicket.Domain.Companies;
using DataTicket.Domain.Tickets;
using DataTicket.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataTicket.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core sobre PostgreSQL: Identity y claves de Data Protection en el esquema <c>identity</c>,
/// dominio en <c>dataticket</c> (<c>db.sql</c>, ADR-0009). Columnas en snake_case por convención
/// (<c>UseSnakeCaseNamingConvention</c>); los nombres de restricciones e índices de <c>db.sql</c> se fijan en
/// <c>Persistence/Configurations</c>.
/// </summary>
public sealed class DataTicketDbContext(DbContextOptions<DataTicketDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IDataProtectionKeyContext
{
    public DbSet<Company> Companies => Set<Company>();

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketParticipant> TicketParticipants => Set<TicketParticipant>();

    public DbSet<Assignment> Assignments => Set<Assignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primero el modelo de Identity; después nuestras configuraciones lo ajustan a db.sql.
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataTicketDbContext).Assembly);
    }
}
