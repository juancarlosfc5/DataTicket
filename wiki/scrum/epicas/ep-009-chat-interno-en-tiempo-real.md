---
title: "EP-009 — Chat interno en tiempo real"
type: epica
status: vigente
estado: Aprobada
tags: [scrum, scrum/epica, producto/chat, signalr]
sources: ["PRD.md §5", "PRD.md §7", "PRD.md §8", "PRD.md §11", "PRD.md §12", "PRD.md §13", "PRD.md §14"]
aliases: ["EP-009", "Chat interno en tiempo real"]
fase_prd: "2"
criterios_prd: ["CA-05", "CA-06", "CA-07", "CA-08", "CA-09", "CA-10"]
historias:
  - "[[hu-026-historial-del-chat-por-cursor]]"
  - "[[hu-027-unirse-al-chat-del-ticket]]"
  - "[[hu-028-enviar-y-recibir-mensajes]]"
  - "[[hu-029-adjuntos-e-imagenes-en-chat]]"
  - "[[hu-030-confirmaciones-de-lectura]]"
  - "[[hu-031-recuperar-mensajes-tras-reconexion]]"
  - "[[hu-032-revocar-acceso-al-retirar-participante]]"
dependencias:
  - "[[ep-001-fundaciones-tecnicas]]"
  - "[[ep-002-identidad-y-acceso]]"
  - "[[ep-005-radicacion-de-tickets]]"
  - "[[ep-006-triage-asignacion-y-participantes]]"
  - "[[ep-007-trabajo-interno-y-estados]]"
created: 2026-10-07
updated: 2026-10-09
---

# EP-009 — Chat interno en tiempo real

Chat privado, persistente y en tiempo real por ticket para las personas internas asociadas, con historial paginado por cursor, adjuntos, confirmaciones de lectura, recuperación tras reconexión y revocación inmediata al retirar a un participante (PRD §7). Se construye con ASP.NET Core SignalR y PostgreSQL como historial ([[adr-0005-signalr-para-chat]]).

## Objetivo

Que Desarrollo, Producción, Elizabeth (PM) y los administradores asociados colaboren sobre cada ticket en un canal interno que:

- entrega los mensajes en tiempo real a quienes están conectados (PRD §7.1);
- guarda cada mensaje en PostgreSQL antes de publicarlo (PRD §7.2);
- da el historial completo a quien se incorpora tarde y se lo niega a quien fue retirado (PRD §5.4, §7.3, §7.5);
- autoriza en el servidor cada unión y cada operación; los grupos de SignalR no son seguridad (PRD §7).

## Valor esperado

- Sustituye conversaciones dispersas por un historial único y auditable por ticket, que es la base para preparar la respuesta formal (PRD §6.5, §7).
- Cumple los criterios del MVP sobre tiempo real, privacidad del chat y lecturas (PRD §14.5–§14.9) sin exponer nada al cliente (PRD §5.6, §14.10).

## Fase del PRD

Fase 2 — Colaboración y entrega, prioridad P0 (PRD §13).

## Actores

- Participante interno vigente de Desarrollo o Producción (`TicketParticipant` activo; `Team.Development` / `Team.Production`).
- Elizabeth, PM (rol `ProductManager`): accede siempre al chat, aunque no sea participante (PRD §5.3).
- Administrador (`Administrator`): solo si está asociado al ticket; ser administrador no da acceso (PRD §5.5).
- Persona retirada del ticket: pierde todo acceso desde su retiro (PRD §7.5).
- Cliente (`Requester`, `CompanyCoordinator`): nunca accede al chat (PRD §5.6, §7).

## Alcance

- Entidad `ChatMessage` y su persistencia; API REST de historial paginado por cursor (`GetChatHistory`).
- Hub autenticado `/hubs/tickets` (`TicketsHub`) con `JoinTicket`, `LeaveTicket`, `SendMessage`, `MarkAsRead` y eventos `MessageCreated`, `ReadReceiptUpdated` (nombres de [[tiempo-real-signalr]], a ratificar en el contrato de cada HU).
- Envío y recepción de mensajes con la regla *persistir antes de publicar* (`SendChatMessage`).
- Adjuntos del chat (PDF, imágenes, XML y Excel de hasta 10 MB) con vista previa de imágenes, marcados como internos.
- Confirmaciones de lectura persistidas por mensaje y persona (`MarkMessageRead`, `MessageReadReceipt`).
- Recuperación de mensajes perdidos tras reconexión o refresco, sin duplicados, con indicador de estado de conexión.
- Revocación de conexiones activas y futuras al retirar a un participante (`IChatConnectionRevoker`).
- Cliente `@microsoft/signalr` en `frontend/src/core/realtime` y módulo de chat MVC en el frontend.

