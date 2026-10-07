---
title: Julián (Producción)
type: entidad
status: vigente
tags: [personas]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §6.3", "PRD.md §6.4"]
aliases: [Julián, Líder de Producción]
created: 2026-10-07
updated: 2026-10-07
---

# Julián (Producción)

Julián lidera el equipo de Producción: dirige la revisión de pull requests y la puesta en producción, y recibe directamente los tickets que corresponden a Producción (PRD §5, §6.3).

## Responsabilidades

| Responsabilidad | Detalle | Fuente |
|---|---|---|
| Liderar Producción | Equipo `Team.Production`, con Elizabeth y Gerardo | PRD §5 |
| Revisión de PR | Lidera la revisión técnica cuando el ticket requiere paso a Producción | PRD §6.3 |
| Puesta en producción | Lidera despliegue o validación; el desarrollador del cambio puede seguir asociado | PRD §6.3 |
| Ruta directa | Recibe tickets de Producción asignados por Elizabeth sin pasar por Desarrollo | PRD §6.2, §6.3 |

## Estados internos asociados

| Estado | `TicketStatus` |
|---|---|
| Revisión de PR | `PullRequestReview` |
| En producción | `InProduction` |

Para el cliente ambos se muestran como **En atención** (`InProgress`) (PRD §6.4). Ver [[estados-del-ticket]].

## Acceso y límites

- Accede al detalle y al chat solo de los tickets a los que está **asociado**; liderar Producción no le da acceso general (PRD §6.2, §10).
- Ve la URL de PR como participante interno autorizado (PRD §6.3).
- **No** emite la respuesta formal ni cierra: eso es de Elizabeth o un administrador (PRD §5, §6.5).
- La revisión de PR se apoya en una URL manual; no hay integración automática con GitHub/GitLab (PRD §6.3).

> [!question] Pendiente
> El PRD no indica quién registra el paso a `PullRequestReview` o `InProduction`, ni qué estado toma un ticket de ruta directa. Ver [[estados-del-ticket]] y [[pendientes]].

## Relacionado

- [[equipo-data-global]] · [[elizabeth-pm]] · [[flujo-del-ticket]]
- [[estados-del-ticket]] · [[roles-y-permisos]] · [[chat-interno]] · [[glosario]]
