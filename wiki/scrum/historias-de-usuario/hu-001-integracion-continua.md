---
title: "HU-001 — Integración continua en cada PR"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, proceso, proceso/ci, arquitectura/infra]
sources: ["PRD.md §12", "PRD.md §13", "PRD.md §14", "AGENTS.md §6", "AGENTS.md §8", "AGENTS.md §10"]
aliases: ["HU-001", "Integración continua en cada PR"]
epica: "[[ep-001-fundaciones-tecnicas]]"
criterios_prd: []
componentes: ["CI (GitHub Actions, propuesta)", "Backend (pruebas y ArchitectureTests)", "Frontend (lint, pruebas, build)", "Docker/Compose"]
dificultad: "Medio"
sprint_sugerido: "Sprint 0"
dependencias: []
relacionadas: ["[[hu-002-shell-y-navegacion-por-rol]]", "[[hu-003-iniciar-y-cerrar-sesion]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-001 — Integración continua en cada PR

Un flujo de integración continua ejecuta en cada Pull Request las mismas verificaciones que `AGENTS.md` §10 pide en local (pruebas de backend con reglas de arquitectura, lint con fronteras MVC, pruebas y build del frontend, validez del Compose) y bloquea el PR si alguna falla.

## Historia de usuario

**COMO** integrante del equipo de desarrollo del repositorio  
**QUIERO** que cada Pull Request ejecute automáticamente las pruebas de backend (incluidas las de arquitectura), el lint con fronteras MVC, las pruebas y el build del frontend y la validación del entorno Docker  
**PARA** detectar antes de la revisión humana cualquier cambio que rompa el comportamiento, la arquitectura hexagonal/MVC o el arranque del stack

## Contexto

Hoy las verificaciones se ejecutan a mano (plantilla `.github/pull_request_template.md`). El repositorio no tiene `.github/workflows/`. La wiki registra como decisión abierta «CI en GitHub Actions (pruebas backend, lint/pruebas/build frontend)» ([[pendientes]] §4, [[flujo-de-trabajo-github]]). Esta HU especifica qué debe verificar el CI y cómo se comprueba; la plataforma concreta queda sujeta a esa decisión.

Detalles del repositorio que condicionan el CI:

- `dotnet` debe ejecutarse **dentro de `backend/`**: allí está `global.json` con `test.runner = Microsoft.Testing.Platform`; desde la raíz el SDK cae en modo VSTest y falla ([[estrategia-de-pruebas]]).
- `Directory.Build.props` activa `TreatWarningsAsErrors`.
- `backend/Dockerfile` tiene una etapa `test` (`docker build --target test ./backend`).
- Node 24 (`frontend/package.json` → `engines.node >= 24`) y `npm ci` con `package-lock.json` versionado.

## Alcance

- Un flujo que se dispara en `pull_request` hacia `main` y en `push` a `main`.
- Trabajo de backend: restaurar, compilar y ejecutar `dotnet test` desde `backend/` (incluye `DataTicket.ArchitectureTests`).
- Trabajo de frontend: `npm ci`, `npm --prefix frontend run lint`, `npm --prefix frontend test`, `npm --prefix frontend run build`.
- Trabajo de entorno: `docker compose config --quiet` y construcción de imágenes (`docker build --target test ./backend` y `docker compose build`).
- Servicio PostgreSQL 18.6 disponible para las pruebas de integración que llegarán con [[hu-003-iniciar-y-cerrar-sesion|HU-003]] (propuesta: servicio del job con credenciales de desarrollo de `.env.example`, o Testcontainers; ratificar en T-01).
- Permisos mínimos del token del flujo y cancelación de ejecuciones obsoletas de la misma rama.
- Documentar el CI en la wiki y en la plantilla del PR.

## Fuera de alcance

- Despliegue continuo (CD) o publicación de imágenes en un registro: la infraestructura productiva está pendiente (PRD §16.1).
- Configurar la protección de `main` y `CODEOWNERS` en GitHub: depende de los usuarios del equipo ([[pendientes]] §5); la HU solo deja los checks listos para exigirse.
- Pruebas E2E con Playwright y análisis de seguridad de dependencias (propuestas futuras).
- Elegir la plataforma de CI: decisión abierta, no se toma aquí.

## Requisitos y reglas de negocio

- Las reglas de dependencias hexagonales se verifican en cada `dotnet test` con `DataTicket.ArchitectureTests` y nunca se desactivan (`AGENTS.md` §6, §11).
- `oxlint` hace cumplir las fronteras MVC del frontend (`AGENTS.md` §6, [[frontend-mvc]]).
- Todo cambio entra por PR revisado con la wiki actualizada (`AGENTS.md` §8).
- Nunca se escriben secretos reales en el repositorio (`AGENTS.md` §11); el CI usa solo valores de desarrollo.
- Cada criterio de aceptación del PRD debe acabar respaldado por una prueba automática ([[estrategia-de-pruebas]]); el CI es el que la ejecuta en cada cambio (PRD §14).

## Invariantes en juego

- Ninguno de producto directamente. Protege indirectamente todos los invariantes de `AGENTS.md` §7, porque ejecuta en cada PR las pruebas que los verifican.

## Criterios del PRD cubiertos

- Ninguno de forma directa. Habilita la verificación continua de PRD CA-01…CA-15 → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-001-fundaciones-tecnicas]]
- Dependencias: decisión abierta «CI en GitHub Actions» ([[pendientes]] §4). La HU no debe pasar a `Aprobada` hasta que esa decisión se registre en un ADR.
- Relacionadas: [[hu-002-shell-y-navegacion-por-rol|HU-002]] (primeras pruebas Vitest de vistas), [[hu-003-iniciar-y-cerrar-sesion|HU-003]] (primer proyecto de pruebas de integración con PostgreSQL).
- Decisiones: [[adr-0001-monorepo-contenedorizado]], [[adr-0002-backend-hexagonal]], [[adr-0003-frontend-mvc]], [[adr-0007-entorno-local-compose-watch]]; ADR de CI pendiente.

