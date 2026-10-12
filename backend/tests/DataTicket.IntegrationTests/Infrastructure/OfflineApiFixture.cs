using Microsoft.Extensions.Hosting;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Hosts del Api SIN base de datos (cadena de conexión que nunca se usa): sirven para inspeccionar el
/// composition root, las políticas y la tabla de endpoints, y para peticiones anónimas que no llegan a la
/// base. No necesita Docker, así que estas pruebas también corren en la etapa <c>test</c> del Dockerfile.
/// </summary>
public sealed class OfflineApiFixture : IAsyncDisposable
{
    // Solo satisface la validación de AddInfrastructure: ninguna prueba sin Docker abre conexión.
    public const string UnusedConnectionString = "Host=postgres.invalid;Database=unused;Username=unused";

    private readonly Lazy<DataTicketApiFactory> _development = new(() => Create(Environments.Development));
    private readonly Lazy<DataTicketApiFactory> _production = new(() => Create(Environments.Production));

    public DataTicketApiFactory Development => _development.Value;

    public DataTicketApiFactory Production => _production.Value;

    public static DataTicketApiFactory Create(string environment) =>
        new(new ApiFactoryOptions
        {
            ConnectionString = UnusedConnectionString,
            Environment = environment,
            TimeProvider = TestClock.Create(),
        });

    public async ValueTask DisposeAsync()
    {
        if (_development.IsValueCreated)
        {
            await _development.Value.DisposeAsync();
        }

        if (_production.IsValueCreated)
        {
            await _production.Value.DisposeAsync();
        }
    }
}
