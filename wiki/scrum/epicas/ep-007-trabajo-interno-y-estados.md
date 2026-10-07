---
title: "EP-007 — Trabajo interno y estados"
type: epica
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/epica, producto/ticket, producto/estados]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §6.3", "PRD.md §6.4", "PRD.md §10", "PRD.md §11", "PRD.md §12", "PRD.md §13", "PRD.md §14"]
aliases: ["EP-007", "Trabajo interno y estados"]
fase_prd: "1"
criterios_prd: [CA-03, CA-10, CA-14]
historias: ["[[hu-020-detalle-interno-del-ticket]]", "[[hu-021-cambiar-estado-interno]]", "[[hu-022-registrar-url-de-pr]]"]
dependencias: ["[[ep-002-identidad-y-acceso]]", "[[ep-004-auditoria-append-only]]", "[[ep-005-radicacion-de-tickets]]", "[[ep-006-triage-asignacion-y-participantes]]"]
created: 2026-10-07
updated: 2026-10-07
---

# EP-007 — Trabajo interno y estados

Capacidades del espacio interno para trabajar un ticket una vez asignado: ver su detalle completo (solo participantes vigentes y la PM), moverlo por los estados internos y registrar a mano la URL del pull request, todo auditado y sin filtrar nada técnico al cliente (PRD §6.3, §6.4).

## Objetivo

Que Desarrollo, Producción, la PM y el administrador asociado puedan consultar el detalle interno de un ticket y registrar su avance real (`TicketStatus` y `PullRequestUrl`) con autorización en servidor, auditoría append-only y un `ClientStatus` derivado que nunca revela la etapa técnica.

## Valor esperado

- Trazabilidad del trabajo real del ticket entre Desarrollo y Producción (PRD §4 objetivo 4, §6.4).
- El acceso al detalle queda ligado a la asociación explícita como participante, no a la pertenencia a un equipo (PRD §6.2 punto 5, §10).
- El cliente sigue viendo un estado resumido seguro (`Received`, `InProgress`, `SolutionDelivered`, `Closed`) (PRD §6.4, §14.10).

## Fase del PRD

Fase 1 — Piloto operativo ("estados", "participantes", "auditoría") (PRD §13).

## Actores

- Elizabeth — PM (rol `ProductManager`): ve todos los tickets y puede mover estados (PRD §5).
- Integrantes de Desarrollo (`Team.Development`) y Producción (`Team.Production`) asociados como `TicketParticipant` (PRD §5, §6.3).
- Administrador (`Administrator`) solo cuando está asociado al ticket (tras tomarlo, [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]]) (PRD §5).
- Cliente (`Requester`, `CompanyCoordinator`): actor indirecto; solo percibe el `ClientStatus` mapeado (PRD §6.4).

## Alcance

- Detalle interno del ticket: descripción completa, adjuntos de radicación, participantes vigentes, estado interno, prioridad calculada y ajustada, URL de PR ([[hu-020-detalle-interno-del-ticket|HU-020]]).
- Cambio de estado interno según el diagrama inferido en [[estados-del-ticket]], con auditoría de valor anterior/nuevo ([[hu-021-cambiar-estado-interno|HU-021]]).
- Registro manual de la URL del PR, visible solo a internos autorizados ([[hu-022-registrar-url-de-pr|HU-022]]).

## Fuera de alcance

- Pasar a `SolutionDelivered` (lo hace la respuesta formal, [[hu-033-emitir-respuesta-formal|HU-033]]) y a `Closed` (cierre manual, [[hu-036-cerrar-ticket-manualmente|HU-036]]).
- Integración automática con GitHub/GitLab o sincronización de PR (PRD §6.3, §13).
- Retrocesos de estado y reapertura de tickets cerrados mientras V-01 siga abierta.
- Chat interno (EP-009) y correo al cliente por cambio de estado (PRD §11: no se revela el estado técnico por correo).
- Ajuste de prioridad ([[hu-015-ajustar-prioridad-manualmente|HU-015]], EP-006); aquí solo se muestra.

## Requisitos y reglas de negocio

- El acceso al detalle y al chat es por asociación explícita como participante, excepto Elizabeth (PRD §6.2 punto 5, §10).
- Un administrador no obtiene acceso al contenido por serlo; al tomar un ticket queda asociado antes de cargar el detalle (PRD §5 regla 5).
- Retirar a un participante revoca su acceso futuro (PRD §5 regla 4).
- Los estados internos y su correspondencia con el estado del cliente están fijados en (PRD §6.4); las transiciones no, y se infieren de (PRD §6.2–§6.5).
- Una persona autorizada agrega a mano la URL del PR; la ven participantes internos autorizados (PRD §6.3).
- El cliente nunca ve estados técnicos ni URL de PR (PRD §5 regla 6, §14.10).
- Cambios de estado se auditan con actor y fecha/hora (PRD §12, §14.14).