## Componentes afectados

- CI: nuevo archivo de flujo (propuesta: `.github/workflows/ci.yml`).
- Backend: solo se ejecuta (`backend/`), sin cambios de código.
- Frontend: solo se ejecuta (`frontend/`), sin cambios de código.
- Docker/Compose: se valida `docker-compose.yml` y se construyen `backend/Dockerfile` y `frontend/Dockerfile`.
- Documentación: `.github/pull_request_template.md` (referencia al CI).

## Dificultad

**Nivel:** Medio

**Justificación:** No hay lógica de negocio, pero coordina tres cadenas de herramientas (.NET 10 con Microsoft.Testing.Platform, Node 24, Docker) con particularidades conocidas (`global.json` dentro de `backend/`, `TreatWarningsAsErrors`) y debe prever PostgreSQL para pruebas de integración. Depende de una decisión abierta.

## Contrato backend ↔ frontend

No aplica: la HU no expone rutas REST ni métodos del hub. Su «contrato» es la lista de comandos que el CI ejecuta, que debe coincidir exactamente con `AGENTS.md` §10:

| Trabajo (propuesta) | Directorio | Comandos | Falla si |
|---|---|---|---|
| `backend` | `backend/` | `dotnet restore` · `dotnet build --no-restore` · `dotnet test --no-build` | Error de compilación, aviso tratado como error, prueba en rojo (incluidas `LayerDependencyTests`) |
| `frontend` | raíz | `npm ci --prefix frontend` · `npm --prefix frontend run lint` · `npm --prefix frontend test` · `npm --prefix frontend run build` | Error de `oxlint` (incluye `no-restricted-imports` MVC), prueba Vitest en rojo, error de `tsc -b` o de Vite |
| `docker` | raíz | `docker compose config --quiet` · `docker build --target test ./backend` · `docker compose build` | Compose inválido, etapa `test` en rojo, imagen que no construye |

## Tareas de desarrollo

