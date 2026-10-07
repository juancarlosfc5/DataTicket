---
title: "HU-027 — Conectarse y unirse al chat del ticket"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/chat, signalr, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §7", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-027", "Conectarse y unirse al chat del ticket"]
epica: "[[ep-009-chat-interno-en-tiempo-real]]"
criterios_prd: ["CA-05", "CA-06"]
componentes: ["Hub SignalR", "Backend (Application, Api)", "Identity", "Frontend (core/realtime, models, controllers, views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 5"
dependencias: ["[[hu-003-iniciar-y-cerrar-sesion]]", "[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-026-historial-del-chat-por-cursor]]"]
relacionadas: ["[[hu-028-enviar-y-recibir-mensajes]]", "[[hu-031-recuperar-mensajes-tras-reconexion]]", "[[hu-032-revocar-acceso-al-retirar-participante]]", "[[hu-020-detalle-interno-del-ticket]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-027 — Conectarse y unirse al chat del ticket

El frontend abre una conexión SignalR autenticada con `/hubs/tickets` y se une al chat de un ticket. El servidor valida la participación vigente **antes** de añadir la conexión al grupo `ticket:{id}` y lleva un registro de conexiones por usuario, que luego usa la revocación (PRD §7, flujos 2 y 3).

## Historia de usuario

**COMO** participante interno vigente de un ticket (o Elizabeth, PM)  
**QUIERO** que al abrir el ticket mi navegador se conecte y se una a su chat en tiempo real  
**PARA** recibir al instante lo que escriben los demás participantes, sin que nadie no autorizado pueda escuchar

## Contexto

El PRD fija SignalR con el cliente `@microsoft/signalr` y un hub autenticado `/hubs/tickets` (PRD §7). El cliente "solicita unirse al ticket" y el servidor "valida la autorización vigente antes de agregar esa conexión al grupo". Los grupos solo enrutan eventos y no son una barrera de seguridad (PRD §7). La sesión es la cookie de Identity same-origin ([[adr-0004-autenticacion-cookie-mismo-origen]], todavía por aceptar): el hub no necesita token en la query string ([[autenticacion-identity]]). La ruta de red `/hubs` con upgrade a WebSocket ya está configurada en `frontend/vite.config.ts` (`'/hubs': { target, ws: true }`) y en `frontend/nginx/default.conf.template` (`Upgrade`/`Connection`, `proxy_read_timeout 1h`). Hoy no existen `TicketsHub`, `AddSignalR()` ni la dependencia `@microsoft/signalr` en `frontend/package.json`.

## Alcance

- `builder.Services.AddSignalR()` y `app.MapHub<TicketsHub>("/hubs/tickets")` en `backend/src/DataTicket.Api/Program.cs`; `TicketsHub` en `backend/src/DataTicket.Api/Realtime` con `[Authorize]`.
- Métodos del hub `JoinTicket(ticketId)` y `LeaveTicket(ticketId)`.
- Caso de uso de autorización de unión que reutiliza la regla de acceso de [[hu-026-historial-del-chat-por-cursor|HU-026]].
- Registro en memoria de conexión → usuario → tickets unidos (`ChatConnectionTracker`, nombre propuesto), alimentado por `OnConnectedAsync`, `JoinTicket`, `LeaveTicket` y `OnDisconnectedAsync`. Es la base de [[hu-032-revocar-acceso-al-retirar-participante|HU-032]].
- Errores del hub mediante `HubException` con código.
- Frontend: dependencia `@microsoft/signalr` (10.0.x, [[stack-y-versiones]]), conexión compartida en `frontend/src/core/realtime` con `withAutomaticReconnect()`, gateway del hub en `models/`, unión y salida al abrir y cerrar el ticket, e indicador básico de estado de conexión.

## Fuera de alcance

- Enviar mensajes ([[hu-028-enviar-y-recibir-mensajes|HU-028]]) y marcar lecturas ([[hu-030-confirmaciones-de-lectura|HU-030]]).
- Recuperación tras reconexión con `after` ([[hu-031-recuperar-mensajes-tras-reconexion|HU-031]]). Aquí la reconexión automática solo se configura.
- Revocación de conexiones al retirar ([[hu-032-revocar-acceso-al-retirar-participante|HU-032]]). Aquí solo se construye el registro que la permite.
- Backplane (Redis o Azure SignalR): una sola instancia en el MVP (PRD §7, §13).
- Presencia ("quién está en línea") e indicador de "escribiendo".

## Requisitos y reglas de negocio

- El hub es autenticado; todo acceso exige autenticación (PRD §7, §12).
- El servidor valida la autorización vigente antes de agregar la conexión al grupo del ticket (PRD §7, flujo 3).
- Solo participantes vigentes; Elizabeth siempre; un administrador solo si está asociado (PRD §5.3, §5.5).
- Los grupos de SignalR no son seguridad; la pertenencia se verifica en cada unión y operación (PRD §7).
- Sin backplane en la primera instalación de una sola instancia (PRD §7).

## Invariantes en juego

- AGENTS §7.2: el cliente nunca accede al chat interno.
- AGENTS §7.3: participación validada en el servidor en cada unión; los grupos no son seguridad.
- AGENTS §7.5: el administrador no accede por serlo.

## Criterios del PRD cubiertos

- PRD CA-06 (parcial: "un no participante no puede unirse al grupo") → [[criterios-de-aceptacion]]
- PRD CA-05 (parcial: conexión en tiempo real de los participantes autorizados; la entrega de mensajes está en HU-028) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-009-chat-interno-en-tiempo-real]]
- Dependencias: [[hu-003-iniciar-y-cerrar-sesion|HU-003 — Iniciar y cerrar sesión]] (cookie de Identity), [[hu-004-contexto-de-usuario-y-autorizacion|HU-004 — Contexto de usuario y autorización]] (`ICurrentUser` en el hub), [[hu-026-historial-del-chat-por-cursor|HU-026 — Historial del chat paginado por cursor]] (regla de acceso al chat).
- Relacionadas: [[hu-028-enviar-y-recibir-mensajes|HU-028]], [[hu-031-recuperar-mensajes-tras-reconexion|HU-031]], [[hu-032-revocar-acceso-al-retirar-participante|HU-032]], [[hu-020-detalle-interno-del-ticket|HU-020]].
- Decisiones: [[adr-0005-signalr-para-chat]] (aceptada); [[adr-0004-autenticacion-cookie-mismo-origen]] (propuesta, **pendiente de aceptar**); backplane solo al escalar ([[pendientes]] §4).

