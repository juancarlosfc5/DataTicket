# DataTicket — Esquema para agentes de IA

Guía operativa para **cualquier agente de IA** que trabaje en este repositorio (Claude Code, Codex, Cursor u otro). En Claude Code se carga desde `CLAUDE.md` y el hilo principal corre como el agente [`orchestrator`](.claude/agents/orchestrator.md).

Este archivo es el **esquema** de la wiki del proyecto (patrón *LLM Wiki*): define cómo está organizada, qué convenciones sigue y qué flujo se ejecuta en cada interacción. Se co-evoluciona con el equipo; si cambias una convención, actualiza este archivo y registra el cambio en `wiki/log.md`.

---

## 1. El proyecto

**DataTicket** es el portal web de soporte de Data Global S.A.S.: los clientes radican tickets; la PM (Elizabeth) hace triage y asigna; Desarrollo y Producción colaboran en un chat interno en tiempo real; la PM o un administrador entrega una respuesta formal y cierra manualmente. Detalle en `wiki/vision-general.md`.

| Capa | Tecnología | Arquitectura |
|---|---|---|
| Backend | C# / .NET 10 (LTS), ASP.NET Core, **ASP.NET Core Identity**, **SignalR**, EF Core + Npgsql | **Hexagonal** (puertos y adaptadores) |
| Frontend | React 19 + TypeScript, Vite, Node 24, `@microsoft/signalr` | **MVC** (modelos, controladores-hook, vistas) |
| Datos | PostgreSQL 18 | Historial de chat y auditoría persistidos |
| Entorno | Docker Compose (monorepo contenedorizado) | `db`, `backend`, `frontend`, `mailpit`, `azurite` |

## 2. Mapa del repositorio

```text
/
├── AGENTS.md              ← este esquema (agnóstico de herramienta)
├── CLAUDE.md              ← entrada de Claude Code (importa AGENTS.md)
├── PRD.md                 ← fuente cruda principal (inmutable salvo petición explícita)
├── PLAN-DE-TRABAJO.md     ← seguimiento por fase y HU (checkmark, responsable, fecha)
├── DataTicket.html        ← prototipo navegable: fuente cruda de la inspiración visual (inmutable)
├── db.sql                 ← diseño de referencia del esquema (ADR-0009)
├── goal_login.md          ← archivo de control del /goal del login (lista de chequeo)
├── loop_chat.md           ← archivo de control del /loop del chat SignalR (lista de chequeo)
├── docker-compose.yml     ← entorno local completo
├── .env.example           ← variables sobrescribibles (copiar a .env, que no se versiona)
├── .claude/
│   ├── settings.json      ← hilo principal = orchestrator + hook Stop de la wiki
│   ├── agents/            ← orchestrator, backend-engineer, frontend-engineer, quality-reviewer, wiki-keeper
│   ├── skills/            ← skills locales (using-agent-skills; diseño y movimiento de emilkowalski/skill)
│   └── hooks/             ← wiki-guard.mjs
├── backend/               ← solución .NET 10 (DataTicket.slnx), hexagonal
├── frontend/              ← SPA React 19 + TS (Vite), MVC
├── scrum-spec-orchestrator/ ← skill para épicas, HU, DoD y validación Scrum (escribe en wiki/scrum/)
└── wiki/                  ← bóveda Obsidian: conocimiento del proyecto (la mantiene el LLM)
```

## 3. Fuentes de verdad y precedencia

1. Instrucciones explícitas de la persona usuaria en la conversación actual.
2. `PRD.md`, `DataTicket.html` (prototipo) y demás fuentes crudas en `wiki/raw/`. **Inmutables**: se leen, no se editan (salvo petición explícita).
3. Decisiones con `status: aceptada` en `wiki/decisiones/`.
4. El resto de la wiki (síntesis mantenida por el LLM).
5. El código existente.

Si dos fuentes se contradicen, **no elijas en silencio**: aplica la de mayor precedencia, registra la contradicción en `wiki/pendientes.md` con un callout `> [!warning]` y avísalo en la respuesta.

## 4. Protocolo obligatorio en cada interacción

1. **Orientarse.** Lee `wiki/index.md`, abre las páginas relacionadas y revisa las últimas entradas del log (`grep "^## \[" wiki/log.md | tail -5`). Ve al PRD solo para verificar o citar un detalle.
2. **Clasificar** la petición: `consulta`, `feature`, `fix`, `refactor`, `infra`, `decision`, `ingesta`, `lint`, `docs`.
3. **Planificar** si el cambio no es trivial: capas afectadas, criterios de aceptación del PRD involucrados (`wiki/producto/criterios-de-aceptacion.md`), pruebas a escribir primero.
4. **Ejecutar**: delega en el agente especialista adecuado (ver §9). Pruebas primero (TDD).
5. **Verificar** con comandos reales (§10). Nunca afirmes que algo funciona sin haberlo ejecutado; si algo falla o se omitió, dilo.
6. **Actualizar la wiki** según la tabla §5.6: páginas afectadas → `index.md` si hay páginas nuevas o renombradas → **entrada en `log.md`**. Si la interacción no produjo cambios ni conocimiento nuevo, no hace falta tocarla.
7. **Responder** con un resumen breve que incluya las páginas de la wiki creadas o modificadas.

