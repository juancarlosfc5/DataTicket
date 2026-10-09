---
title: "HU-041 — Panel del equipo interno"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/dashboard, producto/metricas, producto/seguridad, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §4", "PRD.md §5", "PRD.md §6.2", "PRD.md §10", "PRD.md §14"]
aliases: ["HU-041", "Panel del equipo interno", "Dashboard del equipo"]
epica: "[[ep-012-dashboard-y-metricas]]"
criterios_prd: [CA-13]
componentes: [Backend Application, Backend Infrastructure, Backend Api, Frontend models, Frontend controllers, Frontend views]
dificultad: "Medio"
sprint_sugerido: "Sprint 8"
dependencias: ["[[hu-039-calculo-de-tiempos-habiles]]", "[[hu-040-panel-global-de-la-pm]]", "[[hu-023-bandeja-de-mis-tickets-y-equipo]]", "[[hu-009-administrar-usuarios-internos-y-equipos]]"]
relacionadas: ["[[hu-020-detalle-interno-del-ticket]]", "[[hu-042-resumen-de-estados-en-portal]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-041 — Panel del equipo interno

Panel para Desarrollo y Producción con métricas limitadas a los tickets donde participa el usuario o su equipo, claramente diferenciado de los datos globales de la PM y sin revelar contenido de tickets no asociados (PRD §10).

## Historia de usuario

**COMO** integrante de Desarrollo o Producción  
**QUIERO** ver la carga, la antigüedad y los tiempos de mis tickets y los de mi equipo  
**PARA** priorizar mi trabajo y equilibrar la carga dentro del equipo sin acceder a datos globales

## Contexto

"El panel debe diferenciar datos globales de PM, datos del equipo interno y tickets visibles a una empresa cliente" y "el detalle/chat requiere asociación al ticket, además de la pertenencia al equipo" (PRD §10). La definición de "tickets del equipo" reutiliza la propuesta de [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]] (tickets con al menos un participante vigente del equipo). Los tiempos usan [[hu-039-calculo-de-tiempos-habiles|HU-039]] y las métricas reutilizan las consultas de [[hu-040-panel-global-de-la-pm|HU-040]] con otro alcance.

## Alcance

- Ámbito personal (`me`): métricas de los tickets donde el usuario es participante vigente, con lista de esos tickets.
- Ámbito de equipo (`team`): solo agregados (conteos por estado, antigüedad, carga por integrante del equipo, tiempos) de los tickets del equipo; sin títulos ni números de tickets no asociados.
- Vista del panel con selector de ámbito.

## Fuera de alcance

- Datos globales (otras empresas sin relación con el equipo, cola de triage completa): [[hu-040-panel-global-de-la-pm|HU-040]].
- Detalle o contenido de tickets no asociados.
- SLA, metas y semáforos (PRD §4, §10).
- "Área" y "primera respuesta" (hallazgo 3; [[pendientes]] §1).

## Requisitos y reglas de negocio

- Métricas de antigüedad, carga por responsable y tiempos hasta respuesta formal y cierre (PRD §4, §10).
- Datos del equipo diferenciados de los globales de PM (PRD §10).
- La pertenencia al equipo no da acceso al detalle (PRD §6.2 punto 5, §10).
- Sin semáforos SLA ni metas (PRD §10, §14.13).
- (propuesta) Ámbito `team` solo para equipos a los que pertenece el usuario; el ámbito `team` no lista tickets, solo agregados.
- (propuesta) Mismas definiciones de "activo" y "sin asignar" que HU-040.

## Invariantes en juego

- AGENTS §7.3 — la pertenencia al equipo no sustituye la participación: el panel no revela contenido de tickets no asociados.
- AGENTS §7.5 — un administrador sin equipo no obtiene el ámbito `team`.

## Criterios del PRD cubiertos

- PRD CA-13 (parcial: vista de equipo del dashboard interno) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-012-dashboard-y-metricas]]
- Dependencias: [[hu-039-calculo-de-tiempos-habiles|HU-039]], [[hu-040-panel-global-de-la-pm|HU-040]] (consultas agregadas reutilizables), [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]] (definición de "tickets del equipo"), [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]] (pertenencia a equipos).
- Relacionadas: [[hu-020-detalle-interno-del-ticket|HU-020]] (enlace al detalle de tickets propios), [[hu-042-resumen-de-estados-en-portal|HU-042]].
- Decisiones: hallazgo 1 (roles frente a equipos), V-10, hallazgo 3.

