---
title: "HU-032 — Revocar acceso al retirar un participante"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/chat, producto/seguridad, signalr, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §7", "PRD.md §11", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-032", "Revocar acceso al retirar un participante"]
epica: "[[ep-009-chat-interno-en-tiempo-real]]"
criterios_prd: ["CA-06", "CA-08"]
componentes: ["Hub SignalR", "Backend (Application, Api)", "Frontend (core/realtime, models, controllers, views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 5"
dependencias: ["[[hu-019-reasignar-y-retirar-participantes]]", "[[hu-027-unirse-al-chat-del-ticket]]", "[[hu-028-enviar-y-recibir-mensajes]]"]
relacionadas: ["[[hu-026-historial-del-chat-por-cursor]]", "[[hu-029-adjuntos-e-imagenes-en-chat]]", "[[hu-030-confirmaciones-de-lectura]]", "[[hu-031-recuperar-mensajes-tras-reconexion]]", "[[hu-037-notificaciones-en-la-app]]", "[[hu-038-correo-de-vinculacion]]", "[[hu-018-asignar-y-agregar-participantes]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-032 — Revocar acceso al retirar un participante

Cuando Elizabeth o un administrador retira a un participante, sus conexiones activas salen del grupo del ticket y reciben un aviso de revocación. A partir de ese momento cualquier operación suya sobre el chat de ese ticket se rechaza, mientras sus mensajes y su participación pasada se conservan en el historial y la auditoría (PRD §5.4, §7.5, §14.6).

## Historia de usuario

**COMO** Elizabeth (PM) o administrador que retira a una persona de un ticket  
**QUIERO** que pierda de inmediato el acceso al chat, también en las conexiones que ya tiene abiertas  
**PARA** que nadie fuera del equipo vigente del ticket siga leyendo ni escribiendo en la conversación interna

## Contexto

Retirar a un participante revoca su acceso futuro y conserva su participación pasada (PRD §5.4). La persona retirada deja de leer, enviar, marcar lectura o reconectarse (PRD §7.5). Si está conectada, "sus conexiones activas se deben desconectar o dejar sin autorización antes de permitir nuevas operaciones" (PRD §7). CA-06 exige la revocación en conexiones **existentes y futuras** (PRD §14.6), y CA-08, que el retirado no vuelva a consultar el historial (PRD §14.8). El retiro lo implementa `RemoveParticipant` en [[hu-019-reasignar-y-retirar-participantes|HU-019]] (marca `RemovedAt`/`RemovedBy` y audita). El mapa de [[backend-hexagonal]] le asigna el puerto `IChatConnectionRevoker` con adaptador SignalR. Las HU [[hu-026-historial-del-chat-por-cursor|HU-026]], [[hu-027-unirse-al-chat-del-ticket|HU-027]], [[hu-028-enviar-y-recibir-mensajes|HU-028]] y [[hu-030-confirmaciones-de-lectura|HU-030]] ya revalidan el acceso en cada operación. Esta HU cierra lo que falta: que una conexión que sigue en el grupo deje de **recibir** eventos.

## Alcance

- Puerto `IChatConnectionRevoker` en `Application/Ports/Out` y su adaptador en `Api/Realtime`, que usa el registro de conexiones de [[hu-027-unirse-al-chat-del-ticket|HU-027]].
- Invocación del revocador desde `RemoveParticipant` (HU-019), **después** de persistir el retiro y antes de responder.
- Salida del grupo `ticket:{id}` de todas las conexiones del usuario retirado en ese ticket (todas sus pestañas), sin cerrar la conexión, que puede servir a otros tickets.
- Evento `TicketAccessRevoked` (nombre propuesto) a esas conexiones, para que la UI deje de mostrar el chat.
- Pruebas de integración del hub con dos o más clientes conectados que demuestran la revocación en conexiones existentes y futuras.
- Frontend: tratamiento del evento (estado "sin acceso", limpieza de mensajes en memoria, compositor oculto).

## Fuera de alcance

- La operación de retiro, su endpoint, sus permisos y su auditoría: [[hu-019-reasignar-y-retirar-participantes|HU-019]].
- Notificaciones dentro de la aplicación para la persona retirada: [[hu-037-notificaciones-en-la-app|HU-037]] excluye a los retirados de los destinatarios.
- Correo de vinculación al reincorporar a alguien: [[hu-038-correo-de-vinculacion|HU-038]] (V-08 abierta).
- Invalidar las URL públicas de adjuntos que la persona ya vio: imposible con [[adr-0006-urls-publicas-azure-blob]] (riesgo aceptado).
- Revocación por desactivación de la cuenta de usuario (no por retiro del ticket): pregunta abierta, ver *Notas*.
- Backplane: el registro de conexiones en memoria vale para una sola instancia.

## Requisitos y reglas de negocio

- Retirar revoca el acceso futuro; la participación pasada se conserva en el historial y la auditoría (PRD §5.4, §12).
- La persona retirada deja de leer, enviar, marcar lectura o reconectarse (PRD §7.5).
- Las conexiones activas se desconectan o quedan sin autorización antes de permitir nuevas operaciones (PRD §7).
- Los grupos no son seguridad; la pertenencia se valida en cada operación (PRD §7).
- No se notifica a la persona retirada por mensajes futuros (PRD §11).
- Elizabeth siempre puede acceder al chat (PRD §5.3): retirarla como participante no le quita el acceso que le da su rol.
- Agregar a alguien le da acceso al historial completo (PRD §5.4). Se aplica también a quien se reincorpora (propuesta).

## Invariantes en juego

- AGENTS §7.3: solo participantes vigentes; los grupos no son seguridad; se valida en cada unión y operación.
- AGENTS §7.5: un administrador retirado pierde el acceso; no lo conserva por ser administrador.
- AGENTS §7.8: la auditoría del retiro es append-only (la registra HU-019).

## Criterios del PRD cubiertos

- PRD CA-06 (parcial; la completan HU-026, HU-027, HU-028 y HU-030: "retirarlo revoca el acceso en conexiones existentes y futuras") → [[criterios-de-aceptacion]]
- PRD CA-08 (parcial: "una persona retirada no puede volver a consultarlo") → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-009-chat-interno-en-tiempo-real]]
- Dependencias: [[hu-019-reasignar-y-retirar-participantes|HU-019 — Reasignar y retirar participantes]] (`RemoveParticipant`), [[hu-027-unirse-al-chat-del-ticket|HU-027 — Conectarse y unirse al chat del ticket]] (registro de conexiones, grupo), [[hu-028-enviar-y-recibir-mensajes|HU-028 — Enviar y recibir mensajes en tiempo real]] (`MessageCreated` para demostrar que deja de llegar).
- Relacionadas: [[hu-026-historial-del-chat-por-cursor|HU-026]], [[hu-029-adjuntos-e-imagenes-en-chat|HU-029]], [[hu-030-confirmaciones-de-lectura|HU-030]], [[hu-031-recuperar-mensajes-tras-reconexion|HU-031]] (reconexión del retirado), [[hu-037-notificaciones-en-la-app|HU-037]], [[hu-038-correo-de-vinculacion|HU-038]], [[hu-018-asignar-y-agregar-participantes|HU-018]] (reincorporación).
- Decisiones: [[adr-0005-signalr-para-chat]] (aceptada). Pendientes: V-08 (correo al reincorporar), V-11 (auditar intentos denegados), backplane ([[pendientes]] §4).

