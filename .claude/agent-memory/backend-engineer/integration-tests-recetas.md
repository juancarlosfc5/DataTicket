---
name: integration-tests-recetas
description: Trampas comprobadas al montar DataTicket.IntegrationTests (Testcontainers, WebApplicationFactory, MTP, docker build) y versiones verificadas de paquetes de prueba
metadata:
  type: project
---

Comprobado el 2026-10-09 (B2 del goal del login):

- `new PostgreSqlBuilder(image).Build()` (Testcontainers 4.16) ya valida la conexión a Docker y lanza `DockerUnavailableException`; si se construye en el constructor de una fixture de ensamblado, fallan TODAS las pruebas del ensamblado aunque no usen la base. Construir y arrancar de forma perezosa.
- Dentro de `docker build --target test` no hay Docker: las pruebas con PostgreSQL llevan `[Trait("Category","RequiresDocker")]` y la etapa usa `dotnet test --solution DataTicket.slnx --filter-not-trait "Category=RequiresDocker"` (en modo MTP el filtro se reenvía sin `--`).
- .NET 10 genera `public partial class Program` para las top-level statements: `WebApplicationFactory<Program>` compila sin declararlo.
- `builder.UseSetting("ConnectionStrings:DataTicket", ...)` en la factory gana sobre `appsettings.Development.json` y llega antes de que `Program` lea la configuración. Hay una prueba que lo vigila (el Compose publica 5432 en localhost: si el override fallara, las pruebas tocarían la base del Compose).
- `MigrateAsync` sin migraciones registra un `fail` de EF (SELECT a `__EFMigrationsHistory`) que es ruido, no error.
- En Git Bash de esta máquina, invocar `python` colgó la shell (2 min): usar `perl -0pi -e` o Edit.

Comprobado el 2026-10-12 (correcciones de la revisión de B3):

