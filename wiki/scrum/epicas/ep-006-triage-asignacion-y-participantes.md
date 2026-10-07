---
title: "EP-006 — Triage, asignación y participantes"
type: epica
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/epica, producto/ticket, producto/roles, producto/seguridad]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §6.3", "PRD.md §7", "PRD.md §10", "PRD.md §12", "PRD.md §14"]
aliases: ["EP-006", "Triage, asignación y participantes"]
fase_prd: "1"
criterios_prd: ["PRD CA-03", "PRD CA-04", "PRD CA-14", "PRD CA-08"]
historias: ["[[hu-016-cola-global-de-triage]]", "[[hu-017-cola-de-cobertura-y-tomar-ticket]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-019-reasignar-y-retirar-participantes]]"]
dependencias: ["[[ep-002-identidad-y-acceso]]", "[[ep-003-empresas-usuarios-y-equipos]]", "[[ep-004-auditoria-append-only]]", "[[ep-005-radicacion-de-tickets]]"]
created: 2026-10-07
updated: 2026-10-07
---

# EP-006 — Triage, asignación y participantes

Elizabeth (PM) revisa en una cola global los tickets nuevos y asigna una o varias personas internas; un administrador cubre el triage con una vista restringida y queda asociado antes de ver el detalle; la PM o un administrador agregan, reasignan y retiran participantes, todo auditado (PRD §5, §6.2).

## Objetivo

Implementar la cola de triage global de la PM, la cola de cobertura del administrador con el caso de uso `TakeTicket`, y la gestión de participantes (`AssignParticipants`, `RemoveParticipant` y reasignación), de forma que el acceso al detalle dependa siempre de la asociación explícita como `TicketParticipant` (salvo la PM) y cada cambio quede auditado.

## Valor esperado

- Ningún ticket nuevo se pierde: todos aparecen en la cola de Elizabeth (PRD §6.2, CA-03).
- La ausencia de Elizabeth no bloquea el triage y no rompe el principio de mínimo privilegio del administrador (PRD §5.5).
- El acceso al detalle y al chat queda gobernado por una lista de participantes explícita y auditable (PRD §6.2.5, CA-04).

## Fase del PRD

Fase 1 — Piloto operativo (PRD §13): "triage, participantes".

## Actores

- Elizabeth — PM (`ProductManager`): triage, asignación, reasignación y gestión de participantes; siempre accede.
- Administrador (`Administrator`): cobertura de triage; agrega y retira participantes; sin acceso automático al contenido.
- Personas de Desarrollo (`Team.Development`) y Producción (`Team.Production`), incluido Julián: reciben asignaciones y participan.

## Alcance

- Cola global de triage y seguimiento para la PM (`TriageQueue`).
- Cola de cobertura del administrador con los nueve campos enumerados por el PRD y caso de uso `TakeTicket` (asociar y auditar antes de devolver el detalle).
- Asignación de una o varias personas internas, incluida la asignación directa a Julián; incorporación de participantes.
- Retiro de participantes (marca `RemovedAt`/`RemovedBy`, no borra) y reasignación con valores anterior/nuevo auditados.
- Listado de participantes del ticket para internos autorizados.

## Fuera de alcance

- Cambio de estado interno al asignar (incluida la ruta directa a `InProduction`): [[hu-021-cambiar-estado-interno|HU-021]] (V-01, V-02).
- Detalle interno completo del ticket: [[hu-020-detalle-interno-del-ticket|HU-020]] (consume el control de acceso definido aquí).
- Correo de vinculación: [[hu-038-correo-de-vinculacion|HU-038]].
- Revocación de conexiones activas del chat: [[hu-032-revocar-acceso-al-retirar-participante|HU-032]].
- Bandejas por equipo y responsabilidad: [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]].
- Mecanismo para declarar la "ausencia" de Elizabeth (V-05 abierto).
- Automatizaciones de asignación o escalamiento (PRD §13).

## Requisitos y reglas de negocio

- Al radicarse, el ticket entra en la cola general de Elizabeth con estado `New` (PRD §6.2.1).
- Elizabeth asigna una o varias personas; puede asignar directamente a Julián (PRD §6.2.2, §6.3).
- El administrador de cobertura ve solo número, empresa, título, categoría, urgencia, impacto, prioridad calculada, fecha de creación y estado resumido; no descripción, archivos ni chat; al abrir/tomar queda asociado antes de cargar el detalle (PRD §5.5, §6.2.3, §10).
- Elizabeth o un administrador puede añadir o retirar participantes durante toda la vida del ticket; cada cambio se audita (PRD §5.4, §6.2.4).
- Asignar a un equipo no sustituye la lista de participantes; el acceso es por asociación explícita, excepto Elizabeth (PRD §6.2.5, §10).
- Agregar da acceso al historial completo; retirar revoca el acceso futuro y conserva la participación pasada (PRD §5.4, §7.3, §7.5).

