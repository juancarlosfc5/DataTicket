---
title: "HU-015 — Ajustar prioridad manualmente"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/prioridad, producto/auditoria, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §6.1", "PRD.md §5", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-015", "Ajustar prioridad manualmente", "PriorityOverride"]
epica: "[[ep-005-radicacion-de-tickets]]"
criterios_prd: ["PRD CA-14", "PRD CA-10", "PRD CA-02"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 3"
dependencias: ["[[hu-013-radicar-ticket]]", "[[hu-011-registrar-eventos-auditables]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-020-detalle-interno-del-ticket]]"]
relacionadas: ["[[hu-016-cola-global-de-triage]]", "[[hu-017-cola-de-cobertura-y-tomar-ticket]]", "[[hu-012-consultar-bitacora-del-ticket]]", "[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-040-panel-global-de-la-pm]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-015 — Ajustar prioridad manualmente

Elizabeth (PM) —y un administrador asociado, según propuesta— cambia la prioridad de un ticket con un motivo obligatorio; se conserva la prioridad calculada original, la nueva, quién, cuándo y el motivo, y el cambio queda auditado (PRD §6.1, §12). **Quién puede ajustarla no está cerrado (V-03).**

## Historia de usuario

**COMO** Elizabeth (PM)  
**QUIERO** ajustar la prioridad calculada de un ticket indicando el motivo  
**PARA** reflejar el impacto real del caso cuando la matriz urgencia × impacto no lo captura, dejando constancia de por qué se cambió

## Contexto

El PRD exige que, si un integrante de Data Global ajusta la prioridad, quede registrada la prioridad calculada, la nueva, quién, cuándo y el motivo (PRD §6.1), y que el cambio de prioridad sea un evento auditado (PRD §12, CA-14). No acota qué rol puede hacerlo ni si las colas ordenan por la calculada o la ajustada (V-03). El mapa de puertos no tiene caso de uso para esto (hallazgo 7): se propone `OverrideTicketPriority` (nombre propuesto, ratificar en T-01 y registrar en el glosario). La acción se lanza desde el detalle interno ([[hu-020-detalle-interno-del-ticket|HU-020]]).

## Alcance

- Entidad `PriorityOverride` en el agregado `Ticket` (calculada original, anterior, nueva, autor, fecha, motivo).
- Concepto de prioridad vigente del ticket (`EffectivePriority`, nombre propuesto): la del último ajuste o, si no hay, la calculada.
- Caso de uso `OverrideTicketPriority` con autorización y concurrencia optimista.
- Endpoints `POST` y `GET /api/tickets/{ticketId}/priority-overrides`.
- Entrada de auditoría `PriorityOverridden`.
- Acción "Ajustar prioridad" con diálogo (prioridad + motivo) e historial de ajustes en el detalle interno.
- Migración EF Core.

## Fuera de alcance

- Decidir el orden de las colas por prioridad calculada o vigente (V-03; HU-016/HU-023 lo aplicarán cuando se decida).
- Mostrar la prioridad vigente o el ajuste al cliente (propuesta: nunca; ver reglas).
- Recalcular la prioridad si cambian urgencia o impacto (el PRD no prevé editar urgencia/impacto tras radicar).
- Notificaciones por cambio de prioridad (no figuran en PRD §11).

## Requisitos y reglas de negocio

- Se registran prioridad calculada, nueva prioridad, quién, cuándo y motivo (PRD §6.1).
- El cambio de prioridad es auditable y conserva actor y fecha/hora (PRD §12, §14.14).
- La prioridad calculada original no se modifica nunca (PRD §6.1; [[matriz-de-prioridad]]).
- La cola de cobertura muestra la prioridad calculada (PRD §5.5).
- (propuesta, V-03) Pueden ajustar: `ProductManager` siempre; `Administrator` solo si es participante vigente del ticket (coherente con PRD §5.5). Otros internos y clientes, no.
- (propuesta) Motivo obligatorio de 1 a 500 caracteres tras recortar espacios.
- (propuesta) La nueva prioridad debe ser un valor de `CalculatedPriority` distinto de la prioridad vigente; volver al valor calculado es un ajuste válido.
- (propuesta, V-01) No se ajusta la prioridad de un ticket `Closed`.
- (propuesta, derivada de PRD §14.2) El cliente solo ve datos de radicación, estado resumido y respuesta formal: el ajuste, su motivo y la prioridad vigente no se exponen en el portal.

## Invariantes en juego

- Invariante 2: el cliente no ve el ajuste (actividad interna).
- Invariante 5: un administrador no actúa sobre contenido sin estar asociado (propuesta).
- Invariante 8: el ajuste queda en la auditoría con anterior/nuevo.

## Criterios del PRD cubiertos

- PRD CA-14 (parcial: prioridad) → [[criterios-de-aceptacion]]
- PRD CA-10 y CA-02 (parcial: el ajuste no llega al cliente) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-005-radicacion-de-tickets|EP-005 — Radicación de tickets]]
- Dependencias: [[hu-013-radicar-ticket|HU-013 — Radicar ticket]] · [[hu-011-registrar-eventos-auditables|HU-011 — Registrar eventos auditables]] · [[hu-018-asignar-y-agregar-participantes|HU-018 — Asignar y agregar participantes]] (participación vigente del administrador) · [[hu-020-detalle-interno-del-ticket|HU-020 — Detalle interno del ticket]] (lugar de la acción)
- Relacionadas: [[hu-016-cola-global-de-triage|HU-016]] · [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] · [[hu-012-consultar-bitacora-del-ticket|HU-012]] · [[hu-024-portal-solicitante-consulta-tickets|HU-024]] (no debe exponer el ajuste) · [[hu-040-panel-global-de-la-pm|HU-040]]
- Decisiones: V-03 ([[pendientes]]) · V-01 (ticket cerrado)

