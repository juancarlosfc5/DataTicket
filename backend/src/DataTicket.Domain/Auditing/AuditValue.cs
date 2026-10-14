using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace DataTicket.Domain.Auditing;

/// <summary>
/// Forma serializada de los valores anterior/nuevo de una <see cref="AuditEntry"/> (contrato de HU-011): un
/// objeto JSON con claves en camelCase y valores de dominio en inglés, p. ej. <c>{"status":"New"}</c> o
/// <c>{"assigneeIds":[...]}</c>. Los nombres para mostrar no se guardan: se resuelven al consultar.
/// </summary>
public static class AuditValue
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        // Enumeraciones por nombre (en inglés), nunca por número.
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Convierte un valor al JSON que se guarda en <c>old_value</c>/<c>new_value</c> (<c>jsonb</c>).
    /// <list type="bullet">
    /// <item><c>null</c>: sin valor.</item>
    /// <item><see cref="string"/>: texto JSON ya serializado; se valida y se guarda tal cual.</item>
    /// <item><see cref="JsonElement"/>, <see cref="JsonDocument"/> o <see cref="JsonNode"/>: su texto JSON.</item>
    /// <item>Cualquier otro objeto (p. ej. anónimo): se serializa con propiedades en camelCase y enumeraciones
    /// por nombre.</item>
    /// </list>
    /// </summary>
    /// <exception cref="ArgumentException">El resultado no es un objeto JSON válido.</exception>
    public static string? ToJson(object? value, string? paramName = null)
    {
        if (value is null)
        {
            return null;
        }

        string json;
        try
        {
            json = value switch
            {
                string text => text,
                JsonElement element => element.GetRawText(),
                JsonDocument document => document.RootElement.GetRawText(),
                JsonNode node => node.ToJsonString(Options),
                _ => JsonSerializer.Serialize(value, value.GetType(), Options),
            };
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException or InvalidOperationException)
        {
            throw new ArgumentException("El valor de auditoría no se puede serializar a JSON.", paramName, exception);
        }

        JsonValueKind kind;
        try
        {
            using var document = JsonDocument.Parse(json);
            kind = document.RootElement.ValueKind;
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("El valor de auditoría no es JSON válido.", paramName, exception);
        }

        if (kind != JsonValueKind.Object)
        {
            throw new ArgumentException($"El valor de auditoría debe ser un objeto JSON, no {kind}.", paramName);
        }

        return json;
    }
}
