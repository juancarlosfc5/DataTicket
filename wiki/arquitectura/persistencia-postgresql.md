---
title: Persistencia en PostgreSQL
type: arquitectura
status: vigente
tags: [arquitectura/datos, postgresql, efcore]
sources: ["PRD.md §7, §8, §12, §15", "backend/src/DataTicket.Infrastructure/Persistence"]
aliases: [Base de datos, PostgreSQL, EF Core]
created: 2026-10-07
updated: 2026-10-07
---

# Persistencia en PostgreSQL

PostgreSQL 18.6 es la fuente de verdad de tickets, participantes, mensajes, lecturas, metadatos de adjuntos, auditoría e Identity. El acceso es por **EF Core 10 + Npgsql** desde el adaptador `Infrastructure/Persistence`.

## Estado actual

- `DataTicketDbContext` (vacío) registrado con `UseNpgsql` y cadena `ConnectionStrings:DataTicket`.
- Health check `postgresql` (`AddDbContextCheck`) expuesto en `/api/health`.
- Sin migraciones todavía.

## Convenciones

- Configuración por entidad con `IEntityTypeConfiguration<T>` en `Persistence/Configurations` (se aplican con `ApplyConfigurationsFromAssembly`).
- Migraciones en `Persistence/Migrations`:

  ```bash
  dotnet ef migrations add <Nombre> --project backend/src/DataTicket.Infrastructure --startup-project backend/src/DataTicket.Api --output-dir Persistence/Migrations
  ```

- Fechas en UTC (`timestamptz`); la presentación y los cálculos de tiempos usan `America/Bogota` ([[dashboard-y-metricas]]).
- Binarios de adjuntos **fuera** de la base: solo metadatos y referencia (PRD §8, [[archivos-adjuntos]]).
- Claves de Data Protection de Identity en una tabla propia ([[autenticacion-identity]]).

## Aislamiento multiempresa

- Filtros globales de EF Core por `CompanyId` en las consultas del portal cliente, más pruebas de integración que demuestren que la empresa A no ve la B (CA-01).
- El PRD recomienda **evaluar Row-Level Security** de PostgreSQL como defensa adicional (PRD §12): decisión abierta en [[pendientes]].

## Auditoría y chat

- `AuditEntry` es append-only desde la aplicación (PRD §12). Considerar además revocar `UPDATE`/`DELETE` a nivel de base sobre esa tabla ([[auditoria]]).
- Cada mensaje se guarda **antes** de publicarse por SignalR; las confirmaciones de lectura se persisten por mensaje y persona ([[tiempo-real-signalr]]).

## Entorno local

Servicio `db` de Compose (`postgres:18.6`), volumen `postgres_data` montado en `/var/lib/postgresql` (requisito de la imagen 18). Desde el host: `localhost:5432`; desde contenedores: `db:5432`. `docker compose down -v` borra los datos ([[entorno-docker]]).

## Relacionado

- [[backend-hexagonal]] · [[modelo-de-dominio]] · [[entorno-docker]]
