---
title: "ADR-0010: React Router como enrutador del frontend"
type: decision
status: aceptada
tags: [decision, arquitectura/frontend, react]
sources: ["Decisión D2 de la persona líder del proyecto (2026-10-09)", "npm: react-router@8.4.0 (dist-tag latest, publicada el 2026-09-15)"]
aliases: [ADR-0010, Enrutador del frontend, React Router]
created: 2026-10-09
updated: 2026-10-09
decision_date: 2026-10-09
deciders: [juancarlosfc5 (persona líder del proyecto)]
---

# ADR-0010: React Router como enrutador del frontend

El SPA usa **React Router** (paquete `react-router`, versión exacta `8.4.0`) en **modo librería**: rutas declaradas en `src/app` sin el modo *framework*. No se adopta por ahora una librería de estado de servidor.

## Contexto

- HU-002 necesita rutas públicas (`/ingresar`, `/activar-cuenta`, `/olvide-mi-contrasena`, `/restablecer-contrasena`), superficies protegidas (`/portal/**`, `/interno/**`), una vista `/sin-permiso` y redirección con `returnUrl` ([[hu-002-shell-y-navegacion-por-rol]]).
- Estaba abierta en [[pendientes]] §4 y bloqueaba la tarea T-02 de HU-002 y el login ([[hu-003-iniciar-y-cerrar-sesion]]).
- El frontend sigue MVC ([[adr-0003-frontend-mvc]]): los datos los cargan los controladores-hook a través de los gateways, no el enrutador.

## Opciones consideradas

1. **React Router 8 en modo librería**: es el estándar de facto, tiene rutas anidadas y guardas, y no impone un servidor. Verificado en npm el 2026-10-09: `latest` = 8.4.0 (publicada hace más de dos semanas), peer `react >=19.2.7` (el proyecto usa 19.3) y engine `node >=22.22` (el proyecto usa Node 24).
2. **React Router en modo *framework*** (antes Remix): trae SSR y convenciones de archivos que el proyecto no necesita, y mezcla la carga de datos con las rutas, contra la separación MVC.
3. **TanStack Router**: rutas tipadas, pero menos conocido por el equipo; no aporta lo suficiente para el MVP.
4. **Enrutador propio sobre `history`**: YAGNI y más superficie de errores (redirecciones, `returnUrl`).

## Decisión

Opción 1, con estas reglas:

- Dependencia `react-router@8.4.0` instalada con `--save-exact`. El paquete `react-router-dom` no se usa: desde la v7 basta `react-router`.
- Las rutas y las guardas viven en `src/app` (único lugar donde se juntan controlador y vista). Los `loader`/`action` del enrutador **no** se usan para pedir datos: lo hacen los controladores (`useXController`) con sus gateways. Así `models/` y `views/` siguen sin conocer el enrutador, salvo los tipos.
- El `returnUrl` se sanea en `models/` con una función pura (solo rutas internas relativas).
- **Librería de estado de servidor** (p. ej. TanStack Query): no se adopta ahora. Se reabre si aparecen cachés compartidas entre pantallas (bandejas, dashboard).

## Consecuencias

- HU-002 T-02 queda desbloqueada. El agente `frontend-engineer` ya no tiene que detenerse por falta de ADR del enrutador.
- Las guardas de ruta son UX. La autorización real sigue en el backend (invariantes 1, 2 y 3 de `AGENTS.md` §7).
- Antes de cablear las rutas, verifica la API vigente en la documentación oficial de React Router 8, porque cambió respecto de la v6 (`createBrowserRouter` + `RouterProvider`).

## Relacionado

- [[frontend-mvc]] · [[stack-y-versiones]] · [[hu-002-shell-y-navegacion-por-rol]] · [[pendientes]] · [[plan-goal-login-y-loop-chat]]
