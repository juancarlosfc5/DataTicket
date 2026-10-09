---
title: "HU-030 — Confirmaciones de lectura"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/chat, signalr, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §7", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-030", "Confirmaciones de lectura"]
epica: "[[ep-009-chat-interno-en-tiempo-real]]"
criterios_prd: ["CA-06", "CA-07"]
componentes: ["Hub SignalR", "Backend (Domain, Application, Infrastructure, Api)", "Persistencia PostgreSQL", "Frontend (models, controllers, views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 6"
dependencias: ["[[hu-026-historial-del-chat-por-cursor]]", "[[hu-027-unirse-al-chat-del-ticket]]", "[[hu-028-enviar-y-recibir-mensajes]]"]
relacionadas: ["[[hu-031-recuperar-mensajes-tras-reconexion]]", "[[hu-032-revocar-acceso-al-retirar-participante]]", "[[hu-037-notificaciones-en-la-app]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-030 — Confirmaciones de lectura

Cuando un participante autorizado ve un mensaje, el servidor guarda una `MessageReadReceipt` (persona, mensaje y fecha/hora), sin duplicados, y publica `ReadReceiptUpdated` a los participantes autorizados. Cada participante ve quién leyó cada mensaje y cuándo, también después de cerrar sesión y volver a entrar (PRD §7.4, §14.7).

## Historia de usuario

**COMO** participante interno vigente de un ticket (o Elizabeth, PM)  
**QUIERO** ver quién leyó cada mensaje del chat y cuándo  
**PARA** saber si el equipo ya conoce la información que compartí sin tener que preguntarlo

## Contexto

El PRD exige que cada participante vea quién leyó cada mensaje y en qué fecha/hora, con lecturas persistidas por mensaje y persona (PRD §7.4). Al marcar como leído, el servidor persiste persona, mensaje y hora y publica la actualización a los participantes autorizados (PRD §7, flujo 5). Una persona retirada deja de marcar lectura (PRD §7.5). CA-07 exige que la confirmación identifique persona, mensaje y fecha/hora y siga disponible tras cerrar sesión (PRD §14.7). El [[modelo-de-dominio]] propone un `MessageReadReceipt` por (mensaje, persona), creado solo por un participante vigente. El caso de uso es `MarkMessageRead` ([[backend-hexagonal]]).

## Alcance

- Entidad `MessageReadReceipt` (`ChatMessageId`, `UserId`, `ReadAt`) con unicidad por (mensaje, persona) y su migración EF Core.
- Caso de uso `MarkMessageRead`, idempotente, con revalidación del acceso en cada llamada.
- Método del hub `MarkAsRead(ticketId, messageId)` y evento `ReadReceiptUpdated` al grupo del ticket.
- `readReceipts` poblado en el `ChatMessageDto` del historial ([[hu-026-historial-del-chat-por-cursor|HU-026]]).
- Frontend: marcar como leído cuando el mensaje es visible y la pestaña está activa; indicador "Visto por…" con fecha y hora.

## Fuera de alcance

- Contadores de no leídos en bandejas y notificaciones dentro de la aplicación ([[hu-037-notificaciones-en-la-app|HU-037]]).
- Marcar como "no leído".
- Confirmaciones de entrega por dispositivo o presencia en línea.
- Mostrar lecturas al cliente (nunca: el chat es interno).

## Requisitos y reglas de negocio

- Cada participante ve quién leyó cada mensaje y cuándo; las lecturas se persisten por mensaje y persona (PRD §7.4).
- Al marcar, el servidor persiste persona, mensaje y hora y luego publica a los participantes autorizados (PRD §7, flujo 5).
- Un no participante no puede marcar lecturas (PRD §14.6); una persona retirada tampoco (PRD §7.5).
- Solo participantes vigentes; Elizabeth siempre (PRD §5.3); el administrador solo si está asociado (PRD §5.5).
- La lectura sigue disponible tras cerrar sesión y volver a entrar (PRD §14.7).

## Invariantes en juego

- AGENTS §7.3: solo participantes vigentes marcan lecturas; se valida en cada operación.
- AGENTS §7.4 (por analogía, PRD §7 flujo 5): la lectura se persiste antes de publicarse.
- AGENTS §7.2: el cliente nunca ve lecturas ni mensajes.

## Criterios del PRD cubiertos

- PRD CA-07 (total) → [[criterios-de-aceptacion]]
- PRD CA-06 (parcial: "un no participante no puede marcar lecturas"; el retirado tampoco) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-009-chat-interno-en-tiempo-real]]
- Dependencias: [[hu-026-historial-del-chat-por-cursor|HU-026 — Historial del chat paginado por cursor]] (DTO con `readReceipts`, regla de acceso), [[hu-027-unirse-al-chat-del-ticket|HU-027 — Conectarse y unirse al chat del ticket]] (hub y grupo), [[hu-028-enviar-y-recibir-mensajes|HU-028 — Enviar y recibir mensajes en tiempo real]] (mensajes y `ITicketChatNotifier`).
- Relacionadas: [[hu-031-recuperar-mensajes-tras-reconexion|HU-031]] (volver a marcar tras reconectar), [[hu-032-revocar-acceso-al-retirar-participante|HU-032]], [[hu-037-notificaciones-en-la-app|HU-037]].
- Decisiones: [[adr-0005-signalr-para-chat]] (aceptada).

