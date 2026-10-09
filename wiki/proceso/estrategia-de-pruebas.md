---
title: Estrategia de pruebas
type: guia
status: vigente
tags: [proceso, pruebas]
sources: ["PRD.md §14", "backend/tests", "frontend/src/**/*.test.ts"]
aliases: [Pruebas, Testing, TDD]
created: 2026-10-07
updated: 2026-10-09
---

# Estrategia de pruebas

Desarrollo guiado por pruebas: primero la prueba que falla, luego la implementación mínima, luego la refactorización. Objetivo de cobertura: 80 % en dominio, casos de uso y modelos/controladores del frontend. Cada criterio de aceptación del PRD debe acabar respaldado por al menos una prueba automática ([[criterios-de-aceptacion]]).

## Backend (.NET 10)

| Nivel | Herramientas | Qué cubre |
|---|---|---|
| Arquitectura | `DataTicket.ArchitectureTests` (xUnit v3) | Regla de dependencias hexagonal (5 pruebas, en verde) |
| Unitarias | xUnit v3 + dobles de los puertos | Invariantes de dominio y casos de uso, incluida la autorización negativa |
| Integración | `DataTicket.IntegrationTests`: `WebApplicationFactory<Program>` + Testcontainers `postgres:18.6` (nunca el `db` de Compose) | Endpoints, EF Core, filtros multiempresa (CA-01), hub SignalR |

- Runner: **Microsoft.Testing.Platform**, activado en `backend/global.json` (`test.runner`). Por eso `dotnet` se ejecuta **dentro de `backend/`**: `cd backend && dotnet test`. Desde la raíz no se encuentra `global.json`, el SDK cae en modo VSTest y el comando falla (comprobado el 2026-10-07).
- Cobertura: `cd backend && dotnet test --coverage --coverage-output-format cobertura` (extensión `Microsoft.Testing.Extensions.CodeCoverage`; reporte en `backend/TestResults/`, ignorado por git).
- Sin SDK local: `docker build --target test ./backend` (solo las pruebas sin Docker; ver abajo).

### Pruebas de integración (`DataTicket.IntegrationTests`, desde 2026-10-09)

- **Aislamiento**: un contenedor `postgres:18.6` por ejecución, arrancado cuando la primera prueba lo necesita; una base `dataticket_test_<guid>` por clase (`IClassFixture<ApiFixture>`), migrada con `MigrateAsync` (nunca `EnsureCreated`) y borrada con `DROP DATABASE … WITH (FORCE)`. Una prueba comprueba que el host no apunta al `db` de Compose.
- **Rasgo `Category=RequiresDocker`**: obligatorio en toda clase que use PostgreSQL (lo vigila una prueba de convención). La etapa `test` del `backend/Dockerfile` lo excluye porque dentro de `docker build` no hay Docker; las integraciones completas corren con `cd backend && dotnet test` en una máquina con Docker y, en CI, en un runner con Docker ([[hu-001-integracion-continua]]).
- **Cuentas**: catálogo inmutable `TestAccounts` / `TestCompanies` / `TestRoles` que replica `db.sql` §13.1–13.4 (IDs, correos, estados y roles); la contraseña sintética es una constante del proyecto de pruebas.
- **Reloj**: `TimeProvider` registrado en el composition root y sustituido por `FakeTimeProvider` (inicio 2026-10-09 13:00 UTC; `api.Time.Advance(...)`). Respetan ese reloj la cookie de autenticación y el `SecurityStampValidator`; **no** lo respetan el bloqueo de `UserManager` (`DateTimeOffset.UtcNow`) ni los tokens de `DataProtectorTokenProvider` (afecta a HU-005/HU-006). Fuente: código de `dotnet/aspnetcore` `release/10.0`.
- **Reinicio del backend**: otra `WebApplicationFactory` con la misma cadena de conexión.

## Frontend (React 19)

jsdom se activa por archivo (`// @vitest-environment jsdom` + `import '…/test/domSetup'`); los modelos siguen en `node`. Tras B5 del goal del login hay 89 pruebas Vitest.

| Nivel | Herramientas | Qué cubre |
|---|---|---|
| Estático | `oxlint` (incluye fronteras MVC) + `tsc -b` | Tipos y capas |
| Unitarias | Vitest | Modelos (normalización, reglas) y controladores |
| Componentes | Testing Library + jsdom (desde 2026-10-09) | Vistas con interacción; `app/` monta `<MemoryRouter><App/></MemoryRouter>` con un doble de `fetch` que falla ante rutas no previstas |
| E2E | Playwright (por añadir) | Flujos críticos: radicar, triage, chat, respuesta formal |

## Pruebas imprescindibles por invariante

| Invariante | Prueba |
|---|---|
| Aislamiento multiempresa (CA-01, CA-02) | Integración: usuario de empresa A recibe 404/403 sobre ticket de B |
| Chat solo para participantes (CA-06, CA-08) | Integración del hub: no participante no se une ni envía; retirado pierde la conexión |
| Persistir antes de publicar (CA-05) | Unitaria del caso de uso: el notificador no se llama si falla la persistencia |
| Respuesta formal y cierre solo PM/admin (CA-11, CA-12) | Unitaria + integración por rol |
| Adjuntos (CA-09) | Rechazo de tipo/tamaño/firma inválidos en backend |
| Auditoría (CA-14) | Cada caso de uso relevante deja `AuditEntry` con actor y fecha |

## Relacionado

- [[backend-hexagonal]] · [[frontend-mvc]] · [[criterios-de-aceptacion]] · [[flujo-de-trabajo-github]]
