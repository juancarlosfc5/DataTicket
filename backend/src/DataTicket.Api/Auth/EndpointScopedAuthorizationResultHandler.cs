using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace DataTicket.Api.Auth;

/// <summary>
/// Limita la política de reserva a los endpoints registrados. Sin esto, ASP.NET Core la aplica también a
/// las peticiones que no coinciden con ningún endpoint, y una ruta inexistente respondería 401 a un anónimo
/// en lugar de 404 (HU-003 CHU-09: las rutas de registro responden 404).
/// </summary>
/// <remarks>
/// <para>
/// Solo cambia el caso «no hay endpoint»: la petición sigue por el pipeline y termina en 404, con o sin
/// sesión. Todo endpoint, lleve o no metadatos de autorización, se resuelve con el comportamiento estándar
/// (401 sin sesión, 403 sin permiso).
/// </para>
/// <para>
/// Consecuencia a vigilar: un middleware que sirviera contenido SIN endpoint y colocado después de
/// <c>UseAuthorization</c> (p. ej. archivos estáticos) no quedaría protegido por la política de reserva.
/// Este Api no tiene ninguno: todo lo que sirve son endpoints (minimal APIs, salud, OpenAPI y, más adelante,
/// el hub de SignalR y <c>MapStaticAssets</c>). La regla está escrita junto al mapeo, en <c>Program.cs</c>, y
/// la vigilan <c>Api_does_not_serve_content_through_endpointless_middleware</c> (ArchitectureTests) y
/// <c>Anonimo_nunca_recibe_2xx_fuera_de_la_lista_publica</c> (IntegrationTests).
/// </para>
/// </remarks>
public sealed class EndpointScopedAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public Task HandleAsync(
        RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult) =>
        context.GetEndpoint() is null
            ? next(context)
            : _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
}