## Componentes afectados

- Backend Domain: `MessageReadReceipt`; regla de unicidad y autor que no se marca a sí mismo (propuesta).
- Backend Application: `Ports/In` → `MarkMessageRead`; `Ports/Out` → `IChatRepository` (lecturas), `ITicketChatNotifier.ReadReceiptUpdatedAsync`, `IClock`, `ICurrentUser`.
- Backend Infrastructure: `MessageReadReceiptConfiguration` (índice único `(ChatMessageId, UserId)`), migración `AddMessageReadReceipts` (nombre propuesto).
- Hub SignalR: `TicketsHub.MarkAsRead`.
- Frontend: gateway (`markAsRead`, `onReadReceiptUpdated`), controlador y vista `ReadReceiptsIndicator` (nombre propuesto).

## Dificultad

**Nivel:** Alto

**Justificación:** es transversal (entidad, migración, caso de uso idempotente bajo concurrencia, hub, evento y frontend). Exige autorización en cada marca y una política de cliente que evite marcar mensajes que la persona no vio.

## Contrato backend ↔ frontend

> [!info] Propuesta
> Se ratifica en T-01 y se documenta en [[tiempo-real-signalr]].

**Método del hub:** `MarkAsRead(ticketId: string, messageId: string)` → devuelve un `ReadReceiptDto`, o `null` si el mensaje es del propio usuario (el autor no genera lectura; propuesta).

**Evento al grupo `ticket:{ticketId}`:** `ReadReceiptUpdated(receipt: ReadReceiptDto)`. Se publica solo cuando se **crea** una lectura nueva.

```json
{
  "ticketId": "01928b77-1c2d-7e0f-a3b4-c5d6e7f80912",
  "messageId": "01928c1e-7f3a-7b2e-9c41-5d2f0a8e4b10",
  "userId": "01928a00-0000-7000-8000-000000000005",
  "displayName": "Brayan",
  "readAt": "2026-10-07T15:06:41.004512Z"
}
```

**En el historial** (`ChatMessageDto.readReceipts`): `[{ "userId": "…", "displayName": "Brayan", "readAt": "…" }]`, ordenado por `readAt`.

| Regla | Detalle |
|---|---|
| Idempotencia | Un solo registro por (mensaje, persona). Una llamada repetida devuelve el registro existente (mismo `readAt`) y no publica un evento nuevo. El índice único resuelve la concurrencia |
| `readAt` | Lo fija el servidor con `IClock`, en UTC con precisión de microsegundos; el cliente no lo envía |
| Errores (`HubException`) | `ticket_not_accessible` (no participante, retirado, cliente, administrador no asociado); `validation_failed` con detalle `message_not_found` si el mensaje no existe o es de otro ticket; `internal_error` si falla la persistencia (no se publica) |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar firma, `ReadReceiptDto`, idempotencia sin evento repetido, tratamiento de los mensajes propios, si cuentan las lecturas de Elizabeth sin ser participante (propuesta: sí), y política del cliente para marcar (visible + pestaña activa). Documentar en [[tiempo-real-signalr]] y registrar `ReadReceiptDto` en el [[glosario]].
- [ ] **T-02 — Dominio (prueba primero)** · Capa: Backend Domain · Dificultad: Bajo  
  Descripción: pruebas de `MessageReadReceipt`: requiere mensaje, persona y `ReadAt`; el autor no genera lectura de su propio mensaje (propuesta). Implementar.
- [ ] **T-03 — Caso de uso `MarkMessageRead` (prueba primero)** · Capa: Backend Application · Dificultad: Medio  
  Descripción: pruebas con dobles de `IChatRepository`, `ITicketChatNotifier`, `ICurrentUser` e `IClock`: autorizado → persiste y luego notifica; ya existía → devuelve la existente y no notifica; **falla la persistencia → no notifica**; no autorizado o retirado → ni persiste ni notifica; mensaje de otro ticket → `message_not_found`; conflicto de unicidad concurrente → devuelve la existente.