## Fuera de alcance

- Chat visible para el cliente o respuestas públicas tipo conversación (PRD §13).
- Correo por cada mensaje del chat (PRD §7.6, §11).
- Notificaciones dentro de la aplicación por mensaje nuevo y correo de vinculación: se entregan en [[ep-011-notificaciones]] ([[hu-037-notificaciones-en-la-app|HU-037]], [[hu-038-correo-de-vinculacion|HU-038]]).
- Redis o backplane multiinstancia: el MVP corre en una sola instancia (PRD §7, §13).
- Edición o borrado de mensajes: decisión abierta V-14 ([[pendientes]]); el mensaje se trata como inmutable (propuesta del [[modelo-de-dominio]]).
- MFA/SSO, integración con GitHub/GitLab, SLA y recordatorios (PRD §13).
- Búsqueda en el chat, menciones, reacciones e indicador de "escribiendo" (no aparecen en el PRD).

## Requisitos y reglas de negocio

- Solo participantes vigentes leen o escriben el chat; Elizabeth siempre puede (PRD §5.3, §7).
- Agregar a alguien le da acceso al historial completo; retirarlo revoca su acceso futuro y conserva su participación pasada en el historial y la auditoría (PRD §5.4, §7.3, §7.5).
- Un administrador no obtiene acceso a mensajes y archivos por serlo (PRD §5.5).
- El servidor guarda cada mensaje en PostgreSQL antes de confirmar su publicación; SignalR no es el historial (PRD §7.2).
- Las lecturas se persisten por mensaje y persona, con fecha y hora, y se publican a los participantes autorizados (PRD §7.4, §7 flujo 5).
- Tras reconectar, el cliente se vuelve a autenticar, se vuelve a unir y recupera desde PostgreSQL los mensajes posteriores a su cursor (PRD §7 flujo 6).
- Si se retira a una persona conectada, sus conexiones se desconectan o quedan sin autorización antes de permitir nuevas operaciones (PRD §7).
- El chat acepta texto y adjuntos de los tipos y tamaños de PRD §8, validados en el backend (PRD §7.8, §8).
- Validación de pertenencia al ticket en todas las rutas de chat e historial (PRD §12).
- El cliente nunca ve conversaciones internas ni adjuntos internos (PRD §5.6, §10, §14.10).

## Criterios del PRD cubiertos

- PRD CA-05: tiempo real y recuperación desde PostgreSQL al refrescar o reconectar. Lo cubren HU-028 y HU-031, con apoyo de HU-026 y HU-027 → [[criterios-de-aceptacion]]
- PRD CA-06: un no participante no se une, no carga historial, no envía ni marca lecturas; la revocación aplica a conexiones existentes y futuras. Lo cubren HU-026, HU-027, HU-028, HU-030 y HU-032 → [[criterios-de-aceptacion]]
- PRD CA-07: lecturas con persona, mensaje y fecha/hora, persistentes tras cerrar sesión. Lo cubre HU-030 → [[criterios-de-aceptacion]]
- PRD CA-08: historial completo para quien se agrega tarde; el retirado no vuelve a consultarlo. Lo cubren HU-026, HU-029 y HU-032 → [[criterios-de-aceptacion]]
- PRD CA-09 (parcial, parte del chat): texto e imágenes; PDF, imágenes, XML y Excel de hasta 10 MB rechazados en el backend si no cumplen. Lo cubren HU-028 y HU-029; radicación y respuesta formal van en HU-014 y HU-033 → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial): los adjuntos y mensajes del chat no llegan al portal. Lo cubre HU-029, junto con HU-024 y HU-025 → [[criterios-de-aceptacion]]

## Dependencias

- [[ep-001-fundaciones-tecnicas]]: CI y proyecto de pruebas de integración con `WebApplicationFactory<Program>` y PostgreSQL real.
- [[ep-002-identidad-y-acceso]]: cookie de Identity same-origin ([[adr-0004-autenticacion-cookie-mismo-origen]], todavía por aceptar), `ICurrentUser` y roles ([[hu-003-iniciar-y-cerrar-sesion|HU-003]], [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]]).
- [[ep-005-radicacion-de-tickets]]: `Attachment`, `IFileStorage` y la validación de adjuntos ([[hu-014-adjuntos-en-radicacion|HU-014]]).
- [[ep-006-triage-asignacion-y-participantes]]: `TicketParticipant` vigente o retirado ([[hu-018-asignar-y-agregar-participantes|HU-018]], [[hu-019-reasignar-y-retirar-participantes|HU-019]]) y la cobertura del administrador ([[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]]).
- [[ep-007-trabajo-interno-y-estados]]: detalle interno del ticket donde se monta el chat ([[hu-020-detalle-interno-del-ticket|HU-020]]).
- Relacionada: [[ep-011-notificaciones]] consume `ChatMessage` para las notificaciones dentro de la aplicación.

