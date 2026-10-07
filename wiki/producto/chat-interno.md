---
title: Chat interno
type: concepto
status: vigente
tags: [producto, producto/chat]
sources: ["PRD.md §7", "PRD.md §5", "PRD.md §8", "PRD.md §11", "PRD.md §12", "PRD.md §14"]
aliases: [Chat del ticket, Chat en tiempo real, Chat privado]
created: 2026-10-07
updated: 2026-10-07
---

# Chat interno

Cada ticket tiene un chat privado y persistente, en tiempo real, para las personas internas asociadas. El cliente no participa ni puede consultarlo (PRD §7). Esta página cubre los requisitos **funcionales**; el diseño técnico está en [[tiempo-real-signalr]].

## Quién participa

| Persona | Acceso al chat |
|---|---|
| Participante vigente (`TicketParticipant` activo) | Lee, escribe, marca lectura (PRD §5, §7) |
| Elizabeth (PM) | Siempre, para triage y seguimiento, aunque no esté asociada (PRD §5) |
| Administrador | Solo si está asociado al ticket; ser admin no da acceso (PRD §5) |
| Integrante de equipo no asociado | Ninguno: pertenecer al equipo no basta (PRD §6.2, §10) |
| Persona retirada | Ninguno desde su retiro (PRD §7) |
| Cliente | Nunca (PRD §5, §7) |

## Requisitos funcionales (PRD §7)

| # | Requisito |
|---|---|
| 1 | Un mensaje de una persona autorizada aparece **en tiempo real** para las demás conectadas al ticket |
| 2 | Cada mensaje se guarda en PostgreSQL **antes** de confirmarse su publicación; el transporte en tiempo real no es el historial |
| 3 | Quien se agrega después obtiene el **historial completo**, con mensajes y adjuntos anteriores |
| 4 | Cada participante ve quién leyó cada mensaje y cuándo; las lecturas se persisten por mensaje y persona |
| 5 | Una persona retirada deja de leer, enviar, marcar lectura o reconectarse; la auditoría conserva sus mensajes y su participación |
| 6 | Los mensajes nuevos generan notificación **dentro de la aplicación**; no se envía un correo por mensaje |
| 7 | La primera asociación a un ticket genera un correo de vinculación, que no sustituye el control de acceso |
| 8 | El chat acepta texto y adjuntos de los tipos y tamaños de [[archivos-adjuntos]] |

## Historial para incorporaciones tardías

- Agregar a alguien le da acceso al historial completo del chat (PRD §5, §7).
- Al refrescar o reconectar, un participante recupera desde PostgreSQL los mensajes que se perdió (PRD §7, §14.5).
- Una persona retirada no puede volver a consultar el historial (PRD §14.8).

## Confirmaciones de lectura (`MessageReadReceipt`)

- Registran **persona, mensaje y fecha/hora** (PRD §7, §14.7).
- Siguen disponibles tras cerrar sesión y volver a entrar (PRD §14.7).
- Se publican a los participantes autorizados al marcarse (PRD §7).

## Revocación

- Retirar a un participante revoca su acceso en conexiones **existentes y futuras**: no puede unirse, cargar historial, enviar ni marcar lecturas (PRD §7, §14.6).
- Si está conectado, sus conexiones activas se desconectan o quedan sin autorización antes de permitir nuevas operaciones (PRD §7).
- No recibe notificaciones de mensajes futuros (PRD §11).
- Su participación pasada y sus mensajes permanecen en historial y auditoría (PRD §5, §7, §12).

## Notificaciones

| Evento | Canal |
|---|---|
| Mensaje nuevo | Notificación in-app a los participantes; sin correo (PRD §7, §11) |
| Primera asociación al ticket | Correo de vinculación a esa persona (PRD §7, §11) |

Ver [[notificaciones]].

## Adjuntos en el chat

- Texto e imágenes (PRD §7, §14.9) y, según §7.8 y §14.9, también PDF, XML y Excel (PRD §8).
- Máximo 10 MB por archivo, validado en backend (PRD §8).
- Los desarrolladores preparan aquí el contexto y los adjuntos de la respuesta formal, pero no la envían (PRD §6.5).

> [!question] Pendiente
> - El objetivo de §7 habla de "texto e imágenes", mientras §7.8 y §14.9 admiten también PDF, XML y Excel en el chat. Se asume el conjunto completo de §8.
> - El PRD no dice si un mensaje puede editarse o eliminarse.
> Ver [[pendientes]].

## Relacionado

- [[tiempo-real-signalr]] · [[adr-0005-signalr-para-chat]] · [[persistencia-postgresql]]
- [[roles-y-permisos]] · [[archivos-adjuntos]] · [[notificaciones]] · [[auditoria]]
- [[flujo-del-ticket]] · [[modelo-de-dominio]] · [[criterios-de-aceptacion]] · [[glosario]]
