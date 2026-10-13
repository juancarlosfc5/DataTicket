using System.Diagnostics;

namespace DataTicket.Api.Auth;

/// <summary>
/// Respuestas ProblemDetails (RFC 9457) comunes de la API, con los textos del contrato T-01 (HU-003/HU-004).
/// </summary>
public static class ApiProblems
{
    public const string InvalidCredentialsTitle = "Correo o contraseña incorrectos.";
    public const string InvalidRequestTitle = "La solicitud no es válida.";
    public const string ValidationTitle = "Uno o más campos no son válidos.";
    public const string UnauthenticatedTitle = "Se requiere iniciar sesión.";
    public const string ForbiddenTitle = "No tienes permiso para realizar esta acción.";

    /// <summary>Extensión <c>code</c> del 400 por antiforgery ausente o inválido.</summary>
    public const string AntiforgeryInvalidCode = "antiforgery_invalid";

    private const string TraceIdExtension = "traceId";

    /// <summary>
    /// Escribe un ProblemDetails (con <c>traceId</c>) y sin <c>Location</c>. Se usa desde los eventos de la
    /// cookie, donde no hay <see cref="IResult"/>.
    /// </summary>
    /// <remarks>
    /// No depende de la cabecera <c>Accept</c>: <see cref="IProblemDetailsService"/> solo escribe si el cliente
    /// acepta JSON y <c>WriteAsync</c> lanza si no (un 401 acabaría en 500 para <c>Accept: text/html</c>).
    /// El resultado <c>Problem</c> intenta ese servicio y, si no aplica, escribe el mismo JSON directamente:
    /// la API responde siempre <c>application/problem+json</c>.
    /// </remarks>
    public static Task WriteAsync(HttpContext httpContext, int status, string title)
    {
        httpContext.Response.Headers.Location = default;

        var extensions = new Dictionary<string, object?>
        {
            [TraceIdExtension] = Activity.Current?.Id ?? httpContext.TraceIdentifier,
        };

        return TypedResults.Problem(title: title, statusCode: status, extensions: extensions).ExecuteAsync(httpContext);
    }
}
