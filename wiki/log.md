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

## [2026-10-09] docs | Loop del core en paralelo con el chat (`loop_core.md`)
- Autor: juancarlosfc5
- Cambios:
  - Nuevo `loop_core.md` en la raíz del worktree `../DataGlobal2-core` (rama `feature/core`, creada desde `main`). Se creó ahí para no interferir con el goal del login, que corre en la carpeta principal. Construye una HU por iteración, 35 en total: EP-001 (HU-001), EP-003, HU-005/006, EP-004 a EP-008, EP-010, EP-011, EP-012 y EP-013. Los hitos N1 (núcleo compartido `core-nucleo-v1`), F1 y F2 marcan los momentos de fusionar.
  - Reglas de paralelismo:
    - worktree propio;
    - stack de Docker aislado `dataticket-core` con otros puertos (`.env.core`);
    - tabla de propiedad del código entre loops y cambios aditivos en los archivos compartidos;
    - `git merge main` en cada iteración;
    - resolución de conflictos de migraciones.
  - `loop_chat.md`: R0 ya no construye un núcleo propio; integra `core-nucleo-v1` y espera si aún no existe. Se añade la sincronización con `main` en cada iteración.
  - [[plan-goal-login-y-loop-chat]] (sección «Loop del core» y riesgos), [[trabajar-con-el-agente]], `AGENTS.md` §2 y `PLAN-DE-TRABAJO.md`.
- Verificación: `git worktree list` muestra los dos árboles; la carpeta principal (goal en curso en `feature/auth-login`) no se modificó; enlaces y frontmatter de la wiki del worktree validados.
- Wiki: [[plan-goal-login-y-loop-chat]], [[trabajar-con-el-agente]].
- Pendiente: ninguno; ver la corrección siguiente.

## [2026-10-09] fix | `loop_core.md` va en `main`; sin rama `feature/core` previa
- Autor: juancarlosfc5
- Cambios:
  - La persona líder no quería una rama nueva: `loop_core.md` y los ajustes anteriores (`loop_chat.md`, wiki, `AGENTS.md`, `PLAN-DE-TRABAJO.md`) se registran directamente en `main`.
  - Se eliminan el worktree `../DataGlobal2-core` y la rama `feature/core` creados antes.
  - `loop_core.md` explica cómo crear el worktree y la rama de trabajo desde `main` al lanzar el loop, después de fusionar `feature/auth-login`. La iteración P0 comprueba la carpeta, la rama y que el login esté en `main`.
- Verificación: `git worktree list` y `git branch` sin `feature/core`. La carpeta principal (goal en curso en `feature/auth-login`) no se modificó.
- Wiki: [[plan-goal-login-y-loop-chat]], [[trabajar-con-el-agente]].
## [2026-10-09] setup | Goal del login — B0 preparación
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`)
- Cambios: rama `feature/auth-login`; línea base verificada (`dotnet test` 5/5, `oxlint`, `vitest` 3/3, `vite build`); stack de Compose healthy; HU-002, HU-003 y HU-004 pasan a `En desarrollo` con responsable en `PLAN-DE-TRABAJO.md`.
- Wiki: [[hu-002-shell-y-navegacion-por-rol]], [[hu-003-iniciar-y-cerrar-sesion]], [[hu-004-contexto-de-usuario-y-autorizacion]]
- Pendiente: bloques B1–B8.

## [2026-10-09] decision | Goal del login — B1 contrato de autenticación (T-01 de HU-002/003/004)
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`)
- Cambios: contrato de `/api/auth/antiforgery`, `/login`, `/logout`, `/me`, políticas, públicos y rutas del SPA ratificado con los valores por defecto del goal; equipos implementados como roles de Identity (`db.sql`) pero expuestos en `teams`; nuevos términos en el glosario.
- Wiki: [[autenticacion-identity]], [[glosario]], [[pendientes]] (P-22, roles vs. equipos), [[hu-002-shell-y-navegacion-por-rol]], [[hu-003-iniciar-y-cerrar-sesion]], [[hu-004-contexto-de-usuario-y-autorizacion]]
- Pendiente: ratificar P-22 con el equipo; decisión definitiva de equipos en HU-009.

