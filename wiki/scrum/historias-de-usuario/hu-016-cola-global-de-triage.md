---
title: "HU-016 — Cola global de triage"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/ticket, producto/roles, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §6.2", "PRD.md §5", "PRD.md §10", "PRD.md §14"]
aliases: ["HU-016", "Cola global de triage", "TriageQueue"]
epica: "[[ep-006-triage-asignacion-y-participantes]]"
criterios_prd: ["PRD CA-03"]
componentes: ["Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 2"
dependencias: ["[[hu-013-radicar-ticket]]", "[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-002-shell-y-navegacion-por-rol]]", "[[hu-007-administrar-empresas-cliente]]"]
relacionadas: ["[[hu-017-cola-de-cobertura-y-tomar-ticket]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-015-ajustar-prioridad-manualmente]]", "[[hu-020-detalle-interno-del-ticket]]", "[[hu-023-bandeja-de-mis-tickets-y-equipo]]", "[[hu-040-panel-global-de-la-pm]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-016 — Cola global de triage

Elizabeth (PM) ve en una bandeja global todos los tickets pendientes de triage (`New`) de todas las empresas y, con un filtro, todos los tickets para seguimiento, con número, empresa, título, categoría, urgencia, impacto, prioridad calculada, fecha de creación y estado (PRD §6.2, §10).

## Historia de usuario

**COMO** Elizabeth (PM)  
**QUIERO** ver en una sola cola todos los tickets nuevos de todas las empresas, con su prioridad calculada y antigüedad, y poder ampliar la vista a todos los tickets  
**PARA** hacer el triage sin que ningún ticket se pierda y seguir el estado global del soporte

## Contexto

Al radicarse, el ticket entra en la cola general de Elizabeth con estado `New` (PRD §6.2.1). Elizabeth cuenta con una bandeja global para triage y seguimiento (PRD §10) y puede consultar todos los tickets (PRD §5). CA-03 exige que un ticket nuevo aparezca en esa cola. `TriageQueue` está en el glosario como modelo de lectura, no como agregado ([[modelo-de-dominio]]). El orden de la cola no está definido (V-03). La vista restringida del administrador de cobertura es [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]].

## Alcance

- Caso de uso de lectura `GetTriageQueue` (nombre propuesto) con puerto `ITriageQueueQueries` (nombre propuesto), ambos a ratificar en T-01 y registrar en el glosario.
- Endpoint `GET /api/triage-queue` paginado por cursor, con filtros por estado (por defecto `New`; `all` para seguimiento) y por empresa.
- Orden provisional por fecha de creación ascendente (más antiguo primero) hasta resolver V-03.
- Vista de la cola en el espacio interno con filtros, "Cargar más", acceso al detalle (HU-020) y estado vacío.

## Fuera de alcance

- Vista restringida y toma de tickets por el administrador: [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]].
- Asignar desde la cola: [[hu-018-asignar-y-agregar-participantes|HU-018]].
- Bandejas por equipo y responsabilidad: [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]].
- Métricas, antigüedad en días hábiles y carga: [[hu-040-panel-global-de-la-pm|HU-040]] y [[hu-039-calculo-de-tiempos-habiles|HU-039]].
- Actualización en tiempo real de la cola (no la pide el PRD; se recarga a demanda).
- Vistas guardadas y exportables (PRD §10, §16.4).
- Filtro por "área" (el concepto no existe en el modelo, hallazgo 3).

## Requisitos y reglas de negocio

- El ticket nuevo entra en la cola general de Elizabeth con estado `New` (PRD §6.2.1).
- Elizabeth revisa categoría, urgencia e impacto (PRD §6.2.2).
- Elizabeth puede consultar todos los tickets; bandeja global para triage y seguimiento (PRD §5, §10).
- La cola muestra la prioridad calculada (PRD §5.5; [[matriz-de-prioridad]]).
- (propuesta) Solo el rol `ProductManager` usa esta cola; el administrador usa la de cobertura (HU-017).
- (propuesta, V-03) Orden por `createdAt` ascendente y `id` como desempate; tamaño de página 50 por defecto, máximo 100.
- (propuesta) Cuando exista [[hu-015-ajustar-prioridad-manualmente|HU-015]], cada fila añade `effectivePriority`; el orden no cambia hasta decidir V-03.

## Invariantes en juego

- Invariante 1 (contrapartida): la cola global es interna; ningún rol cliente accede a ella.
- Invariante 2: la cola expone estado interno, por lo que solo la ve la PM.

## Criterios del PRD cubiertos

- PRD CA-03 (parcial: "un ticket nuevo aparece en la cola global de Elizabeth"; la parte del administrador la cubre HU-017) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-006-triage-asignacion-y-participantes|EP-006 — Triage, asignación y participantes]]
- Dependencias: [[hu-013-radicar-ticket|HU-013 — Radicar ticket]] · [[hu-004-contexto-de-usuario-y-autorizacion|HU-004 — Contexto de usuario y autorización]] · [[hu-002-shell-y-navegacion-por-rol|HU-002 — Shell y navegación por rol]] · [[hu-007-administrar-empresas-cliente|HU-007 — Administrar empresas cliente]] (nombre de la empresa)
- Relacionadas: [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] · [[hu-018-asignar-y-agregar-participantes|HU-018]] · [[hu-015-ajustar-prioridad-manualmente|HU-015]] · [[hu-020-detalle-interno-del-ticket|HU-020]] · [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]] · [[hu-040-panel-global-de-la-pm|HU-040]]
- Decisiones: V-03 (orden) · enrutador y estado de servidor del frontend abiertos ([[pendientes]] §4)

