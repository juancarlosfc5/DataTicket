# Stack y restricciones de DataTicket

Fuente viva: `wiki/arquitectura/stack-y-versiones.md` y los ADR de `wiki/decisiones/`. Si esta referencia y la wiki difieren, manda la wiki.

## Monorepo y entorno

- Monorepo contenedorizado con Docker Compose: servicios `db` (PostgreSQL 18), `backend`, `frontend`, `mailpit` (correo local) y `azurite` (Blob local) (ADR-0001, ADR-0007).
- El stack debe seguir levantando con `docker compose up --build`. Un proyecto .NET nuevo exige añadir su `.csproj` a la etapa `restore` de `backend/Dockerfile`.

## Backend

- C# / .NET 10 (LTS), ASP.NET Core, solución `backend/DataTicket.slnx`.
- Arquitectura **hexagonal** (ADR-0002): `Domain` sin dependencias; `Application` con casos de uso y puertos en `Application/Ports/In` y `Application/Ports/Out`; adaptadores secundarios (EF Core, Identity, Blob, SMTP) en `Infrastructure`; adaptadores primarios (endpoints HTTP, hub SignalR) en `Api`.
- `DataTicket.ArchitectureTests` verifica la regla de dependencias; nunca se desactiva.
- **ASP.NET Core Identity** con cookie de mismo origen a través del proxy (ADR-0004, aún por aceptar). Sin registro público: no se expone `MapIdentityApi` completo. La recuperación de contraseña no revela si la cuenta existe.
- **SignalR** (ADR-0005): hub autenticado `/hubs/tickets`; se valida participación en cada unión y operación; se persiste antes de publicar; se revocan conexiones de participantes retirados; recuperación por cursor tras reconexión.
- Pruebas: xUnit v3 sobre Microsoft.Testing.Platform; integración con `WebApplicationFactory<Program>` y PostgreSQL real.

Las tareas pueden cubrir entidades y value objects, casos de uso, puertos, adaptadores, endpoints, métodos/eventos del hub, migraciones y pruebas. No prescribas nombres de clases todavía inexistentes ni estructura no aprobada.

## Frontend

- React 19 + TypeScript, Vite, Node 24, cliente `@microsoft/signalr`.
- Arquitectura **MVC** (ADR-0003): `src/modules/<modulo>/{models,controllers,views}` + `src/core` (http, tiempo real) + `src/shared` (UI genérica) + `src/app` (arranque y rutas).
  - Modelos: tipos, validación y gateways de API/SignalR; sin React.
  - Controladores: hooks `useXController`; sin JSX.
  - Vistas: componentes puros; sin fetch ni SignalR.
- `oxlint` hace cumplir esas fronteras; pruebas con Vitest. Testing Library y Playwright están previstos pero aún no incorporados.
- Enrutador y librería de estado de servidor: **decisión abierta**; no los nombres en tareas.

## Persistencia y archivos

- PostgreSQL 18 con EF Core + Npgsql es la fuente de verdad, incluido el historial de chat, las lecturas y la auditoría.
- Un cambio de esquema exige una migración EF Core.
- Adjuntos en Azure Blob Storage (Azurite en local); URL públicas según ADR-0006, con riesgo pendiente de aprobación.
- Adjuntos permitidos: PDF, imágenes, XML y Excel, máx. 10 MB, validados en backend.
- Row-Level Security: decisión abierta; el filtro multiempresa en backend es obligatorio igualmente.

## Invariantes que toda HU respeta (AGENTS.md §7)

1. El cliente solo ve tickets de su empresa (solicitante: los suyos; coordinador: los de su empresa), filtrado en backend.
2. El cliente nunca ve chat interno, estados técnicos, URL de PR ni nombres de colaboradores internos.
3. Solo participantes vigentes leen o escriben el chat; Elizabeth siempre puede. Los grupos de SignalR no son seguridad.
4. Cada mensaje se persiste en PostgreSQL antes de publicarse.
5. Un administrador no ve contenido por serlo; al tomar un ticket queda asociado como participante antes de cargar el detalle.
6. Solo Elizabeth o un administrador emiten la respuesta formal y cierran manualmente; no hay cierre automático.
7. Adjuntos validados en backend (tipo y tamaño).
8. Auditoría append-only con actor, fecha, acción, objeto y valores anterior/nuevo.
9. Sin registro público; recuperación de contraseña sin enumeración de cuentas.

Cuando el riesgo de una HU lo justifique, contempla cookies y encabezados HTTP (CSP, X-Content-Type-Options, Referrer-Policy, cache headers, `SameSite`). Formula criterios por resultado de seguridad, no por una configuración arbitraria.

## Fuera del alcance del MVP (PRD §13)

MFA/SSO, integración con GitHub/GitLab, formularios personalizados durante el piloto (van en la fase 4), correo entrante, automatizaciones/SLA/recordatorios, base de conocimiento, chat visible para el cliente, app móvil y Redis/backplane multiinstancia.

## Decisiones abiertas

Consulta `wiki/pendientes.md` §4 antes de planificar: no las resuelvas dentro de una HU; regístralas como dependencia o riesgo.