## Componentes afectados

- Backend Application: puerto `IChatConnectionRevoker` (`RevokeTicketAccessAsync(ticketId, userId, ct)`, firma propuesta) y su llamada desde `RemoveParticipant`.
- Hub SignalR: adaptador `SignalRChatConnectionRevoker` (nombre propuesto) en `Api/Realtime`, que consulta `ChatConnectionTracker` (HU-027), usa `IHubContext<TicketsHub>.Groups.RemoveFromGroupAsync` y envía `TicketAccessRevoked` a esas conexiones.
- Frontend: gateway `onTicketAccessRevoked`, controlador del chat y vista de estado "sin acceso".

## Dificultad

**Nivel:** Alto

**Justificación:** es transversal (caso de uso de otra épica, puerto nuevo, hub, registro concurrente y frontend). Tiene riesgo de carrera entre el retiro y la publicación de eventos y exige pruebas de integración con varios clientes y varias pestañas del mismo usuario.

## Contrato backend ↔ frontend

> [!info] Propuesta
> Se ratifica en T-01 y se documenta en [[tiempo-real-signalr]].

**Disparador:** el endpoint de retiro de [[hu-019-reasignar-y-retirar-participantes|HU-019]] (por ejemplo, `DELETE /api/tickets/{ticketId}/participants/{userId}`, según su contrato). Cuando responde con éxito, la revocación ya se ejecutó.

