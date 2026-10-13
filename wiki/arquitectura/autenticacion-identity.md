---
title: Autenticación con ASP.NET Core Identity
type: arquitectura
status: propuesta
tags: [arquitectura/backend, seguridad, identity]
sources: ["PRD.md §5, §12", "Decisión de la persona líder (2026-10-07): usar Identity para el login", "backend/src/DataTicket.Api/Auth", "backend/src/DataTicket.Api/Endpoints/Auth", "backend/src/DataTicket.Api/Health", "backend/src/DataTicket.Application/Authentication", "backend/src/DataTicket.Application/Ports", "backend/src/DataTicket.Infrastructure/Identity", "backend/src/DataTicket.Infrastructure/Persistence/CompanyDirectory.cs", "backend/tests/DataTicket.IntegrationTests/Auth", "backend/tests/DataTicket.IntegrationTests/Conventions"]
aliases: [Identity, Login, Autenticación]
created: 2026-10-07
updated: 2026-10-12
---

# Autenticación con ASP.NET Core Identity

El login usa **ASP.NET Core Identity** sobre PostgreSQL (decisión del equipo, 2026-10-07). El mecanismo concreto —cookie *same-origin* a través del proxy del frontend— quedó **aceptado** el 2026-10-09 en [[adr-0004-autenticacion-cookie-mismo-origen]]. Lo construye el goal de `goal_login.md` ([[plan-goal-login-y-loop-chat]]) por bloques:

- **Implementado en el backend (B3, HU-003):** antiforgery, login, logout, cookie de sesión, bloqueo por intentos fallidos, revalidación del sello de seguridad y respuestas `401`/`403` sin `Location`. Detalle en *Implementación en el backend (B3)*.
- **Implementado en el backend (B4, HU-004):** `GET /api/auth/me`, el puerto `ICurrentUser`, la fábrica de claims, las políticas con nombre y la política de reserva (*fallback*) con su lista exacta de endpoints públicos. Detalle en *Implementación en el backend (B4)*; tres puntos quedan por ratificar en [[pendientes]].
- **Integrado y verificado en el stack (B7, 2026-10-12):** sembrador mínimo solo de Development y flujo completo comprobado con `curl` y en el navegador contra Compose ([[entorno-docker]]). HU-002, HU-003 y HU-004 están `En validación` desde ese día.

La vista `/ingresar` es la de HU-003 (un único formulario), con el estilo de [[sistema-de-diseno]]. Los datos semilla de referencia (cuentas `Active`, `Invited` y `Deactivated` con contraseña sintética) están en `db.sql` §13; el sembrador de Development crea un subconjunto mínimo de ellos ([[persistencia-postgresql]]).

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
- **Antiforgery.** `AntiforgeryValidationFilter` se aplica al grupo raíz en `Program.cs`, así que cubre todas las mutaciones de los endpoints mapeados **dentro de ese grupo**: todo endpoint de producto debe colgar de él. Desde B4 el grupo se declara con `app.MapGroup("/api").RequireAntiforgeryValidation()`, que añade el filtro y un metadato marcador (ver *Implementación en el backend (B4)*). No se usa el middleware `UseAntiforgery` porque solo cubre formularios. El filtro corre después de la autorización: sin sesión, una ruta protegida responde `401` antes que el `400` de antiforgery.
- **`401`/`403`.** Los eventos de redirección de la cookie escriben ProblemDetails con `ApiProblems` y nunca emiten `Location`. La respuesta es `application/problem+json` con `traceId` para **cualquier** `Accept` (también `text/html` o `text/plain`): `ApiProblems` delega en `TypedResults.Problem`, que escribe el JSON aunque ningún `IProblemDetailsWriter` acepte la petición.
- **Cuerpo JSON ilegible.** `RouteHandlerOptions.ThrowOnBadRequest = false` en `Program.cs`: un JSON mal formado responde `400` `problem+json` en todos los entornos (antes, `500` en Development). El título es el del framework («Bad Request»); su idioma queda en [[pendientes]] §4.
- **Validación del login** (`LoginRequest.Validate`): `400` «Uno o más campos no son válidos.» con `errors.email` / `errors.password`; un cuerpo ausente se trata como campos vacíos. El correo debe ser una dirección simple con dominio de al menos dos etiquetas; la contraseña solo se exige no vacía (sin longitud máxima, ver [[pendientes]] §4).
- **Data Protection.** Claves en `identity.data_protection_keys` (`PersistKeysToDbContext<DataTicketDbContext>`), nombre de aplicación `DataTicket`; sin cifrado en reposo ([[pendientes]] §4, [[persistencia-postgresql]]).

