---
title: "HU-019 — Reasignar y retirar participantes"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/ticket, producto/roles, producto/seguridad, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §6.3", "PRD.md §7", "PRD.md §11", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-019", "Reasignar y retirar participantes", "RemoveParticipant"]
epica: "[[ep-006-triage-asignacion-y-participantes]]"
criterios_prd: ["PRD CA-04", "PRD CA-14", "PRD CA-08", "PRD CA-06"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 3"
dependencias: ["[[hu-018-asignar-y-agregar-participantes]]", "[[hu-011-registrar-eventos-auditables]]", "[[hu-020-detalle-interno-del-ticket]]"]
relacionadas: ["[[hu-032-revocar-acceso-al-retirar-participante]]", "[[hu-026-historial-del-chat-por-cursor]]", "[[hu-027-unirse-al-chat-del-ticket]]", "[[hu-037-notificaciones-en-la-app]]", "[[hu-038-correo-de-vinculacion]]", "[[hu-012-consultar-bitacora-del-ticket]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-019 — Reasignar y retirar participantes

Elizabeth (PM) o un administrador retira participantes —sin borrar su registro: se marcan `RemovedAt`/`RemovedBy`— y reasigna la responsabilidad de un ticket a otra persona, con auditoría de valores anterior y nuevo; el retiro revoca el acceso futuro al detalle y Elizabeth nunca pierde el acceso (PRD §5.3, §5.4, §6.2.4, CA-04).

## Historia de usuario

**COMO** Elizabeth (PM)  
**QUIERO** reasignar un ticket a otra persona y retirar participantes que ya no deben intervenir  
**PARA** mantener el ticket en manos de quien corresponde y limitar el acceso a quienes siguen trabajando en él, sin perder la trazabilidad de quién participó

## Contexto

La PM y los administradores pueden retirar participantes; retirarlo revoca su acceso futuro y se conserva su participación pasada en el historial y la auditoría (PRD §5.4, §12). Elizabeth reasigna (PRD §4 objetivo 2, §5) y la reasignación es un evento auditable (PRD §12). Una persona retirada deja de leer, enviar, marcar lectura o reconectarse al chat; sus conexiones activas deben desconectarse o quedar sin autorización (PRD §7.5) — eso lo cubre [[hu-032-revocar-acceso-al-retirar-participante|HU-032]]. El desarrollador que hizo el cambio puede seguir asociado como colaborador cuando el ticket pasa a Producción (PRD §6.3). No hay caso de uso de reasignación en el mapa de puertos (hallazgo 7): se propone `ReassignTicket` (nombre propuesto, ratificar en T-01 y registrar en el glosario). `RemoveParticipant` ya figura en [[backend-hexagonal]].

## Alcance

- Retiro de participante (`RemoveParticipant`): marca `RemovedAt`/`RemovedBy`; si era responsable, cierra su `Assignment` (`EndedAt`/`EndedBy`, campos propuestos).
- Reasignación (`ReassignTicket`): cierra la asignación del responsable anterior y asigna al nuevo, con opción explícita de conservar al anterior como participante colaborador.
- Regla: el rol `ProductManager` nunca pierde el acceso; su participación no se retira.
- Revocación del acceso futuro al detalle y a la bitácora (regla de acceso de HU-018).
- Listado de participantes con históricos (`includeRemoved`).
- Auditoría `ParticipantRemoved` y `TicketReassigned`.
- Acciones "Retirar" y "Reasignar" con confirmación en el panel de participantes.
- Migración EF Core para los campos de cierre de `Assignment`.

## Fuera de alcance

- Desconexión de conexiones activas del hub y bloqueo de operaciones del chat en curso: [[hu-032-revocar-acceso-al-retirar-participante|HU-032]].
- Rechazo de `JoinTicket` e historial del chat para el retirado: [[hu-027-unirse-al-chat-del-ticket|HU-027]] y [[hu-026-historial-del-chat-por-cursor|HU-026]] (aplican la misma regla).
- Supresión de notificaciones futuras al retirado: [[hu-037-notificaciones-en-la-app|HU-037]] (PRD §11).
- Correo al reincorporado (V-08, [[hu-038-correo-de-vinculacion|HU-038]]).
- Motivo obligatorio del retiro o de la reasignación (el PRD no lo pide).

## Requisitos y reglas de negocio

- La PM y los administradores agregan o retiran participantes; cada cambio queda auditado (PRD §5.4, §6.2.4, §12).
- Retirar revoca el acceso futuro; se conserva la participación pasada en historial y auditoría (PRD §5.4, §12).
- Una persona retirada deja de leer, enviar, marcar lectura o reconectarse al chat (PRD §7.5).
- Elizabeth siempre puede acceder para triage y seguimiento (PRD §5.3).
- La reasignación se audita (PRD §12, §14.4).
- El desarrollador puede permanecer asociado como colaborador (PRD §6.3).
- (propuesta) Reasignar = cerrar el `Assignment` del responsable anterior + crear el del nuevo (y su participación si no la tiene); conservar o retirar al anterior como participante es un campo obligatorio de la petición.
- (propuesta) Retirar al rol `ProductManager` responde `409`; reasignar desde la PM cierra su `Assignment` pero nunca su acceso.
- (propuesta) Reincorporar a un retirado crea una participación nueva; la anterior se conserva.
- (propuesta, V-01) No se retira ni reasigna en tickets `Closed`.

## Invariantes en juego

- Invariante 3: solo participantes vigentes acceden; Elizabeth siempre.
- Invariante 8: retiro y reasignación auditados con anterior/nuevo; la participación histórica no se borra.
- Invariante 2: el cliente no ve reasignaciones ni participantes.

## Criterios del PRD cubiertos

- PRD CA-04 (parcial: reasignar y quitar participantes, auditado; asignar y agregar en HU-018) → [[criterios-de-aceptacion]]
- PRD CA-14 (parcial: participantes y asignación) → [[criterios-de-aceptacion]]
- PRD CA-08 (parcial: "una persona retirada no puede volver a consultarlo", a nivel de regla de acceso; el chat en HU-026/HU-032) → [[criterios-de-aceptacion]]
- PRD CA-06 (parcial: revocación en accesos futuros; conexiones existentes en HU-032) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-006-triage-asignacion-y-participantes|EP-006 — Triage, asignación y participantes]]
- Dependencias: [[hu-018-asignar-y-agregar-participantes|HU-018 — Asignar y agregar participantes]] · [[hu-011-registrar-eventos-auditables|HU-011 — Registrar eventos auditables]] · [[hu-020-detalle-interno-del-ticket|HU-020 — Detalle interno del ticket]]
- Relacionadas: [[hu-032-revocar-acceso-al-retirar-participante|HU-032]] · [[hu-026-historial-del-chat-por-cursor|HU-026]] · [[hu-027-unirse-al-chat-del-ticket|HU-027]] · [[hu-037-notificaciones-en-la-app|HU-037]] · [[hu-038-correo-de-vinculacion|HU-038]] · [[hu-012-consultar-bitacora-del-ticket|HU-012]]
- Decisiones: V-10 · V-08 · V-01 ([[pendientes]]) · hallazgo 7

