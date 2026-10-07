---
title: "HU-003 — Iniciar y cerrar sesión"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, seguridad, identity, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §12", "PRD.md §13"]
aliases: ["HU-003", "Iniciar y cerrar sesión"]
epica: "[[ep-002-identidad-y-acceso]]"
criterios_prd: []
componentes: ["Identity", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Frontend (models, controllers, views)", "Pruebas de integración"]
dificultad: "Alto"
sprint_sugerido: "Sprint 0"
dependencias: ["[[hu-002-shell-y-navegacion-por-rol]]"]
relacionadas: ["[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-005-invitar-y-activar-cuentas]]", "[[hu-006-restablecer-contrasena]]", "[[hu-008-administrar-usuarios-cliente]]", "[[hu-001-integracion-continua]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-003 — Iniciar y cerrar sesión

Las personas con cuenta activa inician sesión con correo y contraseña y reciben una cookie de Identity de mismo origen (`HttpOnly`, `SameSite=Lax`, `Secure` fuera de Development); pueden cerrarla en cualquier momento. Los errores no revelan si el correo existe, las cuentas desactivadas no entran y los intentos fallidos repetidos bloquean temporalmente la cuenta.

## Historia de usuario

**COMO** usuario de DataTicket con una cuenta activa (cliente o interno)  
**QUIERO** iniciar sesión con mi correo y mi contraseña y cerrar sesión cuando termine  
**PARA** acceder solo a lo que me corresponde y que nadie pueda usar mi sesión ni averiguar qué correos tienen cuenta

## Contexto

Que el login use **ASP.NET Core Identity** ya está decidido; el mecanismo de sesión (cookie de mismo origen a través del proxy de Vite/nginx) es una propuesta en [[adr-0004-autenticacion-cookie-mismo-origen]] pendiente de aceptación ([[pendientes]] §4). Hoy el backend no tiene Identity, `DataTicketDbContext` está vacío, no hay migraciones ni proyecto de pruebas de integración ([[backend-hexagonal]], [[persistencia-postgresql]]). Diseño previsto en [[autenticacion-identity]]: endpoints propios en `/api/auth`, sin `MapIdentityApi` (expone `/register`, contrario al PRD §5), antiforgery por cabecera y claves de Data Protection en PostgreSQL.

## Alcance

- Identity sobre PostgreSQL: `ApplicationUser` (en `Infrastructure/Identity`) con nombre visible y estado de la cuenta, `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` y migración inicial.
- Claves de Data Protection persistidas en PostgreSQL (las cookies sobreviven reinicios del contenedor).
- Cookie de la aplicación: `HttpOnly`, `SameSite=Lax`, `Secure` fuera de Development, `Path=/`, expiración deslizante; eventos que devuelven 401/403 en lugar de redirigir.
- `GET /api/auth/antiforgery`, `POST /api/auth/login`, `POST /api/auth/logout`.
- Filtro de endpoint que valida la cabecera antiforgery en `POST`, `PUT`, `PATCH` y `DELETE` (la validación automática de minimal APIs solo cubre formularios).
- Bloqueo por intentos fallidos con los valores de Identity (propuesta: 5 intentos fallidos consecutivos; duración del bloqueo a ratificar en T-01).
- Primer proyecto de pruebas de integración (`DataTicket.IntegrationTests`, nombre propuesto) con `WebApplicationFactory<Program>` y PostgreSQL real.
- Frontend: vista de entrada en `/ingresar` (módulo `auth`, nombre propuesto) y acción «Cerrar sesión» del layout de [[hu-002-shell-y-navegacion-por-rol|HU-002]].

## Fuera de alcance

- `GET /api/auth/me`, políticas por rol, `ICurrentUser` y exigencia global de autenticación ([[hu-004-contexto-de-usuario-y-autorizacion|HU-004]]).
- Invitación y activación de cuentas ([[hu-005-invitar-y-activar-cuentas|HU-005]]) y restablecimiento ([[hu-006-restablecer-contrasena|HU-006]]).
- Crear usuarios desde la interfaz ([[ep-003-empresas-usuarios-y-equipos]]); las pruebas usan cuentas creadas por fixtures.
- MFA, SSO, «recordarme» persistente y federación (PRD §13).
- Configuración de proxy/TLS productivo (`UseForwardedHeaders`, `AllowedHosts`): pendiente de despliegue ([[pendientes]] §4).

## Requisitos y reglas de negocio

- Contraseñas gestionadas con solución estándar de identidad y derivación segura; nada de cifrado reversible ni hash casero (PRD §12).
- No hay registro público de clientes (PRD §5).
- El flujo no debe revelar si un correo está registrado (PRD §5): mismo error para correo inexistente, contraseña incorrecta, cuenta desactivada, cuenta con invitación pendiente y cuenta bloqueada (los cuatro últimos casos, propuesta coherente con la regla).
- Data Global desactiva cuentas (PRD §5): una cuenta desactivada no inicia sesión.
- Todo acceso exige autenticación salvo páginas de entrada expresamente públicas (PRD §12): `/api/auth/antiforgery` y `/api/auth/login` son públicos.
- Transporte productivo cifrado por HTTPS (PRD §12): la cookie lleva `Secure` fuera de Development.
- MFA fuera del MVP (PRD §5, §13).

## Invariantes en juego

- Invariante 9 (`AGENTS.md` §7): sin registro público; no se expone `MapIdentityApi` ni ninguna ruta `/register`.
- Reglas de arquitectura (`AGENTS.md` §6): Identity vive en `Infrastructure` y `Api`; `Domain` y `Application` no referencian `Microsoft.AspNetCore*` (las `ArchitectureTests` siguen en verde).

## Criterios del PRD cubiertos

- Ninguno de forma directa. Implementa los requisitos no funcionales de autenticación del PRD §12 que sostienen PRD CA-01, CA-06 y CA-10 → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-002-identidad-y-acceso]]
- Dependencias: aceptación de [[adr-0004-autenticacion-cookie-mismo-origen]] (hoy `propuesta`); [[hu-002-shell-y-navegacion-por-rol|HU-002]] (cliente HTTP con antiforgery, ruta `/ingresar`); PostgreSQL disponible en CI ([[hu-001-integracion-continua|HU-001]], T-05).
- Relacionadas: [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]], [[hu-005-invitar-y-activar-cuentas|HU-005]], [[hu-006-restablecer-contrasena|HU-006]], [[hu-008-administrar-usuarios-cliente|HU-008]] (desactivación), [[hu-010-datos-sinteticos-de-desarrollo|HU-010]] (cuentas para probar a mano).
- Decisiones: [[adr-0004-autenticacion-cookie-mismo-origen]] (propuesta), [[adr-0002-backend-hexagonal]].

