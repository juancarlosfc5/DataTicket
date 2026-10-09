---
title: Bitácora
type: log
status: vigente
tags: [log]
sources: []
aliases: [Log, Historial]
created: 2026-10-07
updated: 2026-10-07
---

# Bitácora

Registro cronológico **append-only** de lo que cambia en el proyecto y en la wiki. Solo se añaden entradas al final. Formato y tipos en `AGENTS.md` §5.5. Últimas entradas: `grep "^## \[" wiki/log.md | tail -5`.

## [2026-10-07] setup | Monorepo, entorno Docker, agentes y wiki inicial
- Autor: juancarlosfc5 (sesión de configuración inicial con Claude Code)
- Cambios:
  - Creados `backend/` (.NET 10, hexagonal: Domain, Application, Infrastructure, Api + pruebas de arquitectura) y `frontend/` (React 19.3 + TS, Vite 8, MVC con fronteras en oxlint y módulo de ejemplo `system/health`).
  - `docker-compose.yml` ampliado de solo PostgreSQL a `db`, `backend`, `frontend`, `mailpit` y `azurite`, con Compose Watch, proxy same-origin `/api` y `/hubs` (WebSocket) y puertos solo en `127.0.0.1`; `.env.example`.
  - Agentes de Claude Code: `orchestrator` (hilo principal vía `.claude/settings.json`), `backend-engineer`, `frontend-engineer`, `quality-reviewer`, `wiki-keeper`; hook `Stop` `wiki-guard.mjs`.
  - Esquema `AGENTS.md` (+ `CLAUDE.md`), `README.md`, `.gitignore`, `.gitattributes` (`merge=union` para este log), `.editorconfig`, plantilla de PR.
- Verificación: backend 0 advertencias y 5/5 pruebas; frontend oxlint limpio, 3/3 pruebas, build OK; cinco servicios arriba y sanos; salud OK directa y vía proxy; `/hubs` llega a Kestrel vía Vite y nginx; versiones dentro de los contenedores: ASP.NET Core 10.0.12, Node 24.21.0, React 19.3.0, PostgreSQL 18.6; hot reload de Compose Watch e imágenes runtime funcionando.
- Wiki: [[vision-general]], [[arquitectura-general]], [[backend-hexagonal]], [[frontend-mvc]], [[entorno-docker]], [[stack-y-versiones]], [[trabajar-con-el-agente]], [[flujo-de-trabajo-github]], [[estrategia-de-pruebas]], [[equipo-del-repositorio]]
- Pendiente: `git init` y repositorio en GitHub (ver [[flujo-de-trabajo-github]]).

## [2026-10-07] ingesta | PRD de DataTicket v0.1
- Autor: juancarlosfc5
- Cambios: PRD destilado en 12 páginas de producto, modelo de dominio propuesto, glosario español ↔ código y páginas de personas. Revisión del PRD con 4 contradicciones y 17 vacíos registrados.
- Wiki: [[fuente-prd-v0-1]], [[flujo-del-ticket]], [[estados-del-ticket]], [[matriz-de-prioridad]], [[roles-y-permisos]], [[chat-interno]], [[archivos-adjuntos]], [[notificaciones]], [[auditoria]], [[dashboard-y-metricas]], [[formularios-configurables]], [[fases-y-alcance]], [[criterios-de-aceptacion]], [[modelo-de-dominio]], [[glosario]], [[equipo-data-global]], [[elizabeth-pm]], [[julian-produccion]], [[pendientes]]
- Pendiente: ingerir `mvp-sistema-tickets.md`, `DataTicket.html` y el ZIP de diseño cuando estén disponibles.

## [2026-10-07] ingesta | Patrón LLM Wiki
- Autor: juancarlosfc5
- Cambios: resumido el documento de idea y documentada su instanciación (bóveda `wiki/`, esquema `AGENTS.md`, `index.md`, este log, operaciones de ingesta/consulta/lint).
- Wiki: [[fuente-patron-llm-wiki]], [[index]]

