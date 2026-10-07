---
title: "HU-002 — Shell de la aplicación y navegación por rol"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, arquitectura/frontend, producto/roles]
sources: ["PRD.md §1", "PRD.md §5", "PRD.md §10", "PRD.md §12"]
aliases: ["HU-002", "Shell de la aplicación y navegación por rol"]
epica: "[[ep-001-fundaciones-tecnicas]]"
criterios_prd: []
componentes: ["Frontend (core/http)", "Frontend (app: arranque y rutas)", "Frontend (shared/ui)", "Frontend (modules/session: models, controllers, views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 0"
dependencias: ["[[hu-004-contexto-de-usuario-y-autorizacion]]"]
relacionadas: ["[[hu-003-iniciar-y-cerrar-sesion]]", "[[hu-005-invitar-y-activar-cuentas]]", "[[hu-006-restablecer-contrasena]]", "[[hu-001-integracion-continua]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-002 — Shell de la aplicación y navegación por rol

La SPA arranca con un shell común que distingue el Portal del cliente del Espacio interno de Data Global según el usuario autenticado, protege las rutas (entrada si la API responde 401, vista «sin permiso» si responde 403) y ofrece un cliente HTTP capaz de mutar datos con cabecera antiforgery y errores ProblemDetails tipados.

## Historia de usuario

**COMO** usuario de DataTicket (cliente o integrante de Data Global)  
**QUIERO** entrar a una aplicación que me lleve a mi superficie (Portal del cliente o Espacio interno), me muestre solo la navegación que corresponde a mi perfil y me avise con claridad cuando mi sesión caducó o no tengo permiso  
**PARA** trabajar sin ver opciones ajenas a mi rol y sin quedar en pantallas rotas ante errores de autenticación o autorización

## Contexto

Hoy `frontend/src/app/App.tsx` solo muestra el estado de salud (`modules/system`) y `core/http/httpClient.ts` solo expone `getJson` (GET con `credentials: 'same-origin'`). No hay enrutador ni sesión. El PRD define dos superficies con permisos distintos (PRD §1, §10) y exige autorización en servidor (PRD §5 regla 2): el shell es una **ayuda de navegación**, nunca la barrera de seguridad. El enrutador y la librería de estado de servidor son una decisión abierta ([[pendientes]] §4, [[frontend-mvc]]); esta HU depende de ella y no nombra librerías como decididas.

## Alcance

- Arranque: al cargar, el shell consulta `GET /api/auth/me` ([[hu-004-contexto-de-usuario-y-autorizacion|HU-004]]) y decide la superficie.
- Rutas públicas (propuesta, ratificar en T-01): `/ingresar`, `/activar-cuenta`, `/olvide-mi-contrasena`, `/restablecer-contrasena`. Rutas protegidas: `/portal/**` (cliente) y `/interno/**` (Data Global). Vista `/sin-permiso`.
- Redirección a `/ingresar?returnUrl=<ruta>` cuando cualquier petición a `/api` responde 401; vista «sin permiso» cuando responde 403 o cuando un usuario abre la superficie que no le corresponde.
- Layout base en `src/shared/ui`: cabecera con nombre del producto, nombre visible del usuario y acción «Cerrar sesión» (la acción la conecta [[hu-003-iniciar-y-cerrar-sesion|HU-003]]); zona de navegación; contenido.
- Navegación por perfil calculada por una función pura en `models/` (sin JSX).
- `core/http/httpClient.ts`: `postJson`, `putJson`, `patchJson`, `deleteRequest` además de `getJson`; cabecera antiforgery en métodos que cambian estado; mapeo de respuestas `application/problem+json` (RFC 9457) a un error tipado.
- Incorporar Testing Library + jsdom para pruebas de vistas con interacción (previsto en [[estrategia-de-pruebas]]).

## Fuera de alcance

- Pantallas de entrada, activación y restablecimiento (las implementan [[hu-003-iniciar-y-cerrar-sesion|HU-003]], [[hu-005-invitar-y-activar-cuentas|HU-005]] y [[hu-006-restablecer-contrasena|HU-006]]).
- Contenido funcional de las secciones (bandejas, radicación, administración): solo marcadores de posición.
- Elegir enrutador o librería de estado de servidor (decisión abierta).
- Diseño visual definitivo: depende del ZIP de diseño (PRD §2); se usan los tokens provisionales de `styles/global.css`.
- Conexión SignalR (`core/realtime`), que llega con [[ep-009-chat-interno-en-tiempo-real]].

## Requisitos y reglas de negocio

- Portal del cliente y Espacio de trabajo de Data Global son superficies con permisos distintos (PRD §1, §10).
- Ocultar una fila u opción en la interfaz no constituye aislamiento; la autorización es del servidor (PRD §5 regla 2).
- Todo acceso exige autenticación, salvo páginas de entrada expresamente públicas (PRD §12).
- El cliente no ve estados técnicos, chat, URL de PR ni nombres de colaboradores internos (PRD §5 regla 6, §10): su navegación no ofrece secciones internas.
- Interfaz web responsiva (PRD §12).

## Invariantes en juego

- Invariante 2 (`AGENTS.md` §7): el cliente nunca ve chat interno, estados técnicos, URL de PR ni nombres internos → la superficie del portal no carga módulos ni llama endpoints internos.
- Invariante 1: el filtro multiempresa es del backend; el shell no filtra datos, solo navega.
- Reglas de arquitectura MVC (`AGENTS.md` §6): vistas sin fetch, controladores sin JSX, modelos sin React.

## Criterios del PRD cubiertos

- Ninguno de forma completa. Prepara PRD CA-02 y PRD CA-10 (superficies separadas), que se verifican en backend con [[hu-024-portal-solicitante-consulta-tickets|HU-024]] y [[hu-025-portal-coordinador-consulta-tickets|HU-025]] → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-001-fundaciones-tecnicas]]
- Dependencias: [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]] (contrato de `GET /api/auth/me`); decisión abierta «enrutador del frontend y librería de estado de servidor» ([[pendientes]] §4) para T-02.
- Relacionadas: [[hu-003-iniciar-y-cerrar-sesion|HU-003]] (vista de entrada, `GET /api/auth/antiforgery`, cierre de sesión), [[hu-005-invitar-y-activar-cuentas|HU-005]] y [[hu-006-restablecer-contrasena|HU-006]] (rutas públicas), [[hu-001-integracion-continua|HU-001]].
- Decisiones: [[adr-0003-frontend-mvc]], [[adr-0004-autenticacion-cookie-mismo-origen]] (propuesta), ADR de enrutador pendiente.

