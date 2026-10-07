# DataTicket

Portal web de soporte de **Data Global S.A.S.**: los clientes radican tickets, la PM hace triage y asigna, Desarrollo y Producción colaboran en un chat interno en tiempo real y la PM entrega una respuesta formal antes del cierre manual. Requisitos completos en [`PRD.md`](PRD.md).

| | Tecnología | Arquitectura |
|---|---|---|
| `backend/` | .NET 10 · ASP.NET Core · Identity · SignalR · EF Core | Hexagonal |
| `frontend/` | React 19 · TypeScript · Vite · Node 24 | MVC |
| Datos | PostgreSQL 18.6 | |
| `wiki/` | Bóveda Obsidian mantenida por el agente | LLM Wiki |

## Arranque rápido

Requisitos: Docker Desktop (Compose v2.22+). Para trabajar fuera de contenedores: .NET SDK 10 y Node 24.

```bash
cp .env.example .env              # opcional: sobrescribir puertos o credenciales locales
docker compose up --build --watch # stack completo con hot reload
```

| Servicio | URL |
|---|---|
| Aplicación (frontend + proxy `/api`, `/hubs`) | http://localhost:5173 |
| API / OpenAPI | http://localhost:8080/openapi/v1.json |
| Salud | http://localhost:5173/api/health |
| Correo de desarrollo (Mailpit) | http://localhost:8025 |
| Azure Blob local (Azurite) | http://localhost:10000 |
| PostgreSQL | `localhost:5432` o el `POSTGRES_PORT` de tu `.env` (usuario/base `dataticket`) |

Todos los puertos se publican solo en `127.0.0.1`. Si tienes PostgreSQL instalado en Windows, usa `POSTGRES_PORT=5433` en `.env`.

Detalles, variables y solución de problemas: [`wiki/arquitectura/entorno-docker.md`](wiki/arquitectura/entorno-docker.md).

## Trabajar con el agente

El repositorio incluye un equipo de agentes para Claude Code. Al abrir `claude` en la raíz, la sesión corre como el **orquestador**, que consulta la wiki, delega en los especialistas (backend, frontend, revisión, wiki), verifica con comandos reales y **actualiza la wiki en cada interacción**. Guía: [`wiki/proceso/trabajar-con-el-agente.md`](wiki/proceso/trabajar-con-el-agente.md). Esquema y reglas para cualquier agente: [`AGENTS.md`](AGENTS.md).

## Wiki del proyecto

Abre la carpeta `wiki/` como bóveda en Obsidian y empieza por `index.md`. La wiki la escribe y mantiene el agente; las personas la leen, la revisan y aportan fuentes nuevas en `wiki/raw/`.

## Colaboración

Ramas `feature/…`, commits convencionales y Pull Request con revisión y wiki actualizada. Ver [`wiki/proceso/flujo-de-trabajo-github.md`](wiki/proceso/flujo-de-trabajo-github.md).