## [2026-10-07] consulta | PRD vs. docker-compose original
- Autor: juancarlosfc5
- Cambios: confirmada la observación de que el Compose no tenía .NET 10, Node 24 ni React 19; versiones verificadas en MCR, Docker Hub, npm, NuGet y calendarios oficiales. Análisis archivado como síntesis; PRD §15 queda desactualizado (registrado, no editado).
- Wiki: [[analisis-prd-vs-docker-compose]], [[stack-y-versiones]], [[pendientes]]

## [2026-10-07] decision | ADR iniciales
- Autor: juancarlosfc5
- Cambios: registradas como aceptadas las decisiones del equipo (monorepo, hexagonal, MVC) y del PRD (SignalR, URLs públicas en Blob), más el entorno con Compose Watch. Propuesta pendiente: sesión de Identity por cookie same-origin.
- Wiki: [[adr-0001-monorepo-contenedorizado]], [[adr-0002-backend-hexagonal]], [[adr-0003-frontend-mvc]], [[adr-0004-autenticacion-cookie-mismo-origen]], [[adr-0005-signalr-para-chat]], [[adr-0006-urls-publicas-azure-blob]], [[adr-0007-entorno-local-compose-watch]], [[autenticacion-identity]], [[tiempo-real-signalr]], [[persistencia-postgresql]]

## [2026-10-07] fix | Correcciones de la revisión de código
- Autor: juancarlosfc5
- Cambios:
  - Puertos de Compose solo en `127.0.0.1`; sincronización de `tsconfig*.json` en Compose Watch.
  - nginx runtime: re-resolución DNS del backend, `client_max_body_size 50m` en `/api/`, `index.html` sin caché, cabeceras de seguridad y de proxy ajustadas.
  - `/api/health` filtra por la etiqueta `ready` y solo muestra detalle en Development; avisos NuGet NU1901–NU1904 no rompen el build; la prueba de arquitectura del dominio también prohíbe `Microsoft.Extensions.*`.
  - Hook `wiki-guard`: falla en abierto si no puede leer su entrada, trata un log ausente como desactualizado, normaliza rutas (git en un repo padre, rutas cortas 8.3) e ignora `.env*` y `*.log`.
  - `dotnet` se ejecuta desde `backend/` (ahí está `global.json`; desde la raíz cae en VSTest y falla): documentación corregida.
  - Este equipo tiene un PostgreSQL nativo de Windows en `5432`: `.env` local con `POSTGRES_PORT=5433` (no versionado) y solución documentada.
- Wiki: [[entorno-docker]], [[backend-hexagonal]], [[autenticacion-identity]], [[estrategia-de-pruebas]], [[pendientes]]

## [2026-10-07] docs | Adaptación de la skill scrum-spec-orchestrator a DataTicket
- Autor: <git config user.name>
- Cambios: la skill pasa del stack de DataHub al de DataTicket (.NET 10 hexagonal, Identity, SignalR, PostgreSQL 18, React 19 MVC); escribe en wiki/scrum/; los criterios de cada historia pasan a CHU-xx y los del PRD se citan como PRD CA-xx.
- Wiki: ninguna página modificada.
- Pendiente: agregar `epica` e `historia-de-usuario` al vocabulario de `type` en AGENTS.md §5.3; decidir si la skill se mueve a .claude/skills/.
## [2026-10-07] setup | Skill using-agent-skills instalada en el proyecto
- Autor: JuanDavidDev6
- Cambios: `npx skills add https://github.com/addyosmani/agent-skills --skill using-agent-skills -a claude-code --copy -y` → `.claude/skills/using-agent-skills/SKILL.md` + `skills-lock.json` (instalación por proyecto, copia sin enlaces simbólicos). Contenido revisado: solo instrucciones, sin scripts.
- Wiki: [[trabajar-con-el-agente]]
- Pendiente: la meta-skill remite a ~24 skills hermanas no instaladas; decidir si se instalan (`--skill '*'`) o se queda solo esta.

