---
title: "HU-020 — Detalle interno del ticket"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/ticket, producto/seguridad, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.1", "PRD.md §6.2", "PRD.md §6.3", "PRD.md §6.4", "PRD.md §10", "PRD.md §14"]
aliases: ["HU-020", "Detalle interno del ticket"]
epica: "[[ep-007-trabajo-interno-y-estados]]"
criterios_prd: [CA-03, CA-10]
componentes: [Backend Domain, Backend Application, Backend Infrastructure, Backend Api, Frontend models, Frontend controllers, Frontend views, Persistencia PostgreSQL]
dificultad: "Medio"
sprint_sugerido: "Sprint 3"
dependencias: ["[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-013-radicar-ticket]]", "[[hu-014-adjuntos-en-radicacion]]", "[[hu-017-cola-de-cobertura-y-tomar-ticket]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-019-reasignar-y-retirar-participantes]]"]
relacionadas: ["[[hu-015-ajustar-prioridad-manualmente]]", "[[hu-021-cambiar-estado-interno]]", "[[hu-022-registrar-url-de-pr]]", "[[hu-023-bandeja-de-mis-tickets-y-equipo]]", "[[hu-026-historial-del-chat-por-cursor]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-020 — Detalle interno del ticket

Vista interna completa de un ticket (descripción, adjuntos de radicación, participantes, estado interno, prioridad calculada y ajustada, URL de PR) que solo reciben los participantes vigentes y Elizabeth; cualquier otro usuario interno recibe 404 (PRD §5, §6.2, §10).

## Historia de usuario

**COMO** integrante de Desarrollo o Producción asociado a un ticket, o como Elizabeth (PM)  
**QUIERO** abrir el detalle interno completo del ticket  
**PARA** entender el caso del cliente y su estado real antes de trabajarlo o coordinarlo en el chat

## Contexto

Asignar a un equipo no da acceso: el detalle y el chat son por asociación explícita como `TicketParticipant`, salvo Elizabeth (PRD §6.2 punto 5, §10). El administrador no ve contenido por serlo: en cobertura ve la cola con campos limitados y, al tomar el ticket ([[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]]), queda asociado antes de recibir el detalle (PRD §5 regla 5, §14.3). Un participante retirado pierde el acceso futuro (PRD §5 regla 4). Esta HU es la puerta de entrada de [[hu-021-cambiar-estado-interno|HU-021]], [[hu-022-registrar-url-de-pr|HU-022]] y del chat (EP-009).

## Alcance

- Endpoint interno de lectura del detalle de un ticket.
- Autorización en el caso de uso: rol `ProductManager` siempre; resto de internos solo si son participantes vigentes.
- Datos: número, empresa, solicitante, categoría, título, descripción, urgencia, impacto, prioridad calculada, ajuste de prioridad vigente (si existe), prioridad efectiva, estado interno, `ClientStatus` derivado, URL de PR, participantes vigentes, adjuntos de radicación, fecha de creación.
- Vista interna del detalle en el frontend con estados de carga, vacío y error.

## Fuera de alcance

- Chat del ticket (EP-009) e historial de auditoría ([[hu-012-consultar-bitacora-del-ticket|HU-012]]).
- Acción de tomar el ticket por el admin ([[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]]): aquí solo se exige que la asociación exista.
- Edición de estado, prioridad, participantes o URL de PR (HU-021, HU-015, HU-018/019, HU-022).
- Adjuntos del chat y de la respuesta formal.
- Vista del cliente ([[hu-024-portal-solicitante-consulta-tickets|HU-024]]).

## Requisitos y reglas de negocio

- El acceso al detalle requiere asociación explícita como participante, excepto Elizabeth (PRD §6.2 punto 5, §10).
- Un administrador no obtiene acceso al contenido por ser administrador; debe quedar asociado antes de cargar detalle y adjuntos (PRD §5 regla 5, §6.2 punto 3).
- Retirar un participante revoca su acceso futuro y conserva su participación pasada en la auditoría (PRD §5 regla 4).
- La autorización se aplica en servidor en cada operación (PRD §5 regla 2).
- La URL del PR se muestra a participantes internos autorizados (PRD §6.3).
- La prioridad ajustada conserva la calculada, la nueva, autor, fecha y motivo (PRD §6.1).
- (propuesta) Un ticket inexistente y uno no visible responden igual (404) para no revelar existencia.
- (propuesta) La lista `participants` contiene solo participantes vigentes; los retirados se consultan en la bitácora ([[hu-012-consultar-bitacora-del-ticket|HU-012]]).

## Invariantes en juego

- AGENTS §7.3 — solo participantes vigentes acceden al contenido del ticket; Elizabeth siempre.
- AGENTS §7.5 — el administrador no ve contenido por ser administrador; la asociación precede al detalle.
- AGENTS §7.2 — este endpoint es interno: nunca lo consume el portal del cliente.

## Criterios del PRD cubiertos

- PRD CA-03 (parcial: el detalle solo se entrega tras la asociación; la cola y la toma están en HU-016/HU-017) → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial: los usuarios cliente no pueden invocar el endpoint interno) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-007-trabajo-interno-y-estados]]
- Dependencias: [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]] (`ICurrentUser`, roles), [[hu-013-radicar-ticket|HU-013]] (agregado `Ticket`), [[hu-014-adjuntos-en-radicacion|HU-014]] (adjuntos de radicación), [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] (`TakeTicket`), [[hu-018-asignar-y-agregar-participantes|HU-018]] y [[hu-019-reasignar-y-retirar-participantes|HU-019]] (`TicketParticipant` vigente/retirado).
- Relacionadas: [[hu-015-ajustar-prioridad-manualmente|HU-015]], [[hu-021-cambiar-estado-interno|HU-021]], [[hu-022-registrar-url-de-pr|HU-022]], [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]], [[hu-026-historial-del-chat-por-cursor|HU-026]].
- Decisiones: [[adr-0002-backend-hexagonal]], [[adr-0003-frontend-mvc]], [[adr-0006-urls-publicas-azure-blob]] (URL de adjuntos). Pendientes: V-10, V-16 (formato del número), hallazgo 4 (contexto de `Attachment`).

