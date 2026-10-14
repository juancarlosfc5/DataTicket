---
title: Tiempo real con SignalR
type: arquitectura
status: vigente
tags: [arquitectura/backend, arquitectura/frontend, signalr, producto/chat]
sources: ["PRD.md §7", "Decisión de la persona líder (2026-10-07): usar SignalR para el chat"]
aliases: [SignalR, Hub de tickets, Diseño técnico del chat]
created: 2026-10-07
updated: 2026-10-14
---

# Tiempo real con SignalR

Diseño técnico del [[chat-interno]]. **ASP.NET Core SignalR** distribuye eventos; **PostgreSQL guarda el historial** (PRD §7). Decisión en [[adr-0005-signalr-para-chat]]. Aún no implementado; la ruta de red (`/hubs` con upgrade a WebSocket por Vite y nginx) ya está verificada.

## Componentes

| Pieza | Ubicación | Nota |
|---|---|---|
| Hub `TicketsHub` en `/hubs/tickets` | `backend/src/DataTicket.Api/Realtime` | `[Authorize]`, cookie de Identity |
| Puerto `ITicketChatNotifier` | `Application/Ports/Out` | Lo implementa `Api/Realtime` con `IHubContext` |
| Casos de uso del chat | `Application` | Autorizan, persisten y luego notifican |
| API de historial por cursor | `GET /api/tickets/{id}/messages?after=<cursor>` | Carga inicial y recuperación tras reconexión |
| Cliente `@microsoft/signalr` | `frontend/src/core/realtime` | `withAutomaticReconnect()` |

Operaciones del hub: `JoinTicket`, `LeaveTicket`, `SendMessage`, `MarkAsRead`. Eventos al cliente: `MessageCreated`, `ReadReceiptUpdated`. Los nombres y el JSON definitivo se fijan al implementar (PRD §7) y se documentan aquí.

## Envío de un mensaje

```mermaid
sequenceDiagram
    participant C as Cliente React
    participant H as TicketsHub
    participant UC as SendChatMessage (caso de uso)
    participant DB as PostgreSQL
    participant G as Grupo ticket:{id}
    C->>H: SendMessage(ticketId, texto, adjuntos)
    H->>UC: ejecutar con ICurrentUser
    UC->>DB: ¿participante vigente? (PM siempre)
    alt no autorizado
        UC-->>H: rechazo
        H-->>C: error
    else autorizado
        UC->>DB: guardar mensaje + adjuntos
        UC->>G: MessageCreated (vía ITicketChatNotifier)
        G-->>C: MessageCreated a participantes conectados
    end
```

## Reconexión

1. React carga el historial por API y guarda el cursor del último mensaje.
2. Conecta al hub y llama `JoinTicket`; el servidor valida la participación antes de añadir la conexión al grupo.
3. En `onreconnected`: `JoinTicket` de nuevo y `GET .../messages?after=<cursor>` para recuperar lo perdido (PRD §7.6, CA-05).

## Reglas de seguridad

- **Los grupos no son seguridad** (PRD §7): se valida participación en cada `JoinTicket`, `SendMessage` y `MarkAsRead`.
- **Persistir antes de publicar** (PRD §7.2).
- **Revocación**: al retirar un participante se desconectan o desautorizan sus conexiones activas (registro conexión → usuario) y no puede reconectarse (CA-06).
- Notificaciones dentro de la app por mensaje nuevo; **sin correo por mensaje** ([[notificaciones]]).

## Usuario actual en el hub

> [!info] Decisión técnica (2026-10-14, orquestador; sin impacto de producto)
> El adaptador HTTP de `ICurrentUser` (`HttpContextCurrentUser`, B4) lee la petición HTTP en curso; dentro de un hub de SignalR esa petición no existe en cada invocación, y la identidad de la conexión se fija una sola vez, al conectar. Por eso el hub `/hubs/tickets` (`loop_chat.md`):
> - obtiene el usuario de `Context.User` (la misma cookie `dataticket.auth`, validada al conectar) mediante un adaptador de `ICurrentUser` propio del hub, en `Api`, que reutiliza la lectura de claims de `PrincipalProfile`;
> - **revalida en cada operación** (`JoinTicket`, enviar, marcar leído…) contra la base que la cuenta sigue `Active` y que es participante vigente del ticket (invariante 3 de `AGENTS.md` §7): nunca confía solo en lo capturado al conectar;
> - si la cuenta se desactiva o deja de ser participante, la operación falla cerrada y la conexión sale del grupo con `TicketAccessRevoked` (P-13).
> Para la persona usuaria no cambia nada visible; es cómo el chat sabe, en cada mensaje, quién escribe y si todavía puede hacerlo.

## Red y despliegue

- Desarrollo: `vite.config.ts` → `'/hubs': { target, ws: true }`.
- Runtime: `frontend/nginx/default.conf.template` → `Upgrade`/`Connection`, `proxy_read_timeout 1h`, `proxy_buffering off`.
- Una sola instancia: sin Redis/backplane en el MVP; al escalar, Redis o Azure SignalR (PRD §7, [[pendientes]]).

## Núcleo disponible para el chat (N1, etiqueta `core-nucleo-v1`)

- Puerto `IChatConnectionRevoker.RevokeTicketAccessAsync(ticketId, userId, ct)` con adaptador nulo; HU-032 lo reemplaza con `services.Replace` y actualiza la prueba de composición que exige un único registro.
- La validación por operación del hub usa `ITicketAccessPolicy` (sustituye al `ChatAccessPolicy` propuesto en HU-026 T-03).
- Fixtures T03 (`TicketSeed`, `TestTickets`) con los IDs de `db.sql` §13: Laura y Juan David vigentes, Brayan retirado, Kevin no participante.

## Relacionado

- [[chat-interno]] · [[adr-0005-signalr-para-chat]] · [[autenticacion-identity]] · [[backend-hexagonal]] · [[frontend-mvc]]
