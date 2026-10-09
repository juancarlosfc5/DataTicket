---
title: "HU-040 — Panel global de la PM"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/dashboard, producto/metricas, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §4", "PRD.md §5", "PRD.md §10", "PRD.md §13", "PRD.md §14", "PRD.md §16"]
aliases: ["HU-040", "Panel global de la PM", "Dashboard global"]
epica: "[[ep-012-dashboard-y-metricas]]"
criterios_prd: [CA-13]
componentes: [Backend Application, Backend Infrastructure, Backend Api, Frontend models, Frontend controllers, Frontend views, Persistencia PostgreSQL]
dificultad: "Alto"
sprint_sugerido: "Sprint 8"
dependencias: ["[[hu-039-calculo-de-tiempos-habiles]]", "[[hu-016-cola-global-de-triage]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-021-cambiar-estado-interno]]", "[[hu-033-emitir-respuesta-formal]]", "[[hu-036-cerrar-ticket-manualmente]]", "[[hu-010-datos-sinteticos-de-desarrollo]]"]
relacionadas: ["[[hu-041-panel-del-equipo-interno]]", "[[hu-042-resumen-de-estados-en-portal]]", "[[hu-023-bandeja-de-mis-tickets-y-equipo]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-040 — Panel global de la PM

Panel con los datos globales de la PM: tickets recibidos por periodo con filtros por estado y empresa, tickets sin asignar, antigüedad, carga por responsable y tiempos hasta respuesta formal y cierre, en días hábiles y sin semáforos SLA ni metas (PRD §4, §10, §14.13).

## Historia de usuario

**COMO** Elizabeth (PM, rol `ProductManager`)  
**QUIERO** un panel con la carga, la antigüedad, el estado y los tiempos de todos los tickets  
**PARA** detectar tickets sin asignar y cargas desbalanceadas y obtener una línea base operativa

## Contexto

Las métricas del MVP están definidas en (PRD §4); el panel debe diferenciar los datos globales de PM de los del equipo y de los visibles a una empresa cliente (PRD §10). Dos huecos del PRD afectan esta HU: "área" aparece como filtro y dimensión, pero no existe en el modelo de dominio (hallazgo 3), y la Fase 3 habla de "primera respuesta", que §4 no define ([[pendientes]] §1). Además, CA-13 está entre los criterios del MVP mientras el dashboard está en la Fase 3 (hallazgo 2). Los tiempos usan la regla de [[hu-039-calculo-de-tiempos-habiles|HU-039]].

## Alcance

- Endpoint de panel global con periodo y filtros por estado interno y empresa.
- Métricas: recibidos (total, por estado y por empresa), sin asignar, antigüedad de tickets abiertos, carga por responsable, tiempo hasta respuesta formal y tiempo hasta cierre (promedio y mediana, propuesta).
- Vista del panel con tablas (sin nuevas librerías de gráficos).

## Fuera de alcance

- Filtro y desglose por "área" hasta resolver el hallazgo 3.
- Métrica de "primera respuesta" hasta que se defina.
- SLA, semáforos, metas, alertas o recordatorios (PRD §4, §10, §13).
- Exportación y vistas guardadas (PRD §10, §16.4).
- Acceso del administrador (propuesta: no, ver Notas).
- Panel del equipo ([[hu-041-panel-del-equipo-interno|HU-041]]) y resumen del portal ([[hu-042-resumen-de-estados-en-portal|HU-042]]).

## Requisitos y reglas de negocio

- Tickets recibidos: creados en un periodo, con filtros por estado, empresa y área (PRD §4).
- Tickets sin asignar: en cola de triage sin responsable operativo (PRD §4).
- Antigüedad: desde la creación hasta la consulta o el cierre (PRD §4).
- Carga por responsable: tickets activos asociados a cada persona (PRD §4).
- Tiempos hasta respuesta formal y hasta cierre (PRD §4, §10).
- Sin semáforos SLA ni metas numéricas (PRD §4, §10, §14.13).
- Datos globales de PM diferenciados de los del equipo y del cliente (PRD §10).
- Elizabeth puede consultar todos los tickets (PRD §5).
- (propuesta, V-10) "Sin asignar" = tickets en `New` sin ninguna `Assignment` vigente.
- (propuesta) "Activo" = estado `New`, `InDevelopment`, `PullRequestReview` o `InProduction`; "carga por responsable" cuenta tickets activos con `Assignment` vigente de esa persona.
- (propuesta) El periodo se expresa en fechas locales de Bogotá (`from`, `to` inclusive), con un máximo de 366 días.

## Invariantes en juego

- AGENTS §7.5 — el administrador no obtiene datos por ser administrador (de ahí la propuesta de excluirlo).
- AGENTS §7.2 — los datos del panel nunca se sirven a usuarios cliente.

## Criterios del PRD cubiertos

- PRD CA-13 (total salvo la dimensión "área", bloqueada por el hallazgo 3) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-012-dashboard-y-metricas]]
- Dependencias: [[hu-039-calculo-de-tiempos-habiles|HU-039]] (tiempos), [[hu-016-cola-global-de-triage|HU-016]] (definición de cola), [[hu-018-asignar-y-agregar-participantes|HU-018]] (`Assignment`), [[hu-021-cambiar-estado-interno|HU-021]] (estados), [[hu-033-emitir-respuesta-formal|HU-033]] y [[hu-036-cerrar-ticket-manualmente|HU-036]] (fechas de respuesta y cierre), [[hu-010-datos-sinteticos-de-desarrollo|HU-010]] (~50 empresas).
- Relacionadas: [[hu-041-panel-del-equipo-interno|HU-041]], [[hu-042-resumen-de-estados-en-portal|HU-042]], [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]].
- Decisiones: hallazgos 2 y 3, "primera respuesta" ([[pendientes]] §1), V-09, V-10.

