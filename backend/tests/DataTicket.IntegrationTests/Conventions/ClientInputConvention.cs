using System.Collections;
using System.IO.Pipelines;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;

namespace DataTicket.IntegrationTests.Conventions;

/// <summary>
/// Convención de HU-004 CHU-07 (PRD §5 regla 1): un endpoint de cliente no recibe la empresa por ningún
/// canal de la petición. Inspecciona la plantilla de ruta, los parámetros del manejador (ruta, consulta,
/// cabeceras, formulario) y, de forma recursiva, las propiedades de sus DTO.
/// </summary>
public static class ClientInputConvention
{
    private const string ForbiddenName = "companyid";
    private const string OwnAssemblyPrefix = "DataTicket";

    private static readonly Type[] FrameworkParameters =
    [
        typeof(HttpContext), typeof(HttpRequest), typeof(HttpResponse), typeof(CancellationToken),
        typeof(ClaimsPrincipal), typeof(Stream), typeof(PipeReader),
    ];

    /// <summary>
    /// Entradas del endpoint que nombran la empresa (vacío si cumple la convención).
    /// </summary>
    /// <param name="endpoint">Endpoint a inspeccionar.</param>
    /// <param name="isService">Indica si un tipo se resuelve del contenedor (no es entrada de la petición).</param>
    public static IReadOnlyList<string> FindCompanyInputs(RouteEndpoint endpoint, Func<Type, bool> isService)
    {
        var findings = new List<string>();

        foreach (var parameter in endpoint.RoutePattern.Parameters.Where(parameter => NamesCompany(parameter.Name)))
        {
            findings.Add($"parámetro de ruta {{{parameter.Name}}}");
        }

        var handler = endpoint.Metadata.GetMetadata<MethodInfo>();
        if (handler is null)
        {
            return findings;
        }

        var visited = new HashSet<Type>();
        foreach (var parameter in handler.GetParameters().Where(parameter => !IsInjected(parameter, isService)))
        {
            Inspect(
                parameter.Name, parameter.ParameterType, parameter.GetCustomAttributes(inherit: true),
                $"parámetro «{parameter.Name}»", visited, findings);
        }

        return findings;
    }

    private static void Inspect(
        string? name, Type type, IEnumerable<object> attributes, string path, HashSet<Type> visited, List<string> findings)
    {
        if (NamesCompany(name))
        {
            findings.Add(path);
        }

        foreach (var boundName in attributes.Select(BoundName).Where(NamesCompany))
        {
            findings.Add($"{path} (enlazado como «{boundName}»)");
        }

        var contentType = ContentType(type);
        if (!IsOwnType(contentType) || !visited.Add(contentType))
        {
            return;
        }

        foreach (var property in contentType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            Inspect(
                property.Name, property.PropertyType, property.GetCustomAttributes(inherit: true),
                $"{path}.{property.Name}", visited, findings);
        }
    }

    /// <summary>Nombre explícito de enlace (<c>[FromQuery(Name = …)]</c>, <c>[FromHeader(Name = …)]</c>, …).</summary>
    private static string? BoundName(object attribute) => attribute switch
    {
        IFromQueryMetadata query => query.Name,
        IFromHeaderMetadata header => header.Name,
        IFromRouteMetadata route => route.Name,
        IFromFormMetadata form => form.Name,
        _ => null,
    };

    private static bool IsInjected(ParameterInfo parameter, Func<Type, bool> isService) =>
        parameter.GetCustomAttributes(inherit: true).Any(attribute => attribute is IFromServiceMetadata)
        || FrameworkParameters.Any(framework => framework.IsAssignableFrom(parameter.ParameterType))
        || isService(parameter.ParameterType);

    /// <summary>Tipo de contenido: desenvuelve <c>Nullable&lt;T&gt;</c>, arreglos y colecciones.</summary>
    private static Type ContentType(Type type)
    {
        var unwrapped = Nullable.GetUnderlyingType(type) ?? type;
        if (unwrapped.IsArray)
        {
            return ContentType(unwrapped.GetElementType()!);
        }

        if (unwrapped != typeof(string) && typeof(IEnumerable).IsAssignableFrom(unwrapped) && unwrapped.IsGenericType)
        {
            return ContentType(unwrapped.GetGenericArguments()[^1]);
        }

        return unwrapped;
    }

    private static bool IsOwnType(Type type) =>
        type.Assembly.GetName().Name?.StartsWith(OwnAssemblyPrefix, StringComparison.Ordinal) == true && !type.IsEnum;

    /// <summary><c>CompanyId</c>, <c>companyId</c>, <c>company_id</c>, <c>X-Company-Id</c>…</summary>
    private static bool NamesCompany(string? name) =>
        name is not null
        && string.Concat(name.Where(char.IsLetterOrDigit)).Contains(ForbiddenName, StringComparison.OrdinalIgnoreCase);
}
