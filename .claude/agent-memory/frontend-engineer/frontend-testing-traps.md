---
name: frontend-testing-traps
description: Trampas comprobadas con React Router 8, Testing Library/jsdom, Vitest 5, TS 6 y oxlint 1.87 en el frontend de DataTicket
metadata:
  type: feedback
---

Trampas comprobadas en B5 (2026-10-09):

- **React Router 8 envuelve `navigate()` en transiciones.** Si un manejador hace `dispatch(estado)` + `navigate('/x')`, el `<Navigate>` de una guarda que reacciona al estado gana (p. ej. terminó en `/ingresar?returnUrl=…` en vez de `/ingresar`). **Cómo aplicar:** que la redirección derive del estado (p. ej. `anonymous.reason = 'signedOut'`), no de un `navigate()` paralelo.
- **Testing Library cuenta como `banner` un `<header>` dentro de `<section>`/`<main>`** en jsdom → "Found multiple elements with the role banner". Para cabeceras de página usa `<div>`; reserva `<header>` para la barra del shell.
- **Sin `globals` en Vitest, RTL no limpia solo**: hace falta `afterEach(cleanup)` (está en `src/test/domSetup.ts`, que cada `*.test.tsx` importa tras `// @vitest-environment jsdom`).
- **`vi.fn(async () => …)` sin parámetros tipa `mock.calls` como `[]`** y `tsc -b` falla en el build (Vitest no lo detecta). Declara `(_input: RequestInfo | URL, _init?: RequestInit)`.
- **oxlint 1.87 sin argumentos no imprime nada al pasar.** Para tener la línea de resumen como evidencia: `node node_modules/oxlint/bin/oxlint --format=default src` → «Found 0 warnings and 0 errors.».
- **`no-restricted-imports` de oxlint** atrapa paths (`react-router`) y patterns (`**/core/**`). Se comprobó con 12 violaciones temporales en models/controllers/views/core/shared.

**Por qué:** cada una costó una iteración roja o un build fallido. Ver [[dependency-versions]].
