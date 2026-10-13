---
title: Persistencia en PostgreSQL
type: arquitectura
status: vigente
tags: [arquitectura/datos, postgresql, efcore]
sources: ["PRD.md §7, §8, §12, §15", "backend/src/DataTicket.Infrastructure/Persistence", "backend/.config/dotnet-tools.json", "db.sql"]
aliases: [Base de datos, PostgreSQL, EF Core]
created: 2026-10-07
updated: 2026-10-12
---

# Persistencia en PostgreSQL

PostgreSQL 18.6 es la fuente de verdad de tickets, participantes, mensajes, lecturas, metadatos de adjuntos, auditoría e Identity. El acceso es por **EF Core 10 + Npgsql** desde el adaptador `Infrastructure/Persistence`.

## Estado actual

- `DataTicketDbContext` es un `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` que además implementa `IDataProtectionKeyContext`. Expone `Companies` y `DataProtectionKeys`. Se registra con `UseNpgsql`, `UseSnakeCaseNamingConvention` y la cadena `ConnectionStrings:DataTicket`.
- Health check `postgresql` (`AddDbContextCheck`) expuesto en `/api/health`.
- **Una migración**: `20261009164647_AddIdentityAndCompanies` (B3 del goal del login, HU-003). Crea Identity, las claves de Data Protection y `companies`; el resto del esquema de `db.sql` llega con las HU siguientes. Detalle en *Migración inicial*.
- **Diseño de referencia aprobado en `db.sql`** (raíz, 2026-10-09). Es un paso previo de validación, no el mecanismo de implementación. La persona líder del proyecto lo revisó y lo aprobó. Se implementa con migraciones de EF Core ([[adr-0009-db-sql-diseno-de-referencia]]).

## Migración inicial (`AddIdentityAndCompanies`, B3)

Implementa `db.sql` §2, §3 y los roles de §12, según [[adr-0009-db-sql-diseno-de-referencia]]. La prueba de integración `SchemaTests` comprueba tablas, nombres de restricciones e índices, valores por defecto y roles contra el diseño ([[estrategia-de-pruebas]]).

| Esquema | Tablas que crea |
|---|---|
| `identity` | `users`, `roles`, `user_roles`, `user_claims`, `role_claims`, `user_logins`, `user_tokens`, `data_protection_keys` |
| `dataticket` | `companies` |

- **Nombres.** `UseSnakeCaseNamingConvention` (paquete `EFCore.NamingConventions`, [[stack-y-versiones]]) pone tablas y columnas en snake_case. Los nombres de claves, restricciones e índices de `db.sql` (`pk_users`, `fk_users_company`, `ck_users_account_status`, `ux_users_normalized_email`…) se fijan en `Persistence/Configurations`, que se aplican **después** del modelo base de Identity.
- **Historial de migraciones.** La convención también cambia las columnas de `__EFMigrationsHistory` a `migration_id` y `product_version`; el nombre de la tabla se conserva (comprobado al generar la migración en B3). No conviene mezclarla con una base migrada sin la convención.
- **El historial vive siempre en `public`** (B7, 2026-10-12): `AddInfrastructure` fija `MigrationsHistoryTable("__EFMigrationsHistory", "public")` (`Schemas.MigrationsHistory`). Sin esquema explícito EF Core crea y busca la tabla en el primer esquema existente del `search_path` (`"$user", public`); como el usuario de PostgreSQL del Compose se llama `dataticket`, igual que el esquema de la aplicación, el primer arranque la creaba en `public` y el segundo la buscaba en `dataticket`, no la encontraba y reaplicaba la migración inicial (`42P07: relation "companies" already exists`). Lo cubre `MigrationHistoryTests` (dos arranques sobre una base con `search_path = dataticket, public`, y recuperación con un historial vacío sobrante). **Regla derivada: ningún objeto SQL de la aplicación va sin esquema explícito.** No hubo migración nueva ni cambió el snapshot.
- **Creado con SQL en la migración** (`migrationBuilder.Sql`), porque EF Core no lo modela sin cambiar su semántica:
  - valores por defecto de columnas que la aplicación siempre escribe: `email_confirmed`, `phone_number_confirmed`, `two_factor_enabled` (`false`), `lockout_enabled` (`true`), `access_failed_count` (`0`), `account_status` (`'Invited'`) y `companies.is_active` (`true`). Un booleano con `DEFAULT true` en el modelo haría que EF omitiera el valor `false` al insertar;
  - `ux_users_id_company`, `UNIQUE (id, company_id)` en `identity.users`: `company_id` admite nulos y no puede ser clave alternativa en EF. Es el destino de la futura FK compuesta de `tickets` (invariante 1);
  - `ux_companies_name_ci`, índice único sobre `lower(name)` en `dataticket.companies`.
- **Roles de referencia.** Los seis roles (`ProductManager`, `Administrator`, `Development`, `Production`, `Requester`, `CompanyCoordinator`) se siembran con `HasData`, con identificadores fijos `a8000000-0000-7000-8000-00000000000N` (N de 1 a 6) y el mismo valor como `concurrency_stamp`.
- **Estado de la cuenta.** `account_status` se guarda como texto (`HasConversion<string>()`) con un `CHECK` de los tres valores de `AccountStatus` ([[glosario]]).
- **Claves de Data Protection.** En `identity.data_protection_keys` (`PersistKeysToDbContext`); hoy sin cifrado en reposo ([[pendientes]] §4, [[autenticacion-identity]]).

