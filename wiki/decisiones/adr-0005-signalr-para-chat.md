---
title: "ADR-0005: SignalR para el chat interno"
type: decision
status: aceptada
tags: [decision, signalr, producto/chat]
sources: ["PRD.md §7", "Decisión de la persona líder (2026-10-07)"]
aliases: [ADR-0005]
created: 2026-10-07
updated: 2026-10-07
decision_date: 2026-10-05
deciders: [Dueño del producto (PRD), Persona líder del proyecto]
---

# ADR-0005: SignalR para el chat interno

## Contexto

El chat interno en tiempo real es prioridad del producto (PRD §2, §7). Debe ser persistente, recuperable tras reconexiones y respetar la membresía por ticket.

## Opciones consideradas

1. **ASP.NET Core SignalR** + cliente `@microsoft/signalr` — WebSockets con *fallback* automático, reconexión, grupos; integrado con la autenticación de ASP.NET Core. Recomendado por Microsoft frente a WebSockets en crudo (PRD §17).
2. **WebSockets manuales** — más control, pero hay que reimplementar transporte, reconexión y enrutamiento.
3. **Sondeo (polling)** — simple, pero no es tiempo real.

## Decisión

SignalR, con PostgreSQL como almacenamiento del historial (persistir antes de publicar) y validación de membresía en servidor en cada operación. Sin backplane en el MVP de una instancia.

## Consecuencias

- El proxy (Vite/nginx) debe permitir *upgrade* a WebSocket en `/hubs` — ya configurado y verificado.
- Al escalar a varias instancias habrá que añadir Redis o Azure SignalR ([[pendientes]]).

## Relacionado

- [[tiempo-real-signalr]] · [[chat-interno]]