## [2026-10-09] infra | Goal del login — B2 infraestructura de pruebas de integración (backend)
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; implementación de `backend-engineer`)
- Cambios: proyecto `DataTicket.IntegrationTests` (`WebApplicationFactory<Program>` + Testcontainers `postgres:18.6`, base aislada por clase, rasgo `RequiresDocker`), catálogo de cuentas de `db.sql` §13, `TimeProvider` en DI sustituible por `FakeTimeProvider`; `Dockerfile` restaura la solución completa. `dotnet test` 19/19.
- Hallazgo: el bloqueo de `UserManager` y los tokens de `DataProtectorTokenProvider` no respetan `TimeProvider` (relevante para CHU-05 de HU-003 y para HU-005/HU-006).
- Wiki: [[estrategia-de-pruebas]], [[backend-hexagonal]], [[entorno-docker]], [[stack-y-versiones]]
- Pendiente: revisión de `quality-reviewer` de B2; B5 (frontend) en curso en paralelo.

## [2026-10-09] feature | Goal del login — B3 y B5 en curso (registro intermedio)
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`)
- Cambios: `frontend-engineer` trabaja en B5 (shell, navegación por rol, `core/http`, tokens) y `backend-engineer` en B3 (Identity, migración, login/logout). Los archivos de `frontend/` y `backend/` aún no están cerrados ni verificados por el orquestador.
- Wiki: sin cambios de contenido todavía; [[frontend-mvc]], [[estrategia-de-pruebas]], [[autenticacion-identity]] y [[persistencia-postgresql]] se actualizan al cerrar cada bloque.
- Pendiente: verificación, revisión de `quality-reviewer` y commit de B3 y B5.

## [2026-10-09] feature | Goal del login — B5 shell, navegación por rol y cliente HTTP (frontend, en revisión)
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; implementación de `frontend-engineer`)
- Cambios: `react-router` 8.4.0, Testing Library + jsdom, `tokens.css` claro/oscuro y Archivo autoalojada; `core/http` con mutaciones, antiforgery y `HttpError`; módulo `session` (modelo, navegación por rol, `sanitizeReturnUrl`, controlador, guardas, layouts, «sin permiso»). lint 0/0, 89 pruebas, build OK. CHU-10 verificado en el navegador integrado (375 px oscuro y 1440 px claro, sin desbordamiento) con un doble del contrato.
- Wiki: [[frontend-mvc]], [[estrategia-de-pruebas]], [[stack-y-versiones]], [[sistema-de-diseno]], [[glosario]], [[hu-002-shell-y-navegacion-por-rol]]
- Pendiente: B3 (backend) en curso. Revisión de B5: 0 CRÍTICO, 0 ALTO; 4 MEDIO pasan a B6.

## [2026-10-09] decision | Contrato: código estable del error antiforgery
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`)
- Cambios: el `400` por antiforgery inválido incluye `"code": "antiforgery_invalid"` para que el SPA invalide su token y reintente una vez (hallazgo MEDIO de la revisión de B5). B5 commiteado (`81dcbab`); B6 en curso con las correcciones MEDIO.
- Wiki: [[autenticacion-identity]]

## [2026-10-09] feature | Goal del login — B6 entrada y sesión (frontend, en revisión)
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; implementación de `frontend-engineer`)
- Cambios: módulo `auth` (`/ingresar`, validación local, mensaje genérico, logout real), guarda por sección declarada, reintento único ante `antiforgery_invalid`, aborto del `/me` en curso. lint 0/0, 147 pruebas, build OK; `/ingresar` revisada en el navegador (claro forzado, 375 y 1440 px).
- Incidencia: el disco C: se llenó (≈3 MB libres); se vaciaron los cachés de npm (9,2 GB) y HTTP de NuGet con sus comandos propios. Docker (`Docker\wsl`, 21,7 GB) es el mayor consumidor.
- Wiki: [[frontend-mvc]], [[autenticacion-identity]], [[estrategia-de-pruebas]], [[sistema-de-diseno]]
- Pendiente: B3 (backend) en curso. Revisión de B6: 0 CRÍTICO, 0 ALTO (3 BAJO a B7).