## Historias de usuario

- [[hu-026-historial-del-chat-por-cursor|HU-026 — Historial del chat paginado por cursor]] · Sprint 5
- [[hu-027-unirse-al-chat-del-ticket|HU-027 — Conectarse y unirse al chat del ticket]] · Sprint 5
- [[hu-028-enviar-y-recibir-mensajes|HU-028 — Enviar y recibir mensajes en tiempo real]] · Sprint 5
- [[hu-032-revocar-acceso-al-retirar-participante|HU-032 — Revocar acceso al retirar un participante]] · Sprint 5
- [[hu-029-adjuntos-e-imagenes-en-chat|HU-029 — Adjuntos e imágenes en el chat]] · Sprint 6
- [[hu-030-confirmaciones-de-lectura|HU-030 — Confirmaciones de lectura]] · Sprint 6
- [[hu-031-recuperar-mensajes-tras-reconexion|HU-031 — Recuperar mensajes tras reconexión]] · Sprint 6

Orden técnico: HU-026 (entidad e historial) → HU-027 (hub y unión) → HU-028 (envío) → HU-032 (revocación). Después HU-029, HU-030 y HU-031 pueden avanzar en paralelo una vez fijado cada contrato (T-01).

## Criterio de completitud

- [ ] Todas las HU obligatorias (HU-026 a HU-032) están `Completada`.
- [ ] Los criterios del PRD listados (CA-05, CA-06, CA-07, CA-08 y la parte del chat de CA-09 y CA-10) tienen evidencia: prueba automatizada o PR enlazado.
- [ ] Existe una prueba de integración del hub con al menos dos clientes SignalR .NET autenticados sobre PostgreSQL real que demuestra el tiempo real, el rechazo a no participantes y la revocación al retirar.
- [ ] Hay una prueba unitaria que demuestra que `ITicketChatNotifier` no se invoca si falla la persistencia del mensaje.
- [ ] El contrato del hub (métodos, eventos, payloads JSON y códigos de error) y la API de historial están documentados en [[tiempo-real-signalr]].
- [ ] `ChatMessage`, `MessageReadReceipt`, el contexto de `Attachment` y los nombres nuevos están en [[glosario]] y [[modelo-de-dominio]].
- [ ] El stack levanta con `docker compose up --build` y el chat funciona con dos navegadores a través del proxy same-origin.
- [ ] No quedan dependencias bloqueantes dentro del alcance.

## Riesgos e incógnitas

- **Formato del cursor** sin definir (PRD §7 deja el esquema para el diseño): se propone en el T-01 de [[hu-026-historial-del-chat-por-cursor|HU-026]].
- **Contradicción de tipos de adjunto en el chat**: "texto e imágenes" (PRD §7, objetivo; §14.9) frente a PDF, XML y Excel (PRD §7.8, §8, §14.9). Se asume lo más amplio ([[pendientes]] §1).
- **`Attachment` sin contexto ni marca interno/externo** en el [[modelo-de-dominio]]: sin ella, un adjunto del chat podría filtrarse al portal. Se propone en [[hu-029-adjuntos-e-imagenes-en-chat|HU-029]].
- **URL pública permanente** de los adjuntos ([[adr-0006-urls-publicas-azure-blob]]): un adjunto del chat es accesible fuera del portal por quien tenga el enlace, y un participante retirado conserva los enlaces que ya vio. Riesgo aceptado, pendiente de aprobación por la dirección (PRD §16.6).
- **Ventana de carrera en la revocación**: entre el retiro y la salida de las conexiones del grupo podría llegar un evento. Mitigación en [[hu-032-revocar-acceso-al-retirar-participante|HU-032]].
- **Edición o borrado de mensajes** (V-14) sin decidir.
- **Máximo de adjuntos por mensaje** sin decidir ([[pendientes]] §4).
- **Backplane** solo al escalar (PRD §7): el registro de conexiones en memoria vale para una instancia.
- **¿Se auditan los intentos denegados?** (V-11): afecta al registro de rechazos del hub.
- **ADR-0004** (cookie same-origin) aún no está aceptado; el hub depende de esa sesión.

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[criterios-de-aceptacion]]
- [[chat-interno]] · [[tiempo-real-signalr]] · [[adr-0005-signalr-para-chat]] · [[archivos-adjuntos]] · [[adr-0006-urls-publicas-azure-blob]]
- [[roles-y-permisos]] · [[notificaciones]] · [[modelo-de-dominio]] · [[persistencia-postgresql]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[pendientes]]