### Implementación en el backend (B4, 2026-10-12)

B4 implementa `GET /api/auth/me`, el puerto `ICurrentUser`, la fábrica de claims, las políticas con nombre y la política de reserva ([[hu-004-contexto-de-usuario-y-autorizacion|HU-004]]). No cambia el esquema: no hay migración.

| Capa | Pieza | Papel |
|---|---|---|
| `Domain/Users` | `RoleNames.Teams`, `RoleNames.Roles` | Separan los equipos (`Development`, `Production`) de los roles propiamente dichos; de ahí salen `teams` y `roles` |
| `Application/Ports/Out` | `ICurrentUser`, `ICompanyDirectory` | Usuario de la sesión para los casos de uso; nombre de una empresa por su identificador |
| `Application/Ports/In/Authentication` | `IGetCurrentUserProfile` (+ `CurrentUserProfile`, `CurrentUserCompany`) | Puerto de entrada de `/me`; no recibe parámetros |
| `Application/Authentication` | `GetCurrentUserProfile` | Compone el perfil desde `ICurrentUser` e `ICompanyDirectory`; devuelve `null` si la sesión no tiene un perfil válido |
| `Infrastructure/Identity` | `DataTicketClaimsPrincipalFactory`, `DataTicketClaimTypes` | Claims de la cookie de sesión |
| `Infrastructure/Persistence` | `CompanyDirectory` | Adaptador de `ICompanyDirectory` sobre EF Core (`dataticket.companies`, consulta sin seguimiento) |
| `Api/Auth` | `HttpContextCurrentUser`, `PrincipalProfile`, `AuthorizationPolicies`, `EndpointScopedAuthorizationResultHandler`, `AntiforgeryValidationMetadata` | Adaptador de `ICurrentUser`, lectura única de los claims, políticas, alcance de la política de reserva y marcador del filtro antiforgery |
| `Api/Endpoints/Auth` | `AuthEndpoints` (`GetCurrentUserAsync`), `CurrentUserResponse` | Endpoint `/me` y su JSON |

#### `GET /api/auth/me`

- **Acceso**: exige sesión (`RequireAuthorization()` explícito, además de la política de reserva).
- **Sin parámetros**: el manejador solo recibe el `HttpContext` (para fijar la cabecera de caché), el caso de uso y el token de cancelación; no enlaza nada de la ruta, la consulta, las cabeceras ni el cuerpo. `MeEndpointTests` lo comprueba con `?userId=`, `?companyId=`, `X-Company-Id`, `X-User-Id` y `X-Forwarded-User`: la respuesta es idéntica, byte a byte, a la que se obtiene sin ellos, en los dos sentidos A↔B (PRD §5 regla 1).
- **`200`**: el JSON del contrato, con los campos en este orden y ninguno más: `id`, `displayName`, `email`, `surface`, `roles`, `teams`, `company`. `company` es `{ id, name }` para clientes y viaja como `null` (presente, no omitido) para internos. `roles` y `teams` salen en el orden de `RoleNames.Roles` y `RoleNames.Teams`. Si la cuenta no tiene nombre visible, `displayName` es el correo.
- **`401`** `application/problem+json` «Se requiere iniciar sesión.»: sin cookie, con una cookie que no emitió el servidor o tras cerrar la sesión.
- **`403`** `application/problem+json` «No tienes permiso para realizar esta acción.»: hay sesión, pero `GetCurrentUserProfile` no obtiene un perfil válido. Ocurre si el perfil no resuelve a una superficie (cliente sin `company_id`, interno con `company_id`, mezcla de roles de cliente e internos, `company_id` repetido o mal formado), si la sesión no trae correo o si la empresa del cliente no existe. No se devuelve ningún dato del usuario.
- **`Cache-Control: no-store`**: el manejador la fija antes de ejecutar el caso de uso; la prueba lo afirma sobre el `200`.