## Componentes afectados

- Backend Application: consulta del panel (nombre propuesto `GetGlobalDashboard` con puerto `IDashboardQueries`, ratificar en T-01 y registrar en el glosario), `ICurrentUser`, `IClock`.
- Backend Infrastructure: agregaciones en SQL vía EF Core.
- Backend Api: `GET /api/dashboard/global`.
- Frontend models/controllers/views: módulo `dashboard` (nombre propuesto).
- Persistencia PostgreSQL: índices para `Ticket(CreatedAt)` y `Assignment` vigente (propuesta).

## Dificultad

**Nivel:** Alto

**Justificación:** varias métricas agregadas con definiciones parcialmente abiertas (sin asignar, activo, área, primera respuesta), cálculo de tiempo hábil sobre conjuntos grandes y una vista nueva; además depende de HU de tres épicas anteriores.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01.

**`GET /api/dashboard/global?from=2026-10-01&to=2026-10-31&status=New&status=InDevelopment&companyId=b3e1...`**

Respuesta `200 OK`:

```json
{
  "scope": "global",
  "period": { "from": "2026-10-01", "to": "2026-10-31", "timeZone": "America/Bogota" },
  "received": {
    "total": 42,
    "byStatus": [
      { "status": "New", "count": 5 },
      { "status": "InDevelopment", "count": 12 },
      { "status": "PullRequestReview", "count": 3 },
      { "status": "InProduction", "count": 4 },
      { "status": "SolutionDelivered", "count": 8 },
      { "status": "Closed", "count": 10 }
    ],
    "byCompany": [ { "companyId": "b3e1...", "companyName": "Empresa Sintética A", "count": 7 } ]
  },
  "unassigned": { "count": 3 },
  "age": { "openTickets": 24, "averageBusinessMinutes": 3120, "maxBusinessMinutes": 14400 },
  "loadByAssignee": [ { "userId": "4c2d...", "displayName": "Laura", "activeTickets": 4 } ],
  "timeToFormalResponse": { "count": 18, "averageBusinessMinutes": 5760, "medianBusinessMinutes": 4320 },
  "timeToClose": { "count": 10, "averageBusinessMinutes": 8640, "medianBusinessMinutes": 7200 }
}
```

- Duraciones en minutos hábiles ([[hu-039-calculo-de-tiempos-habiles|HU-039]]).
- Nunca contiene claves `sla`, `target`, `goal`, `threshold`, `breached` ni colores.
- `area` no se acepta como parámetro hasta resolver el hallazgo 3.

Errores (ProblemDetails):