- [ ] **T-01 — Contrato del CI** · Capa: Transversal · Dificultad: Bajo  
  Descripción: con la decisión de plataforma registrada en un ADR, fijar trabajos, disparadores (`pull_request` a `main`, `push` a `main`), versiones (SDK según `backend/global.json`, Node 24), nombres de los checks que se exigirán en `main` y cómo se provee PostgreSQL a las pruebas de integración. Actualizar la tabla de contrato de esta HU si cambia.
- [ ] **T-02 — Trabajo de backend** · Capa: Backend (CI) · Dificultad: Bajo  
  Descripción: restaurar, compilar y probar con `working-directory: backend`. Antes de darlo por bueno, abrir un PR de prueba con un cambio que viole `LayerDependencyTests` (p. ej. `Domain` referenciando `Npgsql`) y comprobar que el check queda en rojo; revertirlo.
- [ ] **T-03 — Trabajo de frontend** · Capa: Frontend (CI) · Dificultad: Bajo  
  Descripción: `npm ci` con caché por `frontend/package-lock.json`, lint, pruebas y build. Comprobar con un PR de prueba que una vista que importa `core/http` hace fallar `oxlint`; revertirlo.
- [ ] **T-04 — Trabajo de entorno Docker** · Capa: Docker/Compose · Dificultad: Medio  
  Descripción: `docker compose config --quiet`, `docker build --target test ./backend` y `docker compose build` sin `.env` (debe funcionar con los valores por defecto de `docker-compose.yml`/`.env.example`).
- [ ] **T-05 — PostgreSQL para integración** · Capa: Transversal · Dificultad: Medio  
  Descripción: dejar disponible PostgreSQL 18.6 para el trabajo de backend (servicio del job o Testcontainers, según T-01) con credenciales de desarrollo, sin secretos. Se valida cuando exista el primer proyecto de integración ([[hu-003-iniciar-y-cerrar-sesion|HU-003]]).
- [ ] **T-06 — Seguridad y eficiencia del flujo** · Capa: CI · Dificultad: Bajo  
  Descripción: `permissions: contents: read` (mínimo privilegio), `concurrency` que cancele ejecuciones obsoletas de la misma rama, acciones de terceros fijadas a versión (propuesta: fijar por SHA), sin `secrets` en el flujo.
- [ ] **T-07 — Documentación** · Capa: Documentación · Dificultad: Bajo  
  Descripción: actualizar `.github/pull_request_template.md` para indicar que el CI ejecuta las verificaciones; entregar **Notas para la wiki** ([[flujo-de-trabajo-github]], [[estrategia-de-pruebas]], [[pendientes]]).

## Criterios de aceptación

### CHU-01 — El CI se ejecuta en cada PR hacia `main`

**Dado** un PR abierto contra `main` con un cambio que no rompe nada  
**Cuando** se publica o actualiza el PR  
**Entonces** se ejecutan los trabajos `backend`, `frontend` y `docker`, los tres terminan en verde y el PR muestra los tres checks con esos nombres.

### CHU-02 — Una prueba de backend en rojo bloquea el PR

**Dado** un PR que introduce una prueba xUnit que falla, o un aviso de compilación (que `TreatWarningsAsErrors` convierte en error)  
**Cuando** se ejecuta el CI  
**Entonces** el trabajo `backend` termina en rojo y su registro muestra el nombre de la prueba fallida o el código del aviso.

### CHU-03 — Una violación de la arquitectura hexagonal bloquea el PR

**Dado** un PR en el que `DataTicket.Domain` o `DataTicket.Application` referencia `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore` o `Npgsql`  
**Cuando** se ejecuta el CI  
**Entonces** el trabajo `backend` falla en `LayerDependencyTests` y ninguna configuración del flujo omite o filtra `DataTicket.ArchitectureTests`.

### CHU-04 — Una violación de las fronteras MVC bloquea el PR

**Dado** un PR en el que un archivo de `src/modules/*/views/` importa `core/http/httpClient` o `@microsoft/signalr`  
**Cuando** se ejecuta el CI  
**Entonces** el trabajo `frontend` falla en `npm --prefix frontend run lint` con el mensaje «MVC: las vistas reciben datos del controlador por props; no acceden a APIs.» (o el correspondiente de `.oxlintrc.json`).

### CHU-05 — Pruebas y build del frontend se verifican

