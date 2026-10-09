---
title: "HU-018 — Asignar y agregar participantes"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/ticket, producto/roles, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §6.2", "PRD.md §5", "PRD.md §6.3", "PRD.md §7", "PRD.md §10", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-018", "Asignar y agregar participantes", "AssignParticipants"]
epica: "[[ep-006-triage-asignacion-y-participantes]]"
criterios_prd: ["PRD CA-04", "PRD CA-14", "PRD CA-08", "PRD CA-10"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Identity", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 3"
dependencias: ["[[hu-016-cola-global-de-triage]]", "[[hu-011-registrar-eventos-auditables]]", "[[hu-009-administrar-usuarios-internos-y-equipos]]", "[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-020-detalle-interno-del-ticket]]"]
relacionadas: ["[[hu-017-cola-de-cobertura-y-tomar-ticket]]", "[[hu-019-reasignar-y-retirar-participantes]]", "[[hu-038-correo-de-vinculacion]]", "[[hu-021-cambiar-estado-interno]]", "[[hu-026-historial-del-chat-por-cursor]]", "[[hu-023-bandeja-de-mis-tickets-y-equipo]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-018 — Asignar y agregar participantes

Elizabeth (PM) o un administrador asigna una o varias personas internas como responsables de un ticket —incluida la asignación directa a Julián— y agrega participantes adicionales; asignar implica participar (propuesta, V-10), agregar da acceso al historial completo y cada cambio queda auditado (PRD §5.4, §6.2, CA-04).

## Historia de usuario

**COMO** Elizabeth (PM)  
**QUIERO** asignar uno o varios responsables internos a un ticket y agregar otras personas como participantes  
**PARA** que el equipo adecuado trabaje el caso con acceso a todo su contexto, dejando constancia de quién lo decidió

## Contexto

Elizabeth asigna una o varias personas y puede asignar directamente a Julián si corresponde a Producción (PRD §6.2.2, §6.3). La PM y los administradores agregan participantes; agregar da acceso al historial completo del chat (PRD §5.4, §7.3). La asignación a un equipo no sustituye la lista de participantes: el acceso es por asociación explícita, excepto Elizabeth (PRD §6.2.5, §10). El modelo propone que `Assignment` registre el acto de asignar responsables y que asignar implique asociar como `TicketParticipant`, que es lo que concede acceso ([[modelo-de-dominio]]); la diferencia entre responsable, asignación y participante sigue abierta (V-10). Los roles de Identity `Development`/`Production` y los equipos `Team.Development`/`Team.Production` no están alineados (hallazgo 1), lo que afecta a quién es "persona interna asignable".

## Alcance

- Entidades `Assignment` y `TicketParticipant` en el agregado `Ticket`, con origen de la participación (asignación, incorporación, cobertura).
- Caso de uso `AssignParticipants` con dos modos: asignar responsables (crea `Assignment` + participación) y agregar participantes (solo participación).
- Consulta de personas internas asignables (nombre propuesto `IInternalUserDirectory`, ratificar en T-01 y registrar en el glosario) y endpoint para el selector.
- Endpoints de asignación, incorporación y listado de participantes.
- Regla de acceso al ticket basada en participación vigente (consumida por HU-020, HU-012, HU-027).
- Auditoría `TicketAssigned` y `ParticipantAdded`; evento de dominio para el correo de vinculación (consumido por HU-038).
- Panel de participantes en el detalle interno con selector agrupado por equipo.
- Migración EF Core.

## Fuera de alcance

- Envío del correo de vinculación: [[hu-038-correo-de-vinculacion|HU-038]] (esta HU solo expone la primera asociación como evento).
- Cambio de estado al asignar (incluida la ruta directa a `InProduction`): [[hu-021-cambiar-estado-interno|HU-021]] (V-01, V-02).
- Reasignar y retirar: [[hu-019-reasignar-y-retirar-participantes|HU-019]].
- Asignar a un equipo como tal (el PRD exige personas explícitas).
- Historial del chat para el participante agregado: lo sirve [[hu-026-historial-del-chat-por-cursor|HU-026]] con la regla definida aquí.

## Requisitos y reglas de negocio

- Elizabeth asigna una o varias personas; puede asignar directamente a Julián (PRD §6.2.2, §6.3).
- La PM y los administradores agregan participantes durante toda la vida del ticket; cada cambio se audita (PRD §5.4, §6.2.4, §12).
- Agregar a alguien le da acceso al historial completo (PRD §5.4, §7.3).
- La asignación a un equipo no sustituye la participación; el acceso al detalle y al chat es por asociación explícita, excepto Elizabeth (PRD §6.2.5, §10).
- La primera asociación de una persona dispara un correo de vinculación (PRD §7.7, §11) — implementado en HU-038.
- Los clientes nunca ven nombres de participantes internos (PRD §5.6, §10).
- (propuesta, V-10) Asignar implica participar; un participante puede no ser responsable.
- (propuesta) Persona interna asignable = usuario activo sin empresa cliente y con pertenencia a `Team.Development` o `Team.Production` (definición a ratificar por el hallazgo 1).
- (propuesta) Operación todo o nada: si un identificador es inválido, no se aplica ninguno.
- (propuesta) Repetir una asignación o incorporación vigente no duplica filas ni auditoría.
- (propuesta, V-01) No se asigna ni agrega en tickets `Closed`.

## Invariantes en juego

- Invariante 3: la participación vigente es la base del acceso al chat (Elizabeth siempre accede).
- Invariante 2: el cliente no ve participantes internos.
- Invariante 8: asignaciones e incorporaciones auditadas.

## Criterios del PRD cubiertos

- PRD CA-04 (parcial: asignar una o varias personas y agregar participantes, auditado; reasignar y quitar en HU-019) → [[criterios-de-aceptacion]]
- PRD CA-14 (parcial: participantes y asignación) → [[criterios-de-aceptacion]]
- PRD CA-08 (parcial: el participante agregado tiene acceso al historial completo; el chat lo sirve HU-026) → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial: el cliente no accede a la lista de participantes) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-006-triage-asignacion-y-participantes|EP-006 — Triage, asignación y participantes]]
- Dependencias: [[hu-016-cola-global-de-triage|HU-016 — Cola global de triage]] · [[hu-011-registrar-eventos-auditables|HU-011 — Registrar eventos auditables]] · [[hu-009-administrar-usuarios-internos-y-equipos|HU-009 — Administrar usuarios internos y equipos]] (personas y equipos; hallazgo 1) · [[hu-004-contexto-de-usuario-y-autorizacion|HU-004 — Contexto de usuario y autorización]] · [[hu-020-detalle-interno-del-ticket|HU-020 — Detalle interno del ticket]] (lugar del panel)
- Relacionadas: [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] · [[hu-019-reasignar-y-retirar-participantes|HU-019]] · [[hu-038-correo-de-vinculacion|HU-038]] · [[hu-021-cambiar-estado-interno|HU-021]] · [[hu-026-historial-del-chat-por-cursor|HU-026]] · [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]]
- Decisiones: V-10 · V-02 · V-08 ([[pendientes]]) · hallazgo 1

