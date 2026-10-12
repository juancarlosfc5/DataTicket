---
title: Stack y versiones
type: arquitectura
status: vigente
tags: [arquitectura, versiones]
sources: ["PRD.md §17", "Registros oficiales consultados el 2026-10-07: MCR, Docker Hub, npm, NuGet, nodejs/Release, dotnet release-metadata, endoflife.date", "backend/Directory.Packages.props", "backend/.config/dotnet-tools.json", "NuGet (api.nuget.org) consultado el 2026-10-12 para los paquetes de B3"]
aliases: [Versiones, Tecnologías]
created: 2026-10-07
updated: 2026-10-12
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
| `Testcontainers.PostgreSql` | 4.16.0 (verificada en NuGet el 2026-10-09; usa `testcontainers/ryuk:0.14.0`) |
| `Microsoft.AspNetCore.Mvc.Testing` | 10.0.12 |
| `Microsoft.Extensions.TimeProvider.Testing` | 10.10.0 |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.12 (Infrastructure; desde B3 del goal del login) |
| `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` | 10.0.12 (Infrastructure; claves de Data Protection en PostgreSQL; desde B3) |
| `EFCore.NamingConventions` | 10.0.1 (Infrastructure; `UseSnakeCaseNamingConvention`; desde B3) |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.12 (Api, como dependencia privada: solo para `dotnet ef`; desde B3) |

Herramientas locales de .NET (`backend/.config/dotnet-tools.json`; se restauran con `cd backend && dotnet tool restore`):

| Herramienta | Versión |
|---|---|
| `dotnet-ef` | 10.0.12 (fija, `rollForward: false`; migraciones de [[persistencia-postgresql]]) |

Las cinco versiones añadidas en B3 se comprobaron en NuGet el 2026-10-12: existen y son la última 10.0.x publicada de cada paquete.

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
| `react-router` | 8.4.0 exacta ([[adr-0010-enrutador-react-router]]; peer `react >=19.2.7`, engine `node >=22.22`) |
| `@fontsource-variable/archivo` | 5.3.0 (se carga `wdth.css`, familia `'Archivo Variable'`) |
| `@testing-library/react` · `/dom` · `/user-event` · `/jest-dom` | 16.3.3 · 10.4.2 · 14.6.7 · 7.0.1 (desarrollo) |
| `jsdom` | 30.1.2 (desarrollo; engine `^24.15`) |

## Política

- Imágenes de .NET y Node con **tag de versión mayor/menor** (`10.0`, `24`) para recibir parches de seguridad; PostgreSQL y emuladores **fijos** para reproducibilidad.
- Paquetes .NET centralizados; paquetes npm con `package-lock.json` versionado y `npm ci` en las imágenes.
- Google AI Studio sirve solo para prototipado de interfaz; no es fuente de versiones (PRD §17).

## Relacionado

- [[entorno-docker]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[analisis-prd-vs-docker-compose]] · [[persistencia-postgresql]] · [[autenticacion-identity]]
