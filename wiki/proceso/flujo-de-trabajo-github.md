---
title: Flujo de trabajo en GitHub
type: guia
status: vigente
tags: [proceso, git, github]
sources: ["AGENTS.md §8", "Petición de la persona líder (2026-10-07): trabajo colaborativo con Laura, Juan David y Brayan por GitHub"]
aliases: [Git, Pull requests, Ramas]
created: 2026-10-07
updated: 2026-10-07
---

# Flujo de trabajo en GitHub

Cómo colabora el [[equipo-del-repositorio]] en el monorepo: ramas cortas, commits convencionales y Pull Requests revisados que incluyen la wiki actualizada.

## Puesta en marcha (una sola vez)

1. La persona líder crea el repositorio en GitHub e invita a Laura, Juan David y Brayan como colaboradores.
2. Desde la raíz del proyecto:

   ```bash
   git init -b main
   git add .
   git commit -m "chore: estructura inicial, entorno Docker, agentes y wiki"
   git remote add origin <url-del-repositorio>
   git push -u origin main
   ```

3. En GitHub → *Settings → Branches*: proteger `main` (PR obligatorio, al menos 1 aprobación, sin *force push*).
4. Cada colaborador clona, copia `.env.example` a `.env` si lo necesita y ejecuta `docker compose up --build --watch`.

## Día a día

```bash
git switch main && git pull
git switch -c feature/tickets-radicacion
# … trabajar con el agente: código + pruebas + wiki …
git add -A && git commit -m "feat: radicación de tickets con adjuntos"
git push -u origin feature/tickets-radicacion
```

| Prefijo de rama | Uso |
|---|---|
| `feature/<area>-<desc>` | Funcionalidad nueva |
| `fix/<desc>` | Corrección |
| `docs/<desc>` | Solo wiki/documentación |
| `chore/<desc>` | Infraestructura, dependencias, configuración |

Commits convencionales: `feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`, `perf:`, `ci:`.

## Pull Requests

- Plantilla en `.github/pull_request_template.md`: CA del PRD cubiertos, verificaciones ejecutadas y **páginas de la wiki modificadas**.
- Al menos una revisión de otra persona del equipo. Para cambios de seguridad (permisos, chat, multiempresa), pedir además la revisión del agente `quality-reviewer`.
- PRs pequeños: idealmente una funcionalidad vertical (contrato + backend + frontend + wiki).

## La wiki en equipo

- `wiki/log.md` se fusiona con `merge=union` (`.gitattributes`): las entradas de ambas ramas se conservan.
- En conflictos de otras páginas de la wiki, conservar el aporte de ambas ramas y reconciliar.
- ADR: comprobar el último número en `main` antes de crear uno; si colisiona al fusionar, renumerar el más reciente.

## Pendiente

- Usuarios de GitHub del equipo para un `CODEOWNERS` (p. ej. backend/frontend/wiki) → [[pendientes]].
- CI (GitHub Actions) que ejecute pruebas de backend y lint/pruebas/build de frontend en cada PR → [[pendientes]].

## Relacionado

- [[equipo-del-repositorio]] · [[trabajar-con-el-agente]] · [[estrategia-de-pruebas]]
