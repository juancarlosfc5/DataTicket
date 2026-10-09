---
title: "HU-026 — Historial del chat paginado por cursor"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/chat, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §7", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-026", "Historial del chat paginado por cursor"]
epica: "[[ep-009-chat-interno-en-tiempo-real]]"
criterios_prd: ["CA-05", "CA-06", "CA-08"]
componentes: ["Backend (Domain, Application, Infrastructure, Api)", "Persistencia PostgreSQL", "Frontend (models, controllers, views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 5"
dependencias: ["[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-019-reasignar-y-retirar-participantes]]", "[[hu-020-detalle-interno-del-ticket]]"]
relacionadas: ["[[hu-027-unirse-al-chat-del-ticket]]", "[[hu-028-enviar-y-recibir-mensajes]]", "[[hu-029-adjuntos-e-imagenes-en-chat]]", "[[hu-030-confirmaciones-de-lectura]]", "[[hu-031-recuperar-mensajes-tras-reconexion]]", "[[hu-032-revocar-acceso-al-retirar-participante]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-026 — Historial del chat paginado por cursor

Un participante vigente (o Elizabeth) consulta el historial completo del chat de un ticket desde PostgreSQL, paginado por cursor hacia atrás y hacia adelante. Esta HU crea la entidad `ChatMessage`, fija el DTO de mensaje que reutiliza todo el chat y entrega la API que usan la carga inicial y la recuperación tras reconexión (PRD §7).

## Historia de usuario

**COMO** participante interno vigente de un ticket (o Elizabeth, PM)  
**QUIERO** consultar el historial completo del chat del ticket, por páginas y en orden estable  
**PARA** entender lo conversado antes de mi incorporación y no perder mensajes al volver a abrir el ticket

## Contexto

El PRD exige que quien se incorpora tarde vea el historial completo, mensajes y adjuntos incluidos (PRD §5.4, §7.3, §14.8), y que el historial viva en PostgreSQL, no en SignalR (PRD §7.2). El flujo recomendado empieza con una consulta de historial por API que devuelve "su cursor más reciente" (PRD §7, flujo 1), y la misma API sirve para recuperar mensajes tras reconectar (PRD §7, flujo 6). El PRD deja el esquema JSON para el diseño de API (PRD §7, *Interfaces técnicas propuestas*), y la wiki no define el formato del cursor (hallazgo 8; [[tiempo-real-signalr]] solo menciona `?after=<cursor>`). Esta HU lo propone en T-01.

## Alcance

- Entidad de dominio `ChatMessage` (inmutable; propuesta del [[modelo-de-dominio]]) con `Id`, `TicketId`, `AuthorId`, `Body` y `CreatedAt`, y migración EF Core de su tabla con índice `(TicketId, CreatedAt, Id)`.
- Regla de acceso al chat reutilizable por todos los casos de uso del chat: participante vigente **o** rol `ProductManager`.
- Caso de uso `GetChatHistory` y endpoint `GET /api/tickets/{ticketId}/messages` con `after`, `before` y `limit`.
- DTO `ChatMessageDto` completo (con `attachments` y `readReceipts`, vacíos hasta HU-029 y HU-030), para que el contrato no cambie en las HU siguientes.
- Frontend: modelo, gateway REST, controlador y vista de la lista de mensajes con "cargar anteriores".

## Fuera de alcance

- Envío de mensajes y tiempo real ([[hu-027-unirse-al-chat-del-ticket|HU-027]], [[hu-028-enviar-y-recibir-mensajes|HU-028]]).
- Subida y vista previa de adjuntos ([[hu-029-adjuntos-e-imagenes-en-chat|HU-029]]); aquí solo se reserva el campo `attachments` en el DTO.
- Lecturas ([[hu-030-confirmaciones-de-lectura|HU-030]]); aquí solo se reserva `readReceipts`.
- Lógica de reconexión del cliente ([[hu-031-recuperar-mensajes-tras-reconexion|HU-031]]); esta HU entrega el `after` que esa HU usa.
- Búsqueda o filtros dentro del chat.

## Requisitos y reglas de negocio

- Solo participantes vigentes leen el chat; Elizabeth siempre puede (PRD §5.3).
- Agregar a alguien le da acceso al historial completo (PRD §5.4, §7.3, §14.8).
- Una persona retirada no puede volver a consultar el historial (PRD §7.5, §14.8).
- Un administrador no ve el chat por serlo; solo si está asociado (PRD §5.5).
- El historial se consulta desde PostgreSQL, paginado por cursor (PRD §7, *Interfaces técnicas propuestas*).
- Validación de pertenencia al ticket en todas las rutas de chat e historial (PRD §12).

## Invariantes en juego

- AGENTS §7.2: el cliente nunca ve el chat interno.
- AGENTS §7.3: solo participantes vigentes leen el chat; Elizabeth siempre puede; se valida en el servidor.
- AGENTS §7.5: el administrador no ve contenido por ser administrador.

## Criterios del PRD cubiertos

- PRD CA-06 (parcial: "no puede cargar historial") → [[criterios-de-aceptacion]]
- PRD CA-08 (parcial: el incorporado tarde ve el historial completo; el retirado no puede consultarlo) → [[criterios-de-aceptacion]]
- PRD CA-05 (parcial: recuperación desde PostgreSQL mediante `after`; el flujo completo está en HU-031) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-009-chat-interno-en-tiempo-real]]
- Dependencias: [[hu-004-contexto-de-usuario-y-autorizacion|HU-004 — Contexto de usuario y autorización]] (`ICurrentUser`, roles), [[hu-018-asignar-y-agregar-participantes|HU-018 — Asignar y agregar participantes]] y [[hu-019-reasignar-y-retirar-participantes|HU-019 — Reasignar y retirar participantes]] (`TicketParticipant` vigente o retirado), [[hu-020-detalle-interno-del-ticket|HU-020 — Detalle interno del ticket]] (pantalla donde se monta el chat).
- Relacionadas: [[hu-027-unirse-al-chat-del-ticket|HU-027]], [[hu-028-enviar-y-recibir-mensajes|HU-028]], [[hu-029-adjuntos-e-imagenes-en-chat|HU-029]], [[hu-030-confirmaciones-de-lectura|HU-030]], [[hu-031-recuperar-mensajes-tras-reconexion|HU-031]], [[hu-032-revocar-acceso-al-retirar-participante|HU-032]], [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] (el admin queda asociado al tomar el ticket).
- Decisiones: [[adr-0005-signalr-para-chat]] (aceptada), [[adr-0002-backend-hexagonal]], [[adr-0003-frontend-mvc]]. Pendientes: formato del cursor (se propone aquí); Row-Level Security y enrutador del frontend ([[pendientes]] §4).

