---
name: dependency-versions
description: Versiones exactas de dependencias del frontend verificadas en npm (fecha, peers, engines) y compatibilidad comprobada con la imagen node:24-alpine
metadata:
  type: reference
---

Verificadas con `npm view <pkg> version|peerDependencies|engines` el 2026-10-09 (bloque B5 del goal del login), instaladas con `--save-exact`:

- `react-router` 8.4.0 (dep) — peer react/react-dom >=19.2.7, engine node >=22.22. Exporta desde `react-router` (no hace falta `/dom`): BrowserRouter, MemoryRouter, Routes, Route, Navigate, Outlet, useLocation, useNavigate, useSearchParams, useOutletContext.
- `@fontsource-variable/archivo` 5.3.0 (dep) — usar `@fontsource-variable/archivo/wdth.css` (ejes wght 100–900 + wdth 62–125); familia `'Archivo Variable'`.
- `@testing-library/react` 16.3.3, `@testing-library/dom` 10.4.2 (peer obligatorio), `@testing-library/user-event` 14.6.7, `@testing-library/jest-dom` 7.0.1 (peer vitest >=0.32, dom >=10 <11), `jsdom` 30.1.2 (engine node ^22.22.2 || ^24.15.0 || >=26).
- Vitest 5.0.3: jsdom es peer opcional `*`.
- Node local 24.18; imagen `node:24-alpine` trae 24.21 → cumple jsdom. `docker build --target build ./frontend` con el lock nuevo pasó (2026-10-09).

Pendiente: `@microsoft/signalr` 10.0.x aún no instalado (llega con EP-009). Ver [[frontend-testing-traps]].