## 5. La wiki

### 5.1 Capas

| Capa | Ubicación | Quién escribe |
|---|---|---|
| Fuentes crudas | `PRD.md` y `DataTicket.html` (raíz) y `wiki/raw/` (actas, prototipos, diseños; adjuntos en `wiki/raw/assets/`) | Personas. El LLM solo lee. |
| Wiki | `wiki/**/*.md` (salvo `raw/`) | El LLM (personas revisan) |
| Esquema | `AGENTS.md` | Personas + LLM, de común acuerdo |

### 5.2 Estructura

```text
wiki/
├── index.md            ← catálogo de todas las páginas (leer primero)
├── log.md              ← bitácora cronológica append-only
├── vision-general.md   ← síntesis del proyecto (hub)
├── pendientes.md       ← decisiones abiertas y contradicciones
├── glosario.md         ← lenguaje ubicuo español ↔ nombres en código
├── fuentes/            ← un resumen por fuente cruda ingerida
├── producto/           ← requisitos funcionales destilados del PRD
├── dominio/            ← modelo de dominio
├── personas/           ← equipos, roles y personas
├── arquitectura/       ← backend, frontend, Identity, SignalR, datos, Docker, versiones
├── decisiones/         ← ADR numerados (adr-NNNN-titulo.md)
├── proceso/            ← GitHub, pruebas, cómo trabajar con el agente
├── sintesis/           ← respuestas y análisis valiosos archivados desde consultas
├── scrum/              ← tablero-scrum, epicas/ y historias-de-usuario/ (skill scrum-spec-orchestrator)
├── plantillas/         ← plantillas de Obsidian (no son contenido)
└── raw/                ← fuentes crudas adicionales (inmutables)
```

### 5.3 Convenciones Obsidian

- **La bóveda es la carpeta `wiki/`** (en Obsidian: *Open folder as vault* → `wiki`).
- **Nombres de archivo**: `kebab-case`, en español, **sin tildes ni ñ** (evita problemas de normalización entre Windows/macOS/Linux) y **únicos en toda la bóveda**. El título con tildes va en el H1 y en `aliases`.
- **Enlaces internos**: wikilinks sin carpeta, `[[flujo-del-ticket]]` o `[[flujo-del-ticket|Flujo del ticket]]`. Nunca enlaces markdown relativos entre páginas de la wiki.
- **Archivos fuera de la bóveda** (código, `PRD.md`): ruta en código, p. ej. `` `backend/src/DataTicket.Api/Program.cs` ``.
- **Frontmatter YAML obligatorio** en cada página:

  ```yaml
  ---
  title: Flujo del ticket
  type: concepto        # vision | concepto | entidad | fuente | arquitectura | decision | guia | sintesis | pendientes | indice | log | epica | historia-de-usuario
  status: vigente       # borrador | vigente | propuesta | aceptada | reemplazada | obsoleta
  tags: [producto, ticket]
  sources: ["PRD.md §6"]
  aliases: [Ciclo de vida del ticket]
  created: 2026-10-07
  updated: 2026-10-07
  ---
  ```

- **Cuerpo**: `# Título`, un párrafo de resumen de 1–3 líneas, secciones `##`, y al final `## Relacionado` con wikilinks.
- **Citas**: cada afirmación de producto cita su fuente entre paréntesis: `(PRD §6.2)`. Las decisiones técnicas enlazan su ADR.
- **Callouts**: `> [!warning] Contradicción`, `> [!question] Pendiente`, `> [!info] Decisión`, `> [!danger] Riesgo aceptado`.
- **Tags** en frontmatter, jerárquicos cuando aporte: `arquitectura/backend`, `producto/chat`.
- Español en la wiki; identificadores de código en inglés según `glosario.md`.

### 5.4 Operaciones

- **Ingesta** (llega una fuente nueva): guárdala en `wiki/raw/` (o confirma su ruta) → crea `fuentes/fuente-<nombre>.md` con resumen, puntos clave y contradicciones → actualiza las páginas de producto/dominio/arquitectura afectadas → `index.md` → `log.md`. Una fuente puede tocar 10–15 páginas; comenta los hallazgos principales con la persona antes de reescribir síntesis grandes.
- **Consulta**: responde desde la wiki con citas a páginas. Si la respuesta es un análisis reutilizable (comparación, decisión razonada, diagnóstico), archívala en `sintesis/` y enlázala.
- **Lint** (a pedido o cuando el log acumule ~10 entradas desde el último): busca contradicciones, afirmaciones obsoletas, páginas huérfanas (sin enlaces entrantes), enlaces rotos, conceptos mencionados sin página, frontmatter incompleto y desfases entre wiki y código. Corrige y registra.
- **Registro**: toda interacción que cambie algo deja entrada en `log.md`.

