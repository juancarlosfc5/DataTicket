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
| Arquitectura | `DataTicket.ArchitectureTests` (xUnit v3) | Regla de dependencias hexagonal, ubicación de los puertos y, desde B4, el puerto `ICurrentUser` y su adaptador (reglas en [[backend-hexagonal]]) |
| Unitarias | `DataTicket.UnitTests` (xUnit v3 + dobles de los puertos; desde B3) | Invariantes de dominio y casos de uso, incluida la autorización negativa |
| Integración | `DataTicket.IntegrationTests`: `WebApplicationFactory<Program>` + Testcontainers `postgres:18.6` (nunca el `db` de Compose) | Endpoints, EF Core, filtros multiempresa (CA-01), hub SignalR |

- Runner: **Microsoft.Testing.Platform**, activado en `backend/global.json` (`test.runner`). Por eso `dotnet` se ejecuta **dentro de `backend/`**: `cd backend && dotnet test`. Desde la raíz no se encuentra `global.json`, el SDK cae en modo VSTest y el comando falla (comprobado el 2026-10-07).
- Cobertura: `cd backend && dotnet test --coverage --coverage-output-format cobertura` (extensión `Microsoft.Testing.Extensions.CodeCoverage`; reporte en `backend/TestResults/`, ignorado por git).
- Sin SDK local: `docker build --target test ./backend` (solo las pruebas sin Docker; ver abajo).

### Estado tras B7 del goal del login (2026-10-12)

`cd backend && dotnet test`: 169 pruebas en verde; 87 no necesitan Docker.

- **El contenedor de Testcontainers usa el mismo usuario y base que el Compose (`dataticket`)**, a propósito: con el usuario `postgres` el esquema actual era siempre `public` y la suite no veía el fallo del historial de migraciones que sí ocurría en el stack. `CreateDatabaseAsync(searchPath, ct)` permite fijar el `search_path` de una base de prueba.
- **`MigrationHistoryTests`**: segundo arranque sobre una base migrada y recuperación con un historial vacío sobrante.
- **Sembrador** (`Seeding/`): una base vacía por prueba; cubre creación, idempotencia, reposición, negativa fuera de Development (Staging y Production con la clave activa), fallo claro si falta la contraseña y ausencia de la contraseña en el log (`CapturingLoggerProvider`). Los hosts de prueba fuerzan `Seed:Enabled = false` por defecto (`ApiFactoryOptions.SeedEnabled`) para no chocar con `TestDataSeeder`.
- **Lección**: una comprobación en el stack real (dos arranques seguidos) encontró lo que ninguna prueba ni revisión había visto; por eso B7 exige reconstruir y probar con `curl` y navegador.

### Estado tras B4 del goal del login (2026-10-12)

`cd backend && dotnet test`: 154 pruebas en verde; 84 no necesitan Docker (`OfflineApiFixture` permite probar políticas y convenciones sin base). `DataTicket.ArchitectureTests`: 13 pruebas.

- **Convención antiforgery para toda mutación**: `Toda_mutacion_lleva_el_filtro_antiforgery` recorre todos los endpoints, de cualquier prefijo, con una lista de exenciones explícita (hoy vacía); una exención que ya no corresponde a ningún endpoint también falla.
- **Políticas registradas, no supuestas**: las pruebas comparan lo registrado en `AuthorizationOptions` (`RegisteredPolicies`) con las cuatro políticas del contrato, en Development y Production.
- **Todo contenido es un endpoint**: `Anonimo_nunca_recibe_2xx_fuera_de_la_lista_publica` y `Api_does_not_serve_content_through_endpointless_middleware`.
- **`TestEndpoints` es un segundo pipeline** del host de pruebas, con enrutamiento, autenticación y autorización propios: comparte los servicios de DI del Api (políticas, manejadores, eventos de la cookie), no sus middleware, y el filtro del grupo `/api` no lo alcanza.

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
| `Application/GetCurrentUserProfileTests` (B4) | Caso de uso `GetCurrentUserProfile` con dobles de `ICurrentUser` e `ICompanyDirectory`: el cliente recibe su perfil con el nombre de su empresa; el interno, sin empresa y sin consultar el directorio; sin nombre visible se usa el correo. Falla cerrado (sin perfil) sin sesión, sin superficie, cliente sin empresa, interno con empresa, sin correo o si la empresa no existe |

### Pruebas de integración (`DataTicket.IntegrationTests`, desde 2026-10-09)

