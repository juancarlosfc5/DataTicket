---
title: "ADR-0004: Identity con cookie en el mismo origen"
type: decision
status: aceptada
tags: [decision, seguridad, identity]
sources: ["Decisión de la persona líder (2026-10-07): usar ASP.NET Core Identity", "PRD.md §5, §7, §12"]
aliases: [ADR-0004]
created: 2026-10-07
updated: 2026-10-09
decision_date: 2026-10-09
deciders: [juancarlosfc5 (persona líder del proyecto)]
---

# ADR-0004: Identity con cookie en el mismo origen

> [!info] Decisión aceptada (2026-10-09)
> La persona líder del proyecto aceptó la opción 1 (decisión D1 de [[plan-goal-login-y-loop-chat]]). Que el login use **ASP.NET Core Identity** ya estaba decidido; con esta aceptación queda fijado también el **mecanismo de sesión**. Desbloquea HU-003 (DoD-08) y el resto de [[ep-002-identidad-y-acceso]].

## Contexto

- El SPA y la API se sirven tras un proxy que expone un único origen (`/api`, `/hubs`), verificado en Vite y nginx ([[entorno-docker]]).
- SignalR necesita autenticar la conexión del hub (PRD §7).
- No hay registro público y el restablecimiento no debe revelar cuentas (PRD §5).

## Opciones consideradas

1. **Cookie de Identity, mismo origen** — `HttpOnly` (no la lee JavaScript), viaja sola a `/api` y al WebSocket del hub, sin CORS. Requiere antiforgery en mutaciones.
2. **Bearer tokens de `MapIdentityApi`** — sirve para clientes no web, pero el token vive en JavaScript (riesgo ante XSS), SignalR lo envía en la query string y `MapIdentityApi` expone `/register`, contrario al PRD.

## Decisión

Opción 1: cookie de Identity `HttpOnly`, `Secure`, `SameSite=Lax`, endpoints de autenticación propios en `/api/auth`, antiforgery por encabezado y claves de Data Protection en PostgreSQL. Detalle en [[autenticacion-identity]].

## Consecuencias

- El frontend nunca maneja tokens; `fetch` usa `credentials: 'same-origin'` (ya implementado en `core/http/httpClient.ts`).
- Producción debe mantener el mismo origen (nginx del frontend o un proxy equivalente) o rediseñar CORS + cookies.
- Una app móvil nativa (fuera del MVP) requeriría otro esquema.

## Relacionado

- [[autenticacion-identity]] · [[roles-y-permisos]] · [[tiempo-real-signalr]] · [[pendientes]] · [[plan-goal-login-y-loop-chat]] · [[hu-003-iniciar-y-cerrar-sesion]]
