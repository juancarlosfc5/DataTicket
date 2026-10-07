---
title: "HU-021 — Cambiar estado interno"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/estados, producto/auditoria, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §6.3", "PRD.md §6.4", "PRD.md §6.5", "PRD.md §11", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-021", "Cambiar estado interno"]
epica: "[[ep-007-trabajo-interno-y-estados]]"
criterios_prd: [CA-10, CA-12, CA-14]
componentes: [Backend Domain, Backend Application, Backend Infrastructure, Backend Api, Frontend models, Frontend controllers, Frontend views, Persistencia PostgreSQL]
dificultad: "Medio"
sprint_sugerido: "Sprint 4"
dependencias: ["[[hu-011-registrar-eventos-auditables]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-020-detalle-interno-del-ticket]]"]
relacionadas: ["[[hu-022-registrar-url-de-pr]]", "[[hu-023-bandeja-de-mis-tickets-y-equipo]]", "[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-033-emitir-respuesta-formal]]", "[[hu-036-cerrar-ticket-manualmente]]", "[[hu-012-consultar-bitacora-del-ticket]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-021 — Cambiar estado interno

Permite mover un ticket por los estados internos de trabajo (`New → InDevelopment`, `New → InProduction`, `InDevelopment → PullRequestReview`, `PullRequestReview → InProduction`) con auditoría de valor anterior y nuevo; cualquier otra transición responde 409 y el cliente solo percibe el `ClientStatus` mapeado (PRD §6.4, §12).

## Historia de usuario

**COMO** participante vigente de un ticket (Desarrollo o Producción), Elizabeth (PM) o administrador asociado  
**QUIERO** registrar en qué etapa interna está el trabajo del ticket  
**PARA** que el equipo y la PM conozcan el avance real y quede trazado en la bitácora sin revelar la etapa técnica al cliente

## Contexto

El PRD define los estados y su significado, pero no una máquina de transiciones (V-01). [[estados-del-ticket]] infiere un diagrama a partir del flujo narrado en (PRD §6.2–§6.5). Esta HU implementa solo las transiciones de trabajo interno de ese diagrama; `SolutionDelivered` y `Closed` se alcanzan exclusivamente con la respuesta formal ([[hu-033-emitir-respuesta-formal|HU-033]]) y el cierre manual ([[hu-036-cerrar-ticket-manualmente|HU-036]]). El glosario no tiene caso de uso para cambiar estado (hallazgo 7): se propone `ChangeTicketStatus` (nombre propuesto, ratificar en T-01 y registrar en el glosario).

## Alcance

- Máquina de transiciones del dominio con las cuatro transiciones de trabajo interno.
- Caso de uso con autorización por rol/participación y auditoría.
- Concurrencia optimista: el cliente envía el estado que espera encontrar.
- Lista de transiciones permitidas para el usuario en el detalle interno (`allowedTransitions`).
- Selector de estado en la vista de detalle interno.

## Fuera de alcance

- Transiciones a `SolutionDelivered` (HU-033) y a `Closed` (HU-036).
- Retrocesos (p. ej. PR rechazado `PullRequestReview → InDevelopment`), reapertura de `Closed` y `PullRequestReview → SolutionDelivered` mientras V-01 siga abierta (hallazgo 6).
- Correo al cliente por cambio de estado: el estado técnico no se revela por correo y el correo por transición resumida no está garantizado (PRD §11).
- Notificación en la app por cambio de estado (EP-011 cubre mensajes del chat; el cambio solo "se refleja en las bandejas internas", PRD §11).
- Exigir URL de PR para pasar a `PullRequestReview` (ver Notas).

## Requisitos y reglas de negocio

- Estados internos y correspondencia con el cliente: `New → Received`; `InDevelopment`, `PullRequestReview`, `InProduction → InProgress`; `SolutionDelivered → SolutionDelivered`; `Closed → Closed` (PRD §6.4).
- El cliente no ve si el trabajo está en Desarrollo, revisión o Producción (PRD §6.4, §14.10).
- Ruta directa: tickets de Producción pueden asignarse a Julián sin pasar por Desarrollo (PRD §6.2 punto 2, §6.3) → transición `New → InProduction` (inferida, V-02).
- Solo Elizabeth o un administrador lleva a `SolutionDelivered` y `Closed` (PRD §5 regla 7, §6.5).
- Cambios de estado se auditan con actor, fecha/hora, valor anterior y nuevo y resultado (PRD §12, §14.14).
- El cambio de estado interno se refleja en las bandejas internas (PRD §11).
- (propuesta) Pueden mover estado: participantes vigentes, `ProductManager` y `Administrator` asociado como participante. Pendiente de V-01 ("quién mueve cada estado").
- (propuesta) Transición no listada, transición al mismo estado o desde `Closed` → 409.

## Invariantes en juego

- AGENTS §7.2 — el cliente nunca ve estados técnicos: solo el `ClientStatus` derivado.
- AGENTS §7.5 — un administrador no asociado no puede operar sobre el ticket.
- AGENTS §7.6 — no hay ruta alternativa a `SolutionDelivered` ni a `Closed` fuera de HU-033/HU-036.
- AGENTS §7.8 — auditoría append-only con valor anterior/nuevo.

## Criterios del PRD cubiertos

- PRD CA-14 (parcial: cambios de estado con actor y fecha/hora) → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial: el estado técnico se colapsa en `InProgress`) → [[criterios-de-aceptacion]]
- PRD CA-12 (parcial: este endpoint no permite cerrar; refuerza que el cierre es solo manual vía HU-036) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-007-trabajo-interno-y-estados]]
- Dependencias: [[hu-011-registrar-eventos-auditables|HU-011]] (`IAuditLog`), [[hu-018-asignar-y-agregar-participantes|HU-018]] (participantes), [[hu-020-detalle-interno-del-ticket|HU-020]] (regla de acceso y detalle donde se muestra el selector).
- Relacionadas: [[hu-022-registrar-url-de-pr|HU-022]], [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]] (filtro por estado), [[hu-024-portal-solicitante-consulta-tickets|HU-024]] (mapeo visible), [[hu-033-emitir-respuesta-formal|HU-033]], [[hu-036-cerrar-ticket-manualmente|HU-036]], [[hu-012-consultar-bitacora-del-ticket|HU-012]].
- Decisiones: V-01, V-02 y hallazgo 6 en [[pendientes]] (dependencia de producto); hallazgo 7 (nombre del caso de uso).