## Componentes afectados

- Backend (Domain): `Assignment`, `TicketParticipant`, `Ticket.Assign(...)`, `Ticket.AddParticipant(...)`, regla de acceso por participación vigente.
- Backend (Application): `AssignParticipants`, `ITicketRepository`, `IAuditLog`, `ICurrentUser`, `IClock`, `IInternalUserDirectory` (propuesto).
- Backend (Infrastructure): configuraciones EF Core, consulta de personas internas sobre Identity/equipos, migración.
- Backend (Api): endpoints de personas internas, asignaciones y participantes.
- Identity: lectura de roles/equipos de `ApplicationUser`.
- Frontend: panel de participantes y selector (módulo `triage`, integrado en el detalle de HU-020).

## Dificultad

**Nivel:** Alto

**Justificación:** define la regla de acceso que usan el detalle, la bitácora y el chat; cruza Identity, dominio, persistencia y frontend; depende de dos ambigüedades abiertas (V-10, hallazgo 1) que deben quedar aisladas; cada operación debe ser atómica y auditada por persona.

## Contrato backend ↔ frontend

Propuesta, ratificar en T-01.

**`GET /api/internal-users?team=Development&search=lau`** — `ProductManager`, `Administrator`. `200`:

```json
[
  { "userId": "77b3…", "displayName": "Laura", "teams": ["Development"] },
  { "userId": "j01a…", "displayName": "Julián", "teams": ["Production"] }
]
```

