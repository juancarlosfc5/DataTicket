---
title: Estrategia de pruebas
type: guia
status: vigente
tags: [proceso, pruebas]
sources: ["PRD.md §14", "backend/tests", "frontend/src/**/*.test.ts"]
aliases: [Pruebas, Testing, TDD]
created: 2026-10-07
updated: 2026-10-07
---

# Estrategia de pruebas

Desarrollo guiado por pruebas: primero la prueba que falla, luego la implementación mínima, luego la refactorización. Objetivo de cobertura: 80 % en dominio, casos de uso y modelos/controladores del frontend. Cada criterio de aceptación del PRD debe acabar respaldado por al menos una prueba automática ([[criterios-de-aceptacion]]).

## Backend (.NET 10)

| Nivel | Herramientas | Qué cubre |
|---|---|---|
| Arquitectura | `DataTicket.ArchitectureTests` (xUnit v3) | Regla de dependencias hexagonal (5 pruebas, en verde) |
| Unitarias | xUnit v3 + dobles de los puertos | Invariantes de dominio y casos de uso, incluida la autorización negativa |
| Integración | `WebApplicationFactory<Program>` + PostgreSQL real (Compose o Testcontainers) | Endpoints, EF Core, filtros multiempresa (CA-01), hub SignalR |

- Runner: **Microsoft.Testing.Platform**, activado en `backend/global.json` (`test.runner`). Por eso `dotnet` se ejecuta **dentro de `backend/`**: `cd backend && dotnet test`. Desde la raíz no se encuentra `global.json`, el SDK cae en modo VSTest y el comando falla (comprobado el 2026-10-07).
- Cobertura: `cd backend && dotnet test --coverage --coverage-output-format cobertura` (extensión `Microsoft.Testing.Extensions.CodeCoverage`; reporte en `backend/TestResults/`, ignorado por git).
- Sin SDK local: `docker build --target test ./backend`.

## Frontend (React 19)

| Nivel | Herramientas | Qué cubre |
|---|---|---|
| Estático | `oxlint` (incluye fronteras MVC) + `tsc -b` | Tipos y capas |
| Unitarias | Vitest | Modelos (normalización, reglas) y controladores |
| Componentes | Testing Library + jsdom (por añadir) | Vistas con interacción |
| E2E | Playwright (por añadir) | Flujos críticos: radicar, triage, chat, respuesta formal |

## Pruebas imprescindibles por invariante

| Invariante | Prueba |
|---|---|
| Aislamiento multiempresa (CA-01, CA-02) | Integración: usuario de empresa A recibe 404/403 sobre ticket de B |
| Chat solo para participantes (CA-06, CA-08) | Integración del hub: no participante no se une ni envía; retirado pierde la conexión |
| Persistir antes de publicar (CA-05) | Unitaria del caso de uso: el notificador no se llama si falla la persistencia |
| Respuesta formal y cierre solo PM/admin (CA-11, CA-12) | Unitaria + integración por rol |
| Adjuntos (CA-09) | Rechazo de tipo/tamaño/firma inválidos en backend |
| Auditoría (CA-14) | Cada caso de uso relevante deja `AuditEntry` con actor y fecha |

## Relacionado

- [[backend-hexagonal]] · [[frontend-mvc]] · [[criterios-de-aceptacion]] · [[flujo-de-trabajo-github]]