## Componentes afectados

- Identity: `ApplicationUser`, `UserManager`, `SignInManager`, opciones de contraseña y bloqueo.
- Backend `Infrastructure`: `Persistence` (contexto Identity, configuración, migración, tabla de claves de Data Protection), `Identity`.
- Backend `Api`: endpoints `/api/auth/*`, opciones de cookie, eventos 401/403, filtro antiforgery, registro en `Program.cs`.
- Persistencia PostgreSQL: tablas de Identity y de claves de Data Protection.
- Pruebas: nuevo proyecto de integración (añadir a `DataTicket.slnx` y a la etapa `restore` de `backend/Dockerfile`).
- Frontend: `src/modules/auth` (models, controllers, views), acción de cierre en `AppLayout`.

## Dificultad

**Nivel:** Alto

**Justificación:** Cambio transversal (Identity, persistencia con primera migración, Api, frontend) con varias reglas de seguridad que interactúan (antiforgery ligado a la identidad, bloqueo, no enumeración, `Secure` según entorno) y primera infraestructura de pruebas de integración. Depende de un ADR sin aceptar.

## Contrato backend ↔ frontend

Propuesta REST; ratificar en T-01. Errores en `application/problem+json` (RFC 9457).

**`GET /api/auth/antiforgery`** — público

```json
// 200
{ "headerName": "X-XSRF-TOKEN", "token": "CfDJ8N…" }
```

