---
title: Autenticación con ASP.NET Core Identity
type: arquitectura
status: propuesta
tags: [arquitectura/backend, seguridad, identity]
sources: ["PRD.md §5, §12", "Decisión de la persona líder (2026-10-07): usar Identity para el login", "backend/src/DataTicket.Api/Auth", "backend/src/DataTicket.Api/Endpoints/Auth", "backend/src/DataTicket.Application/Authentication", "backend/src/DataTicket.Infrastructure/Identity"]
aliases: [Identity, Login, Autenticación]
created: 2026-10-07
updated: 2026-10-12
---

# Autenticación con ASP.NET Core Identity

El login usa **ASP.NET Core Identity** sobre PostgreSQL (decisión del equipo, 2026-10-07). El mecanismo concreto —cookie *same-origin* a través del proxy del frontend— quedó **aceptado** el 2026-10-09 en [[adr-0004-autenticacion-cookie-mismo-origen]]. Lo construye el goal de `goal_login.md` ([[plan-goal-login-y-loop-chat]]) por bloques:

- **Implementado en el backend (B3, HU-003):** antiforgery, login, logout, cookie de sesión, bloqueo por intentos fallidos, revalidación del sello de seguridad y respuestas `401`/`403` sin `Location`. Detalle en *Implementación en el backend (B3)*.
- **Pendiente (B4, HU-004):** `GET /api/auth/me`, el puerto `ICurrentUser`, la fábrica de claims, las políticas con nombre y la política de reserva (*fallback*).

La vista `/ingresar` es la de HU-003 (un único formulario), con el estilo de [[sistema-de-diseno]]. Los datos semilla de referencia (cuentas `Active`, `Invited` y `Deactivated` con contraseña sintética) están en `db.sql` §13 ([[persistencia-postgresql]]).

## Requisitos del PRD que condicionan el diseño

| Requisito | Fuente | Consecuencia |
|---|---|---|
| Sin registro público; Data Global crea, invita y desactiva cuentas | PRD §5 | No exponer `MapIdentityApi` completo (incluye `/register`); endpoints propios |
| Invitación y restablecimiento seguros, sin revelar si el correo existe | PRD §5, §12 | Token de un solo uso; misma respuesta exista o no la cuenta |
| Derivación segura de contraseñas estándar | PRD §12 | `PasswordHasher` de Identity |
| MFA, SSO y federación fuera del MVP | PRD §13 | No se implementan ahora |
| Autorización en servidor en cada operación | PRD §5.2 | Políticas + verificación en casos de uso |

## Diseño propuesto

- **Almacén**: `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` en `Infrastructure/Persistence`; `ApplicationUser` en `Infrastructure/Identity`. El dominio solo conoce `UserId` ([[backend-hexagonal]]).
- **Sesión**: cookie de Identity `HttpOnly`, `Secure` fuera de Development, `SameSite=Lax`; expiración deslizante. El navegador la envía a `/api` y `/hubs` porque todo es el mismo origen.
- **CSRF**: antiforgery con encabezado en endpoints que cambian estado (el SPA lo obtiene de un endpoint dedicado).
- **Claves de Data Protection** en PostgreSQL (`PersistKeysToDbContext`): las cookies sobreviven reinicios del contenedor.
- **Roles**: `ProductManager`, `Administrator`, `Development`, `Production`, `Requester`, `CompanyCoordinator`. Claim `company_id` para usuarios cliente; nunca se acepta del cuerpo de la petición ([[roles-y-permisos]]).
- **SignalR**: el hub `[Authorize]` usa la misma cookie; no hace falta token en query string ([[tiempo-real-signalr]]).

## Contrato ratificado (T-01 de HU-002, HU-003 y HU-004 — 2026-10-09)

Fijado en el bloque B1 de `goal_login.md`. Los valores marcados *propuesta* son los valores por defecto del goal: se aplican ya y quedan por ratificar con el equipo en [[pendientes]] §3b (P-22).

### Parámetros de la sesión