## Componentes afectados

- Backend Domain: `ChatMessage`.
- Backend Application: `Ports/In` → `GetChatHistory`; `Ports/Out` → `IChatRepository` (según el mapa de [[backend-hexagonal]]); regla de acceso al chat (`ChatAccessPolicy`, nombre propuesto: ratificar en T-01 y registrar en el glosario).
- Backend Infrastructure: `Persistence/Configurations/ChatMessageConfiguration`, adaptador `IChatRepository` con EF Core y migración `AddChatMessages` (nombre propuesto).
- Backend Api: `Endpoints/` → `GET /api/tickets/{ticketId}/messages`.
- Persistencia PostgreSQL: tabla de mensajes.
- Frontend: `src/modules/chat/{models,controllers,views}` (módulo propuesto).

## Dificultad

**Nivel:** Medio

**Justificación:** combina entidad, migración, caso de uso con autorización por participante y rol, paginación por cursor con orden estable y una vista sencilla. No toca el hub. La precisión del cursor (marca de tiempo frente a desempate por `Id`) exige pruebas cuidadosas.

## Contrato backend ↔ frontend

> [!info] Propuesta
> Todo este contrato es una propuesta. Se ratifica en T-01 y se documenta en [[tiempo-real-signalr]]. El DTO `ChatMessageDto` es el mismo que viaja en el evento `MessageCreated` ([[hu-028-enviar-y-recibir-mensajes|HU-028]]).

**Ruta:** `GET /api/tickets/{ticketId}/messages`

| Parámetro | Tipo | Regla |
|---|---|---|
| `after` | cursor opaco | Mensajes estrictamente posteriores al cursor, en orden ascendente. Para la recuperación tras reconexión |
| `before` | cursor opaco | Los `limit` mensajes inmediatamente anteriores al cursor, devueltos en orden ascendente. Para "cargar anteriores" |
| (ninguno) | — | Los `limit` mensajes más recientes, en orden ascendente (carga inicial) |
| `limit` | entero | Por defecto 50; mínimo 1 y máximo 100 (propuesta) |

