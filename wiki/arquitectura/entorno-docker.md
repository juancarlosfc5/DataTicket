---
title: Entorno Docker
type: arquitectura
status: vigente
tags: [arquitectura/infra, docker]
sources: ["docker-compose.yml", "backend/Dockerfile", "frontend/Dockerfile", ".env.example", "PRD.md §15"]
aliases: [Docker Compose, Entorno local, Contenedores]
created: 2026-10-07
updated: 2026-10-07
---

# Entorno Docker

`docker-compose.yml` levanta el stack completo de desarrollo: PostgreSQL, backend .NET 10, frontend React 19/Node 24 y dos emuladores (correo y Blob). **No es configuración productiva** (PRD §15). Decisión en [[adr-0007-entorno-local-compose-watch]].

> [!warning] Contradicción con el PRD
> PRD §15 dice que el Compose "levanta solo PostgreSQL 18.6". Era cierto para la versión original; desde 2026-10-07 incluye todos los servicios. El PRD no se editó (fuente inmutable); ver [[analisis-prd-vs-docker-compose]] y [[pendientes]].

## Servicios

| Servicio | Imagen / build | Puerto host | Función |
|---|---|---|---|
| `db` | `postgres:18.6` | 5432 | Base de datos; volumen `postgres_data` |
| `backend` | `backend/Dockerfile` → etapa `dev` (`mcr.microsoft.com/dotnet/sdk:10.0`) | 8080 | API + `dotnet watch`; *healthcheck* `/api/health/live` |
| `frontend` | `frontend/Dockerfile` → etapa `dev` (`node:24-alpine`) | 5173 | Vite con HMR y proxy de `/api` y `/hubs` |
| `mailpit` | `axllent/mailpit:v1.31` | 8025 (UI), 1025 (SMTP) | Captura correos de Identity y asignaciones |
| `azurite` | `mcr.microsoft.com/azure-storage/azurite:3.37.0` | 10000 | Emulador de Azure Blob (adjuntos); volumen `azurite_data` |

Orden de arranque: `db` sano → `backend` → `frontend`. **Todos los puertos se publican solo en `127.0.0.1`**: las credenciales de desarrollo (PostgreSQL, Azurite) y la bandeja de Mailpit, que muestra enlaces de invitación y restablecimiento, no quedan expuestas en la red.

## Uso

```bash
cp .env.example .env                 # opcional
docker compose up --build --watch    # recomendado: hot reload en backend y frontend
docker compose up -d --build         # sin sincronización de código
docker compose up -d db              # solo base de datos (backend/frontend en el host)
docker compose logs -f backend
docker compose down                  # conserva datos;  -v  los borra
```

**Compose Watch** sincroniza `backend/src` y `frontend/src` (y `public`, `index.html`, `vite.config.ts`) dentro de los contenedores; `dotnet watch` y Vite recargan. Cambios en `package.json`/`package-lock.json`, `Directory.*.props` o `global.json` **reconstruyen** la imagen. Se eligió frente a *bind mounts* porque en Windows los eventos de archivos no cruzan al contenedor y porque evita mezclar `bin/obj`/`node_modules` del host con los del contenedor.

## Variables (`.env.example`)

| Variable | Defecto | Uso |
|---|---|---|
| `POSTGRES_VERSION` · `DOTNET_VERSION` · `NODE_VERSION` | `18.6` · `10.0` · `24` | Versiones de imagen |
| `MAILPIT_VERSION` · `AZURITE_VERSION` | `v1.31` · `3.37.0` | Emuladores |
| `POSTGRES_DB` / `POSTGRES_USER` / `POSTGRES_PASSWORD` | `dataticket` / `dataticket` / `dataticket_local_only` | Solo desarrollo |
| `POSTGRES_PORT` · `BACKEND_PORT` · `FRONTEND_PORT` | `5432` · `8080` · `5173` | Puertos del host |
| `MAILPIT_UI_PORT` · `MAILPIT_SMTP_PORT` · `AZURITE_BLOB_PORT` | `8025` · `1025` · `10000` | Puertos del host |

Configuración que Compose inyecta al backend (contrato para los adaptadores):

| Clave .NET (`__` en variables) | Valor local |
|---|---|
| `ConnectionStrings:DataTicket` | `Host=db;Port=5432;…` |
| `Email:Smtp:Host` / `Email:Smtp:Port` / `Email:FromAddress` | `mailpit` / `1025` / `no-reply@dataticket.local` |
| `Storage:Blob:ConnectionString` | `UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite` (sin clave en el repo) |
| `Storage:Blob:PublicBaseUrl` | `http://localhost:10000/devstoreaccount1` (URL alcanzable desde el navegador) |