**Evento (servidor → conexiones del retirado en ese ticket):** `TicketAccessRevoked`

```json
{
  "ticketId": "01928b77-1c2d-7e0f-a3b4-c5d6e7f80912",
  "revokedAt": "2026-10-07T16:20:00.000000Z"
}
```

- El payload no incluye quién retiró ni el motivo (propuesta).
- La conexión **no** se cierra: sale del grupo `ticket:{id}` y sigue sirviendo a otros tickets.

**Después del retiro**, para esa persona y ese ticket:

| Operación | Resultado |
|---|---|
| `JoinTicket`, `SendMessage`, `MarkAsRead` | `HubException` `ticket_not_accessible` |
| `GET /api/tickets/{id}/messages` | 404 |
| `POST /api/tickets/{id}/chat/attachments` | 404 |
| `MessageCreated` y `ReadReceiptUpdated` del ticket | Ya no llegan |

**Orden en `RemoveParticipant`** (propuesta): autorizar → persistir el retiro y su `AuditEntry` (HU-019) → `IChatConnectionRevoker.RevokeTicketAccessAsync` → responder. Si falla la persistencia, no se revoca nada. Si falla el revocador, el retiro se mantiene, se registra el error en el log y la revalidación por operación sigue bloqueando al retirado.

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar el puerto, el evento `TicketAccessRevoked` y su payload, la decisión de no cerrar la conexión, el orden dentro de `RemoveParticipant`, el comportamiento si falla el revocador y la regla para la reincorporación. Documentar en [[tiempo-real-signalr]] y registrar los nombres nuevos en el [[glosario]].
- [ ] **T-02 — Caso de uso (prueba primero)** · Capa: Backend Application · Dificultad: Medio  
  Descripción: pruebas unitarias de `RemoveParticipant` con dobles de `ITicketRepository`, `IAuditLog` e `IChatConnectionRevoker`: retiro exitoso → revoca **después** de persistir (verificando el orden); falla la persistencia → no revoca; falla el revocador → el retiro se mantiene y el error se registra; retirar a una persona ya retirada → no revoca de nuevo (según HU-019). Añadir el puerto y la llamada.
- [ ] **T-03 — Adaptador revocador (prueba primero)** · Capa: Backend Api/Realtime · Dificultad: Medio  
  Descripción: pruebas unitarias con un doble de `IHubContext` y el registro real: saca del grupo todas las conexiones del usuario unidas a ese ticket, envía `TicketAccessRevoked` solo a ellas, deja intactas sus uniones a otros tickets y actualiza el registro. Implementar `SignalRChatConnectionRevoker` (nombre propuesto).
- [ ] **T-04 — Integración del hub con varios clientes (prueba primero)** · Capa: Backend Api (pruebas) · Dificultad: Alto  
  Descripción: `WebApplicationFactory<Program>`, PostgreSQL real y clientes SignalR .NET: Laura unida a T; Kevin unido a T desde dos conexiones y a U desde una; Elizabeth retira a Kevin de T por REST. Comprobar: Kevin recibe `TicketAccessRevoked` en sus dos conexiones de T; un mensaje posterior de Laura en T no le llega; sigue recibiendo eventos de U; `SendMessage`, `MarkAsRead` y `JoinTicket` en T fallan; `GET` del historial da 404; Laura no se ve afectada y sigue viendo los mensajes antiguos de Kevin.