- **Aislamiento**: un contenedor `postgres:18.6` por ejecución, arrancado cuando la primera prueba lo necesita; una base `dataticket_test_<guid>` por clase (`IClassFixture<ApiFixture>`), migrada con `MigrateAsync` (nunca `EnsureCreated`) y borrada con `DROP DATABASE … WITH (FORCE)`. Una prueba comprueba que el host no apunta al `db` de Compose.
- **Estado compartido por clase** (desde B3): `ApiFixture` crea la base, arranca un host en Development que migra al arrancar y la siembra con `TestDataSeeder`. Host, base y reloj se comparten entre las pruebas de la clase. Una prueba que avanza el reloj o cambia el estado de una cuenta vive en su propia clase (`LockoutTests`, `SecurityStampTests`) o crea otro host con `ApiFixture.CreateFactory(...)`.
- **Rasgo `Category=RequiresDocker`**: obligatorio en toda clase que use PostgreSQL (lo vigila una prueba de convención). La etapa `test` del `backend/Dockerfile` lo excluye porque dentro de `docker build` no hay Docker; las integraciones completas corren con `cd backend && dotnet test` en una máquina con Docker y, en CI, en un runner con Docker ([[hu-001-integracion-continua]]).
- **Cuentas**: catálogo inmutable `TestAccounts` / `TestCompanies` / `TestRoles` que replica `db.sql` §13.1–13.4 (IDs, correos, estados y roles); la contraseña sintética es una constante del proyecto de pruebas.
- **Reloj**: `TimeProvider` registrado en el composition root y sustituido por `FakeTimeProvider` (inicio 2026-10-09 13:00 UTC; `api.Time.Advance(...)`). Respetan ese reloj la cookie de autenticación, el `SecurityStampValidator` y, desde B3, el bloqueo: el `UserManager` estándar usa `DateTimeOffset.UtcNow`, y `DataTicketUserManager` lo redefine con el `TimeProvider` ([[autenticacion-identity]]). **No** lo respetan los tokens de `DataProtectorTokenProvider` (afecta a HU-005/HU-006). Fuente: código de `dotnet/aspnetcore` `release/10.0`.
- **Reinicio del backend**: otra `WebApplicationFactory` con la misma cadena de conexión.
- **Hosts sin base de datos** (`OfflineApiFixture`, desde B4): arranca el Api real con una cadena de conexión que nunca se usa (`Host=postgres.invalid…`), en dos hosts perezosos, `Development` y `Production`. Sirve para inspeccionar el composition root, las políticas y la tabla de endpoints, y para hacer peticiones **sin sesión**, que no llegan a la base. No necesita Docker ni lleva el rasgo `RequiresDocker`: la convención del rasgo solo vigila `ApiFixture` y `PostgreSqlContainerFixture`. Lo que necesite una sesión real (login) sigue yendo con `ApiFixture`.
- **Etapa `test` del `backend/Dockerfile`**: ejecuta `dotnet test --solution DataTicket.slnx --filter-not-trait "Category=RequiresDocker"`. Corren ahí las pruebas de arquitectura, las unitarias y las clases de integración que no arrancan PostgreSQL: catálogo de cuentas, convención del rasgo, composition root y, desde B4, las de `OfflineApiFixture` (`Auth/AuthorizationPolicyTests` y la carpeta `Conventions/`) y `Auth/CurrentUserAdapterTests`, que no usa host.

Utilidades del proyecto (`Infrastructure/` y `Fixtures/`, desde B3):

| Utilidad | Para qué sirve |
|---|---|
| `TestBrowser` | Cliente HTTP que se comporta como el navegador del SPA: guarda y envía las cookies a mano (así cada `Set-Cookie` es visible y una cookie se puede trasladar a otro host), no sigue redirecciones, pide el token antiforgery y lo adjunta en `X-XSRF-TOKEN`. Dirección base `https://localhost` por defecto; `http://localhost` para comprobar Development |
| `TestEndpoints` | Rutas que solo existen en el host de pruebas, añadidas con un `IStartupFilter` detrás del pipeline del Api: se resuelven en un segundo pipeline que comparte los servicios de DI del Api (políticas, manejadores y ProblemDetails), no sus middleware. Permiten observar la sesión y los `401`/`403` mientras no hay rutas protegidas de producto. De B3: `/api/test/authenticated`, `/api/test/administrator-only` (requisito de rol ad hoc, CHU-10 de HU-003) y `/api/test/persistent-cookie`. De B4, excluidas del documento OpenAPI: `/api/test/no-metadata` (sin ningún metadato de autorización: solo la protege la política de reserva), `/api/test/policy/{política}` (una sonda por cada política con nombre: `204` si la sesión la satisface) y `/api/test/client-context` (política `ClientUser`; devuelve la empresa que ve `ICurrentUser`, como la leería un caso de uso) |
| `OfflineApiFixture` (B4) | Hosts del Api sin base de datos, en `Development` y `Production`, para pruebas de políticas y de convención que no necesitan Docker (ver arriba) |
| `TestPrincipals` (B4) | Principales construidos a mano con la forma de la cookie de sesión: id en `NameIdentifier`, correo, `display_name`, un `Role` por rol o equipo y `company_id` solo si se indica; admite claims adicionales (p. ej. un `company_id` mal formado o repetido). `TestPrincipals.Anonymous` es el principal sin sesión |
| `TestDataSeeder` | Persiste el catálogo `TestCompanies` / `TestAccounts` con EF Core y `UserManager`. Es solo de pruebas: el sembrador de Development (`DevelopmentSeeder`, B7) es código de producción con su propio catálogo mínimo de cinco cuentas, que una prueba compara con este; el juego completo llegará con [[hu-010-datos-sinteticos-de-desarrollo]]. Las cuentas `Invited` quedan sin contraseña. `CreateAsync` sirve para cuentas ad hoc de una prueba |
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