## Componentes afectados

- Backend Domain: `TicketStatus`, `ClientStatus`, reglas de transición en `Ticket`.
- Backend Application: `ChangeTicketStatus` (nombre propuesto), `ITicketRepository`, `IAuditLog`, `ICurrentUser`, `IClock`.
- Backend Infrastructure: persistencia del estado con token de concurrencia.
- Backend Api: `POST /api/tickets/{ticketId}/status`; ampliación del DTO de HU-020 con `allowedTransitions`.
- Frontend models/controllers/views: selector de estado en el detalle interno.
- Persistencia PostgreSQL: token de concurrencia (ver T-06).

## Dificultad

**Nivel:** Medio

**Justificación:** la máquina de estados es pequeña y vive en un agregado, pero combina autorización por participación, concurrencia optimista, auditoría y una dependencia de producto abierta (V-01) que puede ampliar las transiciones.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01.

**`POST /api/tickets/{ticketId}/status`**

Request:

```json
{ "expectedStatus": "InDevelopment", "targetStatus": "PullRequestReview" }
```

Respuesta `200 OK`:

```json
{
  "ticketId": "6f1c2b9e-0d4a-4c1e-9a51-3c2f8e7b1a20",
  "previousStatus": "InDevelopment",
  "status": "PullRequestReview",
  "clientStatus": "InProgress",
  "changedAt": "2026-10-07T16:45:00Z",
  "allowedTransitions": ["InProduction"]
}
```

Ampliación de `GET /api/tickets/{ticketId}` ([[hu-020-detalle-interno-del-ticket|HU-020]]): campo `"allowedTransitions": ["PullRequestReview"]`, calculado por el backend para el usuario actual (lista vacía si no puede mover estados).

Errores (ProblemDetails):

| Código | Cuándo | `type` (propuesta) |
|---|---|---|
| 400 | `targetStatus` o `expectedStatus` ausente o fuera de `TicketStatus` | `validation` |
| 401 | Sin sesión | — |
| 403 | Usuario cliente | — |
| 404 | Ticket inexistente o no visible para el usuario (interno no asociado, admin no asociado, retirado) | — |
| 409 | Transición no permitida (incluye `SolutionDelivered`, `Closed`, mismo estado, desde `Closed`) | `/problems/invalid-status-transition` |
| 409 | `expectedStatus` no coincide con el estado actual (otro usuario lo cambió) | `/problems/status-conflict` |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar ruta, DTO, `type` de los ProblemDetails, nombre `ChangeTicketStatus` y la propuesta de quién mueve estados; registrar nombres en el [[glosario]] y llevar V-01/V-02 a la persona responsable del producto.
- [ ] **T-02 — Máquina de transiciones** · Capa: Backend Domain · Dificultad: Medio  
  Descripción: primero una prueba parametrizada xUnit con las 36 combinaciones `TicketStatus × TicketStatus` (4 aceptadas, 32 rechazadas) y una prueba del mapeo `ClientStatus` para los 6 estados; luego el método de cambio de estado en `Ticket` que devuelve el estado anterior y rechaza lo no permitido.