## Componentes afectados

- Backend (Application): `GetTriageQueue`, `ITriageQueueQueries`.
- Backend (Infrastructure): consulta EF Core de solo lectura con proyección (sin cargar descripción).
- Backend (Api): `GET /api/triage-queue`.
- Frontend: módulo propuesto `src/modules/triage` (models, controllers, views).

## Dificultad

**Nivel:** Medio

**Justificación:** consulta de solo lectura con paginación por cursor y filtros, autorización por rol y una vista con estados de carga; sin escritura ni migración propia, pero cruza backend y frontend.

## Contrato backend ↔ frontend

Propuesta, ratificar en T-01.

**`GET /api/triage-queue?status=New&companyId={companyId}&cursor={cursor}&limit=50`** — solo `ProductManager`.

- `status`: `New` (por defecto), cualquier valor de `TicketStatus` o `all`.
- `companyId`: opcional.

Respuesta `200 OK`:

```json
{
  "items": [
    {
      "id": "3f4e…",
      "number": "DT-000123",
      "company": { "id": "a0c1…", "name": "Empresa Sintética A" },
      "title": "Error al generar la factura electrónica",
      "category": { "id": "c1a2…", "name": "Incidente" },
      "urgency": "High",
      "impact": "Medium",
      "calculatedPriority": "High",
      "createdAt": "2026-10-07T15:00:00Z",
      "status": "New",
      "clientStatus": "Received"
    }
  ],
  "nextCursor": null
}
```

| Código | Cuándo |
|---|---|
| 400 | `status` desconocido, `limit` fuera de 1–100, `cursor` mal formado |
| 401 | Sin sesión |
| 403 | Cualquier rol distinto de `ProductManager` (incluido `Administrator`, que usa HU-017) |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: fijar ruta, filtros, forma del cursor, campos y orden provisional (V-03). Registrar `GetTriageQueue` e `ITriageQueueQueries` en [[glosario]] y [[backend-hexagonal]].
- [ ] **T-02 — Application: `GetTriageQueue`** · Capa: Backend (Application) · Dificultad: Bajo  
  Descripción: pruebas unitarias primero: `GetTriageQueue_AsProductManager_ReturnsItems`, `GetTriageQueue_AsAdministrator_IsForbidden`, `GetTriageQueue_AsDeveloper_IsForbidden`, `GetTriageQueue_AsRequester_IsForbidden`. Implementar con `ICurrentUser` e `ITriageQueueQueries`.
- [ ] **T-03 — Infrastructure: consulta EF Core** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: pruebas de integración primero (PostgreSQL real, datos de dos empresas): `TriageQueue_DefaultsToNewOnly`, `TriageQueue_IncludesAllCompanies`, `TriageQueue_FiltersByCompany`, `TriageQueue_OrdersByCreatedAtThenId`, `TriageQueue_PaginatesWithoutGapsOrDuplicates`. Proyección `AsNoTracking` sin `description`; índice `(status, created_at, id)` si hace falta (migración menor).
- [ ] **T-04 — Api: endpoint** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: pruebas de integración primero: `GetTriageQueue_AfterSubmission_ContainsNewTicket`, `GetTriageQueue_AsAdministrator_Returns403`, `GetTriageQueue_StatusUnknown_Returns400`, `GetTriageQueue_Limit101_Returns400`.
- [ ] **T-05 — Frontend models** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: pruebas Vitest primero: `toTriageQueueItem` (normaliza; estado desconocido → marcado como desconocido sin romper), etiquetas en español de estados y prioridades, `mergePage` sin duplicados. Implementar `triageQueue.ts` y `triageQueueGateway.ts`.
- [ ] **T-06 — Frontend controller** · Capa: Frontend (controllers) · Dificultad: Bajo  
  Descripción: `useTriageQueueController` con filtros, `loadMore`, `refresh`, estados `loading/ready/empty/error` y cancelación con `AbortController`. Prueba Vitest del reducer.
- [ ] **T-07 — Frontend views** · Capa: Frontend (views) · Dificultad: Bajo  
  Descripción: `TriageQueueView` (tabla con los campos del contrato, fecha en `America/Bogota`, prioridad con etiqueta textual además de color, filtros de estado y empresa, "Cargar más", "Actualizar"). Lint MVC sin violaciones.

## Criterios de aceptación

### CHU-01 — Un ticket nuevo aparece en la cola

**Dado** un ticket recién radicado por una solicitante de la empresa A (HU-013)  
**Cuando** Elizabeth consulta `GET /api/triage-queue`  
**Entonces** el ticket aparece con número, empresa, título, categoría, urgencia, impacto, prioridad calculada, fecha de creación y estado `New`, y la vista lo muestra en la tabla con la fecha en hora `America/Bogota`.

