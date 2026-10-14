using DataTicket.Domain.Auditing;
using DataTicket.Domain.Tickets;
using DataTicket.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataTicket.Infrastructure.Persistence.Configurations;

/// <summary>
/// <c>dataticket.audit_entries</c> (<c>db.sql</c> §11, HU-011). En la migración, con SQL: el DEFAULT
/// <c>'Succeeded'</c> de <c>result</c>, la función <c>reject_mutation</c> y los triggers append-only. La FK
/// <c>ticket_id → tickets</c> la añade la migración que crea <c>dataticket.tickets</c> (núcleo N1).
/// </summary>
internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries", Schemas.DataTicket, table =>
        {
            table.HasComment(
                "AuditEntry append-only: actor, fecha, acción, objeto, valores anterior/nuevo (jsonb camelCase) y resultado (PRD §12).");
            table.HasCheckConstraint("ck_audit_entries_action", "char_length(btrim(action)) >= 1");
            table.HasCheckConstraint("ck_audit_entries_object_type", "char_length(btrim(object_type)) >= 1");
            // V-11: la columna existe desde el inicio aunque aún no se decida si se auditan denegaciones.
            table.HasCheckConstraint("ck_audit_entries_result", "result IN ('Succeeded', 'Denied', 'Failed')");
        });

        builder.HasKey(entry => entry.Id).HasName("pk_audit_entries");
        builder.Property(entry => entry.Id).HasDefaultValueSql("uuidv7()");
        builder.Property(entry => entry.TicketId);
        builder.Property(entry => entry.ActorId);
        builder.Property(entry => entry.OccurredAt).HasDefaultValueSql("now()");
        builder.Property(entry => entry.Action)
            .HasConversion<string>()
            .HasMaxLength(AuditEntry.ActionMaxLength)
            .HasComment(
                "AuditAction (catálogo en wiki auditoria): TicketSubmitted, PriorityOverridden, StatusChanged, TicketAssigned, TicketReassigned, ParticipantAdded, ParticipantRemoved, FormalResponseDelivered, TicketClosed…");
        builder.Property(entry => entry.ObjectType).HasMaxLength(AuditEntry.ObjectTypeMaxLength).IsRequired();
        builder.Property(entry => entry.ObjectId);
        builder.Property(entry => entry.OldValue).HasColumnType("jsonb");
        builder.Property(entry => entry.NewValue).HasColumnType("jsonb");
        // El DEFAULT 'Succeeded' va en la migración con SQL: modelado en EF, el valor por defecto del enum
        // (Succeeded) no se enviaría en el INSERT y EF avisaría del centinela.
        builder.Property(entry => entry.Result)
            .HasConversion<string>()
            .HasMaxLength(AuditEntry.ResultMaxLength);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(entry => entry.ActorId)
            .HasConstraintName("fk_audit_entries_actor")
            .OnDelete(DeleteBehavior.Restrict);

        // La añade la migración del núcleo de tickets (HuN1_NucleoTickets), que crea dataticket.tickets.
        builder.HasOne<Ticket>()
            .WithMany()
            .HasForeignKey(entry => entry.TicketId)
            .HasConstraintName("fk_audit_entries_ticket")
            .OnDelete(DeleteBehavior.Restrict);

        // Bitácora del ticket en orden determinista (HU-012).
        builder.HasIndex(entry => new { entry.TicketId, entry.OccurredAt, entry.Id })
            .HasDatabaseName("ix_audit_entries_ticket_cursor");
        builder.HasIndex(entry => new { entry.ActorId, entry.OccurredAt })
            .HasDatabaseName("ix_audit_entries_actor_occurred");
        // Auditoría de objetos sin ticket (empresas, usuarios, plantillas).
        builder.HasIndex(entry => new { entry.ObjectType, entry.ObjectId, entry.OccurredAt })
            .HasDatabaseName("ix_audit_entries_object");
    }
}
