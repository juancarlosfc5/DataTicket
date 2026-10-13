using Microsoft.AspNetCore.Antiforgery;

namespace DataTicket.Api.Auth;

/// <summary>
/// Filtro de endpoint del grupo raíz <c>/api</c>: toda mutación (POST, PUT, PATCH, DELETE) exige la cabecera
/// <c>X-XSRF-TOKEN</c> válida para la identidad actual. <c>UseAntiforgery</c> solo cubre formularios, por eso
/// se valida aquí. Corre después de la autorización: sin sesión, una ruta protegida responde 401 antes.
/// </summary>
public sealed partial class AntiforgeryValidationFilter(
    IAntiforgery antiforgery,
    ILogger<AntiforgeryValidationFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        if (IsMutation(httpContext.Request.Method))
        {
            try
            {
                await antiforgery.ValidateRequestAsync(httpContext);
            }
            catch (AntiforgeryValidationException exception)
            {
                LogRejected(logger, httpContext.Request.Method, httpContext.Request.Path, exception.Message);
                return TypedResults.Problem(
                    title: ApiProblems.InvalidRequestTitle,
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?> { ["code"] = ApiProblems.AntiforgeryInvalidCode });
            }
        }

        return await next(context);
    }

    private static bool IsMutation(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);

    [LoggerMessage(Level = LogLevel.Information, Message = "Antiforgery rechazado en {Method} {Path}: {Reason}")]
    private static partial void LogRejected(ILogger logger, string method, PathString path, string reason);
}
