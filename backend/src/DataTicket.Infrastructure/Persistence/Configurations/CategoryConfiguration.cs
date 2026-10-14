using DataTicket.Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataTicket.Infrastructure.Persistence.Configurations;

/// <summary>
/// <c>dataticket.categories</c> (<c>db.sql</c> §4). En la migración, con SQL: el DEFAULT <c>true</c> de
/// <c>is_active</c>, el índice de expresión <c>ux_categories_name_ci</c> sobre <c>lower(name)</c> y la semilla de
/// referencia de <c>db.sql</c> §12 (V-12).
/// </summary>
internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", Schemas.DataTicket, table =>
        {
            table.HasComment("Category del formulario común (PRD §6.1). Catálogo no definido en el PRD (V-12).");
            table.HasCheckConstraint("ck_categories_name", "name = btrim(name) AND char_length(name) >= 1");
        });

        builder.HasKey(category => category.Id).HasName("pk_categories");
        builder.Property(category => category.Id).HasDefaultValueSql("uuidv7()").ValueGeneratedNever();
        builder.Property(category => category.Name).HasMaxLength(Category.NameMaxLength).IsRequired();
        builder.Property(category => category.SortOrder).HasDefaultValue(0);
        builder.Property(category => category.CreatedAt).HasDefaultValueSql("now()");
    }
}