| Tema | Valor |
|---|---|
| Cookie de sesión | `dataticket.auth`, `HttpOnly`, `SameSite=Lax`, `Path=/`, `Secure` fuera de Development (en Development, igual que la petición); expiración deslizante de **8 h** (*propuesta*) |
| Antiforgery | Cabecera `X-XSRF-TOKEN`; cookie del token `dataticket.xsrf` (`HttpOnly`, `SameSite=Strict`). El token depende de la identidad: el SPA lo pide de nuevo **tras el login y tras el logout** (*propuesta*) |
| Bloqueo | 5 intentos fallidos consecutivos → 15 minutos (P-18, *propuesta*) |
| Contraseña | Mínimo 12 caracteres (P-18, *propuesta*); hash `PasswordHasher` de Identity (PRD §12) |
| Sello de seguridad | Revalidación cada **5 minutos** (P-13, *propuesta*); al revalidar, una cuenta que ya no está `Active` pierde la sesión |
| Logout | Expira la cookie del navegador; **no** revoca otras sesiones del mismo usuario (*riesgo aceptado*, ver abajo) |
| Estado de la cuenta | `AccountStatus` {`Invited`, `Active`, `Deactivated`} en `identity.users.account_status` (`db.sql` §3) |
| Esquema | Tablas de Identity y `data_protection_keys` en el esquema `identity`, snake_case, según `db.sql` ([[adr-0009-db-sql-diseno-de-referencia]]) |

> [!danger] Riesgo aceptado — el logout no revoca otras copias de la cookie
> Con cookies sin estado, una copia robada sigue siendo válida hasta su expiración o hasta que cambie el sello de seguridad (máx. 5 min tras el cambio). Se acepta en el MVP; mitigación futura: actualizar el sello al cerrar sesión (cierra todas las sesiones) o un almacén de sesiones en servidor.

### Roles, equipos y claims

- **Equipos = roles de Identity** `Development` y `Production` (decisión provisional de `db.sql` §3 y §12, [[adr-0009-db-sql-diseno-de-referencia]]). La API los expone aparte, en `teams`, para no prejuzgar el modelo definitivo de [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]].
- Claims de la cookie: id (`NameIdentifier`), nombre visible, correo, un `ClaimTypes.Role` por rol (incluidos `Development` y `Production`) y `company_id` **solo** para cuentas cliente. `company_id` nunca se lee del cuerpo, la query ni las cabeceras.
- **Perfiles válidos** (si no se cumple, el login responde el `401` genérico):
  - Cliente: rol `Requester` o `CompanyCoordinator`, sin roles internos y con `company_id`.
  - Interno: al menos uno de `ProductManager`, `Administrator`, `Development` o `Production`, sin roles de cliente y sin `company_id`.
- Un principal de cliente sin `company_id` falla cerrado: no satisface `ClientUser` (`403`) e `ICurrentUser.CompanyId` no lo inventa.

### Políticas de autorización

| Política | Autoriza a |
|---|---|
| *Fallback* | Cualquier usuario autenticado (todo endpoint sin `AllowAnonymous` lo exige) |
| `ClientUser` | `Requester` o `CompanyCoordinator` con claim `company_id` |
| `InternalUser` | `ProductManager`, `Administrator`, `Development` o `Production` |
| `Administrator` | `Administrator` |
| `ProductManagerOrAdministrator` | `ProductManager` o `Administrator` |

Ninguna política concede contenido de tickets por ser `Administrator` (PRD §5 regla 5): eso lo decide la participación, en HU posteriores.

### Endpoints (`/api/auth`)

| Método y ruta | Acceso | Respuestas |
|---|---|---|
| `GET /api/auth/antiforgery` | Público | `200` `{ "headerName": "X-XSRF-TOKEN", "token": "…" }` + cookie `dataticket.xsrf`; `Cache-Control: no-store` |
| `POST /api/auth/login` | Público + antiforgery | `204` + `Set-Cookie: dataticket.auth`; `400` validación (`errors.email`, `errors.password`); `400` antiforgery (sin `errors`); `401` genérico «Correo o contraseña incorrectos.» (correo inexistente, clave incorrecta, `Invited`, `Deactivated`, bloqueada o perfil inválido; JSON idéntico salvo `traceId`) |
| `POST /api/auth/logout` | Autenticado + antiforgery | `204` + `Set-Cookie` que expira `dataticket.auth`; `401` sin sesión; `400` antiforgery |
| `GET /api/auth/me` | Autenticado | `200` (JSON abajo); `401` sin sesión. Ignora query y cabeceras |