## [2026-10-07] docs | Backlog Scrum (13 épicas, 47 HU) y plan de trabajo por fases
- Autor: JuanDavidDev6
- Cambios:
  - Se verificó el repositorio (estado *walking skeleton*, sin dominio implementado) y el PRD frente a la wiki.
  - Con la skill `scrum-spec-orchestrator`, tras aprobar el plan, se generaron 13 épicas y 47 HU en `wiki/scrum/` alineadas con las fases 0–4. Cada HU trae contrato propuesto, tareas por capa, criterios CHU en Dado/Cuando/Entonces, DoD específica y matriz de evidencia. Todas quedan en `Pendiente de aprobación`.
  - Se creó `PLAN-DE-TRABAJO.md` en la raíz, con checkmark, responsable y fecha por fase y por HU.
  - ADR-0008: la consulta básica del portal del cliente pasa a la Fase 1.
  - `AGENTS.md` §2, §5.2 y §5.3: nuevos `PLAN-DE-TRABAJO.md`, `scrum-spec-orchestrator/` y `wiki/scrum/`, y tipos `epica` e `historia-de-usuario`. Esto cierra el pendiente del log anterior.
- Wiki: [[tablero-scrum]], `ep-001`…`ep-013`, `hu-001`…`hu-047`, [[adr-0008-portal-cliente-en-fase-1]], [[fases-y-alcance]], [[pendientes]] (portal resuelto; nuevas contradicciones de dashboard/MVP y roles frente a equipos; V-18…V-25), [[index]].
- Pendiente:
  - Aprobar HU por HU para pasarlas a `Aprobada`.
  - Repartir responsables entre Laura, Juan David y Brayan.
  - Resolver las decisiones que condicionan el backlog (ver [[tablero-scrum]]).
  - Decidir si la skill se mueve a `.claude/skills/`.

## [2026-10-07] docs | Cierre del backlog Scrum: verificación e integración de notas
- Autor: JuanDavidDev6
- Cambios:
  - Se verificó con un script de solo lectura: 13 épicas y 47 HU, frontmatter completo, 418 CHU (de 7 a 13 por HU), sin wikilinks rotos, épicas e HU enlazadas en ambos sentidos, 47 HU en `PLAN-DE-TRABAJO.md` sin duplicados, y CA-01…CA-15 cubiertos.
  - HU-011 (auditoría) se adelantó al Sprint 1 porque HU-007, HU-008 y HU-009 la necesitan.
  - La cobertura de CA del tablero se recalculó a partir del frontmatter de las HU.
- Wiki: [[tablero-scrum]], [[pendientes]] (nueva §3b con las propuestas P-01…P-21 de las HU por ratificar), `hu-007`, `hu-008`, `hu-009`, `hu-011`.
- Pendiente: ratificar las propuestas P-01…P-21 en la T-01 de cada HU y llevar los términos nuevos al [[glosario]] al ratificarlos.

## [2026-10-09] setup | backend-engineer preparado para HU de Sprint 0 y EP-009
- Autor: juancarlosfc5
- Cambios:
  - `.claude/agents/backend-engineer.md` reescrito (166 líneas). Conserva `name`, `description`, `tools`, `model` y `color`, y añade `memory: project` y `effort: high`, campos documentados en la referencia oficial de subagentes. Incluye:
    - lecturas por HU y comprobación del estado real del código con Glob/Grep;
    - flujo por HU: leer la nota → confirmar el contrato T-01 → rojo con los nombres del DoD → implementar → verde → evidencia por CHU;
    - convenciones: RFC 9457, 404 para recursos no visibles (P-01), códigos `HubException`, `IClock`/`TimeProvider`, Guid v7, microsegundos, `no-store`;
    - Identity: antiforgery por filtro, 401/403 sin `Location`, bloqueo (P-18), *fallback* autenticado, recorrido de anónimos, `ICurrentUser` en `Ports/Out`, sin `MapIdentityApi`;
    - SignalR: tracker, `IChatConnectionRevoker`, `TicketAccessRevoked`, revalidación en cada método, validación de `Origin` (P-13), cursor (P-12);
    - `DataTicket.IntegrationTests` con PostgreSQL real aislado y clientes SignalR autenticados;
    - verificación de paquetes en NuGet;
    - formato de retorno con la tabla «Evidencia CHU/DoD».
  - Nuevo `backend/CLAUDE.md` (36 líneas): `dotnet` desde `backend/`, capas, comandos, prohibiciones. Enlaza `AGENTS.md` sin duplicarlo.
  - Sin cambios en el código de producción.