- `after` y `before` son excluyentes: enviar ambos devuelve 400.
- **Orden estable** por `(CreatedAt, Id)` ascendente.
- **Cursor** (propuesta): token opaco en base64url que codifica `CreatedAt` (UTC, con precisión de microsegundos) y el `Id` del mensaje, de 100 caracteres como máximo. El cliente no lo interpreta. `CreatedAt` se trunca a microsegundos al crearse (`IClock`), para que coincida con `timestamptz` de PostgreSQL. `Id` es un Guid v7 (`Guid.CreateVersion7()`, propuesta).
- Encabezado `Cache-Control: no-store` en la respuesta, por tratarse de contenido interno (propuesta).

**Respuesta 200:**

```json
{
  "items": [
    {
      "id": "01928c1e-7f3a-7b2e-9c41-5d2f0a8e4b10",
      "ticketId": "01928b77-1c2d-7e0f-a3b4-c5d6e7f80912",
      "author": { "id": "01928a00-0000-7000-8000-000000000003", "displayName": "Laura" },
      "body": "Ya reproduje el error en el ambiente de pruebas.",
      "createdAt": "2026-10-07T15:04:05.123456Z",
      "cursor": "MjAyNi0xMC0wN1QxNTowNDowNS4xMjM0NTZafDAxOTI4YzFl",
      "attachments": [],
      "readReceipts": []
    }
  ],
  "hasOlder": true,
  "hasNewer": false
}
```

- `attachments` se llena desde [[hu-029-adjuntos-e-imagenes-en-chat|HU-029]] (`{ attachmentId, fileName, contentType, sizeBytes, url }`, misma forma que en [[hu-033-emitir-respuesta-formal|HU-033]]).
- `readReceipts` se llena desde [[hu-030-confirmaciones-de-lectura|HU-030]] (`{ userId, displayName, readAt }`).
- Si la página está vacía, `items` es `[]` y el cliente conserva su cursor.

**Errores** (ProblemDetails, RFC 9457):

| Código | Cuándo |
|---|---|
| 400 | Cursor mal formado (`type`/`code`: `invalid_cursor`), `limit` fuera de rango, `after` y `before` a la vez |
| 401 | Sin sesión |
| 404 | El ticket no existe **o** no es visible para el usuario: no participante, participante retirado, cliente, administrador no asociado. Se responde igual en todos los casos para no revelar la existencia del ticket (propuesta; ratificar en T-01 frente a la convención "403 por rol") |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: ratificar ruta, parámetros, límites (`limit` 50/100), formato y tamaño del cursor, `ChatMessageDto` completo (incluidos `attachments` y `readReceipts`), códigos de error y el uso de 404 para tickets no visibles. Registrar en [[tiempo-real-signalr]] y en el OpenAPI (`/openapi/v1.json`). Registrar los nombres nuevos en el [[glosario]].
- [ ] **T-02 — Dominio `ChatMessage` (prueba primero)** · Capa: Backend Domain · Dificultad: Bajo  
  Descripción: pruebas xUnit de construcción válida (autor, ticket, cuerpo, `CreatedAt` truncado a microsegundos) e inmutabilidad; después, la entidad.
- [ ] **T-03 — Regla de acceso al chat (prueba primero)** · Capa: Backend Application · Dificultad: Medio  
  Descripción: pruebas unitarias con dobles de `ICurrentUser` y del repositorio que cubran: participante vigente → permitido; rol `ProductManager` sin participación → permitido; participante retirado (`RemovedAt` con valor) → denegado; administrador no asociado → denegado; integrante de `Team.Development` no asociado → denegado; cliente → denegado. Implementar la regla reutilizable (`ChatAccessPolicy`, nombre propuesto) y reutilizar la de [[hu-020-detalle-interno-del-ticket|HU-020]] si ya existe.
- [ ] **T-04 — Caso de uso `GetChatHistory` (prueba primero)** · Capa: Backend Application · Dificultad: Medio  
  Descripción: pruebas con un doble de `IChatRepository` para: sin cursor → últimos `limit`; `after` → posteriores; `before` → anteriores; `hasOlder`/`hasNewer`; denegado → resultado "no encontrado" sin consultar mensajes. Definir el método de lectura paginada en `IChatRepository`.