El token depende de la identidad: el SPA debe pedir uno nuevo **después** de iniciar o cerrar sesión.

**`POST /api/auth/login`** — público, requiere cabecera antiforgery

```json
// Petición
{ "email": "ana.perez@empresa-sintetica-01.test", "password": "Clave-Sintetica-2026" }
```

| Respuesta | Cuándo | Cuerpo |
|---|---|---|
| `204 No Content` + `Set-Cookie` | Credenciales válidas y cuenta activa | — |
| `400` | Validación: `email` vacío, con formato inválido o de más de 256 caracteres; `password` vacía | `{ "title": "Uno o más campos no son válidos.", "status": 400, "errors": { "email": ["…"] } }` |
| `400` | Falta o no es válida la cabecera antiforgery | ProblemDetails sin `errors` |
| `401` | Correo inexistente, contraseña incorrecta, cuenta desactivada, invitación pendiente o cuenta bloqueada | `{ "title": "Correo o contraseña incorrectos.", "status": 401 }` (idéntico en todos los casos salvo `traceId`) |

**`POST /api/auth/logout`** — autenticado, requiere cabecera antiforgery

| Respuesta | Cuándo |
|---|---|
| `204 No Content` + `Set-Cookie` que expira la cookie | Sesión válida |
| `401` | Sin sesión |
| `400` | Falta o no es válida la cabecera antiforgery |

Comportamiento común de la API (todas las rutas `/api/**`): sin sesión → `401` `application/problem+json` **sin** cabecera `Location` (nunca `302` a una página de login); autenticado sin permiso → `403` `application/problem+json`.

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: con ADR-0004 aceptado, fijar rutas, cuerpos, códigos, nombre de la cabecera antiforgery (`X-XSRF-TOKEN`, propuesta), nombre de la cookie (propuesta: `dataticket.auth`), duración de la sesión deslizante, parámetros de bloqueo, intervalo de revalidación del sello de seguridad y modelo mínimo del estado de la cuenta (`AccountStatus` {`Invited`, `Active`, `Deactivated`}, nombre propuesto, ratificar con HU-005/HU-008 y registrar en el [[glosario]]). Decidir si el cierre de sesión revoca la cookie en servidor (ver Notas).
- [ ] **T-02 — Proyecto de pruebas de integración** · Capa: Backend (pruebas) · Dificultad: Medio  
  Descripción: crear `DataTicket.IntegrationTests` (nombre propuesto) con `WebApplicationFactory<Program>`, PostgreSQL real (Compose o Testcontainers, según HU-001), base aislada por ejecución y fixture para crear cuentas en cada estado. Añadirlo a `DataTicket.slnx` y a la etapa `restore` de `backend/Dockerfile`.
- [ ] **T-03 — Pruebas de integración primero** · Capa: Backend (pruebas) · Dificultad: Medio  
  Descripción: escribir en rojo las pruebas de CHU-01 a CHU-10 y CHU-12 (cookie y atributos, `Secure` por entorno, respuestas idénticas, cuenta desactivada/pendiente, bloqueo con reloj controlado, validación, antiforgery, cierre, ausencia de `/register`, 401 sin redirección, persistencia de claves).
- [ ] **T-04 — Identity y persistencia** · Capa: Backend (Infrastructure) · Dificultad: Alto  
  Descripción: `ApplicationUser : IdentityUser<Guid>` con `DisplayName` y estado de la cuenta; `DataTicketDbContext` hereda de `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` e implementa el almacén de claves de Data Protection; `AddIdentityCore` + `SignInManager` + roles; opciones de contraseña y bloqueo; `RequireUniqueEmail = true`. El dominio solo conoce `UserId` ([[backend-hexagonal]]).
- [ ] **T-05 — Migración EF Core** · Capa: Persistencia · Dificultad: Bajo  
  Descripción: migración inicial (propuesta: `AddIdentity`) con tablas de Identity y de claves de Data Protection en `Infrastructure/Persistence/Migrations`; aplicar en Development al arrancar o por comando, según T-01.
