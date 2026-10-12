using DataTicket.Api.Endpoints.Auth;
using DataTicket.Application.Ports.In.Authentication;
using DataTicket.Domain.Users;

namespace DataTicket.IntegrationTests.Auth;

/// <summary>
/// Hallazgo BAJO-6 de la revisión de B4: la superficie de <c>GET /api/auth/me</c> se traduce valor por valor.
/// Los literales <c>client</c> e <c>internal</c> son los del contrato T-01. No necesita Docker.
/// </summary>
public sealed class CurrentUserResponseTests
{
    private static readonly CurrentUserProfile Profile = new(
        Guid.Parse("a0000000-0000-7000-8000-000000000009"),
        "Administrador DataTicket",
        "admin@dataticket.local",
        Surface.Internal,
        [RoleNames.Administrator],
        [],
        Company: null);

    [Fact]
    public void Cada_superficie_conocida_tiene_su_literal_del_contrato()
    {
        var expected = new Dictionary<Surface, string> { [Surface.Client] = "client", [Surface.Internal] = "internal" };

        // Si se añade una superficie al dominio, esta prueba obliga a decidir su literal.
        Assert.Equal(Enum.GetValues<Surface>().Order(), expected.Keys.Order());
        foreach (var (surface, literal) in expected)
        {
            Assert.Equal(literal, CurrentUserResponse.From(Profile with { Surface = surface }).Surface);
        }
    }

    [Fact]
    public void Superficie_desconocida_no_se_serializa_como_internal()
    {
        var unknown = (Surface)99;
        Assert.False(Enum.IsDefined(unknown));

        // Falla cerrado: una superficie que el Api no conoce no se presenta como personal interno.
        Assert.Throws<ArgumentOutOfRangeException>(() => CurrentUserResponse.From(Profile with { Surface = unknown }));
    }
}