### 5.5 Formato de `index.md` y `log.md`

- `index.md`: secciones por carpeta; una línea por página: `- [[pagina]] — resumen de una línea`. Se actualiza al crear, renombrar o eliminar páginas.
- `log.md`: **solo se añade al final** (append-only), nunca se reescriben entradas previas. Cada entrada empieza con un encabezado parseable:

  ```markdown
  ## [2026-10-07] feature | Radicación de tickets (backend)
  - Autor: <git config user.name>
  - Cambios: qué se hizo, en una o tres viñetas.
  - Wiki: [[flujo-del-ticket]], [[modelo-de-dominio]]
  - Pendiente: lo que quedó abierto (opcional).
  ```

  Tipos válidos: `setup`, `ingesta`, `consulta`, `feature`, `fix`, `refactor`, `infra`, `decision`, `lint`, `docs`.

### 5.6 Qué actualizar según el cambio

| Cambio | Páginas a revisar |
|---|---|
| Entidad, value object o regla de dominio | `modelo-de-dominio`, `glosario`, página de producto relacionada |
| Caso de uso, puerto o adaptador backend | `backend-hexagonal` (si cambia la estructura) y la página funcional afectada |
| Endpoint REST o evento/método del hub | `tiempo-real-signalr` o la página de API correspondiente (créala si no existe) |
| Login, roles, permisos, multiempresa | `autenticacion-identity`, `roles-y-permisos` |
| Migración o esquema de base de datos | `persistencia-postgresql` |
| Componente, controlador o módulo frontend | `frontend-mvc` (si cambia la estructura) y la página funcional afectada |
| Docker, Compose, puertos, variables | `entorno-docker`, `stack-y-versiones` |
| Decisión técnica o de producto | nuevo ADR en `decisiones/` + `pendientes` si cierra una pregunta |
| Contradicción o duda abierta | `pendientes` |
| Nueva fuente | operación de ingesta (§5.4) |
| Cualquiera de las anteriores | `log.md` (+ `index.md` si hay páginas nuevas) |

### 5.7 Trabajo en equipo sobre la wiki

- `wiki/log.md` usa `merge=union` (`.gitattributes`): las entradas de ramas distintas se conservan ambas al fusionar.
- En conflictos de `index.md` u otras páginas, conserva el contenido de ambas ramas y reconcilia; nunca descartes trabajo ajeno.
- ADR: antes de crear uno, revisa el último número en `main`; si al fusionar hay colisión, renumera el más reciente.

## 6. Reglas de arquitectura (no negociables)

**Backend hexagonal** (detalle: `wiki/arquitectura/backend-hexagonal.md`):
- Dependencias hacia el centro: `Api → Application/Infrastructure`, `Infrastructure → Application → Domain`. `Domain` no depende de nada; `Application` no conoce EF Core, ASP.NET Core ni Npgsql. Lo verifican `backend/tests/DataTicket.ArchitectureTests`.
- Los puertos (interfaces) viven en `Application/Ports/In` y `Application/Ports/Out`; los adaptadores secundarios en `Infrastructure`, los primarios (HTTP, hub SignalR) en `Api`.
- La autorización se aplica en servidor en cada operación (casos de uso), no solo en la UI.

**Frontend MVC** (detalle: `wiki/arquitectura/frontend-mvc.md`; estilo visual: `wiki/arquitectura/sistema-de-diseno.md`):
- `src/modules/<modulo>/{models,controllers,views}` + `src/core` (http, tiempo real) + `src/shared` (UI genérica) + `src/app` (arranque y rutas).
- **Modelos**: tipos, validación, gateways de API/SignalR; sin React. **Controladores**: hooks `useXController` que orquestan modelos y exponen estado + acciones; sin JSX. **Vistas**: componentes puros que reciben props; sin fetch ni SignalR. `oxlint` hace cumplir estas fronteras.
- **Estilo visual** (ADR-0011):
  - Estilo Apple (skill `apple-design`) combinado con la estética de `DataTicket.html` (paleta Data Global, tipografía Archivo, densidad).
  - El prototipo es solo referencia visual: el PRD y las HU de Scrum definen pantallas y comportamiento.
  - Solo tokens de `src/styles/tokens.css`.
  - El orquestador opera y valida el frontend en el navegador.

## 7. Invariantes del PRD que nunca se rompen