- [ ] **T-03 — Caso de uso** · Capa: Backend Application · Dificultad: Medio  
  Descripción: primero pruebas unitarias con dobles (`ITicketRepository`, `IAuditLog`, `ICurrentUser`, `IClock`): participante vigente cambia; PM no participante cambia; admin asociado cambia; admin no asociado → no encontrado; interno no asociado → no encontrado; `expectedStatus` desfasado → conflicto; transición inválida → conflicto sin `AuditEntry` de éxito; el `AuditEntry` lleva actor, `IClock.UtcNow`, valor anterior y nuevo; no se invoca `IEmailSender`. Luego `ChangeTicketStatus`.
- [ ] **T-04 — Transiciones permitidas para el usuario** · Capa: Backend Application · Dificultad: Bajo  
  Descripción: prueba y cálculo de `allowedTransitions` para el detalle de HU-020 (vacío para quien no puede mover estados).
- [ ] **T-05 — Endpoint** · Capa: Backend Api · Dificultad: Bajo  
  Descripción: `POST /api/tickets/{ticketId}/status`; mapeo de resultados a 200/400/404/409; pruebas de integración con `WebApplicationFactory<Program>` y PostgreSQL real.
- [ ] **T-06 — Concurrencia y migración** · Capa: Backend Infrastructure · Dificultad: Bajo  
  Descripción: configurar token de concurrencia en `Ticket` (propuesta: columna de sistema `xmin` de PostgreSQL con Npgsql); migración EF Core si el modelo cambia; prueba de integración con dos cambios simultáneos donde uno recibe 409.
- [ ] **T-07 — Modelo y gateway** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: tipo `TicketStatus`, etiquetas en español ("Nuevo", "En desarrollo", "Revisión de PR", "En producción"…), `changeTicketStatus()` en el gateway; pruebas Vitest de etiquetas y de la normalización de la respuesta.
- [ ] **T-08 — Controlador** · Capa: Frontend controllers · Dificultad: Bajo  
  Descripción: `useTicketStatusController` con acción `changeStatus(target)`, estado `idle | saving | conflict | error` y recarga del detalle tras 409; pruebas Vitest.
- [ ] **T-09 — Vista** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `TicketStatusSelectorView` que solo ofrece `allowedTransitions`; sin opciones si la lista está vacía.

## Criterios de aceptación

### CHU-01 — Transiciones de trabajo permitidas

**Dado** un participante vigente y un ticket en `New`, `InDevelopment` o `PullRequestReview`  
**Cuando** solicita respectivamente `New → InDevelopment`, `New → InProduction`, `InDevelopment → PullRequestReview` o `PullRequestReview → InProduction` con el `expectedStatus` correcto  
**Entonces** cada petición responde 200 con `previousStatus` y `status` correctos, y el detalle refleja el nuevo estado.

### CHU-02 — Transiciones no listadas responden 409

**Dado** un ticket en `InDevelopment`  
**Cuando** se pide `targetStatus = "New"`, `"InDevelopment"` (mismo estado), `"SolutionDelivered"` o `"Closed"`; **o** cualquier transición desde un ticket en `Closed`; **o** `PullRequestReview → SolutionDelivered`  
**Entonces** responde 409 con `type = /problems/invalid-status-transition`, el estado no cambia y la prueba de dominio confirma 32 combinaciones rechazadas de 36.

### CHU-03 — Validación del cuerpo

**Dado** un participante vigente  
**Cuando** envía `targetStatus = "Deployed"` (valor fuera de `TicketStatus`), o omite `expectedStatus`  
**Entonces** responde 400 ProblemDetails con el campo inválido y el estado no cambia.

### CHU-04 — Autorización en backend por rol y participación

**Dado** un ticket con Laura como participante vigente  
**Cuando** pide el cambio un integrante de `Team.Development` no asociado, un administrador no asociado o un participante retirado  
**Entonces** recibe 404; **cuando** lo pide el solicitante o el coordinador de la empresa del ticket recibe 403; **y** la PM no participante recibe 200 (propuesta pendiente de V-01).

### CHU-05 — Conflicto de concurrencia

**Dado** un ticket en `InDevelopment` que otro usuario acaba de mover a `PullRequestReview`  
**Cuando** Laura envía `expectedStatus = "InDevelopment"`, `targetStatus = "PullRequestReview"`  
**Entonces** responde 409 con `type = /problems/status-conflict` y no se crea un segundo `AuditEntry`.

### CHU-06 — Auditoría con valor anterior y nuevo

