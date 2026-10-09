---
title: Persistencia en PostgreSQL
type: arquitectura
status: vigente
tags: [arquitectura/datos, postgresql, efcore]
sources: ["PRD.md §7, §8, §12, §15", "backend/src/DataTicket.Infrastructure/Persistence", "db.sql"]
aliases: [Base de datos, PostgreSQL, EF Core]
created: 2026-10-07
updated: 2026-10-09
---

# Persistencia en PostgreSQL

PostgreSQL 18.6 es la fuente de verdad de tickets, participantes, mensajes, lecturas, metadatos de adjuntos, auditoría e Identity. El acceso es por **EF Core 10 + Npgsql** desde el adaptador `Infrastructure/Persistence`.

## Estado actual

- `DataTicketDbContext` (vacío) registrado con `UseNpgsql` y cadena `ConnectionStrings:DataTicket`.
- Health check `postgresql` (`AddDbContextCheck`) expuesto en `/api/health`.
- Sin migraciones todavía.
- **Diseño de referencia aprobado en `db.sql`** (raíz, 2026-10-09). Es un paso previo de validación, no el mecanismo de implementación. La persona líder del proyecto lo revisó y lo aprobó. Se implementa con migraciones de EF Core ([[adr-0009-db-sql-diseno-de-referencia]]).

## Diseño de referencia (`db.sql`)

> [!info] Decisión
> `db.sql` describe el esquema objetivo completo del backlog, ya validado. No se ejecuta en los entornos del proyecto ni se usa para scaffolding. Cada HU crea su parte con `dotnet ef migrations add …` (ver *Convenciones*), y la revisión del PR compara la migración con el diseño. Si una HU necesita otro esquema, actualiza `db.sql` en el mismo PR. La cabecera del script todavía menciona «DB-first» y un comando de scaffolding: esas líneas son históricas y prevalece [[adr-0009-db-sql-diseno-de-referencia]].

Se validó aplicándolo en un `postgres:18.6` desechable, con pruebas de humo y de regresión de sus restricciones (ver [[log]], 2026-10-09).

| Esquema | Tablas | Origen |
|---|---|---|
| `identity` | `users` (más `display_name`, `account_status`, `company_id`), `roles`, `user_roles`, `user_claims`, `role_claims`, `user_logins`, `user_tokens`, `data_protection_keys` | [[autenticacion-identity]], HU-003/005/008 |
| `dataticket` | `companies`, `categories`, `tickets`, `priority_overrides`, `ticket_participants`, `assignments`, `formal_responses`, `chat_messages`, `message_read_receipts`, `attachments`, `notifications`, `audit_entries`, `form_templates`, `form_template_versions` | [[modelo-de-dominio]], HU de [[tablero-scrum]] |

Cómo llevarlo a EF Core en las migraciones:

- **Identity:** `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` con `ToTable("users", "identity")`, etc.
- **Nombres en snake_case:** paquete `EFCore.NamingConventions` o `HasColumnName`; la versión se verifica en NuGet al adoptarlo.
- **Restricciones:** `HasCheckConstraint`. Índices filtrados: `HasIndex(...).IsUnique().HasFilter(...)`.
- **FK compuestas:** `HasAlternateKey` en el principal y `HasForeignKey(a, b).HasPrincipalKey(a, b)`.
- **Funciones, triggers, secuencia e índices de expresión:** `migrationBuilder.Sql(...)`, siempre con su `Down`.

Decisiones del diseño:

- **Claves:** `uuid` con `uuidv7()`, nativa de PostgreSQL 18.
- **Enumeraciones:** `varchar` + `CHECK` con los valores del [[glosario]]. En EF Core se mapean con `HasConversion<string>()`.
- **Concurrencia:** columna de sistema `xmin`.
- **Prioridad calculada:** un `CHECK` contra la función `dataticket.priority_matrix` obliga a que coincida con la matriz.
- **Número del ticket:** lo genera la secuencia `ticket_number_seq` mediante `next_ticket_number()`, con formato provisional `DT-000001` (V-16). Es único, pero puede tener huecos porque una inserción fallida consume un valor.
- **Participaciones y asignaciones vigentes:** índices únicos parciales (`removed_at IS NULL`, `unassigned_at IS NULL`).
- **Auditoría y versiones de formulario:** el trigger `reject_mutation` rechaza `UPDATE`, `DELETE` y `TRUNCATE` sobre `audit_entries`, y `UPDATE` y `DELETE` sobre `form_template_versions`. Con el superusuario de Compose se puede desactivar: hay que separar los roles de base de datos antes de cualquier entorno compartido.
- **Multiempresa (invariante 1):** la FK compuesta `tickets(requester_id, company_id)` → `identity.users(id, company_id)` obliga a que el solicitante sea de la empresa del ticket. También impide mover de empresa a un usuario que ya tiene tickets.
- **Adjuntos:** el tamaño se restringe a 1–10 485 760 bytes. La columna `context` (`Submission`, `Chat`, `FormalResponse`, como en HU-014/029/033) se vincula con el mensaje o con la respuesta formal mediante FK compuestas con `ticket_id`, de modo que no puede apuntar a otro ticket.
- **Respuestas variables (Fase 4):** `jsonb` con índice GIN; propuesta pendiente del ADR de almacenamiento (HU-046).
- **Siembra de referencia (§12):** los seis roles y las tres categorías provisionales (V-12), presentes en toda instalación.
- **Datos semilla de desarrollo y pruebas (§13, DML, 2026-10-09):**
  - **Qué es:** la referencia del sembrador de Development de HU-010 y de los fixtures de pruebas. Nunca se carga en entornos compartidos.
  - **Contenido:**
    - 50 empresas sintéticas (de la 46 a la 50 inactivas);
    - 169 usuarios: los 9 internos del piloto con `@dataticket.local` (P-21) y los clientes con `@empresa-sintetica-NN.test`, en estados `Active`, `Invited` y `Deactivated`;
    - 10 tickets que cubren los seis estados, la prioridad crítica y un ajuste manual;
    - asignaciones con reasignación, un participante retirado y una toma en cobertura;
    - 8 mensajes de chat (dos con la misma marca de tiempo), lecturas, notificaciones y 5 adjuntos (solo metadatos);
    - 2 respuestas formales, una plantilla publicada y 36 entradas de auditoría.
  - **Identificadores:** deterministas por prefijo (`c0…` empresas, `a0…` internos, `b0…` clientes, `d0…` tickets…).
  - **Contraseña sintética común:** documentada en la cabecera de §13; su hash V3 de Identity se verificó con `PasswordHasher` de ASP.NET Core Identity 10.0.12.
  - **Verificación:** el script completo se aplicó sin errores en un `postgres:18.6` desechable. Se comprobaron conteos, orden estable del chat, número siguiente `DT-000011`, rechazo de `UPDATE` en auditoría y la FK compuesta multiempresa.
- **Fuera del script:** Row-Level Security, roles de base de datos separados y la lista blanca de MIME (V-06) siguen abiertos ([[pendientes]]).

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

- [[backend-hexagonal]] · [[modelo-de-dominio]] · [[entorno-docker]] · [[autenticacion-identity]] · [[pendientes]] · [[tablero-scrum]] · [[adr-0009-db-sql-diseno-de-referencia]]