- [ ] **T-04 — Persistencia y migración** · Capa: Backend Infrastructure · Dificultad: Medio  
  Descripción: `MessageReadReceiptConfiguration` (FK a mensaje y usuario, `timestamptz`, índice único `(ChatMessageId, UserId)`), inserción que trata la violación de unicidad como "ya existía", carga de `readReceipts` en el historial sin N+1, y migración `AddMessageReadReceipts` (nombre propuesto).
- [ ] **T-05 — Hub e integración** · Capa: Backend Api/Realtime · Dificultad: Medio  
  Descripción: pruebas de integración primero (`WebApplicationFactory<Program>`, PostgreSQL real, clientes SignalR .NET de Laura y Brayan unidos, Cristian conectado sin unirse, Kevin retirado). Después, `TicketsHub.MarkAsRead` y `ReadReceiptUpdatedAsync` en el notificador.
- [ ] **T-06 — Modelos y gateway** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: tipo `ReadReceipt`; `applyReadReceipt(mensajes, receipt)` idempotente y `pendingReadIds(mensajes, usuarioActual, visibles)` (excluye propios y ya leídos), probadas con Vitest; `ticketsHubGateway.markAsRead` y `onReadReceiptUpdated`.
- [ ] **T-07 — Controlador** · Capa: Frontend controllers · Dificultad: Medio  
  Descripción: `useTicketChatController` recibe de la vista los ids visibles (callback), marca solo con la pestaña activa (`document.visibilityState`), agrupa llamadas cercanas, no repite marcas ya confirmadas, vuelve a marcar los visibles pendientes tras reconectar (con HU-031) y aplica `ReadReceiptUpdated`.
- [ ] **T-08 — Vistas** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `ReadReceiptsIndicator` (puro): "Visto por Brayan, Elizabeth" con detalle de fecha y hora en `America/Bogota` (tooltip o lista accesible). La lista de mensajes expone un callback `onMessagesVisible(ids)`; la vista no llama al hub.

## Criterios de aceptación

### CHU-01 — Flujo feliz con dos usuarios conectados

**Dado** Laura y Brayan unidos al chat del ticket T y un mensaje de Laura sin leer por Brayan  
**Cuando** el mensaje se hace visible en la pantalla de Brayan con la pestaña activa  
**Entonces** se invoca `MarkAsRead(T, mensaje)`, PostgreSQL guarda (mensaje, Brayan, `readAt`) y Laura recibe `ReadReceiptUpdated` y ve "Visto por Brayan" con fecha y hora, sin recargar.

### CHU-02 — Persiste tras cerrar sesión (CA-07)

**Dado** la lectura de Brayan del CHU-01  
**Cuando** Laura y Brayan cierran sesión, vuelven a entrar y cargan el historial de T  
**Entonces** el mensaje trae en `readReceipts` a Brayan con el mismo `readAt` registrado. La confirmación identifica persona, mensaje y fecha/hora.

### CHU-03 — Idempotencia

**Dado** Brayan, que ya leyó un mensaje  
**Cuando** se invoca `MarkAsRead` dos veces más, también de forma concurrente desde dos pestañas  
**Entonces** existe un solo registro (mensaje, Brayan), el `readAt` no cambia y los demás reciben un único `ReadReceiptUpdated` en total.

### CHU-04 — No participante, retirado, administrador no asociado y cliente rechazados

**Dado** el ticket T con mensajes  
**Cuando** invocan `MarkAsRead(T, mensaje)` Cristian (Desarrollo, no asociado), Kevin (retirado), un `Administrator` no asociado o un usuario cliente  
**Entonces** cada uno recibe `ticket_not_accessible` (o 403 al negociar, el cliente), no se guarda ninguna lectura y no se publica ningún evento.

### CHU-05 — Mensaje de otro ticket

**Dado** Laura, participante de T pero no de U  
**Cuando** invoca `MarkAsRead(T, idDeUnMensajeDeU)`  
**Entonces** recibe `validation_failed` (`message_not_found`) y no se guarda ninguna lectura.

### CHU-06 — Persistir antes de publicar

**Dado** `MarkMessageRead` con un repositorio que falla al guardar  
**Cuando** un participante marca un mensaje  
**Entonces** `ITicketChatNotifier` no se invoca y el cliente recibe `internal_error`.

### CHU-07 — El evento solo llega al grupo autorizado

**Dado** Laura y Brayan unidos a T y Cristian conectado sin unirse a T  
**Cuando** Brayan marca un mensaje  
**Entonces** Laura recibe `ReadReceiptUpdated` y Cristian no.

### CHU-08 — Lectura de Elizabeth y mensajes propios

