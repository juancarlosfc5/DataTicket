using System.Text.Json;
using DataTicket.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DataTicket.Api.Health;

public static class HealthEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// /api/health/live: el proceso responde (sin tocar dependencias).
    /// /api/health: el proceso y sus dependencias marcadas como "ready" (PostgreSQL).
    /// El detalle por dependencia solo se expone si <paramref name="includeDetails"/> es true (Development).
    /// </summary>
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints, bool includeDetails)
    {
        endpoints.MapHealthChecks("/api/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = (context, report) => WriteJsonAsync(context, report, includeDetails: false),
        }).AsPublicProbe();

        endpoints.MapHealthChecks("/api/health", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(DependencyInjection.ReadinessTag),
            ResponseWriter = (context, report) => WriteJsonAsync(context, report, includeDetails),
        }).AsPublicProbe();

        return endpoints;
    }

    /// <summary>
    /// Sondas públicas del contrato: solo GET y sin sesión. <c>MapHealthChecks</c> acepta cualquier método si no se
    /// restringe; al limitarlo a GET no queda ninguna ruta de <c>/api</c> que admita POST fuera del filtro antiforgery.
    /// </summary>
    private static void AsPublicProbe(this IEndpointConventionBuilder builder) =>
        builder.WithMetadata(new HttpMethodMetadata([HttpMethods.Get])).AllowAnonymous();

    private static Task WriteJsonAsync(HttpContext context, HealthReport report, bool includeDetails)
    {
        object payload = includeDetails
            ? new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                }),
            }
            : new { status = report.Status.ToString() };

        context.Response.ContentType = "application/json; charset=utf-8";
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
