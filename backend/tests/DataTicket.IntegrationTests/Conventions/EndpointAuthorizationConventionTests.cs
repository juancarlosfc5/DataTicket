using System.Net;
using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Conventions;

/// <summary>
/// HU-004 CHU-06 (invariante 9, PRD §12): autenticación por defecto en todos los endpoints. La lista de
/// públicos se escribe aquí como literales, a propósito: es la del contrato T-01 y añadir un endpoint
/// anónimo obliga a tocar esta prueba. No necesita Docker: recorre la tabla de endpoints y solo hace
/// peticiones sin sesión, que no llegan a la base.
/// </summary>
public sealed class EndpointAuthorizationConventionTests(OfflineApiFixture api) : IClassFixture<OfflineApiFixture>
{
    private const string UnauthenticatedTitle = "Se requiere iniciar sesión.";
    private const string SampleRouteValue = "00000000-0000-7000-8000-000000000001";

    private static readonly string[] PublicEndpoints =
    [
        "GET /api/health",
        "GET /api/health/live",
        "GET /api/auth/antiforgery",
        "POST /api/auth/login",
    ];

    // GET /openapi/v1.json: la plantilla registrada lleva el nombre del documento como parámetro.
    private static readonly string[] DevelopmentOnlyPublicEndpoints = ["GET /openapi/{documentName}.json"];

    /// <summary>
    /// Rutas que no son endpoints públicos y que un middleware sin endpoint serviría: raíz y archivos
    /// estáticos, documentación por middleware, sondas, el hub (aún sin mapear) y rutas de <c>/api</c>.
    /// </summary>
    private static readonly string[] ProbedPaths =
    [
        "/", "/index.html", "/favicon.ico", "/robots.txt", "/assets/index.js", "/static/app.css", "/appsettings.json",
        "/.well-known/security.txt", "/_framework/aspnetcore-browser-refresh.js",
        "/swagger", "/swagger/index.html", "/swagger/v1/swagger.json", "/scalar", "/scalar/v1", "/openapi", "/openapi/v1.json",
        "/health", "/healthz", "/api/health/ready",
        "/hubs", "/hubs/x", "/hubs/tickets", "/hubs/tickets/negotiate",
        "/api", "/api/", "/api/desconocida", "/api/auth", "/api/auth/register", "/api/tickets", "/api/test",
    ];

    private static readonly HttpMethod[] ProbedMethods = [HttpMethod.Get, HttpMethod.Head, HttpMethod.Post];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Solo_la_lista_publica_admite_anonimos()
    {
        var development = await AnonymousEndpointsAsync(api.Development.Services);
        var production = await AnonymousEndpointsAsync(api.Production.Services);

        // Igualdad exacta: ni un endpoint anónimo de más, ni una entrada de la lista que ya no exista.
        Assert.Equal(Sorted([.. PublicEndpoints, .. DevelopmentOnlyPublicEndpoints]), development);
        Assert.Equal(Sorted(PublicEndpoints), production);
    }

    [Fact]
    public async Task Endpoints_no_publicos_responden_401_sin_sesion()
    {
        var services = api.Development.Services;
        var fallback = await FallbackPolicyAsync(services);
        var protectedEndpoints = EndpointCatalog.All(services)
            .Where(endpoint => !EndpointCatalog.AdmitsAnonymous(endpoint, fallback))
            .ToList();
        using var browser = new TestBrowser(api.Development);
        var failures = new List<string>();

        foreach (var endpoint in protectedEndpoints)
        {
            using var request = AnonymousRequestFor(endpoint);
            using var response = await browser.SendAsync(request, Ct);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                failures.Add($"{request.Method} {request.RequestUri} → {(int)response.StatusCode}");
            }
        }

