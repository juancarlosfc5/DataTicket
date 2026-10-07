---
title: "ADR-0001: Monorepo contenedorizado"
type: decision
status: aceptada
tags: [decision, arquitectura, docker]
sources: ["Decisión de la persona líder del proyecto (2026-10-07)", "PRD.md §13, §17"]
aliases: [ADR-0001, Monorepo]
created: 2026-10-07
updated: 2026-10-07
decision_date: 2026-10-07
deciders: [Persona líder del proyecto]
---

# ADR-0001: Monorepo contenedorizado

## Contexto

El PRD define un monolito modular .NET 10 + React/TypeScript + PostgreSQL con despliegue inicial local en Docker Compose (PRD §13, §17). Cuatro personas (ver [[equipo-del-repositorio]]) colaborarán por GitHub y necesitan un único lugar para código, entorno y conocimiento.

## Opciones consideradas

1. **Monorepo** con `backend/` y `frontend/` y un Compose raíz — un PR puede cambiar API y UI a la vez; un solo `docker compose up`; la wiki cubre todo.
2. **Repositorios separados** — despliegues independientes, pero contratos API que se desincronizan y coordinación más costosa para un equipo pequeño.

## Decisión

Un único repositorio con `backend/` (hexagonal), `frontend/` (MVC), `wiki/` (Obsidian) y `docker-compose.yml` en la raíz. Cada aplicación tiene su `Dockerfile` multi-etapa (dev, test/build, runtime).

## Consecuencias

- Un cambio de funcionalidad completo (contrato + backend + frontend + wiki) viaja en un solo PR.
- El `.gitattributes` fuerza LF para que los contenedores Linux y el equipo en Windows convivan.
- Si en el futuro se separan despliegues, los Dockerfiles ya son independientes por carpeta.

## Relacionado

- [[arquitectura-general]] · [[entorno-docker]] · [[adr-0002-backend-hexagonal]] · [[adr-0003-frontend-mvc]]
