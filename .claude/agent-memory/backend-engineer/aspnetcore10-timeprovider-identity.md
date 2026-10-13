---
name: aspnetcore10-timeprovider-identity
description: Qué componentes de ASP.NET Core 10 (Identity, cookie, Data Protection) respetan TimeProvider de DI y cuáles usan el reloj real; clave para probar bloqueo/expiración con FakeTimeProvider
metadata:
  type: reference
---

Verificado el 2026-10-09 leyendo el código de `dotnet/aspnetcore` rama `release/10.0` (raw.githubusercontent.com).

Respetan el `TimeProvider` registrado en DI (si se sustituye por `FakeTimeProvider` con `RemoveAll<TimeProvider>()` + `AddSingleton`):
- Cookie (`CookieAuthenticationHandler`): `AuthenticationBuilder.AddSchemeHelper` registra un post-configure que pone `options.TimeProvider ??= <TimeProvider de DI>`; `AddAuthentication` hace `TryAddSingleton(TimeProvider.System)`. Expiración, renovación deslizante e `IssuedUtc` siguen el reloj falso.
- `SecurityStampValidator`: `AddSignInManager` (y `AddIdentity`) registran `PostConfigureSecurityStampValidatorOptions` que toma el `TimeProvider` de DI. El intervalo de revalidación sigue el reloj falso.

NO lo respetan (usan el reloj real):
- `UserManager.IsLockedOutAsync` y `AccessFailedAsync`: `DateTimeOffset.UtcNow` fijo (bloqueo P-18). Ambos son `public virtual`: se pueden sobrescribir en un `UserManager` derivado que use `TimeProvider`, o en la prueba retroceder `LockoutEnd` en la base.
- `DataProtectorTokenProvider` (tokens de invitación/restablecimiento, HU-005/006): `DateTimeOffset.UtcNow`.
- Data Protection (`XmlKeyManager`, `KeyRingProvider`): reloj real; no interfiere con un reloj falso.

**How to apply:** el bloqueo ya está resuelto con `DataTicketUserManager` (override con TimeProvider, B3 2026-10-09); los tokens de HU-005/006 siguen con reloj real. Revisa de nuevo si sube la versión mayor de ASP.NET Core. Ver [[integration-tests-recetas]].