## Criterios del PRD cubiertos

- PRD CA-03 — (parcial) el detalle solo se entrega tras la asociación; la cola y la toma están en [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] → [[criterios-de-aceptacion]]
- PRD CA-10 — (parcial) estado técnico y URL de PR nunca llegan al portal; se completa con EP-008 y EP-010 → [[criterios-de-aceptacion]]
- PRD CA-14 — (parcial) cambios de estado con actor y fecha/hora en la auditoría → [[criterios-de-aceptacion]]

## Dependencias

- [[ep-002-identidad-y-acceso]] — sesión, `ICurrentUser`, roles (`ProductManager`, `Administrator`) y equipos.
- [[ep-004-auditoria-append-only]] — `IAuditLog` y `AuditEntry` ([[hu-011-registrar-eventos-auditables|HU-011]]).
- [[ep-005-radicacion-de-tickets]] — agregado `Ticket` y adjuntos de radicación ([[hu-013-radicar-ticket|HU-013]], [[hu-014-adjuntos-en-radicacion|HU-014]]).
- [[ep-006-triage-asignacion-y-participantes]] — `TicketParticipant`, `TakeTicket`, retiro de participantes ([[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]], [[hu-018-asignar-y-agregar-participantes|HU-018]], [[hu-019-reasignar-y-retirar-participantes|HU-019]]).
- Decisiones abiertas: V-01, V-02, V-04 en [[pendientes]].

## Historias de usuario

- [[hu-020-detalle-interno-del-ticket|HU-020 — Detalle interno del ticket]] (Sprint 3)
- [[hu-021-cambiar-estado-interno|HU-021 — Cambiar estado interno]] (Sprint 4)
- [[hu-022-registrar-url-de-pr|HU-022 — Registrar URL de PR]] (Sprint 4)

## Criterio de completitud

- [ ] HU-020, HU-021 y HU-022 están `Completada` con su matriz de evidencia.
- [ ] Existe una prueba de integración que demuestra que un interno no asociado, un administrador no asociado y un participante retirado reciben 404 en el detalle (PRD CA-03 parcial).
- [ ] Existe una prueba de dominio que recorre las 36 combinaciones `TicketStatus × TicketStatus` y solo acepta las cuatro transiciones del alcance.
- [ ] Cada cambio de estado y de URL de PR deja un `AuditEntry` con actor, fecha, valor anterior y nuevo (PRD CA-14 parcial).
- [ ] Las respuestas del portal (`/api/portal/tickets*`) no contienen estados técnicos ni la URL del PR (inspección del JSON en pruebas de integración).
- [ ] V-01, V-02 y V-04 están resueltas o explícitamente aceptadas como propuesta por la persona responsable del producto.

## Riesgos e incógnitas

- **V-01 / hallazgo 6:** el diagrama inferido no tiene `PullRequestReview → SolutionDelivered` ni define retrocesos (PR rechazado) o reapertura. Hasta resolverlo, cualquier transición no listada responde 409; si se aprueba otra transición habrá que ampliar HU-021 y [[hu-033-emitir-respuesta-formal|HU-033]].
- **V-02:** no se sabe en qué estado arranca un ticket asignado directamente a Julián; se propone `New → InProduction` explícito.
- **V-04:** quién es la "persona autorizada" para la URL del PR; se propone participantes vigentes + PM + admin asociado.
- **Hallazgo 7:** el glosario no tiene casos de uso para cambiar estado ni registrar la URL de PR; los nombres usados aquí son propuestos (ratificar en T-01 y registrar en el [[glosario]]).
- **Hallazgo 1:** roles y equipos son inconsistentes (Elizabeth es PM y además pertenece a Producción); las reglas de esta épica se basan en rol + participación, nunca en equipo.
- **Hallazgo 4:** `Attachment` no tiene contexto (radicación/chat/respuesta) ni marca interno/externo; HU-020 lo necesita para listar solo adjuntos de radicación.
- **V-10:** diferencia entre "responsable", "asignación" y "participante" sin definir.

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[estados-del-ticket]] · [[flujo-del-ticket]] · [[roles-y-permisos]]
- [[auditoria]] · [[modelo-de-dominio]] · [[backend-hexagonal]] · [[criterios-de-aceptacion]] · [[pendientes]]
- [[ep-006-triage-asignacion-y-participantes]] · [[ep-008-bandejas-y-portal-del-cliente]] · [[ep-010-respuesta-formal-y-cierre]]