- [ ] **T-05 — Persistencia y migración** · Capa: Backend Infrastructure · Dificultad: Medio  
  Descripción: `ChatMessageConfiguration` (tabla, FK a ticket y autor, `timestamptz`, índice `(TicketId, CreatedAt, Id)`); adaptador EF Core con comparación por tupla `(CreatedAt, Id)`; migración `AddChatMessages` (nombre propuesto) en `Persistence/Migrations`.
- [ ] **T-06 — Endpoint e integración** · Capa: Backend Api · Dificultad: Medio  
  Descripción: pruebas de integración con `WebApplicationFactory<Program>` y PostgreSQL real **primero**: historial completo para el agregado tarde, 404 para no participante, retirado, cliente y administrador no asociado, 400 para cursor inválido, paginación de 120 mensajes sin huecos ni duplicados y `Cache-Control: no-store`. Después, el endpoint minimal API con su política de autenticación y el mapeo a ProblemDetails.
- [ ] **T-07 — Modelo y gateway del chat** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: tipos `ChatMessage` y `ChatHistoryPage` y normalización del JSON. Función pura `mergeMessages(actuales, nuevos)` que ordena por `(createdAt, id)` y descarta duplicados por `id`, probada con Vitest. Gateway `chatGateway.fetchHistory(ticketId, { after | before, limit })` sobre `core/http`.
- [ ] **T-08 — Controlador** · Capa: Frontend controllers · Dificultad: Bajo  
  Descripción: `useTicketChatController(ticketId)` (nombre propuesto) con estado discriminado `loading | ready | notFound | error`, `loadOlder()`, cursor del último mensaje expuesto para HU-031 y cancelación con `AbortController`. La lógica de estado se prueba con Vitest como reductor puro en `models/`.
- [ ] **T-09 — Vistas** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `ChatMessageList` (componente puro: autor, hora en `America/Bogota`, cuerpo como texto plano), botón "Cargar anteriores" visible si `hasOlder`, estado vacío y estado "No tienes acceso a este chat" para el 404. Sin imports de `core/` ni de gateways (`oxlint`).

## Criterios de aceptación

### CHU-01 — Historial completo para quien se agrega tarde

**Dado** un ticket con 120 mensajes enviados antes de que Brayan fuera agregado como participante  
**Cuando** Brayan, ya participante vigente, pide `GET /api/tickets/{id}/messages` y luego pagina con `before` hasta que `hasOlder` es `false`  
**Entonces** recibe los 120 mensajes exactamente una vez, en orden ascendente por `(createdAt, id)`, sin huecos ni duplicados entre páginas.

### CHU-02 — Elizabeth lee sin ser participante

**Dado** un ticket en el que Elizabeth (rol `ProductManager`) no figura como `TicketParticipant`  
**Cuando** pide el historial del chat  
**Entonces** recibe 200 con los mensajes.

### CHU-03 — No participante, cliente y administrador no asociado rechazados

**Dado** un ticket con mensajes  
**Cuando** piden su historial: un integrante de `Team.Development` no asociado, un `Requester` de la empresa dueña del ticket, un `CompanyCoordinator` de otra empresa o un `Administrator` no asociado  
**Entonces** cada uno recibe 404 con ProblemDetails, el cuerpo no contiene ningún mensaje y la respuesta es indistinguible de la de un ticket inexistente.

### CHU-04 — El retirado no vuelve a consultar el historial

**Dado** Kevin, que participó en el ticket y fue retirado (`RemovedAt` con valor)  
**Cuando** pide el historial con o sin cursor  
**Entonces** recibe 404, y sus mensajes anteriores siguen apareciendo para los participantes vigentes con su nombre como autor.

### CHU-05 — Recuperación con `after`

**Dado** un participante que tiene el `cursor` del mensaje 40 de 45  
**Cuando** pide `GET .../messages?after=<cursor>`  
**Entonces** recibe exactamente los mensajes 41 a 45 en orden y `hasNewer: false`. Con un cursor del último mensaje recibe `items: []`.

### CHU-06 — Orden estable con marcas de tiempo iguales

**Dado** dos mensajes con el mismo `createdAt` (al microsegundo)  
**Cuando** se paginan con `limit=1` en ambos sentidos  
**Entonces** cada mensaje aparece una sola vez y el orden se desempata por `id` de forma consistente entre llamadas.

### CHU-07 — Parámetros inválidos

**Dado** un participante vigente  
**Cuando** envía un cursor mal formado, `limit=0`, `limit=101` o `after` y `before` a la vez  
**Entonces** recibe 400 con ProblemDetails que identifica el parámetro inválido, sin ejecutar la consulta de mensajes.