## Componentes afectados

- Frontend `src/core/http`: cliente HTTP con mutaciones, antiforgery y ProblemDetails.
- Frontend `src/app`: arranque, definición de rutas y guardas.
- Frontend `src/shared/ui`: `AppLayout`, vista «sin permiso», indicador de carga (nombres propuestos).
- Frontend `src/modules/session` (nombre de módulo propuesto): `models` (tipo `CurrentUser`, gateway de `/api/auth/me`, `navigationFor`), `controllers` (`useSessionController`), `views`.
- Dependencias de desarrollo: Testing Library + jsdom.

## Dificultad

**Nivel:** Medio

**Justificación:** Solo frontend, pero coordina varias piezas (cliente HTTP, guardas de ruta, sesión, layout, navegación por perfil) y depende de una decisión abierta (enrutador/estado) y del contrato de `/api/auth/me`. La lógica es acotada y testeable en modelos y controladores.

## Contrato backend ↔ frontend

El shell **consume** contratos definidos en HU-003 y HU-004 (propuesta REST; ratificar en T-01):

**`GET /api/auth/me`** (definido en [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]])

```json
// 200 — usuario cliente
{
  "id": "6f1c2a9e-4c1d-4b8e-9a51-1f0e2d3c4b5a",
  "displayName": "Ana Pérez",
  "email": "ana.perez@empresa-sintetica-01.test",
  "surface": "client",
  "roles": ["Requester"],
  "teams": [],
  "company": { "id": "0b7d…", "name": "Empresa Sintética 01" }
}
// 200 — usuario interno
{
  "id": "9a2e…",
  "displayName": "Elizabeth",
  "email": "elizabeth@dataticket.local",
  "surface": "internal",
  "roles": ["ProductManager"],
  "teams": ["Production"],
  "company": null
}
```

