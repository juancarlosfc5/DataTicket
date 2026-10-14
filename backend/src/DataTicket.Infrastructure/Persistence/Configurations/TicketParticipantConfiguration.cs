using DataTicket.Domain.Tickets;
using DataTicket.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataTicket.Infrastructure.Persistence.Configurations;

/// <summary>
/// <c>dataticket.ticket_participants</c> (<c>db.sql</c> §6). La FK al ticket la declara
/// <see cref="TicketConfiguration"/>. Las FK <c>fk_ticket_participants_added_by</c> y
/// <c>fk_ticket_participants_removed_by</c> van con SQL en la migración: en EF Core crearían índices sobre
/// <c>added_by</c> y <c>removed_by</c> que <c>db.sql</c> no tiene.
/// </summary>
internal sealed class TicketParticipantConfiguration : IEntityTypeConfiguration<TicketParticipant>
{
    public void Configure(EntityTypeBuilder<TicketParticipant> builder)
    {
        builder.ToTable("ticket_participants", Schemas.DataTicket, table =>
        {
            table.HasComment(
                "TicketParticipant. Vigente = removed_at IS NULL. Origen: Assignment, Participant o CoverageTake (HU-017/018).");
            table.HasCheckConstraint(
                "ck_ticket_participants_origin", "origin IN ('Assignment', 'Participant', 'CoverageTake')");
            table.HasCheckConstraint(
                "ck_ticket_participants_removal",
                "(removed_at IS NULL) = (removed_by IS NULL) AND (removed_at IS NULL OR removed_at >= added_at)");
        });

        builder.HasKey(participant => participant.Id).HasName("pk_ticket_participants");
        // Se crea desde el agregado: un identificador marcado como generado haría que EF tomara la fila nueva
        // por existente al descubrirla por la navegación (UPDATE en vez de INSERT).
        builder.Property(participant => participant.Id).HasDefaultValueSql("uuidv7()").ValueGeneratedNever();
        builder.Property(participant => participant.Origin).HasConversion<string>().HasMaxLength(20);
        builder.Property(participant => participant.AddedAt).HasDefaultValueSql("now()");
        builder.Ignore(participant => participant.IsActive);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(participant => participant.UserId)
            .HasConstraintName("fk_ticket_participants_user")
            .OnDelete(DeleteBehavior.Restrict);

        // Una sola participación vigente por (ticket, persona) (HU-018); el historial conserva las anteriores.
        builder.HasIndex(participant => new { participant.TicketId, participant.UserId })
            .IsUnique()
            .HasFilter("removed_at IS NULL")
            .HasDatabaseName("ux_ticket_participants_active");
        builder.HasIndex(participant => new { participant.UserId, participant.RemovedAt })
            .HasDatabaseName("ix_ticket_participants_user_removed");
        builder.HasIndex(participant => participant.TicketId).HasDatabaseName("ix_ticket_participants_ticket_id");
    }
}
