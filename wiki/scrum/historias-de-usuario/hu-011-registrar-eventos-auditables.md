---
title: "HU-011 — Registrar eventos auditables"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/auditoria, arquitectura/backend, arquitectura/datos]
sources: ["PRD.md §12", "PRD.md §6.1", "PRD.md §6.2", "PRD.md §6.4", "PRD.md §6.5", "PRD.md §5", "PRD.md §14"]
aliases: ["HU-011", "Registrar eventos auditables"]
epica: "[[ep-004-auditoria-append-only]]"
criterios_prd: ["PRD CA-14", "PRD CA-04"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Persistencia PostgreSQL"]
dificultad: "Medio"
sprint_sugerido: "Sprint 1"
dependencias: ["[[hu-001-integracion-continua]]", "[[hu-004-contexto-de-usuario-y-autorizacion]]"]
relacionadas: ["[[hu-012-consultar-bitacora-del-ticket]]", "[[hu-013-radicar-ticket]]", "[[hu-015-ajustar-prioridad-manualmente]]", "[[hu-017-cola-de-cobertura-y-tomar-ticket]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-019-reasignar-y-retirar-participantes]]", "[[hu-021-cambiar-estado-interno]]", "[[hu-033-emitir-respuesta-formal]]", "[[hu-036-cerrar-ticket-manualmente]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-011 — Registrar eventos auditables

Mecanismo único para que cada caso de uso auditable inserte una `AuditEntry` (actor, fecha/hora UTC, acción, objeto, anterior/nuevo, resultado) en la misma transacción que la acción, sin posibilidad de modificarla ni borrarla desde la aplicación (PRD §12).

## Historia de usuario

**COMO** Elizabeth (PM), responsable de la trazabilidad de los tickets  
**QUIERO** que cada acción relevante sobre un ticket quede registrada de forma inmutable con quién, cuándo, qué, sobre qué objeto y con qué valores anterior y nuevo  
**PARA** reconstruir la historia de cualquier ticket y demostrar ante clientes y dirección quién hizo cada cambio, sin depender del correo

## Contexto

El PRD exige una bitácora append-only desde la aplicación con actor, fecha/hora, acción, objeto, valores anterior/nuevo y resultado, y enumera los eventos relevantes (PRD §12). El modelo propuesto ya define `AuditEntry` como agregado inmutable que solo se inserta ([[modelo-de-dominio]]) y el mapa de puertos menciona `IAuditLog` ([[backend-hexagonal]]). Hoy `DataTicketDbContext` está vacío y no hay migraciones ([[persistencia-postgresql]]). Esta HU entrega la infraestructura; cada HU dueña de una acción emite su evento usando este puerto.

## Alcance

- Entidad de dominio `AuditEntry` y catálogo de acciones (enum `AuditAction`, nombre propuesto, ratificar en T-01 y registrar en el glosario).
- Puerto de salida `IAuditLog` en `Application/Ports/Out` con una única operación de inserción (sin actualizar ni borrar).
- Adaptador EF Core que añade la entrada a la misma unidad de trabajo (`DbContext` con ámbito por petición) que la acción, de modo que ambas se confirman o se revierten juntas.
- Tabla de auditoría con migración EF Core, índices por ticket y fecha, y protección append-only en base de datos (propuesta: trigger que rechaza `UPDATE`, `DELETE` y `TRUNCATE`).
- Pruebas de contrato del catálogo que las HU emisoras reutilizan.

## Fuera de alcance

- Consulta y vista de la bitácora: [[hu-012-consultar-bitacora-del-ticket|HU-012 — Consultar la bitácora del ticket]].
- Emisión de cada evento concreto (lo hace la HU dueña de la acción: HU-013, HU-015, HU-017, HU-018, HU-019, HU-021, HU-033, HU-036).
- Auditoría de intentos denegados (depende de V-11).
- Retención, purga y respaldo (PRD §12, §16.3).
- Separación del rol de base de datos de migraciones y de aplicación (decisión de infraestructura, PRD §16.1).
- Frontend: no aplica (no hay superficie de usuario en esta HU; la vista está en HU-012).

## Requisitos y reglas de negocio

- La bitácora es append-only desde la aplicación (PRD §12).
- Campos mínimos: actor, fecha/hora, acción, objeto, valores anterior/nuevo cuando aplique, resultado (PRD §12).
- Eventos relevantes: radicación, cambio de prioridad, cambio de estado, asignación, reasignación, incorporación de participante, retiro de participante, respuesta formal y cierre manual (PRD §12).
- El ajuste de prioridad registra calculada, nueva, quién, cuándo y motivo (PRD §6.1).
- El cierre conserva quién cerró, cuándo y el estado anterior (PRD §6.5).
- La participación histórica de una persona retirada permanece registrada (PRD §5.4, §12).
- La fecha/hora se guarda en UTC (`timestamptz`) y se presenta en `America/Bogota` ([[persistencia-postgresql]]).

Catálogo de acciones (nombres propuestos, ratificar en T-01 y registrar en el glosario):

| Acción (negocio) | `AuditAction` | Objeto | Valor anterior | Valor nuevo | HU emisora |
|---|---|---|---|---|---|
| Radicación | `TicketSubmitted` | `Ticket` | — | número, empresa, solicitante, categoría, urgencia, impacto, prioridad calculada | [[hu-013-radicar-ticket]] |
| Cambio de prioridad | `PriorityOverridden` | `Ticket` | prioridad vigente | nueva prioridad, prioridad calculada, motivo | [[hu-015-ajustar-prioridad-manualmente]] |
| Cambio de estado | `StatusChanged` | `Ticket` | `TicketStatus` anterior | `TicketStatus` nuevo | [[hu-021-cambiar-estado-interno]] |
| Asignación | `TicketAssigned` | `Assignment` | — | persona(s) asignada(s) | [[hu-018-asignar-y-agregar-participantes]] |
| Reasignación | `TicketReassigned` | `Assignment` | responsable anterior | responsable nuevo | [[hu-019-reasignar-y-retirar-participantes]] |
| Incorporación de participante | `ParticipantAdded` | `TicketParticipant` | — | persona, origen (asignación, incorporación o toma en cobertura) | [[hu-017-cola-de-cobertura-y-tomar-ticket]], [[hu-018-asignar-y-agregar-participantes]] |
| Retiro de participante | `ParticipantRemoved` | `TicketParticipant` | persona y fecha de alta | fecha y autor del retiro | [[hu-019-reasignar-y-retirar-participantes]] |
| Respuesta formal | `FormalResponseDelivered` | `FormalResponse` | — | emisor, fecha, número de adjuntos | [[hu-033-emitir-respuesta-formal]] |
| Cierre manual | `TicketClosed` | `Ticket` | estado anterior | `Closed` | [[hu-036-cerrar-ticket-manualmente]] |

## Invariantes en juego

- Invariante 8 (`AGENTS.md` §7): auditoría append-only con actor, fecha, acción, objeto y valores anterior/nuevo.
- Regla hexagonal (`AGENTS.md` §6): `IAuditLog` vive en `Application/Ports/Out`; `Domain` y `Application` no referencian EF Core.

## Criterios del PRD cubiertos

- PRD CA-14 (parcial: provee el mecanismo; se completa con las HU emisoras) → [[criterios-de-aceptacion]]
- PRD CA-04 (parcial: la parte "cada acción queda auditada") → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-004-auditoria-append-only|EP-004 — Auditoría append-only]]
- Dependencias: [[hu-001-integracion-continua|HU-001 — Integración continua]] (pruebas de integración en CI) · [[hu-004-contexto-de-usuario-y-autorizacion|HU-004 — Contexto de usuario y autorización]] (`ICurrentUser` para el actor)
- Relacionadas: [[hu-012-consultar-bitacora-del-ticket|HU-012]] · [[hu-013-radicar-ticket|HU-013]] · [[hu-015-ajustar-prioridad-manualmente|HU-015]] · [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] · [[hu-018-asignar-y-agregar-participantes|HU-018]] · [[hu-019-reasignar-y-retirar-participantes|HU-019]] · [[hu-021-cambiar-estado-interno|HU-021]] · [[hu-033-emitir-respuesta-formal|HU-033]] · [[hu-036-cerrar-ticket-manualmente|HU-036]]
- Decisiones: [[adr-0002-backend-hexagonal]] · V-11 abierto ([[pendientes]])