- `401` sin sesión → `application/problem+json`.

**`GET /api/auth/antiforgery`** (definido en [[hu-003-iniciar-y-cerrar-sesion|HU-003]])

```json
{ "headerName": "X-XSRF-TOKEN", "token": "CfDJ8N…" }
```

**Errores ProblemDetails (RFC 9457)** que el cliente HTTP debe mapear:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Uno o más campos no son válidos.",
  "status": 400,
  "errors": { "email": ["El correo no tiene un formato válido."] },
  "traceId": "00-4bf92f…-01"
}
```

| Código | Comportamiento del shell |
|---|---|
| 400 | El error tipado expone `title`, `detail` y `errors` por campo para que la vista los muestre |
| 401 | Limpia la sesión en memoria y navega a `/ingresar?returnUrl=<ruta actual>` |
| 403 | Muestra la vista «sin permiso» sin revelar detalles del recurso |
| 404 | El error tipado llega al controlador de la pantalla (cada HU decide el mensaje) |
| 409 / 413 / 415 | El error tipado llega al controlador con `title` y `detail` |
| 5xx o cuerpo no JSON | Mensaje genérico «No pudimos completar la operación. Inténtalo de nuevo.» |

Firma propuesta del cliente HTTP (ratificar en T-01): `getJson<T>(path, options)`, `postJson<T>(path, body, options)`, `putJson<T>`, `patchJson<T>`, `deleteRequest(path, options)`; error `HttpError` con `status` y `problem?: ProblemDetails`.

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar con HU-003/HU-004 la forma de `/api/auth/me`, `/api/auth/antiforgery`, el nombre de la cabecera antiforgery, el formato ProblemDetails y las rutas públicas/protegidas del SPA (que también usan los enlaces de correo de HU-005 y HU-006).
- [ ] **T-02 — Adoptar el enrutador decidido** · Capa: Frontend (app) · Dificultad: Medio  
  Descripción: bloqueada hasta registrar el ADR de enrutador/estado de servidor. Definir las rutas públicas y protegidas en `src/app` y una guarda que, sin sesión, navegue a `/ingresar?returnUrl=`.
- [ ] **T-03 — Cliente HTTP con mutaciones (pruebas primero)** · Capa: Frontend (core/http) · Dificultad: Medio  
  Descripción: escribir primero pruebas Vitest con `fetch` simulado: `postJson` envía `Content-Type: application/json`, `credentials: 'same-origin'` y la cabecera antiforgery; `getJson` no la envía; respuesta `application/problem+json` → `HttpError` con `problem.errors`; cuerpo vacío o no JSON → error genérico; 401 invoca el manejador global registrado; el token antiforgery se pide una vez y se invalida tras login/logout. Luego implementar.
- [ ] **T-04 — Modelo de sesión y navegación (pruebas primero)** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: tipo `CurrentUser`, normalización del JSON de `/me` (valores desconocidos de `surface` → tratar como sin acceso), gateway `sessionGateway.fetchCurrentUser()`, función pura `navigationFor(user)` y `sanitizeReturnUrl(value)` (solo rutas relativas internas que empiezan por `/` y no por `//`).
- [ ] **T-05 — Controlador de sesión** · Capa: Frontend (controllers) · Dificultad: Medio  
  Descripción: `useSessionController` con estado discriminado `loading | anonymous | authenticated | error`, acción `refresh()` y registro del manejador global de 401; pruebas Vitest del controlador.
- [ ] **T-06 — Layout y vistas compartidas** · Capa: Frontend (views, shared/ui) · Dificultad: Medio  
  Descripción: `AppLayout` (cabecera, navegación, contenido), vista «sin permiso» y estado de carga, puras y accesibles (`role="navigation"`, `role="alert"`); incorporar Testing Library + jsdom y probar el render por perfil.
- [ ] **T-07 — Cableado en `App`** · Capa: Frontend (app) · Dificultad: Bajo  
  Descripción: sustituir el arranque actual por el shell; conservar el módulo `system` accesible (p. ej. en el pie o en una ruta interna) sin romper sus pruebas.