## Componentes afectados

- Backend (Domain): `PriorityOverride`, `Ticket.OverridePriority(...)`, prioridad vigente.
- Backend (Application): `OverrideTicketPriority` (nombre propuesto), `ITicketRepository`, `IAuditLog`, `ICurrentUser`, `IClock`.
- Backend (Infrastructure): configuración EF Core, token de concurrencia (`xmin`, propuesta), migración.
- Backend (Api): `POST`/`GET /api/tickets/{ticketId}/priority-overrides`.
- Persistencia PostgreSQL: tabla de ajustes y columna de prioridad vigente en `tickets` (propuesta, para ordenar colas).
- Frontend: diálogo e historial en el detalle interno (módulo que fije HU-020).

## Dificultad

**Nivel:** Medio

**Justificación:** regla de dominio sencilla, pero cruza backend y frontend, introduce concurrencia optimista sobre el agregado `Ticket` y una regla de autorización abierta (V-03) que debe quedar aislada para cambiarla sin reescribir.

## Contrato backend ↔ frontend

Propuesta, ratificar en T-01.

**`POST /api/tickets/{ticketId}/priority-overrides`** — `ProductManager`; `Administrator` participante vigente.

```json
{ "newPriority": "High", "reason": "Bloquea la facturación del cierre de mes", "expectedVersion": "AAAAAAAAB9E=" }
```

Respuesta `201 Created`:

```json
{
  "ticketId": "3f4e…",
  "calculatedPriority": "Medium",
  "effectivePriority": "High",
  "version": "AAAAAAAAB+A=",
  "override": {
    "id": "p9a8…",
    "calculatedPriority": "Medium",
    "previousPriority": "Medium",
    "newPriority": "High",
    "changedBy": { "userId": "5a1c…", "displayName": "Elizabeth" },
    "changedAt": "2026-10-07T15:05:12Z",
    "reason": "Bloquea la facturación del cierre de mes"
  }
}
```

**`GET /api/tickets/{ticketId}/priority-overrides`** — mismos roles más participantes internos vigentes (lectura). `200`: `{ "calculatedPriority": "Medium", "effectivePriority": "High", "items": [ …override… ] }` en orden cronológico.

| Código | Cuándo |
|---|---|
| 400 | `newPriority` inválida o igual a la vigente; `reason` vacío o > 500 |
| 401 | Sin sesión |
| 403 | Rol cliente; interno sin permiso de ajuste (p. ej. `Development`) en `POST` |
| 404 | Ticket inexistente o no visible (administrador no asociado, no participante) — propuesta |
| 409 | Ticket `Closed` o `expectedVersion` desactualizada |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: fijar DTOs, regla de autorización (llevar V-03 a la persona usuaria), límite del motivo, manejo de versión y códigos. Registrar `OverrideTicketPriority` y `EffectivePriority` en [[glosario]] y en el mapa de [[backend-hexagonal]] (hallazgo 7).
- [ ] **T-02 — Domain: `PriorityOverride`** · Capa: Backend (Domain) · Dificultad: Bajo  
  Descripción: pruebas primero: `OverridePriority_KeepsCalculatedPriorityUnchanged`, `OverridePriority_SetsEffectivePriority`, `OverridePriority_WithBlankReason_Throws`, `OverridePriority_WithReasonOf501Chars_Throws`, `OverridePriority_ToSameEffectivePriority_Throws`, `OverridePriority_OnClosedTicket_Throws`, `OverridePriority_Twice_KeepsBothWithPreviousValues`.