## Componentes afectados

- Backend (Domain): `AuditEntry`, `AuditAction`.
- Backend (Application): puerto `IAuditLog`.
- Backend (Infrastructure): adaptador EF Core, configuración de entidad, migración.
- Persistencia PostgreSQL: tabla de auditoría, índices, trigger append-only.

## Dificultad

**Nivel:** Medio

**Justificación:** solo backend, pero coordina dominio, puerto, adaptador, transacción compartida y una protección a nivel de base de datos que debe probarse contra PostgreSQL real; define un contrato que consumen ocho HU.

## Contrato backend ↔ frontend

No aplica a REST ni al hub: esta HU no expone endpoints. El contrato que fija es **interno** y lo consume [[hu-012-consultar-bitacora-del-ticket|HU-012]]:

- Puerto (propuesta, ratificar en T-01):

  ```csharp
  public interface IAuditLog
  {
      void Append(AuditEntry entry); // añade a la unidad de trabajo en curso; no hace SaveChanges
  }
  ```

- Forma serializada de `OldValue`/`NewValue`: objeto JSON (`jsonb`) con claves en camelCase y valores de dominio en inglés (p. ej. `{"status":"New"}`, `{"assigneeIds":["…"]}`). Los nombres para mostrar se resuelven al consultar, no se congelan en la entrada (propuesta).
- Columnas propuestas: `id uuid`, `ticket_id uuid null`, `actor_id uuid not null`, `occurred_at timestamptz not null`, `action text not null`, `object_type text not null`, `object_id uuid not null`, `old_value jsonb null`, `new_value jsonb null`, `result text not null`. El modelo actual usa `ObjectRef`; se propone separarlo en tipo + id y añadir `TicketId` para consultar por ticket (actualizar [[modelo-de-dominio]]).
- `Result`: obligatorio; en esta HU solo toma el valor `Succeeded` (V-11).

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar con backend-engineer y quality-reviewer los nombres `AuditAction` y sus valores, la firma de `IAuditLog`, la forma JSON de `OldValue`/`NewValue` por acción y las columnas. Registrar los nombres nuevos en [[glosario]] y el catálogo en [[auditoria]].
- [ ] **T-02 — Domain: `AuditEntry` y `AuditAction`** · Capa: Backend (Domain) · Dificultad: Bajo  
  Descripción: pruebas primero (xUnit v3): `Create_WithoutActor_Throws`, `Create_WithNonUtcTimestamp_Throws`, `Create_WithEmptyObjectId_Throws`, `AuditEntry_HasNoPublicSetters`. Implementar la entidad inmutable (constructor/factoría validada, sin setters públicos).
