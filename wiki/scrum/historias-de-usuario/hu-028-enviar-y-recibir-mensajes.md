---
title: "HU-028 — Enviar y recibir mensajes en tiempo real"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/chat, signalr, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §7", "PRD.md §11", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-028", "Enviar y recibir mensajes en tiempo real"]
epica: "[[ep-009-chat-interno-en-tiempo-real]]"
criterios_prd: ["CA-05", "CA-06", "CA-09"]
componentes: ["Hub SignalR", "Backend (Domain, Application, Infrastructure, Api)", "Persistencia PostgreSQL", "Frontend (core/realtime, models, controllers, views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 5"
dependencias: ["[[hu-026-historial-del-chat-por-cursor]]", "[[hu-027-unirse-al-chat-del-ticket]]"]
relacionadas: ["[[hu-029-adjuntos-e-imagenes-en-chat]]", "[[hu-030-confirmaciones-de-lectura]]", "[[hu-031-recuperar-mensajes-tras-reconexion]]", "[[hu-032-revocar-acceso-al-retirar-participante]]", "[[hu-037-notificaciones-en-la-app]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-028 — Enviar y recibir mensajes en tiempo real

Un participante autorizado envía un mensaje de texto por el hub. El servidor revalida la membresía, guarda el mensaje en PostgreSQL y solo después publica `MessageCreated` a las conexiones unidas al ticket (PRD §7.1, §7.2, flujo 4).

## Historia de usuario

**COMO** participante interno vigente de un ticket (o Elizabeth, PM)  
**QUIERO** escribir en el chat del ticket y ver al instante lo que escriben los demás  
**PARA** coordinar el trabajo con Desarrollo y Producción sin salir de DataTicket y sin perder nada de lo conversado

## Contexto

El PRD exige que un mensaje de una persona autorizada aparezca en tiempo real para las demás personas conectadas al ticket (PRD §7.1) y que el servidor lo guarde en PostgreSQL **antes** de confirmar su publicación (PRD §7.2). Al enviar, el servidor "vuelve a comprobar la membresía, persiste mensaje y adjuntos, y luego publica el evento en el grupo autorizado" (PRD §7, flujo 4). El caso de uso `SendChatMessage` notifica a través del puerto `ITicketChatNotifier`, que implementa `Api/Realtime` con `IHubContext` ([[tiempo-real-signalr]], [[backend-hexagonal]]). La [[estrategia-de-pruebas]] exige una prueba unitaria que demuestre que el notificador no se invoca si falla la persistencia.

## Alcance

- Método del hub `SendMessage(ticketId, request)` y evento `MessageCreated` con el `ChatMessageDto` de [[hu-026-historial-del-chat-por-cursor|HU-026]].
- Caso de uso `SendChatMessage`: autorizar → validar → persistir → notificar.
- Puerto `ITicketChatNotifier` en `Application/Ports/Out` y su adaptador en `Api/Realtime` (publica en el grupo `ticket:{id}`).
- Validaciones de texto: no vacío y longitud máxima (propuesta).
- Frontend: compositor de mensajes, envío por el gateway del hub, recepción de `MessageCreated` y fusión sin duplicados en la lista.

## Fuera de alcance

- Adjuntos en el mensaje ([[hu-029-adjuntos-e-imagenes-en-chat|HU-029]]). En esta HU, `attachmentIds` debe ir vacío u omitido.
- Lecturas ([[hu-030-confirmaciones-de-lectura|HU-030]]).
- Recuperación tras reconexión ([[hu-031-recuperar-mensajes-tras-reconexion|HU-031]]).
- Notificación dentro de la aplicación por mensaje nuevo ([[hu-037-notificaciones-en-la-app|HU-037]], que se apoya en este caso de uso). Nunca un correo por mensaje (PRD §7.6, §11).
- Edición y borrado de mensajes (V-14, [[pendientes]]): el mensaje es inmutable (propuesta del [[modelo-de-dominio]]).
- Formato enriquecido (markdown o HTML), menciones y reacciones.
- Que el cliente vea o envíe mensajes (PRD §7, §13).

## Requisitos y reglas de negocio

- Mensaje en tiempo real para las demás personas conectadas al ticket (PRD §7.1).
- Persistir en PostgreSQL antes de confirmar la publicación; SignalR no es el historial (PRD §7.2).
- La membresía se vuelve a comprobar en cada envío (PRD §7, flujo 4; "los grupos no son una barrera de seguridad").
- Solo participantes vigentes escriben; Elizabeth siempre puede (PRD §5.3); el administrador solo si está asociado (PRD §5.5).
- Una persona retirada deja de enviar (PRD §7.5).
- Validación de pertenencia en todas las rutas de chat (PRD §12).
- El chat acepta texto (PRD §7.8, §14.9).

## Invariantes en juego

- AGENTS §7.2: el cliente nunca ve el chat interno.
- AGENTS §7.3: solo participantes vigentes escriben; se valida en cada operación.
- AGENTS §7.4: cada mensaje se persiste antes de publicarse por SignalR.
- AGENTS §7.5: el administrador no escribe por ser administrador.

## Criterios del PRD cubiertos

- PRD CA-05 (parcial: entrega en tiempo real a los participantes conectados; la recuperación está en HU-031) → [[criterios-de-aceptacion]]
- PRD CA-06 (parcial: "un no participante no puede enviar mensajes") → [[criterios-de-aceptacion]]
- PRD CA-09 (parcial: "el chat acepta texto") → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-009-chat-interno-en-tiempo-real]]
- Dependencias: [[hu-026-historial-del-chat-por-cursor|HU-026 — Historial del chat paginado por cursor]] (`ChatMessage`, `IChatRepository`, DTO, regla de acceso), [[hu-027-unirse-al-chat-del-ticket|HU-027 — Conectarse y unirse al chat del ticket]] (hub, grupo, gateway).
- Relacionadas: [[hu-029-adjuntos-e-imagenes-en-chat|HU-029]], [[hu-030-confirmaciones-de-lectura|HU-030]], [[hu-031-recuperar-mensajes-tras-reconexion|HU-031]], [[hu-032-revocar-acceso-al-retirar-participante|HU-032]], [[hu-037-notificaciones-en-la-app|HU-037]].
- Decisiones: [[adr-0005-signalr-para-chat]] (aceptada). Pendientes: V-14 (edición y borrado), V-11 (auditar intentos denegados).

