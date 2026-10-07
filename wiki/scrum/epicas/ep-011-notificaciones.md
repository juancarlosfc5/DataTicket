---
title: "EP-011 — Notificaciones acordadas"
type: epica
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/epica, producto/notificaciones, producto/chat]
sources: ["PRD.md §5", "PRD.md §7", "PRD.md §11", "PRD.md §12", "PRD.md §13", "PRD.md §16"]
aliases: ["EP-011", "Notificaciones acordadas"]
fase_prd: "2"
criterios_prd: []
historias: ["[[hu-037-notificaciones-en-la-app]]", "[[hu-038-correo-de-vinculacion]]"]
dependencias: ["[[ep-002-identidad-y-acceso]]", "[[ep-006-triage-asignacion-y-participantes]]", "[[ep-009-chat-interno-en-tiempo-real]]"]
created: 2026-10-07
updated: 2026-10-07
---

# EP-011 — Notificaciones acordadas

Implementa solo las notificaciones que el PRD acuerda para el trabajo interno: aviso dentro de la aplicación por mensajes nuevos del chat (sin correo por mensaje) y correo de vinculación cuando una persona queda asociada por primera vez a un ticket (PRD §7.6, §7.7, §11).

## Objetivo

Que los participantes internos se enteren de la actividad de sus tickets sin depender del correo, y que quien es vinculado a un ticket lo sepa, sin que ningún aviso sustituya el control de acceso del portal (PRD §7.7, §11).

## Valor esperado

- Menos contexto perdido: los participantes ven los mensajes pendientes aunque no tengan abierto el ticket.
- Bajo ruido: no hay correo por mensaje ni por cambio de estado (PRD §11).
- Mínima exposición: los avisos no transportan contenido sensible y no llegan a personas retiradas (PRD §11).

## Fase del PRD

Fase 2 — Colaboración y entrega, "notificaciones acordadas" (PRD §13).

## Actores

- Participantes internos vigentes (`TicketParticipant`): Desarrollo, Producción, administrador asociado.
- Elizabeth — PM (rol `ProductManager`): asigna y recibe avisos según la propuesta de HU-037.
- Persona recién asociada a un ticket (destinataria del correo de vinculación).

## Alcance

- Notificación in-app persistida por cada mensaje nuevo para los participantes vigentes, salvo el autor, con contador de no leídas y lista → [[hu-037-notificaciones-en-la-app|HU-037]].
- Entrega en tiempo real del aviso a la persona destinataria por el hub `/hubs/tickets` (evento propuesto `NotificationCreated`).
- Correo de vinculación en la primera asociación, con número, título y enlace construido con `App:PublicBaseUrl` → [[hu-038-correo-de-vinculacion|HU-038]].

## Fuera de alcance

- Correo por cada mensaje del chat (PRD §7.6, §11).
- Correo al cliente por cada transición de estado resumido (PRD §11: no garantizado en el MVP).
- Notificar estados técnicos al cliente (PRD §11).
- Recordatorios, escalamientos y automatizaciones de SLA (PRD §13).
- Notificaciones push del navegador o del sistema operativo, y app móvil (PRD §13).
- Preferencias de notificación por usuario (no aparecen en el PRD).
- El correo de la respuesta formal: pertenece a [[ep-010-respuesta-formal-y-cierre]] ([[hu-035-correo-de-respuesta-formal]]).

## Requisitos y reglas de negocio

- Los mensajes nuevos generan notificación dentro de la aplicación; no se envía correo por mensaje (PRD §7.6, §11).
- La primera asociación de una persona a un ticket dispara un correo de vinculación; ese correo no sustituye el control de acceso (PRD §7.7, §11).
- No se notifica a la persona retirada por mensajes futuros (PRD §11).
- La auditoría, no el correo, es la fuente de trazabilidad (PRD §11, §12).
- Solo participantes vigentes leen el chat; Elizabeth siempre puede (PRD §5.3).

## Criterios del PRD cubiertos

- Ningún criterio del PRD §14 trata las notificaciones de forma directa. La épica se verifica contra PRD §7.6, §7.7 y §11 → [[criterios-de-aceptacion]]
- PRD CA-06 (complementario) — Retirar a una persona revoca su acceso; esta épica añade que tampoco recibe avisos nuevos ni ve los anteriores de ese ticket (HU-037) → [[criterios-de-aceptacion]]

## Dependencias

- [[ep-009-chat-interno-en-tiempo-real]]: envío de mensajes ([[hu-028-enviar-y-recibir-mensajes]]), confirmaciones de lectura ([[hu-030-confirmaciones-de-lectura]]) y revocación ([[hu-032-revocar-acceso-al-retirar-participante]]).
- [[ep-006-triage-asignacion-y-participantes]]: asociación de participantes ([[hu-017-cola-de-cobertura-y-tomar-ticket]], [[hu-018-asignar-y-agregar-participantes]], [[hu-019-reasignar-y-retirar-participantes]]).
- [[ep-002-identidad-y-acceso]]: adaptador SMTP de `IEmailSender` y `App:PublicBaseUrl` ([[hu-005-invitar-y-activar-cuentas]]).
- [[ep-001-fundaciones-tecnicas]]: shell con espacio para el indicador de notificaciones ([[hu-002-shell-y-navegacion-por-rol]]).
- Decisiones abiertas: V-08 (reenvío del correo y caso del administrador en cobertura), proveedor de correo productivo (PRD §16.2) → [[pendientes]].

## Historias de usuario

- [[hu-037-notificaciones-en-la-app|HU-037 — Notificaciones en la app por mensajes nuevos]]
- [[hu-038-correo-de-vinculacion|HU-038 — Correo de vinculación en la primera asociación]]

## Criterio de completitud

- [ ] HU-037 y HU-038 están `Completada`, con su matriz de evidencia registrada.
- [ ] Hay una prueba que demuestra que enviar un mensaje de chat no invoca `IEmailSender`.
- [ ] Hay una prueba que demuestra que una persona retirada no recibe avisos nuevos del ticket.
- [ ] Hay evidencia en Mailpit (`http://localhost:8025`) del correo de vinculación con enlace basado en `App:PublicBaseUrl`.
- [ ] V-08 está resuelta o el comportamiento provisional está aceptado por la persona usuaria.
- [ ] No quedan dependencias bloqueantes dentro del alcance.

## Riesgos e incógnitas

- **V-08:** no se sabe si se reenvía el correo a quien es retirado y vuelto a agregar, ni si el administrador que toma un ticket en cobertura lo recibe.
- **Elizabeth y el volumen de avisos:** accede a todos los chats; notificarla por todos los mensajes podría saturarla. La HU-037 propone notificarla solo cuando es participante explícita.
- **Entrega por usuario en SignalR:** el aviso va a la persona, no al grupo del ticket; exige resolver la identidad del usuario en el hub (`Clients.User`) y probarlo con varias pestañas abiertas.
- **Proveedor de correo productivo** pendiente (PRD §16.2): la entregabilidad del correo de vinculación no se puede garantizar fuera de Mailpit.
- **`App:PublicBaseUrl` no está configurado** hoy en `docker-compose.yml`; sin él los enlaces quedarían mal construidos o expuestos a envenenamiento por `Host` ([[autenticacion-identity]]).

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[notificaciones]] · [[chat-interno]] · [[tiempo-real-signalr]] · [[autenticacion-identity]] · [[entorno-docker]] · [[criterios-de-aceptacion]] · [[ep-009-chat-interno-en-tiempo-real]] · [[ep-010-respuesta-formal-y-cierre]]