- [ ] **T-06 — Endpoints, cookie y antiforgery** · Capa: Backend (Api) · Dificultad: Alto  
  Descripción: grupo `/api/auth` con `antiforgery`, `login`, `logout`; `SignInManager.PasswordSignInAsync` con `lockoutOnFailure: true` y comprobación del estado de la cuenta antes de emitir la cookie; opciones de cookie (`HttpOnly`, `SameSite=Lax`, `SecurePolicy=Always` fuera de Development); eventos `OnRedirectToLogin` → 401 y `OnRedirectToAccessDenied` → 403; filtro de endpoint antiforgery para métodos de mutación; no registrar `MapIdentityApi`.
- [ ] **T-07 — Frontend: modelo y gateway** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: pruebas Vitest primero para la validación local (`email` requerido y con formato, máximo 256; `password` requerida) y para `authGateway.login/logout` (usa `postJson` de HU-002 e invalida el token antiforgery cacheado tras login y logout).
- [ ] **T-08 — Frontend: controlador** · Capa: Frontend (controllers) · Dificultad: Medio  
  Descripción: `useLoginController` con estado `idle | submitting | error`, mensaje genérico ante 401, errores por campo ante 400, navegación a `returnUrl` saneado o a la superficie del usuario; acción `logout` en el controlador de sesión.
- [ ] **T-09 — Frontend: vistas** · Capa: Frontend (views) · Dificultad: Bajo  
  Descripción: `LoginView` pura (correo, contraseña, botón «Ingresar» deshabilitado mientras envía, enlace «¿Olvidaste tu contraseña?» a `/olvide-mi-contrasena`, mensaje en `role="alert"`); pruebas con Testing Library.
- [ ] **T-10 — Notas para la wiki** · Capa: Documentación · Dificultad: Bajo  
  Descripción: [[autenticacion-identity]] (implementado frente a propuesto), [[adr-0004-autenticacion-cookie-mismo-origen]] (estado), [[persistencia-postgresql]] (primera migración), [[backend-hexagonal]] (estado actual), [[estrategia-de-pruebas]] (proyecto de integración).

## Criterios de aceptación

### CHU-01 — Inicio de sesión correcto con cookie segura

**Dado** una cuenta activa `ana.perez@empresa-sintetica-01.test` con contraseña `Clave-Sintetica-2026` y un token antiforgery válido  
**Cuando** envía `POST /api/auth/login` con esas credenciales  
**Entonces** recibe `204`, una cabecera `Set-Cookie` con atributos `HttpOnly`, `SameSite=Lax` y `Path=/`, y la siguiente petición autenticada con esa cookie es aceptada por el backend.

### CHU-02 — `Secure` fuera de Development

**Dado** el backend ejecutándose con entorno distinto de `Development` (p. ej. `Staging` en `WebApplicationFactory`)  
**Cuando** se inicia sesión correctamente  
**Entonces** la cookie incluye el atributo `Secure`; y en `Development` sobre `http://localhost` el inicio de sesión sigue funcionando.

### CHU-03 — Mismo error para correo inexistente y contraseña incorrecta

**Dado** el correo inexistente `nadie@empresa-sintetica-99.test` y el correo existente `ana.perez@empresa-sintetica-01.test` con contraseña incorrecta  
**Cuando** se envía `POST /api/auth/login` con cada uno  
**Entonces** ambas respuestas son `401` con el mismo `title` («Correo o contraseña incorrectos.») y los mismos campos (comparando el JSON sin `traceId`), y ninguna emite `Set-Cookie` de sesión.

### CHU-04 — Cuentas desactivadas o pendientes no entran

**Dado** una cuenta en estado `Deactivated` y otra en estado `Invited` (sin contraseña fijada; para la prueba se fija una con el fixture)  
**Cuando** se intenta iniciar sesión con la contraseña correcta  
**Entonces** la respuesta es `401` idéntica a CHU-03 y no se emite cookie de sesión.

### CHU-05 — Bloqueo tras intentos fallidos

