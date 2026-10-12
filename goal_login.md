# Goal — Login y autorización base (Sprint 0)

Archivo de control del `/goal` que construye **HU-002, HU-003 y HU-004**. Claude lo lee al empezar cada turno, ejecuta el primer bloque sin marcar y lo marca solo cuando la verificación real pasa. Las personas del equipo lo abren en cualquier momento para ver qué se ejecutó y qué falta.

- Plan y decisiones: `wiki/sintesis/plan-goal-login-y-loop-chat.md`
- Después de este goal va `loop_chat.md` (chat SignalR)
- HU: `wiki/scrum/historias-de-usuario/hu-002-…`, `hu-003-…`, `hu-004-…`

**Leyenda:** `[ ]` pendiente · `[~]` en curso · `[x]` hecho y verificado · `[!]` bloqueado (con el motivo en la línea)

---

## 1. Cómo ejecutarlo

1. Abre Claude Code en la raíz del repo, en **modo auto**. No hace falta tocar Docker Desktop: el stack ya está levantado y Claude ejecuta aquí los comandos de Docker.
2. Pega este comando tal cual (menos de 4.000 caracteres):

```text
/goal Todas las casillas de la sección «3. Lista de control» de goal_login.md (bloques B0 a B8) están marcadas [x] con su evidencia escrita, y en tu último turno pegaste en la conversación: (1) el contenido final de esa sección; (2) las líneas de resumen reales de `cd backend && dotnet test`, `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build`, todas con código de salida 0; (3) el veredicto de quality-reviewer sin hallazgos CRÍTICO ni ALTO abiertos; y (4) la salida de `git log --oneline main..HEAD` con un commit por bloque. Trabaja como orchestrator según AGENTS.md y la sección «2. Reglas del goal» de goal_login.md: al empezar cada turno relee el archivo, toma el primer bloque sin marcar, delega en los especialistas, verifica con comandos reales, marca las casillas, completa el «4. Registro de ejecución» y haz el commit del bloque. Detente y pregúntame si aparece una decisión de producto que el archivo no resuelve o si Docker no está disponible. Si tras 40 turnos no se cumple, detente y entrega un informe de lo que falta.
```

3. Para ver el avance: `/goal` sin argumentos (turnos y última evaluación), este archivo (casillas y registro) y `git log --oneline main..HEAD`.
4. Para detenerlo: `/goal clear`.

## 2. Reglas del goal

- **Rama:** `feature/auth-login`, creada desde `main` actualizado.
- **Commits:** uno por bloque cerrado, convencional (`feat(auth): B3 …`), con el trailer `Co-Authored-By` de la sesión. Nunca push, merge ni force-push.
- **TDD:** cada bloque de código escribe primero las pruebas con los nombres del DoD de la HU y las ve fallar.
- **Delegación:** contrato primero (B1). Después, `backend-engineer` y `frontend-engineer` en paralelo, y `quality-reviewer` al cerrar cada bloque de código. Toda corrección CRÍTICA o ALTA se resuelve antes de marcar.
- **Esquema:** Identity y las claves de Data Protection siguen `db.sql` (esquema `identity`, snake_case), implementado con migraciones de EF Core (`wiki/decisiones/adr-0009-db-sql-diseno-de-referencia.md`).
- **Estilo visual:** `wiki/arquitectura/sistema-de-diseno.md` (inspirado en `DataTicket.html`, sin copiar sus pantallas). El orquestador valida en el navegador integrado.
- **Credenciales:** solo sintéticas, en el sembrador de Development o en fixtures (referencia: `db.sql` §13). Nunca se escriben en el chat ni en la wiki.
- **Docker (lo opera Claude desde Claude Code):** el stack de Compose (`db`, `backend`, `frontend`, `mailpit`, `azurite`) ya está levantado. Claude ejecuta aquí todos los comandos (`docker compose ps/logs/up --build`, `curl`, pruebas). Para probar algo que el stack no tiene, crea un **contenedor desechable** (Testcontainers o `docker run --rm`, nombre `dt-*-check`) y elimínalo al terminar. Nunca borra volúmenes ni carga datos de prueba en el servicio `db` de Compose fuera de las migraciones y del sembrador de Development.
- **Fuente de verdad:** el PRD y las HU de Scrum. `DataTicket.html` es solo referencia visual (paleta, tipografía, estética), nunca de comportamiento ni de pantallas.
- **Datos semilla:** la referencia es la sección 13 (DML) de `db.sql`: IDs deterministas, cuentas `Active`/`Invited`/`Deactivated` y contraseña sintética común documentada allí. El sembrador de Development y los fixtures la replican con EF Core.
- **Prohibido:** editar `PRD.md`, `DataTicket.html`, `db.sql` (salvo que una HU cambie el esquema) y `wiki/raw/`; desactivar pruebas, lint o el hook `wiki-guard`; exponer `MapIdentityApi` o cualquier ruta `register`.
- **Decisiones por defecto** (se registran como *propuesta* en `autenticacion-identity` y `pendientes`; no detienen el goal):

