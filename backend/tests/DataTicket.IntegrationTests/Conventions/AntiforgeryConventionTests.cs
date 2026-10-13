using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace DataTicket.IntegrationTests.Conventions;

/// <summary>
/// Hallazgo BAJO-8 de la revisión de B3 y BAJO-2 de la de B4: TODA mutación (POST, PUT, PATCH, DELETE, o un
/// endpoint sin restricción de método) pasa por el filtro antiforgery, esté o no bajo <c>/api</c>. El filtro
/// cuelga del grupo raíz; un endpoint mapeado fuera del grupo no lo lleva, y esta prueba lo delata.
/// No necesita Docker.
/// </summary>
public sealed class AntiforgeryConventionTests(OfflineApiFixture api) : IClassFixture<OfflineApiFixture>
{
    private const string RogueApiRoute = "/api/tickets/rogue";
    private const string RogueHookRoute = "/hooks/rogue";

    /// <summary>
    /// Mutaciones exentas del filtro antiforgery, como firmas <c>MÉTODO /ruta</c>. Hoy no hay ninguna. Añadir
    /// una exige justificarla aquí (p. ej. un webhook que se autentica por firma y no usa la cookie de sesión).
    /// </summary>
    private static readonly string[] AntiforgeryExemptions = [];

    [Fact]
    public void Toda_mutacion_lleva_el_filtro_antiforgery()
    {
        foreach (var services in new[] { api.Development.Services, api.Production.Services })
        {
            var endpoints = EndpointCatalog.All(services);
            var mutations = endpoints
                .Where(EndpointCatalog.AcceptsMutation)
                .SelectMany(EndpointCatalog.MutationSignatures)
                .ToList();

            // Si el recorrido no viera las mutaciones conocidas, la regla pasaría sin mirar nada.
            Assert.Contains("POST /api/auth/login", mutations);
            Assert.Contains("POST /api/auth/logout", mutations);

            var offenders = EndpointCatalog.MutationsWithoutAntiforgery(endpoints, AntiforgeryExemptions);
            Assert.True(
                offenders.Count == 0,
                "Mapea estos endpoints dentro del grupo raíz /api (RequireAntiforgeryValidation) o, si de verdad no "
                + $"usan la cookie de sesión, decláralos en AntiforgeryExemptions: {string.Join(", ", offenders)}");

            // Una exención que ya no corresponde a ningún endpoint es una puerta abierta para el siguiente.
            var stale = AntiforgeryExemptions.Except(mutations, StringComparer.Ordinal).ToList();
            Assert.True(stale.Count == 0, $"Quita de AntiforgeryExemptions lo que ya no existe: {string.Join(", ", stale)}");
        }
    }

    [Fact]
    public void Una_mutacion_mapeada_fuera_del_grupo_api_se_detecta()
    {
        using var factory = api.Development.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddSingleton<IStartupFilter, RogueMutationEndpoints>()));
        var endpoints = EndpointCatalog.All(factory.Services);

        var offenders = EndpointCatalog.MutationsWithoutAntiforgery(endpoints, exemptions: []);

        // Dentro y fuera del prefijo /api: la regla no depende de la ruta.
        Assert.Equal(
            [
                $"* {RogueApiRoute}/any-method",
                "DELETE /rogue/{id}",
                $"POST {RogueApiRoute}",
                $"POST {RogueHookRoute}",
                $"PUT {RogueHookRoute}/mixed",
            ],
            offenders.Order(StringComparer.Ordinal));

        // Solo una exención explícita y exacta saca una mutación de la lista.
        var withExemption = EndpointCatalog.MutationsWithoutAntiforgery(endpoints, exemptions: [$"POST {RogueHookRoute}"]);
        Assert.Equal(offenders.Except([$"POST {RogueHookRoute}"]).Order(StringComparer.Ordinal), withExemption.Order(StringComparer.Ordinal));
    }

    /// <summary>Simula el descuido: mutaciones mapeadas fuera del grupo raíz, bajo <c>/api</c> y fuera de él.</summary>
    private sealed class RogueMutationEndpoints : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            next(app);

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost(RogueApiRoute, () => Results.NoContent());
                // Sin restricción de método también acepta POST.
                endpoints.Map($"{RogueApiRoute}/any-method", (HttpContext _) => Task.CompletedTask);
                // Un GET no es una mutación: no debe aparecer.
                endpoints.MapGet($"{RogueApiRoute}/read", () => Results.NoContent());

                // Fuera de /api (un webhook, una ruta suelta): también son mutaciones.
                endpoints.MapPost(RogueHookRoute, () => Results.NoContent());
                endpoints.MapDelete("/rogue/{id}", (string id) => Results.Ok(id));
                // De un endpoint con varios métodos solo cuentan los que mutan.
                endpoints.MapMethods($"{RogueHookRoute}/mixed", [HttpMethods.Get, HttpMethods.Put], () => Results.NoContent());
                endpoints.MapGet($"{RogueHookRoute}/read", () => Results.NoContent());
            });
        };
    }
}