- [ ] **T-08 — Notas para la wiki** · Capa: Documentación · Dificultad: Bajo  
  Descripción: entregar cambios para [[frontend-mvc]] (estructura `app`, `core/http`, `shared/ui`, módulo `session`) y [[pendientes]] (enrutador decidido).

## Criterios de aceptación

### CHU-01 — Cada usuario entra a su superficie

**Dado** un usuario autenticado cuyo `/api/auth/me` responde `surface: "client"` (o `"internal"`)  
**Cuando** abre la raíz `/` de la aplicación  
**Entonces** el shell lo lleva a `/portal` (o `/interno`) y muestra en la cabecera su `displayName`.

### CHU-02 — La navegación del portal no ofrece secciones internas

**Dado** un usuario con rol `Requester` o `CompanyCoordinator`  
**Cuando** se calcula `navigationFor(user)` y se renderiza el layout  
**Entonces** ningún elemento de navegación apunta a `/interno/**` ni contiene los textos «Triage», «Chat», «Administración» o «Equipo», y la prueba Vitest lo verifica para ambos perfiles.

### CHU-03 — La navegación interna depende del rol

**Dado** tres usuarios internos: uno con rol `Administrator`, uno con rol `ProductManager` y uno solo con equipo `Development`  
**Cuando** se calcula `navigationFor(user)`  
**Entonces** solo el `Administrator` ve «Administración», solo el `ProductManager` ve «Cola de triage» y el miembro de `Development` no ve ninguna de las dos; la prueba documenta que esto es ayuda de navegación y que la barrera real es el backend (PRD §5 regla 2).

### CHU-04 — Sesión caducada redirige a la entrada

**Dado** un usuario en `/interno/administracion` cuya cookie ya no es válida  
**Cuando** cualquier petición a `/api` responde 401  
**Entonces** el shell descarta la sesión en memoria y navega a `/ingresar?returnUrl=%2Finterno%2Fadministracion`; tras iniciar sesión vuelve a esa ruta.

### CHU-05 — `returnUrl` no permite redirecciones abiertas

**Dado** la URL `/ingresar?returnUrl=//evil.example/x` (o `https://evil.example`)  
**Cuando** el usuario inicia sesión  
**Entonces** `sanitizeReturnUrl` descarta el valor y el shell navega a la superficie por defecto del usuario (`/portal` o `/interno`).

### CHU-06 — 403 y superficie ajena muestran «sin permiso»

**Dado** un usuario cliente autenticado  
**Cuando** escribe a mano `/interno/administracion`, o una petición a `/api` responde 403  
**Entonces** ve la vista «sin permiso» con el texto «No tienes permiso para ver esta página.» y un enlace a su superficie, sin datos del recurso solicitado; y no se ejecuta ninguna petición a endpoints `/api/admin/**`.

### CHU-07 — Mutaciones con antiforgery y credenciales

**Dado** el cliente HTTP  
**Cuando** se invoca `postJson('/x', { a: 1 })` (o `putJson`, `patchJson`, `deleteRequest`)  
**Entonces** la petición sale a `/api/x` con `credentials: 'same-origin'`, `Content-Type: application/json` y la cabecera indicada por `headerName` de `/api/auth/antiforgery`; `getJson` no envía esa cabecera; y ninguna ruta usa `http://localhost:8080` (rutas relativas).

### CHU-08 — Errores ProblemDetails tipados

**Dado** una respuesta 400 `application/problem+json` con `errors: { "email": ["El correo no tiene un formato válido."] }`  
**Cuando** el cliente HTTP la recibe  
**Entonces** lanza `HttpError` con `status = 400` y `problem.errors.email[0]` igual a ese texto; y ante un 502 con cuerpo HTML lanza `HttpError` con `status = 502`, sin `problem`, y la vista muestra «No pudimos completar la operación. Inténtalo de nuevo.».

### CHU-09 — Fronteras MVC respetadas

**Dado** el código nuevo del shell  
**Cuando** se ejecuta `npm --prefix frontend run lint`  
**Entonces** no hay errores; ninguna vista importa `core/**` ni un `*Gateway`, ningún modelo importa `react` y ningún controlador importa vistas.

### CHU-10 — Layout responsivo

