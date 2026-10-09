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
- [ ] Rama `feature/auth-login` creada desde `main` · Evidencia:
- [ ] ADR-0004 y ADR-0010 en estado `aceptada` (verificado en `wiki/decisiones/`) · Evidencia:
- [ ] Línea base en verde: `dotnet test`, `lint`, `test` y `build` del frontend · Evidencia:
- [ ] Stack de Compose operativo desde Claude Code (`docker compose ps`: `db` y `backend` healthy) · Evidencia:
- [ ] HU-002, HU-003 y HU-004 en estado `En desarrollo` y PLAN-DE-TRABAJO con responsable · Evidencia:

### B1 — Contratos T-01 (HU-002, HU-003, HU-004)
- [ ] Contrato de `/api/auth/antiforgery`, `/login`, `/logout` y `/me` ratificado con los valores por defecto · Evidencia:
- [ ] Contrato documentado en `wiki/arquitectura/autenticacion-identity.md`; `AccountStatus` y los nombres nuevos en `glosario` · Evidencia:
- [ ] Rutas del SPA fijadas (`/ingresar`, `/portal/**`, `/interno/**`, `/sin-permiso`, públicas de cuenta) · Evidencia:

### B2 — Infraestructura de pruebas backend
- [ ] `backend/tests/DataTicket.IntegrationTests` creado con `WebApplicationFactory<Program>` y PostgreSQL real aislado · Evidencia:
- [ ] Añadido a `DataTicket.slnx`, a la etapa `restore` del `backend/Dockerfile` y a las reglas de `ArchitectureTests` si aplica · Evidencia:
- [ ] Fixtures de cuentas por estado (`Active`, `Invited`, `Deactivated`) y por rol · Evidencia:
- [ ] Reloj controlable (`TimeProvider`) disponible para las pruebas de bloqueo · Evidencia:

### B3 — Backend HU-003: iniciar y cerrar sesión
- [ ] Pruebas del DoD-02 de HU-003 escritas y en rojo · Evidencia:
- [ ] `ApplicationUser`, `IdentityDbContext` y claves de Data Protection en PostgreSQL · Evidencia:
- [ ] Migración EF Core creada, revisada y aplicada en local (coherente con `db.sql`) · Evidencia:
- [ ] Endpoints `antiforgery`, `login` y `logout`; cookie; eventos 401/403 sin `Location`; filtro antiforgery; bloqueo · Evidencia:
- [ ] CHU-01 a CHU-10, CHU-12 y CHU-13 de HU-003 en verde · Evidencia:

### B4 — Backend HU-004: contexto de usuario y autorización
- [ ] Puerto `ICurrentUser` en `Application/Ports/Out` y adaptador en `Api` · Evidencia:
- [ ] Fábrica de claims (roles, equipos, `company_id` solo para clientes) · Evidencia:
- [ ] Políticas con nombre, *fallback* autenticado y `GET /api/auth/me` · Evidencia:
- [ ] Prueba que recorre los endpoints anónimos y prueba de convención de DTO sin `CompanyId` · Evidencia:
- [ ] CHU-01 a CHU-09 de HU-004 en verde; `ArchitectureTests` en verde · Evidencia:

### B5 — Frontend HU-002: shell, navegación y cliente HTTP
- [ ] `react-router@8.4.0` (exacta), Testing Library y jsdom instalados con versiones verificadas en npm · Evidencia:
- [ ] `src/styles/tokens.css` con los tokens claro/oscuro de `sistema-de-diseno` y la tipografía Archivo · Evidencia:
- [ ] `core/http`: `postJson`, `putJson`, `patchJson`, `deleteRequest`, antiforgery y error ProblemDetails tipado · Evidencia:
- [ ] Modelo de sesión, navegación por rol (función pura), guardas, `returnUrl` saneado y vista «sin permiso» · Evidencia:
- [ ] Layout interno y del portal con nombre del usuario y «Cerrar sesión» · Evidencia:
- [ ] CHU-01 a CHU-10 de HU-002 en verde · Evidencia:

### B6 — Frontend HU-003/HU-004: entrada y sesión
- [ ] Módulo `src/modules/auth` (models, controllers, views) con la vista `/ingresar` de HU-003, con el lenguaje visual de `sistema-de-diseno` · Evidencia:
- [ ] Validación local, mensaje genérico en `role="alert"`, botón deshabilitado durante el envío y logout · Evidencia:
- [ ] Prueba de contrato de `/me` (CHU-10 de HU-004) y CHU-11 de HU-003 en verde · Evidencia:
- [ ] `oxlint` en verde con las fronteras MVC · Evidencia:

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