- [ ] **T-03 — Application: puerto `IAuditLog`** · Capa: Backend (Application) · Dificultad: Bajo  
  Descripción: prueba de arquitectura `IAuditLog_ExposesOnlyAppend` (reflexión: ningún método `Update*`, `Delete*`, `Remove*`). Crear el puerto en `Application/Ports/Out` y un doble en memoria para las pruebas de casos de uso de otras HU.
- [ ] **T-04 — Infrastructure: adaptador EF Core y configuración** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: pruebas de integración primero (`WebApplicationFactory<Program>` + PostgreSQL real): `Append_IsCommittedWithTheActionInSameTransaction`, `Append_IsRolledBackWhenActionFails`, `ActionIsRolledBackWhenAuditInsertFails`. Implementar `IEntityTypeConfiguration<AuditEntry>` (`jsonb`, `timestamptz`, índice `(ticket_id, occurred_at, id)`), adaptador que hace `DbSet.Add` sin `SaveChanges`, y registro en `AddInfrastructure()`.
- [ ] **T-05 — Infrastructure: migración y protección append-only** · Capa: Persistencia PostgreSQL · Dificultad: Medio  
  Descripción: prueba de integración primero `UpdateOrDeleteOnAuditTable_IsRejectedByDatabase` (SQL directo con la conexión de la aplicación). Migración EF Core `AddAuditEntries` que crea la tabla y un trigger `BEFORE UPDATE OR DELETE` (fila) y `BEFORE TRUNCATE` (sentencia) que lanza excepción (propuesta). Documentar en la migración que `REVOKE UPDATE, DELETE` se aplicará cuando exista un rol de aplicación separado.