## Criterios del PRD cubiertos

- PRD CA-03 (total, entre [[hu-016-cola-global-de-triage|HU-016]] y [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]]) → [[criterios-de-aceptacion]]
- PRD CA-04 (total, con la auditoría de [[ep-004-auditoria-append-only]]) → [[criterios-de-aceptacion]]
- PRD CA-14 (parcial: participantes y asignación) → [[criterios-de-aceptacion]]
- PRD CA-08 (parcial: el modelo de participación; la consulta del historial del chat la cubren HU-026 y HU-032) → [[criterios-de-aceptacion]]

## Dependencias

- [[ep-002-identidad-y-acceso]] — roles `ProductManager`/`Administrator` y `ICurrentUser` ([[hu-004-contexto-de-usuario-y-autorizacion|HU-004]]).
- [[ep-003-empresas-usuarios-y-equipos]] — usuarios internos y equipos ([[hu-009-administrar-usuarios-internos-y-equipos|HU-009]]).
- [[ep-004-auditoria-append-only]] — `IAuditLog` ([[hu-011-registrar-eventos-auditables|HU-011]]).
- [[ep-005-radicacion-de-tickets]] — tickets radicados ([[hu-013-radicar-ticket|HU-013]]).

## Historias de usuario

- [[hu-016-cola-global-de-triage|HU-016 — Cola global de triage]] · Sprint 2
- [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017 — Cola de cobertura y tomar ticket]] · Sprint 3
- [[hu-018-asignar-y-agregar-participantes|HU-018 — Asignar y agregar participantes]] · Sprint 3
- [[hu-019-reasignar-y-retirar-participantes|HU-019 — Reasignar y retirar participantes]] · Sprint 3

## Criterio de completitud

- [ ] HU-016, HU-017, HU-018 y HU-019 están `Completada`.
- [ ] Prueba de integración que demuestra que la respuesta de la cola de cobertura no contiene descripción, adjuntos ni chat.
- [ ] Prueba que demuestra que `TakeTicket` persiste el `TicketParticipant` y su `AuditEntry` antes de leer el detalle.
- [ ] Pruebas de autorización por rol (cliente, Desarrollo, Producción, administrador, PM) sobre cada endpoint de la épica.
- [ ] Cada asignación, reasignación, incorporación y retiro deja `AuditEntry` con actor, fecha, acción, objeto y anterior/nuevo.
- [ ] PRD CA-03 y CA-04 pueden marcarse en [[criterios-de-aceptacion]] con evidencia.
- [ ] No quedan dependencias bloqueantes dentro del alcance.

## Riesgos e incógnitas

- **V-05** — cómo se determina la "ausencia" de Elizabeth: se propone que la cola de cobertura esté siempre disponible para administradores hasta decidir.
- **V-10** — diferencia entre responsable, asignación y participante: se propone que asignar implique participar; la reasignación depende de esta definición.
- **V-02** — estado inicial de la ruta directa a Julián: esta épica no cambia estados.
- **V-03** — orden de las colas (prioridad calculada o ajustada).
- **V-08** — correo de vinculación al administrador que toma un ticket y a quien es reincorporado.
- **Hallazgo 1** — roles de Identity `Development`/`Production` frente a equipos `Team.Development`/`Team.Production`: define quién es "persona interna asignable"; resolver antes de HU-018.
- **Hallazgo 3** — "área" no existe en el modelo; la cola no la muestra.
- **Hallazgo 7** — no hay caso de uso de reasignación en el mapa de puertos; HU-019 propone uno.

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[flujo-del-ticket]] · [[roles-y-permisos]] · [[elizabeth-pm]] · [[julian-produccion]]
- [[auditoria]] · [[modelo-de-dominio]] · [[backend-hexagonal]] · [[criterios-de-aceptacion]] · [[pendientes]]
- [[ep-004-auditoria-append-only]] · [[ep-005-radicacion-de-tickets]] · [[ep-007-trabajo-interno-y-estados]] · [[ep-009-chat-interno-en-tiempo-real]] · [[ep-011-notificaciones]]
