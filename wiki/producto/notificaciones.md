---
title: Notificaciones
type: concepto
status: vigente
tags: [producto, producto/notificaciones]
sources: ["PRD.md §11", "PRD.md §7", "PRD.md §6.5", "PRD.md §16"]
aliases: [Correos, Avisos, Notificaciones in-app]
created: 2026-10-07
updated: 2026-10-07
---

# Notificaciones

Qué eventos avisan a quién y por qué canal. El principio es notificar poco: in-app para el chat, correo solo para vinculación y respuesta formal. La trazabilidad vive en la auditoría, no en el correo (PRD §11).

## Tabla de eventos (PRD §11)

| Evento | Canal | Destinatario | Nota |
|---|---|---|---|
| Nuevo mensaje en el chat interno | In-app | Participantes del ticket | **Sin correo por mensaje** (PRD §7, §11) |
| Primera asociación de una persona a un ticket | Correo de vinculación | Esa persona | No sustituye el control de acceso del portal (PRD §7) |
| Respuesta formal final | Portal + correo | Contacto de cliente correspondiente | Texto completo y archivos asociados (PRD §6.5, §11) |
| Cambio de estado interno | Bandejas internas | Equipo interno | **No** se revela el estado técnico por correo al cliente |
| Cambio de estado resumido | Portal | Cliente | El correo automático por transición **no está confirmado** y queda fuera de la garantía del MVP |

## Reglas

- **Personas retiradas:** la política debe evitar notificar al usuario retirado de un ticket por mensajes futuros (PRD §11). Ver [[chat-interno]].
- **Fuente de trazabilidad:** el registro de auditoría, no el correo (PRD §11). Ver [[auditoria]].
- **Opacidad al cliente:** ninguna notificación al cliente revela estados técnicos, participantes internos ni URL de PR (PRD §5, §11). Ver [[estados-del-ticket]].
- **Sin recordatorios:** no hay recordatorios de cierre ni de vencimiento (PRD §6.5, §13).

## Decisiones abiertas

- Proveedor de correo, dominio remitente y configuración de entregabilidad se deciden antes de producción (PRD §16).
- Entorno local de correo: ver [[entorno-docker]].

> [!question] Pendiente
> - "Contacto de cliente correspondiente" no está definido: ¿el solicitante, el coordinador de la empresa o ambos?
> - "Primera asociación": no se aclara si una persona retirada y luego reincorporada recibe de nuevo el correo de vinculación, ni si aplica cuando un administrador se asocia al tomar un ticket en cobertura.
> Ver [[pendientes]].

## Relacionado

- [[chat-interno]] · [[estados-del-ticket]] · [[flujo-del-ticket]] · [[auditoria]]
- [[roles-y-permisos]] · [[tiempo-real-signalr]] · [[entorno-docker]] · [[glosario]]