## Componentes afectados

- Backend Application: consulta del panel de equipo (nombre propuesto `GetTeamDashboard` sobre `IDashboardQueries`, ratificar en T-01), `ICurrentUser`, `IClock`.
- Backend Infrastructure: predicados de alcance `me` y `team` en las agregaciones.
- Backend Api: `GET /api/dashboard/team`.
- Frontend models/controllers/views: módulo `dashboard`.

## Dificultad

**Nivel:** Medio

**Justificación:** reutiliza cálculos y consultas de HU-039/HU-040; lo nuevo es el alcance por participación y equipo y las pruebas que demuestran que no se filtran datos globales ni contenido de tickets ajenos.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01.

**`GET /api/dashboard/team?scope=me`** y **`GET /api/dashboard/team?scope=team&team=Development`**

Respuesta `200 OK` (`scope=me`):

```json
{
  "scope": "me",
  "byStatus": [ { "status": "InDevelopment", "count": 2 }, { "status": "PullRequestReview", "count": 1 } ],
  "age": { "openTickets": 3, "averageBusinessMinutes": 2400, "maxBusinessMinutes": 4320 },
  "timeToFormalResponse": { "count": 4, "averageBusinessMinutes": 5040, "medianBusinessMinutes": 4320 },
  "timeToClose": { "count": 2, "averageBusinessMinutes": 7200, "medianBusinessMinutes": 7200 },
  "tickets": [
    { "id": "6f1c...", "number": "DT-000123", "title": "No genera la factura electrónica", "status": "InDevelopment", "ageBusinessMinutes": 960 }
  ]
}
```

Respuesta `200 OK` (`scope=team`): mismas secciones salvo `tickets`, más `"team": "Development"` y `"loadByMember": [{ "userId": "4c2d...", "displayName": "Laura", "activeTickets": 3 }]` (solo integrantes del equipo).

Errores (ProblemDetails):

| Código | Cuándo |
|---|---|
| 400 | `scope` distinto de `me`/`team`; `scope=team` sin `team`; `team` desconocido |
| 401 | Sin sesión |
| 403 | Usuario cliente; `team` al que el usuario no pertenece |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar ámbitos, ausencia de lista de tickets en `team`, definición de "tickets del equipo" y nombres; registrar en el [[glosario]].
- [ ] **T-02 — Caso de uso** · Capa: Backend Application · Dificultad: Medio  
  Descripción: primero pruebas unitarias: usuario cliente → prohibido; equipo ajeno → prohibido; admin sin equipo con `scope=team` → prohibido; `scope=me` del admin solo con tickets que tomó; luego `GetTeamDashboard`.
- [ ] **T-03 — Alcance en las agregaciones** · Capa: Backend Infrastructure · Dificultad: Medio  
  Descripción: predicados `me` (participante vigente) y `team` (algún participante vigente del equipo) sobre las consultas de HU-040; pruebas de integración con PostgreSQL real que siembran tickets ajenos y verifican que no se cuentan.
- [ ] **T-04 — Endpoint** · Capa: Backend Api · Dificultad: Bajo  
  Descripción: `GET /api/dashboard/team`; pruebas de integración de 200/400/401/403 e inspección de claves (sin `received.byCompany`, sin `tickets` en `team`).
- [ ] **T-05 — Modelos y gateway** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: tipos `TeamDashboard` con unión discriminada por `scope`; pruebas Vitest.
- [ ] **T-06 — Controlador** · Capa: Frontend controllers · Dificultad: Bajo  
  Descripción: `useTeamDashboardController` con selector de ámbito limitado a los equipos de la sesión; pruebas Vitest.
- [ ] **T-07 — Vista** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `TeamDashboardView` con el título "Mi trabajo" o "Equipo Desarrollo/Producción", sin colores de cumplimiento.
- [ ] **T-08 — Migración** · Capa: Persistencia · Dificultad: Bajo  
  Descripción: no aplica (reutiliza índices de HU-023 y HU-040).

## Criterios de aceptación

### CHU-01 — Ámbito personal

**Dado** Laura, participante vigente de T1 (`InDevelopment`), T2 (`InDevelopment`) y T3 (`PullRequestReview`), y retirada de T4  
**Cuando** pide `?scope=me`  
**Entonces** `byStatus` cuenta InDevelopment = 2 y PullRequestReview = 1, y `tickets` contiene T1, T2 y T3 pero no T4.