| Tema | Valor por defecto |
|---|---|
| Cookie | `dataticket.auth`, `HttpOnly`, `SameSite=Lax`, `Secure` fuera de Development, expiración deslizante de 8 h |
| Antiforgery | cabecera `X-XSRF-TOKEN`, token pedido de nuevo tras el login y el logout |
| Bloqueo | 5 intentos fallidos, 15 minutos (P-18); contraseña de 12 caracteres como mínimo |
| Sello de seguridad | revalidación cada 5 minutos (P-13) |
| Logout | expira la cookie; no revoca las demás sesiones (riesgo aceptado y documentado) |
| Estado de cuenta | `AccountStatus` {`Invited`, `Active`, `Deactivated`} |
| Pruebas de integración | `DataTicket.IntegrationTests` con Testcontainers.PostgreSql (versión verificada en NuGet) |
| Enrutador | `react-router` 8.4.0 exacta, modo librería (`wiki/decisiones/adr-0010-enrutador-react-router.md`) |

## 3. Lista de control

### B0 — Preparación
- [x] Rama `feature/auth-login` creada desde `main` · Evidencia: `git checkout -b feature/auth-login` desde `main` al día con `origin/main` (`5bc3418`), árbol limpio.
- [x] ADR-0004 y ADR-0010 en estado `aceptada` (verificado en `wiki/decisiones/`) · Evidencia: `grep status` → `adr-0004…: status: aceptada`, `adr-0010…: status: aceptada`.
- [x] Línea base en verde: `dotnet test`, `lint`, `test` y `build` del frontend · Evidencia: `dotnet test` total 5, error 0 (exit 0); `oxlint` exit 0; `vitest` 3 passed (exit 0); `vite build` ✓ built (exit 0).
- [x] Stack de Compose operativo desde Claude Code (`docker compose ps`: `db` y `backend` healthy) · Evidencia: `db` (postgres:18.6) `Up 2 hours (healthy)`, `backend` `Up 2 hours (healthy)`, `frontend`, `mailpit` (healthy) y `azurite` arriba.
- [x] HU-002, HU-003 y HU-004 en estado `En desarrollo` y PLAN-DE-TRABAJO con responsable · Evidencia: `estado: En desarrollo` + línea de historial en las tres HU; `PLAN-DE-TRABAJO.md` con responsable juancarlosfc5 y goal «En ejecución».