- Verificación:
  - Frontmatter validado con el parser `yaml` de Node en los cinco agentes; solo campos documentados; sin `permissionMode` ni `isolation`.
  - Confirmado que .NET 10 genera `public partial class Program` (requisito de `WebApplicationFactory<Program>`).
- Wiki: [[trabajar-con-el-agente]] (nueva sección «Ejecutar una HU de backend»).
- Pendiente: la memoria del agente (`.claude/agent-memory/backend-engineer/`) se versiona con el repo. Valores propuestos de `Resultado` en la evidencia: `Validado`, `Falla`, `Pendiente`, `Bloqueado`; ratificarlos en la skill Scrum si el equipo los adopta.

## [2026-10-09] docs | Aprobación de todas las épicas e historias de usuario
- Autor: juancarlosfc5
- Cambios:
  - La persona líder del proyecto aprobó explícitamente, en bloque, las 13 épicas y las 47 HU. Las 60 notas pasan a `estado: Aprobada` y `status: vigente`, con `updated: 2026-10-09`.
  - Cada HU registra la aprobación en su `## Historial`.
  - Se actualizaron el aviso del tablero y `PLAN-DE-TRABAJO.md`.
  - Las decisiones abiertas siguen vigentes y se ratifican en la T-01 de cada HU.
- Wiki: [[tablero-scrum]], `wiki/scrum/epicas/*`, `wiki/scrum/historias-de-usuario/*`.

## [2026-10-09] feature | Script DB-first de la base de datos (`db.sql`)
- Autor: juancarlosfc5
- Cambios:
  - Nuevo `db.sql` en la raíz para PostgreSQL 18.6, base del scaffolding con EF Core 10 + Npgsql.
  - Esquema `identity` (ASP.NET Core Identity en snake_case + claves de Data Protection) y esquema `dataticket` (14 tablas de dominio).
  - Funciones de matriz de prioridad y de número de ticket; trigger append-only de auditoría; siembra de 6 roles y 3 categorías provisionales.
- Verificación: aplicado sin errores en un `postgres:18.6` desechable con `ON_ERROR_STOP`. Las pruebas de humo rechazan lo que deben: prioridad incoherente, título de más de 120, `Closed` sin `closed_at`, participación vigente duplicada, adjunto de 10 MB + 1 B, `UPDATE`/`DELETE`/`TRUNCATE` en auditoría y empresa duplicada. El número de ticket no se trunca por encima de 6 dígitos.
- Wiki: [[persistencia-postgresql]] (nueva sección «Esquema DB-first»), [[pendientes]] (contradicción DB-first frente a migraciones; actualización de roles frente a equipos).
- Pendiente: ADR que fije el flujo DB-first frente a migraciones; paquete de convención snake_case para mapear Identity; RLS y roles de base de datos; V-06, V-12, V-13 y V-16.

