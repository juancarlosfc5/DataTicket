---
title: "HU-023 — Bandeja de mis tickets y de equipo"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/dashboard, producto/seguridad, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §6.4", "PRD.md §10", "PRD.md §11"]
aliases: ["HU-023", "Bandeja de mis tickets y de equipo", "Inbox"]
epica: "[[ep-008-bandejas-y-portal-del-cliente]]"
criterios_prd: [CA-03]
componentes: [Backend Application, Backend Infrastructure, Backend Api, Frontend models, Frontend controllers, Frontend views, Persistencia PostgreSQL]
dificultad: "Medio"
sprint_sugerido: "Sprint 4"
dependencias: ["[[hu-009-administrar-usuarios-internos-y-equipos]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-019-reasignar-y-retirar-participantes]]", "[[hu-020-detalle-interno-del-ticket]]"]
relacionadas: ["[[hu-016-cola-global-de-triage]]", "[[hu-017-cola-de-cobertura-y-tomar-ticket]]", "[[hu-021-cambiar-estado-interno]]", "[[hu-041-panel-del-equipo-interno]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-023 — Bandeja de mis tickets y de equipo

Dos listas de trabajo internas (`Inbox`): "mis tickets", con los tickets donde el usuario es participante vigente, y la bandeja de su equipo, que muestra solo campos de lista y no da acceso al detalle sin asociación; ambas filtrables por estado interno (PRD §10).

## Historia de usuario

**COMO** integrante de Desarrollo o Producción  
**QUIERO** ver los tickets en los que participo y los que lleva mi equipo, filtrados por estado interno  
**PARA** organizar mi trabajo diario sin depender de que la PM me avise de cada ticket

## Contexto

"Desarrollo y Producción usan bandejas por equipo y responsabilidad. El detalle/chat requiere asociación al ticket, además de la pertenencia al equipo" (PRD §10). El cambio de estado interno "se refleja en las bandejas internas" (PRD §11). El PRD no define qué campos muestra la bandeja de equipo ni qué significa "responsabilidad" (V-10, hallazgo 1). Se propone reutilizar los campos de la cola de cobertura del administrador (PRD §5 regla 5), que ya son el estándar del PRD para "ver sin acceder al contenido", más el estado interno. `Inbox` y `TriageQueue` son modelos de lectura ([[modelo-de-dominio]]).

## Alcance

- `GET` de "mis tickets": tickets con el usuario como `TicketParticipant` vigente.
- `GET` de bandeja de equipo: tickets con al menos un participante vigente del equipo consultado (propuesta), solo si el usuario pertenece a ese equipo.
- Filtro por uno o varios `TicketStatus`, paginación y total.
- Vistas de ambas bandejas; las filas no asociadas de la bandeja de equipo no enlazan al detalle.

## Fuera de alcance

- Bandeja global y cola de triage de la PM ([[hu-016-cola-global-de-triage|HU-016]]) y cola de cobertura del admin ([[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]]).
- Autoasociarse a un ticket desde la bandeja de equipo (no está en el PRD; la asociación la hacen la PM o un administrador, PRD §5 regla 4).
- Métricas y paneles ([[hu-041-panel-del-equipo-interno|HU-041]]).
- Orden por prioridad (V-03), vistas guardadas y exportación (PRD §10).
- Actualización en tiempo real de la bandeja (no exigida; se recarga al consultar).

## Requisitos y reglas de negocio

- Bandejas por equipo y responsabilidad para Desarrollo y Producción (PRD §10).
- Pertenecer al equipo no da acceso al detalle ni al chat (PRD §6.2 punto 5, §10).
- Retirar un participante revoca su acceso futuro (PRD §5 regla 4): el ticket sale de su "mis tickets".
- El cambio de estado interno se refleja en las bandejas internas (PRD §11).
- (propuesta) Campos de la bandeja de equipo: número, empresa, título, categoría, urgencia, impacto, prioridad calculada, prioridad efectiva, estado interno, fecha de creación e indicador `isParticipant`. Sin descripción, adjuntos, participantes ni URL de PR.
- (propuesta) Orden: fecha de creación descendente, hasta resolver V-03.
- (propuesta) Paginación `page ≥ 1`, `pageSize` entre 1 y 100 (por defecto 25).

## Invariantes en juego

- AGENTS §7.3 — la pertenencia al equipo no sustituye la participación.
- AGENTS §7.5 — el administrador solo ve en "mis tickets" los tickets que ya tomó o a los que fue asociado.

## Criterios del PRD cubiertos

- PRD CA-03 (parcial: la vista limitada sin contenido antes de la asociación se extiende a la bandeja de equipo; la cola global y la de cobertura están en HU-016/HU-017) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-008-bandejas-y-portal-del-cliente]]
- Dependencias: [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]] (equipos), [[hu-018-asignar-y-agregar-participantes|HU-018]] y [[hu-019-reasignar-y-retirar-participantes|HU-019]] (participación vigente), [[hu-020-detalle-interno-del-ticket|HU-020]] (enlace al detalle).
- Relacionadas: [[hu-016-cola-global-de-triage|HU-016]], [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]], [[hu-021-cambiar-estado-interno|HU-021]], [[hu-041-panel-del-equipo-interno|HU-041]].
- Decisiones: V-03, V-10 y hallazgo 1 en [[pendientes]].

