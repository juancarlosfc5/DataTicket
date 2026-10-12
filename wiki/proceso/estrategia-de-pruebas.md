---
title: Estrategia de pruebas
type: guia
status: vigente
tags: [proceso, pruebas]
sources: ["PRD.md §14", "backend/tests", "frontend/src/**/*.test.ts"]
aliases: [Pruebas, Testing, TDD]
created: 2026-10-07
updated: 2026-10-12
---

# Estrategia de pruebas

Desarrollo guiado por pruebas: primero la prueba que falla, luego la implementación mínima, luego la refactorización. Objetivo de cobertura: 80 % en dominio, casos de uso y modelos/controladores del frontend. Cada criterio de aceptación del PRD debe acabar respaldado por al menos una prueba automática ([[criterios-de-aceptacion]]).

## Backend (.NET 10)

| Nivel | Herramientas | Qué cubre |
|---|---|---|
| Arquitectura | `DataTicket.ArchitectureTests` (xUnit v3) | Regla de dependencias hexagonal (8 pruebas, en verde) |
| Unitarias | `DataTicket.UnitTests` (xUnit v3 + dobles de los puertos; desde B3) | Invariantes de dominio y casos de uso, incluida la autorización negativa |
| Integración | `DataTicket.IntegrationTests`: `WebApplicationFactory<Program>` + Testcontainers `postgres:18.6` (nunca el `db` de Compose) | Endpoints, EF Core, filtros multiempresa (CA-01), hub SignalR |

- Runner: **Microsoft.Testing.Platform**, activado en `backend/global.json` (`test.runner`). Por eso `dotnet` se ejecuta **dentro de `backend/`**: `cd backend && dotnet test`. Desde la raíz no se encuentra `global.json`, el SDK cae en modo VSTest y el comando falla (comprobado el 2026-10-07).
- Cobertura: `cd backend && dotnet test --coverage --coverage-output-format cobertura` (extensión `Microsoft.Testing.Extensions.CodeCoverage`; reporte en `backend/TestResults/`, ignorado por git).
- Sin SDK local: `docker build --target test ./backend` (solo las pruebas sin Docker; ver abajo).

### Estado tras B3 del goal del login (2026-10-12)

`cd backend && dotnet test`: 96 pruebas en verde; 43 no necesitan Docker (las que ejecuta la etapa `test` del `Dockerfile`, que excluye `Category=RequiresDocker`).

- **Contar en vez de cronometrar**: `PasswordHasherSpy` decora el `IPasswordHasher` con `WithWebHostBuilder` y `LoginHashingCostTests` afirma una sola verificación de hash por intento de login (correo inexistente, `Invited` sin clave, bloqueada, clave incorrecta y login válido).
- **`TestBrowser` usa el reloj del host** (`FakeTimeProvider`), así que las cookies con `Expires` se evalúan con el mismo reloj que las emite.
- **Rojo de las pruebas de caracterización**: cuando el código ya era correcto y faltaba la prueba, el rojo se demuestra con una mutación temporal del código de producción (se ve fallar y se restaura). El rojo de las 11 pruebas del DoD-02 de HU-003 se comprobó a posteriori: sin `api.MapAuthEndpoints()` fallan 19 de 70.
- **Contrato con aserciones propias** (`SessionContractTests`): expiración deslizante de 8 h, contraseña ≥ 12, bloqueo 5 × 15 min y sello cada 5 min.

### Pruebas unitarias (`DataTicket.UnitTests`, desde 2026-10-12)

Proyecto creado en B3 del goal del login. Referencia **solo** `DataTicket.Domain` y `DataTicket.Application`: ni EF Core, ni ASP.NET Core, ni Docker, ni base de datos. Los puertos de salida se sustituyen por dobles escritos a mano dentro de la propia clase de prueba. Corre también en la etapa `test` del Dockerfile.

