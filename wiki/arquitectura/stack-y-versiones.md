---
title: Stack y versiones
type: arquitectura
status: vigente
tags: [arquitectura, versiones]
sources: ["PRD.md §17", "Registros oficiales consultados el 2026-10-07: MCR, Docker Hub, npm, NuGet, nodejs/Release, dotnet release-metadata, endoflife.date"]
aliases: [Versiones, Tecnologías]
created: 2026-10-07
updated: 2026-10-07
---

# Stack y versiones

Versiones verificadas contra los registros oficiales el **2026-10-07**; coinciden con las del PRD §17 (que pide volver a verificarlas al iniciar la implementación). Al actualizar una versión, cambia esta página y registra la entrada en el log.

## Runtime e imágenes

| Componente | Versión en uso | Dónde se fija | Soporte |
|---|---|---|---|
| .NET / ASP.NET Core | 10.0.12 (runtime) · SDK 10.0.401 en la imagen | `DOTNET_VERSION=10.0` (tag flotante de parches) · `backend/global.json` | LTS, hasta 2028-11-14 |
| Node.js | 24.21.0 (Krypton) | `NODE_VERSION=24` → `node:24-alpine` | LTS; pasa a *Maintenance* el 2026-10-20; fin 2028-04-30 |
| PostgreSQL | 18.6 | `POSTGRES_VERSION=18.6` (fijo) | Hasta 2030-11-14 |
| nginx (runtime frontend) | 1.30 (estable) | `NGINX_VERSION` en `frontend/Dockerfile` | — |
| Mailpit | v1.31 | `.env.example` | Solo desarrollo |
| Azurite | 3.37.0 | `.env.example` | Solo desarrollo |

> [!info] Node 24 vs Node 26
> Node 26 se vuelve LTS activo el 2026-10-28. El proyecto usa Node 24 por decisión del equipo; sigue soportado hasta 2028-04-30. Reevaluar al planificar el despliegue productivo.

## Paquetes backend (`Directory.Packages.props`)

| Paquete | Versión |
|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.3 |
| `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` | 10.0.12 |
| `Microsoft.AspNetCore.OpenApi` | 10.0.12 |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 10.0.12 |
| `xunit.v3` | 4.0.1 (sobre Microsoft.Testing.Platform) |
| `Microsoft.Testing.Extensions.CodeCoverage` | 18.12.0 |
| Por incorporar: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.12 disponible |

SignalR viene incluido en el framework compartido de ASP.NET Core 10.

## Paquetes frontend (`package.json`)

| Paquete | Versión |
|---|---|
| `react` / `react-dom` | 19.3.0 |
| `vite` | 8.3.3 (requiere Node ≥ 20.19 / 22.12) |
| `@vitejs/plugin-react` | 6.1.2 |
| `typescript` | 6.0.x (el que trae la plantilla oficial de Vite; TypeScript 7.0 nativo ya existe, migración pendiente de evaluar) |
| `vitest` | 5.0.3 |
| `oxlint` | 1.81+ |
| Por incorporar: `@microsoft/signalr` | 10.0.11 disponible |

## Política

- Imágenes de .NET y Node con **tag de versión mayor/menor** (`10.0`, `24`) para recibir parches de seguridad; PostgreSQL y emuladores **fijos** para reproducibilidad.
- Paquetes .NET centralizados; paquetes npm con `package-lock.json` versionado y `npm ci` en las imágenes.
- Google AI Studio sirve solo para prototipado de interfaz; no es fuente de versiones (PRD §17).

## Relacionado

- [[entorno-docker]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[analisis-prd-vs-docker-compose]]