| Código | Cuándo |
|---|---|
| 400 | `from` o `to` ausentes o mal formados; `from > to`; periodo mayor que 366 días; `status` fuera de `TicketStatus`; `companyId` no es GUID |
| 401 | Sin sesión |
| 403 | Cualquier rol distinto de `ProductManager` (incluido `Administrator`, propuesta) |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: ratificar métricas, definiciones de "sin asignar" y "activo", agregaciones (promedio y mediana), acceso del administrador y tratamiento de "área" y "primera respuesta" con la persona responsable del producto; registrar nombres en el [[glosario]].
- [ ] **T-02 — Caso de uso** · Capa: Backend Application · Dificultad: Medio  
  Descripción: primero pruebas unitarias con dobles de `IDashboardQueries`, `ICurrentUser` e `IClock`: solo `ProductManager`; validación de periodo (366 días válido, 367 inválido, `from > to` inválido); luego `GetGlobalDashboard`.
- [ ] **T-03 — Consultas agregadas** · Capa: Backend Infrastructure · Dificultad: Alto  
  Descripción: agregaciones en SQL (conteos por estado y empresa, sin asignar, carga) y obtención de fechas para el cálculo de tiempo hábil de HU-039; pruebas de integración con PostgreSQL real y un escenario sembrado con resultados conocidos.
- [ ] **T-04 — Índices y migración** · Capa: Persistencia · Dificultad: Bajo  
  Descripción: migración EF Core con los índices propuestos; verificar el plan de consulta con el volumen de [[hu-010-datos-sinteticos-de-desarrollo|HU-010]].
- [ ] **T-05 — Endpoint** · Capa: Backend Api · Dificultad: Bajo  
  Descripción: `GET /api/dashboard/global`; pruebas de integración de 200/400/401/403 y de ausencia de claves de SLA.
- [ ] **T-06 — Modelos y gateway** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: tipos `GlobalDashboard`, construcción de la query de filtros, reutilización de `formatBusinessDuration` de HU-039; pruebas Vitest.
- [ ] **T-07 — Controlador** · Capa: Frontend controllers · Dificultad: Medio  
  Descripción: `useGlobalDashboardController` con filtros (periodo, estados, empresa), valores por defecto (mes en curso en Bogotá) y estados `loading | ready | empty | error`; pruebas Vitest.
- [ ] **T-08 — Vista** · Capa: Frontend views · Dificultad: Medio  
  Descripción: `GlobalDashboardView` con tablas por sección, sin colores de cumplimiento ni metas; textos en español.

## Criterios de aceptación

### CHU-01 — Recibidos por periodo con filtros

**Dado** 10 tickets creados en octubre de 2026 (6 de la empresa A, 4 de la B; 3 en `New`) y 2 creados en septiembre  
**Cuando** la PM pide `?from=2026-10-01&to=2026-10-31`  
**Entonces** `received.total = 10`, `byCompany` muestra A = 6 y B = 4; **con** `&status=New` el total es 3; **y** con `&companyId={B}` el total es 4.

### CHU-02 — Límite del periodo en hora de Bogotá

**Dado** un ticket creado `2026-11-01T03:00:00Z` (31 de octubre, 22:00 en Bogotá)  
**Cuando** la PM pide `?from=2026-10-01&to=2026-10-31`  
**Entonces** el ticket se cuenta en octubre.

### CHU-03 — Sin asignar y carga por responsable

**Dado** 3 tickets en `New` sin asignación, 1 en `New` asignado a Laura, 2 en `InDevelopment` asignados a Laura y 1 `Closed` asignado a Laura  
**Cuando** la PM pide el panel  
**Entonces** `unassigned.count = 3` y `loadByAssignee` muestra Laura con `activeTickets = 3` (propuesta de "activo").

### CHU-04 — Tiempos en días hábiles

**Dado** un ticket creado `2026-10-09T22:00:00Z`, con respuesta formal `2026-10-12T14:00:00Z` y cierre `2026-10-14T14:00:00Z`  
**Cuando** la PM pide el panel  
**Entonces** su tiempo hasta respuesta formal entra en el cálculo como 960 minutos y su tiempo hasta cierre como 3840 minutos (regla de [[hu-039-calculo-de-tiempos-habiles|HU-039]], festivo del 12 de octubre incluido).

### CHU-05 — Validación del periodo con valores límite

**Dado** la PM autenticada  
**Cuando** pide un periodo de 367 días, `from=2026-10-31&to=2026-10-01`, `from=2026-13-01` o `status=Deployed`  
**Entonces** recibe 400 ProblemDetails; **y** un periodo de 366 días responde 200.

