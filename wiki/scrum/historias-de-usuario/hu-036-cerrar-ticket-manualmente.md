---
title: "HU-036 — Cerrar ticket manualmente"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/ticket, producto/estados, producto/auditoria]
sources: ["PRD.md §5", "PRD.md §6.4", "PRD.md §6.5", "PRD.md §10", "PRD.md §12", "PRD.md §13", "PRD.md §14"]
aliases: ["HU-036", "Cerrar ticket manualmente", "Cierre manual"]
epica: "[[ep-010-respuesta-formal-y-cierre]]"
criterios_prd: ["CA-12", "CA-14", "CA-02"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 7"
dependencias: ["[[hu-033-emitir-respuesta-formal]]", "[[hu-011-registrar-eventos-auditables]]", "[[hu-020-detalle-interno-del-ticket]]", "[[hu-021-cambiar-estado-interno]]"]
relacionadas: ["[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-025-portal-coordinador-consulta-tickets]]", "[[hu-034-portal-respuesta-formal]]", "[[hu-039-calculo-de-tiempos-habiles]]", "[[hu-040-panel-global-de-la-pm]]", "[[hu-012-consultar-bitacora-del-ticket]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-036 — Cerrar ticket manualmente

Tras confirmar por teléfono con el cliente que el caso quedó resuelto, Elizabeth (PM) o un administrador registra el cierre del ticket (`CloseTicket`). Se guarda quién cerró, cuándo y el estado anterior; no existe cierre automático, temporizador ni recordatorio (PRD §6.5, §14.12).

## Historia de usuario

**COMO** Elizabeth, PM de Data Global (o administrador que cubre su ausencia)  
**QUIERO** cerrar manualmente un ticket después de confirmar con el cliente por teléfono que quedó resuelto  
**PARA** dar por terminado el caso de forma deliberada y trazable, sin cierres automáticos

## Contexto

El cierre lo ejecuta manualmente la PM o un administrador después de confirmar verbalmente con el cliente (PRD §2, §6.5). El sistema no graba ni transcribe la llamada; la conversación se representa únicamente por el acto de cierre (PRD §6.5). El registro conserva quién cerró, cuándo y el estado anterior (PRD §6.5).

Hallazgo del plan de backlog: el `Ticket` del [[modelo-de-dominio]] no tiene campos de cierre. Se propone guardar `ClosedAt` y `ClosedBy` en el ticket (necesarios para la métrica "tiempo hasta cierre", PRD §4) y además una entrada `TicketClosed` en la auditoría con el estado anterior.

## Alcance

- Caso de uso `CloseTicket` con autorización en servidor.
- Transición a `Closed` desde `SolutionDelivered` (exigir `SolutionDelivered` previo es propuesta del modelo, sujeta a V-01).
- Campos `ClosedAt` y `ClosedBy` en `Ticket` (propuesta) y migración.
- Entrada de auditoría con actor, fecha/hora, estado anterior y nuevo.
- Botón "Cerrar ticket" en el detalle interno con diálogo de confirmación que recuerda la confirmación telefónica, sin campos de texto sobre la llamada.
- Bloqueo de nuevas emisiones de respuesta formal y de un segundo cierre sobre un ticket cerrado.
- Evidencia de que no existe ningún proceso programado que cierre tickets.

## Fuera de alcance

- Cierre automático, temporizador de vencimiento y recordatorios (PRD §6.5, §13).
- Guardar notas, duración, número o grabación de la llamada (PRD §6.5).
- Reapertura de tickets cerrados (sin definir, V-01).
- Efectos del cierre sobre el chat y los participantes (solo lectura o no): pendiente de V-01; esta HU no los cambia.
- Correo al cliente por el cierre (PRD §11: el correo por transición no está garantizado).
- Encuesta de satisfacción (PRD §13).

## Requisitos y reglas de negocio

- Solo Elizabeth o un administrador registra el cierre (PRD §6.5, §14.12).
- No hay cierre automático, temporizador de vencimiento ni recordatorios (PRD §6.5).
- El registro conserva quién cerró, cuándo y el estado anterior; no se almacenan detalles de la llamada (PRD §6.5).
- El cliente ve el estado resumido "Cerrado" (PRD §6.4).
- El cierre manual es un evento auditable con actor y fecha/hora (PRD §12, §14.14).
- Propuesta (V-01): solo se cierra desde `SolutionDelivered`; desde otros estados → 409.
- Propuesta: el administrador debe ser participante vigente para cerrar; si no lo es, recibe 404 (coherente con PRD §5.5, [[hu-020-detalle-interno-del-ticket|HU-020]] y [[hu-033-emitir-respuesta-formal|HU-033]]).
- Propuesta: un ticket cerrado no admite nueva respuesta formal ni un segundo cierre (409).

## Invariantes en juego

- Invariante 6 (`AGENTS.md` §7): solo Elizabeth o un administrador cierra; no hay cierre automático.
- Invariante 8: auditoría append-only con actor, fecha, acción, objeto y valores anterior/nuevo.
- Invariante 2: el cliente ve "Cerrado", no estados técnicos ni quién cerró.
- Invariante 5: el administrador no actúa sobre el contenido sin estar asociado.

## Criterios del PRD cubiertos

- PRD CA-12 (total) — Cierre únicamente manual por Elizabeth o administrador; sin cierre automático ni recordatorio → [[criterios-de-aceptacion]]
- PRD CA-14 (parcial: cierre manual con actor y fecha/hora) → [[criterios-de-aceptacion]]
- PRD CA-02 (parcial: el cliente ve el estado resumido `Closed`) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-010-respuesta-formal-y-cierre]]
- Dependencias: [[hu-033-emitir-respuesta-formal|HU-033]] (estado `SolutionDelivered`), [[hu-011-registrar-eventos-auditables|HU-011]] (`IAuditLog`), [[hu-020-detalle-interno-del-ticket|HU-020]] (vista), [[hu-021-cambiar-estado-interno|HU-021]] (máquina de estados).
- Relacionadas: [[hu-024-portal-solicitante-consulta-tickets|HU-024]] y [[hu-025-portal-coordinador-consulta-tickets|HU-025]] (estado `Closed` en el portal), [[hu-034-portal-respuesta-formal|HU-034]], [[hu-039-calculo-de-tiempos-habiles|HU-039]] y [[hu-040-panel-global-de-la-pm|HU-040]] (tiempo hasta cierre), [[hu-012-consultar-bitacora-del-ticket|HU-012]].
- Decisiones: abierta V-01 (transiciones, reapertura, `Closed` exige `SolutionDelivered`) → [[pendientes]].