Las claves de correo y Blob aún no las consume el código; los adaptadores deben usarlas. Verificar `DevelopmentStorageProxyUri` con el SDK de Azure Storage al implementar.

## Etapas de los Dockerfiles

| Archivo | Etapas | Validación |
|---|---|---|
| `backend/Dockerfile` | `restore` → `dev` · `test` · `build` → `runtime` (`aspnet:10.0`, usuario `app` no root) | `docker build --target test ./backend` ejecuta las pruebas |
| `frontend/Dockerfile` | `deps` → `dev` · `build` → `runtime` (`nginxinc/nginx-unprivileged:1.30-alpine`) | nginx sirve el SPA y hace proxy de `/api` y `/hubs` (WebSocket) a `${BACKEND_UPSTREAM}` |

Al crear un proyecto .NET nuevo, agrega su `.csproj` a la etapa `restore`.

### nginx (imagen runtime del frontend)

| Aspecto | Configuración | Motivo |
|---|---|---|
| Upstream | `resolver ${NGINX_RESOLVER}` (por defecto `127.0.0.11`, DNS de Docker) + `proxy_pass` con variable | Re-resuelve el backend si su contenedor se recrea (verificado) |
| Tamaño de petición en `/api/` | `client_max_body_size 50m` | Adjuntos de hasta 10 MB y varios por solicitud (PRD §8); el límite real lo valida el backend |
| `/hubs/` | `Upgrade`/`Connection`, `proxy_read_timeout 1h`, `proxy_buffering off` | WebSockets de SignalR |
| Cabeceras al backend | `Host $http_host`, `X-Forwarded-For $remote_addr`, `X-Forwarded-Proto $scheme` | Pensado para nginx como borde; detrás de un terminador TLS hay que revisarlo |
| Caché | `index.html` con `no-cache`; `/assets/` inmutable 1 año y 404 si no existe | Evita servir un `index.html` viejo tras un despliegue |
| Seguridad | `X-Content-Type-Options`, `X-Frame-Options: DENY`, `Referrer-Policy` | CSP pendiente: depende del dominio público del Blob ([[pendientes]]) |

### Salud del backend

- `GET /api/health/live`: el proceso responde (no toca dependencias); lo usa el *healthcheck* de Compose.
- `GET /api/health`: dependencias con etiqueta `ready` (PostgreSQL). **Solo en Development** incluye el detalle por dependencia; en otros entornos devuelve únicamente `{"status": …}`.

## Verificación (2026-10-07)

- Los 5 servicios arriba; `db`, `backend` y `mailpit` *healthy*.
- `GET :8080/api/health` y `GET :5173/api/health` (vía proxy) → `Healthy` con check `postgresql`.
- `/hubs/*` vía Vite y vía nginx llega a Kestrel (incluido el *upgrade* WebSocket).
- Versiones dentro de los contenedores: SDK 10.0.401, ASP.NET Core 10.0.12, Node 24.21.0, React 19.3.0, PostgreSQL 18.6.
- Compose Watch: cambios sincronizados y `dotnet watch` aplicó hot reload.
- Imágenes runtime: backend 351 MB (uid `app`), frontend 90 MB.
- Tras la revisión de código: puertos en `127.0.0.1`; en runtime, `/api/health` sin detalle, `index.html` con `no-cache` y cabeceras de seguridad, POST de 12 MB aceptado por nginx, backend recreado alcanzado sin reiniciar nginx.

## Problemas frecuentes

- **PostgreSQL instalado en Windows** ocupa `5432` (pasa en el equipo de la persona líder): el contenedor no puede publicar ese puerto y, peor, `localhost:5432` llega a la base nativa. Solución: `POSTGRES_PORT=5433` en `.env`. Si ejecutas el backend fuera de Docker, sobrescribe la cadena: `ConnectionStrings__DataTicket="Host=localhost;Port=5433;Database=dataticket;Username=dataticket;Password=dataticket_local_only"` (variable de entorno o `dotnet user-secrets`).
- **Otro puerto ocupado** (8080, 5173…): cámbialo en `.env`.
- **Dependencias npm nuevas no aparecen**: con `--watch` se reconstruye solo; sin él, `docker compose up -d --build frontend`.
- **Rutas en Git Bash** (`/app/...` convertido a `C:/Program Files/Git/...`) al usar `docker compose exec`: antepón `MSYS_NO_PATHCONV=1`.
- **Base corrupta o quieres empezar de cero**: `docker compose down -v`.

## Relacionado

- [[arquitectura-general]] · [[stack-y-versiones]] · [[adr-0001-monorepo-contenedorizado]] · [[adr-0007-entorno-local-compose-watch]] · [[analisis-prd-vs-docker-compose]]
