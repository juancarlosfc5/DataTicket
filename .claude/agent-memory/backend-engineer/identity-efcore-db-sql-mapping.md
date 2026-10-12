---
name: identity-efcore-db-sql-mapping
description: Trampas comprobadas al mapear ASP.NET Core Identity 10 + EF Core 10/Npgsql al esquema de db.sql (snake_case, passkeys, defaults, history table, Data Protection al arrancar)
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

**How to apply:** al tocar Identity, migraciones o fixtures de integración. Ver [[integration-tests-recetas]] y [[aspnetcore10-timeprovider-identity]].