## Componentes afectados

- Backend (Domain): `Ticket.Close(...)`, campos `ClosedAt`/`ClosedBy` (propuestos).
- Backend (Application): caso de uso `CloseTicket`; puertos `ITicketRepository`, `IAuditLog`, `ICurrentUser`, `IClock`.
- Backend (Infrastructure): configuración EF Core y migración.
- Backend (Api): endpoint REST.
- Persistencia PostgreSQL.
- Frontend (models, controllers, views): detalle interno del ticket.

## Dificultad

**Nivel:** Medio

**Justificación:** la transición es simple, pero exige autorización por rol y participación, auditoría en la misma transacción, cambio de esquema, una prueba que demuestre la ausencia de cierres programados y coherencia con una decisión abierta (V-01).

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01.

**`POST /api/tickets/{ticketId}/close`** · sin cuerpo (no hay campos sobre la llamada) · requiere sesión y token antiforgery.

Respuesta `200 OK`:

```json
{
  "ticketId": "1b0e7c52-8d4a-4f0e-b3a9-5c2d6e7f8a90",
  "previousStatus": "SolutionDelivered",
  "status": "Closed",
  "closedAt": "2026-10-09T15:30:00Z",
  "closedBy": { "userId": "2c9d…", "displayName": "Elizabeth" }
}
```

`closedBy` solo aparece en endpoints internos. En el portal, el detalle devuelve `"clientStatus": "Closed"` sin `closedBy` ni `previousStatus`.