### CHU-06 — Solo la PM accede

**Dado** un administrador, Laura (Desarrollo), un solicitante y un coordinador  
**Cuando** llaman a `GET /api/dashboard/global`  
**Entonces** todos reciben 403 (el administrador, como propuesta pendiente de decisión) y sin sesión 401.

### CHU-07 — Sin SLA ni metas

**Dado** una respuesta 200 del panel  
**Cuando** se inspeccionan recursivamente sus claves  
**Entonces** ninguna contiene `sla`, `target`, `goal`, `threshold`, `breached` ni `color`; **y** la vista no aplica colores de cumplimiento a ningún valor.

### CHU-08 — UI vacía y con error

**Dado** un periodo sin tickets  
**Cuando** la PM consulta el panel  
**Entonces** ve "No hay tickets en el periodo seleccionado" y los tiempos como `—`; **y** ante un 500 ve un error con "Reintentar" conservando los filtros elegidos.

## Definition of Done

- [ ] CHU-01 a CHU-08 validados con evidencia.
- [ ] Pruebas en verde (`cd backend && dotnet test`): `GlobalDashboard_FiltraPorEmpresaYEstado`, `GlobalDashboard_PeriodoEnHoraDeBogota`, `GlobalDashboard_SinAsignarExcluyeAsignados`, `GlobalDashboard_AdministradorProhibido`, `GlobalDashboard_SinClavesDeSla`, `GlobalDashboard_Periodo367Dias_Rechazado`.
- [ ] Prueba de integración con un escenario sembrado de resultados conocidos contra PostgreSQL real.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Migración EF Core de índices creada y revisada.
- [ ] Vitest, `npm --prefix frontend run lint` y `npm --prefix frontend run build` en verde.
- [ ] El stack levanta con `docker compose up --build` y el panel responde con el volumen sintético de HU-010.
- [ ] Wiki actualizada: [[dashboard-y-metricas]] (definiciones ratificadas de sin asignar, activo, agregaciones), [[roles-y-permisos]] (acceso al panel), [[glosario]], [[criterios-de-aceptacion]] (evidencia de CA-13 o su bloqueo por "área").
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
| CHU-08 | Pendiente | — | — |
| DoD-01 Pruebas backend nombradas | Pendiente | — | — |
| DoD-02 Escenario sembrado en PostgreSQL | Pendiente | — | — |
| DoD-03 Pruebas de arquitectura | Pendiente | — | — |
| DoD-04 Migración de índices | Pendiente | — | — |
| DoD-05 Vitest, lint y build | Pendiente | — | — |
| DoD-06 `docker compose up --build` | Pendiente | — | — |
| DoD-07 Wiki actualizada | Pendiente | — | — |
| DoD-08 `quality-reviewer` | Pendiente | — | — |
| DoD-09 PR revisado | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- **Hallazgo 3 ("área"):** PRD §4, §10 y §14.13 la mencionan, pero no existe en el modelo; ¿es el equipo (`Team.Development`/`Team.Production`), la categoría u otro catálogo? Hasta decidirlo, CA-13 queda parcialmente cubierto.
- **"Primera respuesta"** (PRD §13) no está definida en §4; no se implementa.
- **Hallazgo 2:** si el "MVP" termina en la Fase 2, CA-13 no se cumpliría en el MVP; debe aclararse con la persona responsable del producto.
- (propuesta) El administrador no accede: el PRD §10 habla de "datos globales de PM" y §5 niega al administrador acceso por su rol. Si se decide lo contrario, el panel solo contiene agregados y nombres de empresas, sin contenido de tickets.
- **CHU-04:** del 9 de octubre (22:00Z) al 14 de octubre (14:00Z) = 7 h (vie) + 24 h (lun festivo) + 24 h (mar) + 9 h (mié) = 64 h = 3840 minutos.

## Relacionado

- [[ep-012-dashboard-y-metricas]] · [[tablero-scrum]] · [[dashboard-y-metricas]] · [[roles-y-permisos]] · [[elizabeth-pm]]
- [[estados-del-ticket]] · [[persistencia-postgresql]] · [[frontend-mvc]] · [[criterios-de-aceptacion]] · [[pendientes]]