> [!warning] Contradicción — el contrato de `/me` no lista el `403`
> La tabla *Endpoints* del contrato T-01 (y la HU-004) solo prevén `200` y `401` para `GET /api/auth/me`. El código responde además `403` cuando la sesión no tiene perfil válido. Es coherente con «falla cerrado» (CHU-07 de HU-004), pero no está ratificado, y el SPA no lo distingue: `fetchCurrentUser` solo trata el `401` como «sin sesión», así que un `403` de `/me` acaba hoy en el estado de error de carga y no en la vista «sin permiso». No se ha cambiado ni el contrato ni el código → [[pendientes]] §1.

#### Claims de la cookie y `DataTicketClaimsPrincipalFactory`

| Dato | Tipo de claim | Origen |
|---|---|---|
| Identificador | `ClaimTypes.NameIdentifier` | Identity (estándar) |
| Correo | `ClaimTypes.Email` | Identity (estándar) |
| Nombre visible | `display_name` (`DataTicketClaimTypes.DisplayName`) | `ApplicationUser.DisplayName` |
| Roles y equipos | Un `ClaimTypes.Role` por rol o equipo | Roles de Identity de la cuenta |
| Empresa | `company_id` (`DataTicketClaimTypes.CompanyId`, GUID con guiones) | `ApplicationUser.CompanyId`, **solo** si el perfil resuelve a cliente |

- La fábrica implementa `IUserClaimsPrincipalFactory<ApplicationUser>` directamente (no hereda de la estándar de Identity) y se registra con `AddClaimsPrincipalFactory<DataTicketClaimsPrincipalFactory>()` en `Infrastructure/DependencyInjection.cs`.
- **Lista cerrada de claims.** La fábrica emite solo lo que la aplicación decide: id, nombre de usuario, correo, sello de seguridad, `display_name`, roles y `company_id`. **No lee `identity.user_claims` ni `identity.role_claims`**: una fila escrita a mano no cambia la empresa, el nombre ni los roles de nadie (PRD §5 regla 1). Los roles salen únicamente de `identity.user_roles` (`UserManager.GetRolesAsync`), filtrados por `RoleNames.All`. Lo fija `ClaimsFactoryTests` (`Company_id_sale_solo_de_la_cuenta_y_no_de_claims_almacenados`, `Roles_salen_solo_de_user_roles_y_no_de_claims_almacenados`, `Rol_desconocido_de_user_roles_no_llega_a_la_sesion`).
- **`company_id` solo para clientes.** Se emite si `AccountAccess.ResolveSurface(roles, CompanyId)` da `Client`; una cuenta interna nunca lo lleva.
- **Los claims se regeneran al revalidar el sello.** La fábrica corre al iniciar sesión y en cada revalidación del sello de seguridad (5 min, `IdentitySettings.SecurityStampValidationInterval`): un cambio de rol, equipo o empresa llega a la cookie sin cerrar la sesión, con un desfase máximo de 5 minutos. `ClaimsFreshnessTests`: a los 4 minutos `/me` aún devuelve los roles anteriores; a los 6, los nuevos.
- **Falla cerrado con perfil no válido.** Si `AccountAccess.ResolveSurface` no resuelve superficie (cliente sin empresa, interno con empresa, perfil mixto), el principal identifica a la cuenta pero no lleva roles ni `company_id`: solo satisface la política de reserva (`Perfil_no_valido_no_satisface_ninguna_politica_con_nombre`).
- **Todo contenido es un endpoint.** Como la política de reserva protege endpoints registrados, el Api no sirve nada por middleware sin endpoint (`UseStaticFiles`, `UseHealthChecks`, ramas `app.Map`, UI de Swagger). Está escrito en `Program.cs` y lo vigilan `Api_does_not_serve_content_through_endpointless_middleware` (arquitectura) y `Anonimo_nunca_recibe_2xx_fuera_de_la_lista_publica` (30 rutas no registradas × GET/HEAD/POST, en Development y Production). Límite conocido: un `app.Use` en línea que sirva una ruta fuera de las sondeadas no se detecta.

