using System.Collections;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Políticas con nombre REALMENTE registradas en el <see cref="AuthorizationOptions"/> del host. Las pruebas
/// las leen de aquí para no depender de una lista escrita a mano ni de la que declara el propio Api.
/// </summary>
/// <remarks>
/// <see cref="AuthorizationOptions"/> no expone la lista (solo <c>GetPolicy(nombre)</c>): se lee su
/// diccionario privado <c>PolicyMap</c> por reflexión (comprobado en ASP.NET Core 10.0). Si una versión
/// futura lo renombra, <see cref="Names"/> falla con un mensaje claro en lugar de devolver una lista vacía.
/// </remarks>
public static class RegisteredPolicies
{
    private const string PolicyMapProperty = "PolicyMap";

    /// <summary>Nombres de las políticas registradas, en orden ordinal.</summary>
    public static IReadOnlyList<string> Names(IServiceProvider services)
    {
        var options = services.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        var policyMap = typeof(AuthorizationOptions)
            .GetProperty(PolicyMapProperty, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(options) as IDictionary
            ?? throw new InvalidOperationException(
                $"AuthorizationOptions ya no tiene el diccionario privado '{PolicyMapProperty}': revisa RegisteredPolicies.");

        return [.. policyMap.Keys.Cast<string>().Order(StringComparer.Ordinal)];
    }
}
