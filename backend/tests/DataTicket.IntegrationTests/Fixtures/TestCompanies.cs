namespace DataTicket.IntegrationTests.Fixtures;

/// <summary>
/// Empresas sintéticas de <c>db.sql</c> §13.1: <c>c0000000-0000-7000-8000-0000000000NN</c>.
/// La 01 y la 02 son las empresas A y B de las pruebas de aislamiento (CA-01).
/// </summary>
public static class TestCompanies
{
    public static TestCompany Company01 { get; } = Create(1);

    public static TestCompany Company02 { get; } = Create(2);

    public static IReadOnlyList<TestCompany> All { get; } = [Company01, Company02];

    public static Guid IdOf(int number) => Guid.Parse($"c0000000-0000-7000-8000-{number:D12}");

    private static TestCompany Create(int number) =>
        new(IdOf(number), $"Empresa Sintética {number:D2}", IsActive: number <= 45);
}