## [2026-10-09] infra | Goal del login — Docker sin respuesta tras el disco lleno (bloqueo)
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`)
- Cambios: correcciones BAJO de la revisión de B6 en el frontend (aviso de logout obsoleto, error de `/me` tras el login con «Reintentar», `method="post"`), 150 pruebas en verde, sin commit (entran con B7). B3 del backend a medio camino (migración `AddIdentityAndCompanies` creada).
- Incidencia: tras llenarse el disco C:, `docker version` no responde (procesos de Docker Desktop/WSL vivos). Según las reglas del goal, se detiene la ejecución y se consulta a la persona líder.
- Wiki: [[entorno-docker]] (sin cambios de contenido)
- Pendiente: recuperar Docker; terminar B3, luego B4, B7 y B8.

## [2026-10-12] feature | Goal del login — reanudación tras recuperar Docker; B3 verificado y en revisión
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`)
- Cambios: la persona líder reinició Docker y liberó disco (58 GB libres; 5 servicios arriba, `db` y `backend` healthy); el orquestador no modificó Docker. B3 del backend, que quedó sin informe por el corte de sesión, se verificó de nuevo: `dotnet build` 0 advertencias / 0 errores y `dotnet test` 70/70 (exit 0). Rojo de TDD comprobado a posteriori en una copia fuera del repo: sin `api.MapAuthEndpoints()` fallan 19 de 70, incluidas las 11 pruebas del DoD-02 de HU-003.
- Hallazgo: la base `db` del Compose aún no tiene la migración `AddIdentityAndCompanies` (el `backend` del stack corre la imagen anterior); se aplicará al reconstruir el servicio, después de la revisión.
- Wiki: sin cambios de contenido; [[autenticacion-identity]], [[persistencia-postgresql]], [[backend-hexagonal]], [[estrategia-de-pruebas]] y [[glosario]] se actualizan al cerrar B3.
- Pendiente: veredicto de `quality-reviewer` sobre B3, cierre de wiki y commit de B3; después B4, B7 y B8.

## [2026-10-12] feature | Goal del login — B3 revisado; correcciones en curso y wiki de B3 al día
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; revisión de `quality-reviewer`, wiki de `wiki-keeper`)
- Cambios: `quality-reviewer` sobre B3: 0 CRÍTICO, 0 ALTO (4 MEDIO, 10 BAJO); la migración `AddIdentityAndCompanies` coincide con `db.sql` §2–§3. `backend-engineer` está corrigiendo en `backend/` los MEDIO y BAJO de la revisión (401/403 con cualquier `Accept`, política `Secure`, defensa de tiempos con hash señuelo del hasher inyectado, JSON mal formado, migración solo en Development, reglas de arquitectura); esos archivos aún no están verificados por el orquestador.
- Wiki: [[autenticacion-identity]], [[persistencia-postgresql]], [[backend-hexagonal]], [[estrategia-de-pruebas]], [[glosario]], [[stack-y-versiones]], [[entorno-docker]], [[pendientes]], [[index]]
- Pendiente: verificar las correcciones, aplicar la migración al `db` del Compose reconstruyendo `backend`, marcar y commitear B3. Preguntas abiertas en [[pendientes]]: IDs fijos de roles frente a `db.sql` §12, estado `propuesta` de [[autenticacion-identity]], endurecimiento del login y cookie no persistente.