> [!info] Diferencia menor con `db.sql` §12
> En `db.sql` el `id` de cada rol sale del `DEFAULT uuidv7()` y el `concurrency_stamp` de `gen_random_uuid()`: son aleatorios. La migración usa valores fijos porque `HasData` necesita un modelo estable entre migraciones. Las referencias a roles se hacen por `normalized_name`, así que el comportamiento no cambia. `db.sql` no se modificó; queda anotado en [[pendientes]] §1 para decidir si se alinea.

### Cuándo se aplican las migraciones

- **Development:** el Api las aplica al arrancar (`DatabaseMigrations.ApplyAsync`) si `Persistence:ApplyMigrationsOnStartup` es `true`, valor de `appsettings.Development.json`. `Program.cs` solo honra la clave en Development ([[entorno-docker]]).
- **Otros entornos:** no se aplican solas; se ejecutan por comando o en el despliegue. El procedimiento está por definir con la infraestructura (PRD §16.1, [[pendientes]] §2).
- **Pruebas de integración:** cada base aislada se migra con `MigrateAsync`, nunca con `EnsureCreated` ([[estrategia-de-pruebas]]).

### Sembrador de Development (B7, 2026-10-12)

`Infrastructure/Seeding/DevelopmentSeeder` corre al arrancar, después de migrar, **solo en Development y solo con `Seed:Enabled = true`** (ambas claves, `Seed:Enabled` y `Seed:DefaultPassword`, están únicamente en `appsettings.Development.json`). Fuera de Development la clave se ignora con una advertencia en el log y `SeedAsync` se niega a ejecutarse aunque se invoque directamente.

- Siembra el subconjunto mínimo de `db.sql` §13.2–13.4 con **los mismos IDs**: la Empresa Sintética 01 y cinco cuentas (Elizabeth `ProductManager` + `Production`, Laura `Development`, `ana.perez` `Requester` activa, una `Invited` sin contraseña y una `Deactivated`).
- Es idempotente: omite por id lo que ya existe sin modificarlo y repone lo que falte, en una transacción. La contraseña sintética es la de `db.sql` §13, se guarda solo como hash de Identity y no se escribe en el log.
- `DevelopmentSeedCatalog` (producción, 5 cuentas) se compara fila a fila con el catálogo de pruebas `TestAccounts` (`Catalogo_del_sembrador_coincide_con_el_de_db_sql`); la fuente de verdad sigue siendo `db.sql` §13.
- El juego completo de datos sintéticos es de [[hu-010-datos-sinteticos-de-desarrollo]], que amplía este sembrador.

## Diseño de referencia (`db.sql`)

> [!info] Decisión
> `db.sql` describe el esquema objetivo completo del backlog, ya validado. No se ejecuta en los entornos del proyecto ni se usa para scaffolding. Cada HU crea su parte con `dotnet ef migrations add …` (ver *Convenciones*), y la revisión del PR compara la migración con el diseño. Si una HU necesita otro esquema, actualiza `db.sql` en el mismo PR. La cabecera del script todavía menciona «DB-first» y un comando de scaffolding: esas líneas son históricas y prevalece [[adr-0009-db-sql-diseno-de-referencia]].

Se validó aplicándolo en un `postgres:18.6` desechable, con pruebas de humo y de regresión de sus restricciones (ver [[log]], 2026-10-09).

| Esquema | Tablas | Origen |
|---|---|---|
| `identity` | `users` (más `display_name`, `account_status`, `company_id`), `roles`, `user_roles`, `user_claims`, `role_claims`, `user_logins`, `user_tokens`, `data_protection_keys` | [[autenticacion-identity]], HU-003/005/008 |
| `dataticket` | `companies`, `categories`, `tickets`, `priority_overrides`, `ticket_participants`, `assignments`, `formal_responses`, `chat_messages`, `message_read_receipts`, `attachments`, `notifications`, `audit_entries`, `form_templates`, `form_template_versions` | [[modelo-de-dominio]], HU de [[tablero-scrum]] |

Cómo llevarlo a EF Core en las migraciones:

- **Identity:** `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` con `ToTable("users", "identity")`, etc. (hecho en B3).
- **Nombres en snake_case:** paquete `EFCore.NamingConventions` con `UseSnakeCaseNamingConvention` (adoptado en B3, [[stack-y-versiones]]).
- **Restricciones:** `HasCheckConstraint`. Índices filtrados: `HasIndex(...).IsUnique().HasFilter(...)`.
- **FK compuestas:** `HasAlternateKey` en el principal y `HasForeignKey(a, b).HasPrincipalKey(a, b)`. No sirve si el principal incluye una columna que admite nulos: es el caso de `identity.users (id, company_id)`, cuya restricción `ux_users_id_company` B3 tuvo que crear con SQL (ver *Migración inicial* y [[pendientes]] §4).
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
- Migraciones en `Persistence/Migrations`. `dotnet-ef` es una **herramienta local** fijada en `backend/.config/dotnet-tools.json` ([[stack-y-versiones]]): el manifiesto solo se descubre desde `backend/` (desde la raíz del repositorio `dotnet tool list` no la muestra), igual que `global.json`. El proyecto de arranque `DataTicket.Api` referencia `Microsoft.EntityFrameworkCore.Design` como dependencia privada, solo para estas órdenes.

  ```bash
  cd backend
  dotnet tool restore
  dotnet ef migrations add <Nombre> --project src/DataTicket.Infrastructure --startup-project src/DataTicket.Api --output-dir Persistence/Migrations
  ```

- Lo que EF Core no modela (valores por defecto que cambiarían la semántica, `UNIQUE` con columnas que admiten nulos, índices de expresión) va en `migrationBuilder.Sql(...)` dentro de la misma migración.

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