Clases nuevas de `Auth/` en B4 ([[hu-004-contexto-de-usuario-y-autorizacion|HU-004]]). Los nombres de políticas, claims y campos del JSON se escriben como literales, a propósito: son los del contrato.

| Clase | Docker | Qué cubre |
|---|---|---|
| `Auth/MeEndpointTests` | Sí | CHU-01 a CHU-04. `/me` de un cliente (con su empresa y `no-store`) y de un interno (`company` presente y `null`); roles y equipos en campos distintos para cada perfil del catálogo; exactamente los campos del contrato y en su orden. Prueba negativa A↔B: con `?userId=`, `?companyId=` y cabeceras de empresa o de usuario la respuesta es idéntica byte a byte y no contiene nada del otro usuario ni de su empresa. Sin cookie, con una cookie inventada o tras el logout: `401` |
| `Auth/AuthorizationPolicyTests` | No (`OfflineApiFixture`) | CHU-05 y CHU-08. Matriz perfil → políticas con el `IAuthorizationService` del composition root real y `TestPrincipals`, incluidos los perfiles no válidos, que no satisfacen ninguna. La política de reserva exige usuario autenticado y no mira el perfil. Las políticas registradas son exactamente las cuatro del contrato y no hay ninguna de contenido de tickets. Las políticas son las mismas en un host de `Production` |
| `Auth/SessionAuthorizationTests` | Sí | CHU-05 a CHU-08 con sesiones reales. La misma matriz a través de las sondas `/api/test/policy/{política}`. `ICurrentUser.CompanyId` entrega la empresa de la sesión aunque la petición traiga la de otra empresa en la consulta y en una cabecera. Un cliente cuya cookie no trae `company_id` (fábrica de claims defectuosa, sustituida en la prueba) sigue autenticado pero recibe `403` en `ClientUser`, en el endpoint de cliente, en `/me` y en las demás políticas. El endpoint sin metadatos admite a cualquier usuario autenticado. Una ruta inexistente responde `404` también con sesión |
| `Auth/ClaimsFactoryTests` | Sí | T-03. Claims que emite la fábrica para una cuenta cliente y una interna; `company_id` y `display_name` salen de la cuenta aunque haya filas con otros valores en `identity.user_claims`. Clase propia porque modifica `user_claims` |
| `Auth/ClaimsFreshnessTests` | Sí | «Frescura de claims». Un rol concedido con la sesión abierta no aparece en `/me` antes del intervalo del sello (4 min) y sí después (6 min), cuando la misma sesión pasa además a satisfacer la política `Administrator`, sin haberse cerrado. Clase propia porque avanza el reloj y cambia roles |
| `Auth/CurrentUserAdapterTests` | No (sin host) | T-03 y CHU-07. `HttpContextCurrentUser` sobre principales construidos: cliente con empresa, cliente sin empresa, interno, interno con empresa, perfil mixto, `company_id` repetido, mal formado o vacío, roles desconocidos, sin sesión, sin `HttpContext` y sesión cuyo identificador no es un GUID |

#### Pruebas de convención (`Conventions/`, desde B4)

Recorren la tabla de endpoints del host (`EndpointDataSource`) con `OfflineApiFixture`: no necesitan Docker y corren también en la etapa `test` del Dockerfile. Cada una incluye un control para que la regla no pase «sin mirar nada» (afirma que el recorrido ve endpoints conocidos, o provoca la infracción y comprueba que se detecta).