## [2026-10-12] feature | Goal del login — B3 iniciar y cerrar sesión (backend)
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; implementación de `backend-engineer`, revisión de `quality-reviewer`)
- Cambios: Identity sobre `DataTicketDbContext`, migración `AddIdentityAndCompanies`, endpoints `antiforgery`/`login`/`logout`, cookie, bloqueo con `TimeProvider`, sello de seguridad y claves de Data Protection en PostgreSQL. Correcciones de la revisión: 401/403 `problem+json` con cualquier `Accept` (antes 500), JSON mal formado → 400, hash señuelo `DecoyPasswordHash` con el hasher de la aplicación, reglas de arquitectura para `DataTicket.UnitTests` y la ubicación de los puertos.
- Verificación: `dotnet build` 0/0; `dotnet test` 96/96 (exit 0); `quality-reviewer` 0 CRÍTICO, 0 ALTO; `docker compose up --build -d backend` aplicó la migración al `db` del Compose (9 tablas, 6 roles, sin datos de prueba) y los endpoints responden en `:8080`.
- Wiki: [[autenticacion-identity]], [[persistencia-postgresql]], [[backend-hexagonal]], [[estrategia-de-pruebas]], [[glosario]], [[stack-y-versiones]], [[entorno-docker]], [[pendientes]], [[index]]
- Pendiente: B4 (`ICurrentUser`, claims, políticas, *fallback*, `/api/auth/me` y prueba de convención de antiforgery); B7 y B8. Preguntas abiertas en [[pendientes]]: IDs fijos de roles frente a `db.sql` §12, endurecimiento del login, idioma del 400 genérico.

## [2026-10-12] feature | Goal del login — B4 contexto de usuario y autorización (backend, en revisión)
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; implementación de `backend-engineer`)
- Cambios: puerto `ICurrentUser` con adaptador en `Api`, `DataTicketClaimsPrincipalFactory` (`company_id` solo para clientes y solo desde la cuenta), políticas con nombre, política de reserva autenticada, `GET /api/auth/me`, pruebas de convención (lista pública de anónimos, antiforgery en toda mutación de `/api`, DTO de cliente sin `CompanyId`). Sin migración. Verificado por el orquestador: `dotnet build` 0/0 y `dotnet test` 146/146 (exit 0).
- Decisión por ratificar: la política de reserva se aplica solo a endpoints registrados, de modo que una ruta inexistente responde 404 también a un anónimo (conserva HU-003 CHU-09); se registra en [[pendientes]].
- Wiki: en curso con `wiki-keeper` — [[autenticacion-identity]], [[roles-y-permisos]], [[backend-hexagonal]], [[estrategia-de-pruebas]], [[glosario]], [[pendientes]], [[index]]
- Pendiente: veredicto de `quality-reviewer` sobre B4, marcar y commitear B4; después B7 y B8.

## [2026-10-12] feature | Goal del login — B4 revisado; correcciones en curso y wiki de B4 al día
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; revisión de `quality-reviewer`, wiki de `wiki-keeper`)
- Cambios: `quality-reviewer` sobre B4: 0 CRÍTICO, 0 ALTO (2 MEDIO, 6 BAJO); aprueba el 404 para rutas inexistentes con mitigación. `backend-engineer` corrige en `backend/`: los roles de la sesión deben salir solo de `identity.user_roles` (hoy un claim de rol almacenado en `user_claims`/`role_claims` llega a la cookie), la fábrica de claims debe fallar cerrado con perfil no válido, regla «todo contenido es un endpoint» con prueba, convención antiforgery para toda mutación y políticas registradas desde una única tabla. Esos archivos aún no están verificados por el orquestador.
- Wiki: [[autenticacion-identity]], [[roles-y-permisos]], [[backend-hexagonal]], [[estrategia-de-pruebas]], [[glosario]], [[pendientes]], [[criterios-de-aceptacion]], [[index]]
- Pendiente: verificar las correcciones, marcar y commitear B4; B7 y B8. Por ratificar en [[pendientes]] (P-23): 404 frente a 401 en rutas inexistentes, 403 de `/me` con perfil no válido y claim `display_name`. Nota para el loop del chat: decidir cómo obtiene `ICurrentUser` el hub de SignalR (`IHttpContextAccessor` no es fiable dentro de las invocaciones).

