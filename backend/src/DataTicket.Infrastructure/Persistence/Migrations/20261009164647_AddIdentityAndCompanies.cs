using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataTicket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityAndCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dataticket");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "dataticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deactivated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                    table.CheckConstraint("ck_companies_deactivation", "is_active = (deactivated_at IS NULL)");
                    table.CheckConstraint("ck_companies_name_trimmed", "name = btrim(name) AND char_length(name) >= 1");
                },
                comment: "Empresa cliente (Company). Cada ticket pertenece a exactamente una (PRD §12). Se desactiva, no se borra.");

            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(type: "text", nullable: true),
                    xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                },
                comment: "IdentityRole<Guid>: ProductManager, Administrator, Development, Production, Requester, CompanyCoordinator.");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    account_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Invited → Active → Deactivated (HU-005, HU-008)."),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Empresa del usuario cliente (Requester/CompanyCoordinator); NULL para internos. Origen del claim company_id."),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_users_access_failed_count", "access_failed_count >= 0");
                    table.CheckConstraint("ck_users_account_status", "account_status IN ('Invited', 'Active', 'Deactivated')");
                    table.CheckConstraint("ck_users_display_name", "display_name = btrim(display_name) AND char_length(display_name) >= 1");
                    table.ForeignKey(
                        name: "fk_users_company",
                        column: x => x.company_id,
                        principalSchema: "dataticket",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "ApplicationUser. Sin registro público: Data Global crea, invita y desactiva cuentas (PRD §5).");

            migrationBuilder.CreateTable(
                name: "role_claims",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_claims_role",
                        column: x => x.role_id,
                        principalSchema: "identity",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_claims",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_claims_user",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_logins",
                schema: "identity",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    provider_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_user_logins_user",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "identity",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_roles_role",
                        column: x => x.role_id,
                        principalSchema: "identity",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_user",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tokens",
                schema: "identity",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_user_tokens_user",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "roles",
                columns: new[] { "id", "concurrency_stamp", "name", "normalized_name" },
                values: new object[,]
                {
                    { new Guid("a8000000-0000-7000-8000-000000000001"), "a8000000-0000-7000-8000-000000000001", "ProductManager", "PRODUCTMANAGER" },
                    { new Guid("a8000000-0000-7000-8000-000000000002"), "a8000000-0000-7000-8000-000000000002", "Administrator", "ADMINISTRATOR" },
                    { new Guid("a8000000-0000-7000-8000-000000000003"), "a8000000-0000-7000-8000-000000000003", "Development", "DEVELOPMENT" },
                    { new Guid("a8000000-0000-7000-8000-000000000004"), "a8000000-0000-7000-8000-000000000004", "Production", "PRODUCTION" },
                    { new Guid("a8000000-0000-7000-8000-000000000005"), "a8000000-0000-7000-8000-000000000005", "Requester", "REQUESTER" },
                    { new Guid("a8000000-0000-7000-8000-000000000006"), "a8000000-0000-7000-8000-000000000006", "CompanyCoordinator", "COMPANYCOORDINATOR" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_role_claims_role_id",
                schema: "identity",
                table: "role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ux_roles_normalized_name",
                schema: "identity",
                table: "roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_claims_user_id",
                schema: "identity",
                table: "user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_user_id",
                schema: "identity",
                table: "user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                schema: "identity",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_company_id",
                schema: "identity",
                table: "users",
                column: "company_id",
                filter: "company_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_users_normalized_email",
                schema: "identity",
                table: "users",
                column: "normalized_email",
                unique: true,
                filter: "normalized_email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_users_normalized_user_name",
                schema: "identity",
                table: "users",
                column: "normalized_user_name",
                unique: true);

            // Elementos de db.sql que EF Core no modela sin cambiar su semántica (revisado contra db.sql §2–§3):
            // - DEFAULT de columnas que la aplicación siempre escribe: en el modelo, un bool con DEFAULT true
            //   haría que EF omitiera el valor false al insertar.
            // - ux_users_id_company: UNIQUE sobre una columna nullable (no admite clave alternativa en EF);
            //   es el destino de la FK compuesta tickets(requester_id, company_id) (invariante 1).
            // - ux_companies_name_ci: índice de expresión lower(name).
            migrationBuilder.Sql("""
                ALTER TABLE identity.users
                    ALTER COLUMN email_confirmed SET DEFAULT false,
                    ALTER COLUMN phone_number_confirmed SET DEFAULT false,
                    ALTER COLUMN two_factor_enabled SET DEFAULT false,
                    ALTER COLUMN lockout_enabled SET DEFAULT true,
                    ALTER COLUMN access_failed_count SET DEFAULT 0,
                    ALTER COLUMN account_status SET DEFAULT 'Invited';

                ALTER TABLE identity.users ADD CONSTRAINT ux_users_id_company UNIQUE (id, company_id);

                ALTER TABLE dataticket.companies ALTER COLUMN is_active SET DEFAULT true;

                CREATE UNIQUE INDEX ux_companies_name_ci ON dataticket.companies (lower(name));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_protection_keys",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "role_claims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_claims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_logins",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_tokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "dataticket");
        }
    }
}