### B1 — Contratos T-01 (HU-002, HU-003, HU-004)
- [x] Contrato de `/api/auth/antiforgery`, `/login`, `/logout` y `/me` ratificado con los valores por defecto · Evidencia: `autenticacion-identity` § «Contrato ratificado»: cookie `dataticket.auth` (HttpOnly, Lax, Secure fuera de Development, 8 h deslizante), `X-XSRF-TOKEN` renovado tras login/logout, bloqueo 5 × 15 min, contraseña ≥ 12, sello 5 min, logout sin revocar otras sesiones (riesgo aceptado), cuerpos y códigos 204/400/401 de cada ruta y `/me`; valores por defecto registrados como P-22 en `pendientes`.
- [x] Contrato documentado en `wiki/arquitectura/autenticacion-identity.md`; `AccountStatus` y los nombres nuevos en `glosario` · Evidencia: secciones nuevas en `wiki/arquitectura/autenticacion-identity.md` (parámetros, roles/claims, políticas, endpoints, comportamiento común, públicos); `glosario`: `AccountStatus`, `Surface`, `ICurrentUser`/`CurrentUser`, `company_id`, políticas y equipos como roles; `pendientes`: roles vs. equipos (DoD-06 HU-004) y P-22.
- [x] Rutas del SPA fijadas (`/ingresar`, `/portal/**`, `/interno/**`, `/sin-permiso`, públicas de cuenta) · Evidencia: tabla «Rutas del SPA» en `autenticacion-identity`: `/ingresar` (con `returnUrl` saneado), `/activar-cuenta`, `/olvide-mi-contrasena`, `/restablecer-contrasena` públicas; `/` redirige por `surface`; `/portal/**` solo cliente; `/interno/**` solo interno; `/sin-permiso`; resto → `/`.

### B2 — Infraestructura de pruebas backend
- [x] `backend/tests/DataTicket.IntegrationTests` creado con `WebApplicationFactory<Program>` y PostgreSQL real aislado · Evidencia: proyecto creado; Testcontainers.PostgreSql 4.16.0 con `postgres:18.6`, una base `dataticket_test_<guid>` por clase borrada al terminar; `Health_responde_200_con_PostgreSQL_del_contenedor` y `El_host_usa_la_base_aislada_y_no_la_del_Compose` en verde; `dotnet test` 19/19 exit 0; sin contenedores huérfanos.
- [x] Añadido a `DataTicket.slnx`, a la etapa `restore` del `backend/Dockerfile` y a las reglas de `ArchitectureTests` si aplica · Evidencia: en `DataTicket.slnx`; `restore` del `Dockerfile` copia los `.csproj` de `tests/` y restaura la solución; etapa `test` filtra `Category=RequiresDocker` → `docker build --target test` 14/14 exit 0; `ArchitectureTests` 5/5 sin regla nueva (el proyecto de pruebas no es una capa).
- [x] Fixtures de cuentas por estado (`Active`, `Invited`, `Deactivated`) y por rol · Evidencia: catálogo `TestAccounts`/`TestCompanies`/`TestRoles` = `db.sql` §13.1–13.4 (9 internas, 10 cliente de empresas 01 y 02 en `Active`/`Invited`/`Deactivated`); `TestAccountsCatalogTests` 7/7; persistencia en `ApiFixture.SeedAsync` (B3).
- [x] Reloj controlable (`TimeProvider`) disponible para las pruebas de bloqueo · Evidencia: `TimeProvider.System` en DI (prueba roja → verde) y `FakeTimeProvider` en la factory (`TestClockTests` 2/2). Hallazgo: el bloqueo de `UserManager` usa `DateTimeOffset.UtcNow` → B3 lo resuelve con un `UserManager` que usa `TimeProvider`. `quality-reviewer`: 0 CRÍTICO, 0 ALTO (6 BAJO, se atienden en B3).

