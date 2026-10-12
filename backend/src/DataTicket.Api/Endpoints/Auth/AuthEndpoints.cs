using DataTicket.Api.Auth;
using DataTicket.Application.Ports.In.Authentication;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DataTicket.Api.Endpoints.Auth;

/// <summary>
/// <c>/api/auth</c> (HU-003): antiforgery, login y logout. Endpoints propios: nunca <c>MapIdentityApi</c>,
/// que expone <c>/register</c> (prohibido por el PRD §5).
/// </summary>
public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder api)
    {
        var auth = api.MapGroup("/auth").WithTags("Autenticación");

        auth.MapGet("/antiforgery", GetAntiforgeryToken)
            .WithName("GetAntiforgeryToken")
            .WithSummary("Emite el token antiforgery para la identidad actual (pídelo de nuevo tras login y logout).");

        auth.MapPost("/login", LoginAsync)
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

        return api;
    }

    private static Ok<AntiforgeryTokenResponse> GetAntiforgeryToken(HttpContext httpContext, IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(httpContext);
        httpContext.Response.Headers.CacheControl = "no-store";
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
}

/// <summary>Respuesta de <c>GET /api/auth/antiforgery</c>.</summary>
public sealed record AntiforgeryTokenResponse(string HeaderName, string Token);
