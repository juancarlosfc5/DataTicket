namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>
/// Rasgos (traits) de xUnit usados para filtrar pruebas.
/// </summary>
public static class TestCategories
{
    public const string Name = "Category";

    /// <summary>
    /// La prueba arranca PostgreSQL con Testcontainers. La etapa <c>test</c> del Dockerfile las excluye con
    /// <c>--filter-not-trait "Category=RequiresDocker"</c> porque dentro de <c>docker build</c> no hay Docker.
    /// </summary>
    public const string RequiresDocker = "RequiresDocker";
}