## [2026-10-12] feature | Goal del login — B4 contexto de usuario y autorización (backend)
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; implementación de `backend-engineer`, revisión de `quality-reviewer`, wiki de `wiki-keeper`)
- Cambios: puerto `ICurrentUser` con adaptador en `Api`, caso de uso `GetCurrentUserProfile`, `DataTicketClaimsPrincipalFactory` con lista cerrada de claims (roles solo de `identity.user_roles`; `company_id` solo para clientes y solo desde la cuenta), cuatro políticas con nombre desde una única tabla, política de reserva autenticada, `GET /api/auth/me`, regla «todo contenido es un endpoint» y pruebas de convención (lista pública, antiforgery en toda mutación, DTO de cliente sin `CompanyId`). Sin migración.
- Verificación: `dotnet build` 0/0; `dotnet test` 154/154 (exit 0; 84 sin Docker); `quality-reviewer` 0 CRÍTICO, 0 ALTO, con sus 2 MEDIO y 6 BAJO corregidos con prueba.
- Wiki: [[autenticacion-identity]], [[roles-y-permisos]], [[backend-hexagonal]], [[estrategia-de-pruebas]], [[glosario]], [[pendientes]], [[criterios-de-aceptacion]], [[index]]
- Pendiente: B7 (sembrador de Development, stack reconstruido y verificación en navegador) y B8. Por ratificar (P-23): 404 en rutas inexistentes, 403 de `/me` con perfil no válido, claim `display_name`; variante estricta del manejador de la reserva.

## [2026-10-12] feature | Goal del login — B7 en curso: sembrador mínimo de Development
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`)
- Cambios: B4 commiteado (`a6c4d5f`). `backend-engineer` implementa en `backend/` el sembrador solo de Development (subconjunto mínimo de `db.sql` §13.2–13.4 con los mismos IDs, idempotente, con doble guarda de entorno y configuración); esos archivos aún no están verificados por el orquestador. Las tres correcciones BAJO del frontend pendientes desde B6 siguen sin commit y entran en B7 (lint 0/0, 150 pruebas, build OK).
- Wiki: sin cambios de contenido todavía; [[entorno-docker]], [[persistencia-postgresql]] y [[autenticacion-identity]] se actualizan al cerrar B7.
- Pendiente: verificar el sembrador, `docker compose up --build -d backend frontend`, comprobación con `curl` y en el navegador integrado (cuenta interna y cliente, claro/oscuro, 375/1440 px, teclado); después B8.

## [2026-10-12] fix | Goal del login — B7: el backend del stack no arrancaba por segunda vez (historial de migraciones)
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`)
- Cambios: sembrador de Development implementado por `backend-engineer` y verificado (`dotnet test` 167/167). Al reconstruir `backend` y `frontend` con `docker compose up --build -d`, el backend falló al arrancar: reaplicaba `AddIdentityAndCompanies` (`42P07: relation "companies" already exists`).
- Causa (comprobada en solo lectura): el usuario de PostgreSQL del Compose se llama `dataticket`, como el esquema de la migración, y `search_path` es `"$user", public`; la tabla de historial de EF Core no tenía esquema explícito, así que se creó en `public` en el primer arranque y se buscó en `dataticket` en el segundo. Las pruebas usan otro usuario y no lo detectaban. Quedó una tabla vacía `dataticket."__EFMigrationsHistory"` creada por el arranque fallido; no se ha tocado el `db` a mano.
- Wiki: sin cambios de contenido todavía; [[persistencia-postgresql]] y [[entorno-docker]] se actualizan al verificar el arreglo.
- Pendiente: arreglo en curso (esquema explícito del historial + prueba de regresión con dos arranques), reconstruir el stack y seguir con B7.

