using DataTicket.Domain.Companies;
using DataTicket.Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataTicket.Infrastructure.Persistence.Configurations;

/// <summary>
/// <c>dataticket.tickets</c> (<c>db.sql</c> §6), raíz del agregado con sus participaciones y asignaciones.
/// </summary>
/// <remarks>
/// En la migración, con SQL (no se modelan en EF Core sin cambiar su semántica):
/// <list type="bullet">
/// <item>Las funciones <c>priority_matrix</c> y <c>next_ticket_number</c> y la secuencia
/// <c>ticket_number_seq</c> (<c>db.sql</c> §1), que usan el CHECK de la prioridad calculada y el DEFAULT de
/// <c>number</c>.</item>
/// <item>Los DEFAULT de <c>number</c> y <c>status</c>: modelados en EF, el valor por defecto del enum
/// (<c>New</c>) no se enviaría en el INSERT.</item>
/// <item><c>fk_tickets_requester_company</c>: FK compuesta hacia <c>identity.users (id, company_id)</c>, cuyo
/// destino es un UNIQUE con columna nullable (no admite clave alternativa en EF).</item>
/// <item><c>fk_tickets_closed_by</c>: en EF Core crearía un índice sobre <c>closed_by</c> que <c>db.sql</c> no
/// tiene.</item>
/// </list>
/// <c>form_template_version_id</c>, <c>custom_field_values</c> y sus índices y CHECK (Fase 4, HU-045 a HU-047)
/// llegarán con la migración que cree <c>form_template_versions</c>.
/// </remarks>
internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    private const string Priorities = "'VeryLow', 'Low', 'Medium', 'High', 'Critical'";

    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets", Schemas.DataTicket, table =>
        {
            table.HasComment(
                "Ticket (raíz de agregado). company_id y requester_id salen de la cuenta autenticada y no cambian (PRD §6.1).");
            table.HasCheckConstraint("ck_tickets_title", "title = btrim(title) AND char_length(title) BETWEEN 1 AND 120");
            table.HasCheckConstraint("ck_tickets_description", "char_length(btrim(description)) BETWEEN 1 AND 10000");
            table.HasCheckConstraint("ck_tickets_urgency", "urgency IN ('Low', 'Medium', 'High')");
            table.HasCheckConstraint("ck_tickets_impact", "impact IN ('Low', 'Medium', 'High')");
            table.HasCheckConstraint(
                "ck_tickets_calculated_priority", "calculated_priority = dataticket.priority_matrix(urgency, impact)");
            table.HasCheckConstraint("ck_tickets_current_priority", $"current_priority IN ({Priorities})");
            table.HasCheckConstraint(
                "ck_tickets_status",
                "status IN ('New', 'InDevelopment', 'PullRequestReview', 'InProduction', 'SolutionDelivered', 'Closed')");
            table.HasCheckConstraint(
                "ck_tickets_pull_request_url", "pull_request_url IS NULL OR pull_request_url ~ '^https://'");
            table.HasCheckConstraint(
                "ck_tickets_closure",
                "(status = 'Closed') = (closed_at IS NOT NULL) AND (closed_at IS NULL) = (closed_by IS NULL)");
        });

        builder.HasKey(ticket => ticket.Id).HasName("pk_tickets");
        // El dominio asigna el identificador (UUID v7): EF no debe tratarlo como generado por la base.
        builder.Property(ticket => ticket.Id).HasDefaultValueSql("uuidv7()").ValueGeneratedNever();
        builder.Property(ticket => ticket.Number)
            .HasMaxLength(Ticket.NumberMaxLength)
            .IsRequired()
            .HasComment(
                "Número visible generado en servidor (formato provisional, V-16). Si el agregado lo necesita antes de guardar, el adaptador ejecuta SELECT dataticket.next_ticket_number().");
        // UNIQUE como restricción (db.sql), no como índice: el número no cambia tras radicar.
        builder.HasAlternateKey(ticket => ticket.Number).HasName("ux_tickets_number");
        builder.Property(ticket => ticket.Title).HasMaxLength(Ticket.TitleMaxLength).IsRequired();
        builder.Property(ticket => ticket.Description).HasColumnType("text").IsRequired();
        builder.Property(ticket => ticket.Urgency).HasConversion<string>().HasMaxLength(10);
        builder.Property(ticket => ticket.Impact).HasConversion<string>().HasMaxLength(10);
        builder.Property(ticket => ticket.CalculatedPriority)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasComment("CalculatedPriority derivada de la matriz; la base de datos verifica la coherencia.");
        builder.Property(ticket => ticket.CurrentPriority)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasComment("Prioridad vigente: igual a la calculada salvo PriorityOverride (HU-015, propuesta).");
        builder.Property(ticket => ticket.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasComment("TicketStatus interno. ClientStatus se deriva, no se guarda (nunca se expone al cliente).");
        builder.Property(ticket => ticket.PullRequestUrl)
            .HasMaxLength(Ticket.PullRequestUrlMaxLength)
            .HasComment("PullRequestUrl manual e interna; nunca visible al cliente (PRD §5, §6.3).");
        builder.Property(ticket => ticket.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(ticket => ticket.UpdatedAt).HasDefaultValueSql("now()");
        builder.Property(ticket => ticket.ClosedAt);
        builder.Property(ticket => ticket.ClosedBy);

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(ticket => ticket.CompanyId)
            .HasConstraintName("fk_tickets_company")
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(ticket => ticket.CategoryId)
            .HasConstraintName("fk_tickets_category")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ticket => ticket.Participants)
            .WithOne()
            .HasForeignKey(participant => participant.TicketId)
            .HasConstraintName("fk_ticket_participants_ticket")
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(ticket => ticket.Participants).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(ticket => ticket.Assignments)
            .WithOne()
            .HasForeignKey(assignment => assignment.TicketId)
            .HasConstraintName("fk_assignments_ticket")
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(ticket => ticket.Assignments).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Portal del solicitante (HU-024) y del coordinador (HU-025): siempre filtrado por empresa.
        builder.HasIndex(ticket => new { ticket.CompanyId, ticket.RequesterId, ticket.CreatedAt, ticket.Id })
            .HasDatabaseName("ix_tickets_company_requester_created");
        builder.HasIndex(ticket => new { ticket.CompanyId, ticket.CreatedAt, ticket.Id })
            .HasDatabaseName("ix_tickets_company_created");
        // Cola de triage y bandejas internas (HU-016, HU-023) y paneles (HU-040).
        builder.HasIndex(ticket => new { ticket.Status, ticket.CreatedAt, ticket.Id })
            .HasDatabaseName("ix_tickets_status_created");
        builder.HasIndex(ticket => ticket.CreatedAt).HasDatabaseName("ix_tickets_created_at");
        builder.HasIndex(ticket => ticket.CategoryId).HasDatabaseName("ix_tickets_category_id");
        builder.HasIndex(ticket => ticket.RequesterId).HasDatabaseName("ix_tickets_requester_id");
    }
}
