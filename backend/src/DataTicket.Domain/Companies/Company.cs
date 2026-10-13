using DataTicket.Domain.Common;

namespace DataTicket.Domain.Companies;

/// <summary>
/// Empresa cliente (glosario <c>Company</c>). Delimita qué tickets ve cada usuario cliente (PRD §5, §12).
/// Se desactiva, no se borra. Por ahora solo lo necesario para la cuenta del cliente; la administración
/// llega con HU-007.
/// </summary>
public sealed class Company
{
    public const int NameMaxLength = 150;

    public Company(Guid id, string name, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador de la empresa es obligatorio.", nameof(id));
        }

        Id = id;
        Name = NormalizeName(name);
        IsActive = true;
        CreatedAt = Timestamps.ToStoredPrecision(createdAt);
        UpdatedAt = CreatedAt;
    }

    // Para EF Core.
    private Company()
    {
        Name = string.Empty;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? DeactivatedAt { get; private set; }

    private static string NormalizeName(string name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length is 0 or > NameMaxLength)
        {
            throw new ArgumentException(
                $"El nombre de la empresa debe tener entre 1 y {NameMaxLength} caracteres.", nameof(name));
        }

        return trimmed;
    }
}