## [2026-10-12] feature | Goal del login — B7: stack reconstruido y verificado con curl y navegador (en revisión)
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; sembrador y arreglo de `backend-engineer`)
- Cambios: sembrador solo de Development (`Infrastructure/Seeding`) y arreglo del historial de migraciones (`MigrationsHistoryTable` en `public` + `MigrationHistoryTests`; Testcontainers con el usuario del Compose). `dotnet test` 169/169 (exit 0). `docker compose up --build -d backend frontend` (exit 0); dos arranques seguidos sanos; 1 empresa y 5 cuentas sembradas.
- Verificación: por el proxy de `:5173`, `antiforgery` 200 → `login` 204 → `/me` 200 → `logout` 204 → `/me` 401 con Elizabeth (interna) y Ana Pérez (cliente); `Invited` y `Deactivated` → 401 genérico. Navegador integrado: sin sesión `/interno/bandeja` → `/ingresar?returnUrl=…`; intento fallido con mensaje genérico; ingreso por teclado a `/interno/bandeja` y a `/portal`; «sin permiso» fuera de la superficie o sección; cierre de sesión por teclado → `/ingresar`; claro y oscuro, 375 y 1440 px sin desbordamiento (`scrollWidth = innerWidth`), foco visible. Hallazgo de teclado: en `/ingresar` el enlace «¿Olvidaste tu contraseña?» queda entre Correo y Contraseña en el orden de tabulación; `frontend-engineer` lo corrige.
- Wiki: [[persistencia-postgresql]], [[entorno-docker]], [[estrategia-de-pruebas]], [[backend-hexagonal]], [[pendientes]]
- Pendiente: veredicto de `quality-reviewer` sobre B7, corrección del orden de foco, reconstruir `frontend`, marcar y commitear B7; después B8.

## [2026-10-12] fix | Goal del login — B7: orden de foco y alto del botón en `/ingresar`
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; orden de foco de `frontend-engineer`)
- Cambios: el enlace «¿Olvidaste tu contraseña?» pasa debajo de «Ingresar» (orden de foco Correo → Contraseña → Ingresar → enlace, igual que el DOM). `src/main.tsx` importa `tokens.css` y `global.css` antes que `App`: los estilos base se cargaban después de los del módulo y `.btn` dejaba «Ingresar» en 30 px en vez de 48 px.
- Verificación: lint 0/0, 152 pruebas, build OK (exit 0); `docker compose up --build -d frontend`; en el navegador, botón de 48 px a 375 y 1440 px, sin desbordamiento, orden de foco comprobado con Tab.
- Wiki: [[sistema-de-diseno]], [[frontend-mvc]]
- Pendiente: veredicto de `quality-reviewer` sobre B7, marcar y commitear B7; después B8.

## [2026-10-12] feature | Goal del login — B7 integración y verificación manual
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; `backend-engineer`, `frontend-engineer`, revisión de `quality-reviewer`)
- Cambios: sembrador solo de Development; historial de migraciones fijado en `public` (el backend del stack no arrancaba por segunda vez); `appsettings.Development.json` excluido de la publicación; en `/ingresar`, orden de foco corregido y estilos base importados antes que `App`; correcciones menores pendientes de B6 (aviso de logout, error de `/me` tras el login, `method="post"`).
- Verificación: `dotnet build` 0/0 y `dotnet test` 169/169 (exit 0); lint 0/0, 152 pruebas Vitest y build (exit 0); `docker compose up --build -d backend frontend` (exit 0), dos arranques sanos; flujo completo con `curl` y en el navegador integrado con una cuenta interna y una cliente; `quality-reviewer` 0 CRÍTICO, 0 ALTO (su MEDIO, corregido).
- Wiki: [[persistencia-postgresql]], [[entorno-docker]], [[estrategia-de-pruebas]], [[backend-hexagonal]], [[sistema-de-diseno]], [[frontend-mvc]], [[pendientes]]
- Pendiente: B8 (revisión final de la rama, matrices de las HU, verificación final y borrador del PR). Residuo local: tabla vacía `dataticket."__EFMigrationsHistory"` en el `db` del Compose (inofensiva; no se tocó).