        // El recorrido incluye, al menos, el contexto del usuario, el cierre de sesión y el control sin metadatos.
        var visited = protectedEndpoints.SelectMany(EndpointCatalog.Signatures).ToList();
        Assert.Contains("GET /api/auth/me", visited);
        Assert.Contains("POST /api/auth/logout", visited);
        Assert.Contains($"GET {TestEndpoints.NoMetadata}", visited);
        Assert.True(failures.Count == 0, $"Deben responder 401 sin sesión: {string.Join("; ", failures)}");
    }

    /// <summary>
    /// Control de CHU-06: un endpoint registrado sin ningún metadato de autorización (como el que alguien
    /// añadiría por descuido) queda protegido por la política de reserva.
    /// </summary>
    [Fact]
    public async Task Endpoint_sin_metadatos_exige_sesion()
    {
        var control = EndpointCatalog.All(api.Development.Services)
            .Single(endpoint => EndpointCatalog.Route(endpoint) == TestEndpoints.NoMetadata);
        Assert.Null(control.Metadata.GetMetadata<IAuthorizeData>());
        Assert.Null(control.Metadata.GetMetadata<AuthorizationPolicy>());
        Assert.Null(control.Metadata.GetMetadata<IAllowAnonymous>());
        using var browser = new TestBrowser(api.Development);

        using var response = await browser.GetAsync(TestEndpoints.NoMetadata, Ct);

        await ProblemResponse.AssertAsync(response, HttpStatusCode.Unauthorized, UnauthenticatedTitle, Ct);
    }

    /// <summary>
    /// La política de reserva protege endpoints, no rutas que no existen: siguen respondiendo 404 (HU-003 CHU-09),
    /// nunca contenido.
    /// </summary>
    [Theory]
    [InlineData("/api/no-existe")]
    [InlineData("/api/auth/no-existe")]
    [InlineData("/api/tickets/00000000-0000-7000-8000-000000000001")]
    [InlineData("/no-existe")]
    public async Task Ruta_inexistente_sin_sesion_responde_404(string path)
    {
        foreach (var factory in new[] { api.Development, api.Production })
        {
            using var browser = new TestBrowser(factory);

            using var response = await browser.GetAsync(path, Ct);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ProblemResponse.ContentType, response.Content.Headers.ContentType?.MediaType);
        }
    }

    /// <summary>
    /// Hallazgo MEDIO de la revisión de B4. La política de reserva solo cubre endpoints: para que una ruta
    /// inexistente responda 404, el manejador de autorización deja pasar lo que no tiene endpoint. Si alguien
    /// añadiera un middleware que sirve contenido SIN endpoint (archivos estáticos, UI de Swagger, sondas por
    /// middleware, una rama <c>app.Map</c>), un anónimo lo recibiría. Esta prueba recorre, sobre el pipeline
    /// real, las rutas que esos middleware suelen servir: sin sesión, solo la lista pública responde 2xx.
    /// </summary>
    [Fact]
    public async Task Anonimo_nunca_recibe_2xx_fuera_de_la_lista_publica()
    {
        var hosts = new (DataTicketApiFactory Factory, string[] Public)[]
        {
            (api.Development, [.. PublicEndpoints, "GET /openapi/v1.json"]),
            (api.Production, PublicEndpoints),
        };
        var leaks = new List<string>();

        foreach (var (factory, publicEndpoints) in hosts)
        {
            using var browser = new TestBrowser(factory);

            // Si el sondeo no viera el 2xx de un endpoint público, la regla pasaría sin mirar nada.
            using var control = await browser.GetAsync("/api/health/live", Ct);
            Assert.Equal(HttpStatusCode.OK, control.StatusCode);

            foreach (var path in ProbedPaths)
            {
                foreach (var method in ProbedMethods)
                {
                    using var request = new HttpRequestMessage(method, path);
                    using var response = await browser.SendAsync(request, Ct);
                    if (response.IsSuccessStatusCode && !publicEndpoints.Contains($"{method.Method} {path}"))
                    {
                        leaks.Add($"{factory.Options.Environment}: {method.Method} {path} → {(int)response.StatusCode}");
                    }
                }
            }
        }

        Assert.True(
            leaks.Count == 0,
            "Un anónimo recibió contenido fuera de la lista pública. Sírvelo como endpoint (la política de reserva "
            + $"no cubre middleware sin endpoint): {string.Join("; ", leaks)}");
    }

    [Fact]
    public async Task OpenApi_no_se_publica_fuera_de_Development()
    {
        using var browser = new TestBrowser(api.Production);

        using var response = await browser.GetAsync("/openapi/v1.json", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain(
            EndpointCatalog.All(api.Production.Services),
            endpoint => EndpointCatalog.Route(endpoint).Contains("openapi", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<List<string>> AnonymousEndpointsAsync(IServiceProvider services)
    {
        var fallback = await FallbackPolicyAsync(services);
        return Sorted(EndpointCatalog.All(services)
            .Where(endpoint => EndpointCatalog.AdmitsAnonymous(endpoint, fallback))
            .SelectMany(EndpointCatalog.Signatures));
    }

    private static Task<AuthorizationPolicy?> FallbackPolicyAsync(IServiceProvider services) =>
        services.GetRequiredService<IAuthorizationPolicyProvider>().GetFallbackPolicyAsync();

    private static List<string> Sorted(IEnumerable<string> signatures) =>
        [.. signatures.Order(StringComparer.Ordinal)];

    private static HttpRequestMessage AnonymousRequestFor(RouteEndpoint endpoint)
    {
        var method = EndpointCatalog.Methods(endpoint).FirstOrDefault() ?? HttpMethod.Get.Method;
        var path = EndpointCatalog.Route(endpoint);
        foreach (var parameter in endpoint.RoutePattern.Parameters)
        {
            path = path.Replace($"{{{parameter.Name}}}", SampleRouteValue, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("{", path, StringComparison.Ordinal);
        return new HttpRequestMessage(new HttpMethod(method), path);
    }
}