## Componentes afectados

- Backend Domain: regla de participación vigente en `Ticket`.
- Backend Application: caso de uso de lectura del detalle (nombre propuesto `GetTicketDetail`, ratificar en T-01 y registrar en el glosario), `ITicketRepository`, `ICurrentUser`.
- Backend Infrastructure: consulta EF Core con proyección del detalle.
- Backend Api: endpoint `GET /api/tickets/{ticketId}` con política de usuarios internos.
- Frontend models/controllers/views: módulo interno de tickets (nombre de módulo propuesto `tickets`).
- Persistencia PostgreSQL: solo lectura (sin cambio de esquema si HU-013/HU-014/HU-018 ya crearon las tablas).

## Dificultad

**Nivel:** Medio

**Justificación:** lectura de un solo agregado, pero con reglas de autorización por rol y participación que deben probarse en negativo (interno no asociado, admin no asociado, retirado, cliente) y con dependencia del contexto de `Attachment` (hallazgo 4).

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01.

**`GET /api/tickets/{ticketId}`** — solo usuarios internos autenticados.

Respuesta `200 OK`:

```json
{
  "id": "6f1c2b9e-0d4a-4c1e-9a51-3c2f8e7b1a20",
  "number": "DT-000123",
  "company": { "id": "b3e1...", "name": "Empresa Sintética A" },
  "requester": { "id": "9a7d...", "displayName": "Solicitante A1" },
  "category": { "id": "c-01", "name": "Error en módulo" },
  "title": "No genera la factura electrónica",
  "description": "Desde el lunes el módulo de facturación…",
  "urgency": "High",
  "impact": "Medium",
  "calculatedPriority": "High",
  "priorityOverride": {
    "calculated": "High",
    "newPriority": "Critical",
    "changedBy": { "id": "e11z...", "displayName": "Elizabeth" },
    "changedAt": "2026-10-07T15:20:00Z",
    "reason": "Bloquea la facturación del cliente"
  },
  "effectivePriority": "Critical",
  "status": "InDevelopment",
  "clientStatus": "InProgress",
  "pullRequestUrl": null,
  "participants": [
    { "userId": "4c2d...", "displayName": "Laura", "teams": ["Development"], "addedAt": "2026-10-07T14:10:00Z" }
  ],
  "attachments": [
    { "id": "a1b2...", "fileName": "error.png", "contentType": "image/png", "sizeBytes": 245760, "url": "https://<cuenta>.blob.core.windows.net/..." }
  ],
  "createdAt": "2026-10-07T14:03:00Z"
}
```

- `priorityOverride` es `null` si no hay ajuste; `effectivePriority` = `newPriority` del ajuste vigente o `calculatedPriority`.
- `number`: formato pendiente (V-16); el ejemplo es ilustrativo.
- `attachments`: solo adjuntos de radicación (depende del hallazgo 4).
- [[hu-021-cambiar-estado-interno|HU-021]] añadirá `allowedTransitions` a esta respuesta.

Errores (ProblemDetails, RFC 9457):

