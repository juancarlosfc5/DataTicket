---
title: "ADR-0002: Arquitectura hexagonal en el backend"
type: decision
status: aceptada
tags: [decision, arquitectura/backend, hexagonal]
sources: ["Decisión de la persona líder del proyecto (2026-10-07)"]
aliases: [ADR-0002]
created: 2026-10-07
updated: 2026-10-07
decision_date: 2026-10-07
deciders: [Persona líder del proyecto]
---

# ADR-0002: Arquitectura hexagonal en el backend

## Contexto

Las reglas críticas de DataTicket (aislamiento multiempresa, permisos del chat, respuesta formal, auditoría; PRD §5, §7, §12) deben poder probarse sin base de datos ni red, y el backend depende de varias tecnologías externas intercambiables (PostgreSQL, Azure Blob, SMTP, SignalR, Identity).

## Opciones consideradas

1. **Hexagonal (puertos y adaptadores)** — el núcleo no conoce la infraestructura; reglas testeables en aislamiento; adaptadores reemplazables (Azurite ↔ Azure, Mailpit ↔ proveedor real).
2. **Capas tradicionales con EF Core en los servicios** — menos ceremonia, pero las reglas quedan acopladas a la persistencia.

## Decisión

Backend hexagonal con cuatro proyectos: `Domain` → `Application` (puertos) → `Infrastructure` / `Api` (adaptadores). La regla de dependencias se verifica automáticamente con `DataTicket.ArchitectureTests`.

## Consecuencias

- Más interfaces y mapeos (entidades de dominio ≠ entidades de EF / `ApplicationUser`).
- Las reglas del PRD se prueban con dobles de los puertos.
- Romper la regla hace fallar `dotnet test` y la etapa `test` del Dockerfile.

## Relacionado

- [[backend-hexagonal]] · [[adr-0001-monorepo-contenedorizado]] · [[estrategia-de-pruebas]]