### CHU-08 — Sin sesión y sin caché

**Dado** una petición sin cookie de sesión  
**Cuando** se llama al endpoint  
**Entonces** responde 401. Las respuestas 200 llevan `Cache-Control: no-store` (propuesta).

### CHU-09 — Comportamiento de la UI

**Dado** el detalle interno de un ticket abierto en el navegador  
**Cuando** el historial carga, falla por red o responde 404  
**Entonces** la vista muestra, respectivamente: los mensajes más recientes con "Cargar anteriores" si hay más; un mensaje de error con "Reintentar"; "No tienes acceso a este chat", sin contenido residual de otro ticket. Un cuerpo con `<script>` se muestra como texto literal.

## Definition of Done

- [ ] DoD-01 — CHU-01 a CHU-09 validados con evidencia registrada en la matriz.
- [ ] DoD-02 — Pruebas escritas primero y en verde con `cd backend && dotnet test`: unitarias `ChatMessageTests` y `ChatAccessPolicyTests` (participante vigente, PM, retirado, admin no asociado, equipo no asociado, cliente) y `GetChatHistoryTests` con dobles de `IChatRepository` e `ICurrentUser`.
- [ ] DoD-03 — Pruebas de integración en verde con `WebApplicationFactory<Program>` y PostgreSQL real: `Historial_AgregadoTarde_RecibeTodoSinDuplicados`, `Historial_NoParticipante_Devuelve404`, `Historial_Retirado_Devuelve404`, `Historial_AdminNoAsociado_Devuelve404`, `Historial_Cliente_Devuelve404`, `Historial_After_DevuelveSoloPosteriores`, `Historial_CursorInvalido_Devuelve400`.
- [ ] DoD-04 — `DataTicket.ArchitectureTests` en verde: `Application` no referencia EF Core ni ASP.NET Core.
- [ ] DoD-05 — Migración `AddChatMessages` (nombre propuesto) creada, aplicada en una base limpia del Compose y revisada (índice `(TicketId, CreatedAt, Id)`).
- [ ] DoD-06 — Frontend: `npm --prefix frontend run lint` sin errores (fronteras MVC), `npm --prefix frontend test` en verde (`mergeMessages` y reductor del chat) y `npm --prefix frontend run build` correcto.
- [ ] DoD-07 — El contrato ratificado está en [[tiempo-real-signalr]] y coincide con el OpenAPI de `/openapi/v1.json`.
- [ ] DoD-08 — `docker compose up --build` levanta el stack; verificación manual en `http://localhost:5173` con dos sesiones (participante y no participante) en navegadores distintos.
- [ ] DoD-09 — Wiki actualizada: [[chat-interno]], [[tiempo-real-signalr]], [[persistencia-postgresql]] (tabla e índice), [[modelo-de-dominio]] y [[glosario]] (`GetChatHistory`, `ChatMessageDto`, cursor, `ChatAccessPolicy`), mediante Notas para la wiki.
- [ ] DoD-10 — Revisión de `quality-reviewer` sin hallazgos CRÍTICO o ALTO abiertos.
- [ ] DoD-11 — PR revisado y aprobado por otra persona del equipo.
- [ ] DoD-12 — Trazabilidad de la HU y de [[ep-009-chat-interno-en-tiempo-real]] actualizada.

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
| DoD-11 | Pendiente | — | — |
| DoD-12 | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- **Hallazgo 8, cursor sin formato:** se propone un token opaco `(CreatedAt, Id)` en base64url. Se ratifica en T-01 y se registra en [[tiempo-real-signalr]].
- **404 frente a 403** para tickets no visibles: propuesta coherente con la convención REST del backlog. Si el equipo prefiere 403 para el rol cliente, cambia en T-01 y en las HU del chat.
- El historial no se audita como acceso (PRD §12 no lo exige). Auditar intentos denegados depende de V-11.
- **Row-Level Security** sigue abierta ([[pendientes]] §4). El filtro por participación se hace en el caso de uso de todas formas.

## Relacionado

- [[ep-009-chat-interno-en-tiempo-real]] · [[tablero-scrum]] · [[criterios-de-aceptacion]]
- [[chat-interno]] · [[tiempo-real-signalr]] · [[persistencia-postgresql]] · [[modelo-de-dominio]] · [[roles-y-permisos]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[estrategia-de-pruebas]]