**Dado** una cuenta activa y los parámetros de bloqueo ratificados en T-01 (propuesta: 5 intentos fallidos consecutivos)  
**Cuando** se envían 5 contraseñas incorrectas seguidas y luego la contraseña correcta  
**Entonces** el sexto intento responde `401` idéntico a CHU-03 (sin revelar el bloqueo); y, con el reloj de pruebas avanzado más allá de la duración del bloqueo, la contraseña correcta responde `204` y el contador de fallos vuelve a cero.

### CHU-06 — Validación con valores límite

**Dado** un token antiforgery válido  
**Cuando** se envía `POST /api/auth/login` con un `email` de 257 caracteres, con `email` = `"ana.perez"` (sin dominio) o con `password` vacía  
**Entonces** cada petición responde `400` `application/problem+json` con `errors` en el campo correspondiente; y un `email` de exactamente 256 caracteres con formato válido no produce error de validación (responde `401` si no existe).

### CHU-07 — Antiforgery obligatorio en mutaciones

**Dado** una petición `POST /api/auth/login` o `POST /api/auth/logout` sin la cabecera `X-XSRF-TOKEN` o con un token de otra sesión  
**Cuando** llega al backend  
**Entonces** responde `400` y no cambia el estado de la sesión; y tras un login correcto, un logout con el token pedido **después** del login responde `204`.

### CHU-08 — Cierre de sesión

**Dado** un usuario con sesión iniciada  
**Cuando** envía `POST /api/auth/logout` con antiforgery válido  
**Entonces** recibe `204` con una `Set-Cookie` que expira la cookie de sesión, y una petición posterior del navegador a un endpoint protegido responde `401`.

### CHU-09 — Sin registro público

**Dado** el backend en cualquier entorno  
**Cuando** se envía `POST /api/auth/register`, `POST /register` o se consulta `/openapi/v1.json` en Development  
**Entonces** las rutas de registro responden `404` y el documento OpenAPI no contiene ninguna ruta `register`, `confirmEmail` ni `manage` de `MapIdentityApi`.

### CHU-10 — La API responde 401/403, nunca 302

**Dado** una petición sin cookie a un endpoint protegido de `/api`  
**Cuando** llega al backend  
**Entonces** responde `401` con `Content-Type: application/problem+json` y sin cabecera `Location`; y un usuario autenticado sin el rol requerido recibe `403` con el mismo formato.

### CHU-11 — La vista de entrada no revela cuentas y maneja errores

**Dado** la vista `/ingresar`  
**Cuando** el backend responde `401` a un intento  
**Entonces** la vista muestra «Correo o contraseña incorrectos.» en `role="alert"`, conserva el correo escrito, vacía la contraseña y vuelve a habilitar «Ingresar»; mientras la petición está en curso el botón está deshabilitado; y ante `204` navega a la superficie del usuario (o al `returnUrl` saneado de HU-002).

### CHU-12 — La sesión sobrevive al reinicio del backend

**Dado** una cookie emitida por una instancia del backend  
**Cuando** se reinicia el backend (en pruebas: una segunda `WebApplicationFactory` sobre la misma base)  
**Entonces** la misma cookie sigue siendo aceptada, porque las claves de Data Protection están en PostgreSQL.

### CHU-13 — Contraseñas con hash estándar

