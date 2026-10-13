using DataTicket.Api.Auth;
using DataTicket.Api.Endpoints.Auth;
using DataTicket.Application.Ports.Out;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Conventions;

/// <summary>
/// HU-004 CHU-07 (PRD §5 regla 1): la empresa de un usuario cliente sale solo de la sesión. Ningún endpoint
/// al que pueda llegar un cliente la recibe por el cuerpo, la consulta, la ruta ni una cabecera. No necesita Docker.
/// </summary>
public sealed class ClientInputConventionTests(OfflineApiFixture api) : IClassFixture<OfflineApiFixture>
{
    [Fact]
    public void DTOs_de_cliente_no_declaran_CompanyId()
    {
        var services = api.Development.Services;
        var isService = services.GetRequiredService<IServiceProviderIsService>();
        var clientEndpoints = EndpointCatalog.All(services)
            .Where(endpoint => !EndpointCatalog.IsInternalOnly(endpoint))
            .ToList();

        var offenders = clientEndpoints
            .SelectMany(endpoint => ClientInputConvention.FindCompanyInputs(endpoint, isService.IsService)
                .Select(finding => $"{string.Join(", ", EndpointCatalog.Signatures(endpoint))}: {finding}"))
            .ToList();

        // Si el recorrido no viera los endpoints de cliente conocidos, la regla pasaría sin mirar nada:
        // el login (cuerpo), el contexto del usuario y un endpoint de control con política ClientUser.
        var inspected = clientEndpoints.SelectMany(EndpointCatalog.Signatures).ToList();
        Assert.Contains("POST /api/auth/login", inspected);
        Assert.Contains("GET /api/auth/me", inspected);
        Assert.Contains($"GET {TestEndpoints.ClientContext}", inspected);
        Assert.DoesNotContain($"GET {TestEndpoints.PolicyProbe(AuthorizationPolicies.Administrator)}", inspected);

        Assert.True(
            offenders.Count == 0,
            $"La empresa sale del claim company_id, nunca de la petición: {string.Join("; ", offenders)}");
    }

    [Fact]
    public void El_endpoint_de_control_de_cliente_exige_la_politica_ClientUser()
    {
        var control = EndpointCatalog.All(api.Development.Services)
            .Single(endpoint => EndpointCatalog.Route(endpoint) == TestEndpoints.ClientContext);

        Assert.Contains(
            control.Metadata.GetOrderedMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>(),
            data => data.Policy == "ClientUser");
        Assert.False(EndpointCatalog.IsInternalOnly(control));
    }

    [Fact]
    public void El_detector_reconoce_la_empresa_en_cualquier_canal()
    {
        var undetected = Violations
            .Where(violation => !ClientInputConvention.FindCompanyInputs(violation.Endpoint, isService: _ => false)
                .Any(finding => finding.Contains(violation.ExpectedFinding, StringComparison.Ordinal)))
            .Select(violation => violation.Channel)
            .ToList();

        Assert.True(undetected.Count == 0, $"No se detectó la empresa en: {string.Join(", ", undetected)}");
    }

    [Fact]
    public void El_detector_ignora_servicios_y_entradas_sin_empresa()
    {
        // ICurrentUser declara CompanyId, pero es un servicio (la sesión), no una entrada de la petición.
        var injected = Endpoint("/api/tickets", (LoginRequest request, ICurrentUser currentUser, CancellationToken _) => Results.Ok());
        var fromServices = Endpoint("/api/tickets/{id}", (Guid id, [FromServices] ICurrentUser currentUser) => Results.Ok());

        Assert.Empty(ClientInputConvention.FindCompanyInputs(injected, type => type == typeof(ICurrentUser)));
        Assert.Empty(ClientInputConvention.FindCompanyInputs(fromServices, _ => false));
    }

    /// <summary>Un manejador por canal por el que podría colarse la empresa, con el hallazgo esperado.</summary>
    private static (string Channel, RouteEndpoint Endpoint, string ExpectedFinding)[] Violations =>
    [
        ("cuerpo", Endpoint("/api/tickets", (BodyWithCompany body) => Results.Ok()), ".CompanyId"),
        ("cuerpo anidado", Endpoint("/api/tickets", (BodyWithNestedFilter body) => Results.Ok()), ".Filter.CompanyId"),
        ("lista en el cuerpo", Endpoint("/api/tickets", (BodyWithItems body) => Results.Ok()), ".Items.CompanyId"),
        ("consulta", Endpoint("/api/tickets", (Guid companyId) => Results.Ok()), "«companyId»"),
        (
            "consulta con nombre",
            Endpoint("/api/tickets", ([FromQuery(Name = "company_id")] Guid tenant) => Results.Ok()),
            "«company_id»"),
        (
            "cabecera",
            Endpoint("/api/tickets", ([FromHeader(Name = "X-Company-Id")] string? tenant) => Results.Ok()),
            "«X-Company-Id»"),
        ("ruta", Endpoint("/api/companies/{companyId}/tickets", () => Results.Ok()), "{companyId}"),
        ("parámetros agrupados", Endpoint("/api/tickets", ([AsParameters] QueryWithCompany query) => Results.Ok()), ".CompanyId"),
    ];

    private static RouteEndpoint Endpoint(string pattern, Delegate handler)
    {
        var builder = new RouteEndpointBuilder(_ => Task.CompletedTask, RoutePatternFactory.Parse(pattern), order: 0);
        builder.Metadata.Add(handler.Method);
        return (RouteEndpoint)builder.Build();
    }

    private sealed record BodyWithCompany(string Title, Guid CompanyId);

    private sealed record BodyWithNestedFilter(string Title, NestedFilter Filter);

    private sealed record NestedFilter(Guid? CompanyId);

    private sealed record BodyWithItems(IReadOnlyList<BodyWithCompany> Items);

    private sealed record QueryWithCompany(string? Status, Guid? CompanyId);
}