## [2026-10-12] docs | Goal del login — B8 en curso: revisión final de la rama y aclaración del alcance de `docker build --target test`
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; revisión de `quality-reviewer`)
- Cambios: revisión final de `feature/auth-login` frente a `main` (8 commits): 0 CRÍTICO, 0 ALTO abiertos; 2 MEDIO de wiki (tablero y matrices de las HU, en curso con `wiki-keeper`) y 8 BAJO. `AGENTS.md` §10 y `backend/CLAUDE.md` aclaran que `docker build --target test` ejecuta solo las pruebas que no necesitan Docker (excluye `Category=RequiresDocker`); la batería completa es `cd backend && dotnet test`. HU-002 CHU-10 medido también a 360 y 1280 px en el navegador contra el stack (navegación y «Cerrar sesión» dentro del viewport, `scrollWidth = innerWidth`).
- Wiki: sin cambios de contenido en esta entrada; las matrices de [[hu-002-shell-y-navegacion-por-rol]], [[hu-003-iniciar-y-cerrar-sesion]] y [[hu-004-contexto-de-usuario-y-autorizacion]], [[tablero-scrum]] y [[pendientes]] se cierran con B8.
- Pendiente: cierre de wiki de B8, verificación final de los cuatro comandos, commit de B8 y borrador del PR. Decisión de la persona líder: la contraseña sintética aparece literal en los criterios de HU-003 y HU-005 (viene de `main`).

## [2026-10-12] docs | Goal del login — B8 calidad y cierre
- Autor: juancarlosfc5 (orquestador, `/goal` de `goal_login.md`; revisión de `quality-reviewer`, matrices y lint de `wiki-keeper`)
- Cambios: revisión final de la rama `feature/auth-login` (0 CRÍTICO, 0 ALTO abiertos). HU-002, HU-003 y HU-004 pasan a `En validación` con su matriz de evidencia completa (18/20, 23/24 y 19/20 filas en `Cumple`; lo pendiente depende del PR y de su revisión por otra persona). Tablero, épicas EP-001 y EP-002 y `PLAN-DE-TRABAJO.md` actualizados. Lint acotado de la wiki: 0 enlaces rotos. Hallazgos BAJO de la revisión final registrados en [[pendientes]] §4.
- Verificación final: `cd backend && dotnet test` 169/169 (exit 0); `npm --prefix frontend run lint` sin hallazgos (exit 0); `npm --prefix frontend test` 17 archivos / 152 pruebas (exit 0); `npm --prefix frontend run build` (exit 0).
- Wiki: [[hu-002-shell-y-navegacion-por-rol]], [[hu-003-iniciar-y-cerrar-sesion]], [[hu-004-contexto-de-usuario-y-autorizacion]], [[tablero-scrum]], [[ep-001-fundaciones-tecnicas]], [[ep-002-identidad-y-acceso]], [[estrategia-de-pruebas]], [[glosario]], [[autenticacion-identity]], [[entorno-docker]], [[backend-hexagonal]], [[frontend-mvc]], [[vision-general]], [[pendientes]], [[index]]
- Pendiente: abrir el PR (sin push todavía) y su revisión por otra persona; capturas de 360/1280 px en el PR; ratificar P-18, P-22 y P-23; decidir sobre la contraseña sintética escrita en los criterios de HU-003 y HU-005; después, `loop_chat.md`.