## Componentes afectados

- Backend Domain: fábrica o validación de `ChatMessage` (cuerpo no vacío y longitud máxima).
- Backend Application: `Ports/In` → `SendChatMessage`; `Ports/Out` → `IChatRepository` (escritura), `ITicketChatNotifier`, `IClock`, `ICurrentUser`.
- Backend Infrastructure: escritura en `IChatRepository` (EF Core).
- Hub SignalR: `TicketsHub.SendMessage`; adaptador `ITicketChatNotifier` con `IHubContext<TicketsHub>` en `Api/Realtime`.
- Frontend: gateway del hub (`sendMessage`, suscripción a `MessageCreated`), `useTicketChatController`, vista `ChatComposer` (nombre propuesto).

## Dificultad

**Nivel:** Alto

**Justificación:** es transversal (hub, caso de uso, persistencia y frontend) y garantiza un orden estricto entre persistir y publicar. Requiere pruebas de integración con varios clientes conectados para demostrar que nadie fuera del grupo autorizado recibe el evento.

## Contrato backend ↔ frontend

> [!info] Propuesta
> Se ratifica en T-01 y se documenta en [[tiempo-real-signalr]].

**Método del hub (cliente → servidor):** `SendMessage(ticketId: string, request: SendMessageRequest)` → devuelve el `ChatMessageDto` persistido (confirmación al emisor).

```json
// SendMessageRequest
{
  "body": "Ya reproduje el error en el ambiente de pruebas.",
  "attachmentIds": []
}
```

Reglas (propuesta):