Solo personas internas activas; nunca usuarios cliente.

**`POST /api/tickets/{ticketId}/assignments`** — `ProductManager`, `Administrator`.

```json
{ "assigneeIds": ["77b3…", "b5r4…"] }
```

**`POST /api/tickets/{ticketId}/participants`** — `ProductManager`, `Administrator`.

```json
{ "userIds": ["k3v1…"] }
```

Ambos responden `200 OK` con la lista vigente (misma forma que el `GET`):

**`GET /api/tickets/{ticketId}/participants`** — `ProductManager`, `Administrator`, participantes internos vigentes.

```json
{
  "ticketId": "3f4e…",
  "participants": [
    {
      "userId": "77b3…",
      "displayName": "Laura",
      "teams": ["Development"],
      "isAssignee": true,
      "origin": "Assignment",
      "addedAt": "2026-10-08T14:00:00Z",
      "addedBy": { "userId": "5a1c…", "displayName": "Elizabeth" }
    }
  ]
}
```

| Código | Cuándo |
|---|---|
| 400 | Lista vacía; identificador desconocido, inactivo o de usuario cliente (`errors` por identificador) |
| 401 | Sin sesión |
| 403 | Rol cliente; interno distinto de PM/administrador en `POST` |
| 404 | Ticket inexistente; en `GET`, interno no participante (propuesta) |
| 409 | Ticket `Closed` (propuesta, V-01) |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: fijar rutas, DTOs, orígenes de participación y códigos; resolver con la persona usuaria la definición de "persona interna asignable" (hallazgo 1) y ratificar "asignar implica participar" (V-10). Registrar `IInternalUserDirectory` y el origen de participación en [[glosario]].
- [ ] **T-02 — Domain: `Assignment` y `TicketParticipant`** · Capa: Backend (Domain) · Dificultad: Medio  
  Descripción: pruebas primero: `Assign_CreatesAssignmentAndActiveParticipation`, `Assign_SeveralPeople_CreatesOneAssignmentEach`, `Assign_AlreadyActiveAssignee_IsNoOp`, `AddParticipant_DoesNotCreateAssignment`, `AddParticipant_OnClosedTicket_Throws`, `CanAccess_ActiveParticipant_IsTrue`, `CanAccess_TeamMemberNotParticipant_IsFalse`, `CanAccess_ProductManager_IsAlwaysTrue`. Implementar métodos del agregado y la regla de acceso.
- [ ] **T-03 — Application: `AssignParticipants`** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: pruebas unitarias primero: `AssignParticipants_AsProductManager_Succeeds`, `AssignParticipants_AsAdministratorNotParticipant_Succeeds`, `AssignParticipants_AsDeveloper_IsForbidden`, `AssignParticipants_WithClientUserId_RejectsAll`, `AssignParticipants_AppendsOneAuditEntryPerPerson`, `AssignParticipants_RaisesFirstAssociationEvent`.
- [ ] **T-04 — Infrastructure: persistencia, directorio y migración** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: pruebas de integración primero: `Participants_PersistWithAddedByAndAddedAt`, `InternalUserDirectory_ExcludesClientAndInactiveUsers`. Configuraciones EF Core (índice único parcial de participación vigente por `(ticket_id, user_id)` donde `removed_at IS NULL`, propuesta) y migración `AddAssignmentsAndParticipants` (si HU-017 no la creó).
- [ ] **T-05 — Api: endpoints** · Capa: Backend (Api) · Dificultad: Medio  
  Descripción: pruebas de integración primero: `PostAssignments_TwoPeople_Returns200AndBothCanOpenDetail`, `PostAssignments_DirectToJulian_KeepsStatusNew`, `PostParticipants_WithClientUserId_Returns400AndAddsNobody`, `PostAssignments_AsDeveloper_Returns403`, `GetParticipants_AsRequester_Returns403`.
- [ ] **T-06 — Frontend models** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: pruebas Vitest primero: `toParticipant`, `groupByTeam`, `validateSelection` (lista vacía, duplicados), `toSelectionErrors`. Implementar `participants.ts` y `participantsGateway.ts`.
- [ ] **T-07 — Frontend controller** · Capa: Frontend (controllers) · Dificultad: Bajo  
  Descripción: `useParticipantsController(ticketId)` con búsqueda de personas, modos "asignar" y "agregar", envío y refresco de la lista. Prueba Vitest del reducer.