| Código | Cuándo |
|---|---|
| 400 | `ticketId` no es un GUID válido |
| 401 | Sin sesión |
| 403 | Usuario cliente (`Requester`, `CompanyCoordinator`): rol insuficiente para el espacio interno |
| 404 | Ticket inexistente, o interno no participante vigente, o administrador no asociado, o participante retirado (propuesta: mismo cuerpo en todos los casos) |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar ruta, DTO `TicketDetail` (nombre propuesto), política de autorización, cuerpo único del 404 y nombre del caso de uso; registrar nombres nuevos en el [[glosario]] y en [[backend-hexagonal]].
- [ ] **T-02 — Regla de participación en el dominio** · Capa: Backend Domain · Dificultad: Bajo  
  Descripción: primero pruebas xUnit de `Ticket` (participante vigente → puede ver; retirado con `RemovedAt` → no puede; usuario ajeno → no puede); luego el método de consulta de participación vigente (nombre propuesto `HasActiveParticipant`).
- [ ] **T-03 — Caso de uso de detalle** · Capa: Backend Application · Dificultad: Medio  
  Descripción: primero pruebas unitarias con dobles de `ITicketRepository` y `ICurrentUser` para PM no participante (ve), participante vigente (ve), interno no asociado (no encontrado), admin no asociado (no encontrado), retirado (no encontrado), ticket inexistente (no encontrado); luego `GetTicketDetail` en `Ports/In` con resultado discriminado (encontrado/no encontrado).
- [ ] **T-04 — Consulta EF Core** · Capa: Backend Infrastructure · Dificultad: Medio  
  Descripción: proyección del detalle con participantes vigentes y adjuntos de radicación, sin cargar mensajes del chat; prueba de integración contra PostgreSQL real que verifica que no aparecen adjuntos de chat ni participantes retirados.
- [ ] **T-05 — Endpoint** · Capa: Backend Api · Dificultad: Bajo  
  Descripción: `GET /api/tickets/{ticketId}` con política de usuario interno; mapeo a 404 ProblemDetails; pruebas con `WebApplicationFactory<Program>` para 200, 401, 403 y los cuatro casos de 404.
- [ ] **T-06 — Modelo y gateway** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: tipos `TicketDetail`, normalización (`TicketStatus` o prioridad desconocida → valor de error explícito) y `ticketDetailGateway` sobre `core/http`; pruebas Vitest de normalización.
- [ ] **T-07 — Controlador** · Capa: Frontend controllers · Dificultad: Bajo  
  Descripción: `useTicketDetailController` con estados `loading | ready | notFound | error`, cancelación con `AbortController` y `retry()`; pruebas Vitest del mapeo 404 → `notFound`.
- [ ] **T-08 — Vista** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `TicketDetailView` pura: muestra prioridad calculada y ajustada (con motivo), estado interno, lista de participantes, adjuntos y "Sin PR registrado" cuando `pullRequestUrl` es `null`.
- [ ] **T-09 — Migración** · Capa: Persistencia · Dificultad: Bajo  
  Descripción: no aplica si HU-013, HU-014 y HU-018 ya crearon tablas y columnas; si falta el contexto del adjunto (hallazgo 4), se añade en la HU dueña, no aquí.

## Criterios de aceptación

### CHU-01 — Participante vigente ve el detalle completo

**Dado** un ticket de la empresa A en `InDevelopment` con Laura como participante vigente, un ajuste de prioridad `High → Critical` y dos adjuntos de radicación  
**Cuando** Laura hace `GET /api/tickets/{ticketId}`  
**Entonces** recibe 200 con `description`, los dos `attachments`, `participants` con Laura, `status = "InDevelopment"`, `clientStatus = "InProgress"`, `calculatedPriority = "High"`, `priorityOverride.newPriority = "Critical"`, `priorityOverride.reason` y `effectivePriority = "Critical"`.

### CHU-02 — Elizabeth ve el detalle sin ser participante

**Dado** un ticket sin Elizabeth en `participants`  
**Cuando** un usuario con rol `ProductManager` pide el detalle  
**Entonces** recibe 200 con el detalle completo.

### CHU-03 — Integrante de equipo no asociado recibe 404

**Dado** Kevin, del equipo `Team.Development`, sin asociación al ticket, aunque Laura (de su mismo equipo) sí es participante  
**Cuando** Kevin pide el detalle  
**Entonces** recibe 404 ProblemDetails cuyo cuerpo no contiene el título, la descripción ni el número del ticket, idéntico al de un `ticketId` inexistente.

### CHU-04 — Administrador no asociado recibe 404 hasta tomar el ticket

**Dado** un administrador que no es participante del ticket  
**Cuando** pide el detalle  
**Entonces** recibe 404; **y** después de tomarlo con [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] la misma petición devuelve 200 y él aparece en `participants`.

### CHU-05 — Participante retirado pierde el acceso

**Dado** Brayan, participante que obtuvo 200 en el detalle  
**Cuando** la PM lo retira ([[hu-019-reasignar-y-retirar-participantes|HU-019]]) y Brayan repite la petición  
**Entonces** recibe 404 y Brayan ya no aparece en `participants` del detalle consultado por la PM.

