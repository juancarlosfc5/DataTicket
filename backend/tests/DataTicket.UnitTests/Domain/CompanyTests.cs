using DataTicket.Domain.Companies;

namespace DataTicket.UnitTests.Domain;

public sealed class CompanyTests
{
    private static readonly Guid Id = Guid.Parse("c0000000-0000-7000-8000-000000000001");

    [Fact]
    public void Nueva_empresa_queda_activa_con_nombre_recortado_y_fechas_en_UTC_a_microsegundos()
    {
        var createdAt = new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.FromHours(-5)).AddTicks(1234567);

        var company = new Company(Id, "  Empresa Sintética 01 ", createdAt);

        Assert.Equal("Empresa Sintética 01", company.Name);
        Assert.True(company.IsActive);
        Assert.Null(company.DeactivatedAt);
        Assert.Equal(TimeSpan.Zero, company.CreatedAt.Offset);
        Assert.Equal(new DateTimeOffset(2026, 9, 1, 13, 0, 0, TimeSpan.Zero).AddTicks(1234560), company.CreatedAt);
        Assert.Equal(company.CreatedAt, company.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Nombre_vacio_se_rechaza(string name)
    {
        Assert.Throws<ArgumentException>(() => new Company(Id, name, DateTimeOffset.UnixEpoch));
    }

    [Fact]
    public void Nombre_de_mas_de_150_caracteres_se_rechaza()
    {
        Assert.Throws<ArgumentException>(() => new Company(Id, new string('a', 151), DateTimeOffset.UnixEpoch));
        Assert.Equal(150, new Company(Id, new string('a', 150), DateTimeOffset.UnixEpoch).Name.Length);
    }

    [Fact]
    public void Identificador_vacio_se_rechaza()
    {
        Assert.Throws<ArgumentException>(() => new Company(Guid.Empty, "Empresa", DateTimeOffset.UnixEpoch));
    }
}
