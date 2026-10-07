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
        });

        endpoints.MapHealthChecks("/api/health", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(DependencyInjection.ReadinessTag),
            ResponseWriter = (context, report) => WriteJsonAsync(context, report, includeDetails),
        });

        return endpoints;
    }

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
