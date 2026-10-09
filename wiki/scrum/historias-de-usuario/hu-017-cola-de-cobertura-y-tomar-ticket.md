---
title: "HU-017 — Cola de cobertura y tomar ticket"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/roles, producto/seguridad, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §10", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-017", "Cola de cobertura y tomar ticket", "TakeTicket"]
epica: "[[ep-006-triage-asignacion-y-participantes]]"
criterios_prd: ["PRD CA-03", "PRD CA-14"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 3"
dependencias: ["[[hu-016-cola-global-de-triage]]", "[[hu-011-registrar-eventos-auditables]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-020-detalle-interno-del-ticket]]", "[[hu-004-contexto-de-usuario-y-autorizacion]]"]
relacionadas: ["[[hu-014-adjuntos-en-radicacion]]", "[[hu-019-reasignar-y-retirar-participantes]]", "[[hu-027-unirse-al-chat-del-ticket]]", "[[hu-038-correo-de-vinculacion]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-017 — Cola de cobertura y tomar ticket

Durante la ausencia de Elizabeth, un administrador ve una cola de triage reducida a nueve campos (sin descripción, adjuntos ni chat) y, al tomar un ticket, el sistema lo asocia como participante y lo audita **antes** de devolverle el detalle (PRD §5.5, §6.2.3, CA-03).

## Historia de usuario

**COMO** administrador de DataTicket que cubre el triage en ausencia de Elizabeth  
**QUIERO** ver los tickets pendientes con los datos mínimos para decidir y tomar uno para trabajarlo  
**PARA** que el triage no se detenga sin obtener acceso al contenido de tickets en los que no participo

## Contexto

Un administrador no obtiene acceso general a mensajes y archivos por serlo. Durante la ausencia de Elizabeth puede consultar la cola de triage con número, empresa, título, categoría, urgencia, impacto, prioridad calculada, fecha de creación y estado resumido; no ve descripción completa, archivos ni chat. Al abrir/tomar un ticket, la acción lo asocia como participante antes de cargar el detalle y sus adjuntos (PRD §5.5, §6.2.3, §10). La asociación es una incorporación de participante auditable ([[auditoria]]). Cómo se determina la "ausencia" no está definido (V-05). El caso de uso `TakeTicket` ya figura en el mapa de puertos ([[backend-hexagonal]]).

## Alcance

- Consulta `GetCoverageQueue` (nombre propuesto, ratificar en T-01 y registrar en el glosario): proyección restringida de `TriageQueue` con exactamente los nueve campos del PRD más el `id` técnico.
- Caso de uso `TakeTicket`: crea `TicketParticipant` (origen cobertura), registra `ParticipantAdded`, confirma la transacción y solo después lee y devuelve el detalle interno (forma de [[hu-020-detalle-interno-del-ticket|HU-020]]).
- Endpoints `GET /api/coverage-queue` y `POST /api/tickets/{ticketId}/take`.
- Vista de la cola de cobertura con acción "Tomar ticket" y confirmación explícita.

## Fuera de alcance

- Mecanismo para declarar o detectar la ausencia de Elizabeth (V-05): la cola está disponible para administradores siempre (propuesta).
- Correo de vinculación al administrador que toma (V-08; [[hu-038-correo-de-vinculacion|HU-038]]).
- Unirse al chat tras tomar: [[hu-027-unirse-al-chat-del-ticket|HU-027]] (validará la participación creada aquí).
- Asignar a otras personas desde la cobertura: [[hu-018-asignar-y-agregar-participantes|HU-018]].
- Tomar tickets que ya no están en `New` (propuesta: el administrador se agrega con HU-018).

## Requisitos y reglas de negocio

- Campos visibles en la cola de cobertura: número, empresa, título, categoría, urgencia, impacto, prioridad calculada, fecha de creación y estado resumido (PRD §5.5, §6.2.3, §10).
- No se muestran descripción completa, archivos ni chat antes de asociarse (PRD §5.5, §10).
- Al abrir/tomar, la asociación como participante ocurre antes de cargar el detalle y los adjuntos (PRD §5.5, §6.2.3, §14.3).
- La incorporación de participante se audita (PRD §6.2.4, §12).
- "Estado resumido" = `ClientStatus` (PRD §6.4).
- (propuesta, V-05) La cola de cobertura está siempre disponible para `Administrator`.
- (propuesta) Solo aparecen tickets en `New`; tomar un ticket en otro estado responde `409`.
- (propuesta) `TakeTicket` es idempotente para un participante vigente: no duplica participación ni auditoría.
- (propuesta) Solo `Administrator` usa la cola de cobertura y `TakeTicket`; la PM accede siempre sin tomar.

## Invariantes en juego

- Invariante 5: el administrador no ve contenido por serlo; queda asociado como participante **antes** de cargar el detalle.
- Invariante 8: la toma queda auditada con actor y fecha.
- Invariante 1 (contrapartida): ningún rol cliente accede a estas rutas.

## Criterios del PRD cubiertos

- PRD CA-03 (total junto con [[hu-016-cola-global-de-triage|HU-016]]: campos limitados y asociación previa al detalle) → [[criterios-de-aceptacion]]
- PRD CA-14 (parcial: participantes) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-006-triage-asignacion-y-participantes|EP-006 — Triage, asignación y participantes]]
- Dependencias: [[hu-016-cola-global-de-triage|HU-016 — Cola global de triage]] (consulta base) · [[hu-011-registrar-eventos-auditables|HU-011 — Registrar eventos auditables]] · [[hu-018-asignar-y-agregar-participantes|HU-018 — Asignar y agregar participantes]] (entidad `TicketParticipant` y su tabla; si HU-017 se implementa primero, crea la entidad y HU-018 la reutiliza) · [[hu-020-detalle-interno-del-ticket|HU-020 — Detalle interno del ticket]] (forma del detalle y control de acceso) · [[hu-004-contexto-de-usuario-y-autorizacion|HU-004 — Contexto de usuario y autorización]]
- Relacionadas: [[hu-014-adjuntos-en-radicacion|HU-014]] · [[hu-019-reasignar-y-retirar-participantes|HU-019]] · [[hu-027-unirse-al-chat-del-ticket|HU-027]] · [[hu-038-correo-de-vinculacion|HU-038]]
- Decisiones: V-05 · V-08 ([[pendientes]])

