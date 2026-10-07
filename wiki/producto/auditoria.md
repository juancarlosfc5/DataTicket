---
title: Auditoría
type: concepto
status: vigente
tags: [producto, producto/auditoria, producto/seguridad]
sources: ["PRD.md §12", "PRD.md §6.1", "PRD.md §6.2", "PRD.md §6.4", "PRD.md §6.5", "PRD.md §7", "PRD.md §11", "PRD.md §14"]
aliases: [Bitácora de auditoría, Bitácora, AuditEntry]
created: 2026-10-07
updated: 2026-10-07
---

# Auditoría

Bitácora **append-only** que registra quién hizo qué, cuándo y sobre qué objeto en la vida de un ticket. Es la fuente de trazabilidad del producto, por encima del correo (PRD §11, §12).

## Principios

- Append-only desde la aplicación: no se editan ni borran entradas (PRD §12).
- La participación histórica de una persona retirada permanece registrada (PRD §5, §12).
- La auditoría conserva los mensajes y la participación anterior de una persona retirada del chat (PRD §7).
- Retención exacta pendiente de decisión de infraestructura (PRD §12, §16).

## Campos mínimos (`AuditEntry`)

| Campo | Contenido (PRD §12) |
|---|---|
| Actor | Persona que ejecutó la acción |
| Fecha/hora | Momento de la acción |
| Acción | Tipo de evento (ver tabla siguiente) |
| Objeto | Entidad afectada (ticket, participante, respuesta…) |
| Valor anterior / nuevo | Cuando aplique (p. ej. estado, prioridad) |
| Resultado | Resultado de la acción |

## Eventos auditados

| Evento | Datos específicos | Fuente |
|---|---|---|
| Radicación del ticket | — | PRD §12 |
| Cambio de prioridad (ajuste manual) | Prioridad calculada, nueva prioridad, quién, cuándo, **motivo** | PRD §6.1, §12 |
| Cambio de estado | Estado anterior y nuevo | PRD §6.4, §12 |
| Asignación | Persona(s) asignada(s) | PRD §6.2, §12 |
| Reasignación | Asignación anterior y nueva | PRD §6.2, §12 |
| Incorporación de participante | Persona, quién la agregó | PRD §6.2, §12 |
| Retiro de participante | Persona, quién la retiró | PRD §6.2, §12 |
| Respuesta formal | Emisor y fecha | PRD §12 |
| Cierre manual | Quién cerró, cuándo, **estado anterior** | PRD §6.5, §12 |

- "Los movimientos internos, reasignaciones y cambios de participantes" se registran en la bitácora (PRD §6.4).
- Cuando un administrador toma un ticket en cobertura queda asociado como participante; por tanto es una incorporación auditable (PRD §5, §6.2).

## Lo que no se registra

- Detalles de la llamada de confirmación: la conversación telefónica se representa **solo** por el acto de cierre manual (PRD §6.5).

## Criterios de aceptación ligados

- **CA-04**: asignar, reasignar y agregar/quitar participantes queda auditado (PRD §14.4).
- **CA-14**: estado, participantes, prioridad, asignación y cierre conservan actor y fecha/hora (PRD §14.14).

> [!question] Pendiente
> - El campo "resultado" no está definido: ¿se auditan también los intentos denegados (p. ej. un no participante intentando unirse al chat) o solo las acciones exitosas?
> - Retención y respaldo de la auditoría quedan pendientes (PRD §12, §16).
> Ver [[pendientes]].

## Relacionado

- [[flujo-del-ticket]] · [[estados-del-ticket]] · [[matriz-de-prioridad]] · [[roles-y-permisos]]
- [[chat-interno]] · [[notificaciones]] · [[persistencia-postgresql]]
- [[modelo-de-dominio]] · [[criterios-de-aceptacion]] · [[glosario]]
