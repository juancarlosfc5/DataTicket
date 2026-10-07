---
title: "Fuente: PRD de DataTicket v0.1"
type: fuente
status: vigente
tags: [fuente, producto]
sources: ["PRD.md"]
aliases: [PRD, Product Requirements Document]
created: 2026-10-07
updated: 2026-10-07
source_date: 2026-10-05
source_author: Data Global S.A.S.
---

# Fuente: PRD de DataTicket v0.1

`PRD.md` (raíz del repositorio) es el documento de requisitos de producto, versión 0.1 "borrador funcional para revisión", del 5 de octubre de 2026. Consolida la conversación de producto, el borrador del desarrollador (`mvp-sistema-tickets.md`) y el prototipo `DataTicket.html`; las decisiones del dueño del producto prevalecen sobre esos insumos (PRD §2). Es la fuente de mayor autoridad después de las instrucciones explícitas del equipo.

## Puntos clave

- Dos superficies: portal del cliente y espacio de trabajo interno (PRD §1).
- Trazabilidad del ticket y colaboración interna son el centro; formularios por cliente van al final (PRD §1, §9).
- Chat interno en tiempo real con SignalR y persistencia en PostgreSQL (PRD §7).
- Autorización estricta en servidor: multiempresa, participantes, administrador sin acceso automático (PRD §5).
- Adjuntos en Azure Blob con **URL pública permanente** como riesgo aceptado (PRD §8).
- Cierre manual tras confirmación telefónica; sin SLA en el MVP (PRD §4, §6.5).
- Stack: .NET 10, React + TypeScript, PostgreSQL 18, Docker Compose (PRD §17).

## Mapa de secciones → wiki

| Sección | Páginas |
|---|---|
| §1–§4 | [[vision-general]], [[dashboard-y-metricas]] |
| §5 | [[roles-y-permisos]], [[equipo-data-global]], [[elizabeth-pm]], [[julian-produccion]] |
| §6 | [[flujo-del-ticket]], [[estados-del-ticket]], [[matriz-de-prioridad]] |
| §7 | [[chat-interno]], [[tiempo-real-signalr]], [[adr-0005-signalr-para-chat]] |
| §8 | [[archivos-adjuntos]], [[adr-0006-urls-publicas-azure-blob]] |
| §9 | [[formularios-configurables]] |
| §10–§11 | [[dashboard-y-metricas]], [[notificaciones]] |
| §12 | [[auditoria]], [[autenticacion-identity]], [[persistencia-postgresql]] |
| §13 | [[fases-y-alcance]] |
| §14 | [[criterios-de-aceptacion]] |
| §15 | [[entorno-docker]] (desactualizada en el PRD) |
| §16 | [[pendientes]] |
| §17 | [[stack-y-versiones]] |

## Contradicciones y vacíos

> [!warning] Contradicción
> §15 describe un Compose con solo PostgreSQL y sin proyectos .NET/React; desde el 2026-10-07 el repositorio tiene ambos y un Compose completo ([[analisis-prd-vs-docker-compose]]).

Además se detectaron más de veinte vacíos o ambigüedades (transiciones de estado, destinatario del correo de respuesta formal, tipos de adjunto en el chat, métrica de "primera respuesta"…). Están consolidados en [[pendientes]].

## Fuentes citadas por el PRD aún no ingeridas

- `mvp-sistema-tickets.md` (borrador del desarrollador) y `DataTicket.html` (prototipo navegable).
- ZIP de diseño del equipo.

Cuando estén disponibles, colocarlas en `wiki/raw/` e ingerirlas.

## Relacionado

- [[vision-general]] · [[pendientes]] · [[fuente-patron-llm-wiki]]