> [!question] Pendiente — nombre del claim `display_name`
> El contrato T-01 dice «nombre visible» sin fijar el tipo del claim; el código usa `display_name`. Por ratificar ([[pendientes]] §3b, P-23). El claim `team` que la HU-004 proponía para los equipos no existe: los equipos viajan como `ClaimTypes.Role`.

#### `ICurrentUser`, `HttpContextCurrentUser` y `PrincipalProfile`

- `ICurrentUser` (`Application/Ports/Out`) expone `IsAuthenticated`, `UserId`, `DisplayName`, `Email`, `Surface`, `Roles`, `Teams`, `CompanyId` e `IsInRole(role)`, sin tipos de ASP.NET Core.
- `PrincipalProfile` (`Api/Auth`, interno) es la **lectura única** de los claims. La usan el adaptador `HttpContextCurrentUser` y las políticas con nombre, para que ambos decidan con las mismas reglas. Solo lee el principal: nunca el cuerpo, la consulta, la ruta ni las cabeceras.
- **Sesión**: `IsAuthenticated` exige una identidad autenticada cuyo `NameIdentifier` sea un GUID no vacío; en otro caso el perfil es anónimo.
- **Falla cerrado**: si el perfil no es uno de los «Perfiles válidos», `Surface` es `null`, `Roles` y `Teams` quedan vacíos y `CompanyId` es `null`. La sesión sigue autenticada (por eso la respuesta es `403` y no `401`) y conserva `UserId`, `DisplayName` y `Email`.
- **`company_id`**: sin claim, `null`; repetido con valores distintos, no GUID o GUID vacío, el perfil no es válido.
- **Roles y equipos separados**: `Roles` es la intersección con `RoleNames.Roles` y `Teams` con `RoleNames.Teams`; un rol desconocido se ignora. `IsInRole` solo mira `Roles`: un equipo no es un rol.
- `HttpContextCurrentUser` se registra como servicio *scoped* y lee el principal en cada acceso (el middleware de autenticación puede fijarlo después de que alguien resuelva el servicio); sin `HttpContext` devuelve el perfil anónimo. `CurrentUserAdapterTests` cubre estos casos sin host ni Docker.

#### Políticas con nombre y política de reserva

- `AuthorizationPolicies.AddDataTicketAuthorization()` (llamado desde `Program.cs`) registra las cuatro políticas del contrato, la política de reserva, el adaptador de `ICurrentUser` y el manejador de resultados. La matriz por perfil, tal como está implementada, está en [[roles-y-permisos]].
- Cada política con nombre exige sesión y una condición sobre `PrincipalProfile`: un perfil no válido no satisface ninguna.
- **Política de reserva**: `RequireAuthenticatedUser`. No mira el perfil: la satisface cualquier usuario autenticado, también uno sin roles. Todo endpoint registrado sin `AllowAnonymous` exige sesión, aunque no declare ningún metadato de autorización.
- **Lista pública exacta.** `EndpointAuthorizationConventionTests` recorre la tabla de endpoints y exige **igualdad** con la lista siguiente, escrita como literales en la prueba. Añadir un endpoint anónimo obliga a tocarla; los de HU-005 y HU-006 entrarán así.

