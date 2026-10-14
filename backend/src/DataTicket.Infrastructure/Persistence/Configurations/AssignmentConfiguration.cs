using DataTicket.Domain.Tickets;
using DataTicket.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataTicket.Infrastructure.Persistence.Configurations;

/// <summary>
/// <c>dataticket.assignments</c> (<c>db.sql</c> §6). La FK al ticket la declara <see cref="TicketConfiguration"/>.
/// Las FK <c>fk_assignments_assigned_by</c> y <c>fk_assignments_unassigned_by</c> van con SQL en la migración: en
/// EF Core crearían índices que <c>db.sql</c> no tiene.
/// </summary>
internal sealed class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("assignments", Schemas.DataTicket, table =>
        {
            table.HasComment(
                "Assignment. Vigente = unassigned_at IS NULL; la reasignación cierra la anterior y abre otra.");
            table.HasCheckConstraint(
                "ck_assignments_unassignment",
                "(unassigned_at IS NULL) = (unassigned_by IS NULL) AND (unassigned_at IS NULL OR unassigned_at >= assigned_at)");
        });

        builder.HasKey(assignment => assignment.Id).HasName("pk_assignments");
        // Se crea desde el agregado (ver TicketParticipantConfiguration).
        builder.Property(assignment => assignment.Id).HasDefaultValueSql("uuidv7()").ValueGeneratedNever();
        builder.Property(assignment => assignment.AssignedAt).HasDefaultValueSql("now()");
        builder.Ignore(assignment => assignment.IsActive);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(assignment => assignment.AssigneeId)
            .HasConstraintName("fk_assignments_assignee")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(assignment => new { assignment.TicketId, assignment.AssigneeId })
            .IsUnique()
            .HasFilter("unassigned_at IS NULL")
            .HasDatabaseName("ux_assignments_active");
        // Carga por responsable y tickets sin asignar (HU-040).
        builder.HasIndex(assignment => assignment.AssigneeId)
            .HasFilter("unassigned_at IS NULL")
            .HasDatabaseName("ix_assignments_assignee_active");
        builder.HasIndex(assignment => assignment.TicketId).HasDatabaseName("ix_assignments_ticket_id");
    }
}