| Clase | Qué cubre |
|---|---|
| `Domain/AccountAccessTests` | «Perfiles válidos» de [[autenticacion-identity]]: superficie cliente o interna, y fallo cerrado para cliente sin empresa, interno con empresa, perfil mixto, sin roles o con roles desconocidos; solo una cuenta `Active` con perfil válido puede tener sesión |
| `Domain/CompanyTests` | `Company`: nombre recortado y limitado a 150 caracteres, identificador obligatorio, alta activa y fechas en UTC truncadas a microsegundos |
| `Application/SignInTests` | Casos de uso `SignIn` y `SignOut` con dobles de `ICredentialVerifier` e `IUserSession`: no se emite sesión si las credenciales no se verifican, si la cuenta no está `Active` o si el perfil no es válido |

### Pruebas de integración (`DataTicket.IntegrationTests`, desde 2026-10-09)

- **Aislamiento**: un contenedor `postgres:18.6` por ejecución, arrancado cuando la primera prueba lo necesita; una base `dataticket_test_<guid>` por clase (`IClassFixture<ApiFixture>`), migrada con `MigrateAsync` (nunca `EnsureCreated`) y borrada con `DROP DATABASE … WITH (FORCE)`. Una prueba comprueba que el host no apunta al `db` de Compose.
- **Estado compartido por clase** (desde B3): `ApiFixture` crea la base, arranca un host en Development que migra al arrancar y la siembra con `TestDataSeeder`. Host, base y reloj se comparten entre las pruebas de la clase. Una prueba que avanza el reloj o cambia el estado de una cuenta vive en su propia clase (`LockoutTests`, `SecurityStampTests`) o crea otro host con `ApiFixture.CreateFactory(...)`.
- **Rasgo `Category=RequiresDocker`**: obligatorio en toda clase que use PostgreSQL (lo vigila una prueba de convención). La etapa `test` del `backend/Dockerfile` lo excluye porque dentro de `docker build` no hay Docker; las integraciones completas corren con `cd backend && dotnet test` en una máquina con Docker y, en CI, en un runner con Docker ([[hu-001-integracion-continua]]).
- **Cuentas**: catálogo inmutable `TestAccounts` / `TestCompanies` / `TestRoles` que replica `db.sql` §13.1–13.4 (IDs, correos, estados y roles); la contraseña sintética es una constante del proyecto de pruebas.
- **Reloj**: `TimeProvider` registrado en el composition root y sustituido por `FakeTimeProvider` (inicio 2026-10-09 13:00 UTC; `api.Time.Advance(...)`). Respetan ese reloj la cookie de autenticación, el `SecurityStampValidator` y, desde B3, el bloqueo: el `UserManager` estándar usa `DateTimeOffset.UtcNow`, y `DataTicketUserManager` lo redefine con el `TimeProvider` ([[autenticacion-identity]]). **No** lo respetan los tokens de `DataProtectorTokenProvider` (afecta a HU-005/HU-006). Fuente: código de `dotnet/aspnetcore` `release/10.0`.
- **Reinicio del backend**: otra `WebApplicationFactory` con la misma cadena de conexión.
- **Etapa `test` del `backend/Dockerfile`**: ejecuta `dotnet test --solution DataTicket.slnx --filter-not-trait "Category=RequiresDocker"`. Corren ahí las pruebas de arquitectura, las unitarias y las clases de integración que no arrancan PostgreSQL (catálogo de cuentas, convención del rasgo, composition root).

Utilidades del proyecto (`Infrastructure/` y `Fixtures/`, desde B3):

| Utilidad | Para qué sirve |
|---|---|
| `TestBrowser` | Cliente HTTP que se comporta como el navegador del SPA: guarda y envía las cookies a mano (así cada `Set-Cookie` es visible y una cookie se puede trasladar a otro host), no sigue redirecciones, pide el token antiforgery y lo adjunta en `X-XSRF-TOKEN`. Dirección base `https://localhost` por defecto; `http://localhost` para comprobar Development |
| `TestEndpoints` | Rutas que solo existen en el host de pruebas (`/api/test/authenticated`, `/api/test/administrator-only`), añadidas con un `IStartupFilter` detrás del pipeline del Api. Permiten observar la sesión y los `401`/`403` mientras no hay rutas protegidas de producto; el requisito de rol es ad hoc hasta que B4 traiga las políticas con nombre |
| `TestDataSeeder` | Persiste el catálogo `TestCompanies` / `TestAccounts` con EF Core y `UserManager`, como hará el sembrador de Development ([[hu-010-datos-sinteticos-de-desarrollo]]). Las cuentas `Invited` quedan sin contraseña. `CreateAsync` sirve para cuentas ad hoc de una prueba |
| `ProblemResponse` | Aserciones sobre ProblemDetails: estado, `Content-Type` `application/problem+json`, título y ausencia de `Location`; compara cuerpos sin `traceId` para demostrar que dos respuestas son idénticas |