**Dado** Elizabeth unida a T sin ser participante, y Laura autora de un mensaje  
**Cuando** Elizabeth marca el mensaje y Laura marca su propio mensaje  
**Entonces** la lectura de Elizabeth se guarda y se publica (propuesta), y la de Laura sobre su propio mensaje devuelve `null`, sin guardar ni publicar nada (propuesta).

### CHU-09 — UI: solo marca lo visto y tolera fallos

**Dado** el chat abierto con 30 mensajes no leídos  
**Cuando** la pestaña está en segundo plano, solo 10 mensajes son visibles al volver o la conexión cae mientras se marca  
**Entonces** no se marca nada en segundo plano; al volver se marcan solo los 10 visibles; si la marca falla, el chat sigue usable, sin errores no controlados, y los visibles pendientes se vuelven a marcar tras reconectar.

## Definition of Done

- [ ] DoD-01 — CHU-01 a CHU-09 validados con evidencia registrada en la matriz.
- [ ] DoD-02 — Pruebas unitarias escritas primero y en verde con `cd backend && dotnet test`: `MarkMessageRead_Autorizado_PersisteYLuegoNotifica`, `MarkMessageRead_Repetido_NoDuplicaNiNotifica`, `MarkMessageRead_FallaPersistencia_NoNotifica`, `MarkMessageRead_Retirado_Rechaza`, `MarkMessageRead_MensajeDeOtroTicket_Rechaza`, `MarkMessageRead_MensajePropio_NoGeneraLectura`.
- [ ] DoD-03 — Pruebas de integración en verde con `WebApplicationFactory<Program>`, cliente SignalR .NET y PostgreSQL real: `MarkAsRead_DosParticipantes_EmisorRecibeLectura`, `MarkAsRead_Concurrente_UnSoloRegistro`, `MarkAsRead_NoParticipante_Rechazado`, `MarkAsRead_ConexionNoUnida_NoRecibeEvento`, `Historial_TrasNuevoLogin_ConservaLecturas`.
- [ ] DoD-04 — `DataTicket.ArchitectureTests` en verde.
- [ ] DoD-05 — Migración `AddMessageReadReceipts` (nombre propuesto) creada, aplicada en una base limpia del Compose y revisada (índice único).
- [ ] DoD-06 — Frontend: `npm --prefix frontend run lint` sin errores, `npm --prefix frontend test` en verde (`applyReadReceipt`, `pendingReadIds`, política de visibilidad) y `npm --prefix frontend run build` correcto.
- [ ] DoD-07 — `docker compose up --build` levanta el stack. Verificación manual con dos navegadores en `http://localhost:5173`: "Visto por" aparece en tiempo real, se conserva tras cerrar y abrir sesión, y no se marca nada con la pestaña oculta.
- [ ] DoD-08 — Wiki actualizada mediante Notas para la wiki: [[chat-interno]] (reglas de lectura), [[tiempo-real-signalr]] (`MarkAsRead`, `ReadReceiptUpdated`), [[persistencia-postgresql]] (tabla e índice único), [[modelo-de-dominio]] y [[glosario]] (`ReadReceiptDto`).
- [ ] DoD-09 — Revisión de `quality-reviewer` sin hallazgos CRÍTICO o ALTO abiertos.
- [ ] DoD-10 — PR revisado y aprobado por otra persona del equipo.
- [ ] DoD-11 — Trazabilidad de la HU y de [[ep-009-chat-interno-en-tiempo-real]] actualizada; en las Notas para la wiki se indica si PRD CA-07 puede marcarse en [[criterios-de-aceptacion]].

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

## Notas y decisiones

- **Lectura por mensaje y no "hasta aquí":** se sigue el PRD (persona por mensaje). Si el volumen de llamadas resulta alto, se puede proponer `MarkAsRead(ticketId, messageIds[])` en T-01 sin cambiar el modelo.
- **Lecturas de Elizabeth sin ser participante:** se propone registrarlas, porque Elizabeth lee el chat (PRD §5.3). Hay que confirmarlo, porque su lectura aparecería ante el equipo.
- **Lecturas de personas retiradas:** las anteriores al retiro se conservan y se muestran como parte del historial (PRD §5.4, §7.5). Propuesta.
- Las lecturas no son eventos auditables según PRD §12. No generan `AuditEntry`.

## Relacionado

- [[ep-009-chat-interno-en-tiempo-real]] · [[tablero-scrum]] · [[criterios-de-aceptacion]]
- [[chat-interno]] · [[tiempo-real-signalr]] · [[modelo-de-dominio]] · [[persistencia-postgresql]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[estrategia-de-pruebas]]