**Dado** un PR con una prueba Vitest en rojo o con un error de tipos detectado por `tsc -b`  
**Cuando** se ejecuta el CI  
**Entonces** el trabajo `frontend` falla en `npm --prefix frontend test` o en `npm --prefix frontend run build`, respectivamente.

### CHU-06 — El entorno Docker sigue siendo válido

**Dado** un PR que deja `docker-compose.yml` inválido o rompe la construcción de `backend/Dockerfile` (p. ej. un proyecto .NET nuevo no añadido a la etapa `restore`)  
**Cuando** se ejecuta el CI  
**Entonces** el trabajo `docker` falla en `docker compose config --quiet`, en `docker build --target test ./backend` o en `docker compose build`, sin necesitar un archivo `.env`.

### CHU-07 — `dotnet` se ejecuta desde `backend/`

**Dado** el flujo de CI  
**Cuando** se inspecciona el paso de pruebas de backend  
**Entonces** usa `backend/` como directorio de trabajo y su registro muestra la ejecución con Microsoft.Testing.Platform (no VSTest).

### CHU-08 — Mínimo privilegio y sin secretos

**Dado** el archivo del flujo  
**Cuando** se revisa  
**Entonces** declara `permissions: contents: read` (o más restrictivo), no referencia `secrets.*` y las únicas credenciales que usa son los valores de desarrollo publicados en `.env.example` (`dataticket_local_only`).

### CHU-09 — Las ejecuciones obsoletas se cancelan

**Dado** dos pushes consecutivos a la misma rama de un PR  
**Cuando** el segundo dispara el CI mientras el primero sigue en curso  
**Entonces** la ejecución del primero se cancela y solo la del último commit determina el estado del PR.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-09 validados con evidencia: enlaces a ejecuciones del CI (una en verde y una en rojo por cada CHU-02 a CHU-06, obtenidas con PR de prueba que luego se descartan).
- [ ] **DoD-02** — Decisión de plataforma de CI registrada como ADR `aceptada` y tachada en [[pendientes]] §4.
- [ ] **DoD-03** — Ejecutadas en local y en verde, con salida adjunta al PR: `cd backend && dotnet test`, `npm --prefix frontend run lint`, `npm --prefix frontend test`, `npm --prefix frontend run build`, `docker compose config --quiet`, `docker build --target test ./backend`.
- [ ] **DoD-04** — `docker compose up --build` sigue levantando los cinco servicios (el PR no cambia Compose de forma incompatible).
- [ ] **DoD-05** — `DataTicket.ArchitectureTests` aparece ejecutado en el registro del CI (5 pruebas de `LayerDependencyTests` como mínimo).
- [ ] **DoD-06** — Wiki actualizada vía Notas para la wiki: [[flujo-de-trabajo-github]] (sección CI, checks exigibles en `main`), [[estrategia-de-pruebas]] (dónde corre cada nivel), [[pendientes]] (decisión cerrada) y `log.md`.
- [ ] **DoD-07** — `.github/pull_request_template.md` actualizado para referenciar el CI.
- [ ] **DoD-08** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (permisos del token, secretos, acciones fijadas).
- [ ] **DoD-09** — PR revisado y aprobado por otra persona del equipo.
- [ ] **DoD-10** — Trazabilidad de la HU y de [[ep-001-fundaciones-tecnicas]] actualizada.

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

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- La plataforma (GitHub Actions u otra) no se decide en esta HU; los nombres `ci.yml`, `permissions` y `concurrency` asumen GitHub Actions solo como referencia por ser la opción registrada en [[pendientes]].
- Si el equipo exige los checks en la protección de `main`, los nombres de trabajo fijados en T-01 no deben cambiarse después sin actualizar esa protección.
- Riesgo: con Compose Watch y `dotnet watch` la etapa `dev` no se ejecuta en el CI; se construyen `test` y las imágenes por defecto.

## Relacionado

- [[ep-001-fundaciones-tecnicas]] · [[tablero-scrum]] · [[flujo-de-trabajo-github]] · [[estrategia-de-pruebas]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[entorno-docker]] · [[pendientes]]
