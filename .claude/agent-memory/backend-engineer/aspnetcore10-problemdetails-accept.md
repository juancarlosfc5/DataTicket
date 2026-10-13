---
name: aspnetcore10-problemdetails-accept
description: En ASP.NET Core 10 los ProblemDetails dependen de la cabecera Accept y del entorno (ThrowOnBadRequest); qué rutas lanzan, cuáles caen a texto plano y cómo se resolvió
metadata:
  type: reference
---

Comprobado el 2026-10-12 con pruebas de integración (runtime ASP.NET Core 10.0.5), al corregir la revisión de B3:

- `DefaultProblemDetailsWriter.CanWrite` solo acepta peticiones sin `Accept` o con `application/json`, `application/problem+json` o comodín. Con `Accept: text/html` o `text/plain`:
  - `IProblemDetailsService.WriteAsync` **lanza** `InvalidOperationException: Unable to find a registered IProblemDetailsWriter`. Desde los eventos de la cookie (`OnRedirectToLogin`/`OnRedirectToAccessDenied`) eso convertía el 401/403 en 500 vacío.
  - `TypedResults.Problem(...).ExecuteAsync(httpContext)` intenta el servicio y, si no aplica, escribe el JSON directamente como `application/problem+json` (con `type` por defecto, **sin** `traceId` salvo que se pase en `extensions`). Es la vía segura fuera de un endpoint.
  - `UseStatusCodePages` cae a `text/plain` («Status Code: 400; Bad Request»): no es 500, pero tampoco ProblemDetails.
- `RouteHandlerOptions.ThrowOnBadRequest` vale `true` solo en Development: un JSON mal formado lanza `BadHttpRequestException` y `UseExceptionHandler` lo vuelve 500; fuera de Development es 400. Con `Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = false)` es 400 `problem+json` en todos los entornos (título del framework, «Bad Request»).
- El enlace del cuerpo JSON ocurre **antes** de los filtros de endpoint: un cuerpo ilegible responde 400 sin pasar por el filtro antiforgery.

**How to apply:** al escribir ProblemDetails fuera de un `IResult` (eventos de autenticación, middleware, hub) y al añadir pruebas de errores, incluir casos con `Accept: text/html`. Ver [[integration-tests-recetas]].