## [2026-10-09] setup | frontend-engineer preparado para HU-002 a HU-004 y EP-009
- Autor: juancarlosfc5
- Cambios:
  - `.claude/agents/frontend-engineer.md` reescrito (184 líneas). Conserva `name`, `description`, `tools`, `model` y `color`, y añade `memory: project` y `effort: high`. Incluye:
    - lecturas por HU y estado actual comprobado (hoy: `httpClient` solo con GET; sin enrutador, Testing Library, jsdom ni `core/realtime`);
    - decisiones vigentes: sin ADR del enrutador → detenerse y preguntar; Testing Library + jsdom por archivo con `// @vitest-environment jsdom`;
    - flujo por HU y tabla del MVC estricto;
    - `core/http`: mutaciones, antiforgery en memoria, `HttpError` con ProblemDetails, 401 con `returnUrl` saneado, 403;
    - `core/realtime`: conexión única, resincronización `after`, `mergeMessages`, `parseHubError`, `TicketAccessRevoked`;
    - seguridad y UX, y verificación de dependencias en npm con `--save-exact`;
    - retorno con evidencia CHU/DoD y pasos de verificación manual en el navegador.
  - Nuevo `frontend/CLAUDE.md` (34 líneas): comandos, MVC y prohibiciones. Enlaza `AGENTS.md`.
  - Sin cambios en el código de producción.
- Verificación:
  - Frontmatter de los cinco agentes validado con el parser `yaml` de Node.
  - Sintaxis por archivo de Vitest 5.0.3 comprobada en su documentación.
  - Versiones candidatas consultadas en npm: `@microsoft/signalr` 10.0.11, `@testing-library/react` 16.3.3 (requiere `@testing-library/dom` ^10), `@testing-library/user-event` 14.6.7, `jsdom` 30.1.2.
- Wiki: [[trabajar-con-el-agente]] (nueva sección «Ejecutar una HU de frontend»).
- Pendiente: el término «D2» no aparece en ningún documento. Se interpretó como la decisión «Enrutador y librería de estado del frontend» ([[tablero-scrum]], [[pendientes]] §4), que sigue sin ADR y bloquea HU-002 T-02.

## [2026-10-09] fix | Correcciones de la revisión de `db.sql`
- Autor: juancarlosfc5
- Cambios (hallazgos de quality-reviewer: 0 críticos, 1 alto):
  - ALTO: FK compuesta `tickets(requester_id, company_id)` → `identity.users(id, company_id)`. El solicitante tiene que ser de la empresa del ticket y un usuario con tickets no puede cambiar de empresa.
  - FK compuestas con `ticket_id` en adjuntos (mensaje y respuesta formal) y en notificaciones.
  - Un adjunto `FormalResponse` exige `formal_response_id`.
  - El contexto `ChatMessage` pasa a `Chat`, como en HU-029/033.
  - Las versiones de formulario quedan inmutables (trigger y `ON DELETE RESTRICT`).
  - La URL de PR solo admite `https`; la descripción tiene un máximo de 10 000 caracteres; `number` pasa a `varchar(24)`.
  - Nuevo índice de auditoría por objeto; el comentario de la acción pasa a `FormalResponseDelivered`.
  - La cabecera documenta las limitaciones del scaffolding y el riesgo del superusuario.
- Verificación: el script se reaplica sin errores en un `postgres:18.6` desechable. Las 9 pruebas de regresión rechazan y aceptan lo esperado.
- Wiki: [[persistencia-postgresql]].
- Pendiente:
  - Separar los roles de BD (app frente a migraciones) antes de un entorno compartido.
  - Riesgo del cursor `(created_at, id)` del chat frente al orden de commit: reconectar con margen y deduplicar (HU-031).
  - Registrar `AttachmentContext` e `InAppNotification` frente a `Notification` en [[glosario]].
  - El GIN `jsonb_path_ops` solo cubre `@>`; definirlo en el ADR de respuestas variables.
  - `leads` de equipos (HU-009) sin columna mientras los equipos sean roles.

