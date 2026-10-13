using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using DataTicket.Api.Auth;

namespace DataTicket.ArchitectureTests;

/// <summary>
/// Hallazgo MEDIO de la revisión de B4 (HU-004). La política de reserva solo cubre endpoints:
/// <see cref="EndpointScopedAuthorizationResultHandler"/> deja pasar las peticiones sin endpoint para que una
/// ruta inexistente responda 404. Por eso el Api sirve TODO su contenido como endpoint (minimal APIs,
/// <c>MapHealthChecks</c>, <c>MapOpenApi</c>, <c>MapHub</c>, <c>MapStaticAssets</c>) y nunca con un middleware
/// sin endpoint, que quedaría fuera de la política de reserva.
/// </summary>
/// <remarks>
/// Lee las referencias a métodos del ensamblado compilado del Api, no el código fuente. Detecta los
/// middleware con nombre de la lista; un <c>app.Use(...)</c> en línea que escriba la respuesta no se puede
/// detectar así: lo cubre, en lo posible, <c>Anonimo_nunca_recibe_2xx_fuera_de_la_lista_publica</c>.
/// </remarks>
public sealed class EndpointOnlyContentTests
{
    private const string BuilderNamespace = "Microsoft.AspNetCore.Builder";

    /// <summary>Clases de extensión cuyos métodos sirven contenido sin pasar por un endpoint.</summary>
    private static readonly string[] EndpointlessMiddleware =
    [
        $"{BuilderNamespace}.StaticFileExtensions",                    // UseStaticFiles
        $"{BuilderNamespace}.DefaultFilesExtensions",                  // UseDefaultFiles
        $"{BuilderNamespace}.FileServerExtensions",                    // UseFileServer
        $"{BuilderNamespace}.DirectoryBrowserExtensions",              // UseDirectoryBrowser
        $"{BuilderNamespace}.HealthCheckApplicationBuilderExtensions", // UseHealthChecks
        $"{BuilderNamespace}.MapExtensions",                           // app.Map("/ruta", rama => ...)
        $"{BuilderNamespace}.MapWhenExtensions",                       // app.MapWhen(...)
        $"{BuilderNamespace}.RunExtensions",                           // app.Run(contexto => ...)
        $"{BuilderNamespace}.SwaggerBuilderExtensions",                // UseSwagger
        $"{BuilderNamespace}.SwaggerUIBuilderExtensions",              // UseSwaggerUI
        $"{BuilderNamespace}.ReDocBuilderExtensions",                  // UseReDoc
        $"{BuilderNamespace}.SpaApplicationBuilderExtensions",         // UseSpa
        $"{BuilderNamespace}.SpaStaticFilesExtensions",                // UseSpaStaticFiles
    ];

    [Fact]
    public void Api_does_not_serve_content_through_endpointless_middleware()
    {
        var calls = ReferencedMethods(typeof(EndpointScopedAuthorizationResultHandler).Assembly.Location);

        // Si la lectura no viera las llamadas conocidas del pipeline, la regla pasaría sin mirar nada.
        Assert.Contains($"{BuilderNamespace}.AuthAppBuilderExtensions.UseAuthentication", calls);
        Assert.Contains($"{BuilderNamespace}.AuthorizationAppBuilderExtensions.UseAuthorization", calls);

        var offenders = calls
            .Where(call => EndpointlessMiddleware.Any(type => call.StartsWith($"{type}.", StringComparison.Ordinal)))
            .ToList();
        Assert.True(
            offenders.Count == 0,
            "La política de reserva no cubre middleware sin endpoint: sirve ese contenido como endpoint "
            + $"(MapStaticAssets, MapHealthChecks, MapHub, minimal APIs). Llamadas encontradas: {string.Join(", ", offenders)}");
    }

    /// <summary>Métodos de otros ensamblados a los que llama el ensamblado, como <c>Espacio.Tipo.Método</c>.</summary>
    private static List<string> ReferencedMethods(string assemblyPath)
    {
        using var stream = File.OpenRead(assemblyPath);
        using var portableExecutable = new PEReader(stream);
        var metadata = portableExecutable.GetMetadataReader();

        var calls = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var handle in metadata.MemberReferences)
        {
            var member = metadata.GetMemberReference(handle);
            if (member.Parent.Kind != HandleKind.TypeReference)
            {
                continue;
            }

            var type = metadata.GetTypeReference((TypeReferenceHandle)member.Parent);
            calls.Add($"{metadata.GetString(type.Namespace)}.{metadata.GetString(type.Name)}.{metadata.GetString(member.Name)}");
        }

        return [.. calls];
    }
}
