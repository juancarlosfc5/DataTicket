using DataTicket.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace DataTicket.IntegrationTests.Smoke;

/// <summary>
/// El host resuelve el <see cref="FakeTimeProvider"/> de la fixture: base de las pruebas con reloj
/// controlado (bloqueo, expiración de la cookie, sello de seguridad).
/// </summary>
[Trait(TestCategories.Name, TestCategories.RequiresDocker)]
public sealed class TestClockTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    [Fact]
    public void TimeProvider_de_DI_es_el_FakeTimeProvider_de_la_fixture()
    {
        var resolved = api.Factory.Services.GetRequiredService<TimeProvider>();

        Assert.Same(api.Time, resolved);
        Assert.IsType<FakeTimeProvider>(resolved);
    }

    [Fact]
    public void Avanzar_el_reloj_de_la_fixture_se_ve_desde_DI()
    {
        var resolved = api.Factory.Services.GetRequiredService<TimeProvider>();
        var before = resolved.GetUtcNow();

        api.Time.Advance(TimeSpan.FromMinutes(16));

        Assert.Equal(before + TimeSpan.FromMinutes(16), resolved.GetUtcNow());
    }
}