| Código | Caso | `type` |
|---|---|---|
| 400 | La petición trae cuerpo con campos (p. ej. `notes`) | `urn:dataticket:validation` |
| 401 | Sin sesión | — |
| 403 | Usuario cliente, o participante de Desarrollo o Producción (rol insuficiente) | `urn:dataticket:forbidden` |
| 404 | Ticket inexistente o no visible: interno no participante, administrador no asociado o retirado (convención de [[hu-020-detalle-interno-del-ticket\|HU-020]]) | `urn:dataticket:not-found` |
| 409 | Estado distinto de `SolutionDelivered` (incluido `Closed`); conflicto de concurrencia | `urn:dataticket:invalid-ticket-transition` |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar ruta, ausencia de cuerpo, DTO y códigos; confirmar con la persona usuaria el estado de origen exigido (V-01) y la regla del administrador participante; registrar `ClosedAt`/`ClosedBy` en [[modelo-de-dominio]].
- [ ] **T-02 — Dominio** · Capa: Backend (Domain) · Dificultad: Bajo  
  Descripción: primero pruebas: `Close_FromSolutionDelivered_SetsClosedWithActorAndTime`, `Close_FromInProduction_Throws`, `Close_WhenAlreadyClosed_Throws`, `DeliverFormalResponse_WhenClosed_Throws`. Luego `Ticket.Close(actorId, now)` que devuelve el estado anterior; `Status` sin *setter* público.
- [ ] **T-03 — Caso de uso `CloseTicket`** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: primero pruebas: `CloseTicket_ByDeveloper_ReturnsForbidden`, `CloseTicket_ByUnassociatedAdmin_ReturnsNotFound`, `CloseTicket_ByPm_WritesAuditWithPreviousStatus`, `CloseTicket_UsesClockForClosedAt`. Implementar con `ICurrentUser`, `IClock` e `IAuditLog` (`TicketClosed`, anterior `SolutionDelivered`, nuevo `Closed`) en una unidad de trabajo.
- [ ] **T-04 — Persistencia y migración** · Capa: Backend (Infrastructure) · Dificultad: Bajo  
  Descripción: columnas `closed_at` (`timestamptz`) y `closed_by`; migración `AddTicketClosure`; prueba de integración.
- [ ] **T-05 — Endpoint y ausencia de cierre automático** · Capa: Backend (Api) · Dificultad: Medio  
  Descripción: primero pruebas de integración (200, 403 por rol, 404 para administrador no asociado, 409, 400 con cuerpo). Añadir a `DataTicket.ArchitectureTests` una prueba `NoHostedService_DependsOnCloseTicket` (ningún `IHostedService`/`BackgroundService` referencia `CloseTicket` ni `Ticket.Close`) e inspección documentada de que no hay *jobs* programados.
- [ ] **T-06 — Frontend** · Capa: Frontend (models/controllers/views) · Dificultad: Bajo  
  Descripción: Vitest del modelo (normalización del DTO, estado desconocido); `useCloseTicketController` con `confirming/closing/closed/error`; vista con botón visible solo para `ProductManager`/`Administrator` en `SolutionDelivered` y diálogo "¿Confirmaste por teléfono con el cliente que el caso quedó resuelto? Esta acción cierra el ticket."

## Criterios de aceptación

### CHU-01 — Cierre por la PM

**Dado** un ticket en `SolutionDelivered` y Elizabeth autenticada  
**Cuando** confirma el diálogo y se envía `POST /api/tickets/{ticketId}/close`  
**Entonces** recibe `200`, el ticket queda en `Closed` con `closedAt` igual a la hora del `IClock` y `closedBy` igual a su usuario.

### CHU-02 — Cierre por un administrador asociado y rechazo al no asociado

**Dado** un ticket en `SolutionDelivered`  
**Cuando** lo cierra un administrador participante vigente  
**Entonces** recibe `200`; **y cuando** lo intenta un administrador no asociado, recibe `404` sin datos del ticket y este sigue en `SolutionDelivered` (propuesta; convención de [[hu-020-detalle-interno-del-ticket|HU-020]]).

### CHU-03 — Roles no autorizados

**Dado** un ticket en `SolutionDelivered`  
**Cuando** un desarrollador participante, Julián (Producción) participante o el coordinador de la empresa llaman al endpoint directamente  
**Entonces** reciben `403` y no se crea entrada `TicketClosed`.

### CHU-04 — Estados de origen no permitidos

**Dado** un ticket en `InProduction` y otro ya en `Closed`  
**Cuando** la PM intenta cerrarlos  
**Entonces** recibe `409` en ambos y los estados no cambian (propuesta V-01).

### CHU-05 — Auditoría del cierre

