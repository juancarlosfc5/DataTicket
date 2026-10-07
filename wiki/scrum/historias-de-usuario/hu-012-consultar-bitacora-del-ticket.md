---
title: "HU-012 — Consultar la bitácora del ticket"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/auditoria, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §12", "PRD.md §5", "PRD.md §6.4", "PRD.md §10", "PRD.md §14"]
aliases: ["HU-012", "Consultar la bitácora del ticket"]
epica: "[[ep-004-auditoria-append-only]]"
criterios_prd: ["PRD CA-04", "PRD CA-14", "PRD CA-10"]
componentes: ["Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 4"
dependencias: ["[[hu-011-registrar-eventos-auditables]]", "[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-020-detalle-interno-del-ticket]]"]
relacionadas: ["[[hu-019-reasignar-y-retirar-participantes]]", "[[hu-021-cambiar-estado-interno]]", "[[hu-024-portal-solicitante-consulta-tickets]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-012 — Consultar la bitácora del ticket

Vista cronológica, dentro del detalle interno del ticket, de las entradas de auditoría (quién, cuándo, qué, sobre qué objeto, anterior/nuevo) para personas internas autorizadas. El cliente nunca la ve (PRD §5.6, §12). **Quién puede verla no está definido en el PRD: la regla de acceso es propuesta.**

## Historia de usuario

**COMO** Elizabeth (PM) o persona interna participante del ticket  
**QUIERO** ver en orden cronológico la bitácora de auditoría del ticket con actor, fecha/hora, acción y valores anterior/nuevo  
**PARA** entender qué pasó con el ticket (prioridad, estados, asignaciones, participantes, respuesta, cierre) sin preguntar por el chat ni depender del correo

## Contexto

[[hu-011-registrar-eventos-auditables|HU-011]] deja las entradas append-only; esta HU las expone. El PRD establece que los movimientos internos, reasignaciones y cambios de participantes se registran en la bitácora (PRD §6.4) y que el registro de auditoría es la fuente de trazabilidad (PRD §11), pero no dice quién puede consultarla. Como la bitácora contiene nombres de colaboradores internos, estados técnicos y reasignaciones, nunca puede llegar al cliente (PRD §5.6, CA-10). La vista se integra en el detalle interno de [[hu-020-detalle-interno-del-ticket|HU-020]].

## Alcance

- Caso de uso de consulta `GetTicketAuditTrail` (nombre propuesto, ratificar en T-01 y registrar en el glosario) con puerto de lectura `IAuditTrailQueries` (nombre propuesto).
- Endpoint `GET /api/tickets/{ticketId}/audit-entries` paginado por cursor, orden cronológico ascendente.
- Regla de acceso (propuesta): PM (`ProductManager`) siempre; participante interno vigente; administrador solo si es participante vigente.
- Resolución de nombres para mostrar (actor y personas en anterior/nuevo) en el momento de la consulta.
- Pestaña o sección "Bitácora" en el detalle interno con etiquetas en español por acción y fechas en `America/Bogota`.

## Fuera de alcance

- Bitácora global multi-ticket, filtros avanzados y exportación (PRD §10, §16.4).
- Auditoría de la propia consulta de la bitácora (no la enumera PRD §12).
- Visualización de intentos denegados (V-11).
- Cualquier exposición en el portal del cliente.

## Requisitos y reglas de negocio

- La bitácora registra actor, fecha/hora, acción, objeto, valores anterior/nuevo y resultado (PRD §12).
- Movimientos internos, reasignaciones y cambios de participantes se registran en la bitácora (PRD §6.4).
- El cliente nunca ve mensajes internos, estados técnicos, URL de PR, nombres de colaboradores internos ni actividad privada (PRD §5.6).
- El administrador no obtiene acceso al contenido por ser administrador (PRD §5.5).
- Un participante retirado pierde el acceso futuro (PRD §5.4).
- (propuesta) Acceso: PM siempre; participantes internos vigentes; administrador solo si es participante vigente. Coherente con PRD §5.3–§5.5, pero el PRD no lo define (nueva duda para [[pendientes]]).
- (propuesta) Orden ascendente por `(occurredAt, id)`; tamaño de página por defecto 50, máximo 100.

## Invariantes en juego

- Invariante 2: el cliente nunca ve actividad interna (la bitácora completa es interna).
- Invariante 5: el administrador no ve contenido por serlo.
- Invariante 8: la consulta no modifica entradas (solo lectura).

## Criterios del PRD cubiertos

- PRD CA-04 (parcial: hace visible la auditoría de asignar, reasignar y agregar/quitar) → [[criterios-de-aceptacion]]
- PRD CA-14 (parcial: hace visibles actor y fecha/hora) → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial: la bitácora no es accesible al cliente) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-004-auditoria-append-only|EP-004 — Auditoría append-only]]
- Dependencias: [[hu-011-registrar-eventos-auditables|HU-011 — Registrar eventos auditables]] · [[hu-004-contexto-de-usuario-y-autorizacion|HU-004 — Contexto de usuario y autorización]] · [[hu-018-asignar-y-agregar-participantes|HU-018 — Asignar y agregar participantes]] (participación vigente) · [[hu-020-detalle-interno-del-ticket|HU-020 — Detalle interno del ticket]] (contenedor de la vista)
- Relacionadas: [[hu-019-reasignar-y-retirar-participantes|HU-019]] · [[hu-021-cambiar-estado-interno|HU-021]] · [[hu-024-portal-solicitante-consulta-tickets|HU-024]]
- Decisiones: regla de acceso pendiente (nueva duda) · V-11 · enrutador y estado de servidor del frontend abiertos ([[pendientes]] §4)

