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

Versiones verificadas en NuGet (flatcontainer index.json, solo estables) el 2026-10-09: Testcontainers.PostgreSql 4.16.0 (net8/9/10), Microsoft.AspNetCore.Mvc.Testing 10.0.12, Microsoft.Extensions.TimeProvider.Testing 10.10.0, xunit.v3 4.0.1. SDK local 10.0.201, runtime ASP.NET Core local 10.0.5.

**Why:** evitar repetir los diagnósticos; **How to apply:** al añadir pruebas de integración o tocar la etapa test del Dockerfile. Ver [[aspnetcore10-timeprovider-identity]].