### CHU-06 — Usuarios cliente no acceden al endpoint interno

**Dado** el solicitante A1 que radicó el ticket y el coordinador de la empresa A  
**Cuando** cualquiera de los dos hace `GET /api/tickets/{ticketId}`  
**Entonces** recibe 403 y el cuerpo no contiene ningún dato del ticket (ni `status`, ni `pullRequestUrl`, ni `participants`).

### CHU-07 — Solo adjuntos de radicación

**Dado** un ticket con un adjunto de radicación y otro adjunto enviado en el chat  
**Cuando** un participante vigente pide el detalle  
**Entonces** `attachments` contiene únicamente el adjunto de radicación.

### CHU-08 — Validación del identificador

**Dado** un usuario interno autenticado  
**Cuando** pide `GET /api/tickets/no-es-un-guid`  
**Entonces** recibe 400 ProblemDetails; **y** sin sesión cualquier petición recibe 401.

### CHU-09 — Comportamiento de la UI ante error y vacío

**Dado** la vista de detalle interno  
**Cuando** el backend responde 404  
**Entonces** la vista muestra "Ticket no disponible o sin acceso" sin datos parciales del ticket; **cuando** responde 500 muestra un error con botón "Reintentar"; **y** con `pullRequestUrl = null` y `priorityOverride = null` muestra "Sin PR registrado" y solo la prioridad calculada.

## Definition of Done

- [ ] CHU-01 a CHU-09 validados con evidencia.
- [ ] Pruebas escritas primero y en verde, nombradas por comportamiento, p. ej. `GetTicketDetail_ParticipanteRetirado_DevuelveNoEncontrado`, `GetTicketDetail_AdminNoAsociado_DevuelveNoEncontrado`, `GetTicketDetail_ProductManagerNoParticipante_DevuelveDetalle`, y de integración `GetTicket_InternoDelMismoEquipoNoAsociado_Responde404` (`cd backend && dotnet test`).
- [ ] La autorización está en el caso de uso (no solo en la política del endpoint), demostrada por pruebas unitarias del caso de uso.
- [ ] `DataTicket.ArchitectureTests` en verde: el caso de uso no referencia EF Core ni ASP.NET Core.
- [ ] Vitest en verde para el modelo y el controlador (`npm --prefix frontend test`); `npm --prefix frontend run lint` sin errores (fronteras MVC) y `npm --prefix frontend run build` correcto.
- [ ] Contrato publicado en OpenAPI (`/openapi/v1.json`) coherente con los tipos del frontend.
- [ ] Sin migración nueva, o migración justificada si faltaba algún campo.
- [ ] El stack levanta con `docker compose up --build` y el detalle se ve desde el frontend con datos sintéticos.
- [ ] Wiki actualizada: [[roles-y-permisos]] (respuesta 404 para no visibles), [[backend-hexagonal]] (caso de uso nuevo), [[frontend-mvc]] (módulo de tickets internos), [[glosario]] (nombres ratificados).
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
| CHU-09 | Pendiente | — | — |
| DoD-01 Pruebas backend nombradas en verde | Pendiente | — | — |
| DoD-02 Autorización en el caso de uso | Pendiente | — | — |
| DoD-03 Pruebas de arquitectura | Pendiente | — | — |
| DoD-04 Vitest, lint y build del frontend | Pendiente | — | — |
| DoD-05 Contrato OpenAPI coherente | Pendiente | — | — |
| DoD-06 Migración (no aplica o justificada) | Pendiente | — | — |
| DoD-07 `docker compose up --build` | Pendiente | — | — |
| DoD-08 Wiki actualizada | Pendiente | — | — |
| DoD-09 `quality-reviewer` | Pendiente | — | — |
| DoD-10 PR revisado | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- (propuesta) 404 en lugar de 403 para internos no asociados, admin no asociado y retirados, para no revelar la existencia del ticket. Los usuarios cliente reciben 403 porque la política de espacio interno los rechaza antes de buscar el ticket. Ratificar en T-01.
- (propuesta) El admin de cobertura que llega desde la cola no llama a este endpoint directamente: primero `TakeTicket` ([[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]]) y luego el detalle.
- ¿Se audita la lectura del detalle o los intentos denegados? No lo exige el PRD §12; depende de V-11.

## Relacionado

- [[ep-007-trabajo-interno-y-estados]] · [[tablero-scrum]] · [[roles-y-permisos]] · [[flujo-del-ticket]] · [[estados-del-ticket]]
- [[matriz-de-prioridad]] · [[archivos-adjuntos]] · [[modelo-de-dominio]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[criterios-de-aceptacion]]
