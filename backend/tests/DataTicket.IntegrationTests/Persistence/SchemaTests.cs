using DataTicket.Infrastructure.Persistence;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DataTicket.IntegrationTests.Persistence;

/// <summary>
/// La migración inicial produce el esquema de <c>db.sql</c> §2–§3 y §12 (nombres, restricciones, índices,
/// valores por defecto y roles de referencia).
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class SchemaTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Migracion_crea_las_tablas_de_identity_y_dataticket()
    {
        var tables = await QueryListAsync(
            api.Database.ConnectionString,
            "SELECT table_schema || '.' || table_name FROM information_schema.tables WHERE table_schema IN ('identity', 'dataticket')");

        string[] expected =
        [
            "dataticket.companies", "identity.data_protection_keys", "identity.role_claims", "identity.roles",
            "identity.user_claims", "identity.user_logins", "identity.user_roles", "identity.user_tokens", "identity.users",
        ];
        Assert.Equal(expected.Order(StringComparer.Ordinal), tables.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Migracion_crea_restricciones_e_indices_con_los_nombres_de_db_sql()
    {
        var constraints = await QueryListAsync(
            api.Database.ConnectionString,
            "SELECT conname FROM pg_constraint c JOIN pg_namespace n ON n.oid = c.connamespace WHERE n.nspname IN ('identity', 'dataticket')");
        var indexes = await QueryListAsync(
            api.Database.ConnectionString,
            "SELECT indexname || ':' || indexdef FROM pg_indexes WHERE schemaname IN ('identity', 'dataticket')");

        foreach (var name in new[]
        {
            "pk_companies", "ck_companies_name_trimmed", "ck_companies_deactivation",
            "pk_users", "ux_users_id_company", "fk_users_company", "ck_users_account_status", "ck_users_display_name",
            "ck_users_access_failed_count", "pk_roles", "pk_user_roles", "fk_user_roles_user", "fk_user_roles_role",
            "pk_user_claims", "fk_user_claims_user", "pk_role_claims", "fk_role_claims_role", "pk_user_logins",
            "fk_user_logins_user", "pk_user_tokens", "fk_user_tokens_user", "pk_data_protection_keys",
        })
        {
            Assert.Contains(name, constraints);
        }

        Assert.Contains(indexes, index => index.StartsWith("ux_companies_name_ci:", StringComparison.Ordinal)
            && index.Contains("UNIQUE", StringComparison.Ordinal) && index.Contains("lower((name)", StringComparison.Ordinal));
        Assert.Contains(indexes, index => index.StartsWith("ux_users_normalized_email:", StringComparison.Ordinal)
            && index.Contains("UNIQUE", StringComparison.Ordinal) && index.Contains("WHERE (normalized_email IS NOT NULL)", StringComparison.Ordinal));
        Assert.Contains(indexes, index => index.StartsWith("ix_users_company_id:", StringComparison.Ordinal)
            && index.Contains("WHERE (company_id IS NOT NULL)", StringComparison.Ordinal));
        foreach (var name in new[] { "ux_users_normalized_user_name", "ux_roles_normalized_name", "ix_user_roles_role_id",
                     "ix_user_claims_user_id", "ix_role_claims_role_id", "ix_user_logins_user_id" })
        {
            Assert.Contains(indexes, index => index.StartsWith(name + ":", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task Migracion_fija_los_valores_por_defecto_de_db_sql()
    {
        var defaults = await QueryListAsync(
            api.Database.ConnectionString,
            """
            SELECT table_name || '.' || column_name || '=' || column_default FROM information_schema.columns
            WHERE table_schema IN ('identity', 'dataticket') AND column_default IS NOT NULL
            """);

        foreach (var expected in new[]
        {
            "users.id=uuidv7()", "users.email_confirmed=false", "users.phone_number_confirmed=false",
            "users.two_factor_enabled=false", "users.lockout_enabled=true", "users.access_failed_count=0",
            "users.account_status='Invited'::character varying", "users.created_at=now()", "roles.id=uuidv7()",
            "companies.id=uuidv7()", "companies.is_active=true", "companies.created_at=now()", "companies.updated_at=now()",
        })
        {
            Assert.Contains(expected, defaults);
        }
    }

    [Fact]
    public async Task Migracion_siembra_los_seis_roles_de_referencia()
    {
        var roles = await QueryListAsync(api.Database.ConnectionString, "SELECT normalized_name FROM identity.roles");

        Assert.Equal(
            new[] { "ADMINISTRATOR", "COMPANYCOORDINATOR", "DEVELOPMENT", "PRODUCTION", "PRODUCTMANAGER", "REQUESTER" },
            roles.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Development_aplica_las_migraciones_al_arrancar()
    {
        await using var database = await api.Postgres.CreateDatabaseAsync(Ct);
        await using var factory = new DataTicketApiFactory(new ApiFactoryOptions
        {
            ConnectionString = database.ConnectionString,
            ApplyMigrationsOnStartup = true,
        });

        _ = factory.Services; // Arranca el host.

        var tables = await QueryListAsync(
            database.ConnectionString,
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'identity'");
        Assert.Contains("users", tables);
    }

    /// <summary>
    /// Fuera de Development el host nunca migra por su cuenta, aunque la configuración lo pida: las
    /// migraciones se aplican por comando o en el despliegue.
    /// </summary>
    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task Fuera_de_Development_no_se_aplican_las_migraciones_al_arrancar(string environment)
    {
        await using var database = await api.Postgres.CreateDatabaseAsync(Ct);
        await using var factory = new DataTicketApiFactory(new ApiFactoryOptions
        {
            ConnectionString = database.ConnectionString,
            Environment = environment,
            ApplyMigrationsOnStartup = true,
        });

        _ = factory.Services; // Arranca el host.

        var configuration = factory.Services.GetRequiredService<IConfiguration>();
        Assert.True(configuration.GetValue<bool>(DatabaseMigrations.ApplyOnStartupKey));
        var tables = await QueryListAsync(
            database.ConnectionString,
            "SELECT table_schema || '.' || table_name FROM information_schema.tables WHERE table_schema IN ('identity', 'dataticket', 'public')");
        Assert.DoesNotContain("identity.users", tables);
        Assert.Empty(tables);
    }

    private static async Task<List<string>> QueryListAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(Ct);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(Ct);
        var values = new List<string>();
        while (await reader.ReadAsync(Ct))
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }
}