- [ ] **T-08 — Frontend views** · Capa: Frontend (views) · Dificultad: Bajo  
  Descripción: `ParticipantsPanelView` (lista con responsable/participante, quién y cuándo) y `AssignPeopleDialogView` (selector multiselección agrupado por equipo). Visible solo para PM/administrador según `/api/auth/me`; la autorización real es del backend. Lint MVC sin violaciones.

## Criterios de aceptación

### CHU-01 — Asignar varias personas

**Dado** un ticket `New` de la empresa A y Elizabeth con sesión  
**Cuando** envía `POST /api/tickets/{ticketId}/assignments` con Laura y Brayan  
**Entonces** recibe `200` con ambos como `isAssignee: true`, `origin: "Assignment"`, `addedBy` = Elizabeth; existen dos `Assignment` y dos participaciones vigentes; y ambos obtienen `200` al abrir el detalle interno (HU-020).

### CHU-02 — Ruta directa a Julián

**Dado** un ticket `New` que corresponde a Producción  
**Cuando** Elizabeth asigna solo a Julián  
**Entonces** recibe `200`, Julián es responsable y participante vigente y el estado del ticket sigue en `New` (el cambio de estado es HU-021; V-02).

### CHU-03 — Agregar participante con acceso al historial

**Dado** un ticket con actividad previa (entradas de auditoría y, cuando exista, mensajes de chat anteriores)  
**Cuando** un administrador agrega a Kevin con `POST /api/tickets/{ticketId}/participants`  
**Entonces** Kevin aparece con `isAssignee: false` y `origin: "Participant"`; la regla de acceso le concede el ticket; y puede consultar la bitácora completa (HU-012) y el historial completo del chat (HU-026), incluidos elementos anteriores a su incorporación.

### CHU-04 — Validación todo o nada

**Dado** Elizabeth y un ticket abierto  
**Cuando** envía `assigneeIds: []`; luego `[Laura, <usuario cliente de A>]`; luego `[Laura, <usuario interno inactivo>]`; luego `[Laura, <GUID inexistente>]`  
**Entonces** todas responden `400` con el identificador problemático en `errors`, y Laura no queda asignada en ninguno de los casos ni se crea auditoría.

### CHU-05 — Autorización por rol en backend

**Dado** un administrador no participante, Laura (Desarrollo, participante), Julián (Producción, participante), una coordinadora de la empresa A y una petición sin sesión  
**Cuando** cada uno intenta asignar a Brayan  
**Entonces** el administrador recibe `200` (PRD §5.4); Laura, Julián y la coordinadora `403`; sin sesión `401`.

### CHU-06 — Auditoría por persona

**Dado** la asignación de CHU-01 con `IClock` fijo en `2026-10-08T14:00:00Z`  
**Cuando** se consulta la auditoría  
**Entonces** hay, para Laura y para Brayan, una entrada `TicketAssigned` (objeto `Assignment`, `new_value` con el `assigneeId`) y una `ParticipantAdded` (objeto `TicketParticipant`, `new_value.origin` = `Assignment`), todas con actor Elizabeth y fecha `2026-10-08 14:00:00+00`.

### CHU-07 — Idempotencia

**Dado** Laura ya asignada y vigente  
**Cuando** Elizabeth vuelve a asignarla y luego la agrega como participante  
**Entonces** ambas respuestas son `200`, sigue existiendo una sola participación vigente y un solo `Assignment` vigente de Laura, y no se crean entradas de auditoría nuevas.

### CHU-08 — El equipo no da acceso

**Dado** Cristian, del equipo Desarrollo, no asociado al ticket  
**Cuando** solicita el detalle interno o la lista de participantes  
**Entonces** recibe `404` (propuesta) y no ve descripción, adjuntos ni participantes.

### CHU-09 — Sin exposición al cliente

**Dado** un ticket de la empresa A con Laura y Brayan asignados  
**Cuando** la solicitante de A llama a `GET /api/tickets/{ticketId}/participants` y consulta su ticket en el portal (contrato de HU-024)  
**Entonces** la primera responde `403` y la respuesta del portal no contiene los nombres "Laura" ni "Brayan" ni ninguna clave de participantes.