## Componentes afectados

- Hub SignalR: `backend/src/DataTicket.Api/Realtime/TicketsHub.cs`, registro de conexiones y mapeo de errores a `HubException`.
- Backend Application: caso de uso de unión (`JoinTicketChat`, nombre propuesto: ratificar en T-01 y registrar en el glosario) sobre la regla de acceso de HU-026.
- Backend Api: `Program.cs` (`AddSignalR`, `MapHub`); `ICurrentUser` resuelto desde `Context.User` en el hub.
- Identity: autenticación por cookie en la negociación (`/hubs/tickets/negotiate`) y en el WebSocket.
- Frontend: `src/core/realtime/` (conexión), `src/modules/chat/models/ticketsHubGateway.ts` (nombre propuesto), controlador y vista de estado de conexión.
- Pruebas: proyecto de integración con `Microsoft.AspNetCore.SignalR.Client` (paquete por añadir en `backend/Directory.Packages.props`).

## Dificultad

**Nivel:** Alto

**Justificación:** es transversal: hub, autenticación por cookie en SignalR, autorización por ticket, registro de conexiones concurrente, cliente de tiempo real en el frontend y pruebas de integración con clientes SignalR reales. Además depende de un ADR todavía no aceptado.

## Contrato backend ↔ frontend

> [!info] Propuesta
> Nombres y payloads según [[tiempo-real-signalr]]. Se ratifican en T-01 y se documentan allí.

**Endpoint:** `/hubs/tickets` (negociación en `POST /hubs/tickets/negotiate`). Autenticación: cookie de Identity same-origin. Política del hub: usuario autenticado de personal interno (`InternalStaff`, nombre propuesto). La negociación sin sesión responde 401; la de un usuario cliente, 403 (propuesta).

