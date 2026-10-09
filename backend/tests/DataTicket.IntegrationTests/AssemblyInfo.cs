using DataTicket.IntegrationTests.Infrastructure;

// Un único contenedor de PostgreSQL por ejecución del ensamblado; se arranca de forma perezosa
// la primera vez que una prueba pide una base, así las pruebas sin Docker no lo necesitan.
[assembly: AssemblyFixture(typeof(PostgreSqlContainerFixture))]
