---
title: Backend hexagonal
type: arquitectura
status: vigente
tags: [arquitectura/backend, hexagonal, dotnet]
sources: ["PRD.md §7, §12, §13", "backend/"]
aliases: [Backend, Puertos y adaptadores, Arquitectura hexagonal]
created: 2026-10-07
updated: 2026-10-12
---

# Backend hexagonal

El backend (`backend/`, C# sobre .NET 10) sigue **puertos y adaptadores**: el dominio y los casos de uso no conocen la web, la base de datos ni SignalR; todo lo externo entra o sale por interfaces ([[adr-0002-backend-hexagonal]]).

## Proyectos y regla de dependencias

```mermaid
flowchart TB
    API[DataTicket.Api<br/>adaptadores primarios + composition root] --> APP
    API --> INF
    INF[DataTicket.Infrastructure<br/>adaptadores secundarios] --> APP
    APP[DataTicket.Application<br/>casos de uso + puertos] --> DOM
    DOM[DataTicket.Domain<br/>entidades y reglas]
```

| Proyecto | Contiene | Puede depender de |
|---|---|---|
| `DataTicket.Domain` | Entidades, value objects, enums, invariantes ([[modelo-de-dominio]]) | Nada |
| `DataTicket.Application` | `Ports/In` (casos de uso), `Ports/Out` (repositorios, `IEmailSender`, `IFileStorage`, `ITicketChatNotifier`, `ICurrentUser`, `IClock`), implementación de casos de uso, DTOs | `Domain` + abstracciones de DI |
| `DataTicket.Infrastructure` | `Persistence/` (EF Core, configuraciones, migraciones), `Identity/`, `Storage/` (Blob), `Email/` (SMTP) | `Application`, EF Core, Npgsql, ASP.NET Core |
| `DataTicket.Api` | `Endpoints/` (minimal APIs), `Realtime/` (hub `/hubs/tickets` y notificador), `Health/`, `Program.cs` | `Application`, `Infrastructure` |

La regla se **verifica en cada `dotnet test`** (ejecutado dentro de `backend/`) con `tests/DataTicket.ArchitectureTests/LayerDependencyTests.cs`: `Domain` y `Application` no pueden referenciar `Microsoft.AspNetCore*`, `Microsoft.EntityFrameworkCore*` ni `Npgsql`; `Infrastructure` no puede referenciar `Api`.

## Mapa funcional → puertos (propuesta)

| Funcionalidad | Puerto de entrada (caso de uso) | Puertos de salida | Adaptadores |
|---|---|---|---|
| Radicación ([[flujo-del-ticket]]) | `SubmitTicket` | `ITicketRepository`, `IFileStorage`, `IAuditLog` | HTTP · EF Core · Blob |
| Triage y participantes | `AssignParticipants`, `RemoveParticipant`, `TakeTicket` | `ITicketRepository`, `IEmailSender`, `IChatConnectionRevoker` | HTTP · EF Core · SMTP · SignalR |
| Chat ([[chat-interno]]) | `SendChatMessage`, `MarkMessageRead`, `GetChatHistory` | `IChatRepository`, `ITicketChatNotifier` | Hub · EF Core · SignalR |
| Respuesta formal y cierre | `DeliverFormalResponse`, `CloseTicket` | `ITicketRepository`, `IEmailSender`, `IAuditLog` | HTTP · EF Core · SMTP |
| Portal cliente | `ListClientTickets`, `GetClientTicket` | `IClientTicketQueries` | HTTP · EF Core |
| Inicio y cierre de sesión ([[autenticacion-identity]]) — **implementado en B3** | `SignIn`, `SignOut` (puertos `ISignIn`, `ISignOut`) | `ICredentialVerifier`, `IUserSession` | HTTP · Identity |

Nombres según [[glosario]]. Es una guía de partida: se ajusta al implementar y se documenta aquí. Solo la fila de inicio y cierre de sesión existe ya en el código.

## Convenciones

- Código en inglés; mensajes y comentarios en español.
- `Directory.Build.props`: `net10.0`, `Nullable`, `ImplicitUsings`, **`TreatWarningsAsErrors`**.
- `Directory.Packages.props`: versiones centralizadas (los `.csproj` no llevan `Version`).
- `global.json`: SDK 10.0.x (`rollForward: latestFeature`) y `test.runner = Microsoft.Testing.Platform`.
- Autorización dentro de los casos de uso con `ICurrentUser`; las políticas de endpoint son la primera barrera, no la única ([[roles-y-permisos]]).
- Configuración tipada con el patrón Options. Claves esperadas: `ConnectionStrings:DataTicket`, `Email:Smtp:*`, `Email:FromAddress`, `Storage:Blob:*` ([[entorno-docker]]). Desde B3 también `Persistence:ApplyMigrationsOnStartup`, que solo se honra en Development ([[persistencia-postgresql]]).
- **Puertos de entrada**: una carpeta por área en `Application/Ports/In/<Area>` con la interfaz del caso de uso y sus tipos de entrada y resultado (p. ej. `ISignIn`, `SignInCommand`, `SignInOutcome`); la implementación va en `Application/<Area>` y se registra en `AddApplication()`.
- **Endpoints HTTP**: minimal APIs en `Api/Endpoints/<Area>`, mapeados **dentro del grupo raíz `/api`** de `Program.cs`. Ese grupo lleva el filtro `AntiforgeryValidationFilter`, que valida la cabecera antiforgery en toda mutación; un endpoint mapeado fuera del grupo no queda cubierto ([[autenticacion-identity]]).

## Cómo añadir una funcionalidad

1. Prueba de dominio/caso de uso que falle (TDD, [[estrategia-de-pruebas]]).
2. Entidad o regla en `Domain`; caso de uso y puertos en `Application`.
3. Adaptadores en `Infrastructure` (y migración si cambia el esquema, [[persistencia-postgresql]]).
4. Endpoint (dentro del grupo `/api`) o método del hub en `Api`; registro en `AddApplication()` / `AddInfrastructure()`.
5. Si se crea un proyecto nuevo: agregarlo a `DataTicket.slnx`, a la etapa `restore` del `backend/Dockerfile` y a las pruebas de arquitectura.

## Estado actual

| Elemento | Estado |
|---|---|
| Cuatro proyectos + regla de dependencias (8 pruebas: capas, `DataTicket.UnitTests` sin adaptadores ni frameworks, interfaces de `Application` solo en `Ports.In`/`Ports.Out`) | Hecho |
| `DataTicketDbContext` (Identity, `Company` y claves de Data Protection) + migración `AddIdentityAndCompanies` + health check de PostgreSQL ([[persistencia-postgresql]]) | Hecho (B3 del goal del login) |
| `GET /api/health/live` (proceso), `GET /api/health` (checks `ready`; detalle solo en Development), OpenAPI en Development | Hecho |
| Avisos de vulnerabilidad NuGet (NU1901–NU1904) visibles pero sin romper el build | Hecho |
| `TimeProvider.System` registrado en el composition root (reloj único, base del puerto `IClock`; las pruebas lo sustituyen) | Hecho (B2 del goal del login) |
| Proyecto `DataTicket.IntegrationTests` (`WebApplicationFactory<Program>` + Testcontainers); `Program` es público por generación de código de .NET 10 | Hecho (B2) |
| Dominio: `Domain/Users` (`AccountStatus`, `RoleNames`, `Surface`, `AccountAccess`), `Domain/Companies` (`Company`) y `Domain/Common` (`Timestamps`) | Hecho (B3) |
| Puertos reales: `Application/Ports/In/Authentication` (`ISignIn`, `ISignOut`) y `Application/Ports/Out` (`ICredentialVerifier`, `IUserSession`); casos de uso `SignIn` y `SignOut` en `Application/Authentication` | Hecho (B3) |
| Adaptadores de Identity en `Infrastructure/Identity` (`ApplicationUser`, `DataTicketUserManager`, `DataTicketSignInManager`, `IdentityCredentialVerifier`, `IdentityUserSession`, `IdentitySettings`) | Hecho (B3) |
| `Api/Auth` (`AuthenticationSetup`, `AntiforgeryValidationFilter`, `ApiProblems`) y `Api/Endpoints/Auth` (`AuthEndpoints`, `LoginRequest`): antiforgery, login y logout | Hecho (B3) |
| Proyecto `DataTicket.UnitTests`: referencia solo `Domain` y `Application` (dobles de los puertos, sin Docker ni base de datos) ([[estrategia-de-pruebas]]) | Hecho (B3) |
| `GET /api/auth/me`, `ICurrentUser`, fábrica de claims, políticas con nombre y de reserva | Pendiente (B4, HU-004) |
| Resto del dominio y de los casos de uso (tickets, chat, auditoría), correo, almacenamiento y SignalR | Pendiente (fase 0–2) |

## Relacionado

- [[arquitectura-general]] · [[autenticacion-identity]] · [[tiempo-real-signalr]] · [[persistencia-postgresql]] · [[stack-y-versiones]]