## Componentes afectados

- Backend (Domain): `Ticket.RemoveParticipant(...)`, `Ticket.Reassign(...)`, cierre de `Assignment`.
- Backend (Application): `RemoveParticipant`, `ReassignTicket` (propuesto), `ITicketRepository`, `IAuditLog`, `ICurrentUser`, `IClock`; punto de extensión hacia `IChatConnectionRevoker` (implementado en HU-032).
- Backend (Infrastructure): configuración EF Core y migración.
- Backend (Api): `DELETE` de participante, `POST` de reasignación, `GET` con `includeRemoved`.
- Frontend: acciones en `ParticipantsPanelView` (módulo `triage`).

## Dificultad

**Nivel:** Medio

**Justificación:** reutiliza la estructura de HU-018, pero añade reglas de estado (vigente/retirado, asignación cerrada), la excepción de la PM, varias entradas de auditoría atómicas por operación y la coordinación con la revocación del chat de HU-032.

## Contrato backend ↔ frontend

Propuesta, ratificar en T-01.

**`DELETE /api/tickets/{ticketId}/participants/{userId}`** — `ProductManager`, `Administrator`. Sin cuerpo. `204 No Content`.

**`POST /api/tickets/{ticketId}/reassignments`** — `ProductManager`, `Administrator`.