Petición de login: `{ "email": string (requerido, formato de correo, ≤ 256), "password": string (requerida) }`.

Respuesta de `/me` (ejemplo con los IDs de `db.sql` §13):

```json
{
  "id": "b0000000-0000-7000-8000-000001000001",
  "displayName": "Ana Pérez",
  "email": "ana.perez@empresa-sintetica-01.test",
  "surface": "client",
  "roles": ["Requester"],
  "teams": [],
  "company": { "id": "c0000000-0000-7000-8000-000000000001", "name": "Empresa Sintética 01" }
}
```

| Campo | Regla |
|---|---|
| `surface` | `"client"` o `"internal"` según el perfil válido |
| `roles` | Subconjunto de `ProductManager`, `Administrator`, `Requester`, `CompanyCoordinator` |
| `teams` | Subconjunto de `Development`, `Production` |
| `company` | `{ id, name }` solo si `surface = "client"`; `null` para internos |

### Comportamiento común de la API

- Toda mutación (`POST`, `PUT`, `PATCH`, `DELETE`) bajo `/api` valida la cabecera antiforgery con un filtro de endpoint: si falta o no es válida → `400` ProblemDetails «La solicitud no es válida.» con la extensión `"code": "antiforgery_invalid"` (añadida tras la revisión de B5: el SPA invalida su token cacheado y reintenta **una** vez).
- Sin sesión → `401` `application/problem+json` «Se requiere iniciar sesión.», **sin** `Location` (nunca `302`).
- Autenticado sin permiso → `403` `application/problem+json` «No tienes permiso para realizar esta acción.».
- **Endpoints públicos** (lista explícita, verificada por prueba): `GET /api/health`, `GET /api/health/live`, `GET /api/auth/antiforgery`, `POST /api/auth/login` y `GET /openapi/v1.json` solo en Development. HU-005 y HU-006 añadirán `POST /api/auth/invitations/accept`, `POST /api/auth/password/forgot` y `POST /api/auth/password/reset`.
- No se registra `MapIdentityApi` ni ninguna ruta `register`, `confirmEmail` o `manage`.

### Implementación en el backend (B3, 2026-10-12)

B3 implementa `GET /api/auth/antiforgery`, `POST /api/auth/login` y `POST /api/auth/logout` con el reparto por capas de [[backend-hexagonal]]. Los valores de la tabla *Parámetros de la sesión* están en `IdentitySettings` (Infrastructure) y `AuthenticationSetup` (Api).

| Capa | Pieza | Papel |
|---|---|---|
| `Domain/Users` | `AccountAccess`, `RoleNames`, `AccountStatus`, `Surface` | `AccountAccess.ResolveSurface` aplica los «Perfiles válidos»; `CanSignIn` exige cuenta `Active` **y** perfil válido |
| `Application/Ports/In/Authentication` | `ISignIn`, `ISignOut` (+ `SignInCommand`, `SignInOutcome`) | Puertos de entrada; el resultado es solo `Succeeded` o `InvalidCredentials`, sin el motivo del fallo (PRD §5) |
| `Application/Authentication` | `SignIn`, `SignOut` | `SignIn` verifica credenciales y comprueba `CanSignIn` **antes** de emitir la sesión |
| `Application/Ports/Out` | `ICredentialVerifier` (+ `VerifiedAccount`), `IUserSession` | Verificar correo y contraseña con bloqueo; emitir y cerrar la sesión del canal actual |
| `Infrastructure/Identity` | `IdentityCredentialVerifier`, `IdentityUserSession`, `DataTicketUserManager`, `DataTicketSignInManager`, `ApplicationUser`, `IdentitySettings` | Adaptadores sobre Identity (abajo) |
| `Api/Auth` | `AuthenticationSetup`, `AntiforgeryValidationFilter`, `ApiProblems` | Cookie, antiforgery, textos y escritura de ProblemDetails |
| `Api/Endpoints/Auth` | `AuthEndpoints`, `LoginRequest` | Endpoints mínimos propios y validación de forma del cuerpo |

