using DataTicket.Domain.Common;

namespace DataTicket.Domain.Tickets;

/// <summary>
/// Categoría del formulario común (glosario <c>Category</c>, PRD §6.1). El catálogo no está en el PRD (V-12): la
/// semilla provisional («Incidente», «Requerimiento», «Consulta») la crea la migración como datos de referencia
/// (<c>db.sql</c> §12) y se cambia como datos, sin tocar código. Solo se listan las activas (HU-013).
/// </summary>
public sealed class Category
{
    public const int NameMaxLength = 80;

    public Category(Guid id, string name, int sortOrder, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador de la categoría es obligatorio.", nameof(id));
        }

        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length is 0 or > NameMaxLength)
        {
            throw new ArgumentException(
                $"El nombre de la categoría debe tener entre 1 y {NameMaxLength} caracteres.", nameof(name));
        }

        Id = id;
        Name = trimmed;
        SortOrder = sortOrder;
        IsActive = true;
        CreatedAt = Timestamps.ToStoredPrecision(createdAt);
    }

    // Para EF Core.
    private Category()
    {
        Name = string.Empty;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
