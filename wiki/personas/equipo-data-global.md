---
title: Equipo de Data Global
type: entidad
status: vigente
tags: [personas]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §6.3", "PRD.md §10", "AGENTS.md §8"]
aliases: [Equipos internos, Desarrollo y Producción, Equipo Data Global]
created: 2026-10-07
updated: 2026-10-07
---

# Equipo de Data Global

Personas internas que usarán DataTicket en el piloto, organizadas en los equipos Desarrollo y Producción. Es la **lista inicial del piloto**; los usuarios y asignaciones futuras se administran dentro de DataTicket (PRD §5).

## Integrantes del piloto (PRD §5)

| Persona | Equipo | Código | Papel en el producto |
|---|---|---|---|
| Juan David | Desarrollo | `Team.Development` | Trabaja tickets asociados |
| Laura | Desarrollo | `Team.Development` | Trabaja tickets asociados |
| Brayan | Desarrollo | `Team.Development` | Trabaja tickets asociados |
| Cristian | Desarrollo | `Team.Development` | Trabaja tickets asociados |
| Kevin | Desarrollo | `Team.Development` | Trabaja tickets asociados |
| Julián | Producción | `Team.Production` | Líder de Producción; ver [[julian-produccion]] |
| Elizabeth | Producción | `Team.Production` | Además es la PM (`ProductManager`); ver [[elizabeth-pm]] |
| Gerardo | Producción | `Team.Production` | Integrante de Producción |

## Qué hace cada equipo

| Equipo | Responsabilidad (PRD §5, §6.3) |
|---|---|
| Desarrollo | Análisis e implementación de los tickets asociados a sus integrantes; coordinación en el [[chat-interno]]; si genera un PR, una persona autorizada registra su URL a mano |
| Producción | Casos que corresponden a Producción: revisión de PR y puesta en producción, liderados por Julián |

## Pertenecer a un equipo no da acceso

- El acceso al detalle y al chat de un ticket es por **asociación explícita** como participante, además de la pertenencia al equipo (PRD §6.2, §10).
- La asignación a un equipo no sustituye la lista de participantes (PRD §6.2).
- Única excepción: Elizabeth, que siempre accede para triage y seguimiento (PRD §5).
- Cada equipo trabaja desde su bandeja por equipo y responsabilidad (PRD §10). Ver [[dashboard-y-metricas]].

## Doble papel: usuarios y constructores

> [!info] Nota
> **Laura, Juan David y Brayan** son, a la vez, integrantes de Desarrollo que usarán DataTicket y desarrolladores que construyen DataTicket en este repositorio (`AGENTS.md` §8). Ver [[equipo-del-repositorio]].

## Lo que el PRD no dice

- No nombra a las personas con rol `Administrator` en el piloto.
- No detalla las responsabilidades de Gerardo más allá de pertenecer a Producción.

Ver [[pendientes]].

## Relacionado

- [[elizabeth-pm]] · [[julian-produccion]] · [[equipo-del-repositorio]]
- [[roles-y-permisos]] · [[flujo-del-ticket]] · [[chat-interno]] · [[glosario]]
