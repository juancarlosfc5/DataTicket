---
title: "HU-037 — Notificaciones en la app por mensajes nuevos"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/notificaciones, producto/chat, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §7", "PRD.md §11", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-037", "Notificaciones en la app por mensajes nuevos", "Notificaciones in-app"]
epica: "[[ep-011-notificaciones]]"
criterios_prd: ["CA-06"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Hub SignalR", "Persistencia PostgreSQL", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 6"
dependencias: ["[[hu-028-enviar-y-recibir-mensajes]]", "[[hu-027-unirse-al-chat-del-ticket]]", "[[hu-032-revocar-acceso-al-retirar-participante]]", "[[hu-002-shell-y-navegacion-por-rol]]"]
relacionadas: ["[[hu-030-confirmaciones-de-lectura]]", "[[hu-031-recuperar-mensajes-tras-reconexion]]", "[[hu-038-correo-de-vinculacion]]", "[[hu-019-reasignar-y-retirar-participantes]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-037 — Notificaciones en la app por mensajes nuevos

Cada mensaje nuevo del chat genera una notificación dentro de la aplicación para los participantes vigentes del ticket, excepto su autor: contador de no leídas y lista, en tiempo real y persistida. No se envía correo por mensaje y las personas retiradas no reciben avisos (PRD §7.6, §11).

## Historia de usuario

**COMO** participante interno de un ticket (Desarrollo, Producción o administrador asociado)  
**QUIERO** ver un aviso dentro de DataTicket cuando llega un mensaje nuevo en el chat de mis tickets  
**PARA** enterarme a tiempo sin tener cada ticket abierto y sin llenar mi correo

## Contexto

PRD §7.6: "Los mensajes nuevos generan notificación dentro de la aplicación. No se envía un correo por cada mensaje del chat". PRD §11 añade que la política debe evitar notificar al usuario retirado por mensajes futuros. El evento `MessageCreated` del hub solo llega a las conexiones unidas al grupo del ticket ([[hu-027-unirse-al-chat-del-ticket|HU-027]]); quien no tiene abierto el ticket no lo recibe. Por eso se propone una notificación persistida por destinatario y un evento dirigido a la persona (`Clients.User`), no al grupo.

## Alcance

- Entidad persistida de notificación por (mensaje, destinatario) (nombre propuesto `InAppNotification`, ratificar en T-01 y registrar en el glosario).
- Creación en la misma transacción que el `ChatMessage`, antes de publicar (coherente con "persistir antes de publicar").
- Destinatarios: participantes vigentes del ticket al momento del mensaje, excepto el autor.
- Evento de hub dirigido al usuario (nombre propuesto `NotificationCreated`).
- API para listar, contar no leídas y marcar como leídas (una, todas, o las de un ticket).
- Indicador con contador en el shell y panel con la lista; al hacer clic se abre el ticket.
- Al leer los mensajes en el chat ([[hu-030-confirmaciones-de-lectura|HU-030]]) se marcan leídas las notificaciones de esos mensajes (propuesta).
- Ocultar las notificaciones de tickets donde la persona ya no es participante vigente.

## Fuera de alcance

- Correo por mensaje (PRD §7.6, §11).
- Notificaciones push del navegador o del sistema operativo; app móvil (PRD §13).
- Notificaciones por cambio de estado, asignación u otros eventos (no acordadas en PRD §11 como in-app).
- Notificaciones para clientes (no participan en el chat, PRD §7).
- Preferencias o silenciado por usuario (no aparecen en el PRD).
- Correo de vinculación → [[hu-038-correo-de-vinculacion|HU-038]].

## Requisitos y reglas de negocio

- Los mensajes nuevos generan notificación dentro de la aplicación para participantes del ticket; sin correo por mensaje (PRD §7.6, §11).
- No se notifica al usuario retirado por mensajes futuros (PRD §11).
- Solo participantes vigentes leen el chat; Elizabeth siempre puede (PRD §5.3).
- El servidor guarda cada mensaje antes de confirmar su publicación (PRD §7.2).
- Propuesta: Elizabeth recibe notificaciones solo de los tickets donde es participante explícita (tiene acceso a todos los chats; notificarla por todos podría saturarla). Confirmar con ella.
- Propuesta: la vista previa del mensaje se limita a 100 caracteres y omite adjuntos.
- Propuesta: el autor nunca recibe notificación de su propio mensaje.

## Invariantes en juego

- Invariante 3 (`AGENTS.md` §7): solo participantes vigentes; los grupos de SignalR no son seguridad, por eso el aviso se dirige al usuario y se valida participación al crear y al listar.
- Invariante 4: el mensaje (y sus notificaciones) se persiste antes de publicarse.
- Invariante 2: ningún cliente recibe notificaciones del chat.

## Criterios del PRD cubiertos

- PRD CA-06 (complementario: la persona retirada tampoco recibe avisos nuevos ni ve los anteriores del ticket) → [[criterios-de-aceptacion]]
- Requisitos verificados además contra PRD §7.6 y §11, que no tienen un CA propio.

## Dependencias y relaciones

- Épica: [[ep-011-notificaciones]]
- Dependencias: [[hu-028-enviar-y-recibir-mensajes|HU-028]] (caso de uso `SendChatMessage`), [[hu-027-unirse-al-chat-del-ticket|HU-027]] (hub autenticado), [[hu-032-revocar-acceso-al-retirar-participante|HU-032]] (retiro y revocación), [[hu-002-shell-y-navegacion-por-rol|HU-002]] (espacio del indicador).
- Relacionadas: [[hu-030-confirmaciones-de-lectura|HU-030]] (lectura de mensajes), [[hu-031-recuperar-mensajes-tras-reconexion|HU-031]] (recuperación tras reconexión), [[hu-038-correo-de-vinculacion|HU-038]], [[hu-019-reasignar-y-retirar-participantes|HU-019]].
- Decisiones: [[adr-0005-signalr-para-chat]]; abiertas: enrutador del frontend y backplane de SignalR ([[pendientes]] §4).

## Componentes afectados

- Backend (Domain): `InAppNotification` (propuesto).
- Backend (Application): ampliación de `SendChatMessage`; casos de uso `ListNotifications` y `MarkNotificationsRead` (nombres propuestos); puerto de salida para avisos por usuario (propuesta: ampliar `ITicketChatNotifier` con `NotifyUserAsync`, o puerto `IUserNotifier`; decidir en T-01).
- Backend (Infrastructure): configuración EF Core, índice `(recipient_user_id, read_at)` y migración.
- Backend (Api): endpoints REST y adaptador del hub `/hubs/tickets` (`Clients.User`).
- Frontend (models, controllers, views): módulo de notificaciones y `core/realtime`.

## Dificultad

**Nivel:** Alto

**Justificación:** cruza backend, hub y frontend; añade entrega dirigida por usuario en SignalR, persistencia en la transacción del mensaje, filtrado por participación vigente y sincronización con las confirmaciones de lectura.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01 junto con el contrato del hub de [[hu-028-enviar-y-recibir-mensajes|HU-028]].

**REST** (roles internos; clientes → 403):

`GET /api/notifications?unreadOnly=true&limit=20&before={cursor}` → `200`:

```json
{
  "unreadCount": 3,
  "items": [
    {
      "notificationId": "9a7b…",
      "ticketId": "1b0e7c52-8d4a-4f0e-b3a9-5c2d6e7f8a90",
      "ticketNumber": "DT-000123",
      "ticketTitle": "Error al generar factura electrónica",
      "messageId": "c3f0…",
      "authorDisplayName": "Laura",
      "messagePreview": "Ya subí la corrección a la rama de pruebas, ¿alguien puede…",
      "createdAt": "2026-10-07T18:02:11Z",
      "readAt": null
    }
  ],
  "nextCursor": "MjAyNi0xMC0wN1QxODowMjoxMVo"
}
```

- `limit`: 1–50 (por defecto 20); fuera de rango → 400.
- `POST /api/notifications/{notificationId}/read` → `204`; notificación de otro usuario o inexistente → `404`.
- `POST /api/notifications/read-all` → `204`.
- `POST /api/tickets/{ticketId}/notifications/read` → `204` (marca las del ticket; interno no participante o retirado → `404`, convención de [[hu-020-detalle-interno-del-ticket|HU-020]]).

**Hub `/hubs/tickets`** — evento nuevo (nombre propuesto) dirigido a `Clients.User(recipientUserId)`:

```json
// NotificationCreated
{ "notificationId": "9a7b…", "ticketId": "1b0e…", "ticketNumber": "DT-000123", "ticketTitle": "Error al generar factura electrónica", "messageId": "c3f0…", "authorDisplayName": "Laura", "messagePreview": "Ya subí la corrección…", "createdAt": "2026-10-07T18:02:11Z", "unreadCount": 3 }
```

Sin métodos nuevos invocables desde el cliente: marcar como leída va por REST. Tras reconexión, el cliente vuelve a pedir `GET /api/notifications` para sincronizar el contador.

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: ratificar entidad, endpoints, evento `NotificationCreated`, puerto de salida (ampliar `ITicketChatNotifier` o `IUserNotifier`), política para Elizabeth y vista previa; registrar los nombres en [[glosario]] y en [[tiempo-real-signalr]].
- [ ] **T-02 — Dominio** · Capa: Backend (Domain) · Dificultad: Bajo  
  Descripción: primero pruebas: `Notification_MarkRead_SetsReadAtOnce`, `Recipients_ExcludeAuthor`, `Recipients_ExcludeRemovedParticipants`. Luego `InAppNotification` y la regla de destinatarios.
- [ ] **T-03 — Creación al enviar mensaje** · Capa: Backend (Application) · Dificultad: Alto  
  Descripción: primero pruebas: `SendChatMessage_CreatesNotificationForEachActiveParticipantExceptAuthor`, `SendChatMessage_DoesNotNotifyRemovedParticipant`, `SendChatMessage_DoesNotCallEmailSender`, `SendChatMessage_WhenPersistenceFails_PublishesNothing`. Luego crear notificaciones en la misma unidad de trabajo y publicar `NotificationCreated` después del commit.
- [ ] **T-04 — Consulta y marcado** · Capa: Backend (Application/Infrastructure) · Dificultad: Medio  
  Descripción: primero pruebas: `ListNotifications_HidesTicketsWhereUserWasRemoved`, `MarkRead_OtherUsersNotification_ReturnsNotFound`, `MarkMessagesRead_AlsoMarksNotifications`. Luego casos de uso, configuración EF Core, índice y migración `AddInAppNotifications`.
- [ ] **T-05 — Endpoints y hub** · Capa: Backend (Api) · Dificultad: Medio  
  Descripción: primero pruebas de integración (REST con `WebApplicationFactory<Program>` y cliente SignalR de prueba): el destinatario recibe `NotificationCreated` aunque no haya hecho `JoinTicket`; el autor y el retirado no lo reciben; cliente → 403. Luego endpoints y adaptador con `Clients.User` (identificador de usuario por claim de Identity).
- [ ] **T-06 — Modelo y gateway** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: primero Vitest: normalización del DTO, recorte de vista previa, actualización del contador al recibir un evento duplicado (idempotencia por `notificationId`). Luego `notificationsGateway.ts` (REST) y suscripción en `core/realtime`.
- [ ] **T-07 — Controlador** · Capa: Frontend (controllers) · Dificultad: Medio  
  Descripción: `useNotificationsController`: carga inicial, suscripción al evento, resincronización en `onreconnected`, `markRead`, `markAllRead`, estado de error.
- [ ] **T-08 — Vistas** · Capa: Frontend (views) · Dificultad: Bajo  
  Descripción: `NotificationBellView` (contador accesible con `aria-label`, "9+" por encima de 9) y `NotificationListView` (autor, ticket, vista previa, hora relativa); clic → abre el ticket y marca la notificación.

## Criterios de aceptación

### CHU-01 — Aviso a los participantes vigentes, salvo el autor

**Dado** un ticket con participantes vigentes Laura (autora), Brayan y Julián, y Brayan sin el ticket abierto  
**Cuando** Laura envía un mensaje  
**Entonces** se persisten dos notificaciones (Brayan y Julián), ambos reciben `NotificationCreated` en tiempo real y su contador sube en 1; Laura no recibe notificación.

### CHU-02 — Sin correo por mensaje

**Dado** un ticket con cinco participantes vigentes y Mailpit vacío  
**Cuando** se envían diez mensajes  
**Entonces** Mailpit sigue sin correos nuevos y la prueba `SendChatMessage_DoesNotCallEmailSender` pasa.

### CHU-03 — Persona retirada no recibe avisos ni los ve

**Dado** Brayan retirado del ticket ([[hu-019-reasignar-y-retirar-participantes|HU-019]]) con dos notificaciones previas sin leer de ese ticket  
**Cuando** otro participante envía un mensaje nuevo  
**Entonces** no se crea notificación para Brayan, no recibe `NotificationCreated`, y `GET /api/notifications` ya no le devuelve las dos anteriores de ese ticket ni las cuenta en `unreadCount`.

### CHU-04 — Persistencia antes de publicar

**Dado** que la escritura en PostgreSQL falla al guardar el mensaje  
**Cuando** se procesa `SendMessage`  
**Entonces** no se publica `MessageCreated` ni `NotificationCreated` y no queda ninguna notificación huérfana.

### CHU-05 — Aislamiento entre usuarios

**Dado** una notificación de Brayan  
**Cuando** Julián llama `POST /api/notifications/{id}/read` con ese identificador  
**Entonces** recibe `404` y la notificación de Brayan sigue sin leer.

### CHU-06 — Clientes y no participantes

**Dado** un solicitante autenticado y un desarrollador que no es participante del ticket  
**Cuando** el solicitante llama `GET /api/notifications` y el desarrollador llama `POST /api/tickets/{ticketId}/notifications/read`  
**Entonces** el solicitante recibe `403` (rol cliente) y el desarrollador recibe `404` sin datos del ticket (convención de [[hu-020-detalle-interno-del-ticket|HU-020]]).

### CHU-07 — Marcado de leídas y sincronización con el chat

**Dado** Brayan con tres notificaciones sin leer del ticket T  
**Cuando** abre el chat de T y sus mensajes quedan marcados como leídos ([[hu-030-confirmaciones-de-lectura|HU-030]])  
**Entonces** las tres notificaciones quedan con `readAt` y `unreadCount` baja en 3; `POST /api/notifications/read-all` deja `unreadCount` en 0.

### CHU-08 — Límites de paginación

**Dado** Brayan con 25 notificaciones  
**Cuando** pide `limit=20`, luego `before={nextCursor}`, y por último `limit=0` y `limit=51`  
**Entonces** recibe 20, luego 5 sin repetir, y `400` para `0` y `51`.

### CHU-09 — Reconexión y contador en la interfaz

**Dado** Brayan desconectado del hub mientras llegan dos mensajes  
**Cuando** la conexión se recupera  
**Entonces** el controlador vuelve a consultar `GET /api/notifications` y el contador muestra el valor correcto sin duplicar; si la consulta falla, el indicador muestra un estado de error discreto y reintenta.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-09 validados con evidencia en la matriz.
- [ ] **DoD-02** — Pruebas escritas primero y en verde (`cd backend && dotnet test`): `SendChatMessage_CreatesNotificationForEachActiveParticipantExceptAuthor`, `SendChatMessage_DoesNotNotifyRemovedParticipant`, `SendChatMessage_DoesNotCallEmailSender`, `ListNotifications_HidesTicketsWhereUserWasRemoved`, `MarkRead_OtherUsersNotification_ReturnsNotFound` e integración del hub con cliente SignalR.
- [ ] **DoD-03** — `DataTicket.ArchitectureTests` en verde (`Application` no referencia SignalR).
- [ ] **DoD-04** — Migración `AddInAppNotifications` con índice por destinatario creada y aplicada en local.
- [ ] **DoD-05** — Frontend: Vitest del modelo y del manejo de eventos en verde, `npm --prefix frontend run lint` (sin `@microsoft/signalr` en vistas) y `npm --prefix frontend run build` correctos.
- [ ] **DoD-06** — Contrato REST y evento del hub documentados y coherentes entre backend y frontend.
- [ ] **DoD-07** — `docker compose up --build` y verificación manual con dos navegadores: aviso en tiempo real sin abrir el ticket; Mailpit sin correos.
- [ ] **DoD-08** — Wiki actualizada: [[notificaciones]], [[tiempo-real-signalr]] (evento `NotificationCreated` y entrega por usuario), [[chat-interno]], [[glosario]] (`InAppNotification`), [[persistencia-postgresql]]; política para Elizabeth registrada en [[pendientes]].
- [ ] **DoD-09** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO y PR revisado por otra persona del equipo.
- [ ] **DoD-10** — La trazabilidad de la HU y de [[ep-011-notificaciones]] está actualizada.

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

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- **Propuesta:** notificaciones persistidas por destinatario (no solo eventos efímeros), para sobrevivir a cierres de sesión y reconexiones.
- **Propuesta:** evento `NotificationCreated` dirigido al usuario, no al grupo del ticket.
- **Propuesta:** Elizabeth solo recibe avisos de tickets donde es participante explícita.
- **Alinear con [[hu-028-enviar-y-recibir-mensajes|HU-028]] en T-01:** HU-028 prevé que la notificación in-app se enganche después de persistir y que un fallo de publicación no invalide el envío. Aquí se propone guardar las notificaciones en la misma unidad de trabajo que el mensaje y publicar `NotificationCreated` después del commit; un fallo al publicar el evento solo se registra en logs (la lista se recupera por REST).
- **Riesgo:** con varias instancias, la entrega por usuario requiere backplane (decisión abierta; el MVP es de una instancia).

## Relacionado

- [[ep-011-notificaciones]] · [[tablero-scrum]] · [[notificaciones]] · [[chat-interno]] · [[tiempo-real-signalr]] · [[adr-0005-signalr-para-chat]] · [[frontend-mvc]] · [[persistencia-postgresql]] · [[criterios-de-aceptacion]]
