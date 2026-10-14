using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataTicket.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Núcleo N1 (adelanto de HU-013, HU-018 y HU-019): <c>dataticket.categories</c>, <c>tickets</c>,
    /// <c>ticket_participants</c> y <c>assignments</c> (<c>db.sql</c> §1, §4, §6 y §12), y la FK
    /// <c>fk_audit_entries_ticket</c> que HU-011 dejó pendiente.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Con SQL, fuera del modelo de EF Core (revisado contra <c>db.sql</c>): las funciones <c>priority_matrix</c> y
    /// <c>next_ticket_number</c> con la secuencia <c>ticket_number_seq</c> (antes de crear <c>tickets</c>, porque su
    /// CHECK y su DEFAULT las usan); los DEFAULT de <c>tickets.number</c>, <c>tickets.status</c> y
    /// <c>categories.is_active</c>; la FK compuesta <c>fk_tickets_requester_company</c> (destino UNIQUE con columna
    /// nullable); las FK de auditoría de personas (<c>closed_by</c>, <c>added_by</c>, <c>removed_by</c>,
    /// <c>assigned_by</c>, <c>unassigned_by</c>), que en EF Core crearían índices que <c>db.sql</c> no tiene; el
    /// índice de expresión <c>ux_categories_name_ci</c>; y la semilla de referencia de categorías (V-12).
    /// </para>
    /// <para>
    /// Se omiten <c>form_template_version_id</c>, <c>custom_field_values</c>, su CHECK, su FK y sus índices: son de
    /// la Fase 4 (HU-045 a HU-047) y su tabla destino, <c>form_template_versions</c>, aún no existe.
    /// </para>
    /// </remarks>
    public partial class HuN1_NucleoTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // db.sql §1: funciones de dominio que usan el CHECK de la prioridad y el DEFAULT del número.
            migrationBuilder.Sql("""
                CREATE FUNCTION dataticket.priority_matrix(p_urgency varchar, p_impact varchar)
                RETURNS varchar
                LANGUAGE sql
                IMMUTABLE
                STRICT
                PARALLEL SAFE
                AS $$
                    SELECT CASE p_impact || ':' || p_urgency
                        WHEN 'Low:Low'       THEN 'VeryLow'
                        WHEN 'Low:Medium'    THEN 'Low'
                        WHEN 'Low:High'      THEN 'Medium'
                        WHEN 'Medium:Low'    THEN 'Low'
                        WHEN 'Medium:Medium' THEN 'Medium'
                        WHEN 'Medium:High'   THEN 'High'
                        WHEN 'High:Low'      THEN 'Medium'
                        WHEN 'High:Medium'   THEN 'High'
                        WHEN 'High:High'     THEN 'Critical'
                    END
                $$;

                COMMENT ON FUNCTION dataticket.priority_matrix(varchar, varchar)
                    IS 'CalculatedPriority = matriz(Urgency, Impact) según PRD §6.1.';

                CREATE SEQUENCE dataticket.ticket_number_seq AS bigint START WITH 1 INCREMENT BY 1;

                CREATE FUNCTION dataticket.next_ticket_number()
                RETURNS varchar
                LANGUAGE plpgsql
                VOLATILE
                AS $$
                DECLARE
                    v_next bigint := nextval('dataticket.ticket_number_seq');
                BEGIN
                    RETURN 'DT-' || lpad(v_next::text, greatest(6, length(v_next::text)), '0');
                END
                $$;

                COMMENT ON FUNCTION dataticket.next_ticket_number()
                    IS 'Genera el número visible del ticket (formato provisional, V-16).';
                """);

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "dataticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                    table.CheckConstraint("ck_categories_name", "name = btrim(name) AND char_length(name) >= 1");
                },
                comment: "Category del formulario común (PRD §6.1). Catálogo no definido en el PRD (V-12).");

            migrationBuilder.CreateTable(
                name: "tickets",
                schema: "dataticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    number = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false, comment: "Número visible generado en servidor (formato provisional, V-16). Si el agregado lo necesita antes de guardar, el adaptador ejecuta SELECT dataticket.next_ticket_number()."),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    urgency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    impact = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    calculated_priority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "CalculatedPriority derivada de la matriz; la base de datos verifica la coherencia."),
                    current_priority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Prioridad vigente: igual a la calculada salvo PriorityOverride (HU-015, propuesta)."),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "TicketStatus interno. ClientStatus se deriva, no se guarda (nunca se expone al cliente)."),
                    pull_request_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true, comment: "PullRequestUrl manual e interna; nunca visible al cliente (PRD §5, §6.3)."),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tickets", x => x.id);
                    table.UniqueConstraint("ux_tickets_number", x => x.number);
                    table.CheckConstraint("ck_tickets_calculated_priority", "calculated_priority = dataticket.priority_matrix(urgency, impact)");
                    table.CheckConstraint("ck_tickets_closure", "(status = 'Closed') = (closed_at IS NOT NULL) AND (closed_at IS NULL) = (closed_by IS NULL)");
                    table.CheckConstraint("ck_tickets_current_priority", "current_priority IN ('VeryLow', 'Low', 'Medium', 'High', 'Critical')");
                    table.CheckConstraint("ck_tickets_description", "char_length(btrim(description)) BETWEEN 1 AND 10000");
                    table.CheckConstraint("ck_tickets_impact", "impact IN ('Low', 'Medium', 'High')");
                    table.CheckConstraint("ck_tickets_pull_request_url", "pull_request_url IS NULL OR pull_request_url ~ '^https://'");
                    table.CheckConstraint("ck_tickets_status", "status IN ('New', 'InDevelopment', 'PullRequestReview', 'InProduction', 'SolutionDelivered', 'Closed')");
                    table.CheckConstraint("ck_tickets_title", "title = btrim(title) AND char_length(title) BETWEEN 1 AND 120");
                    table.CheckConstraint("ck_tickets_urgency", "urgency IN ('Low', 'Medium', 'High')");
                    table.ForeignKey(
                        name: "fk_tickets_category",
                        column: x => x.category_id,
                        principalSchema: "dataticket",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tickets_company",
                        column: x => x.company_id,
                        principalSchema: "dataticket",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Ticket (raíz de agregado). company_id y requester_id salen de la cuenta autenticada y no cambian (PRD §6.1).");

            migrationBuilder.CreateTable(
                name: "assignments",
                schema: "dataticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    unassigned_by = table.Column<Guid>(type: "uuid", nullable: true),
                    unassigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignments", x => x.id);
                    table.CheckConstraint("ck_assignments_unassignment", "(unassigned_at IS NULL) = (unassigned_by IS NULL) AND (unassigned_at IS NULL OR unassigned_at >= assigned_at)");
                    table.ForeignKey(
                        name: "fk_assignments_assignee",
                        column: x => x.assignee_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assignments_ticket",
                        column: x => x.ticket_id,
                        principalSchema: "dataticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Assignment. Vigente = unassigned_at IS NULL; la reasignación cierra la anterior y abre otra.");

            migrationBuilder.CreateTable(
                name: "ticket_participants",
                schema: "dataticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    added_by = table.Column<Guid>(type: "uuid", nullable: false),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    removed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    removed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_participants", x => x.id);
                    table.CheckConstraint("ck_ticket_participants_origin", "origin IN ('Assignment', 'Participant', 'CoverageTake')");
                    table.CheckConstraint("ck_ticket_participants_removal", "(removed_at IS NULL) = (removed_by IS NULL) AND (removed_at IS NULL OR removed_at >= added_at)");
                    table.ForeignKey(
                        name: "fk_ticket_participants_ticket",
                        column: x => x.ticket_id,
                        principalSchema: "dataticket",
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ticket_participants_user",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "TicketParticipant. Vigente = removed_at IS NULL. Origen: Assignment, Participant o CoverageTake (HU-017/018).");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_assignee_active",
                schema: "dataticket",
                table: "assignments",
                column: "assignee_id",
                filter: "unassigned_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_ticket_id",
                schema: "dataticket",
                table: "assignments",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ux_assignments_active",
                schema: "dataticket",
                table: "assignments",
                columns: new[] { "ticket_id", "assignee_id" },
                unique: true,
                filter: "unassigned_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_participants_ticket_id",
                schema: "dataticket",
                table: "ticket_participants",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_participants_user_removed",
                schema: "dataticket",
                table: "ticket_participants",
                columns: new[] { "user_id", "removed_at" });

            migrationBuilder.CreateIndex(
                name: "ux_ticket_participants_active",
                schema: "dataticket",
                table: "ticket_participants",
                columns: new[] { "ticket_id", "user_id" },
                unique: true,
                filter: "removed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_category_id",
                schema: "dataticket",
                table: "tickets",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_company_created",
                schema: "dataticket",
                table: "tickets",
                columns: new[] { "company_id", "created_at", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_company_requester_created",
                schema: "dataticket",
                table: "tickets",
                columns: new[] { "company_id", "requester_id", "created_at", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_created_at",
                schema: "dataticket",
                table: "tickets",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_requester_id",
                schema: "dataticket",
                table: "tickets",
                column: "requester_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_status_created",
                schema: "dataticket",
                table: "tickets",
                columns: new[] { "status", "created_at", "id" });

            migrationBuilder.AddForeignKey(
                name: "fk_audit_entries_ticket",
                schema: "dataticket",
                table: "audit_entries",
                column: "ticket_id",
                principalSchema: "dataticket",
                principalTable: "tickets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Elementos de db.sql §4, §6 y §12 que EF Core no modela sin cambiar su semántica (ver el comentario de
            // la clase).
            migrationBuilder.Sql("""
                ALTER TABLE dataticket.categories ALTER COLUMN is_active SET DEFAULT true;
                CREATE UNIQUE INDEX ux_categories_name_ci ON dataticket.categories (lower(name));

                ALTER TABLE dataticket.tickets
                    ALTER COLUMN number SET DEFAULT dataticket.next_ticket_number(),
                    ALTER COLUMN status SET DEFAULT 'New';

                -- El solicitante pertenece a la empresa del ticket; además impide mover de empresa a un usuario que
                -- ya tiene tickets (PRD §5, invariante 1, HU-008). Destino: ux_users_id_company.
                ALTER TABLE dataticket.tickets
                    ADD CONSTRAINT fk_tickets_requester_company FOREIGN KEY (requester_id, company_id)
                        REFERENCES identity.users (id, company_id) ON DELETE RESTRICT ON UPDATE RESTRICT,
                    ADD CONSTRAINT fk_tickets_closed_by FOREIGN KEY (closed_by)
                        REFERENCES identity.users (id) ON DELETE RESTRICT;

                ALTER TABLE dataticket.ticket_participants
                    ADD CONSTRAINT fk_ticket_participants_added_by FOREIGN KEY (added_by)
                        REFERENCES identity.users (id) ON DELETE RESTRICT,
                    ADD CONSTRAINT fk_ticket_participants_removed_by FOREIGN KEY (removed_by)
                        REFERENCES identity.users (id) ON DELETE RESTRICT;

                ALTER TABLE dataticket.assignments
                    ADD CONSTRAINT fk_assignments_assigned_by FOREIGN KEY (assigned_by)
                        REFERENCES identity.users (id) ON DELETE RESTRICT,
                    ADD CONSTRAINT fk_assignments_unassigned_by FOREIGN KEY (unassigned_by)
                        REFERENCES identity.users (id) ON DELETE RESTRICT;
                """);

            // db.sql §12: semilla provisional de categorías (V-12). Es dato de referencia de toda instalación; se
            // cambia como datos, sin tocar código.
            migrationBuilder.Sql("""
                INSERT INTO dataticket.categories (name, sort_order) VALUES
                    ('Incidente',     1),
                    ('Requerimiento', 2),
                    ('Consulta',      3);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_audit_entries_ticket",
                schema: "dataticket",
                table: "audit_entries");

            migrationBuilder.DropTable(
                name: "assignments",
                schema: "dataticket");

            migrationBuilder.DropTable(
                name: "ticket_participants",
                schema: "dataticket");

            migrationBuilder.DropTable(
                name: "tickets",
                schema: "dataticket");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "dataticket");

            // Las funciones y la secuencia las creó esta migración. DROP FUNCTION es RESTRICT por defecto: si algo
            // aún las usara, fallaría en vez de llevárselo.
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS dataticket.next_ticket_number();
                DROP SEQUENCE IF EXISTS dataticket.ticket_number_seq;
                DROP FUNCTION IF EXISTS dataticket.priority_matrix(varchar, varchar);
                """);
        }
    }
}