### CHU-10 — Ticket cerrado

**Dado** un ticket `Closed`  
**Cuando** Elizabeth intenta asignar o agregar a alguien  
**Entonces** recibe `409` (propuesta, V-01) y no cambia nada.

### CHU-11 — Comportamiento de la UI ante error

**Dado** el diálogo de asignación con dos personas seleccionadas  
**Cuando** el backend responde `400` por una de ellas o falla la red  
**Entonces** con `400` la persona problemática se marca con el mensaje y la selección se conserva; con fallo de red se muestra `role="alert"` con "Reintentar"; durante el envío el botón está deshabilitado.

## Definition of Done

- [ ] CHU-01 a CHU-11 validados con evidencia.
- [ ] Definición de "persona interna asignable" (hallazgo 1) y V-10 ratificados o registrados como propuesta vigente.
- [ ] Pruebas backend escritas primero y en verde (`cd backend && dotnet test`): `Assign_SeveralPeople_CreatesOneAssignmentEach`, `CanAccess_TeamMemberNotParticipant_IsFalse`, `CanAccess_ProductManager_IsAlwaysTrue`, `AssignParticipants_WithClientUserId_RejectsAll`, `AssignParticipants_AppendsOneAuditEntryPerPerson`, `PostAssignments_DirectToJulian_KeepsStatusNew`, `GetParticipants_AsRequester_Returns403`.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Migración EF Core `AddAssignmentsAndParticipants` creada (o reutilizada de HU-017), aplicada en Compose y reversible.
- [ ] Pruebas Vitest en verde (`npm --prefix frontend test`): `toParticipant`, `groupByTeam`, `validateSelection`, reducer.
- [ ] `npm --prefix frontend run lint` sin errores y `npm --prefix frontend run build` correcto.
- [ ] `docker compose up --build`: Elizabeth asigna a dos personas sintéticas y ambas ven el ticket en el detalle.
- [ ] Wiki: [[flujo-del-ticket]], [[roles-y-permisos]] (acceso por participación), [[modelo-de-dominio]] (`Assignment` vs `TicketParticipant`, origen), [[auditoria]], [[backend-hexagonal]], [[glosario]], [[equipo-data-global]] (equipos vs roles) mediante Notas para la wiki.
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
| CHU-11 | Pendiente | — | — |
| DoD-01 Hallazgo 1 y V-10 | Pendiente | — | — |
| DoD-02 Pruebas backend | Pendiente | — | — |
| DoD-03 ArchitectureTests | Pendiente | — | — |
| DoD-04 Migración `AddAssignmentsAndParticipants` | Pendiente | — | — |
| DoD-05 Pruebas Vitest | Pendiente | — | — |
| DoD-06 Lint y build frontend | Pendiente | — | — |
| DoD-07 `docker compose up --build` | Pendiente | — | — |
| DoD-08 Wiki actualizada | Pendiente | — | — |
| DoD-09 quality-reviewer | Pendiente | — | — |
| DoD-10 PR revisado | Pendiente | — | — |
| DoD-11 Trazabilidad | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- **V-10** — si se decide que asignar no implica participar, solo cambia `Ticket.Assign(...)`; las pruebas CHU-01 y CHU-06 lo detectarán.
- **Hallazgo 1** — `autenticacion-identity` propone roles `Development`/`Production`; el glosario habla de equipos `Team.Development`/`Team.Production`. Para asignar se necesita una sola fuente de verdad sobre "quién es interno y de qué equipo".
- **V-08** — el evento de primera asociación se emite también tras un retiro y reincorporación; HU-038 decide si envía correo.
- (propuesta) `GET /api/tickets/{ticketId}/participants` permitido al administrador no asociado: gestionar participantes es una facultad suya (PRD §5.4) y la lista no es contenido del ticket.

## Relacionado

- [[ep-006-triage-asignacion-y-participantes]] · [[tablero-scrum]] · [[flujo-del-ticket]] · [[roles-y-permisos]] · [[julian-produccion]] · [[elizabeth-pm]]
- [[equipo-data-global]] · [[modelo-de-dominio]] · [[auditoria]] · [[autenticacion-identity]] · [[backend-hexagonal]] · [[criterios-de-aceptacion]] · [[pendientes]]