**Grupo:** `ticket:{ticketId}` (solo enruta eventos).

| Método (cliente → servidor) | Argumentos | Resultado | Errores (`HubException`) |
|---|---|---|---|
| `JoinTicket` | `ticketId: string (Guid)` | `{ "ticketId": "…", "joinedAt": "2026-10-07T15:04:05.123456Z" }` | `ticket_not_accessible` |
| `LeaveTicket` | `ticketId: string (Guid)` | `null`, idempotente (no falla si no estaba unido) | — |

Ejemplo de invocación desde el gateway:

```ts
await connection.invoke("JoinTicket", "01928b77-1c2d-7e0f-a3b4-c5d6e7f80912");
// → { ticketId: "01928b77-…", joinedAt: "2026-10-07T15:04:05.123456Z" }
```

**Códigos de error del hub** (propuesta, comunes a todo el chat): el `HubException` lleva como mensaje un código estable que el cliente traduce en `models/` (`parseHubError`, nombre propuesto).

| Código | Significado |
|---|---|
| `ticket_not_accessible` | Ticket inexistente o sin permiso: no participante, retirado, cliente o administrador no asociado. Equivale al 404 REST |
| `validation_failed` | Argumentos inválidos (lo usan HU-028, HU-029 y HU-030) |
| `internal_error` | Fallo no esperado; el servidor no expone detalles (`EnableDetailedErrors` desactivado fuera de Development) |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: ratificar ruta, política del hub (401/403 en la negociación), firmas de `JoinTicket` y `LeaveTicket`, nombre del grupo, códigos de error del hub y forma del resultado. Documentarlo en [[tiempo-real-signalr]] y los nombres nuevos en el [[glosario]].
- [ ] **T-02 — Caso de uso de unión (prueba primero)** · Capa: Backend Application · Dificultad: Bajo  
  Descripción: pruebas unitarias con dobles de `ICurrentUser` y del repositorio de tickets: participante vigente y PM autorizados; retirado, no participante, administrador no asociado y cliente rechazados. Implementar `JoinTicketChat` (nombre propuesto) sobre la regla de HU-026.
- [ ] **T-03 — Registro de conexiones (prueba primero)** · Capa: Backend Api/Realtime · Dificultad: Medio  
  Descripción: pruebas unitarias del registro (añadir conexión, unir a ticket, salir, desconectar, consultar conexiones de un usuario en un ticket, concurrencia). Implementar `ChatConnectionTracker` (nombre propuesto, en memoria, singleton) en `Api/Realtime`.
- [ ] **T-04 — Hub `TicketsHub` y pruebas de integración** · Capa: Backend Api/Realtime · Dificultad: Alto  
  Descripción: añadir `Microsoft.AspNetCore.SignalR.Client` y, si no existe, `Microsoft.AspNetCore.Mvc.Testing` a `backend/Directory.Packages.props` y al proyecto de pruebas de integración (y a la etapa `restore` del `backend/Dockerfile` si el proyecto es nuevo). Escribir primero las pruebas con `WebApplicationFactory<Program>`, PostgreSQL real y clientes `HubConnection` autenticados con la cookie de sesión (o con un esquema de autenticación de prueba, a decidir en T-01): negociación anónima → 401, cliente → 403, `JoinTicket` de participante → OK, de no participante, retirado o admin no asociado → `HubException` `ticket_not_accessible` sin entrar al grupo. Después, el hub con `[Authorize]`, `AddSignalR()`, `MapHub` y el mapeo de excepciones.
- [ ] **T-05 — Conexión compartida** · Capa: Frontend core/realtime · Dificultad: Medio  
  Descripción: añadir `@microsoft/signalr` 10.0.x a `frontend/package.json`. Crear `src/core/realtime/ticketsHubConnection.ts` (nombre propuesto) con `HubConnectionBuilder().withUrl("/hubs/tickets").withAutomaticReconnect()`, una única conexión por pestaña, `start`/`stop` y suscripción a `onreconnecting`/`onreconnected`/`onclose`. Ruta relativa, sin `localhost:8080`.