- **Registro de Identity** (`Infrastructure/DependencyInjection.cs`): `AddIdentityCore<ApplicationUser>` con roles `IdentityRole<Guid>`, almacenes de EF Core sobre `DataTicketDbContext` y proveedores de tokens por defecto; esquema de Identity `Version1` (sin tabla de *passkeys*) y correo único. Los esquemas de autenticación los registra el `Api`.
- **`DataTicketUserManager` — bloqueo con el reloj de la aplicación.** El `UserManager` estándar de ASP.NET Core 10 calcula el bloqueo con el reloj real (`DateTimeOffset.UtcNow`). Esta clase redefine `IsLockedOutAsync` y `AccessFailedAsync` con el `TimeProvider` inyectado: no hay un reloj paralelo y el bloqueo se prueba con `FakeTimeProvider` ([[estrategia-de-pruebas]]). Al alcanzar el umbral bloquea y reinicia el contador, como Identity.
- **`DataTicketSignInManager` — sello de seguridad.** Al revalidar el sello (cada 5 min) exige además que la cuenta siga `Active` y con perfil válido (`AccountAccess.CanSignIn`): una cuenta desactivada pierde la sesión aunque su sello no haya cambiado.
- **`IdentityCredentialVerifier` — tiempos de respuesta.** Comprueba la contraseña con `lockoutOnFailure: true`. Si el correo no existe, la cuenta no tiene contraseña o ya está bloqueada, verifica igualmente la clave recibida contra un hash señuelo (misma parametrización que los reales) para que el tiempo de respuesta no delate el motivo. El señuelo es `DecoyPasswordHash`, un *singleton* que se genera con el `IPasswordHasher<ApplicationUser>` de la aplicación (mismo coste que los hashes reales); todo intento de login cuesta **exactamente una** verificación de hash, y lo fija `LoginHashingCostTests` contando llamadas, sin cronómetros.
- **`IdentityUserSession` — cookie no persistente.** Emite la sesión con `isPersistent: false`: la cookie vive mientras el navegador esté abierto y su tique expira a las 8 h deslizantes.
- **Antiforgery.** `AntiforgeryValidationFilter` se aplica al grupo raíz en `Program.cs` (`app.MapGroup("/api").AddEndpointFilter<AntiforgeryValidationFilter>()`), así que cubre todas las mutaciones de los endpoints mapeados **dentro de ese grupo**: todo endpoint de producto debe colgar de él. No se usa el middleware `UseAntiforgery` porque solo cubre formularios. El filtro corre después de la autorización: sin sesión, una ruta protegida responde `401` antes que el `400` de antiforgery.
- **`401`/`403`.** Los eventos de redirección de la cookie escriben ProblemDetails con `ApiProblems` y nunca emiten `Location`. La respuesta es `application/problem+json` con `traceId` para **cualquier** `Accept` (también `text/html` o `text/plain`): `ApiProblems` delega en `TypedResults.Problem`, que escribe el JSON aunque ningún `IProblemDetailsWriter` acepte la petición.
- **Cuerpo JSON ilegible.** `RouteHandlerOptions.ThrowOnBadRequest = false` en `Program.cs`: un JSON mal formado responde `400` `problem+json` en todos los entornos (antes, `500` en Development). El título es el del framework («Bad Request»); su idioma queda en [[pendientes]] §4.
- **Validación del login** (`LoginRequest.Validate`): `400` «Uno o más campos no son válidos.» con `errors.email` / `errors.password`; un cuerpo ausente se trata como campos vacíos. El correo debe ser una dirección simple con dominio de al menos dos etiquetas; la contraseña solo se exige no vacía (sin longitud máxima, ver [[pendientes]] §4).
- **Data Protection.** Claves en `identity.data_protection_keys` (`PersistKeysToDbContext<DataTicketDbContext>`), nombre de aplicación `DataTicket`; sin cifrado en reposo ([[pendientes]] §4, [[persistencia-postgresql]]).

> [!question] Pendiente — B4 (HU-004)
> Todavía no existen `GET /api/auth/me`, el puerto `ICurrentUser`, la fábrica de claims propia (nombre visible y `company_id`) ni las políticas con nombre. `AddAuthorization()` no define política de reserva: hoy solo exigen sesión los endpoints que llaman a `RequireAuthorization()` (en B3, `POST /api/auth/logout`). El login tampoco comprueba aún que la empresa de la cuenta esté activa ([[hu-007-administrar-empresas-cliente|HU-007]], [[pendientes]] §4).