| Endpoint público | Dónde se declara `AllowAnonymous` | Entornos |
|---|---|---|
| `GET /api/health` | `HealthEndpoints.AsPublicProbe()` | Todos |
| `GET /api/health/live` | `HealthEndpoints.AsPublicProbe()` | Todos |
| `GET /api/auth/antiforgery` | `AuthEndpoints` | Todos |
| `POST /api/auth/login` | `AuthEndpoints` | Todos |
| `GET /openapi/v1.json` (plantilla `GET /openapi/{documentName}.json`) | `app.MapOpenApi().AllowAnonymous()` en `Program.cs` | Solo Development; fuera de Development no se mapea y responde `404` |

> [!question] Pendiente — propuesta por ratificar: una ruta inexistente responde `404`, no `401`
> Según la documentación del propio manejador y la nota de implementación de `backend-engineer`, en ASP.NET Core 10 la política de reserva se aplica también a las peticiones que no coinciden con ningún endpoint: un anónimo que pide una ruta inexistente recibiría `401`, y eso rompe CHU-09 de HU-003 (las rutas `register` responden `404`). `EndpointScopedAuthorizationResultHandler` (`IAuthorizationMiddlewareResultHandler`) limita la reserva a los **endpoints registrados**: si la petición no tiene endpoint, sigue por el pipeline y termina en `404`, con o sin sesión; todo endpoint conserva el comportamiento estándar (`401` sin sesión, `403` sin permiso). Lo cubren `EndpointAuthorizationConventionTests` (anónimo, en hosts de Development y de Production) y `SessionAuthorizationTests` (con sesión).
> **Contrapartida:** un middleware que sirviera contenido sin endpoint y colocado después de `UseAuthorization` (p. ej. archivos estáticos) no quedaría protegido por la reserva. Hoy el Api no tiene ninguno: todo lo que sirve son endpoints. Es una **propuesta en uso, por ratificar** ([[pendientes]] §3b, P-23).

#### Antiforgery: marcador y prueba de convención

- `RequireAntiforgeryValidation()` (extensión de `RouteGroupBuilder`) añade al grupo el filtro `AntiforgeryValidationFilter` y el metadato marcador `AntiforgeryValidationMetadata`. `Program.cs` lo aplica al grupo raíz: `app.MapGroup("/api").RequireAntiforgeryValidation()`.
- `AntiforgeryConventionTests` exige que todo endpoint bajo `/api` que acepte `POST`, `PUT`, `PATCH` o `DELETE` (o que no restrinja el método) lleve el marcador. Una segunda prueba mapea a propósito mutaciones fuera del grupo y comprueba que se detectan.
- **Endpoints de salud limitados a `GET`.** `MapHealthChecks` no restringe el método y las sondas se mapean fuera del grupo raíz. `AsPublicProbe()` les añade `HttpMethodMetadata` con `GET`, de modo que no queda ninguna ruta de `/api` que admita una mutación sin pasar por el filtro.

#### Lo que B4 no cubre

- **Empresa activa.** Ni el login ni `/me` comprueban que la empresa del cliente esté activa: `ICompanyDirectory.FindNameAsync` devuelve el nombre de la empresa, esté activa o no. Es tarea de [[hu-007-administrar-empresas-cliente|HU-007]] ([[pendientes]] §4).
- **Autorización por ticket.** Ninguna política concede contenido de tickets; eso lo decidirá la participación en los casos de uso de las HU de tickets y chat ([[roles-y-permisos]]).
- **Políticas con nombre sin uso en producto.** Ningún endpoint de producto las usa todavía: `logout` y `/me` solo exigen sesión. Hoy las ejercitan las pruebas ([[estrategia-de-pruebas]]).

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
- Desde B7 (2026-10-12): el enlace «¿Olvidaste tu contraseña?» va debajo de «Ingresar» (orden de foco Correo → Contraseña → Ingresar → enlace) y el formulario declara `method="post"`, para que la contraseña nunca pueda acabar en la URL si fallara el JavaScript.
- El cierre de sesión vive en `useLogoutController`, del módulo `auth` ([[frontend-mvc]]): con `401` descarta igualmente la sesión local; con otro error la conserva y avisa.

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