```json
{ "fromUserId": "77b3…", "toUserId": "b5r4…", "keepPreviousAsParticipant": false }
```

`200 OK` con la lista de participantes vigente (forma de HU-018).

**`GET /api/tickets/{ticketId}/participants?includeRemoved=true`** — amplía la forma de HU-018 con `removedAt` y `removedBy` (nulos si vigente):

```json
{
  "ticketId": "3f4e…",
  "participants": [
    {
      "userId": "k3v1…",
      "displayName": "Kevin",
      "isAssignee": false,
      "origin": "Participant",
      "addedAt": "2026-10-08T14:10:00Z",
      "addedBy": { "userId": "5a1c…", "displayName": "Elizabeth" },
      "removedAt": "2026-10-09T09:00:00Z",
      "removedBy": { "userId": "5a1c…", "displayName": "Elizabeth" }
    }
  ]
}
```

| Código | Cuándo |
|---|---|
| 400 | `fromUserId` igual a `toUserId`; `toUserId` no es persona interna activa; `fromUserId` no es responsable vigente; falta `keepPreviousAsParticipant` |
| 401 | Sin sesión |
| 403 | Rol cliente o interno distinto de PM/administrador |
| 404 | Ticket inexistente; la persona nunca fue participante |
| 409 | Participación ya retirada; destino del retiro con rol `ProductManager`; ticket `Closed` (propuesta) |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: fijar rutas, cuerpo de reasignación, regla de la PM, códigos y la forma con `includeRemoved`. Registrar `ReassignTicket` y los campos `EndedAt`/`EndedBy` de `Assignment` en [[glosario]], [[modelo-de-dominio]] y el mapa de [[backend-hexagonal]] (hallazgo 7).
- [ ] **T-02 — Domain: retiro y reasignación** · Capa: Backend (Domain) · Dificultad: Medio  
  Descripción: pruebas primero: `RemoveParticipant_MarksRemovedAtAndRemovedBy_DoesNotDelete`, `RemoveParticipant_Assignee_EndsAssignment`, `RemoveParticipant_AlreadyRemoved_Throws`, `RemoveParticipant_ProductManager_Throws`, `CanAccess_RemovedParticipant_IsFalse`, `Reassign_EndsPreviousAndCreatesNewAssignment`, `Reassign_KeepPrevious_LeavesPreviousAsParticipant`, `Reassign_FromProductManager_KeepsHerAccess`, `ReAdd_AfterRemoval_CreatesNewParticipation`.
- [ ] **T-03 — Application: `RemoveParticipant` y `ReassignTicket`** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: pruebas unitarias primero: `RemoveParticipant_AsAdministratorNotParticipant_Succeeds`, `RemoveParticipant_AsDeveloper_IsForbidden`, `RemoveParticipant_AppendsParticipantRemovedWithOldAndNew`, `ReassignTicket_AppendsReassignedRemovedAndAddedAtomically`, `RemoveParticipant_InvokesConnectionRevocationHook` (doble de `IChatConnectionRevoker`; la implementación real es de HU-032).
- [ ] **T-04 — Infrastructure: persistencia y migración** · Capa: Backend (Infrastructure) · Dificultad: Bajo  
  Descripción: pruebas de integración primero: `RemovedParticipation_RemainsInDatabase`, `ParticipantsQuery_IncludeRemoved_ReturnsHistory`. Migración `AddAssignmentEnd` (`ended_at`, `ended_by`).
- [ ] **T-05 — Api: endpoints** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: pruebas de integración primero: `DeleteParticipant_Returns204_AndRemovedUserGets404OnDetail`, `DeleteParticipant_ProductManager_Returns409`, `PostReassignment_LauraToBrayan_Returns200`, `DeleteParticipant_AsRequester_Returns403`, `DeleteParticipant_Twice_Returns409`.
- [ ] **T-06 — Frontend models** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: pruebas Vitest primero: `toParticipant` con `removedAt`, `validateReassignment` (mismo origen y destino, destino vacío), `describeParticipation` ("Retirado el … por …"). Ampliar `participants.ts` y su gateway.
- [ ] **T-07 — Frontend controller** · Capa: Frontend (controllers) · Dificultad: Bajo  
  Descripción: ampliar `useParticipantsController` con `remove(userId)`, `reassign(...)` y alternancia "mostrar retirados". Prueba Vitest del reducer.
