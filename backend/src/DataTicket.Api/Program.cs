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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthEndpoints(includeDetails: app.Environment.IsDevelopment());

// Grupo raíz de la API: el filtro antiforgery cubre TODAS sus mutaciones (POST, PUT, PATCH, DELETE).
var api = app.MapGroup("/api").AddEndpointFilter<AntiforgeryValidationFilter>();
api.MapAuthEndpoints();

await app.RunAsync();
