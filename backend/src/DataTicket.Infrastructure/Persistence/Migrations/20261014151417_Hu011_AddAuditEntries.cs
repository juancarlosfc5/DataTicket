using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataTicket.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// HU-011: bitácora de auditoría append-only <c>dataticket.audit_entries</c> (<c>db.sql</c> §11).
    /// </summary>
    /// <remarks>
    /// <para>
    /// La FK <c>fk_audit_entries_ticket</c> (<c>ticket_id → dataticket.tickets (id) ON DELETE RESTRICT</c>) de
    /// <c>db.sql</c> se omite porque la tabla de tickets aún no existe; la añade la migración que la crea
    /// (núcleo de tickets, N1).
    /// </para>
    /// <para>
    /// Append-only en la base: los triggers rechazan UPDATE, DELETE y TRUNCATE con la función
    /// <c>dataticket.reject_mutation</c> (P-10). Hoy la aplicación se conecta con el mismo usuario que aplica
    /// las migraciones y es dueño de las tablas, así que <c>REVOKE UPDATE, DELETE, TRUNCATE</c> no tendría
    /// efecto (y el dueño podría desactivar el trigger): la revocación llegará cuando exista un rol de
    /// aplicación separado del de migraciones (PRD §16.1, fuera del alcance de HU-011).
    /// </para>
    /// </remarks>
    public partial class Hu011_AddAuditEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_entries",
                schema: "dataticket",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    action = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false, comment: "AuditAction (catálogo en wiki auditoria): TicketSubmitted, PriorityOverridden, StatusChanged, TicketAssigned, TicketReassigned, ParticipantAdded, ParticipantRemoved, FormalResponseDelivered, TicketClosed…"),
                    object_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    object_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_value = table.Column<string>(type: "jsonb", nullable: true),
                    new_value = table.Column<string>(type: "jsonb", nullable: true),
                    result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_entries", x => x.id);
                    table.CheckConstraint("ck_audit_entries_action", "char_length(btrim(action)) >= 1");
                    table.CheckConstraint("ck_audit_entries_object_type", "char_length(btrim(object_type)) >= 1");
                    table.CheckConstraint("ck_audit_entries_result", "result IN ('Succeeded', 'Denied', 'Failed')");
                    table.ForeignKey(
                        name: "fk_audit_entries_actor",
                        column: x => x.actor_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "AuditEntry append-only: actor, fecha, acción, objeto, valores anterior/nuevo (jsonb camelCase) y resultado (PRD §12).");

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_actor_occurred",
                schema: "dataticket",
                table: "audit_entries",
                columns: new[] { "actor_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_object",
                schema: "dataticket",
                table: "audit_entries",
                columns: new[] { "object_type", "object_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_ticket_cursor",
                schema: "dataticket",
                table: "audit_entries",
                columns: new[] { "ticket_id", "occurred_at", "id" });

            // Fuera del modelo de EF Core (db.sql §1 y §11):
            // - DEFAULT 'Succeeded' de result: modelado en EF, el valor por defecto del enum no se enviaría.
            // - reject_mutation(): protección append-only (auditoría) e inmutabilidad (versiones de formulario,
            //   HU-045, que la reutilizará). Esta migración la crea, así que su Down la elimina.
            // - Triggers append-only: por fila para UPDATE y DELETE, por sentencia para TRUNCATE.
            // - Sin FK de ticket_id todavía (ver el comentario de la clase).
            migrationBuilder.Sql("""
                ALTER TABLE dataticket.audit_entries ALTER COLUMN result SET DEFAULT 'Succeeded';

                CREATE FUNCTION dataticket.reject_mutation()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    RAISE EXCEPTION '%.% es inmutable: % no está permitido', TG_TABLE_SCHEMA, TG_TABLE_NAME, TG_OP
                        USING ERRCODE = 'insufficient_privilege';
                END
                $$;

                CREATE TRIGGER trg_audit_entries_no_update_delete
                    BEFORE UPDATE OR DELETE ON dataticket.audit_entries
                    FOR EACH ROW EXECUTE FUNCTION dataticket.reject_mutation();

                CREATE TRIGGER trg_audit_entries_no_truncate
                    BEFORE TRUNCATE ON dataticket.audit_entries
                    FOR EACH STATEMENT EXECUTE FUNCTION dataticket.reject_mutation();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS trg_audit_entries_no_truncate ON dataticket.audit_entries;
                DROP TRIGGER IF EXISTS trg_audit_entries_no_update_delete ON dataticket.audit_entries;
                """);

            migrationBuilder.DropTable(
                name: "audit_entries",
                schema: "dataticket");

            // La función la creó esta migración. DROP FUNCTION es RESTRICT por defecto: si otra tabla aún la usara,
            // fallaría en vez de llevarse sus triggers.
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dataticket.reject_mutation();");
        }
    }
}