### CHU-02 — Cola global de todas las empresas

**Dado** tickets `New` de las empresas A y B  
**Cuando** Elizabeth consulta la cola sin filtro y luego con `companyId` = B  
**Entonces** sin filtro aparecen los de A y B; con el filtro solo los de B.

### CHU-03 — Filtro de estado para seguimiento

**Dado** tickets en `New`, `InDevelopment`, `SolutionDelivered` y `Closed`  
**Cuando** Elizabeth consulta sin `status`, con `status=all` y con `status=Bogus`  
**Entonces** sin `status` solo aparecen los `New`; con `all` aparecen los cuatro; con `Bogus` la respuesta es `400`.

### CHU-04 — Orden y paginación

**Dado** 120 tickets `New` creados en instantes distintos, dos de ellos con el mismo `createdAt`  
**Cuando** se recorren páginas con `limit=50`  
**Entonces** se obtienen 50 + 50 + 20 tickets ordenados del más antiguo al más reciente (desempate por `id`), sin duplicados ni omisiones; `limit=101` responde `400`.

### CHU-05 — Autorización por rol en backend

**Dado** un administrador, un desarrollador, Julián (Producción), un coordinador de la empresa A y una petición sin sesión  
**Cuando** cada uno llama a `GET /api/triage-queue`  
**Entonces** administrador, desarrollador, Julián y coordinador reciben `403`; sin sesión `401`; ninguno recibe filas.

### CHU-06 — Estado vacío

**Dado** que no hay tickets `New`  
**Cuando** Elizabeth abre la cola  
**Entonces** recibe `200` con `items: []` y la vista muestra "No hay tickets pendientes de triage".

### CHU-07 — Comportamiento de la UI ante error

**Dado** la cola abierta con una página cargada  
**Cuando** "Cargar más" falla por red o el backend responde `500`  
**Entonces** las filas ya cargadas se conservan, se muestra un `role="alert"` con "Reintentar" y reintentar solicita el mismo cursor sin duplicar filas.

## Definition of Done

- [ ] CHU-01 a CHU-07 validados con evidencia.
- [ ] Pruebas backend escritas primero y en verde (`cd backend && dotnet test`): `GetTriageQueue_AfterSubmission_ContainsNewTicket`, `TriageQueue_IncludesAllCompanies`, `TriageQueue_DefaultsToNewOnly`, `TriageQueue_PaginatesWithoutGapsOrDuplicates`, `GetTriageQueue_AsAdministrator_Returns403`.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Migración solo si se añade el índice `(status, created_at, id)`; en ese caso, aplicada y reversible.
- [ ] Pruebas Vitest en verde (`npm --prefix frontend test`): `toTriageQueueItem`, `mergePage`, reducer del controlador.
- [ ] `npm --prefix frontend run lint` sin errores y `npm --prefix frontend run build` correcto.
- [ ] `docker compose up --build`: con datos sintéticos (HU-010) Elizabeth ve la cola en `http://localhost:5173`.
- [ ] Wiki: [[flujo-del-ticket]] (cola implementada), [[elizabeth-pm]], [[backend-hexagonal]] (consulta nueva), [[frontend-mvc]] (módulo `triage`), [[glosario]] mediante Notas para la wiki; V-03 anotado con el orden provisional.
- [ ] `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos.
- [ ] PR revisado y aprobado por otra persona del equipo.
- [ ] Trazabilidad de la HU y de [[ep-006-triage-asignacion-y-participantes]] actualizada.

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
| DoD-01 Pruebas backend | Pendiente | — | — |
| DoD-02 ArchitectureTests | Pendiente | — | — |
| DoD-03 Migración de índice (si aplica) | Pendiente | — | — |
| DoD-04 Pruebas Vitest | Pendiente | — | — |
| DoD-05 Lint y build frontend | Pendiente | — | — |
| DoD-06 `docker compose up --build` | Pendiente | — | — |
| DoD-07 Wiki actualizada | Pendiente | — | — |
| DoD-08 quality-reviewer | Pendiente | — | — |
| DoD-09 PR revisado | Pendiente | — | — |
| DoD-10 Trazabilidad | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- **V-03** — orden provisional "más antiguo primero"; si se decide ordenar por prioridad (calculada o vigente), solo cambia la consulta de T-03.
- (propuesta) Cuando exista HU-018, conviene añadir a cada fila un indicador "sin asignar" para apoyar la métrica de PRD §4; se deja como mejora en HU-018/HU-040.
- La PM tiene acceso total por rol (PRD §5.3); por eso no hay prueba multiempresa negativa para ella, pero sí para los roles cliente (CHU-05).

## Relacionado

- [[ep-006-triage-asignacion-y-participantes]] · [[tablero-scrum]] · [[flujo-del-ticket]] · [[elizabeth-pm]] · [[roles-y-permisos]]
- [[matriz-de-prioridad]] · [[estados-del-ticket]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[criterios-de-aceptacion]] · [[pendientes]]