- [ ] **T-03 — Application: `OverrideTicketPriority`** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: pruebas unitarias primero: `Override_AsProductManager_Succeeds`, `Override_AsAdministratorParticipant_Succeeds`, `Override_AsAdministratorNotParticipant_ReturnsNotFound`, `Override_AsDeveloperParticipant_IsForbidden`, `Override_AppendsPriorityOverriddenAuditEntry`. Implementar con la regla de autorización encapsulada en un único punto.
- [ ] **T-04 — Infrastructure: persistencia y migración** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: pruebas de integración primero: `ConcurrentOverrides_SecondFailsWithConflict`, `Overrides_PersistWithCalculatedAndPrevious`. Configuración EF Core, token de concurrencia y migración `AddPriorityOverrides` (tabla + columna de prioridad vigente, propuesta).
- [ ] **T-05 — Api: endpoints** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: pruebas de integración primero: `PostOverride_AsRequester_Returns403`, `PostOverride_ReasonOf501_Returns400`, `PostOverride_OnClosedTicket_Returns409`, `GetOverrides_AsRequester_Returns403`. Mapear endpoints con ProblemDetails.
- [ ] **T-06 — Frontend models** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: pruebas Vitest primero: `validateOverride` (motivo 0/1/500/501, prioridad igual a la vigente), `toPriorityOverride`, etiquetas en español de `CalculatedPriority`. Implementar `priorityOverride.ts` y su gateway sobre `core/http`.
- [ ] **T-07 — Frontend controller** · Capa: Frontend (controllers) · Dificultad: Bajo  
  Descripción: `usePriorityOverrideController(ticketId)` con estado del diálogo, envío con `expectedVersion`, manejo de `409` (recargar) y actualización del historial. Prueba Vitest del reducer.
- [ ] **T-08 — Frontend views** · Capa: Frontend (views) · Dificultad: Bajo  
  Descripción: `PriorityOverrideDialogView` (selector, motivo con contador 0/500, errores) y `PriorityOverrideHistoryView`; la acción solo se muestra si `/api/auth/me` indica permiso (la autorización real es del backend). Lint MVC sin violaciones.

## Criterios de aceptación

### CHU-01 — Ajuste exitoso conserva la calculada

**Dado** un ticket con urgencia `Medium` e impacto `Medium` (calculada `Medium`) sin ajustes  
**Cuando** Elizabeth envía `newPriority: "High"` con motivo "Bloquea la facturación del cierre de mes"  
**Entonces** recibe `201` con `calculatedPriority: "Medium"` y `effectivePriority: "High"`; el ajuste persistido guarda calculada `Medium`, anterior `Medium`, nueva `High`, autor Elizabeth, fecha UTC y el motivo; y el detalle interno muestra "Alta (ajustada; calculada: Media)".

### CHU-02 — Validaciones con valores límite

**Dado** el mismo ticket con prioridad vigente `High`  
**Cuando** se envían motivos de 0 caracteres, solo espacios, 1, 500 y 501 caracteres; `newPriority: "Urgent"`; y `newPriority: "High"`  
**Entonces** 1 y 500 caracteres (con prioridad distinta) → `201`; 0, solo espacios y 501 → `400` en `reason`; `Urgent` → `400` en `newPriority`; `High` (igual a la vigente) → `400`; y los rechazos no crean ajuste ni auditoría.

### CHU-03 — Autorización por rol en backend

**Dado** un administrador participante vigente, un administrador no asociado, Laura (Desarrollo, participante), Julián (Producción, participante), un solicitante de la empresa dueña y una petición sin sesión  
**Cuando** cada uno envía un ajuste válido  
**Entonces** el administrador participante recibe `201`; el no asociado `404`; Laura y Julián `403`; el solicitante `403`; sin sesión `401` (regla propuesta, V-03).

### CHU-04 — Auditoría del ajuste

**Dado** el ajuste de CHU-01 con `IClock` fijo en `2026-10-07T15:05:12Z`  
**Cuando** se consulta la auditoría  
**Entonces** existe una entrada `PriorityOverridden` con actor Elizabeth, fecha `2026-10-07 15:05:12+00`, objeto `Ticket`/id, `old_value` `{"priority":"Medium"}` y `new_value` `{"priority":"High","calculatedPriority":"Medium","reason":"Bloquea la facturación del cierre de mes"}`, en la misma transacción.

### CHU-05 — Historial de ajustes sucesivos

**Dado** un ticket calculado `Medium` ajustado a `High` y luego a `Critical`  
**Cuando** se consulta `GET /api/tickets/{ticketId}/priority-overrides`  
**Entonces** devuelve dos ajustes en orden cronológico: (anterior `Medium` → `High`) y (anterior `High` → `Critical`), ambos con calculada `Medium`, y `effectivePriority: "Critical"`.

