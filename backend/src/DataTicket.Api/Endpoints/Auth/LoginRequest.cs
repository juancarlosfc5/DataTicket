using System.Net.Mail;

namespace DataTicket.Api.Endpoints.Auth;

/// <summary>Cuerpo de <c>POST /api/auth/login</c>.</summary>
public sealed record LoginRequest(string? Email, string? Password)
{
    public const int EmailMaxLength = 256;

    /// <summary>
    /// Validación de forma (HU-003 CHU-06). Las claves del diccionario son los nombres JSON (camelCase).
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Validate()
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(Email))
        {
            errors["email"] = ["El correo es obligatorio."];
        }
        else if (Email.Length > EmailMaxLength)
        {
            errors["email"] = [$"El correo admite como máximo {EmailMaxLength} caracteres."];
        }
        else if (!IsEmailAddress(Email))
        {
            errors["email"] = ["El correo no tiene un formato válido."];
        }

        if (string.IsNullOrEmpty(Password))
        {
            errors["password"] = ["La contraseña es obligatoria."];
        }

        return errors;
    }

    // Dirección simple (sin nombre visible ni espacios) con dominio de al menos dos etiquetas.
    private static bool IsEmailAddress(string value) =>
        MailAddress.TryCreate(value, out var address)
        && string.Equals(address.Address, value, StringComparison.Ordinal)
        && address.Host.Contains('.', StringComparison.Ordinal);
}