- [ ] **T-06 — Gateway del hub** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: `ticketsHubGateway.joinTicket(ticketId)` / `leaveTicket(ticketId)` sobre `core/realtime`, y `parseHubError(error)` → código de dominio, probado con Vitest. Modelo puro `ConnectionStatus` (`connecting | connected | reconnecting | disconnected`) con su reductor, también probado con Vitest.
- [ ] **T-07 — Controlador** · Capa: Frontend controllers · Dificultad: Medio  
  Descripción: extender `useTicketChatController` (HU-026): al montar, iniciar la conexión y llamar a `JoinTicket`; al desmontar o cambiar de ticket, `LeaveTicket`. Exponer `connectionStatus` y `accessDenied`. Sin JSX.
- [ ] **T-08 — Vista de estado** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `ConnectionStatusBanner` (nombre propuesto), puro, con `role="status"`: "Conectando…", "En línea", "Reconectando…", "Sin conexión". Muestra "No tienes acceso a este chat" si la unión se rechaza. No importa `@microsoft/signalr` ni `core/` (lo comprueba `oxlint`).

## Criterios de aceptación

### CHU-01 — Conexión anónima rechazada

**Dado** un navegador o cliente SignalR sin cookie de sesión  
**Cuando** intenta negociar con `/hubs/tickets`  
**Entonces** el servidor responde 401 y no se establece ninguna conexión ni se registra en el registro de conexiones.

### CHU-02 — Participante vigente se une

**Dado** Laura, participante vigente del ticket T, autenticada  
**Cuando** su cliente se conecta e invoca `JoinTicket(T)`  
**Entonces** recibe `{ ticketId, joinedAt }`, su conexión queda en el grupo `ticket:T` y en el registro asociada a su usuario y a T.

### CHU-03 — Elizabeth se une sin ser participante

**Dado** Elizabeth (rol `ProductManager`), que no es `TicketParticipant` del ticket T  
**Cuando** invoca `JoinTicket(T)`  
**Entonces** la unión se acepta.

### CHU-04 — No participante, retirado y administrador no asociado rechazados

**Dado** el ticket T con Laura conectada y unida  
**Cuando** invocan `JoinTicket(T)`: Cristian (Desarrollo, no asociado), Kevin (retirado de T) o un `Administrator` no asociado  
**Entonces** cada uno recibe `HubException` con código `ticket_not_accessible`, su conexión **no** se añade al grupo `ticket:T`, y un evento publicado después en `ticket:T` (prueba con dos clientes: Laura emite un evento de prueba por el notificador) no llega a su conexión.

### CHU-05 — El cliente no accede al hub

**Dado** un usuario `Requester` o `CompanyCoordinator` autenticado  
**Cuando** intenta negociar con `/hubs/tickets` o, si llegara a conectarse, invoca `JoinTicket` sobre un ticket de su propia empresa  
**Entonces** la negociación responde 403 (propuesta) o, en su defecto, `JoinTicket` falla con `ticket_not_accessible`. En ningún caso entra al grupo.

### CHU-06 — La validación ocurre en cada unión

**Dado** Laura, que se unió a T y luego fue retirada en la base de datos (sin pasar por la revocación de HU-032)  
**Cuando** su conexión vuelve a invocar `JoinTicket(T)`  
**Entonces** se rechaza con `ticket_not_accessible`. La autorización se consulta en cada invocación y no se guarda en caché en la conexión.

### CHU-07 — Salir y desconectar limpian el registro

**Dado** Laura unida a T  
**Cuando** invoca `LeaveTicket(T)` (dos veces seguidas) o cierra la pestaña  
**Entonces** su conexión sale del grupo `ticket:T`, el registro deja de asociarla a T (o la elimina en `OnDisconnectedAsync`) y la segunda llamada a `LeaveTicket` no produce error.

### CHU-08 — UI ante rechazo y desconexión

**Dado** el detalle interno del ticket abierto  
**Cuando** la unión se rechaza, el backend se detiene o la red se corta  
**Entonces** la vista muestra "No tienes acceso a este chat" en el rechazo, "Reconectando…" durante los reintentos automáticos y "Sin conexión" si se agotan, sin errores no controlados en la consola y sin bloquear el resto del detalle.