- [ ] **T-06 — Prueba de contrato del catálogo** · Capa: Backend (Application) · Dificultad: Bajo  
  Descripción: helper de pruebas `AssertAuditEntry(action, actorId, objectType, objectId, oldValue, newValue)` reutilizable por las HU emisoras, más una prueba `AuditAction_ContainsAllPrdEvents` que fija los nueve valores del catálogo.
- [ ] **T-07 — Frontend** · Capa: Frontend · Dificultad: —  
  Descripción: no aplica; la vista de la bitácora es HU-012.

## Criterios de aceptación

### CHU-01 — Entrada completa al ejecutarse una acción auditable

**Dado** un caso de uso que ejecuta una acción del catálogo (en la prueba, un caso de uso de ejemplo con `ICurrentUser` = usuario U y `IClock` fijo en `2026-10-07T15:00:00Z`)  
**Cuando** la acción se confirma  
**Entonces** existe exactamente una fila en la tabla de auditoría con `actor_id` = U, `occurred_at` = `2026-10-07 15:00:00+00`, `action` del catálogo, `object_type` y `object_id` del objeto afectado, `old_value`/`new_value` según la tabla del catálogo y `result` = `Succeeded`.

### CHU-02 — Misma transacción: fallo de la acción

**Dado** un caso de uso que llama a `IAuditLog.Append` y luego falla al confirmar su propio cambio (p. ej. violación de una restricción única)  
**Cuando** la transacción se revierte  
**Entonces** no queda ninguna fila nueva en la tabla de auditoría ni el cambio de la acción.

### CHU-03 — Misma transacción: fallo de la auditoría

**Dado** una acción válida cuya `AuditEntry` no puede insertarse (en la prueba se fuerza un fallo en la inserción de auditoría)  
**Cuando** se intenta confirmar  
**Entonces** la acción tampoco queda persistida y el caso de uso devuelve error; nunca existe una acción auditable confirmada sin su entrada.

### CHU-04 — Append-only en la aplicación

**Dado** el puerto `IAuditLog` y su adaptador  
**Cuando** se inspeccionan por reflexión en `DataTicket.ArchitectureTests`  
**Entonces** solo exponen la operación de inserción (ningún método de actualización o borrado) y `AuditEntry` no tiene setters públicos.

### CHU-05 — Append-only en la base de datos

**Dado** una fila existente en la tabla de auditoría  
**Cuando** se ejecuta `UPDATE`, `DELETE` o `TRUNCATE` sobre la tabla con la cadena de conexión de la aplicación  
**Entonces** PostgreSQL rechaza la sentencia con error y la fila permanece intacta.

### CHU-06 — Validación de la entrada

**Dado** un intento de crear una `AuditEntry` sin actor, con fecha no UTC (`DateTimeKind.Local`), con acción fuera del catálogo o sin objeto  
**Cuando** se invoca la factoría de dominio  
**Entonces** se lanza una excepción de dominio y no se llega a persistir nada.

### CHU-07 — Catálogo completo del PRD

**Dado** el enum `AuditAction`  
**Cuando** se ejecuta la prueba `AuditAction_ContainsAllPrdEvents`  
**Entonces** contiene exactamente los nueve eventos de PRD §12 de la tabla del catálogo (radicación, prioridad, estado, asignación, reasignación, incorporación, retiro, respuesta formal, cierre).