**Dado** un cambio exitoso `InDevelopment → PullRequestReview` hecho por Laura  
**Cuando** se consulta la bitácora del ticket  
**Entonces** existe exactamente un `AuditEntry` con actor Laura, fecha/hora UTC de `IClock`, acción de cambio de estado, objeto el ticket, `OldValue = "InDevelopment"`, `NewValue = "PullRequestReview"` y resultado exitoso.

### CHU-07 — El cliente solo ve el estado resumido

**Dado** tres tickets de la empresa A en `InDevelopment`, `PullRequestReview` e `InProduction`  
**Cuando** el solicitante los consulta en el portal ([[hu-024-portal-solicitante-consulta-tickets|HU-024]])  
**Entonces** los tres muestran `clientStatus = "InProgress"` y el JSON del portal no contiene las cadenas `InDevelopment`, `PullRequestReview` ni `InProduction`; **y** no se envía ningún correo al cliente (Mailpit sin mensajes nuevos).

### CHU-08 — La UI solo ofrece transiciones permitidas y maneja el conflicto

**Dado** el detalle de un ticket en `InDevelopment` con `allowedTransitions = ["PullRequestReview"]`  
**Cuando** el usuario abre el selector  
**Entonces** solo ve "Revisión de PR"; **si** el backend responde 409 la vista muestra "El estado cambió; se recargó el ticket" y presenta el estado actual; **y** con `allowedTransitions = []` el selector no se muestra.

## Definition of Done

- [ ] CHU-01 a CHU-08 validados con evidencia.
- [ ] Prueba parametrizada `Ticket_CambiarEstado_SoloAceptaCuatroTransicionesDeTrabajo` (36 casos) y `Ticket_ClientStatus_ColapsaTresEstadosEnInProgress` en verde (`cd backend && dotnet test`).
- [ ] Pruebas unitarias del caso de uso: `ChangeTicketStatus_AdminNoAsociado_DevuelveNoEncontrado`, `ChangeTicketStatus_TransicionInvalida_NoAuditaExito`, `ChangeTicketStatus_Exito_AuditaValorAnteriorYNuevo`, `ChangeTicketStatus_NoEnviaCorreo`.
- [ ] Pruebas de integración de 200/400/403/404/409 y de concurrencia contra PostgreSQL real.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Migración EF Core aplicable (si se añadió token de concurrencia) y revisada.
- [ ] Vitest, `npm --prefix frontend run lint` y `npm --prefix frontend run build` en verde.
- [ ] El stack levanta con `docker compose up --build` y el selector funciona con datos sintéticos.
- [ ] Wiki actualizada: [[estados-del-ticket]] (transiciones implementadas y quién las mueve), [[auditoria]] (acción de cambio de estado), [[glosario]] (`ChangeTicketStatus` si se ratifica), [[backend-hexagonal]]; registrar en [[pendientes]] el estado de V-01/V-02.
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
| DoD-01 Prueba de dominio de 36 combinaciones y mapeo | Pendiente | — | — |
| DoD-02 Pruebas unitarias del caso de uso | Pendiente | — | — |
| DoD-03 Pruebas de integración y concurrencia | Pendiente | — | — |
| DoD-04 Pruebas de arquitectura | Pendiente | — | — |
| DoD-05 Migración EF Core | Pendiente | — | — |
| DoD-06 Vitest, lint y build | Pendiente | — | — |
| DoD-07 `docker compose up --build` | Pendiente | — | — |
| DoD-08 Wiki actualizada | Pendiente | — | — |
| DoD-09 `quality-reviewer` | Pendiente | — | — |
| DoD-10 PR revisado | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- **Dependencia V-01 / hallazgo 6:** el diagrama no tiene `PullRequestReview → SolutionDelivered`; si un ticket en revisión de PR puede recibir la respuesta formal sin pasar por Producción, debe decidirse antes de HU-033. Retrocesos (PR rechazado) y reapertura tampoco están definidos: hoy responden 409.
- **Dependencia V-02:** la ruta directa se modela como `New → InProduction` explícito; queda por confirmar si la asignación a Julián lo dispara automáticamente (no en esta HU).
- (propuesta) No se exige `PullRequestUrl` para pasar a `PullRequestReview`: el diagrama rotula la flecha "URL de PR registrada", pero el PRD no lo exige. Ratificar en V-01.
- (propuesta) ¿Se auditan los intentos rechazados (409/404)? Depende de V-11; por defecto, no.

## Relacionado

- [[ep-007-trabajo-interno-y-estados]] · [[tablero-scrum]] · [[estados-del-ticket]] · [[flujo-del-ticket]] · [[auditoria]]
- [[notificaciones]] · [[roles-y-permisos]] · [[modelo-de-dominio]] · [[backend-hexagonal]] · [[criterios-de-aceptacion]] · [[pendientes]]