Qué cubre cada clase de `Auth/` y `Persistence/` (HU-003; CHU = criterio de la historia):

| Clase | Qué cubre |
|---|---|
| `Auth/LoginTests` | CHU-01, CHU-02 (Development), CHU-03, CHU-04, CHU-06. Login válido: `204` y cookie `HttpOnly`, `SameSite=Lax`, `Path=/`, sin `Secure` sobre `http://localhost`. Correo inexistente y clave incorrecta: mismo `401` (JSON idéntico salvo `traceId`) y sin cookie. Cuentas `Deactivated` e `Invited` y perfil mixto: `401` genérico. Validación: correo vacío, sin formato o de más de 256 caracteres y contraseña vacía dan `400` con `errors`; 256 exactos pasa la validación |
| `Auth/LockoutTests` | CHU-05. El quinto fallo bloquea 15 minutos según el reloj de la aplicación; con la clave correcta la respuesta sigue siendo el `401` genérico; al vencer el bloqueo la cuenta entra y el contador queda en cero. Cuatro fallos no bloquean y un acierto reinicia el contador |
| `Auth/SecurityStampTests` | P-13. Una cuenta desactivada sin tocar su sello conserva la sesión hasta la revalidación (5 min) y la pierde después; una sesión activa sigue válida tras revalidar |
| `Auth/AntiforgeryTests` | CHU-07. Mutación sin cabecera o con el token de otro navegador: `400` con `code: "antiforgery_invalid"` y sin `errors`. El token pedido antes del login no sirve para el logout; el pedido después sí. El endpoint entrega el token con `no-store` y la cookie `HttpOnly` y `SameSite=Strict` |
| `Auth/LogoutTests` | CHU-08. `204`, cookie de sesión expirada con `Path=/` y `401` en la siguiente petición |
| `Auth/AuthorizationResponseTests` | CHU-10. Sin sesión: `401` en ProblemDetails y sin `Location` (también en el logout, porque la autorización actúa antes que el filtro antiforgery). Con sesión y sin el rol exigido: `403` |
| `Auth/RegisterRouteTests` | CHU-09. Las rutas `register` responden `404` y el documento OpenAPI no contiene `register`, `confirmEmail`, `manage` ni `refresh` |
| `Auth/SecureCookieTests` | CHU-02. En un host de otro entorno («Staging») sobre HTTPS, la cookie de sesión y la de antiforgery llevan `Secure` |
| `Auth/DataProtectionKeysTests` | CHU-12. La cookie emitida por un host la acepta otro host nuevo sobre la misma base: las claves están en `identity.data_protection_keys` |
| `Auth/PasswordHashTests` | CHU-13. `password_hash` leído directamente de la tabla: formato V3 de Identity, nunca la contraseña en claro |
| `Persistence/SchemaTests` | La migración crea las tablas, restricciones, índices, valores por defecto y roles de `db.sql` §2, §3 y §12, y un host en Development migra al arrancar ([[persistencia-postgresql]]) |

## Frontend (React 19)

jsdom se activa por archivo (`// @vitest-environment jsdom` + `import '…/test/domSetup'`); los modelos siguen en `node`. Tras B6 del goal del login hay 147 pruebas Vitest en 17 archivos; las de `app/` cubren login, logout, la carrera de `/me` y los intentos de saltar la guarda con rutas codificadas. Los controladores se prueban con `renderHook` en jsdom.

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

- [[backend-hexagonal]] · [[frontend-mvc]] · [[criterios-de-aceptacion]] · [[flujo-de-trabajo-github]] · [[autenticacion-identity]] · [[persistencia-postgresql]]