## [2026-10-12] docs | Goal del login — rama publicada y fusionada en `main`; cierre del goal
- Autor: juancarlosfc5 (fusión y push de `main`, hechos por la persona líder; registro del orquestador)
- Cambios: `feature/auth-login` (9 commits, B0–B8) publicada en `origin` por el orquestador. La persona líder la fusionó en `main` con `git merge --no-ff` (`718e0a8`, 2026-10-12 19:53) y publicó `main`; la fusión incluye también `eaef0ff` (`loop_core.md`), que solo existía en su `main` local. Sin conflictos. El orquestador no ejecutó la fusión: el control de permisos de la sesión la bloqueó por ser una fusión sin revisión, y `gh` no está instalado para abrir el Pull Request.
- Contradicción registrada: `AGENTS.md` §8 pide Pull Request con revisión de otra persona; se aplicó la instrucción explícita de la persona usuaria (`AGENTS.md` §3). HU-002, HU-003 y HU-004 siguen `En validación` hasta esa revisión.
- Wiki: [[pendientes]], [[hu-002-shell-y-navegacion-por-rol]], [[hu-003-iniciar-y-cerrar-sesion]], [[hu-004-contexto-de-usuario-y-autorizacion]], [[tablero-scrum]], [[ep-001-fundaciones-tecnicas]], [[ep-002-identidad-y-acceso]], [[vision-general]]; fuera de la bóveda, `PLAN-DE-TRABAJO.md` y `goal_login.md` §4.
- Pendiente: revisión de lo fusionado por otra persona del equipo (`git diff 5bc3418..718e0a8`) y capturas de 360/1280 px; ratificar P-18, P-22 y P-23; decidir sobre la contraseña sintética escrita en los criterios de HU-003 y HU-005, la tabla vacía `dataticket."__EFMigrationsHistory"` del `db` local y la alineación de `db.sql` §12; decidir cómo obtiene `ICurrentUser` el hub antes de `loop_chat.md`.

## [2026-10-14] docs | Goal del login — cierre: HU-002, HU-003 y HU-004 completadas y P-18, P-22, P-23 ratificadas
- Autor: juancarlosfc5 (decisión de la persona líder; registro del orquestador)
- Cambios: la persona líder da por completadas HU-002, HU-003 y HU-004 (dispensa la revisión por otra persona del equipo y las capturas adjuntas al PR; filas `Dispensado` en las matrices) y ratifica P-18, P-22 y P-23, incluido el `403` de `/me` con perfil no válido; [[autenticacion-identity]] pasa a `status: vigente`. Se eliminó del `db` del Compose la tabla vacía `dataticket."__EFMigrationsHistory"` (0 filas comprobadas antes; queda solo `public."__EFMigrationsHistory"` con `AddIdentityAndCompanies`; backend `200`). Decisión técnica sobre el usuario actual en el hub de SignalR registrada para `loop_chat.md`.
- Contradicción: cerrar sin revisión de otra persona se aparta de `AGENTS.md` §8 y de la regla de cierre de `scrum-spec-orchestrator`; se aplicó la instrucción explícita de la persona usuaria (`AGENTS.md` §3), anotado en [[pendientes]] §1.
- Wiki: [[pendientes]], [[autenticacion-identity]], [[hu-002-shell-y-navegacion-por-rol]], [[hu-003-iniciar-y-cerrar-sesion]], [[hu-004-contexto-de-usuario-y-autorizacion]], [[tablero-scrum]], [[ep-001-fundaciones-tecnicas]], [[ep-002-identidad-y-acceso]], [[tiempo-real-signalr]], [[vision-general]], [[index]]; fuera de la bóveda, `PLAN-DE-TRABAJO.md` y `goal_login.md` §4.
- Pendiente: goal del login cerrado; siguen `loop_core.md` y `loop_chat.md`. Quedan en el backlog, sin bloquear: endurecimiento del login (limitador, cifrado de claves, `UseForwardedHeaders`, empresas inactivas en HU-007), contraseña sintética escrita en los criterios de HU-003 y HU-005, alineación de `db.sql` §12 con los IDs fijos de los roles y los BAJO de la revisión final.
