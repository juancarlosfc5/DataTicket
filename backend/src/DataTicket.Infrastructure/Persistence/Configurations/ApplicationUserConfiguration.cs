using DataTicket.Domain.Companies;
using DataTicket.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataTicket.Infrastructure.Persistence.Configurations;

/// <summary>
/// <c>identity.users</c> (<c>db.sql</c> §3). Se aplica después del modelo de Identity y lo ajusta: nombres de
/// tabla, restricciones e índices de <c>db.sql</c>, columnas propias y FK a <c>dataticket.companies</c>.
/// La restricción <c>ux_users_id_company</c> (columna nullable, no modelable como clave alternativa) y los
/// valores por defecto de las columnas que la aplicación siempre escribe se crean con SQL en la migración.
/// </summary>
internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("users", Schemas.Identity, table =>
        {
            table.HasComment(
                "ApplicationUser. Sin registro público: Data Global crea, invita y desactiva cuentas (PRD §5).");
            table.HasCheckConstraint(
                "ck_users_account_status", "account_status IN ('Invited', 'Active', 'Deactivated')");
            table.HasCheckConstraint(
                "ck_users_display_name", "display_name = btrim(display_name) AND char_length(display_name) >= 1");
            table.HasCheckConstraint("ck_users_access_failed_count", "access_failed_count >= 0");
        });

        builder.HasKey(user => user.Id).HasName("pk_users");
        builder.Property(user => user.Id).HasDefaultValueSql("uuidv7()");

        builder.Property(user => user.DisplayName).HasMaxLength(ApplicationUser.DisplayNameMaxLength).IsRequired();
        builder.Property(user => user.AccountStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasComment("Invited → Active → Deactivated (HU-005, HU-008).");
        builder.Property(user => user.CompanyId).HasComment(
            "Empresa del usuario cliente (Requester/CompanyCoordinator); NULL para internos. Origen del claim company_id.");
        builder.Property(user => user.CreatedAt).HasDefaultValueSql("now()");

        // Índices de Identity con los nombres de db.sql; el correo es único (RequireUniqueEmail).
        builder.HasIndex(user => user.NormalizedUserName).HasDatabaseName("ux_users_normalized_user_name").IsUnique();
        builder.HasIndex(user => user.NormalizedEmail)
            .HasDatabaseName("ux_users_normalized_email")
            .IsUnique()
            .HasFilter("normalized_email IS NOT NULL");
        builder.HasIndex(user => user.CompanyId)
            .HasDatabaseName("ix_users_company_id")
            .HasFilter("company_id IS NOT NULL");

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(user => user.CompanyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_users_company");

        builder.HasMany<IdentityUserClaim<Guid>>().WithOne().HasForeignKey(claim => claim.UserId)
            .HasConstraintName("fk_user_claims_user");
        builder.HasMany<IdentityUserLogin<Guid>>().WithOne().HasForeignKey(login => login.UserId)
            .HasConstraintName("fk_user_logins_user");
        builder.HasMany<IdentityUserToken<Guid>>().WithOne().HasForeignKey(token => token.UserId)
            .HasConstraintName("fk_user_tokens_user");
        builder.HasMany<IdentityUserRole<Guid>>().WithOne().HasForeignKey(userRole => userRole.UserId)
            .HasConstraintName("fk_user_roles_user");
    }
}
