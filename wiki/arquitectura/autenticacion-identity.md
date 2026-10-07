---
title: Autenticación con ASP.NET Core Identity
type: arquitectura
status: propuesta
tags: [arquitectura/backend, seguridad, identity]
sources: ["PRD.md §5, §12", "Decisión de la persona líder (2026-10-07): usar Identity para el login"]
aliases: [Identity, Login, Autenticación]
created: 2026-10-07
updated: 2026-10-07
---

# Autenticación con ASP.NET Core Identity

El login usa **ASP.NET Core Identity** sobre PostgreSQL (decisión del equipo, 2026-10-07). El mecanismo concreto —cookie *same-origin* a través del proxy del frontend— es una **propuesta** en [[adr-0004-autenticacion-cookie-mismo-origen]] pendiente de aceptación. Aún no está implementado.

## Requisitos del PRD que condicionan el diseño

| Requisito | Fuente | Consecuencia |
|---|---|---|
| Sin registro público; Data Global crea, invita y desactiva cuentas | PRD §5 | No exponer `MapIdentityApi` completo (incluye `/register`); endpoints propios |
| Invitación y restablecimiento seguros, sin revelar si el correo existe | PRD §5, §12 | Token de un solo uso; misma respuesta exista o no la cuenta |
| Derivación segura de contraseñas estándar | PRD §12 | `PasswordHasher` de Identity |
| MFA, SSO y federación fuera del MVP | PRD §13 | No se implementan ahora |
| Autorización en servidor en cada operación | PRD §5.2 | Políticas + verificación en casos de uso |

## Diseño propuesto

- **Almacén**: `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` en `Infrastructure/Persistence`; `ApplicationUser` en `Infrastructure/Identity`. El dominio solo conoce `UserId` ([[backend-hexagonal]]).
- **Sesión**: cookie de Identity `HttpOnly`, `Secure` fuera de Development, `SameSite=Lax`; expiración deslizante. El navegador la envía a `/api` y `/hubs` porque todo es el mismo origen.
- **CSRF**: antiforgery con encabezado en endpoints que cambian estado (el SPA lo obtiene de un endpoint dedicado).
- **Claves de Data Protection** en PostgreSQL (`PersistKeysToDbContext`): las cookies sobreviven reinicios del contenedor.
- **Roles**: `ProductManager`, `Administrator`, `Development`, `Production`, `Requester`, `CompanyCoordinator`. Claim `company_id` para usuarios cliente; nunca se acepta del cuerpo de la petición ([[roles-y-permisos]]).
- **SignalR**: el hub `[Authorize]` usa la misma cookie; no hace falta token en query string ([[tiempo-real-signalr]]).

## Endpoints previstos (`/api/auth`)

| Método | Ruta | Uso |
|---|---|---|
| POST | `/login` · `/logout` | Inicio y cierre de sesión |
| GET | `/me` | Usuario actual, roles y empresa (para la UI) |
| GET | `/antiforgery` | Token CSRF para el SPA |
| POST | `/invitations` | Data Global invita a un usuario (PM/administrador) |
| POST | `/invitations/accept` | El invitado define su contraseña |
| POST | `/password/forgot` · `/password/reset` | Restablecimiento sin enumeración |

## Correo

Invitación, restablecimiento y vinculación a un ticket salen por `IEmailSender` (puerto). **Los enlaces de esos correos se construyen con una URL base configurada** (p. ej. `App:PublicBaseUrl`), nunca con el `Host` de la petición: un `Host` falsificado podría envenenar los enlaces de restablecimiento. Al desplegar detrás de un proxy, configurar además `UseForwardedHeaders` con los proxies conocidos y fijar `AllowedHosts` (hoy `*` en `appsettings.json`). En local llegan a **Mailpit** (`http://localhost:8025`); el proveedor productivo está pendiente (PRD §16.2, [[pendientes]]).

## Relacionado

- [[adr-0004-autenticacion-cookie-mismo-origen]] · [[roles-y-permisos]] · [[persistencia-postgresql]] · [[entorno-docker]]