## [2026-10-09] decision | `db.sql` aprobado como diseño de referencia; implementación con migraciones de EF Core
- Autor: juancarlosfc5
- Cambios:
  - La persona líder del proyecto aclaró que pidió `db.sql` como paso previo para validar el esquema antes de implementarlo con EF Core. Revisó la base de datos y la aprobó.
  - Nuevo [[adr-0009-db-sql-diseno-de-referencia]] (aceptada): `db.sql` es el diseño de referencia y no se ejecuta ni se usa para scaffolding. Cada HU crea su parte con migraciones code-first equivalentes; si una HU cambia el esquema, actualiza `db.sql` en el mismo PR.
  - La contradicción «DB-first frente a migraciones» pasa a resuelta en [[pendientes]]. Los DoD de las HU siguen igual.
  - [[persistencia-postgresql]]: la sección pasa a «Diseño de referencia», con la guía para llevarlo a EF Core (Identity, snake_case, `HasCheckConstraint`, FK compuestas, `migrationBuilder.Sql`).
  - `db.sql` no se modificó, por instrucción expresa: su cabecera aún dice «DB-first» y el ADR prevalece.
- Wiki: [[adr-0009-db-sql-diseno-de-referencia]] (nueva), [[persistencia-postgresql]], [[pendientes]], [[index]].
- Pendiente: alinear la cabecera de `db.sql` cuando se toque el script; siguen abiertas las decisiones provisionales del diseño (ver ADR-0009).

## [2026-10-09] decision | Plan goal del login y loop del chat; ADR-0004, ADR-0010 y ADR-0011; ingesta de DataTicket.html
- Autor: juancarlosfc5
- Cambios:
  - Decisiones D1–D6 de la persona líder del proyecto:
    - [[adr-0004-autenticacion-cookie-mismo-origen]] pasa a **aceptada**;
    - nuevo [[adr-0010-enrutador-react-router]] (`react-router` 8.4.0 exacta, modo librería; verificado en npm: `latest`, peer `react >=19.2.7`, `node >=22.22`);
    - orden: primero el goal y después el loop;
    - commits locales por bloque y por rebanada.
  - Nuevos archivos de control en la raíz, con lista de chequeo y registro de ejecución:
    - `goal_login.md`: `/goal` de HU-002, HU-003 y HU-004 en los bloques B0–B8; el texto del comando tiene 1084 caracteres;
    - `loop_chat.md`: `/loop` autopacado de EP-009 en las rebanadas P0 y R0–R12. R0 adelanta lo mínimo de HU-018/019.
  - Ingesta de `DataTicket.html` (raíz, fuente cruda inmutable):
    - nuevas [[fuente-prototipo-dataticket-html]] y [[sistema-de-diseno]] (tokens claro/oscuro, Archivo, componentes, movimiento, mapeo de estados como propuesta);
    - nuevo [[adr-0011-estilo-visual-inspirado-en-el-prototipo]]: estilo sí, pantallas no; el orquestador opera el frontend.
  - Skills locales de `emilkowalski/skill` (subconjunto web: 11 skills, solo Markdown y revisadas) en `.claude/skills/` y `skills-lock.json`. `frontend-engineer` precarga `emil-design-eng`.
  - Agentes y esquema:
    - `frontend-engineer` y `frontend/CLAUDE.md` adoptan ADR-0010 y la guía visual (se elimina el bloqueo por falta de ADR del enrutador);
    - `orchestrator` asume la operación del frontend y el uso de los archivos de control;
    - `AGENTS.md` añade `DataTicket.html`, `db.sql`, `goal_login.md`, `loop_chat.md` y `.claude/skills/` al mapa, `DataTicket.html` como fuente cruda y la regla de estilo visual.
  - `PLAN-DE-TRABAJO.md` incluye la sección «Ejecución automatizada en curso».
- Verificación:
  - frontmatter completo y 0 enlaces rotos en las 13 páginas tocadas (script de Node);
  - `skills-lock.json` es JSON válido;
  - las skills instaladas no traen scripts ni comandos de red.
