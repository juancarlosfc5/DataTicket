---
title: Bitácora
type: log
status: vigente
tags: [log]
sources: []
aliases: [Log, Historial]
created: 2026-10-07
updated: 2026-10-07
---

# Bitácora

Registro cronológico **append-only** de lo que cambia en el proyecto y en la wiki. Solo se añaden entradas al final. Formato y tipos en `AGENTS.md` §5.5. Últimas entradas: `grep "^## \[" wiki/log.md | tail -5`.

## [2026-10-07] setup | Monorepo, entorno Docker, agentes y wiki inicial
- Autor: juancarlosfc5 (sesión de configuración inicial con Claude Code)
- Cambios:
  - Creados `backend/` (.NET 10, hexagonal: Domain, Application, Infrastructure, Api + pruebas de arquitectura) y `frontend/` (React 19.3 + TS, Vite 8, MVC con fronteras en oxlint y módulo de ejemplo `system/health`).
  - `docker-compose.yml` ampliado de solo PostgreSQL a `db`, `backend`, `frontend`, `mailpit` y `azurite`, con Compose Watch, proxy same-origin `/api` y `/hubs` (WebSocket) y puertos solo en `127.0.0.1`; `.env.example`.
  - Agentes de Claude Code: `orchestrator` (hilo principal vía `.claude/settings.json`), `backend-engineer`, `frontend-engineer`, `quality-reviewer`, `wiki-keeper`; hook `Stop` `wiki-guard.mjs`.
  - Esquema `AGENTS.md` (+ `CLAUDE.md`), `README.md`, `.gitignore`, `.gitattributes` (`merge=union` para este log), `.editorconfig`, plantilla de PR.
- Verificación: backend 0 advertencias y 5/5 pruebas; frontend oxlint limpio, 3/3 pruebas, build OK; cinco servicios arriba y sanos; salud OK directa y vía proxy; `/hubs` llega a Kestrel vía Vite y nginx; versiones dentro de los contenedores: ASP.NET Core 10.0.12, Node 24.21.0, React 19.3.0, PostgreSQL 18.6; hot reload de Compose Watch e imágenes runtime funcionando.
- Wiki: [[vision-general]], [[arquitectura-general]], [[backend-hexagonal]], [[frontend-mvc]], [[entorno-docker]], [[stack-y-versiones]], [[trabajar-con-el-agente]], [[flujo-de-trabajo-github]], [[estrategia-de-pruebas]], [[equipo-del-repositorio]]
- Pendiente: `git init` y repositorio en GitHub (ver [[flujo-de-trabajo-github]]).

## [2026-10-07] ingesta | PRD de DataTicket v0.1
- Autor: juancarlosfc5
- Cambios: PRD destilado en 12 páginas de producto, modelo de dominio propuesto, glosario español ↔ código y páginas de personas. Revisión del PRD con 4 contradicciones y 17 vacíos registrados.
- Wiki: [[fuente-prd-v0-1]], [[flujo-del-ticket]], [[estados-del-ticket]], [[matriz-de-prioridad]], [[roles-y-permisos]], [[chat-interno]], [[archivos-adjuntos]], [[notificaciones]], [[auditoria]], [[dashboard-y-metricas]], [[formularios-configurables]], [[fases-y-alcance]], [[criterios-de-aceptacion]], [[modelo-de-dominio]], [[glosario]], [[equipo-data-global]], [[elizabeth-pm]], [[julian-produccion]], [[pendientes]]
- Pendiente: ingerir `mvp-sistema-tickets.md`, `DataTicket.html` y el ZIP de diseño cuando estén disponibles.

## [2026-10-07] ingesta | Patrón LLM Wiki
- Autor: juancarlosfc5
- Cambios: resumido el documento de idea y documentada su instanciación (bóveda `wiki/`, esquema `AGENTS.md`, `index.md`, este log, operaciones de ingesta/consulta/lint).
- Wiki: [[fuente-patron-llm-wiki]], [[index]]

## [2026-10-07] consulta | PRD vs. docker-compose original
- Autor: juancarlosfc5
- Cambios: confirmada la observación de que el Compose no tenía .NET 10, Node 24 ni React 19; versiones verificadas en MCR, Docker Hub, npm, NuGet y calendarios oficiales. Análisis archivado como síntesis; PRD §15 queda desactualizado (registrado, no editado).
- Wiki: [[analisis-prd-vs-docker-compose]], [[stack-y-versiones]], [[pendientes]]

## [2026-10-07] decision | ADR iniciales
- Autor: juancarlosfc5
- Cambios: registradas como aceptadas las decisiones del equipo (monorepo, hexagonal, MVC) y del PRD (SignalR, URLs públicas en Blob), más el entorno con Compose Watch. Propuesta pendiente: sesión de Identity por cookie same-origin.
- Wiki: [[adr-0001-monorepo-contenedorizado]], [[adr-0002-backend-hexagonal]], [[adr-0003-frontend-mvc]], [[adr-0004-autenticacion-cookie-mismo-origen]], [[adr-0005-signalr-para-chat]], [[adr-0006-urls-publicas-azure-blob]], [[adr-0007-entorno-local-compose-watch]], [[autenticacion-identity]], [[tiempo-real-signalr]], [[persistencia-postgresql]]

## [2026-10-07] fix | Correcciones de la revisión de código
- Autor: juancarlosfc5
- Cambios:
  - Puertos de Compose solo en `127.0.0.1`; sincronización de `tsconfig*.json` en Compose Watch.
  - nginx runtime: re-resolución DNS del backend, `client_max_body_size 50m` en `/api/`, `index.html` sin caché, cabeceras de seguridad y de proxy ajustadas.
  - `/api/health` filtra por la etiqueta `ready` y solo muestra detalle en Development; avisos NuGet NU1901–NU1904 no rompen el build; la prueba de arquitectura del dominio también prohíbe `Microsoft.Extensions.*`.
  - Hook `wiki-guard`: falla en abierto si no puede leer su entrada, trata un log ausente como desactualizado, normaliza rutas (git en un repo padre, rutas cortas 8.3) e ignora `.env*` y `*.log`.
  - `dotnet` se ejecuta desde `backend/` (ahí está `global.json`; desde la raíz cae en VSTest y falla): documentación corregida.
  - Este equipo tiene un PostgreSQL nativo de Windows en `5432`: `.env` local con `POSTGRES_PORT=5433` (no versionado) y solución documentada.
- Wiki: [[entorno-docker]], [[backend-hexagonal]], [[autenticacion-identity]], [[estrategia-de-pruebas]], [[pendientes]]