### B3 — Backend HU-003: iniciar y cerrar sesión
- [x] Pruebas del DoD-02 de HU-003 escritas y en rojo · Evidencia: las 11 pruebas con los nombres del DoD-02 existen (`LoginTests`, `SecureCookieTests`, `LockoutTests`, `AntiforgeryTests`, `LogoutTests`, `RegisterRouteTests`, `AuthorizationResponseTests`, `DataProtectionKeysTests`). El rojo original no quedó registrado (corte de sesión del 2026-10-09); comprobado a posteriori el 2026-10-12 en una copia fuera del repo: sin `api.MapAuthEndpoints()` → `total: 70, error: 19` (las 11 incluidas); con la implementación → 70/70. Las 26 pruebas añadidas tras la revisión se vieron en rojo (natural o por mutación temporal) antes del verde.
- [x] `ApplicationUser`, `IdentityDbContext` y claves de Data Protection en PostgreSQL · Evidencia: `Infrastructure/Identity/ApplicationUser` sobre `DataTicketDbContext` (`AddIdentityCore` + roles `IdentityRole<Guid>`), `DataTicketUserManager` (bloqueo con `TimeProvider`), `DataTicketSignInManager`; claves en `identity.data_protection_keys` (`PersistKeysToDbContext`): `Cookie_sobrevive_reinicio_por_claves_en_PostgreSQL` y `Contrasena_se_guarda_con_hash_estandar_de_Identity` en verde. `Domain`/`Application` sin Identity ni EF Core (`ArchitectureTests` 8/8).
- [x] Migración EF Core creada, revisada y aplicada en local (coherente con `db.sql`) · Evidencia: `20261009164647_AddIdentityAndCompanies`; `quality-reviewer` la comparó columna a columna con `db.sql` §2–§3 (coincide; diferencia menor: IDs fijos de los 6 roles, en `pendientes`); `dotnet ef migrations has-pending-model-changes` → sin cambios; `SchemaTests` en verde. Aplicada al `db` del Compose con `docker compose up --build -d backend` (exit 0, `backend` healthy): 9 tablas en `identity`/`dataticket`, historial `…AddIdentityAndCompanies / 10.0.12`, 6 roles, 0 usuarios, 0 empresas. Solo migra en Development (`Fuera_de_Development_no_se_aplican_las_migraciones_al_arrancar`).
- [x] Endpoints `antiforgery`, `login` y `logout`; cookie; eventos 401/403 sin `Location`; filtro antiforgery; bloqueo · Evidencia: `Api/Endpoints/Auth/AuthEndpoints` (sin `MapIdentityApi`), `AuthenticationSetup` (cookie `dataticket.auth` HttpOnly/Lax, `Secure` = `Always` fuera de Development, 8 h deslizante), `ApiProblems` (401/403 `problem+json` sin `Location` con cualquier `Accept`), `AntiforgeryValidationFilter` en el grupo `/api` (`code: antiforgery_invalid`), bloqueo 5 × 15 min. En el stack: `GET /api/auth/antiforgery` → 200, `POST /api/auth/logout` sin sesión con `Accept: text/html` → 401 `application/problem+json`, `POST /api/auth/register` → 404.
- [x] CHU-01 a CHU-10, CHU-12 y CHU-13 de HU-003 en verde · Evidencia: `cd backend && dotnet build` → 0 advertencias, 0 errores; `cd backend && dotnet test` → `total: 96, error: 0, correcto: 96, omitido: 0` (exit 0; 43 sin Docker). Tabla CHU → prueba de `quality-reviewer`: todos cubiertos; **0 CRÍTICO, 0 ALTO**. Los 4 MEDIO y los BAJO 1–7 y 9 corregidos con prueba (401/403 que daba 500 sin `Accept` JSON, política `Secure`, defensa de tiempos con `DecoyPasswordHash`, JSON mal formado → 400, reglas de arquitectura, contrato de 8 h y 12 caracteres); BAJO-8 pasa a B4 y BAJO-10 a `pendientes`.