- [ ] **T-08 — Frontend views** · Capa: Frontend (views) · Dificultad: Bajo  
  Descripción: `RemoveParticipantConfirmView` ("Kevin perderá el acceso al detalle y al chat de este ticket; su participación anterior se conserva"), `ReassignDialogView` (destino y casilla "Mantener a la persona anterior como colaboradora") y marca visual de retirados. Acción "Retirar" oculta para la PM. Lint MVC sin violaciones.

## Criterios de aceptación

### CHU-01 — Retiro sin borrado

**Dado** Kevin, participante vigente, y `IClock` fijo en `2026-10-09T09:00:00Z`  
**Cuando** Elizabeth envía `DELETE /api/tickets/{ticketId}/participants/{kevinId}`  
**Entonces** recibe `204`; la fila de participación de Kevin sigue existiendo con `removed_at` = `2026-10-09 09:00:00+00` y `removed_by` = Elizabeth; y existe una entrada `ParticipantRemoved` (actor Elizabeth, objeto `TicketParticipant`, `old_value` con `userId` y `addedAt`, `new_value` con `removedAt` y `removedBy`).

### CHU-02 — Revocación del acceso futuro

**Dado** Kevin recién retirado  
**Cuando** solicita el detalle interno (HU-020), la bitácora (HU-012) o la lista de participantes  
**Entonces** recibe `404` (propuesta) en todas, y la regla de acceso del dominio devuelve falso para Kevin; la revocación de conexiones activas del chat se verifica en HU-032.

### CHU-03 — Historia conservada y reincorporación

**Dado** Kevin retirado  
**Cuando** Elizabeth consulta `GET …/participants?includeRemoved=true` y después lo vuelve a agregar (HU-018)  
**Entonces** la primera consulta muestra a Kevin con `addedAt`, `addedBy`, `removedAt` y `removedBy`; tras reincorporarlo existen dos participaciones de Kevin (la retirada y una nueva vigente), recupera el acceso y la auditoría conserva ambas.

### CHU-04 — Reasignación con anterior y nuevo auditados

**Dado** Laura como responsable vigente  
**Cuando** Elizabeth envía `fromUserId` = Laura, `toUserId` = Brayan, `keepPreviousAsParticipant: false`  
**Entonces** recibe `200`; el `Assignment` de Laura queda cerrado (`ended_at`, `ended_by`) y su participación retirada; Brayan es responsable y participante vigente; y en una sola transacción se registran `TicketReassigned` (`old_value` `{"assigneeId":"<Laura>"}`, `new_value` `{"assigneeId":"<Brayan>"}`), `ParticipantRemoved` (Laura) y `ParticipantAdded` (Brayan), con actor Elizabeth y la misma fecha.

### CHU-05 — Reasignación conservando colaboradora

**Dado** Laura como responsable vigente  
**Cuando** se reasigna a Julián con `keepPreviousAsParticipant: true`  
**Entonces** Laura sigue como participante vigente con `isAssignee: false`, conserva el acceso al detalle y no se registra `ParticipantRemoved` para ella (PRD §6.3).

### CHU-06 — Elizabeth nunca pierde el acceso

**Dado** Elizabeth asignada como responsable de un ticket  
**Cuando** un administrador intenta `DELETE` de su participación y luego la reasigna a Julián con `keepPreviousAsParticipant: false`  
**Entonces** el `DELETE` responde `409` sin cambios; la reasignación responde `200`, cierra el `Assignment` de Elizabeth y ella sigue obteniendo `200` en el detalle interno.

### CHU-07 — Validaciones

**Dado** un ticket con Laura responsable y Kevin ya retirado  
**Cuando** se reasigna de Laura a Laura; de Brayan (no responsable) a Kevin; de Laura a un usuario cliente; se omite `keepPreviousAsParticipant`; se retira de nuevo a Kevin; y se retira a una persona que nunca participó  
**Entonces** las cuatro primeras responden `400`, el segundo retiro de Kevin `409` y la persona ajena `404`; ninguna crea cambios ni auditoría.

### CHU-08 — Autorización por rol en backend

