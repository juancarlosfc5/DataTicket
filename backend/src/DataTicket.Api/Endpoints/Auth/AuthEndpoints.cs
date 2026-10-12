using DataTicket.Api.Auth;
using DataTicket.Application.Ports.In.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DataTicket.Api.Endpoints.Auth;

/// <summary>
/// <c>/api/auth</c> (HU-003 y HU-004): antiforgery, login, logout y contexto del usuario. Endpoints propios:
/// nunca <c>MapIdentityApi</c>, que expone el alta pública de cuentas (prohibida por el PRD §5).
/// </summary>
/// <remarks>
/// Solo <c>antiforgery</c> y <c>login</c> son públicos (<c>AllowAnonymous</c> explícito); el resto queda bajo
/// la política de reserva, que exige un usuario autenticado.
/// </remarks>
public static class AuthEndpoints
{
    private const string NoStore = "no-store";

    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder api)
    {
        var auth = api.MapGroup("/auth").WithTags("Autenticación");

        auth.MapGet("/antiforgery", GetAntiforgeryToken)
            .AllowAnonymous()
            .WithName("GetAntiforgeryToken")
            .WithSummary("Emite el token antiforgery para la identidad actual (pídelo de nuevo tras login y logout).");

        auth.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Inicia sesión con correo y contraseña; emite la cookie dataticket.auth.")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        auth.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .WithSummary("Cierra la sesión del navegador (expira la cookie).")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        auth.MapGet("/me", GetCurrentUserAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithSummary("Devuelve el usuario de la sesión: identidad, superficie, roles, equipos y empresa (solo cliente).")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return api;
    }

    private static Ok<AntiforgeryTokenResponse> GetAntiforgeryToken(HttpContext httpContext, IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(httpContext);
        httpContext.Response.Headers.CacheControl = NoStore;
        return TypedResults.Ok(new AntiforgeryTokenResponse(AuthenticationSetup.AntiforgeryHeaderName, tokens.RequestToken!));
    }

    private static async Task<Results<NoContent, ValidationProblem, ProblemHttpResult>> LoginAsync(
        LoginRequest? request, ISignIn signIn, CancellationToken cancellationToken)
    {
        request ??= new LoginRequest(null, null);
        var errors = request.Validate();
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, title: ApiProblems.ValidationTitle);
        }

        var outcome = await signIn.ExecuteAsync(new SignInCommand(request.Email!, request.Password!), cancellationToken);
        return outcome == SignInOutcome.Succeeded
            ? TypedResults.NoContent()
            : TypedResults.Problem(title: ApiProblems.InvalidCredentialsTitle, statusCode: StatusCodes.Status401Unauthorized);
    }

    private static async Task<NoContent> LogoutAsync(ISignOut signOut, CancellationToken cancellationToken)
    {
        await signOut.ExecuteAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// No declara parámetros de la petición: la respuesta sale solo de la sesión (cualquier <c>?userId=</c> o
    /// cabecera de empresa se ignora). Una sesión sin perfil válido no recibe datos: 403.
    /// </summary>
    private static async Task<Results<Ok<CurrentUserResponse>, ProblemHttpResult>> GetCurrentUserAsync(
        HttpContext httpContext, IGetCurrentUserProfile getCurrentUserProfile, CancellationToken cancellationToken)
    {
        httpContext.Response.Headers.CacheControl = NoStore;

        var profile = await getCurrentUserProfile.ExecuteAsync(cancellationToken);
        return profile is null
            ? TypedResults.Problem(title: ApiProblems.ForbiddenTitle, statusCode: StatusCodes.Status403Forbidden)
            : TypedResults.Ok(CurrentUserResponse.From(profile));
    }
}

/// <summary>Respuesta de <c>GET /api/auth/antiforgery</c>.</summary>
public sealed record AntiforgeryTokenResponse(string HeaderName, string Token);