- `body` sin espacios en los extremos y de 1 a 4000 caracteres. Puede ir vacío solo si hay adjuntos (desde HU-029).
- `attachmentIds` debe ir vacío u omitido hasta [[hu-029-adjuntos-e-imagenes-en-chat|HU-029]].
- `MaximumReceiveMessageSize` del hub se mantiene en el valor por defecto (32 KB), suficiente para 4000 caracteres en UTF-8.
- `createdAt` y `author` los fija el servidor (`IClock`, `ICurrentUser`); el cliente no los envía.

**Evento (servidor → grupo `ticket:{ticketId}`):** `MessageCreated(message: ChatMessageDto)`. El grupo incluye las conexiones del propio emisor (otras pestañas); el cliente descarta duplicados por `id`.

```json
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
```

**Errores (`HubException`, códigos de [[hu-027-unirse-al-chat-del-ticket|HU-027]]):**

| Código | Cuándo |
|---|---|
| `ticket_not_accessible` | No participante, retirado, cliente, administrador no asociado o ticket inexistente |
| `validation_failed` | Cuerpo vacío sin adjuntos, más de 4000 caracteres o `attachmentIds` no vacío antes de HU-029 |
| `internal_error` | Falla la persistencia. El mensaje **no** se publica y el emisor debe reintentar |

**Fallo al publicar después de persistir** (propuesta): el mensaje ya está guardado y la invocación se confirma al emisor. El fallo se registra en el log. Los demás lo recuperan por historial ([[hu-031-recuperar-mensajes-tras-reconexion|HU-031]]).

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: ratificar la firma de `SendMessage`, `SendMessageRequest`, la longitud máxima (propuesta: 4000), la respuesta al emisor, el evento `MessageCreated` (incluido el emisor en el grupo y la deduplicación por `id`), los códigos de error y el tratamiento de un fallo al publicar. Documentarlo en [[tiempo-real-signalr]].
- [ ] **T-02 — Reglas de dominio (prueba primero)** · Capa: Backend Domain · Dificultad: Bajo  
  Descripción: pruebas de `ChatMessage`: cuerpo vacío o solo espacios → error; 4001 caracteres → error; 4000 → válido; recorte de espacios. Implementar la fábrica o validación.
- [ ] **T-03 — Caso de uso `SendChatMessage` (prueba primero)** · Capa: Backend Application · Dificultad: Medio  
  Descripción: pruebas unitarias con dobles de `IChatRepository`, `ITicketChatNotifier`, `ICurrentUser` e `IClock`: (a) autorizado → persiste y luego notifica, verificando el orden de llamadas; (b) **si la persistencia lanza una excepción, `ITicketChatNotifier` no se invoca** y el error se propaga; (c) no autorizado → no persiste ni notifica; (d) cuerpo inválido → no persiste ni notifica; (e) si la notificación falla tras persistir, el resultado es éxito y se registra en el log. Definir `ITicketChatNotifier.MessageCreatedAsync(ticketId, dto, ct)` en `Ports/Out`.
- [ ] **T-04 — Persistencia** · Capa: Backend Infrastructure · Dificultad: Bajo  
  Descripción: método de escritura en el adaptador EF Core de `IChatRepository`, con confirmación (`SaveChangesAsync`) antes de devolver. No cambia el esquema (la tabla es de HU-026).
- [ ] **T-05 — Hub y notificador** · Capa: Backend Api/Realtime · Dificultad: Alto  
  Descripción: pruebas de integración primero (`WebApplicationFactory<Program>`, PostgreSQL real, cuatro clientes SignalR .NET: Laura y Brayan unidos a T, Cristian conectado sin unirse ni ser participante, Julián unido a otro ticket U). Después, `TicketsHub.SendMessage`, mapeo de errores a `HubException` y adaptador `SignalRTicketChatNotifier` (nombre propuesto) con `IHubContext<TicketsHub>.Clients.Group("ticket:{id}")`.
- [ ] **T-06 — Gateway** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: `ticketsHubGateway.sendMessage(ticketId, body)` y `onMessageCreated(handler)` con su función de baja; validación de cliente (no vacío, ≤ 4000) como ayuda de interfaz, probada con Vitest. La lista se fusiona con `mergeMessages` (HU-026).
- [ ] **T-07 — Controlador** · Capa: Frontend controllers · Dificultad: Medio  
  Descripción: `useTicketChatController` expone `send(body)`, `sending` y `sendError`. Al confirmar, inserta el DTO devuelto. Al recibir `MessageCreated` del ticket abierto, fusiona sin duplicados e ignora los eventos de otros tickets. La lógica se prueba con Vitest como reductor puro.