### Rutas del SPA

| Ruta | Acceso | Contenido |
|---|---|---|
| `/ingresar` | Pública | Vista de entrada (HU-003); acepta `?returnUrl=` saneado (solo rutas internas que empiezan por `/` y no por `//` ni `/\`) |
| `/activar-cuenta`, `/olvide-mi-contrasena`, `/restablecer-contrasena` | Públicas | Marcadores hasta HU-005 y HU-006 (enlaces de correo con `App:PublicBaseUrl`) |
| `/` | Autenticada | Redirige a `/portal` o `/interno` según `surface` |
| `/portal/**` | Solo `surface = "client"` | Portal del cliente: `/portal` (inicio), `/portal/tickets`, `/portal/radicar` (marcadores) |
| `/interno/**` | Solo `surface = "internal"` | Espacio interno: `/interno` (inicio), `/interno/bandeja`, `/interno/triage` (en la navegación solo para `ProductManager`) y `/interno/administracion` (solo para `Administrator`) (marcadores) |
| `/sin-permiso` | Autenticada | «No tienes permiso para ver esta página.» con enlace a la superficie propia |
| Cualquier otra | — | Redirige a `/` |

Un `401` de cualquier petición a `/api` lleva a `/ingresar?returnUrl=<ruta actual>`; un `403`, o abrir la superficie ajena, a la vista «sin permiso». La navegación por rol es ayuda de interfaz: la barrera es el backend (PRD §5 regla 2).

### Implementación en el SPA (B5–B6, 2026-10-09)

- El token antiforgery vive solo en memoria; `login` y `logout` lo invalidan (en `finally`). Ante `code: "antiforgery_invalid"` la mutación pide un token nuevo y reintenta **una** vez; ningún otro `400` se reintenta.
- `/me`, `login` y `logout` no disparan el manejador global de `401` (`notifyAuthErrors: false`).
- Ante cualquier `401` del login la vista muestra su texto fijo «Correo o contraseña incorrectos.» (no usa el `title` del servidor), conserva el correo, vacía la contraseña y lleva el foco a ella.
- La validación local del correo (una `@`, sin espacios, ≤ 256) es deliberadamente más laxa que la del servidor.
- `returnUrl` nunca apunta a `/ingresar` ni a `/sin-permiso`; tras un cierre de sesión explícito no se recuerda la ruta anterior.
- La entrada se muestra siempre en tema claro ([[sistema-de-diseno]]).

### Otros endpoints previstos (HU posteriores)

| Método | Ruta | HU |
|---|---|---|
| POST | `/api/auth/invitations` | Data Global invita a un usuario ([[hu-005-invitar-y-activar-cuentas|HU-005]]) |
| POST | `/api/auth/invitations/accept` | El invitado define su contraseña (HU-005) |
| POST | `/api/auth/password/forgot` · `/api/auth/password/reset` | Restablecimiento sin enumeración ([[hu-006-restablecer-contrasena|HU-006]]) |

## Correo

Invitación, restablecimiento y vinculación a un ticket salen por `IEmailSender` (puerto). **Los enlaces de esos correos se construyen con una URL base configurada** (p. ej. `App:PublicBaseUrl`), nunca con el `Host` de la petición: un `Host` falsificado podría envenenar los enlaces de restablecimiento. Al desplegar detrás de un proxy, configurar además `UseForwardedHeaders` con los proxies conocidos y fijar `AllowedHosts` (hoy `*` en `appsettings.json`). En local llegan a **Mailpit** (`http://localhost:8025`); el proveedor productivo está pendiente (PRD §16.2, [[pendientes]]).

## Relacionado

- [[adr-0004-autenticacion-cookie-mismo-origen]] · [[hu-002-shell-y-navegacion-por-rol]] · [[hu-003-iniciar-y-cerrar-sesion]] · [[hu-004-contexto-de-usuario-y-autorizacion]] · [[glosario]] · [[sistema-de-diseno]] · [[plan-goal-login-y-loop-chat]] · [[roles-y-permisos]] · [[persistencia-postgresql]] · [[entorno-docker]] · [[backend-hexagonal]] · [[estrategia-de-pruebas]] · [[pendientes]]