## Componentes afectados

- Backend (Domain): `Ticket.AddParticipant(...)` con origen de cobertura (propuesta).
- Backend (Application): `GetCoverageQueue` (propuesto), `TakeTicket`, `ITicketRepository`, `IAuditLog`, `ICurrentUser`, `IClock`.
- Backend (Infrastructure): proyección restringida; tabla de participantes (si no la creó HU-018).
- Backend (Api): `GET /api/coverage-queue`, `POST /api/tickets/{ticketId}/take`.
- Frontend: módulo `src/modules/triage` (cola de cobertura y acción de toma).

## Dificultad

**Nivel:** Alto

**Justificación:** implementa un invariante de seguridad del PRD cuyo orden (asociar y auditar antes de leer) debe demostrarse con pruebas; la respuesta de la cola debe probarse por ausencia de campos; depende del detalle de HU-020 y de la entidad de participantes en el mismo sprint.

## Contrato backend ↔ frontend

Propuesta, ratificar en T-01.

**`GET /api/coverage-queue?cursor={cursor}&limit=50`** — solo `Administrator`. Orden provisional por `createdAt` ascendente (V-03).

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
      "clientStatus": "Received"
    }
  ],
  "nextCursor": null
}
```

Cada elemento tiene **exactamente** las claves `id`, `number`, `company`, `title`, `category`, `urgency`, `impact`, `calculatedPriority`, `createdAt`, `clientStatus`. Nunca `description`, `attachments`, `messages`, `participants`, `status` interno ni `effectivePriority`.

**`POST /api/tickets/{ticketId}/take`** — solo `Administrator`. Sin cuerpo. Respuesta `200 OK` con el detalle interno (forma de HU-020), por ejemplo:

```json
{
  "id": "3f4e…",
  "number": "DT-000123",
  "company": { "id": "a0c1…", "name": "Empresa Sintética A" },
  "title": "Error al generar la factura electrónica",
  "description": "Desde ayer el módulo de facturación devuelve un error 500…",
  "status": "New",
  "attachments": [ { "id": "a1b2…", "fileName": "factura-error.pdf", "url": "http://localhost:10000/…" } ],
  "participants": [ { "userId": "ad01…", "displayName": "Administrador Sintético", "addedAt": "2026-10-08T13:00:00Z", "origin": "CoverageTake" } ]
}
```

El chat no viaja en esta respuesta: se carga con su API ([[hu-026-historial-del-chat-por-cursor|HU-026]]) tras la asociación.

| Código | Cuándo |
|---|---|
| 400 | `limit` fuera de 1–100 o `cursor` mal formado |
| 401 | Sin sesión |
| 403 | Rol distinto de `Administrator` |
| 404 | Ticket inexistente |
| 409 | Ticket fuera de `New` (propuesta) |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: fijar la lista exacta de claves de la cola, la forma del detalle (alineada con HU-020), la idempotencia y los códigos. Llevar V-05 a la persona usuaria con la propuesta "siempre disponible". Registrar `GetCoverageQueue` en [[glosario]].
- [ ] **T-02 — Domain: participación por cobertura** · Capa: Backend (Domain) · Dificultad: Bajo  
  Descripción: pruebas primero: `AddParticipant_FromCoverage_SetsAddedByAndAddedAt`, `AddParticipant_WhenAlreadyActive_IsNoOp`, `AddParticipant_AfterRemoval_CreatesNewParticipation`. Reutilizar `TicketParticipant` (HU-018) con origen `CoverageTake` (nombre propuesto).
- [ ] **T-03 — Application: `TakeTicket` y `GetCoverageQueue`** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: pruebas unitarias primero con dobles/espías: `TakeTicket_PersistsParticipantAndAuditBeforeReadingDetail` (orden de llamadas), `TakeTicket_WhenSaveFails_NeverReadsDetail`, `TakeTicket_WhenAlreadyParticipant_DoesNotDuplicateAudit`, `TakeTicket_OnNonNewTicket_ReturnsConflict`, `TakeTicket_AsDeveloper_IsForbidden`, `GetCoverageQueue_AsRequester_IsForbidden`.
- [ ] **T-04 — Infrastructure: proyección restringida** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: pruebas de integración primero: `CoverageQueue_ProjectsOnlyAllowedColumns` (la consulta SQL generada no selecciona `description`), `CoverageQueue_ListsOnlyNewTickets`. Proyección a un DTO dedicado; nunca reutilizar el DTO del detalle.
- [ ] **T-05 — Api: endpoints** · Capa: Backend (Api) · Dificultad: Medio  
  Descripción: pruebas de integración primero: `GetCoverageQueue_ItemsHaveExactlyAllowedKeys`, `GetCoverageQueue_BodyNeverContainsDescriptionText`, `PostTake_ReturnsDetail_AndParticipantExists`, `PostTake_AsRequester_Returns403`, `PostTake_Twice_SingleParticipationAndAuditEntry`.
- [ ] **T-06 — Frontend models** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: pruebas Vitest primero: `toCoverageQueueItem` (ignora claves no esperadas, no las propaga), `toTakeResult`. Implementar `coverageQueue.ts` y gateway (`fetchCoverageQueue`, `takeTicket`).
- [ ] **T-07 — Frontend controller** · Capa: Frontend (controllers) · Dificultad: Bajo  
  Descripción: `useCoverageQueueController` con confirmación de toma, estado `taking`, manejo de `409` y navegación al detalle con los datos devueltos. Prueba Vitest del reducer.
- [ ] **T-08 — Frontend views** · Capa: Frontend (views) · Dificultad: Bajo  
  Descripción: `CoverageQueueView` (solo las columnas del contrato) y `TakeTicketConfirmView` ("Al tomarlo quedarás asociado como participante y quedará registrado en la auditoría"). Lint MVC sin violaciones.

## Criterios de aceptación

### CHU-01 — La cola de cobertura muestra solo los campos enumerados

**Dado** un ticket `New` de la empresa A con descripción "Desde ayer el módulo…" y un PDF adjunto  
**Cuando** un administrador llama a `GET /api/coverage-queue`  
**Entonces** recibe `200` y el elemento del ticket tiene exactamente las claves `id`, `number`, `company`, `title`, `category`, `urgency`, `impact`, `calculatedPriority`, `createdAt`, `clientStatus`, con `clientStatus: "Received"`.

### CHU-02 — Ausencia verificada de descripción, adjuntos y chat

**Dado** el mismo ticket con descripción, adjunto y (cuando exista) mensajes de chat  
**Cuando** se inspecciona el cuerpo completo de la respuesta de la cola  
**Entonces** no contiene las claves `description`, `attachments`, `messages`, `participants` ni `status`, ni el texto de la descripción, ni el nombre del archivo, ni su URL.

### CHU-03 — Tomar asocia y audita antes de entregar el detalle

**Dado** un administrador no asociado al ticket `New` y `IClock` fijo en `2026-10-08T13:00:00Z`  
**Cuando** envía `POST /api/tickets/{ticketId}/take`  
**Entonces** recibe `200` con descripción y adjuntos; existe un `TicketParticipant` vigente con `AddedBy` = el administrador y `AddedAt` = `2026-10-08 13:00:00+00`; existe una entrada `ParticipantAdded` (actor = administrador, objeto `TicketParticipant`, `new_value.origin` = `CoverageTake`); y la prueba con espías demuestra que la confirmación de participante y auditoría ocurre antes de la lectura del detalle.

### CHU-04 — Si la asociación falla, no hay detalle

**Dado** un fallo forzado al confirmar la participación  
**Cuando** el administrador intenta tomar el ticket  
**Entonces** la respuesta es de error (ProblemDetails) sin `description` ni `attachments`, la consulta del detalle no se ejecuta y no queda participante ni entrada de auditoría.

### CHU-05 — Sin tomar no hay detalle

**Dado** un administrador no asociado  
**Cuando** solicita directamente el detalle interno del ticket ([[hu-020-detalle-interno-del-ticket|HU-020]]) o su bitácora (HU-012)  
**Entonces** recibe una respuesta 4xx sin descripción, adjuntos ni chat.

### CHU-06 — Idempotencia

**Dado** un administrador que ya tomó el ticket  
**Cuando** vuelve a enviar `POST /api/tickets/{ticketId}/take`  
**Entonces** recibe `200` con el detalle y sigue existiendo una sola participación vigente y una sola entrada `ParticipantAdded` de esa toma.

### CHU-07 — Autorización por rol en backend

**Dado** Elizabeth (PM), Laura (Desarrollo), Julián (Producción), una solicitante de la empresa A y una petición sin sesión  
**Cuando** cada uno llama a `GET /api/coverage-queue` y a `POST /api/tickets/{ticketId}/take`  
**Entonces** PM, Laura, Julián y la solicitante reciben `403` en ambas; sin sesión `401`; y no se crea ninguna participación.

### CHU-08 — Estado y existencia

**Dado** un ticket en `InDevelopment` y un identificador inexistente  
**Cuando** un administrador intenta tomarlos  
**Entonces** el primero responde `409` (propuesta) y no aparece en la cola de cobertura; el segundo responde `404`.

### CHU-09 — Comportamiento de la UI

**Dado** la cola de cobertura abierta  
**Cuando** el administrador pulsa "Tomar ticket"  
**Entonces** se muestra la confirmación que explica la asociación y la auditoría; al confirmar se deshabilita el botón y, con `200`, se abre el detalle; con `409` se muestra "Este ticket ya no está pendiente de triage; pide a la PM que te agregue" y la fila se retira al recargar; con fallo de red se muestra `role="alert"` con "Reintentar" sin asociar dos veces.

## Definition of Done

- [ ] CHU-01 a CHU-09 validados con evidencia.
- [ ] Pruebas backend escritas primero y en verde (`cd backend && dotnet test`): `GetCoverageQueue_ItemsHaveExactlyAllowedKeys`, `GetCoverageQueue_BodyNeverContainsDescriptionText`, `TakeTicket_PersistsParticipantAndAuditBeforeReadingDetail`, `TakeTicket_WhenSaveFails_NeverReadsDetail`, `PostTake_Twice_SingleParticipationAndAuditEntry`, `PostTake_AsRequester_Returns403`.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Migración de participantes aplicada y reversible (si esta HU crea la tabla antes que HU-018).
- [ ] Pruebas Vitest en verde (`npm --prefix frontend test`): `toCoverageQueueItem`, `toTakeResult`, reducer.
- [ ] `npm --prefix frontend run lint` sin errores y `npm --prefix frontend run build` correcto.
- [ ] `docker compose up --build`: un administrador sintético ve la cola reducida, toma un ticket y ve el detalle.
- [ ] Wiki: [[roles-y-permisos]] (cobertura implementada y propuesta V-05), [[flujo-del-ticket]], [[auditoria]] (origen `CoverageTake`), [[backend-hexagonal]], [[glosario]] mediante Notas para la wiki; PRD CA-03 propuesto para marcar en [[criterios-de-aceptacion]] junto con HU-016.
- [ ] `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (revisión explícita del orden asociar → leer).
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
| CHU-08 | Pendiente | — | — |
| CHU-09 | Pendiente | — | — |
| DoD-01 Pruebas backend | Pendiente | — | — |
| DoD-02 ArchitectureTests | Pendiente | — | — |
| DoD-03 Migración de participantes (si aplica) | Pendiente | — | — |
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

- **V-05** — sin definición de "ausencia", la propuesta es no condicionar la cola a ningún interruptor; el control real es que la cola no expone contenido y toda toma queda auditada.
- **V-08** — si se decide enviar correo de vinculación al administrador que toma, lo implementa HU-038 escuchando la participación creada aquí.
- (propuesta) DTO dedicado para la cola de cobertura: impide que un cambio en el DTO de la cola de la PM filtre campos por accidente.

## Relacionado

- [[ep-006-triage-asignacion-y-participantes]] · [[tablero-scrum]] · [[roles-y-permisos]] · [[flujo-del-ticket]] · [[auditoria]]
- [[backend-hexagonal]] · [[frontend-mvc]] · [[modelo-de-dominio]] · [[criterios-de-aceptacion]] · [[pendientes]]
