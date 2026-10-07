---
title: "Análisis: PRD vs. docker-compose original"
type: sintesis
status: vigente
tags: [sintesis, docker, versiones]
sources: ["PRD.md §7, §8, §15, §17", "docker-compose.yml (versión original del 2026-10-05)", "Registros oficiales consultados el 2026-10-07"]
aliases: [Brechas del Compose]
created: 2026-10-07
updated: 2026-10-07
question: "El PRD menciona .NET 10, Node 24 y React 19, pero el Compose no los contiene: ¿qué falta y cómo se corrige para que funcione?"
---

# Análisis: PRD vs. docker-compose original

**Pregunta:** el PRD detalla .NET 10, Node 24 y React 19, pero el `docker-compose.yml` no los contiene. ¿Qué falta y cómo se corrige considerando Identity y SignalR?

**Respuesta corta:** la observación era correcta. El Compose original solo tenía `postgres:18.6` porque aún no existían los proyectos (el propio PRD §15 lo reconoce). Se crearon `backend/` y `frontend/`, sus Dockerfiles multi-etapa y un Compose con cinco servicios; todo se verificó levantando el stack el 2026-10-07.

## Lo que había

| Elemento | Compose original | PRD |
|---|---|---|
| PostgreSQL | `postgres:18.6`, volumen, healthcheck | §15, §17 ✔ |
| Backend .NET 10 | — | §7, §13, §17 |
| Frontend React + Node | — | §7, §17 (Node 24 y React 19 por decisión del equipo) |
| Correo (Identity, notificaciones) | — | §5, §11 (correo real fuera del Compose, §15) |
| Azure Blob | — | §8 (no incluido, §15) |

## Verificación de versiones (2026-10-07)

| PRD dice | Registro oficial | Resultado |
|---|---|---|
| .NET 10.0.12 LTS hasta 2028-11-14 | `mcr.microsoft.com/dotnet/aspnet:10.0.12`; release-metadata: 10.0.12 del 2026-09-08, EOL 2028-11-14 | ✔ |
| React 19.3 | npm `react@19.3.0` (*latest*) | ✔ |
| PostgreSQL 18.6 | Docker Hub `postgres:18.6` | ✔ |
| Node 24 (decisión del equipo) | `node:24-alpine` → 24.21.0; LTS hasta 2028-04-30 | ✔ (pasa a *Maintenance* el 2026-10-20) |

## Lo que se añadió

- **backend** (`mcr.microsoft.com/dotnet/sdk:10.0` en desarrollo, `aspnet:10.0` en runtime), hexagonal, con `/api/health` contra PostgreSQL.
- **frontend** (`node:24-alpine` en desarrollo, nginx en runtime), React 19.3 MVC.
- **Identity**: proxy same-origin para cookies ([[adr-0004-autenticacion-cookie-mismo-origen]]) y **Mailpit** para correos de invitación y restablecimiento.
- **SignalR**: `/hubs` con *upgrade* a WebSocket en Vite y nginx ([[tiempo-real-signalr]]).
- **Azurite** para adjuntos en Blob.
- **Compose Watch** para hot reload sin *bind mounts* ([[adr-0007-entorno-local-compose-watch]]).

## Evidencia

Detalle de comandos y resultados en [[entorno-docker]] (sección *Verificación*): cinco servicios arriba, salud `Healthy` directa y vía proxy, `/hubs` llega a Kestrel, versiones correctas dentro de los contenedores, hot reload aplicado e imágenes runtime funcionando.

> [!warning] Consecuencia documental
> PRD §15 queda desactualizado. No se editó (fuente inmutable); queda registrado en [[pendientes]] para que el dueño del PRD lo actualice en la próxima versión.

## Relacionado

- [[entorno-docker]] · [[stack-y-versiones]] · [[fuente-prd-v0-1]]