| Clase | Qué garantiza |
|---|---|
| `EndpointAuthorizationConventionTests` | CHU-06 de HU-004 (invariante 9, PRD §12). Los endpoints que admiten anónimos son **exactamente** los de la lista pública, escrita como literales, en `Development` y en `Production`: ni uno de más, ni una entrada que ya no exista. Todo endpoint no público responde `401` a una petición sin sesión. Un endpoint sin ningún metadato de autorización queda protegido por la política de reserva. Una ruta inexistente responde `404` en `problem+json` a un anónimo. El documento OpenAPI no se publica fuera de Development |
| `AntiforgeryConventionTests` | Toda mutación bajo `/api` (o endpoint sin restricción de método) lleva el marcador `AntiforgeryValidationMetadata`, es decir, cuelga del grupo raíz y pasa por el filtro. Control: mutaciones mapeadas a propósito fuera del grupo se detectan; un `GET` no cuenta |
| `ClientInputConventionTests` | CHU-07 de HU-004 (PRD §5 regla 1). Ningún endpoint al que pueda llegar un cliente (todo el que no exija una política solo interna) nombra la empresa en la plantilla de ruta, en los parámetros del manejador (ruta, consulta, cabeceras, formulario) ni, de forma recursiva, en las propiedades de sus DTO. Reconoce `CompanyId`, `company_id`, `X-Company-Id` y variantes; ignora los servicios inyectados (`ICurrentUser` declara `CompanyId`, pero es la sesión). Control: una infracción por cada canal |
| `EndpointCatalog`, `ClientInputConvention` | Utilidades, no pruebas: lectura de la tabla de endpoints (firmas `MÉTODO /ruta`, `* /ruta` si no restringe el método) y detector de entradas que nombran la empresa |

Al añadir un endpoint: si es público, entra en la lista literal de `EndpointAuthorizationConventionTests`; si es una mutación, se mapea dentro del grupo `/api`; si lo puede llamar un cliente, no recibe la empresa ([[backend-hexagonal]]).

## Frontend (React 19)

jsdom se activa por archivo (`// @vitest-environment jsdom` + `import '…/test/domSetup'`); los modelos siguen en `node`. Tras B6 del goal del login había 147 pruebas Vitest en 17 archivos; tras B7 (2026-10-12) son **152 en los mismos 17 archivos** (B7 añadió, entre otras, las del orden de foco de `/ingresar`, el aviso de un cierre de sesión fallido y el error de `/me` tras el login). Las de `app/` cubren login, logout, la carrera de `/me` y los intentos de saltar la guarda con rutas codificadas. Los controladores se prueban con `renderHook` en jsdom.

| Nivel | Herramientas | Qué cubre |
|---|---|---|
| Estático | `oxlint` (incluye fronteras MVC) + `tsc -b` | Tipos y capas |
| Unitarias | Vitest | Modelos (normalización, reglas) y controladores |
| Componentes | Testing Library + jsdom (desde 2026-10-09) | Vistas con interacción; `app/` monta `<MemoryRouter><App/></MemoryRouter>` con un doble de `fetch` que falla ante rutas no previstas |
| E2E | Playwright (por añadir) | Flujos críticos: radicar, triage, chat, respuesta formal |

## Pruebas imprescindibles por invariante

| Invariante | Prueba |
|---|---|
| Aislamiento multiempresa (CA-01, CA-02) | Integración: usuario de empresa A recibe 404/403 sobre ticket de B (pendiente, HU de tickets). Hecho en B4: la empresa sale solo de la sesión, con prueba negativa A↔B sobre `/me` y sobre `ICurrentUser`, y la convención `ClientInputConventionTests` |
| Autenticación por defecto y lista pública (invariante 9) | Convención `EndpointAuthorizationConventionTests`: igualdad exacta con la lista de endpoints públicos (hecho en B4) |
| Chat solo para participantes (CA-06, CA-08) | Integración del hub: no participante no se une ni envía; retirado pierde la conexión |
| Persistir antes de publicar (CA-05) | Unitaria del caso de uso: el notificador no se llama si falla la persistencia |
| Respuesta formal y cierre solo PM/admin (CA-11, CA-12) | Unitaria + integración por rol |
| Adjuntos (CA-09) | Rechazo de tipo/tamaño/firma inválidos en backend |
| Auditoría (CA-14) | Cada caso de uso relevante deja `AuditEntry` con actor y fecha |

## Relacionado

- [[backend-hexagonal]] · [[frontend-mvc]] · [[criterios-de-aceptacion]] · [[flujo-de-trabajo-github]] · [[autenticacion-identity]] · [[roles-y-permisos]] · [[persistencia-postgresql]]
