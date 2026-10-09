# backend/ — reglas críticas

Reglas mínimas para trabajar en el backend .NET 10. El contrato completo (arquitectura, invariantes del PRD, wiki) está en `../AGENTS.md` §6, §7 y §10; el detalle operativo, en `../.claude/agents/backend-engineer.md`. No lo dupliques aquí.

## Ejecuta `dotnet` desde `backend/`

`global.json` vive aquí: fija el SDK 10.0.x y activa Microsoft.Testing.Platform. Desde la raíz del repo, `dotnet test` cae en modo VSTest y falla.

```bash
cd backend && dotnet build                 # 0 advertencias: TreatWarningsAsErrors está activo
cd backend && dotnet test                  # incluye DataTicket.ArchitectureTests
cd backend && dotnet ef migrations add <Nombre> --project src/DataTicket.Infrastructure --startup-project src/DataTicket.Api --output-dir Persistence/Migrations
docker build --target test .               # pruebas sin SDK local
```

## Capas (las dependencias apuntan al dominio)

| Proyecto | Puede depender de |
|---|---|
| `DataTicket.Domain` | nada (ni `Microsoft.Extensions.*`) |
| `DataTicket.Application` | `Domain` + abstracciones de DI; puertos en `Ports/In` y `Ports/Out` |
| `DataTicket.Infrastructure` | `Application`, EF Core, Npgsql, Identity |
| `DataTicket.Api` | `Application`, `Infrastructure`; hub SignalR y endpoints |

- `Domain` y `Application` no usan ASP.NET Core, EF Core, Npgsql ni SignalR. `ArchitectureTests` lo verifica; nunca se desactiva.
- La autorización vive en los casos de uso, vía `ICurrentUser`, en cada operación.
- Proyecto nuevo → `DataTicket.slnx` + etapa `restore` del `Dockerfile` + `ArchitectureTests`.
- Versiones de paquetes solo en `Directory.Packages.props`, verificadas antes en NuGet.

## Prohibido

- **`MapIdentityApi`**: expone `/register` y el PRD prohíbe el registro público. Los endpoints de autenticación son propios, en `/api/auth`.
- Aceptar `companyId` del cuerpo, la consulta o las cabeceras: sale del claim `company_id`.
- Publicar por SignalR antes de persistir en PostgreSQL.
- Usar `DateTime.UtcNow`: usa `IClock`/`TimeProvider`.
- Secretos reales en `appsettings*.json`.
