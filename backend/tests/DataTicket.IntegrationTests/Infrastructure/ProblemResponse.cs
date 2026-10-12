using System.Net;
using System.Text.Json.Nodes;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>Aserciones sobre respuestas ProblemDetails (RFC 9457).</summary>
public static class ProblemResponse
{
    public const string ContentType = "application/problem+json";

    /// <summary>
    /// Comprueba estado, <c>Content-Type</c>, título y ausencia de <c>Location</c>; devuelve el cuerpo.
    /// </summary>
    public static async Task<JsonObject> AssertAsync(
        HttpResponseMessage response, HttpStatusCode status, string title, CancellationToken cancellationToken)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal(ContentType, response.Content.Headers.ContentType?.MediaType);
        Assert.Null(response.Headers.Location);

        var body = await ReadAsync(response, cancellationToken);
        Assert.Equal(title, body["title"]?.GetValue<string>());
        Assert.Equal((int)status, body["status"]?.GetValue<int>());
        return body;
    }

    public static async Task<JsonObject> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken))?.AsObject()
        ?? throw new InvalidOperationException("La respuesta no es un objeto JSON.");

    /// <summary>JSON del cuerpo sin <c>traceId</c>, para comparar respuestas que deben ser idénticas.</summary>
    public static async Task<string> ReadWithoutTraceIdAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await ReadAsync(response, cancellationToken);
        body.Remove("traceId");
        return body.ToJsonString();
    }
}