- [ ] **T-08 — Vistas** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `ChatComposer` (nombre propuesto): área de texto, contador hasta 4000, botón "Enviar" deshabilitado si el texto está vacío, mientras se envía o sin conexión. Si el envío falla, el texto se conserva y aparece un mensaje de error. Los mensajes se muestran como texto plano.

## Criterios de aceptación

### CHU-01 — Flujo feliz con dos usuarios conectados

**Dado** Laura y Brayan, participantes vigentes del ticket T, conectados y unidos en navegadores distintos  
**Cuando** Laura envía "Ya reproduje el error"  
**Entonces** Laura recibe como respuesta el `ChatMessageDto` con `id` y `createdAt` del servidor; Brayan recibe `MessageCreated` con el mismo `id` sin recargar; y el mensaje aparece una sola vez en ambas pantallas.

### CHU-02 — Persistir antes de publicar

**Dado** el caso de uso `SendChatMessage` con un `IChatRepository` que falla al guardar  
**Cuando** un participante vigente envía un mensaje  
**Entonces** `ITicketChatNotifier` no se invoca, el emisor recibe `HubException` `internal_error` y ningún otro cliente recibe `MessageCreated`. En la integración, al recibir `MessageCreated`, el mensaje ya existe en PostgreSQL.

### CHU-03 — Solo el grupo autorizado recibe el evento

**Dado** Laura y Brayan unidos a T, Cristian conectado sin unirse a T y Julián unido al ticket U  
**Cuando** Laura envía un mensaje a T  
**Entonces** solo las conexiones unidas a T (Brayan y las de Laura) reciben `MessageCreated`. Cristian y Julián no lo reciben dentro del tiempo de espera de la prueba.

### CHU-04 — Revalidación en cada envío

**Dado** Kevin, unido a T y luego retirado en la base de datos mientras su conexión sigue en el grupo (sin la revocación de HU-032)  
**Cuando** invoca `SendMessage(T, …)`  
**Entonces** recibe `HubException` `ticket_not_accessible`, no se guarda ningún mensaje y nadie recibe `MessageCreated`.

### CHU-05 — No participante, administrador no asociado y cliente rechazados

**Dado** el ticket T  
**Cuando** invocan `SendMessage(T, …)` (sin pasar por `JoinTicket`) un integrante de Desarrollo no asociado, un `Administrator` no asociado o un usuario cliente que lograra conectarse  
**Entonces** cada intento termina en `ticket_not_accessible` (o en 403 al negociar, para el cliente) y no se persiste nada.

### CHU-06 — Elizabeth escribe sin ser participante

**Dado** Elizabeth (`ProductManager`) unida a T sin ser `TicketParticipant`  
**Cuando** envía un mensaje  
**Entonces** se persiste con ella como autora y los participantes unidos lo reciben.

### CHU-07 — Validaciones del mensaje

**Dado** un participante vigente  
**Cuando** envía un cuerpo vacío o de solo espacios, uno de 4001 caracteres o `attachmentIds` no vacío antes de HU-029  
**Entonces** recibe `HubException` `validation_failed`, no se persiste nada y no se publica ningún evento. Con exactamente 4000 caracteres, el envío se acepta.

### CHU-08 — Texto seguro en la UI

**Dado** un mensaje cuyo cuerpo es `<img src=x onerror=alert(1)>`  
**Cuando** se muestra en la lista de otro participante  
**Entonces** aparece como texto literal y no se ejecuta ningún script.

### CHU-09 — UI ante error y desconexión

**Dado** el compositor con un texto escrito  
**Cuando** el envío falla (`internal_error` o `validation_failed`) o la conexión está en "Reconectando…" o "Sin conexión"  
**Entonces** el texto no se pierde, se muestra un error comprensible o el botón queda deshabilitado con el estado de conexión visible, y no aparece en la lista ningún mensaje que el servidor no haya confirmado.