### CHU-02 — Ámbito de equipo solo con agregados

**Dado** T5, con Brayan (Desarrollo) como participante y sin Laura  
**Cuando** Laura pide `?scope=team&team=Development`  
**Entonces** T5 se cuenta en `byStatus` y en la carga de Brayan, la respuesta no tiene la clave `tickets` y el cuerpo crudo no contiene el título ni el número de T5.

### CHU-03 — No incluye datos globales

**Dado** T6, de la empresa B, sin ningún participante de Desarrollo  
**Cuando** Laura pide `?scope=team&team=Development`  
**Entonces** T6 no se cuenta en ninguna sección y la respuesta no tiene `received.byCompany` ni `unassigned`.

### CHU-04 — Autorización por equipo y rol

**Dado** Julián (solo `Team.Production`), un administrador sin equipo, un solicitante y un coordinador  
**Cuando** Julián pide `team=Development`, el administrador pide `scope=team&team=Production` y los usuarios cliente llaman a cualquier ámbito  
**Entonces** todos reciben 403.

### CHU-05 — Validación de parámetros

**Dado** un integrante de Desarrollo  
**Cuando** pide `?scope=all`, `?scope=team` sin `team` o `?scope=team&team=Sales`  
**Entonces** recibe 400 ProblemDetails.

### CHU-06 — Sin SLA ni metas

**Dado** cualquier respuesta 200 del panel  
**Cuando** se inspeccionan recursivamente sus claves  
**Entonces** ninguna contiene `sla`, `target`, `goal`, `threshold`, `breached` ni `color`.

### CHU-07 — UI diferenciada, vacía y con error

**Dado** un integrante sin tickets asociados  
**Cuando** abre "Mi trabajo"  
**Entonces** ve "No tienes tickets asociados" y tiempos `—`; **el** título identifica el ámbito ("Mi trabajo" o "Equipo Desarrollo") y nunca muestra "Global"; **y** ante un 500 ve un error con "Reintentar".

## Definition of Done

- [ ] CHU-01 a CHU-07 validados con evidencia.
- [ ] Pruebas en verde (`cd backend && dotnet test`): `TeamDashboard_Me_ExcluyeParticipacionRetirada`, `TeamDashboard_Team_NoIncluyeTitulosNiNumeros`, `TeamDashboard_Team_NoCuentaTicketsAjenosAlEquipo`, `TeamDashboard_EquipoAjeno_Prohibido`, `TeamDashboard_SinClavesDeSla`.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Vitest, `npm --prefix frontend run lint` y `npm --prefix frontend run build` en verde.
- [ ] El stack levanta con `docker compose up --build` y el panel muestra datos sintéticos de ambos ámbitos.
- [ ] Sin migración (justificado en T-08).
- [ ] Wiki actualizada: [[dashboard-y-metricas]] (panel del equipo: ámbitos y campos), [[roles-y-permisos]] (acceso por equipo), [[glosario]].
- [ ] `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos.
- [ ] PR revisado y aprobado por otra persona del equipo.

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
| DoD-01 Pruebas backend nombradas | Pendiente | — | — |
| DoD-02 Pruebas de arquitectura | Pendiente | — | — |
| DoD-03 Vitest, lint y build | Pendiente | — | — |
| DoD-04 `docker compose up --build` | Pendiente | — | — |
| DoD-05 Migración (no aplica) | Pendiente | — | — |
| DoD-06 Wiki actualizada | Pendiente | — | — |
| DoD-07 `quality-reviewer` | Pendiente | — | — |
| DoD-08 PR revisado | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- **Hallazgo 1:** Elizabeth es PM y pertenece a Producción; en el ámbito `team=Production` aparecerá como integrante, pero su vista principal es la global (HU-040).
- (propuesta) El ámbito `team` sin lista de tickets evita que la pertenencia al equipo revele títulos de tickets no asociados; si el equipo pide ver la lista, la bandeja de equipo de HU-023 ya ofrece campos limitados.

## Relacionado

- [[ep-012-dashboard-y-metricas]] · [[tablero-scrum]] · [[dashboard-y-metricas]] · [[roles-y-permisos]] · [[equipo-data-global]]
- [[persistencia-postgresql]] · [[frontend-mvc]] · [[criterios-de-aceptacion]] · [[pendientes]]