**Dado** un administrador no participante, Laura (Desarrollo, participante), Julián (Producción, participante), un coordinador de la empresa dueña y una petición sin sesión  
**Cuando** cada uno intenta retirar a Kevin  
**Entonces** el administrador recibe `204`; Laura, Julián y el coordinador `403`; sin sesión `401`.

### CHU-09 — Sin exposición al cliente

**Dado** un ticket de la empresa A con una reasignación y un retiro registrados  
**Cuando** la solicitante de A consulta su ticket en el portal (contrato de HU-024) y llama a `GET …/participants?includeRemoved=true`  
**Entonces** la segunda responde `403` y la respuesta del portal no contiene nombres de participantes, reasignaciones ni retiros.

### CHU-10 — Comportamiento de la UI

**Dado** el panel de participantes abierto por Elizabeth  
**Cuando** pulsa "Retirar" sobre Kevin, confirma, y el backend responde `409` (ya retirado) o falla la red  
**Entonces** la confirmación explica la pérdida de acceso antes de enviar; con `204` Kevin pasa a la sección de retirados; con `409` se muestra "La participación ya estaba retirada" y se recarga la lista; con fallo de red un `role="alert"` con "Reintentar"; la PM no ve la acción "Retirar" sobre sí misma.

## Definition of Done

- [ ] CHU-01 a CHU-10 validados con evidencia.
- [ ] Pruebas backend escritas primero y en verde (`cd backend && dotnet test`): `RemoveParticipant_MarksRemovedAtAndRemovedBy_DoesNotDelete`, `CanAccess_RemovedParticipant_IsFalse`, `RemoveParticipant_ProductManager_Throws`, `ReassignTicket_AppendsReassignedRemovedAndAddedAtomically`, `DeleteParticipant_Returns204_AndRemovedUserGets404OnDetail`, `DeleteParticipant_AsRequester_Returns403`, `RemoveParticipant_InvokesConnectionRevocationHook`.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Migración EF Core `AddAssignmentEnd` creada, aplicada en Compose y reversible.
- [ ] Pruebas Vitest en verde (`npm --prefix frontend test`): `toParticipant` con retiro, `validateReassignment`, reducer.
- [ ] `npm --prefix frontend run lint` sin errores y `npm --prefix frontend run build` correcto.
- [ ] `docker compose up --build`: Elizabeth reasigna y retira con usuarios sintéticos; el retirado deja de ver el ticket.
- [ ] Wiki: [[flujo-del-ticket]] (reasignación), [[roles-y-permisos]] (retiro y excepción de la PM), [[modelo-de-dominio]] (`Assignment` con cierre), [[auditoria]], [[backend-hexagonal]] (`ReassignTicket`), [[glosario]] mediante Notas para la wiki; PRD CA-04 propuesto para marcar en [[criterios-de-aceptacion]] junto con HU-018.
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
| CHU-08 | Pendiente | — | — |
| CHU-09 | Pendiente | — | — |
| CHU-10 | Pendiente | — | — |
| DoD-01 Pruebas backend | Pendiente | — | — |
| DoD-02 ArchitectureTests | Pendiente | — | — |
| DoD-03 Migración `AddAssignmentEnd` | Pendiente | — | — |
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

- **V-10** — la reasignación depende de distinguir responsable y participante; si se decide que no hay diferencia, la reasignación se reduce a retirar + asignar (HU-018) y `TicketReassigned` se registra igualmente para cumplir PRD §12.
- (propuesta) `keepPreviousAsParticipant` es obligatorio para que la decisión sea siempre explícita (PRD §6.3 permite que el desarrollador siga como colaborador).
- (propuesta) `RemoveParticipant` invoca `IChatConnectionRevoker` desde esta HU con un doble; el adaptador SignalR real llega con HU-032, de modo que no haya que tocar este caso de uso después.

## Relacionado

- [[ep-006-triage-asignacion-y-participantes]] · [[tablero-scrum]] · [[flujo-del-ticket]] · [[roles-y-permisos]] · [[chat-interno]] · [[tiempo-real-signalr]]
- [[modelo-de-dominio]] · [[auditoria]] · [[backend-hexagonal]] · [[elizabeth-pm]] · [[criterios-de-aceptacion]] · [[pendientes]]