- Wiki: [[plan-goal-login-y-loop-chat]] (nueva), [[sistema-de-diseno]] (nueva), [[fuente-prototipo-dataticket-html]] (nueva), [[adr-0010-enrutador-react-router]] (nueva), [[adr-0011-estilo-visual-inspirado-en-el-prototipo]] (nueva), [[adr-0004-autenticacion-cookie-mismo-origen]], [[autenticacion-identity]], [[frontend-mvc]], [[stack-y-versiones]], [[pendientes]], [[trabajar-con-el-agente]], [[hu-002-shell-y-navegacion-por-rol]], [[hu-003-iniciar-y-cerrar-sesion]], [[hu-004-contexto-de-usuario-y-autorizacion]], [[index]].
- Pendiente:
  - logo oficial de Data Global;
  - ratificar en la T-01 de HU-003 la «puerta solo visual» y en las HU de pantallas el mapeo de colores de estado;
  - verificar en npm el paquete autoalojado de Archivo;
  - nada está en commit: abrir una rama `docs/` o `chore/` para estos cambios antes del goal.

## [2026-10-09] feature | DML de datos semilla en `db.sql`, 14 skills de Emil, estilo Apple y Docker operado desde Claude Code
- Autor: juancarlosfc5
- Cambios:
  - `db.sql`:
    - nueva sección **13. DML**, referencia del sembrador de Development de HU-010 y de los fixtures. Contenido: 50 empresas (5 inactivas), 169 usuarios (9 internos del piloto; clientes `Active`, `Invited` y `Deactivated`), 10 tickets en los seis estados, ajuste de prioridad, asignaciones con reasignación, participante retirado, toma en cobertura, 8 mensajes de chat (par con igual marca de tiempo), lecturas, notificaciones, 5 adjuntos (metadatos), 2 respuestas formales, una plantilla publicada y 36 entradas de auditoría;
    - IDs deterministas por prefijo;
    - contraseña sintética común con hash V3 de Identity;
    - cabecera alineada con ADR-0009 (ya no dice DB-first ni propone scaffolding; explica cómo validarlo en un contenedor desechable y cómo llevarlo a EF Core). Era el pendiente del log del 2026-10-09.
  - Skills: se instalan también `apple-design`, `animate-expo` y `write-swift`; quedan las 14 de `emilkowalski/skill`, revisadas (solo Markdown). `frontend-engineer` precarga `apple-design` y `emil-design-eng`.
  - Decisiones de la persona líder:
    - el PRD y las HU de Scrum rigen todo;
    - `DataTicket.html` es **solo referencia visual**, así que sus diferencias (dos puertas, respuestas públicas, estados) no se tienen en cuenta;
    - el estilo combina Apple y la estética del prototipo;
    - Docker lo opera el agente desde Claude Code, con contenedores desechables `dt-*-check` para lo que el stack no tiene.
  - `goal_login.md` y `loop_chat.md`:
    - reglas de Docker, fuente de verdad y datos semilla;
    - se eliminan la «entrada dos puertas» y las instrucciones de Docker Desktop;
    - el sembrador y los fixtures replican `db.sql` §13.
  - `AGENTS.md` §6 y §10, `orchestrator` y `frontend-engineer` actualizados con estas reglas.
- Verificación:
  - `db.sql` completo aplicado con `ON_ERROR_STOP` en un `postgres:18.6` desechable (`dt-dbsql-check`, eliminado al terminar; el stack de Compose no se tocó). Conteos y orden estable del chat correctos; siguiente número `DT-000011`; `UPDATE` en auditoría y FK compuesta multiempresa rechazados.
  - Hash de contraseña verificado con `PasswordHasher` de `Microsoft.Extensions.Identity.Core` 10.0.12: la correcta da `Success` y una incorrecta, `Failed`.
  - `skills-lock.json` válido (15 skills).
  - Wiki: 115 páginas con frontmatter y 0 enlaces rotos.
- Wiki: [[persistencia-postgresql]], [[sistema-de-diseno]], [[fuente-prototipo-dataticket-html]], [[adr-0011-estilo-visual-inspirado-en-el-prototipo]], [[autenticacion-identity]], [[pendientes]], [[trabajar-con-el-agente]], [[hu-010-datos-sinteticos-de-desarrollo]], [[index]].
- Pendiente:
  - confirmar P-21 (nombres reales del equipo con correos `@dataticket.local`);
  - los binarios de los adjuntos semilla no existen en Azurite;
  - nada está en commit.
