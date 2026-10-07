---
title: "ADR-0003: Arquitectura MVC en el frontend"
type: decision
status: aceptada
tags: [decision, arquitectura/frontend, mvc]
sources: ["Decisión de la persona líder del proyecto (2026-10-07)"]
aliases: [ADR-0003]
created: 2026-10-07
updated: 2026-10-07
decision_date: 2026-10-07
deciders: [Persona líder del proyecto]
---

# ADR-0003: Arquitectura MVC en el frontend

## Contexto

El SPA tendrá dos superficies con permisos distintos (portal del cliente y espacio interno; PRD §1, §10) y un chat en tiempo real. Sin una separación clara, la lógica de datos y de SignalR tiende a mezclarse con los componentes.

## Decisión

MVC por módulo funcional, adaptado a React:

- **Modelo** (`models/`): tipos, normalización de datos externos y gateways de API/SignalR; TypeScript puro.
- **Controlador** (`controllers/`): hooks `useXController` que orquestan modelos y exponen estado + acciones.
- **Vista** (`views/`): componentes puros que reciben props.

Las fronteras se hacen cumplir con `no-restricted-imports` en `oxlint` (ver [[frontend-mvc]]).

## Consecuencias

- Las vistas se pueden diseñar y probar sin backend; los controladores, sin DOM.
- Más archivos por pantalla; el módulo `system` sirve de plantilla.
- Una violación de capas rompe `npm run lint`.

## Relacionado

- [[frontend-mvc]] · [[adr-0001-monorepo-contenedorizado]]