### B4 — Backend HU-004: contexto de usuario y autorización
- [x] Puerto `ICurrentUser` en `Application/Ports/Out` y adaptador en `Api` · Evidencia: `Application/Ports/Out/ICurrentUser` (+ `ICompanyDirectory`) y `Ports/In/Authentication/IGetCurrentUserProfile`; adaptador `Api/Auth/HttpContextCurrentUser` (lee el `ClaimsPrincipal` vía `PrincipalProfile`); `Application` sin ASP.NET ni claims. `CurrentUserPortTests` (4) en verde: interfaz en `Ports.Out`, sin tipos de ASP.NET (también dentro de genéricos y `Nullable<T>`), adaptador solo en `Api`.
- [x] Fábrica de claims (roles, equipos, `company_id` solo para clientes) · Evidencia: `Infrastructure/Identity/DataTicketClaimsPrincipalFactory`: lista cerrada (id, usuario, correo, sello, `display_name`, roles/equipos y `company_id` solo si el perfil resuelve a cliente); no lee `identity.user_claims` ni `role_claims`; roles solo de `user_roles` filtrados por `RoleNames.All`; perfil no válido → sin roles ni `company_id`. Pruebas `Company_id_sale_solo_de_la_cuenta_y_no_de_claims_almacenados`, `Roles_salen_solo_de_user_roles_y_no_de_claims_almacenados`, `Perfil_no_valido_no_satisface_ninguna_politica_con_nombre`, `Cambio_de_rol_llega_a_la_sesion_al_revalidar_el_sello` en verde (rojo real: `Expected: "Development" / Actual: "Administrator"`).
- [x] Políticas con nombre, *fallback* autenticado y `GET /api/auth/me` · Evidencia: `Api/Auth/AuthorizationPolicies`: `ClientUser`, `InternalUser`, `Administrator`, `ProductManagerOrAdministrator` desde una única tabla + reserva `RequireAuthenticatedUser`; `GET /api/auth/me` (200 con los 7 campos del contrato, 401 sin sesión, 403 con perfil no válido, `no-store`). `Politicas_por_rol_cumplen_la_matriz` (16 perfiles × 4 políticas), `Politicas_con_sesion_real_cumplen_la_matriz`, `Me_de_cliente_incluye_solo_su_empresa`, `Me_de_interno_no_tiene_empresa`, `Me_sin_sesion_responde_401` en verde. Propuesta por ratificar (P-23): la reserva cubre endpoints registrados y una ruta inexistente responde 404 (conserva HU-003 CHU-09).
- [x] Prueba que recorre los endpoints anónimos y prueba de convención de DTO sin `CompanyId` · Evidencia: `Solo_la_lista_publica_admite_anonimos` (igualdad exacta en Development y Production: `GET /api/health`, `GET /api/health/live`, `GET /api/auth/antiforgery`, `POST /api/auth/login` y OpenAPI solo en Development), `Anonimo_nunca_recibe_2xx_fuera_de_la_lista_publica` (30 rutas × GET/HEAD/POST), `DTOs_de_cliente_no_declaran_CompanyId` (cuerpo, query, ruta, cabeceras y `[AsParameters]`) y `Toda_mutacion_lleva_el_filtro_antiforgery` (BAJO-8 de B3). Rojos por mutación comprobados: `/me` con `AllowAnonymous`, `LoginRequest` con `CompanyId`, `POST` fuera del grupo.
- [x] CHU-01 a CHU-09 de HU-004 en verde; `ArchitectureTests` en verde · Evidencia: rojo inicial `total: 140, error: 38`; verde final `cd backend && dotnet build` → 0 advertencias, 0 errores y `cd backend && dotnet test` → `total: 154, error: 0, correcto: 154, omitido: 0` (exit 0; 84 sin Docker; `ArchitectureTests` 13/13); las 8 pruebas del DoD-02 con su nombre exacto; sin migración (`has-pending-model-changes` → sin cambios). `quality-reviewer`: tabla CHU-01…09 «Cumple», **0 CRÍTICO, 0 ALTO**; los 2 MEDIO (roles desde claims almacenados, vigilancia del manejador de la reserva) y los BAJO 1–6 corregidos con prueba.

