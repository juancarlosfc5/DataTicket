---
title: Elizabeth (PM)
type: entidad
status: vigente
tags: [personas]
sources: ["PRD.md §4", "PRD.md §5", "PRD.md §6.1", "PRD.md §6.2", "PRD.md §6.5", "PRD.md §10", "PRD.md §14"]
aliases: [Elizabeth, PM, Product Manager]
created: 2026-10-07
updated: 2026-10-07
---

# Elizabeth (PM)

Elizabeth es la Product Manager de DataTicket: responsable del triage, la asignación, la respuesta formal y el cierre manual de los tickets. También pertenece al equipo de Producción (PRD §5).

## Rol en código

Sus permisos se modelan con el rol **`ProductManager`**, no con su identidad personal (ver [[glosario]]). Así un administrador puede cubrirla y el sistema no depende de un usuario concreto.

## Responsabilidades y permisos

| Responsabilidad | Detalle | Fuente |
|---|---|---|
| Ver todos los tickets | Bandeja global de triage y seguimiento | PRD §5, §10 |
| Triage | Revisa categoría, urgencia e impacto de cada ticket nuevo | PRD §6.2 |
| Asignar y reasignar | Una o varias personas; ruta directa a Julián para Producción | PRD §6.2 |
| Gestionar participantes | Agrega o retira participantes durante toda la vida del ticket; auditado | PRD §5, §6.2 |
| Acceso permanente al chat | Siempre puede leer el chat de cualquier ticket, sin estar asociada | PRD §5 |
| Ajustar prioridad | Como integrante de Data Global, con motivo registrado | PRD §6.1 |
| Respuesta formal | Única junto a los administradores; texto + archivos, portal y correo | PRD §5, §6.5 |
| Confirmación verbal | Llama al cliente para confirmar que el caso quedó resuelto | PRD §6.5 |
| Cierre manual | Registra el cierre tras la confirmación telefónica | PRD §6.5 |
| Dashboard | Vista global de carga, antigüedad, estado y tiempos | PRD §4, §10 |

## Cuando no está

Un administrador cubre el triage con una vista limitada de la cola y queda asociado como participante al tomar un ticket (PRD §5, §6.2). Ver [[roles-y-permisos]].

## Lo que no hace el sistema por ella

- No graba ni transcribe la llamada de confirmación; solo queda el acto de cierre (PRD §6.5).
- No hay recordatorios ni cierre automático (PRD §6.5).

## Criterios de aceptación ligados

CA-03, CA-04, CA-11 y CA-12 (PRD §14). Ver [[criterios-de-aceptacion]].

## Relacionado

- [[equipo-data-global]] · [[julian-produccion]] · [[roles-y-permisos]]
- [[flujo-del-ticket]] · [[estados-del-ticket]] · [[matriz-de-prioridad]]
- [[chat-interno]] · [[dashboard-y-metricas]] · [[auditoria]]