## Definition of Done

- [ ] DoD-01 — CHU-01 a CHU-08 validados con evidencia registrada en la matriz.
- [ ] DoD-02 — Pruebas unitarias escritas primero y en verde con `cd backend && dotnet test`: `JoinTicketChatTests` (vigente, PM, retirado, no participante, admin no asociado, cliente) y `ChatConnectionTrackerTests`.
- [ ] DoD-03 — Pruebas de integración del hub en verde con `WebApplicationFactory<Program>`, cliente SignalR .NET y PostgreSQL real: `Hub_SinSesion_Rechaza401`, `Hub_Cliente_Rechaza403`, `JoinTicket_Participante_EntraAlGrupo`, `JoinTicket_NoParticipante_NoRecibeEventosDelGrupo` (dos clientes conectados), `JoinTicket_Retirado_Rechazado`, `JoinTicket_AdminNoAsociado_Rechazado`.
- [ ] DoD-04 — `DataTicket.ArchitectureTests` en verde: el hub y el registro viven en `Api`, y `Application` no referencia `Microsoft.AspNetCore.SignalR`.
- [ ] DoD-05 — Frontend: `npm --prefix frontend run lint` sin errores (ninguna vista importa `@microsoft/signalr` ni `core/`), `npm --prefix frontend test` en verde (`parseHubError`, reductor de `ConnectionStatus`) y `npm --prefix frontend run build` correcto.
- [ ] DoD-06 — `docker compose up --build` levanta el stack. Verificación manual con dos navegadores en `http://localhost:5173`: participante unido (en DevTools, el WebSocket `/hubs/tickets` con estado 101) y no participante rechazado. Repetir a través del nginx de la etapa `runtime` si se usa en Compose.
- [ ] DoD-07 — Contrato del hub ratificado y documentado en [[tiempo-real-signalr]]; nombres nuevos en [[glosario]].
- [ ] DoD-08 — Wiki actualizada mediante Notas para la wiki: [[tiempo-real-signalr]] (estado "implementado", contrato), [[frontend-mvc]] (`core/realtime`), [[stack-y-versiones]] (`@microsoft/signalr` y `Microsoft.AspNetCore.SignalR.Client`), [[chat-interno]].
- [ ] DoD-09 — Revisión de `quality-reviewer` sin hallazgos CRÍTICO o ALTO abiertos (autenticación del hub, autorización por unión, fuga de eventos).
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

- **Una conexión por pestaña, varios tickets por conexión** (propuesta): por eso la revocación de [[hu-032-revocar-acceso-al-retirar-participante|HU-032]] saca la conexión del grupo del ticket en lugar de cerrarla.
- **Secuestro de WebSocket entre sitios (CSWSH):** el hub se autentica con cookie. La cookie `SameSite=Lax` de [[adr-0004-autenticacion-cookie-mismo-origen]] no viaja en peticiones de subrecursos entre sitios. Se propone, además, validar el encabezado `Origin` en la negociación como defensa en profundidad (propuesta para `quality-reviewer`).
- **Sesión caducada durante la conexión:** el WebSocket sigue abierto, pero la próxima negociación o reconexión responderá 401. Cómo tratarlo en la UI se cubre en [[hu-031-recuperar-mensajes-tras-reconexion|HU-031]].
- El registro de conexiones es en memoria y vale para una sola instancia. Al escalar, se revisa junto con el backplane ([[pendientes]] §4).
- No se audita la unión al chat (PRD §12 no lo exige). Los intentos denegados dependen de V-11.

## Relacionado

- [[ep-009-chat-interno-en-tiempo-real]] · [[tablero-scrum]] · [[criterios-de-aceptacion]]
- [[tiempo-real-signalr]] · [[adr-0005-signalr-para-chat]] · [[autenticacion-identity]] · [[adr-0004-autenticacion-cookie-mismo-origen]] · [[chat-interno]] · [[roles-y-permisos]] · [[frontend-mvc]] · [[entorno-docker]] · [[estrategia-de-pruebas]]
