using DataTicket.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataTicket.Infrastructure.Persistence.Configurations;

/// <summary>
/// <c>identity.roles</c> (<c>db.sql</c> §3) con los seis roles de referencia de §12. Son catálogo de toda
/// instalación (también producción), así que se siembran con <c>HasData</c> en la migración: IDs y sellos
/// fijos para que el modelo sea estable entre migraciones (en <c>db.sql</c> son aleatorios; las referencias
/// se hacen por <c>normalized_name</c>).
/// </summary>
internal sealed class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    // Prefijo a8… reservado para roles (db.sql §13 usa a0…a7 para otras entidades).
    private static readonly (Guid Id, string Name)[] ReferenceRoles =
    [
        (Guid.Parse("a8000000-0000-7000-8000-000000000001"), RoleNames.ProductManager),
        (Guid.Parse("a8000000-0000-7000-8000-000000000002"), RoleNames.Administrator),
        (Guid.Parse("a8000000-0000-7000-8000-000000000003"), RoleNames.Development),
        (Guid.Parse("a8000000-0000-7000-8000-000000000004"), RoleNames.Production),
        (Guid.Parse("a8000000-0000-7000-8000-000000000005"), RoleNames.Requester),
        (Guid.Parse("a8000000-0000-7000-8000-000000000006"), RoleNames.CompanyCoordinator),
    ];

    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder.ToTable("roles", Schemas.Identity, table => table.HasComment(
            "IdentityRole<Guid>: ProductManager, Administrator, Development, Production, Requester, CompanyCoordinator."));

        builder.HasKey(role => role.Id).HasName("pk_roles");
        builder.Property(role => role.Id).HasDefaultValueSql("uuidv7()");
        builder.HasIndex(role => role.NormalizedName).HasDatabaseName("ux_roles_normalized_name").IsUnique();

        builder.HasMany<IdentityUserRole<Guid>>().WithOne().HasForeignKey(userRole => userRole.RoleId)
            .HasConstraintName("fk_user_roles_role");
        builder.HasMany<IdentityRoleClaim<Guid>>().WithOne().HasForeignKey(claim => claim.RoleId)
            .HasConstraintName("fk_role_claims_role");

        builder.HasData(ReferenceRoles.Select(role => new IdentityRole<Guid>
        {
            Id = role.Id,
            Name = role.Name,
            NormalizedName = role.Name.ToUpperInvariant(),
            ConcurrencyStamp = role.Id.ToString(),
        }));
    }
}