## Componentes afectados

- Backend (Application): `GetTicketAuditTrail`, `IAuditTrailQueries`, regla de acceso reutilizada del ticket.
- Backend (Infrastructure): consulta EF Core sobre la tabla de auditoría con proyección y resolución de nombres.
- Backend (Api): endpoint `GET /api/tickets/{ticketId}/audit-entries`.
- Frontend (models): tipos, gateway y formateo de etiquetas/fechas.
- Frontend (controllers): `useTicketAuditTrailController`.
- Frontend (views): `TicketAuditTrailView`.

## Dificultad

**Nivel:** Medio

**Justificación:** consulta de solo lectura, pero cruza backend y frontend, aplica una regla de acceso sensible (cliente, administrador no asociado, retirado) y transforma valores anterior/nuevo heterogéneos en texto legible.

## Contrato backend ↔ frontend

Propuesta, ratificar en T-01 (convenciones REST comunes: base `/api`, ProblemDetails RFC 9457).

`GET /api/tickets/{ticketId}/audit-entries?cursor={cursor}&limit=50`

Respuesta `200 OK`:

```json
{
  "items": [
    {
      "id": "0b9f6c1e-2a7d-4c31-9a55-1f2e3d4c5b6a",
      "occurredAt": "2026-10-07T15:00:00Z",
      "actor": { "userId": "5a1c…", "displayName": "Elizabeth" },
      "action": "ParticipantAdded",
      "object": { "type": "TicketParticipant", "id": "9e2d…" },
      "oldValue": null,
      "newValue": { "userId": "77b3…", "displayName": "Laura", "origin": "Assignment" },
      "result": "Succeeded"
    },
    {
      "id": "1c0a7d2f-…",
      "occurredAt": "2026-10-07T15:05:12Z",
      "actor": { "userId": "5a1c…", "displayName": "Elizabeth" },
      "action": "PriorityOverridden",
      "object": { "type": "Ticket", "id": "3f4e…" },
      "oldValue": { "priority": "Medium" },
      "newValue": { "priority": "High", "calculatedPriority": "Medium", "reason": "Afecta la facturación del cierre de mes" },
      "result": "Succeeded"
    }
  ],
  "nextCursor": "MjAyNi0xMC0wN1QxNTowNToxMlp8MWMwYTdkMmY"
}
```

- `nextCursor` es opaco (propuesta: base64 de `occurredAt|id`); `null` cuando no hay más.
- `limit`: 1–100; por defecto 50.