1. Un usuario cliente solo ve tickets de su empresa: el solicitante los suyos, el coordinador los de su empresa. Se filtra **en el backend** (PRD §5, §14.1).
2. El cliente nunca ve chat interno, estados técnicos, URL de PR ni nombres de colaboradores internos (PRD §5.6).
3. Solo participantes vigentes leen/escriben el chat; Elizabeth siempre puede. Los grupos de SignalR **no** son seguridad: se valida membresía en cada unión y operación (PRD §7).
4. Cada mensaje se persiste en PostgreSQL **antes** de publicarse por SignalR (PRD §7.2).
5. Un administrador no ve contenido de tickets por ser administrador; al tomar un ticket queda asociado como participante **antes** de cargar el detalle (PRD §5.5).
6. Solo Elizabeth o un administrador emite la respuesta formal y cierra manualmente; no hay cierre automático (PRD §6.5).
7. Adjuntos: PDF, imágenes, XML y Excel, máx. 10 MB, validados en backend (PRD §8).
8. La auditoría es append-only y registra actor, fecha, acción, objeto y valores anterior/nuevo (PRD §12).
9. No hay registro público de clientes; la recuperación de contraseña no revela si el correo existe (PRD §5).

## 8. Colaboración en GitHub

Equipo del repositorio: la persona líder del proyecto, **Laura**, **Juan David** y **Brayan** (ver `wiki/personas/equipo-del-repositorio.md` y `wiki/proceso/flujo-de-trabajo-github.md`).
- Rama por cambio desde `main`: `feature/<area>-<descripcion>`, `fix/...`, `docs/...`, `chore/...`.
- Commits convencionales: `feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`, `perf:`, `ci:`.
- Todo cambio entra por Pull Request con al menos una revisión de otra persona del equipo y la wiki actualizada.
- Un agente **nunca** hace commit, push, merge ni force-push sin que la persona lo pida explícitamente.

## 9. Agentes del proyecto (Claude Code)

| Agente | Úsalo para |
|---|---|
| `orchestrator` | Hilo principal: orienta, planifica, delega, verifica y cierra la wiki de cada interacción |
| `backend-engineer` | Todo lo que ocurre dentro de `backend/` |
| `frontend-engineer` | Todo lo que ocurre dentro de `frontend/` |
| `quality-reviewer` | Revisión de solo lectura tras cambios de código: arquitectura, invariantes del PRD, seguridad, pruebas |
| `wiki-keeper` | Ingestas de fuentes, lint de la wiki y actualizaciones que toquen muchas páginas |

Los especialistas no editan `wiki/index.md` ni `wiki/log.md`: devuelven **Notas para la wiki** y el orquestador (o `wiki-keeper`) las integra. Así se evitan ediciones concurrentes sobre los mismos archivos.

## 10. Comandos frecuentes

```bash
docker compose up --build --watch                 # stack completo con hot reload
docker compose ps                                 # estado de servicios
docker compose logs -f backend                    # logs de un servicio
docker compose down                               # detener (conserva datos); -v borra volúmenes
cd backend && dotnet build                        # compilar backend (desde backend/: ahí está global.json)
cd backend && dotnet test                         # pruebas backend (Microsoft.Testing.Platform)
docker build --target test ./backend              # pruebas backend sin SDK local
npm --prefix frontend run lint                    # oxlint (incluye fronteras MVC)
npm --prefix frontend test                        # vitest
npm --prefix frontend run build                   # typecheck + build
```

**Docker lo opera el agente desde Claude Code**: el stack de Compose ya está levantado y los comandos (`docker compose …`, `curl`, pruebas) se ejecutan desde la sesión, no desde Docker Desktop. Para probar algo que el stack no contiene, crea un contenedor desechable (Testcontainers o `docker run --rm --name dt-<algo>-check …`) y elimínalo al terminar. Nunca borres volúmenes ni cargues datos de prueba en el servicio `db` de Compose fuera de las migraciones y del sembrador de Development. Los datos semilla de referencia están en `db.sql` §13.

URLs locales: frontend `http://localhost:5173`, API `http://localhost:8080` (OpenAPI en `/openapi/v1.json`), salud `/api/health`, Mailpit `http://localhost:8025`, Azurite Blob `http://localhost:10000`.

## 11. Lo que un agente nunca hace

- Editar `PRD.md`, `DataTicket.html` o archivos de `wiki/raw/` sin petición explícita.
- Escribir secretos reales en el repositorio (`.env` no se versiona; `appsettings.Development.json` solo lleva valores locales de ejemplo).
- Desactivar pruebas de arquitectura o reglas de lint para "hacer pasar" un cambio.
- Reescribir o borrar entradas previas de `wiki/log.md`.
- Afirmar resultados de comandos que no ejecutó.