### CHU-08 — Orden estable y consulta por ticket

**Dado** dos entradas del mismo ticket con idéntico `occurred_at`  
**Cuando** se consultan por `ticket_id` ordenadas por `(occurred_at, id)`  
**Entonces** el orden es determinista entre ejecuciones y la migración define el índice `(ticket_id, occurred_at, id)`.

### CHU-09 — Fronteras hexagonales

**Dado** la solución tras añadir `AuditEntry`, `IAuditLog` y el adaptador  
**Cuando** se ejecuta `cd backend && dotnet test`  
**Entonces** las pruebas de `DataTicket.ArchitectureTests` siguen en verde (`Domain` y `Application` sin referencias a EF Core ni Npgsql).

## Definition of Done

- [ ] CHU-01 a CHU-09 validados con evidencia.
- [ ] Pruebas escritas antes de la implementación y en verde: `Create_WithoutActor_Throws`, `Create_WithNonUtcTimestamp_Throws`, `IAuditLog_ExposesOnlyAppend`, `Append_IsCommittedWithTheActionInSameTransaction`, `Append_IsRolledBackWhenActionFails`, `ActionIsRolledBackWhenAuditInsertFails`, `UpdateOrDeleteOnAuditTable_IsRejectedByDatabase`, `AuditAction_ContainsAllPrdEvents` (`cd backend && dotnet test`).
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Migración EF Core `AddAuditEntries` creada en `Infrastructure/Persistence/Migrations`, aplicada contra el PostgreSQL de Compose y reversible (`Down` elimina trigger y tabla).
- [ ] `docker compose up --build` levanta el stack y `/api/health` responde `Healthy` con la migración aplicada.
- [ ] Helper `AssertAuditEntry` publicado en el proyecto de pruebas para las HU emisoras.
- [ ] Wiki: [[auditoria]] (catálogo y forma de valores), [[persistencia-postgresql]] (tabla, trigger, propuesta de roles), [[backend-hexagonal]] (puerto `IAuditLog` implementado), [[modelo-de-dominio]] (`ObjectType`/`ObjectId`/`TicketId`) y [[glosario]] (`AuditAction`) actualizadas mediante Notas para la wiki.
- [ ] `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos.
- [ ] PR revisado y aprobado por otra persona del equipo.
- [ ] La trazabilidad de la HU y de [[ep-004-auditoria-append-only]] está actualizada.

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
| DoD-01 Pruebas TDD en verde | Pendiente | — | — |
| DoD-02 ArchitectureTests | Pendiente | — | — |
| DoD-03 Migración `AddAuditEntries` | Pendiente | — | — |
| DoD-04 `docker compose up --build` | Pendiente | — | — |
| DoD-05 Helper `AssertAuditEntry` | Pendiente | — | — |
| DoD-06 Wiki actualizada | Pendiente | — | — |
| DoD-07 quality-reviewer | Pendiente | — | — |
| DoD-08 PR revisado | Pendiente | — | — |
| DoD-09 Trazabilidad | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- (propuesta) Trigger en lugar de `REVOKE`: el usuario `dataticket` de Compose es propietario de las tablas; la revocación solo es efectiva con un rol de aplicación distinto del de migraciones. Decisión de infraestructura pendiente.
- (propuesta) Los nombres para mostrar (persona, categoría) no se guardan en `old_value`/`new_value`; se resuelven al consultar para no duplicar datos personales en la bitácora.
- V-11: la columna `result` existe desde el inicio para no requerir otra migración cuando se decida si se auditan denegaciones.

## Relacionado

- [[ep-004-auditoria-append-only]] · [[tablero-scrum]] · [[auditoria]] · [[persistencia-postgresql]] · [[backend-hexagonal]]
- [[modelo-de-dominio]] · [[glosario]] · [[estrategia-de-pruebas]] · [[criterios-de-aceptacion]] · [[pendientes]]
