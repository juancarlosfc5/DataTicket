---
title: "ADR-0007: Entorno local con Compose Watch y emuladores"
type: decision
status: aceptada
tags: [decision, docker, arquitectura/infra]
sources: ["Petición de la persona líder (2026-10-07): actualizar el Compose con .NET 10, Node 24 y React 19, considerando Identity y SignalR", "PRD.md §8, §15"]
aliases: [ADR-0007]
created: 2026-10-07
updated: 2026-10-07
decision_date: 2026-10-07
deciders: [Persona líder del proyecto (petición), orquestador (diseño)]
---

# ADR-0007: Entorno local con Compose Watch y emuladores

## Contexto

El Compose original solo levantaba PostgreSQL (PRD §15); faltaban las imágenes de .NET 10 y Node 24/React 19. El equipo trabaja mayoritariamente en Windows, donde los *bind mounts* no propagan eventos de archivos a contenedores Linux y mezclan `bin/obj` y `node_modules` del host con los del contenedor. Identity necesita enviar correos (invitación, restablecimiento) y los adjuntos necesitan Blob (PRD §5, §8).

## Opciones consideradas

1. **Compose Watch** (`develop.watch`: `sync` + `rebuild`) — cambios copiados al contenedor, eventos nativos, sin conflictos de dependencias.
2. **Bind mounts con *polling*** — funciona, pero es más lento y requiere volúmenes anónimos para `node_modules`/`obj`.
3. **Solo base de datos en Docker** — cada persona instala SDK y Node; menos reproducible.

## Decisión

- Compose Watch para `backend` (`dotnet watch`) y `frontend` (Vite).
- Dockerfiles multi-etapa con `dev`, `test`/`build` y `runtime` (no root).
- **Mailpit** como SMTP local y **Azurite** (solo Blob) como emulador de Azure Storage.
- Proxy same-origin `/api` + `/hubs` (WebSocket) en Vite y nginx.

## Consecuencias

- `docker compose up --build --watch` es el flujo diario; sin `--watch` no hay hot reload.
- Agregar un proyecto .NET exige actualizar la etapa `restore` del Dockerfile.
- Sigue siendo un entorno **de desarrollo**: sin TLS, respaldos ni secretos productivos.
- Verificado el 2026-10-07 (ver [[entorno-docker]]).

## Relacionado

- [[entorno-docker]] · [[adr-0001-monorepo-contenedorizado]] · [[analisis-prd-vs-docker-compose]]