## Definition of Done

- [ ] DoD-01 — CHU-01 a CHU-09 validados con evidencia registrada en la matriz.
- [ ] DoD-02 — Pruebas unitarias escritas primero y en verde con `cd backend && dotnet test`: `SendChatMessage_Autorizado_PersisteYLuegoNotifica`, `SendChatMessage_FallaPersistencia_NoInvocaNotificador`, `SendChatMessage_NoAutorizado_NoPersisteNiNotifica`, `SendChatMessage_CuerpoInvalido_Rechaza`, `SendChatMessage_FallaNotificacion_ConservaMensaje`, y pruebas de dominio de longitud.
- [ ] DoD-03 — Pruebas de integración del hub en verde con `WebApplicationFactory<Program>`, cliente SignalR .NET y PostgreSQL real: `SendMessage_DosParticipantes_RecibenEnTiempoReal`, `SendMessage_ConexionNoUnida_NoRecibe`, `SendMessage_OtroTicket_NoRecibe`, `SendMessage_Retirado_Rechazado`, `SendMessage_AdminNoAsociado_Rechazado`, `MessageCreated_MensajeYaPersistido`.
- [ ] DoD-04 — `DataTicket.ArchitectureTests` en verde: `ITicketChatNotifier` está en `Application/Ports/Out` y su implementación en `Api`.
- [ ] DoD-05 — Frontend: `npm --prefix frontend run lint` sin errores, `npm --prefix frontend test` en verde (validación de cliente, reductor con `MessageCreated` duplicado y de otro ticket) y `npm --prefix frontend run build` correcto.
- [ ] DoD-06 — `docker compose up --build` levanta el stack. Verificación manual en `http://localhost:5173` con dos navegadores (o una ventana privada) de dos participantes y un tercero no participante: el mensaje llega al segundo participante y no al tercero.
- [ ] DoD-07 — Contrato `SendMessage` / `MessageCreated` documentado en [[tiempo-real-signalr]] y coherente con el gateway del frontend.
- [ ] DoD-08 — Wiki actualizada mediante Notas para la wiki: [[tiempo-real-signalr]], [[chat-interno]], [[modelo-de-dominio]] (longitud máxima, inmutabilidad), [[glosario]] (`SendMessageRequest`, `ChatMessageDto`), [[backend-hexagonal]] (estado de `ITicketChatNotifier`).
- [ ] DoD-09 — Revisión de `quality-reviewer` sin hallazgos CRÍTICO o ALTO abiertos (orden persistir/publicar, revalidación, XSS).
- [ ] DoD-10 — PR revisado y aprobado por otra persona del equipo.
- [ ] DoD-11 — Trazabilidad de la HU y de [[ep-009-chat-interno-en-tiempo-real]] actualizada.

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

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- **V-14:** edición y borrado de mensajes sin decidir. Se modela `ChatMessage` como inmutable y no se exponen métodos de edición.
- **Auditoría:** PRD §12 no incluye los mensajes del chat entre los eventos auditables. El mensaje persistido e inmutable es su propio registro ("la auditoría conserva sus mensajes", PRD §7.5). Propuesta: no generar `AuditEntry` por mensaje; confirmar con el equipo.
- **Longitud máxima de 4000 caracteres:** propuesta; el PRD no la fija.
- **Ventana de carrera con la revocación:** un evento publicado justo durante el retiro de un participante se trata en [[hu-032-revocar-acceso-al-retirar-participante|HU-032]].
- [[hu-037-notificaciones-en-la-app|HU-037]] engancha la notificación dentro de la aplicación después de persistir, sin bloquear la publicación.

## Relacionado

- [[ep-009-chat-interno-en-tiempo-real]] · [[tablero-scrum]] · [[criterios-de-aceptacion]]
- [[chat-interno]] · [[tiempo-real-signalr]] · [[adr-0005-signalr-para-chat]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[persistencia-postgresql]] · [[notificaciones]] · [[estrategia-de-pruebas]]