### B5 — Frontend HU-002: shell, navegación y cliente HTTP
- [x] `react-router@8.4.0` (exacta), Testing Library y jsdom instalados con versiones verificadas en npm · Evidencia: `npm view` + `--save-exact`: `react-router` 8.4.0, `@testing-library/react` 16.3.3, `/dom` 10.4.2, `/user-event` 14.6.7, `/jest-dom` 7.0.1, `jsdom` 30.1.2; «found 0 vulnerabilities».
- [x] `src/styles/tokens.css` con los tokens claro/oscuro de `sistema-de-diseno` y la tipografía Archivo · Evidencia: `tokens.css` con la paleta del prototipo en claro y oscuro (`prefers-color-scheme` y `[data-theme]`), escalas de espacio/tipo/movimiento; `@fontsource-variable/archivo` 5.3.0 autoalojada (navegador: `font-family: "Archivo Variable"…`); sin colores sueltos en CSS nuevo.
- [x] `core/http`: `postJson`, `putJson`, `patchJson`, `deleteRequest`, antiforgery y error ProblemDetails tipado · Evidencia: `getJson/postJson/putJson/patchJson/deleteRequest`, `antiforgery.ts` (caché en memoria + `invalidateAntiforgeryToken`), `HttpError`/`problemDetails`, `authEvents` 401/403; pruebas `postJson envía cabecera antiforgery…`, `getJson no envía…`, `problem+json se mapea a HttpError…`, `401 invoca el manejador global` en verde.
- [x] Modelo de sesión, navegación por rol (función pura), guardas, `returnUrl` saneado y vista «sin permiso» · Evidencia: `modules/session`: `toCurrentUser` (surface desconocido → `none`), `navigationFor`/`canAccessPath`, `sanitizeReturnUrl`, `useSessionController`, guardas `RequireSession`/`RequireAccess`, `ForbiddenView`; navegador: cliente en `/interno/administracion` → «No tienes permiso para ver esta página.» + «Ir a mi portal».
- [x] Layout interno y del portal con nombre del usuario y «Cerrar sesión» · Evidencia: `AppLayout` (variantes `internal` y `portal`) con nombre y «Cerrar sesión» (`AppLayout muestra el nombre del usuario y Cerrar sesión` en verde); navegador 375 px oscuro y 1440 px claro: `scrollWidth = innerWidth`, ningún elemento fuera del viewport, con nombre y empresa extremos; foco visible (`:focus-visible`, contorno sólido).
- [x] CHU-01 a CHU-10 de HU-002 en verde · Evidencia: lint 0/0 (43 archivos), Vitest 11 archivos / 89 pruebas, build ✓ (exit 0 los tres); CHU-01…09 con pruebas nombradas y CHU-10 en el navegador integrado (Vite :5180 + doble del contrato). `quality-reviewer`: 0 CRÍTICO, 0 ALTO (4 MEDIO → B6).

### B6 — Frontend HU-003/HU-004: entrada y sesión
- [x] Módulo `src/modules/auth` (models, controllers, views) con la vista `/ingresar` de HU-003, con el lenguaje visual de `sistema-de-diseno` · Evidencia: `modules/auth` con `loginForm`, `loginState`, `authGateway` · `useLoginController`, `useLogoutController` · `LoginView`, `LoginField`; `/ingresar` revisada en el navegador integrado: entrada en claro aunque el sistema esté en oscuro (decisión de `sistema-de-diseno`), dos columnas a 1440 px y una a 375 px sin desbordamiento, campos de 16 px con `autocomplete` correcto.
- [x] Validación local, mensaje genérico en `role="alert"`, botón deshabilitado durante el envío y logout · Evidencia: validación (requerido, formato, ≤ 256) sin llamar al backend; 401 → «Correo o contraseña incorrectos.» en `role="alert"`, conserva correo, vacía contraseña y la enfoca; botón «Ingresando…» deshabilitado y sin doble envío; logout real `POST /api/auth/logout` con `X-XSRF-TOKEN` e `isSigningOut` (pruebas `Cerrar sesión llama a POST /api/auth/logout…`, `useLogoutController` 3/3).
- [x] Prueba de contrato de `/me` (CHU-10 de HU-004) y CHU-11 de HU-003 en verde · Evidencia: HU-004 CHU-10: `currentUser.test.ts` › «contrato de /me» (dos JSON de la HU y `surface: "partner"` → sin acceso) + `App.test.tsx` (partner → «sin permiso»); HU-003 CHU-11: 6 pruebas de `App.test.tsx` › «Entrada /ingresar», `LoginView.test.tsx` 7/7, `useLoginController.test.ts` 6/6 — 147/147 en verde.
- [x] `oxlint` en verde con las fronteras MVC · Evidencia: `oxlint --format=default src`: «Found 0 warnings and 0 errors» en 58 archivos (exit 0); build exit 0. `quality-reviewer`: 0 CRÍTICO, 0 ALTO; los 4 MEDIO de B5 verificados como resueltos con prueba.