**Dado** una cuenta creada con contraseña `Clave-Sintetica-2026`  
**Cuando** se lee su fila en la tabla de usuarios de Identity  
**Entonces** `PasswordHash` no contiene la contraseña en claro y es verificable por el `PasswordHasher` de Identity; no existe ningún hash propio ni cifrado reversible en el código.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-13 validados con evidencia.
- [ ] **DoD-02** — Pruebas de integración escritas primero y en verde, con nombres por comportamiento: `Login_valido_emite_cookie_HttpOnly_SameSiteLax`, `Login_fuera_de_Development_emite_cookie_Secure`, `Login_con_correo_inexistente_y_clave_incorrecta_responde_igual`, `Login_de_cuenta_desactivada_o_pendiente_responde_401_generico`, `Quinto_fallo_bloquea_y_no_lo_revela`, `Login_valida_limites_de_email`, `Mutacion_sin_antiforgery_responde_400`, `Logout_expira_la_cookie`, `No_existe_ruta_register`, `Api_sin_sesion_responde_401_sin_Location`, `Cookie_sobrevive_reinicio_por_claves_en_PostgreSQL`.
- [ ] **DoD-03** — Pruebas Vitest de `auth` (modelo, gateway, controlador, vista) en verde.
- [ ] **DoD-04** — `cd backend && dotnet test` en verde, incluidas `DataTicket.ArchitectureTests` (Identity no se filtra a `Domain`/`Application`).
- [ ] **DoD-05** — `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build` en verde.
- [ ] **DoD-06** — Migración EF Core creada, aplicada en local y revisada (tablas de Identity y de claves de Data Protection).
- [ ] **DoD-07** — `docker compose up --build` levanta; con una cuenta creada por fixture o script local se inicia y cierra sesión en `http://localhost:5173/ingresar` (verificación manual documentada en el PR).
- [ ] **DoD-08** — [[adr-0004-autenticacion-cookie-mismo-origen]] en estado `aceptada` (o reemplazado) antes de fusionar, y tachado en [[pendientes]] §4.
- [ ] **DoD-09** — Wiki actualizada vía Notas para la wiki: [[autenticacion-identity]], [[persistencia-postgresql]], [[backend-hexagonal]], [[estrategia-de-pruebas]], [[glosario]] (estado de la cuenta).
- [ ] **DoD-10** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (enumeración, cookies, antiforgery, `MapIdentityApi`).
- [ ] **DoD-11** — PR revisado y aprobado por otra persona del equipo; trazabilidad de HU y épica actualizada.

## Evidencia de validación

| Elemento | Resultado | Evidencia | Observación |
|---|---|---|---|
| CHU-01 | Pendiente | — | — |
| CHU-02 | Pendiente | — | — |
| CHU-03 | Pendiente | — | — |
| CHU-04 | Pendiente | — | — |
| CHU-05 | Pendiente | — | — |
| CHU-06 | Pendiente | — | — |
| CHU-07 | Pendiente | — | — |
| CHU-08 | Pendiente | — | — |
| CHU-09 | Pendiente | — | — |
| CHU-10 | Pendiente | — | — |
| CHU-11 | Pendiente | — | — |
| CHU-12 | Pendiente | — | — |
| CHU-13 | Pendiente | — | — |
| DoD-01 | Pendiente | — | — |
| DoD-02 | Pendiente | — | — |
| DoD-03 | Pendiente | — | — |
| DoD-04 | Pendiente | — | — |
| DoD-05 | Pendiente | — | — |
| DoD-06 | Pendiente | — | — |
| DoD-07 | Pendiente | — | — |
| DoD-08 | Pendiente | — | — |
| DoD-09 | Pendiente | — | — |
| DoD-10 | Pendiente | — | — |
| DoD-11 | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- **Riesgo — revocación al cerrar sesión**: con cookies sin estado, una copia robada de la cookie sigue siendo válida tras el logout hasta su expiración o hasta que cambie el sello de seguridad. Opciones a decidir en T-01: aceptar el riesgo, actualizar el sello al cerrar (cierra todas las sesiones del usuario) o un almacén de sesiones en servidor.
- **Auditoría**: el PRD §12 no lista el inicio de sesión como evento auditable; registrar intentos fallidos en logs (no en `AuditEntry`) es propuesta, ligada a V-11.
- **Temporización**: Identity responde más rápido cuando el correo no existe (no calcula hash); igualar tiempos en el login es propuesta a evaluar junto con el limitador de peticiones.
- **Cuentas para probar a mano**: hasta [[hu-010-datos-sinteticos-de-desarrollo|HU-010]] no hay cuentas sembradas; la verificación del Sprint 0 es automática.
- No confundir el puerto propio `IEmailSender` (HU-005) con `Microsoft.AspNetCore.Identity.IEmailSender<TUser>`.

## Relacionado

- [[ep-002-identidad-y-acceso]] · [[tablero-scrum]] · [[autenticacion-identity]] · [[adr-0004-autenticacion-cookie-mismo-origen]] · [[persistencia-postgresql]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[estrategia-de-pruebas]] · [[roles-y-permisos]]
