using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace DataTicket.Api.Auth;

/// <summary>
/// Autenticación por cookie de Identity y antiforgery por cabecera (contrato T-01 de HU-003, ADR-0004).
/// Identity (almacenes, gestores, bloqueo, Data Protection) se registra en Infrastructure.
/// </summary>
public static class AuthenticationSetup
{
    public const string AuthCookieName = "dataticket.auth";
    public const string AntiforgeryCookieName = "dataticket.xsrf";
    public const string AntiforgeryHeaderName = "X-XSRF-TOKEN";

    /// <summary>Expiración deslizante de la sesión (P-22, propuesta).</summary>
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);

    public static IServiceCollection AddDataTicketAuthentication(this IServiceCollection services, IHostEnvironment environment)
    {
        // Fuera de Development las cookies solo viajan por HTTPS; en local, igual que la petición (http://localhost).
        var securePolicy = environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = AuthCookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = securePolicy;
            options.Cookie.Path = "/";
            options.ExpireTimeSpan = SessionLifetime;
            options.SlidingExpiration = true;

            // La API nunca redirige a una página de login: 401/403 en ProblemDetails y sin Location.
            options.Events.OnRedirectToLogin = context =>
                ApiProblems.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, ApiProblems.UnauthenticatedTitle);
            options.Events.OnRedirectToAccessDenied = context =>
                ApiProblems.WriteAsync(context.HttpContext, StatusCodes.Status403Forbidden, ApiProblems.ForbiddenTitle);
        });

        services.AddAntiforgery(options =>
        {
            options.HeaderName = AntiforgeryHeaderName;
            options.Cookie.Name = AntiforgeryCookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = securePolicy;
        });

        services.AddAuthorization();

        return services;
    }
}
