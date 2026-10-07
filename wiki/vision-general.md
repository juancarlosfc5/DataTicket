---
title: DataTicket — visión general
type: vision
status: vigente
tags: [vision, producto, arquitectura]
sources: ["PRD.md §1–§4, §13, §17"]
aliases: [DataTicket, Overview, Resumen del proyecto]
created: 2026-10-07
updated: 2026-10-07
---

# DataTicket — visión general

**DataTicket** es el portal web de soporte de Data Global S.A.S. para los clientes que adquirieron sus productos de desarrollo. Centraliza la radicación, el triage de la PM, la colaboración entre Desarrollo y Producción y la entrega formal de la solución (PRD §1). Esta página es el punto de entrada de la wiki.

## El producto en cinco ideas

1. **Dos superficies**: el **portal del cliente** (radicar, ver estado resumido y respuesta formal) y el **espacio interno** (triage, participantes, chat, dashboard) (PRD §1) → [[roles-y-permisos]].
2. **Trazabilidad**: cada ticket conserva participantes, mensajes, adjuntos, cambios de estado y respuesta final en una auditoría append-only (PRD §4, §12) → [[auditoria]].
3. **Colaboración interna en tiempo real**: chat privado por ticket para participantes, persistido y con confirmaciones de lectura (PRD §7) → [[chat-interno]].
4. **Cierre humano**: la PM o un administrador envía la respuesta formal y cierra manualmente tras confirmar por teléfono (PRD §6.5) → [[flujo-del-ticket]].
5. **Medir antes de prometer**: métricas operativas sin SLA en el MVP (PRD §4) → [[dashboard-y-metricas]].

## Actores

Solicitante y coordinador de empresa cliente; [[elizabeth-pm]] (PM y triage); administradores; equipos de Desarrollo y Producción, este último liderado por [[julian-produccion]] → [[equipo-data-global]].

## Cómo está construido

| Capa | Tecnología | Página |
|---|---|---|
| Backend | .NET 10, ASP.NET Core, Identity, SignalR, EF Core — hexagonal | [[backend-hexagonal]] |
| Frontend | React 19 + TypeScript, Vite, Node 24 — MVC | [[frontend-mvc]] |
| Datos | PostgreSQL 18.6 | [[persistencia-postgresql]] |
| Entorno | Docker Compose con Compose Watch, Mailpit y Azurite | [[entorno-docker]] |

Vista de conjunto en [[arquitectura-general]]; versiones en [[stack-y-versiones]].

## Dónde estamos (2026-10-07)

- **Fase 0 — base técnica** (bloqueante): esqueleto creado y verificado (proyectos, pruebas de arquitectura, Compose completo). Faltan Identity, empresas, usuarios y permisos.
- Fases siguientes y alcance en [[fases-y-alcance]]; checklist del MVP en [[criterios-de-aceptacion]].
- Preguntas abiertas en [[pendientes]].

## Cómo se trabaja

Monorepo en GitHub ([[flujo-de-trabajo-github]]) con un agente orquestador que mantiene esta wiki en cada interacción ([[trabajar-con-el-agente]]).

## Relacionado

- [[index]] · [[glosario]] · [[fuente-prd-v0-1]]