**Dado** el layout autenticado  
**Cuando** se muestra en un ancho de 360 px y en uno de 1280 px  
**Entonces** la navegación y la acción «Cerrar sesión» son accesibles sin desplazamiento horizontal (verificación manual con capturas adjuntas al PR).

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-10 validados con evidencia.
- [ ] **DoD-02** — Pruebas Vitest escritas primero y en verde, con nombres por comportamiento: `postJson envía cabecera antiforgery y credenciales same-origin`, `getJson no envía cabecera antiforgery`, `problem+json se mapea a HttpError con errores por campo`, `401 invoca el manejador global`, `navigationFor de Requester no incluye secciones internas`, `navigationFor solo muestra Administración al Administrator`, `sanitizeReturnUrl rechaza //host y URL absolutas`, `AppLayout muestra el nombre del usuario y Cerrar sesión`.
- [ ] **DoD-03** — `npm --prefix frontend run lint` sin errores (fronteras MVC), `npm --prefix frontend test` y `npm --prefix frontend run build` en verde.
- [ ] **DoD-04** — ADR de enrutador/estado de servidor `aceptada` antes de cerrar T-02 y la decisión tachada en [[pendientes]] §4.
- [ ] **DoD-05** — Contrato con HU-003/HU-004 coherente: el shell funciona contra la implementación real de `/api/auth/me` y `/api/auth/antiforgery` (verificación manual en `docker compose up --build` documentada en el PR) o contra dobles que respetan el contrato si aquellas aún no se fusionan.
- [ ] **DoD-06** — `docker compose up --build` sigue levantando; `http://localhost:5173` muestra la entrada sin errores en consola.
- [ ] **DoD-07** — Capturas de 360 px y 1280 px adjuntas al PR (CHU-10).
- [ ] **DoD-08** — Wiki actualizada vía Notas para la wiki: [[frontend-mvc]] (estructura y módulo `session`), [[estrategia-de-pruebas]] (Testing Library incorporado), [[pendientes]].
- [ ] **DoD-09** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (redirección abierta, fronteras MVC, manejo de 401/403).
- [ ] **DoD-10** — PR revisado y aprobado por otra persona del equipo; trazabilidad de HU y épica actualizada.

## Evidencia de validación

| Elemento | Resultado | Evidencia | Observación |
|---|---|---|---|
| CHU-01 | Pendiente | — | — |
| CHU-02 | Pendiente | — | — |
| CHU-03 | Pendiente | — | — |
| CHU-04 | Pendiente | — | — |
| CHU-05 | Pendiente | — | — |
| CHU-06 | Pendiente | — | — |
| CHU-07 | Pendiente | — | — |
| CHU-08 | Pendiente | — | — |
| CHU-09 | Pendiente | — | — |
| CHU-10 | Pendiente | — | — |
| DoD-01 | Pendiente | — | — |
| DoD-02 | Pendiente | — | — |
| DoD-03 | Pendiente | — | — |
| DoD-04 | Pendiente | — | — |
| DoD-05 | Pendiente | — | — |
| DoD-06 | Pendiente | — | — |
| DoD-07 | Pendiente | — | — |
| DoD-08 | Pendiente | — | — |
| DoD-09 | Pendiente | — | — |
| DoD-10 | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- Las rutas del SPA en español (`/ingresar`, `/activar-cuenta`, `/olvide-mi-contrasena`, `/restablecer-contrasena`, `/portal`, `/interno`, `/sin-permiso`) son propuesta; HU-005 y HU-006 las usan para construir enlaces de correo con `App:PublicBaseUrl`.
- El campo `surface` de `/me` es una propuesta de [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]]; si se descarta, el shell lo deriva de los roles.
- Un usuario interno que abre `/portal` también recibe «sin permiso» (propuesta): el portal no debe mostrar vistas de cliente a personal interno.
- Nombre de la cabecera antiforgery `X-XSRF-TOKEN`: propuesta de HU-003.

## Relacionado

- [[ep-001-fundaciones-tecnicas]] · [[tablero-scrum]] · [[frontend-mvc]] · [[roles-y-permisos]] · [[autenticacion-identity]] · [[adr-0003-frontend-mvc]] · [[adr-0004-autenticacion-cookie-mismo-origen]] · [[estrategia-de-pruebas]]