- [ ] **T-05 — Gateway** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: `ticketsHubGateway.onTicketAccessRevoked(handler)`; transición del reductor a `accessDenied` al recibir el evento del ticket abierto (los eventos de otro ticket se ignoran), probada con Vitest.
- [ ] **T-06 — Controlador y vista** · Capa: Frontend controllers/views · Dificultad: Bajo  
  Descripción: `useTicketChatController` limpia los mensajes y adjuntos en memoria, descarta los pendientes, oculta el compositor y deja de marcar lecturas. La vista muestra "Ya no tienes acceso a este chat" (puro, con `role="status"`).

## Criterios de aceptación

### CHU-01 — Revocación de una conexión existente (dos usuarios conectados)

**Dado** Laura y Kevin unidos al chat del ticket T en navegadores distintos  
**Cuando** Elizabeth retira a Kevin de T y el endpoint responde con éxito  
**Entonces** Kevin recibe `TicketAccessRevoked` y su vista muestra "Ya no tienes acceso a este chat" sin mensajes; el siguiente mensaje que Laura envía a T **no** le llega como `MessageCreated`, y Laura sí lo ve.

### CHU-02 — Operaciones posteriores rechazadas

**Dado** Kevin retirado de T y con la conexión todavía abierta  
**Cuando** invoca `SendMessage(T, …)`, `MarkAsRead(T, …)` o `JoinTicket(T)`, pide `GET /api/tickets/T/messages` o sube un adjunto al chat de T  
**Entonces** las operaciones del hub fallan con `ticket_not_accessible`, las REST responden 404, no se persiste nada y nadie recibe eventos derivados.

### CHU-03 — Conexiones futuras y reconexión

**Dado** Kevin retirado de T  
**Cuando** abre una pestaña nueva, refresca o su cliente se reconecta automáticamente  
**Entonces** puede conectarse al hub (sigue siendo usuario interno), pero `JoinTicket(T)` se rechaza y no recupera ningún mensaje de T (CA-06, CA-08).

### CHU-04 — Todas las pestañas del retirado, sin afectar a otros tickets

**Dado** Kevin unido a T desde dos pestañas y al ticket U desde una de ellas  
**Cuando** se le retira de T  
**Entonces** ambas conexiones salen del grupo `ticket:T` y reciben `TicketAccessRevoked`, y la que estaba en U sigue recibiendo `MessageCreated` de U.

### CHU-05 — La historia se conserva

**Dado** Kevin, que envió mensajes y marcó lecturas en T antes de ser retirado  
**Cuando** Laura carga el historial de T y Elizabeth consulta la bitácora del ticket ([[hu-012-consultar-bitacora-del-ticket|HU-012]])  
**Entonces** los mensajes y lecturas de Kevin siguen visibles con su nombre, y la bitácora muestra el retiro con actor, fecha y valores anterior/nuevo (registrado por HU-019).

### CHU-06 — Persistir el retiro antes de revocar

**Dado** `RemoveParticipant` con un repositorio que falla al guardar el retiro  
**Cuando** Elizabeth intenta retirar a Kevin  
**Entonces** `IChatConnectionRevoker` no se invoca, Kevin sigue como participante vigente y conserva su acceso. Si el retiro se guarda pero el revocador falla, el retiro se mantiene y la siguiente operación de Kevin se rechaza igualmente.

### CHU-07 — Administrador retirado y Elizabeth

**Dado** un `Administrator` asociado a T y Elizabeth registrada además como `TicketParticipant` de T  
**Cuando** se retira a ambos como participantes  
**Entonces** el administrador pierde el acceso igual que cualquier participante (no lo conserva por ser administrador). Elizabeth conserva el acceso por su rol `ProductManager` (PRD §5.3) y no recibe `TicketAccessRevoked`.

### CHU-08 — Reincorporación

**Dado** Kevin retirado de T  
**Cuando** Elizabeth lo vuelve a agregar ([[hu-018-asignar-y-agregar-participantes|HU-018]])  
**Entonces** Kevin puede volver a unirse y el historial le devuelve todos los mensajes, incluidos los enviados mientras estuvo retirado (propuesta, coherente con PRD §5.4).

## Definition of Done

