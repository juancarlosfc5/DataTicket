using DataTicket.Api.Auth;
using DataTicket.Api.Endpoints.Auth;
using DataTicket.Api.Health;
using DataTicket.Application;
using DataTicket.Infrastructure;
using DataTicket.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Reloj único de la aplicación (casos de uso, cookie de autenticación, sello de seguridad, bloqueo).
// Se registra antes que cualquier otro servicio; las pruebas de integración lo sustituyen por FakeTimeProvider.
builder.Services.AddSingleton(TimeProvider.System);

// Composition root: el adaptador primario conecta los casos de uso con los adaptadores secundarios.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddDataTicketAuthentication(builder.Environment);
builder.Services.AddDataTicketAuthorization();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Un cuerpo ilegible (p. ej. JSON mal formado) es un error del cliente: 400 en ProblemDetails en todos los
// entornos. Por defecto Development lanza BadHttpRequestException, que el manejador de excepciones convierte en 500.
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);

var app = builder.Build();

// Solo Development aplica las migraciones al arrancar (Persistence:ApplyMigrationsOnStartup en appsettings.Development.json).
if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>(DatabaseMigrations.ApplyOnStartupKey))
{
    await DatabaseMigrations.ApplyAsync(app.Services, app.Lifetime.ApplicationStopping);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

// Con la política de reserva, todo endpoint exige sesión salvo los públicos del contrato, que lo declaran con
// AllowAnonymous: salud, antiforgery, login y, solo en Development, el documento OpenAPI.
//
// REGLA: el contenido se sirve SIEMPRE como endpoint: minimal APIs, MapHealthChecks, MapOpenApi y, cuando lleguen, MapHub y
// MapStaticAssets. Nunca UseStaticFiles, UseHealthChecks, una UI de Swagger por middleware ni ramas app.Map(...),
// app.MapWhen(...) o app.Run(...) sin endpoint. La política de reserva solo cubre endpoints: para que una ruta
// inexistente responda 404 y no 401, EndpointScopedAuthorizationResultHandler deja pasar lo que no tiene endpoint,
// así que un middleware que sirva contenido después de UseAuthorization lo entregaría a un anónimo.
// Lo vigilan Api_does_not_serve_content_through_endpointless_middleware (ArchitectureTests) y
// Anonimo_nunca_recibe_2xx_fuera_de_la_lista_publica (IntegrationTests).
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.MapHealthEndpoints(includeDetails: app.Environment.IsDevelopment());

// Grupo raíz de la API: el filtro antiforgery cubre TODAS sus mutaciones (POST, PUT, PATCH, DELETE). Todo endpoint
// de producto cuelga de este grupo; una prueba de convención delata cualquier mutación mapeada fuera, sea cual sea
// su ruta (Toda_mutacion_lleva_el_filtro_antiforgery).
var api = app.MapGroup("/api").RequireAntiforgeryValidation();
api.MapAuthEndpoints();

await app.RunAsync();