### CHU-06 — Sin exposición al cliente

**Dado** un ticket de la empresa A con un ajuste de prioridad  
**Cuando** el solicitante o el coordinador de A llaman a `GET` o `POST /api/tickets/{ticketId}/priority-overrides`  
**Entonces** reciben `403`, y el contrato de consulta del portal ([[hu-024-portal-solicitante-consulta-tickets|HU-024]]) no contiene `effectivePriority`, `override` ni `reason` (verificado por la prueba de forma de esa HU).

### CHU-07 — Estado y concurrencia

**Dado** un ticket `Closed` y otro ticket abierto sobre el que dos personas envían ajustes con la misma `expectedVersion`  
**Cuando** se procesan  
**Entonces** el ajuste del ticket cerrado responde `409` (propuesta, V-01); de los concurrentes uno responde `201` y el otro `409`, y solo existe un ajuste y una entrada de auditoría nuevos.

### CHU-08 — Comportamiento de la UI ante error

**Dado** el diálogo de ajuste abierto con un motivo escrito  
**Cuando** el backend responde `400` (motivo), `409` (versión) o falla la red  
**Entonces** con `400` el error aparece junto al campo; con `409` se muestra "El ticket cambió; recarga para ver la prioridad actual" con acción de recarga; con fallo de red un `role="alert"` con "Reintentar"; en todos los casos el motivo escrito se conserva.

## Definition of Done

- [ ] CHU-01 a CHU-08 validados con evidencia.
- [ ] Regla de autorización (V-03) ratificada por la persona usuaria o ajustada antes de cerrar.
- [ ] Pruebas backend escritas primero y en verde (`cd backend && dotnet test`): `OverridePriority_KeepsCalculatedPriorityUnchanged`, `OverridePriority_WithReasonOf501Chars_Throws`, `Override_AsDeveloperParticipant_IsForbidden`, `Override_AsAdministratorNotParticipant_ReturnsNotFound`, `Override_AppendsPriorityOverriddenAuditEntry`, `ConcurrentOverrides_SecondFailsWithConflict`, `PostOverride_AsRequester_Returns403`.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Migración EF Core `AddPriorityOverrides` creada, aplicada en Compose y reversible.
- [ ] Pruebas Vitest en verde (`npm --prefix frontend test`): `validateOverride`, `toPriorityOverride`, reducer del diálogo.
- [ ] `npm --prefix frontend run lint` sin errores y `npm --prefix frontend run build` correcto.
- [ ] `docker compose up --build`: Elizabeth (usuario sintético) ajusta la prioridad desde el detalle interno y la ve en la bitácora.
- [ ] Wiki: [[matriz-de-prioridad]] (quién ajusta, prioridad vigente), [[modelo-de-dominio]] (`PriorityOverride` con anterior y `EffectivePriority`), [[backend-hexagonal]] (caso de uso nuevo), [[auditoria]], [[glosario]] mediante Notas para la wiki; V-03 actualizado en [[pendientes]].
- [ ] `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos.
- [ ] PR revisado y aprobado por otra persona del equipo.
- [ ] Trazabilidad de la HU y de [[ep-005-radicacion-de-tickets]] actualizada.

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
| DoD-01 V-03 ratificado | Pendiente | — | — |
| DoD-02 Pruebas backend | Pendiente | — | — |
| DoD-03 ArchitectureTests | Pendiente | — | — |
| DoD-04 Migración `AddPriorityOverrides` | Pendiente | — | — |
| DoD-05 Pruebas Vitest | Pendiente | — | — |
| DoD-06 Lint y build frontend | Pendiente | — | — |
| DoD-07 `docker compose up --build` | Pendiente | — | — |
| DoD-08 Wiki actualizada | Pendiente | — | — |
| DoD-09 quality-reviewer | Pendiente | — | — |
| DoD-10 PR revisado | Pendiente | — | — |
| DoD-11 Trazabilidad | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- **V-03** — el PRD dice "un integrante de Data Global"; la restricción a PM y administrador asociado es propuesta. Si se amplía a participantes internos, solo cambia la regla encapsulada en T-03.
- (propuesta) Se añade `previousPriority` al registro, además de la calculada que exige el PRD, para que ajustes sucesivos sean legibles.
- (propuesta) Concurrencia optimista con la columna de sistema `xmin` de PostgreSQL, expuesta como `version` opaca.

## Relacionado

- [[ep-005-radicacion-de-tickets]] · [[tablero-scrum]] · [[matriz-de-prioridad]] · [[auditoria]] · [[roles-y-permisos]]
- [[modelo-de-dominio]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[criterios-de-aceptacion]] · [[pendientes]]
