---
name: identity-efcore-db-sql-mapping
description: Trampas comprobadas al mapear Identity 10 y EF Core 10/Npgsql al esquema de db.sql (snake_case, passkeys, defaults, history table, claves de hijos de agregado, índices de FK, catálogo y search_path, SQLSTATE de RESTRICT)
metadata:
  type: project
---

Comprobado el 2026-10-09 (B3 del goal del login, migración `AddIdentityAndCompanies`):

- `IdentityUserContext` en .NET 10 elige el modelo por `StoreOptions.SchemaVersion` leído del proveedor de servicios de la app; por defecto (0.0) cae en Version1 (sin tabla de passkeys, `phone_number` text, claves de login sin longitud). Se fija `Stores.SchemaVersion = Version1` y las longitudes 128 de `db.sql` a mano.
- `EFCore.NamingConventions` 10.0.1 (`UseSnakeCaseNamingConvention`) también pone en snake_case las columnas de `__EFMigrationsHistory` (`migration_id`, `product_version`); el nombre de tabla se mantiene. Funciona; no mezclar con bases migradas sin la convención.
- La convención respeta los nombres explícitos de Identity (`UserNameIndex`, `EmailIndex`) salvo que se renombren después de `base.OnModelCreating`: aplicar las configuraciones DESPUÉS del base.
- DEFAULT `true` en bool (lockout_enabled, is_active) no se modela en EF (omitiría `false` al insertar): va en `migrationBuilder.Sql` junto con `ux_users_id_company` (UNIQUE con columna nullable, no admite clave alternativa) y el índice de expresión `lower(name)`.
- Data Protection lee `identity.data_protection_keys` al arrancar el host: si una fixture arranca el host y migra después, aparecen errores 42P01 en el log (se recupera). Migrar al arrancar (Development) o antes de crear el host.
- `dotnet new tool-manifest` en SDK 10 crea `dotnet-tools.json` en el directorio actual, no en `.config/`; se movió a `backend/.config/` (ambas rutas se descubren).
- `dotnet ef migrations script --no-build` justo tras `migrations add` usa el ensamblado viejo (sale solo la tabla de historial): compilar antes.
- Los logs del host solo se ven en la salida de MTP cuando la prueba falla; para inspeccionarlos, una prueba sonda temporal con `Assert.Fail`.

Comprobado el 2026-10-12 (B7, fallo de arranque 42P07 en el Compose):

- **`__EFMigrationsHistory` sin esquema explícito depende del `search_path`**: EF la crea y la busca en el primer esquema EXISTENTE. Con `search_path = "$user", public` y un usuario de PostgreSQL llamado igual que un esquema de la app (Compose: usuario y esquema `dataticket`), el primer arranque deja el historial en `public` (el esquema aún no existe) y el segundo lo busca en `dataticket`, no lo ve, crea otro vacío y reaplica la migración inicial → `42P07 relation "companies" already exists`. Arreglo: `UseNpgsql(cs, o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "public"))`; no cambia el modelo ni el snapshot (`dotnet ef migrations has-pending-model-changes` limpio) y el script generado califica `public."__EFMigrationsHistory"`.
- Testcontainers usa por defecto el usuario `postgres`, que no reproduce esa condición: la fixture ahora arranca con `WithUsername("dataticket").WithDatabase("dataticket")`, como el Compose (sin el arreglo, 74 de 124 pruebas de integración fallaban). Para reproducirlo sin depender del usuario: `ALTER DATABASE … SET search_path = dataticket, public` por la conexión de administración ANTES de abrir sesiones contra la base (solo rige para sesiones nuevas; el pool de Npgsql es por cadena de conexión).
- `dotnet ef … --no-build` funciona con el manifiesto de `backend/.config` tras `dotnet tool restore`; `has-pending-model-changes` no conecta a la base.

**How to apply:** al tocar Identity, migraciones o fixtures de integración; cualquier objeto SQL sin esquema explícito es sospechoso en el Compose. Ver [[integration-tests-recetas]] y [[aspnetcore10-timeprovider-identity]].

Comprobado el 2026-10-14 (núcleo N1, migración `HuN1_NucleoTickets`):

- **Hijos de un agregado con `HasDefaultValueSql("uuidv7()")` en la clave**: EF marca la clave como generada y, al descubrir por la navegación una fila nueva con Id ya asignado, la trataría como existente (UPDATE). Se añade `.ValueGeneratedNever()` DESPUÉS de `HasDefaultValueSql`: el DEFAULT sigue en la migración y la fila nueva se inserta (lo prueba `Participante_agregado_tarde_accede_al_ticket`).
- **Índices de FK**: EF NO crea índice propio si otro índice empieza por las columnas de la FK (incluso uno filtrado, p. ej. `ix_assignments_assignee_active`). Las FK sin índice en db.sql (`added_by`, `closed_by`…) generarían índices extra: van con `migrationBuilder.Sql(ALTER TABLE … ADD CONSTRAINT …)`.
- `UNIQUE (number)` de db.sql es restricción, no índice: `HasAlternateKey(...).HasName("ux_…")` (con `HasIndex().IsUnique()` sale como índice único).
- Un CHECK o DEFAULT que llama a una función de la migración: el `migrationBuilder.Sql(CREATE FUNCTION …)` va al PRINCIPIO de `Up`, antes del `CreateTable` que la usa.
- **`column_default` y `pg_get_constraintdef` omiten el esquema si está en el `search_path`** (usuario `dataticket` → `"$user"`): salen `next_ticket_number()` y `priority_matrix(…)` sin calificar. En las pruebas de catálogo se abre la conexión con `SearchPath = "public"` (`NpgsqlConnectionStringBuilder`).
- `DELETE` bloqueado por una FK `ON DELETE RESTRICT` da SQLSTATE **23001** (`RestrictViolation`), no 23503.