| Código | Cuándo |
|---|---|
| 400 | `limit` fuera de 1–100 o `cursor` mal formado |
| 401 | Sin sesión |
| 403 | Rol cliente (`Requester`, `CompanyCoordinator`) |
| 404 | Ticket inexistente o no visible para la persona interna: no participante, retirado o administrador no asociado (propuesta: 404 para no revelar existencia) |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: fijar ruta, DTO, forma del cursor, códigos y la regla de acceso propuesta; llevar la regla a la persona usuaria como duda nueva antes de implementar. Registrar `GetTicketAuditTrail` e `IAuditTrailQueries` en [[glosario]] y [[backend-hexagonal]].
- [ ] **T-02 — Application: `GetTicketAuditTrail`** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: pruebas unitarias primero con dobles de `ICurrentUser` e `IAuditTrailQueries`: `GetAuditTrail_AsProductManager_ReturnsEntries`, `GetAuditTrail_AsActiveParticipant_ReturnsEntries`, `GetAuditTrail_AsRemovedParticipant_ReturnsNotFound`, `GetAuditTrail_AsAdministratorNotParticipant_ReturnsNotFound`, `GetAuditTrail_AsClient_ReturnsForbidden`. Implementar el caso de uso reutilizando la regla de acceso al ticket de HU-020.
- [ ] **T-03 — Infrastructure: consulta EF Core** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: pruebas de integración primero (PostgreSQL real): `AuditTrail_IsOrderedByOccurredAtThenId`, `AuditTrail_PaginatesWithCursorWithoutGapsOrDuplicates`, `AuditTrail_ResolvesDisplayNames`. Implementar `IAuditTrailQueries` con `AsNoTracking`, filtro por `ticket_id` y keyset pagination.
- [ ] **T-04 — Api: endpoint** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: pruebas de integración primero con `WebApplicationFactory<Program>`: `GetAuditEntries_AsRequester_Returns403`, `GetAuditEntries_AsNonParticipantDeveloper_Returns404`, `GetAuditEntries_LimitOver100_Returns400`. Mapear el endpoint con política de usuario interno y ProblemDetails.
- [ ] **T-05 — Frontend models** · Capa: Frontend (models) · Dificultad: Medio  
  Descripción: pruebas Vitest primero: `toAuditEntry` normaliza el JSON (acción desconocida → etiqueta genérica), `describeAuditEntry` produce texto en español por acción (p. ej. "Elizabeth cambió la prioridad de Media a Alta. Motivo: …"), `formatBogotaDateTime` presenta la hora en `America/Bogota`. Implementar `auditTrail.ts` y `auditTrailGateway.ts` sobre `core/http` (módulo propuesto `src/modules/audit`).
- [ ] **T-06 — Frontend controller** · Capa: Frontend (controllers) · Dificultad: Bajo  
  Descripción: `useTicketAuditTrailController(ticketId)` con estado discriminado `loading/ready/error/empty`, `loadMore()` por cursor y cancelación con `AbortController`. Prueba Vitest de la lógica de paginación (función pura de acumulación sin duplicados).
- [ ] **T-07 — Frontend view** · Capa: Frontend (views) · Dificultad: Bajo  
  Descripción: `TicketAuditTrailView` puro: lista cronológica, "Cargar más", `role="alert"` ante error con botón Reintentar, mensaje de vacío. Integrarla en el detalle interno de HU-020. `npm --prefix frontend run lint` sin violaciones MVC.

## Criterios de aceptación

### CHU-01 — Flujo feliz cronológico

**Dado** un ticket con tres entradas: `TicketSubmitted` (15:00:00Z), `ParticipantAdded` (15:02:00Z) y `PriorityOverridden` (15:05:12Z)  
**Cuando** Elizabeth (PM) solicita `GET /api/tickets/{ticketId}/audit-entries`  
**Entonces** recibe `200` con las tres entradas en ese orden, cada una con `occurredAt`, `actor.displayName`, `action`, `object`, `oldValue`, `newValue` y `result`, y la vista las muestra en hora `America/Bogota` (10:00, 10:02 y 10:05).

### CHU-02 — Paginación por cursor sin huecos

**Dado** un ticket con 120 entradas  
**Cuando** se solicitan páginas con `limit=50` siguiendo `nextCursor`  
**Entonces** se obtienen 50 + 50 + 20 entradas sin duplicados ni omisiones y la última respuesta trae `nextCursor: null`; `limit=0` y `limit=101` responden `400`.

### CHU-03 — El cliente nunca accede

**Dado** un solicitante y un coordinador de la empresa A, dueña del ticket  
**Cuando** cualquiera de ellos llama a `GET /api/tickets/{ticketId}/audit-entries`  
**Entonces** recibe `403` sin cuerpo de entradas, y ninguna respuesta del portal del cliente incluye datos de la bitácora.

### CHU-04 — Internos no autorizados

**Dado** un desarrollador que no es participante, un desarrollador retirado del ticket y un administrador no asociado  
**Cuando** cada uno solicita la bitácora  
**Entonces** cada uno recibe `404` (propuesta) y la vista muestra "No tienes acceso a este ticket" sin revelar entradas.

### CHU-05 — Participante vigente y administrador asociado

**Dado** un desarrollador participante vigente y un administrador que tomó el ticket en cobertura ([[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]])  
**Cuando** solicitan la bitácora  
**Entonces** ambos reciben `200` con todas las entradas del ticket, incluidas las anteriores a su incorporación.

