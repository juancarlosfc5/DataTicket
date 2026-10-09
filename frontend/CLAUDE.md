# frontend/ — reglas críticas

Reglas mínimas para trabajar en el SPA (React 19 + TypeScript, Vite, Node 24). El contrato completo está en `../AGENTS.md` §6 y §10; el detalle operativo, en `../.claude/agents/frontend-engineer.md`. No lo dupliques aquí.

## Comandos

```bash
npm --prefix frontend run lint      # oxlint: incluye las fronteras MVC
npm --prefix frontend test          # vitest run (node; las vistas: // @vitest-environment jsdom)
npm --prefix frontend run build     # tsc -b + vite build
docker compose up --build --watch   # stack completo; app en http://localhost:5173
```

## MVC por módulo (`src/modules/<modulo>/`)

| Carpeta | Regla |
|---|---|
| `models/` | TypeScript puro, sin React: tipos, validación de todo JSON externo, gateways (`xxxGateway.ts`), reductores |
| `controllers/` | Hooks `useXController` en `.ts`, sin JSX ni vistas; estado discriminado (`kind`) |
| `views/` | `.tsx` puros: props → JSX. Sin `fetch`, gateways, `core/` ni `@microsoft/signalr` |
| `src/app/` | Páginas y rutas que conectan controlador y vista |

- `src/core/http` (cliente same-origin, antiforgery, `HttpError`) y `src/core/realtime` (única `HubConnection`) solo se usan desde modelos y controladores.
- Ejemplo vivo: `src/modules/system`.
- `.oxlintrc.json` hace cumplir las fronteras. **Nunca desactives una regla**: corrige la dependencia.

## Prohibido

- Tokens o datos de sesión en `localStorage`/`sessionStorage`: la sesión es la cookie `HttpOnly` del backend.
- URLs absolutas al backend (`http://localhost:8080`): usa `/api` y `/hubs`.
- Que el portal del cliente importe el módulo `chat` o muestre datos internos.
- `dangerouslySetInnerHTML`.
- Instalar otro enrutador que no sea `react-router` 8.4.0 (ADR-0010) o usar sus `loader`/`action` para pedir datos.
- Copiar pantallas de `DataTicket.html` o usar colores sueltos: el estilo sale de `wiki/arquitectura/sistema-de-diseno.md` (ADR-0011).
- Dependencias sin verificar su versión en npm (`--save-exact`); `package-lock.json` siempre regenerado.
