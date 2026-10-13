using System.Net;
using DataTicket.IntegrationTests.Infrastructure;
using DataTicket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace DataTicket.IntegrationTests.Smoke;

/// <summary>
/// Humo de la infraestructura: el host en memoria arranca contra la base aislada del contenedor.
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class HealthEndpointsTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task Health_live_responde_200()
    {
        using var client = api.Factory.CreateClient();

        using var response = await client.GetAsync("/api/health/live", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_responde_200_con_PostgreSQL_del_contenedor()
    {
        using var client = api.Factory.CreateClient();

        using var response = await client.GetAsync("/api/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("\"status\":\"Healthy\"", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task El_host_usa_la_base_aislada_y_no_la_del_Compose()
    {
        await using var scope = api.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataTicketDbContext>();

        var used = new NpgsqlConnectionStringBuilder(dbContext.Database.GetConnectionString());
        var expected = new NpgsqlConnectionStringBuilder(api.Database.ConnectionString);

        Assert.StartsWith("dataticket_test_", used.Database, StringComparison.Ordinal);
        Assert.Equal(expected.Database, used.Database);
        Assert.Equal(expected.Port, used.Port);
        Assert.NotEqual("dataticket", used.Database);
        Assert.True(
            await dbContext.Database.CanConnectAsync(TestContext.Current.CancellationToken),
            "La base aislada debe aceptar conexiones.");
    }
}