### CHU-06 — Valores anterior/nuevo legibles

**Dado** entradas `StatusChanged` (`New` → `InDevelopment`), `TicketReassigned` (Laura → Brayan) y `ParticipantRemoved` (Kevin)  
**Cuando** la vista las presenta  
**Entonces** muestra "Estado: Nuevo → En desarrollo", "Reasignado de Laura a Brayan" y "Kevin fue retirado por Elizabeth" con fecha y hora, usando los nombres resueltos al consultar.

### CHU-07 — Solo lectura

**Dado** la ruta `/api/tickets/{ticketId}/audit-entries`  
**Cuando** se envía `POST`, `PUT`, `PATCH` o `DELETE`  
**Entonces** la respuesta es `405` y el número de filas de la tabla de auditoría no cambia.

### CHU-08 — Comportamiento de la UI ante error

**Dado** que el backend responde `500` o hay un fallo de red al cargar la bitácora  
**Cuando** la vista termina de cargar  
**Entonces** se muestra un mensaje con `role="alert"` y un botón "Reintentar" que vuelve a solicitar la misma página; las entradas ya cargadas no se pierden al fallar "Cargar más".

## Definition of Done

- [ ] CHU-01 a CHU-08 validados con evidencia.
- [ ] La regla de acceso propuesta fue ratificada por la persona usuaria (o ajustada) antes de cerrar la HU.
- [ ] Pruebas backend escritas primero y en verde (`cd backend && dotnet test`): `GetAuditTrail_AsRemovedParticipant_ReturnsNotFound`, `GetAuditTrail_AsAdministratorNotParticipant_ReturnsNotFound`, `GetAuditTrail_AsClient_ReturnsForbidden`, `AuditTrail_PaginatesWithCursorWithoutGapsOrDuplicates`, `GetAuditEntries_AsRequester_Returns403`.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Pruebas Vitest en verde (`npm --prefix frontend test`): `toAuditEntry`, `describeAuditEntry`, `formatBogotaDateTime`, acumulación por cursor.
- [ ] `npm --prefix frontend run lint` sin errores (fronteras MVC) y `npm --prefix frontend run build` correcto.
- [ ] Contrato documentado en OpenAPI (`/openapi/v1.json`) coincide con los tipos del modelo frontend.
- [ ] `docker compose up --build` levanta el stack y la pestaña Bitácora se ve en el detalle interno con datos sintéticos.
- [ ] Wiki: [[auditoria]] (quién consulta y cómo), [[roles-y-permisos]] (regla de acceso), [[frontend-mvc]] (módulo `audit` si se crea) actualizadas mediante Notas para la wiki; nueva duda registrada en [[pendientes]].
- [ ] `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos.
- [ ] PR revisado y aprobado por otra persona del equipo.
- [ ] Trazabilidad de la HU y de [[ep-004-auditoria-append-only]] actualizada.

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
| DoD-01 Regla de acceso ratificada | Pendiente | — | — |
| DoD-02 Pruebas backend | Pendiente | — | — |
| DoD-03 ArchitectureTests | Pendiente | — | — |
| DoD-04 Pruebas Vitest | Pendiente | — | — |
| DoD-05 Lint y build frontend | Pendiente | — | — |
| DoD-06 Contrato OpenAPI | Pendiente | — | — |
| DoD-07 `docker compose up --build` | Pendiente | — | — |
| DoD-08 Wiki actualizada | Pendiente | — | — |
| DoD-09 quality-reviewer | Pendiente | — | — |
| DoD-10 PR revisado | Pendiente | — | — |
| DoD-11 Trazabilidad | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

> [!question] Pendiente
> El PRD no define quién puede consultar la bitácora. La regla de esta HU (PM, participantes vigentes internos, administrador asociado) es propuesta y debe ratificarse; registrar como duda nueva en [[pendientes]].

- (propuesta) 404 en lugar de 403 para internos sin acceso, coherente con la convención de no revelar existencia; 403 solo para roles cliente, que nunca pueden acceder a rutas internas.
- Sprint 4 porque necesita el detalle interno (HU-020) y la participación (HU-018/HU-019) para tener contenido útil y reglas de acceso verificables.

## Relacionado

- [[ep-004-auditoria-append-only]] · [[tablero-scrum]] · [[auditoria]] · [[roles-y-permisos]] · [[frontend-mvc]]
- [[backend-hexagonal]] · [[estados-del-ticket]] · [[criterios-de-aceptacion]] · [[pendientes]]