## Componentes afectados

- Backend Application: consultas de bandeja (nombres propuestos `ListMyInbox`, `ListTeamInbox` y puerto `IInboxQueries`, ratificar en T-01 y registrar en el glosario), `ICurrentUser`.
- Backend Infrastructure: consultas EF Core paginadas con proyección de campos de lista.
- Backend Api: `GET /api/inbox/mine`, `GET /api/inbox/team`.
- Frontend models/controllers/views: módulo de bandejas internas.
- Persistencia PostgreSQL: índices para `TicketParticipant(UserId, RemovedAt)` y `Ticket(Status, CreatedAt)` (propuesta).

## Dificultad

**Nivel:** Medio

**Justificación:** consultas de lectura con filtros y paginación, pero la regla de "ver sin acceder" debe garantizarse por proyección y probarse inspeccionando el JSON, y la definición de "bandeja de equipo" depende de decisiones abiertas.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01.

**`GET /api/inbox/mine?status=InDevelopment&status=PullRequestReview&page=1&pageSize=25`**

**`GET /api/inbox/team?team=Development&status=New&page=1&pageSize=25`** (`team` ∈ {`Development`, `Production`})

Respuesta `200 OK` (ambas):

```json
{
  "items": [
    {
      "id": "6f1c2b9e-0d4a-4c1e-9a51-3c2f8e7b1a20",
      "number": "DT-000123",
      "company": { "id": "b3e1...", "name": "Empresa Sintética A" },
      "title": "No genera la factura electrónica",
      "category": { "id": "c-01", "name": "Error en módulo" },
      "urgency": "High",
      "impact": "Medium",
      "calculatedPriority": "High",
      "effectivePriority": "Critical",
      "status": "InDevelopment",
      "createdAt": "2026-10-07T14:03:00Z",
      "isParticipant": true
    }
  ],
  "page": 1,
  "pageSize": 25,
  "totalCount": 1
}
```

Errores (ProblemDetails):

| Código | Cuándo |
|---|---|
| 400 | `status` fuera de `TicketStatus`; `page < 1`; `pageSize` 0 o mayor que 100; `team` desconocido |
| 401 | Sin sesión |
| 403 | Usuario cliente; o `team` al que el usuario no pertenece |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar rutas, campos de lista, definición operativa de "bandeja de equipo", orden y límites de paginación; registrar `ListMyInbox`, `ListTeamInbox` e `IInboxQueries` en el [[glosario]].
- [ ] **T-02 — Casos de uso** · Capa: Backend Application · Dificultad: Medio  
  Descripción: primero pruebas unitarias con dobles de `IInboxQueries` y `ICurrentUser`: usuario cliente → prohibido; equipo ajeno → prohibido; validación de `page`/`pageSize`/`status`; luego los casos de uso.
- [ ] **T-03 — Consultas EF Core** · Capa: Backend Infrastructure · Dificultad: Medio  
  Descripción: proyección directa al DTO de lista (sin cargar descripción ni adjuntos); pruebas de integración con PostgreSQL real: participante retirado excluido, filtro múltiple de estado, `totalCount` correcto, `isParticipant` correcto.
- [ ] **T-04 — Índices y migración** · Capa: Persistencia · Dificultad: Bajo  
  Descripción: migración EF Core con los índices propuestos si no existen; revisar el plan con datos sintéticos de [[hu-010-datos-sinteticos-de-desarrollo|HU-010]].
- [ ] **T-05 — Endpoints** · Capa: Backend Api · Dificultad: Bajo  
  Descripción: `GET /api/inbox/mine` y `GET /api/inbox/team`; pruebas de integración de 200/400/401/403 y de la lista de claves del JSON.
- [ ] **T-06 — Modelos y gateway** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: tipos `InboxItem`, `InboxPage`, construcción de la query (`status` repetido) y normalización; pruebas Vitest.
- [ ] **T-07 — Controladores** · Capa: Frontend controllers · Dificultad: Bajo  
  Descripción: `useInboxController` con pestaña (`mine | team`), filtros de estado, paginación y estados `loading | ready | empty | error`; pruebas Vitest.
- [ ] **T-08 — Vistas** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `InboxView` con tabla, filtros por estado (etiquetas en español) y paginador; las filas con `isParticipant = false` se muestran sin enlace y con la marca "Requiere asociación".

## Criterios de aceptación

### CHU-01 — "Mis tickets" lista solo la participación vigente