**Dado** un cierre exitoso  
**Cuando** se consulta la bitácora del ticket  
**Entonces** existe una entrada `TicketClosed` con actor, fecha/hora UTC, valor anterior `SolutionDelivered` y valor nuevo `Closed`, y no hay campos con detalles de la llamada.

### CHU-06 — Sin cierre automático

**Dado** un ticket en `SolutionDelivered` desde hace 60 días simulados (`IClock` adelantado)  
**Cuando** arranca la aplicación y transcurre la ejecución de las pruebas de integración  
**Entonces** el ticket sigue en `SolutionDelivered`, no se envía ningún recordatorio y `NoHostedService_DependsOnCloseTicket` pasa.

### CHU-07 — Sin datos de la llamada

**Dado** la PM autenticada  
**Cuando** envía el cierre con cuerpo `{ "notes": "Llamé a las 3 p. m." }`  
**Entonces** recibe `400` y no se guarda ningún texto; la interfaz no ofrece campos de notas.

### CHU-08 — El cliente ve "Cerrado" sin datos internos

**Dado** un ticket cerrado de la empresa A  
**Cuando** el solicitante consulta el detalle del portal  
**Entonces** ve `clientStatus: "Closed"` y la respuesta formal, sin `closedBy`, `previousStatus` ni estado interno; el coordinador de la empresa B recibe `404` sobre ese ticket.

### CHU-09 — Ticket cerrado no admite nueva respuesta

**Dado** un ticket en `Closed`  
**Cuando** la PM intenta emitir una respuesta formal  
**Entonces** recibe `409` ([[hu-033-emitir-respuesta-formal|HU-033]], CHU-06).

### CHU-10 — Interfaz ante error

**Dado** que otra persona cerró el ticket mientras la PM tenía abierto el detalle  
**Cuando** la PM confirma el cierre  
**Entonces** la vista muestra "El ticket ya no puede cerrarse en su estado actual" y recarga el detalle mostrando `Closed`.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-10 validados con evidencia en la matriz.
- [ ] **DoD-02** — Pruebas escritas primero y en verde (`cd backend && dotnet test`): `Close_*`, `CloseTicket_*` e integración del endpoint con PostgreSQL real.
- [ ] **DoD-03** — `DataTicket.ArchitectureTests` en verde, incluida `NoHostedService_DependsOnCloseTicket`.
- [ ] **DoD-04** — Migración `AddTicketClosure` creada y aplicada en local.
- [ ] **DoD-05** — Entrada `TicketClosed` con estado anterior verificada por prueba.
- [ ] **DoD-06** — Frontend: `npm --prefix frontend test`, `npm --prefix frontend run lint` y `npm --prefix frontend run build` correctos.
- [ ] **DoD-07** — `docker compose up --build` y verificación manual del cierre de punta a punta (detalle interno → portal muestra "Cerrado").
- [ ] **DoD-08** — Wiki actualizada: [[estados-del-ticket]] (transición `SolutionDelivered → Closed`), [[flujo-del-ticket]], [[modelo-de-dominio]] (`ClosedAt`/`ClosedBy`), [[auditoria]], [[persistencia-postgresql]]; nota para [[criterios-de-aceptacion]] (CA-12 con prueba).
- [ ] **DoD-09** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO y PR revisado por otra persona del equipo.
- [ ] **DoD-10** — La trazabilidad de la HU y de [[ep-010-respuesta-formal-y-cierre]] está actualizada.

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
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- **Propuesta (V-01):** `Closed` exige `SolutionDelivered` previo; reapertura sin definir.
- **Propuesta:** `ClosedAt`/`ClosedBy` en `Ticket` además de la auditoría, para la métrica "tiempo hasta cierre" sin recorrer la bitácora.
- **Propuesta:** el administrador debe ser participante para cerrar.
- **Pendiente:** si el chat de un ticket cerrado queda en solo lectura (decidir junto con V-01).

## Relacionado

- [[ep-010-respuesta-formal-y-cierre]] · [[tablero-scrum]] · [[flujo-del-ticket]] · [[estados-del-ticket]] · [[auditoria]] · [[modelo-de-dominio]] · [[dashboard-y-metricas]] · [[roles-y-permisos]] · [[criterios-de-aceptacion]]