- [ ] DoD-01 — CHU-01 a CHU-08 validados con evidencia registrada en la matriz.
- [ ] DoD-02 — Pruebas unitarias escritas primero y en verde con `cd backend && dotnet test`: `RemoveParticipant_Exito_RevocaDespuesDePersistir`, `RemoveParticipant_FallaPersistencia_NoRevoca`, `RemoveParticipant_FallaRevocador_MantieneRetiro`, `Revoker_SacaTodasLasConexionesDelTicket`, `Revoker_NoAfectaOtrosTickets`.
- [ ] DoD-03 — Pruebas de integración del hub en verde con `WebApplicationFactory<Program>`, varios clientes SignalR .NET y PostgreSQL real: `Retiro_ConexionActiva_DejaDeRecibirMensajes`, `Retiro_OperacionesPosteriores_Rechazadas`, `Retiro_HistorialDevuelve404`, `Retiro_DosPestanas_AmbasRevocadas`, `Retiro_OtroTicketIntacto`, `Retiro_PM_ConservaAccesoPorRol`, `Reincorporacion_RecuperaAcceso`.
- [ ] DoD-04 — `DataTicket.ArchitectureTests` en verde: `IChatConnectionRevoker` en `Application/Ports/Out` y su adaptador en `Api`.
- [ ] DoD-05 — Frontend: `npm --prefix frontend run lint` sin errores, `npm --prefix frontend test` en verde (reductor ante `TicketAccessRevoked` del ticket abierto y de otro ticket) y `npm --prefix frontend run build` correcto.
- [ ] DoD-06 — `docker compose up --build` levanta el stack. Verificación manual con tres navegadores (Elizabeth, Laura y Kevin) en `http://localhost:5173`: retiro en vivo, Kevin ve "sin acceso" y deja de recibir los mensajes de Laura.
- [ ] DoD-07 — La auditoría del retiro (HU-019) sigue registrando actor, fecha, acción, objeto y valores anterior/nuevo; la revocación no crea entradas que alteren las existentes.
- [ ] DoD-08 — Wiki actualizada mediante Notas para la wiki: [[tiempo-real-signalr]] (sección *Revocación*, evento `TicketAccessRevoked`), [[chat-interno]], [[backend-hexagonal]] (estado de `IChatConnectionRevoker`), [[glosario]].
- [ ] DoD-09 — Revisión de `quality-reviewer` sin hallazgos CRÍTICO o ALTO abiertos (carreras, fuga de eventos, revalidación).
- [ ] DoD-10 — PR revisado y aprobado por otra persona del equipo.
- [ ] DoD-11 — Trazabilidad de la HU y de [[ep-009-chat-interno-en-tiempo-real]] actualizada; en las Notas para la wiki se indica si PRD CA-06 y CA-08 pueden marcarse en [[criterios-de-aceptacion]] (junto con HU-026 a HU-030).

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

## Notas y decisiones

> [!danger] Riesgo aceptado
> Ventana de carrera: un mensaje persistido justo antes de confirmar el retiro puede publicarse después y llegar todavía a la conexión del retirado, porque se creó mientras estaba autorizado. Garantía propuesta: ningún mensaje persistido **después** de que el endpoint de retiro responda con éxito llega a sus conexiones. Lo valida `quality-reviewer`.

- **Sacar del grupo en vez de cerrar la conexión:** cumple "dejar sin autorización" (PRD §7) sin cortar el tiempo real de otros tickets abiertos en la misma conexión.
- **Desactivación de cuenta con conexión abierta:** el PRD no la regula para el chat. La cookie de Identity sigue siendo válida hasta la próxima validación del *security stamp*. Pregunta propuesta para [[pendientes]].
- **Reincorporación:** el correo de vinculación al volver a agregar a alguien sigue abierto (V-08, [[hu-038-correo-de-vinculacion|HU-038]]).
- Las URL públicas de adjuntos que el retirado ya vio siguen funcionando ([[adr-0006-urls-publicas-azure-blob]]).

## Relacionado

- [[ep-009-chat-interno-en-tiempo-real]] · [[tablero-scrum]] · [[criterios-de-aceptacion]]
- [[chat-interno]] · [[tiempo-real-signalr]] · [[roles-y-permisos]] · [[auditoria]] · [[backend-hexagonal]] · [[notificaciones]] · [[adr-0005-signalr-para-chat]] · [[adr-0006-urls-publicas-azure-blob]] · [[pendientes]]
