using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataTicket.Infrastructure.Persistence.Configurations;

// Tablas auxiliares de Identity y de Data Protection en el esquema identity (db.sql §3). Las FK se nombran
// desde ApplicationUserConfiguration e IdentityRoleConfiguration; los índices de FK salen de la convención
// snake_case (ix_<tabla>_<columna>), que coincide con db.sql.

internal sealed class IdentityUserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
    {
        builder.ToTable("user_roles", Schemas.Identity);
        builder.HasKey(userRole => new { userRole.UserId, userRole.RoleId }).HasName("pk_user_roles");
    }
}

internal sealed class IdentityUserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder)
    {
        builder.ToTable("user_claims", Schemas.Identity);
        builder.HasKey(claim => claim.Id).HasName("pk_user_claims");
    }
}

internal sealed class IdentityRoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        builder.ToTable("role_claims", Schemas.Identity);
        builder.HasKey(claim => claim.Id).HasName("pk_role_claims");
    }
}

/// <summary>Inicio de sesión externo: fuera del MVP (PRD §13); se conserva por compatibilidad con Identity.</summary>
internal sealed class IdentityUserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
    {
        builder.ToTable("user_logins", Schemas.Identity);
        builder.HasKey(login => new { login.LoginProvider, login.ProviderKey }).HasName("pk_user_logins");
        builder.Property(login => login.LoginProvider).HasMaxLength(128);
        builder.Property(login => login.ProviderKey).HasMaxLength(128);
    }
}

internal sealed class IdentityUserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
    {
        builder.ToTable("user_tokens", Schemas.Identity);
        builder.HasKey(token => new { token.UserId, token.LoginProvider, token.Name }).HasName("pk_user_tokens");
        builder.Property(token => token.LoginProvider).HasMaxLength(128);
        builder.Property(token => token.Name).HasMaxLength(128);
    }
}

/// <summary>Claves de Data Protection (<c>PersistKeysToDbContext</c>): las cookies sobreviven reinicios.</summary>
internal sealed class DataProtectionKeyConfiguration : IEntityTypeConfiguration<DataProtectionKey>
{
    public void Configure(EntityTypeBuilder<DataProtectionKey> builder)
    {
        builder.ToTable("data_protection_keys", Schemas.Identity);
        builder.HasKey(key => key.Id).HasName("pk_data_protection_keys");
    }
}
