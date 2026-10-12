---
name: aspnetcore10-authorization-fallback
description: Efectos comprobados de FallbackPolicy en ASP.NET Core 10 (rutas inexistentes pasan a 401), MapHealthChecks sin método, claims de user_claims/role_claims en la cookie, refresco de claims al revalidar el sello, enumerar políticas registradas y vigilar middleware sin endpoint
metadata:
  type: reference
---

Comprobado el 2026-10-12 con pruebas de integración (B4 del goal del login, HU-004; runtime ASP.NET Core 10.0.5):

- **`FallbackPolicy` también se aplica a peticiones sin endpoint.** Con `SetFallbackPolicy(RequireAuthenticatedUser)`, un anónimo que pide una ruta INEXISTENTE recibe 401, no 404 (rompió `No_existe_ruta_register`, HU-003 CHU-09). Se resolvió con `EndpointScopedAuthorizationResultHandler` (`IAuthorizationMiddlewareResultHandler`: si `context.GetEndpoint()` es null llama a `next`, si no delega en `AuthorizationMiddlewareResultHandler`). El singleton se registra antes de `AddAuthorizationBuilder()` y surte efecto.
- **`MapHealthChecks` no declara método HTTP**: acepta POST. Para limitarlo: `.WithMetadata(new HttpMethodMetadata([HttpMethods.Get]))`.
- **La fábrica de claims estándar copia `identity.user_claims` Y `identity.role_claims` a la cookie** (`UserClaimsPrincipalFactory<TUser,TRole>.GenerateClaimsAsync`, leído en `release/10.0`): una fila `company_id` cambiaba la empresa, y una fila de tipo `ClaimTypes.Role` elevaba a un usuario (o, en `role_claims`, a un equipo entero). Desde la revisión de B4, `DataTicketClaimsPrincipalFactory` implementa `IUserClaimsPrincipalFactory<ApplicationUser>` directamente (lista cerrada: id, nombre, correo, sello, `display_name`, roles de `GetRolesAsync` filtrados por `RoleNames.All`, `company_id`) y no lee esas tablas. Si se hereda de nuevo de la fábrica de Identity, el agujero vuelve.
- **Los claims se regeneran al revalidar el sello** (`SecurityStampValidator` → `CreateUserPrincipalAsync`): un `AddToRoleAsync` no cierra la sesión y llega a la cookie pasado el intervalo (5 min).
- Las minimal APIs dejan el `MethodInfo` del manejador en `endpoint.Metadata`: sirve para inspeccionar parámetros y DTO en pruebas de convención. Para saber si un endpoint lleva el filtro antiforgery se usa un metadato marcador (`AntiforgeryValidationMetadata`).
- `RequireAuthorization("Politica")` con una política no registrada solo falla (500) al pedir ese endpoint, no al arrancar.
- **`AuthorizationOptions` no expone la lista de políticas registradas**: solo `GetPolicy(nombre)`. Para enumerarlas en una prueba se lee por reflexión la propiedad privada `PolicyMap` (`Dictionary<string, Task<AuthorizationPolicy?>>`, insensible a mayúsculas); helper `RegisteredPolicies.Names` en IntegrationTests.
- **Variante «404 sin llamar a `next` cuando no hay endpoint y la autorización falla»** (probada y descartada el 2026-10-12): rompe 10 pruebas, porque los `TestEndpoints` viven en un segundo pipeline y una petición anónima solo llega a ellos si el manejador la deja pasar. Mapearlos en la tabla del Api desde el `IStartupFilter` con `app.Properties["__GlobalEndpointRouteBuilder"]` NO funciona: tras `next(app)` la clave ya no está (`KeyNotFoundException`).
- **Vigilar middleware sin endpoint sin leer el código fuente**: `System.Reflection.Metadata` (`PEReader` + `MetadataReader.MemberReferences`) lista los métodos externos a los que llama `DataTicket.Api.dll`; `UseStaticFiles` aparece como `Microsoft.AspNetCore.Builder.StaticFileExtensions.UseStaticFiles`. Funciona también en `docker build --target test`. No detecta un `app.Use(...)` en línea.

**How to apply:** al añadir middleware que sirva contenido sin endpoint (archivos estáticos) recordar que la política de reserva NO lo cubre con el manejador actual; al mapear el hub o endpoints nuevos, colgarlos del grupo `/api` o darles `AllowAnonymous` explícito y actualizar la lista de la prueba. Ver [[integration-tests-recetas]] y [[aspnetcore10-problemdetails-accept]].
