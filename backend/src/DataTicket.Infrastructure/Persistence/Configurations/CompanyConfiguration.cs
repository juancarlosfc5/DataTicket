using DataTicket.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataTicket.Infrastructure.Persistence.Configurations;

/// <summary>
/// <c>dataticket.companies</c> (<c>db.sql</c> §2). El índice único <c>ux_companies_name_ci</c> sobre
/// <c>lower(name)</c> y el valor por defecto de <c>is_active</c> se crean con SQL en la migración.
/// </summary>
internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies", Schemas.DataTicket, table =>
        {
            table.HasComment(
                "Empresa cliente (Company). Cada ticket pertenece a exactamente una (PRD §12). Se desactiva, no se borra.");
            table.HasCheckConstraint("ck_companies_name_trimmed", "name = btrim(name) AND char_length(name) >= 1");
            table.HasCheckConstraint("ck_companies_deactivation", "is_active = (deactivated_at IS NULL)");
        });

        builder.HasKey(company => company.Id).HasName("pk_companies");
        builder.Property(company => company.Id).HasDefaultValueSql("uuidv7()");
        builder.Property(company => company.Name).HasMaxLength(Company.NameMaxLength).IsRequired();
        builder.Property(company => company.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(company => company.UpdatedAt).HasDefaultValueSql("now()");
    }
}