- Para sustituir o envolver un servicio en una sola prueba: `api.Factory.WithWebHostBuilder(b => b.ConfigureTestServices(...))` crea otro host sobre la misma base y el MISMO `FakeTimeProvider` (hereda `ConfigureWebHost` de la factory). Así se cuenta el uso del `IPasswordHasher` sin cronómetros (`PasswordHasherSpy`).
- Pruebas que caracterizan código ya correcto (políticas de cookie, constantes del contrato, guardas de entorno): el rojo se demuestra con una mutación temporal del código de producción (copia previa en el scratchpad y `cmp` al restaurar), no solo viendo el verde.
- Para distinguir `CookieSecurePolicy.Always` de `SameAsRequest` hay que afirmar las opciones (`IOptionsMonitor<CookieAuthenticationOptions>.Get(IdentityConstants.ApplicationScheme)`, `IOptions<AntiforgeryOptions>`): sobre HTTPS ambas emiten la misma cabecera, y con `Always` el antiforgery lanza (500) si la petición llega por HTTP.
- `TestBrowser` toma el `TimeProvider` del host: con el reloj real descartaba las cookies con `Expires` (el reloj falso arranca en el pasado).
- Un proyecto de pruebas xunit.v3 (Exe) puede referenciar a otro con `ProjectReference` sin avisos ni descubrimiento duplicado (`ArchitectureTests` → `UnitTests`).
- Ejecutar una sola clase: `dotnet test --project tests/DataTicket.IntegrationTests --no-build --filter-class "<nombre completo>"`.
- `perl -0pi -e` con rutas de Windows (`..\..\`) en el patrón falla en silencio: para `.csproj` usar Edit.

Comprobado el 2026-10-12 (B4, HU-004):

- Un host SIN base (`OfflineApiFixture`, cadena de conexión inservible) arranca, expone `EndpointDataSource`, `IAuthorizationService` y atiende peticiones anónimas (401/404): las pruebas de convención y de políticas no necesitan Docker.
- Los `TestEndpoints` viven en un SEGUNDO pipeline (tras `next(app)`): aparecen en el servicio `EndpointDataSource` y en el documento OpenAPI. `No_existe_ruta_register` prohíbe las subcadenas `register`, `manage`, `confirmEmail` y `refresh` en ese documento, y `ProductManager` contiene «manage»: los endpoints de control llevan `.ExcludeFromDescription()` y los resúmenes de OpenAPI no nombran ese rol.
- xunit.v3 en MTP: `--filter-method "*.NombreA" "*.NombreB"` y `--filter-class "A" "B"` admiten varios valores; `--output Detailed` lista también las pruebas correctas.
- `JsonObject.ToJsonString()` escapa los caracteres no ASCII (`é` → `é`): un `DoesNotContain("Sintética 02", json)` pasa siempre. Comparar valores con `GetValue<string>()`.
- Cobertura sin ensuciar el repo: `dotnet test --no-build --coverage --coverage-output-format cobertura --results-directory <scratchpad>`. El 64 % de `DataTicket.Api` es código generado de OpenAPI; las clases propias se leen por `<class>`.

Comprobado el 2026-10-12 (B7, sembrador de Development):

- El código de `Program` posterior a `builder.Build()` (migración, sembrador) SÍ se ejecuta en `WebApplicationFactory`, al arrancar el host (primer acceso a `factory.Services`). Una excepción ahí sale por `factory.Services` (basta `Assert.ThrowsAny` y recorrer `InnerException`).
- Los hosts de prueba en Development leen `appsettings.Development.json` del Api: una clave activada allí (p. ej. `Seed:Enabled`) afecta a TODAS las fixtures. `DataTicketApiFactory` la fuerza a `false` con `UseSetting` por defecto (`ApiFactoryOptions.SeedEnabled`); si no, el sembrador chocaría con `TestDataSeeder` (mismos IDs) y los hosts sin base fallarían al arrancar.
- Una clase de pruebas puede recibir el `PostgreSqlContainerFixture` (fixture de ensamblado) directamente en el constructor y pedir una base vacía por prueba; para hosts que no son de Development, migrar antes con un host Development desechable.
- `UserManager.CreateAsync`/`AddToRolesAsync` participan en una transacción abierta con `dbContext.Database.BeginTransactionAsync` (mismo `DbContext` con ámbito).
- Mutación con código inalcanzable (`&& false`): compilar con `dotnet build -p:TreatWarningsAsErrors=false`; `-warnaserror-` no existe en `dotnet build` (MSB1001) y, si el build falla, `--no-build` ejecuta los binarios viejos y da un verde falso. Comprobar siempre la línea «0 Errores» antes de leer el resultado.
- `IPasswordHasher<ApplicationUser>` es de ámbito: resolverlo desde un `CreateAsyncScope()`, no desde el proveedor raíz.

Versiones verificadas en NuGet (flatcontainer index.json, solo estables) el 2026-10-09: Testcontainers.PostgreSql 4.16.0 (net8/9/10), Microsoft.AspNetCore.Mvc.Testing 10.0.12, Microsoft.Extensions.TimeProvider.Testing 10.10.0, xunit.v3 4.0.1. SDK local 10.0.201, runtime ASP.NET Core local 10.0.5.

**Why:** evitar repetir los diagnósticos; **How to apply:** al añadir pruebas de integración o tocar la etapa test del Dockerfile. Ver [[aspnetcore10-timeprovider-identity]].

Comprobado el 2026-10-14 (HU-011, worktree `DataGlobal2-core`):

- Esta máquina solo tiene SDK 9: build, `dotnet ef` y pruebas en un contenedor desechable `mcr.microsoft.com/dotnet/sdk:10.0` montando `backend/` en `/src` (con `-v /var/run/docker.sock:/var/run/docker.sock -e TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal` para Testcontainers y `MSYS_NO_PATHCONV=1`). El build da 20 avisos NU1903 (`System.Security.Cryptography.Xml` 10.0.12), que no son error por `WarningsNotAsErrors`.
- Rojo de pruebas de integración sin cambiar el modelo: añadir el `DbSet<T>` y, temporalmente, `modelBuilder.Ignore<T>()`. Sin el `Ignore`, el `DbSet` entra en el modelo, `MigrateAsync` lanzaría `PendingModelChangesWarning` (comportamiento documentado de EF 9+, no reproducido aquí) y fallarían TODAS las pruebas por la razón equivocada. Con el `Ignore` el rojo fue el correcto (servicio `IAuditLog` ausente, tabla inexistente).
- Probar el `Down` de una migración: `dbContext.GetService<IMigrator>().MigrateAsync(<id anterior>)` sobre una base propia, afirmar que no quedan objetos y volver a `MigrateAsync()`.
- Un heredoc largo de Git Bash con varios archivos C# falló con «unexpected EOF while looking for matching `''» sin escribir nada: para archivos nuevos usar Write.
- Un `.cs` compartido entre proyectos de prueba se enlaza con `<Compile Include="../Shared/…" Link="…" />` y la clase `internal` (no choca aunque `ArchitectureTests` referencie `UnitTests`).