### B7 — Integración y verificación manual
- [ ] Sembrador solo de Development con el subconjunto mínimo de cuentas de `db.sql` §13.2–13.4 (Elizabeth, un interno, `ana.perez`, una `Invited` y una `Deactivated`), con los mismos IDs, sin contraseñas en el chat · Evidencia:
- [ ] `docker compose up --build -d backend frontend` (ejecutado por Claude) reconstruye sin errores; `curl` a `/api/auth/antiforgery` → 200, `/login` → 204 y `/me` → 200 · Evidencia:
- [ ] Navegador integrado: login y logout con una cuenta interna y una cliente, redirección a la superficie correcta, 401 → `/ingresar?returnUrl=` · Evidencia:
- [ ] Revisión visual en tema claro y oscuro, a 375 y 1440 px, y con teclado (foco visible) · Evidencia:

### B8 — Calidad y cierre
- [ ] `quality-reviewer` sobre todos los cambios: 0 CRÍTICO y 0 ALTO abiertos · Evidencia:
- [ ] Matrices «Evidencia de validación» de HU-002, HU-003 y HU-004 completas; estado `En validación` · Evidencia:
- [ ] Wiki: `autenticacion-identity`, `persistencia-postgresql`, `backend-hexagonal`, `frontend-mvc`, `estrategia-de-pruebas`, `glosario`, `stack-y-versiones`, `pendientes` y entrada en `log.md` · Evidencia:
- [ ] Verificación final: `dotnet test`, `lint`, `test` y `build` en verde y pegados en la conversación · Evidencia:
- [ ] Borrador de descripción del PR (HU y CA cubiertos, páginas de la wiki) en el último turno · Evidencia:

## 4. Registro de ejecución

Una fila por turno que cambie algo. La añade Claude; no se borran filas.

| Fecha | Bloque | Resultado | Commit | Nota |
|---|---|---|---|---|
| — | — | — | — | Goal creado el 2026-10-09, sin ejecutar |
| 2026-10-09 | B0 | Hecho | ver `git log` (`chore(auth): B0 …`) | Rama creada, línea base en verde, stack healthy, HU en desarrollo |
| 2026-10-09 | B1 | Hecho | ver `git log` (`docs(auth): B1 …`) | Contrato T-01 ratificado con los valores por defecto; equipos = roles de Identity expuestos en `teams` |
| 2026-10-09 | B2 | Hecho | ver `git log` (`test(auth): B2 …`) | Infraestructura de integración con Testcontainers; revisión 0 CRÍTICO / 0 ALTO |
| 2026-10-09 | B5 | Hecho | ver `git log` (`feat(shell): B5 …`) | Shell, navegación por rol y `core/http`; revisión 0/0; MEDIO trasladados a B6 |
| 2026-10-09 | B6 | Hecho | ver `git log` (`feat(auth): B6 …`) | Módulo `auth`, `/ingresar`, logout real y MEDIO de B5 corregidos; revisión 0/0. Disco lleno resuelto vaciando cachés de npm/NuGet |
| 2026-10-12 | B3 | Hecho | ver `git log` (`feat(auth): B3 …`) | Reanudado tras recuperar Docker. Identity, migración y login/logout; revisión 0 CRÍTICO / 0 ALTO con sus MEDIO corregidos; migración aplicada al `db` del Compose reconstruyendo solo `backend` |
| 2026-10-12 | B4 | Hecho | ver `git log` (`feat(auth): B4 …`) | `ICurrentUser`, fábrica de claims cerrada, políticas, reserva y `/me`; revisión 0 CRÍTICO / 0 ALTO con sus MEDIO corregidos; sin migración. Por ratificar (P-23): 404 en rutas inexistentes, 403 de `/me` con perfil no válido, claim `display_name` |
