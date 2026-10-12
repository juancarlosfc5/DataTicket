using System.Security.Claims;
using DataTicket.Api.Auth;
using DataTicket.Application.Ports.Out;
using DataTicket.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Endpoints que solo existen en el host de pruebas, para observar la sesión, las políticas y las respuestas
/// 401/403 mientras no hay rutas protegidas de producto.
/// </summary>
/// <remarks>
/// <para>
/// No se añaden a la tabla de rutas del Api: se resuelven en un SEGUNDO pipeline, montado a continuación del
/// del Api, con su propio enrutamiento, autenticación y autorización (<c>UseRouting</c>,
/// <c>UseAuthentication</c>, <c>UseAuthorization</c>, <c>UseEndpoints</c>). Lo que comparten con el Api son
/// los SERVICIOS de inyección de dependencias (esquema y eventos de la cookie, políticas con nombre y de
/// reserva, manejador de resultados de autorización, ProblemDetails), no sus middleware de enrutamiento y
/// autorización.
/// </para>
/// <para>
/// Consecuencias: una petición a estas rutas atraviesa antes el pipeline del Api sin coincidir con ningún
/// endpoint y, si no trae sesión, llega aquí solo porque <c>EndpointScopedAuthorizationResultHandler</c> deja
/// pasar lo que no tiene endpoint; y un filtro o una convención aplicados al grupo raíz <c>/api</c> del Api (p. ej. el filtro
/// antiforgery) NO alcanzan a estos endpoints. Lo que estas pruebas demuestran del Api es el comportamiento
/// de sus servicios de autenticación y autorización, no el orden de su pipeline.
/// </para>
/// </remarks>
public sealed class TestEndpoints : IStartupFilter
{
    public const string Prefix = "/api/test/";
    public const string Authenticated = "/api/test/authenticated";
    public const string AdministratorOnly = "/api/test/administrator-only";
    public const string PersistentCookie = "/api/test/persistent-cookie";
    public const string PersistentCookieName = "dataticket.test.persistent";

    /// <summary>Control de HU-004 CHU-06: sin ningún metadato de autorización; solo lo protege la política de reserva.</summary>
    public const string NoMetadata = "/api/test/no-metadata";

    /// <summary>
    /// Endpoint de cliente (política <c>ClientUser</c>) que devuelve la empresa que ve <see cref="ICurrentUser"/>,
    /// como la leería un caso de uso (HU-004 CHU-07).
    /// </summary>
    public const string ClientContext = "/api/test/client-context";

    public static readonly TimeSpan PersistentCookieLifetime = TimeSpan.FromHours(1);

    /// <summary>Ruta de sondeo de una política con nombre: 204 si la sesión la satisface.</summary>
    public static string PolicyProbe(string policy) => $"/api/test/policy/{policy}";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        next(app);

        // Segundo pipeline: middleware propios, servicios de DI compartidos con el Api (ver el resumen de la clase).
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet(Authenticated, (ClaimsPrincipal user) =>
                    Results.Ok(new { userId = user.FindFirstValue(ClaimTypes.NameIdentifier) }))
                .RequireAuthorization();

            // Requisito de rol ad hoc (HU-003 CHU-10); las políticas con nombre se sondean más abajo.
            endpoints.MapGet(AdministratorOnly, () => Results.NoContent())
                .RequireAuthorization(policy => policy.RequireRole(RoleNames.Administrator));

            // Cookie persistente que vence según el reloj del host (para probar el propio TestBrowser).
            endpoints.MapGet(PersistentCookie, (HttpContext context, TimeProvider clock) =>
                {
                    context.Response.Cookies.Append(
                        PersistentCookieName,
                        "1",
                        new CookieOptions { Expires = clock.GetUtcNow() + PersistentCookieLifetime, Path = "/" });
                    return Results.NoContent();
                })
                .RequireAuthorization();

            // Los controles de HU-004 no se describen en OpenAPI: no son rutas de producto.
            endpoints.MapGet(NoMetadata, () => Results.NoContent()).ExcludeFromDescription();

            foreach (var policy in AuthorizationPolicies.All)
            {
                endpoints.MapGet(PolicyProbe(policy), () => Results.NoContent())
                    .RequireAuthorization(policy)
                    .ExcludeFromDescription();
            }

            endpoints.MapGet(ClientContext, ([FromServices] ICurrentUser currentUser, [AsParameters] ClientContextQuery query) =>
                    Results.Ok(new { companyId = currentUser.CompanyId, status = query.Status }))
                .RequireAuthorization(AuthorizationPolicies.ClientUser)
                .ExcludeFromDescription();
        });
    };
}

/// <summary>Entrada de ejemplo de un endpoint de cliente: filtros propios, nunca la empresa.</summary>
public sealed record ClientContextQuery(string? Status);