**Dado** Laura, participante vigente de T1 y T2, retirada de T3 y sin relación con T4  
**Cuando** pide `GET /api/inbox/mine`  
**Entonces** recibe 200 con T1 y T2, `totalCount = 2` y `isParticipant = true` en ambos.

### CHU-02 — Filtro por estado interno

**Dado** T1 en `InDevelopment` y T2 en `PullRequestReview`, ambos de Laura  
**Cuando** pide `?status=PullRequestReview`  
**Entonces** recibe solo T2; **y** con `?status=InDevelopment&status=PullRequestReview` recibe ambos.

### CHU-03 — Bandeja de equipo sin contenido

**Dado** T5, con Brayan (Desarrollo) como participante vigente y sin Laura  
**Cuando** Laura (Desarrollo) pide `GET /api/inbox/team?team=Development`  
**Entonces** T5 aparece con `isParticipant = false` y el JSON del ítem tiene exactamente las claves del contrato: no contiene `description`, `attachments`, `participants` ni `pullRequestUrl`; **y** `GET /api/tickets/{T5}` por Laura sigue respondiendo 404 ([[hu-020-detalle-interno-del-ticket|HU-020]]).

### CHU-04 — Validación de parámetros con valores límite

**Dado** un usuario interno  
**Cuando** pide `pageSize=0`, `pageSize=101`, `page=0` o `status=Deployed`  
**Entonces** recibe 400 ProblemDetails; **y** con `pageSize=100` y `page=1` recibe 200.

### CHU-05 — Autorización por rol y equipo

**Dado** Julián, que pertenece solo a `Team.Production`  
**Cuando** pide `?team=Development`  
**Entonces** recibe 403; **cuando** un solicitante o coordinador llama a cualquiera de las dos rutas recibe 403; **y** un administrador sin equipo recibe en "mis tickets" solo los tickets que tomó.

### CHU-06 — El cambio de estado se refleja

**Dado** T1 de Laura en `InDevelopment`  
**Cuando** se mueve a `PullRequestReview` ([[hu-021-cambiar-estado-interno|HU-021]]) y Laura recarga su bandeja filtrada por `InDevelopment`  
**Entonces** T1 ya no aparece; **y** con el filtro `PullRequestReview` sí aparece.

### CHU-07 — UI vacía y con error

**Dado** un usuario sin tickets asociados  
**Cuando** abre "Mis tickets"  
**Entonces** ve "No tienes tickets asignados" (no una tabla vacía); **si** el backend responde 500 ve un error con "Reintentar"; **y** al hacer clic en una fila "Requiere asociación" no se navega al detalle.

## Definition of Done

- [ ] CHU-01 a CHU-07 validados con evidencia.
- [ ] Pruebas en verde (`cd backend && dotnet test`): `ListMyInbox_ExcluyeParticipacionesRetiradas`, `ListTeamInbox_EquipoAjeno_Prohibido`, integración `TeamInbox_ItemNoContieneCamposDeDetalle` (lista de claves permitidas).
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Migración EF Core de índices creada y revisada (si aplica).
- [ ] Vitest, `npm --prefix frontend run lint` y `npm --prefix frontend run build` en verde.
- [ ] El stack levanta con `docker compose up --build` y ambas bandejas muestran datos sintéticos.
- [ ] Wiki actualizada: [[dashboard-y-metricas]] (bandejas por equipo: campos y regla), [[roles-y-permisos]] (bandeja de equipo sin contenido), [[glosario]] (nombres ratificados), [[frontend-mvc]] (módulo nuevo).
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
| DoD-01 Pruebas backend nombradas | Pendiente | — | — |
| DoD-02 Pruebas de arquitectura | Pendiente | — | — |
| DoD-03 Migración de índices | Pendiente | — | — |
| DoD-04 Vitest, lint y build | Pendiente | — | — |
| DoD-05 `docker compose up --build` | Pendiente | — | — |
| DoD-06 Wiki actualizada | Pendiente | — | — |
| DoD-07 `quality-reviewer` | Pendiente | — | — |
| DoD-08 PR revisado | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- (propuesta) "Bandeja de equipo" = tickets con al menos un participante vigente del equipo. Alternativas: por asignación (`Assignment`) o por categoría. Depende de V-10 y del hallazgo 1 (Elizabeth es PM y pertenece a Producción: en la bandeja de Producción verá los tickets donde participa, pero su vista principal es la global de HU-016).
- (propuesta) Los campos de lista replican la cola de cobertura (PRD §5 regla 5) más estado interno y prioridad efectiva; confirmar con la PM.

## Relacionado

- [[ep-008-bandejas-y-portal-del-cliente]] · [[tablero-scrum]] · [[dashboard-y-metricas]] · [[roles-y-permisos]] · [[estados-del-ticket]]
- [[equipo-data-global]] · [[modelo-de-dominio]] · [[frontend-mvc]] · [[criterios-de-aceptacion]] · [[pendientes]]
