using System.Security.Claims;
using DataTicket.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Endpoints que solo existen en el host de pruebas, para observar la sesión y las respuestas 401/403 mientras
/// no hay rutas protegidas de producto (llegan con HU-004 en adelante). Se añaden después del pipeline del Api,
/// así que pasan por su autenticación, sus eventos de cookie y sus ProblemDetails.
/// </summary>
public sealed class TestEndpoints : IStartupFilter
{
    public const string Authenticated = "/api/test/authenticated";
    public const string AdministratorOnly = "/api/test/administrator-only";
    public const string PersistentCookie = "/api/test/persistent-cookie";
    public const string PersistentCookieName = "dataticket.test.persistent";

    public static readonly TimeSpan PersistentCookieLifetime = TimeSpan.FromHours(1);

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        next(app);

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet(Authenticated, (ClaimsPrincipal user) =>
                    Results.Ok(new { userId = user.FindFirstValue(ClaimTypes.NameIdentifier) }))
                .RequireAuthorization();

            // Requisito de rol ad hoc: las políticas con nombre llegan en B4 (HU-004).
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
        });
    };
}
