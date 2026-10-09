---
title: "HU-004 — Contexto del usuario y autorización por rol"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, seguridad, identity, producto/roles, arquitectura/backend]
sources: ["PRD.md §5", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-004", "Contexto del usuario y autorización por rol"]
epica: "[[ep-002-identidad-y-acceso]]"
criterios_prd: ["CA-01"]
componentes: ["Identity", "Backend (Application: ICurrentUser)", "Backend (Api: políticas, endpoint /me)", "Frontend (models: contrato de /me)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 0"
dependencias: ["[[hu-003-iniciar-y-cerrar-sesion]]"]
relacionadas: ["[[hu-002-shell-y-navegacion-por-rol]]", "[[hu-008-administrar-usuarios-cliente]]", "[[hu-009-administrar-usuarios-internos-y-equipos]]", "[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-025-portal-coordinador-consulta-tickets]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-004 — Contexto del usuario y autorización por rol

El backend expone el contexto del usuario autenticado (`GET /api/auth/me`), lo pone a disposición de los casos de uso mediante el puerto `ICurrentUser`, define políticas por rol y exige autenticación en todos los endpoints salvo una lista explícita de públicos. La empresa de un usuario cliente sale siempre del claim `company_id` de la sesión, nunca de la petición.

## Historia de usuario

**COMO** usuario autenticado de DataTicket  
**QUIERO** que la aplicación conozca mi identidad, mis roles, mis equipos y, si soy cliente, mi empresa, y que el servidor decida con eso qué puedo hacer en cada operación  
**PARA** ver y hacer solo lo que me corresponde, sin depender de lo que muestre u oculte la interfaz

## Contexto

Tras [[hu-003-iniciar-y-cerrar-sesion|HU-003]] existe la cookie de sesión, pero ningún endpoint la usa para autorizar. El PRD exige autorización en servidor en cada operación (PRD §5 regla 2) y autenticación en todo acceso salvo páginas de entrada públicas (PRD §12). [[autenticacion-identity]] prevé roles `ProductManager`, `Administrator`, `Development`, `Production`, `Requester`, `CompanyCoordinator` y el claim `company_id`; [[backend-hexagonal]] prevé el puerto `ICurrentUser` para autorizar dentro de los casos de uso. El shell de [[hu-002-shell-y-navegacion-por-rol|HU-002]] consume `/me` para elegir la superficie.

## Alcance

- `GET /api/auth/me` con id, nombre visible, correo, superficie, roles, equipos y empresa (solo cliente).
- Fábrica de claims que añade roles, equipos y `company_id` (solo cliente) al principal.
- Puerto `ICurrentUser` en `Application/Ports/Out` (id, autenticado, roles, equipos, `CompanyId?`, `IsInRole`) y adaptador en `Api` que lo lee de los claims.
- Políticas de autorización con nombre (propuestas): `ClientUser`, `InternalUser`, `Administrator`, `ProductManagerOrAdministrator`.
- Política de reserva (*fallback*) que exige usuario autenticado; lista explícita de endpoints públicos con `AllowAnonymous`.
- Prueba que recorre todos los endpoints registrados y falla si alguno fuera de la lista admite acceso anónimo.
- Prueba de convención: ningún DTO de entrada de un endpoint de cliente declara `CompanyId` (propuesta).

## Fuera de alcance

- Autorización por ticket (participante vigente, propietario, coordinador): llega con las HU de tickets y chat.
- Crear usuarios, asignar roles o equipos ([[hu-008-administrar-usuarios-cliente|HU-008]], [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]]).
- Row-Level Security de PostgreSQL (decisión abierta, [[pendientes]] §4).
- Autorización del hub `/hubs/tickets` (llega con [[ep-009-chat-interno-en-tiempo-real]]; la prueba de endpoints lo cubrirá cuando exista).

## Requisitos y reglas de negocio

- La autorización se aplica en servidor en cada operación; ocultar una fila en la interfaz no constituye aislamiento (PRD §5 regla 2).
- Los usuarios cliente solo consultan tickets de la empresa a la que pertenece su cuenta (PRD §5 regla 1); la cuenta determina la empresa y no se elige libremente (PRD §6.1).
- Todo acceso exige autenticación, salvo páginas de entrada expresamente públicas (PRD §12).
- Un administrador no obtiene acceso general al contenido por ser administrador (PRD §5 regla 5): las políticas por rol no conceden acceso a tickets.
- Pertenecer a un equipo no da acceso al detalle ni al chat de un ticket (PRD §6.2.5).

## Invariantes en juego

- Invariante 1 (`AGENTS.md` §7): el filtro multiempresa depende de `ICurrentUser.CompanyId`, que solo proviene de la sesión.
- Invariante 5: ninguna política concede contenido de tickets por rol `Administrator`.
- Invariante 9: los endpoints públicos se limitan a la lista explícita.
- Regla de arquitectura (`AGENTS.md` §6): `ICurrentUser` en `Application` sin referencias a ASP.NET Core; el adaptador vive en `Api`.

## Criterios del PRD cubiertos

- PRD CA-01 (parcial): el backend obtiene la empresa del usuario cliente exclusivamente de la sesión y deja preparada la base para las pruebas negativas sobre tickets de [[hu-024-portal-solicitante-consulta-tickets|HU-024]] y [[hu-025-portal-coordinador-consulta-tickets|HU-025]] → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-002-identidad-y-acceso]]
- Dependencias: [[hu-003-iniciar-y-cerrar-sesion|HU-003]] (Identity, cookie, proyecto de integración).
- Relacionadas: [[hu-002-shell-y-navegacion-por-rol|HU-002]] (consume `/me`), [[hu-008-administrar-usuarios-cliente|HU-008]] y [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]] (cambian roles, equipos y empresa), [[hu-024-portal-solicitante-consulta-tickets|HU-024]], [[hu-025-portal-coordinador-consulta-tickets|HU-025]].
- Decisiones: [[adr-0004-autenticacion-cookie-mismo-origen]] (propuesta), [[adr-0002-backend-hexagonal]]. Riesgo abierto: roles frente a equipos (ver Notas).

## Componentes afectados

- Identity: fábrica de claims (`IUserClaimsPrincipalFactory<ApplicationUser>`).
- Backend `Application`: puerto `ICurrentUser`.
- Backend `Api`: adaptador de `ICurrentUser`, políticas, política de reserva, `GET /api/auth/me`, `AllowAnonymous` explícito en públicos (`/api/health`, `/api/health/live`, `/api/auth/*` públicos).
- Pruebas: integración (`/me`, recorrido de endpoints) y unitarias (políticas, adaptador).
- Frontend: prueba de contrato del normalizador de `/me` (módulo `session` de HU-002).

## Dificultad

**Nivel:** Alto

**Justificación:** Define la base de autorización de todo el producto (políticas, *fallback*, puerto de aplicación, claims) y su error tiene impacto transversal en seguridad multiempresa. Arrastra una inconsistencia abierta entre roles y equipos que condiciona el formato de los claims.

## Contrato backend ↔ frontend

Propuesta REST; ratificar en T-01.

**`GET /api/auth/me`** — autenticado

```json
// 200 — Solicitante de la empresa A
{
  "id": "6f1c2a9e-4c1d-4b8e-9a51-1f0e2d3c4b5a",
  "displayName": "Ana Pérez",
  "email": "ana.perez@empresa-sintetica-01.test",
  "surface": "client",
  "roles": ["Requester"],
  "teams": [],
  "company": { "id": "0b7d6c1e-2f3a-4b5c-8d9e-0a1b2c3d4e5f", "name": "Empresa Sintética 01" }
}
```

```json
// 200 — PM, integrante de Producción
{
  "id": "9a2e4f60-7b1c-4d2e-8f3a-5b6c7d8e9f01",
  "displayName": "Elizabeth",
  "email": "elizabeth@dataticket.local",
  "surface": "internal",
  "roles": ["ProductManager"],
  "teams": ["Production"],
  "company": null
}
```

| Campo | Regla |
|---|---|
| `surface` | `"client"` si tiene `Requester` o `CompanyCoordinator`; `"internal"` en otro caso (propuesta) |
| `roles` | Subconjunto de `ProductManager`, `Administrator`, `Requester`, `CompanyCoordinator` |
| `teams` | Subconjunto de `Development`, `Production` (forma definitiva sujeta a la decisión roles/equipos) |
| `company` | Solo para `surface = "client"`; `null` para internos |

| Respuesta | Cuándo |
|---|---|
| `200` | Sesión válida |
| `401` `application/problem+json` | Sin sesión o sesión revocada |

No admite parámetros: cualquier `?userId=` o cabecera adicional se ignora.

**Lista explícita de endpoints públicos** (propuesta): `GET /api/health`, `GET /api/health/live`, `GET /api/auth/antiforgery`, `POST /api/auth/login`, `POST /api/auth/invitations/accept`, `POST /api/auth/password/forgot`, `POST /api/auth/password/reset`, y `GET /openapi/v1.json` solo en Development.

**Códigos comunes**: `401` sin sesión; `403` autenticado sin el rol de la política; ambos en `application/problem+json`.

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: fijar el JSON de `/me`, la regla de `surface`, los nombres de políticas y claims (`company_id`, claim de equipo propuesto `team`), la lista de públicos y **cómo se representan los equipos** (rol Identity frente a claim/tabla `Team.*`), registrando la decisión en un ADR o en [[pendientes]] sin resolverla en silencio.
- [ ] **T-02 — Puerto `ICurrentUser` (pruebas primero)** · Capa: Backend (Application) · Dificultad: Bajo  
  Descripción: interfaz en `Application/Ports/Out` (`UserId`, `IsAuthenticated`, `Roles`, `Teams`, `CompanyId?`, `IsInRole`), sin dependencias de ASP.NET Core; las `ArchitectureTests` lo verifican.
- [ ] **T-03 — Adaptador de `ICurrentUser` y fábrica de claims** · Capa: Backend (Api / Infrastructure) · Dificultad: Medio  
  Descripción: pruebas unitarias primero: principal de cliente con `company_id` → `CompanyId` correcto; principal de cliente sin `company_id` → falla cerrada (no autorizado); principal interno → `CompanyId = null`. La fábrica de claims añade roles, equipos y `company_id` solo a cuentas cliente; una cuenta con rol cliente e interno a la vez se rechaza (propuesta).
- [ ] **T-04 — Políticas y *fallback* (pruebas primero)** · Capa: Backend (Api) · Dificultad: Medio  
  Descripción: pruebas con `IAuthorizationService` y principales construidos para la matriz de CHU-05; luego registrar las políticas, la política de reserva `RequireAuthenticatedUser` y `AllowAnonymous` en los públicos existentes (`/api/health`, `/api/health/live`) y de HU-003.
- [ ] **T-05 — Endpoint `/api/auth/me`** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: prueba de integración primero para cliente e interno; el endpoint compone la respuesta desde `ICurrentUser` y el nombre de la empresa, sin aceptar parámetros.
- [ ] **T-06 — Prueba de recorrido de endpoints** · Capa: Backend (pruebas) · Dificultad: Medio  
  Descripción: prueba de integración que obtiene `EndpointDataSource`, recorre todos los `RouteEndpoint` y falla si alguno con metadatos `IAllowAnonymous` no está en la lista explícita; complementa con peticiones anónimas a los endpoints `GET` no públicos esperando `401`.
- [ ] **T-07 — Prueba de convención de DTOs de cliente** · Capa: Backend (pruebas) · Dificultad: Bajo  
  Descripción: (propuesta) prueba que inspecciona los tipos de entrada de los endpoints con política `ClientUser` y falla si alguno declara una propiedad `CompanyId`.
- [ ] **T-08 — Frontend: prueba de contrato de `/me`** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: añadir al módulo `session` de HU-002 pruebas Vitest que normalizan los dos ejemplos JSON de esta HU y un `surface` desconocido (→ sin acceso).
- [ ] **T-09 — Notas para la wiki** · Capa: Documentación · Dificultad: Bajo  
  Descripción: [[autenticacion-identity]] (claims, políticas, públicos), [[roles-y-permisos]] (matriz de políticas), [[backend-hexagonal]] (`ICurrentUser`), [[glosario]] (políticas y claim, si se adoptan), [[pendientes]] (roles frente a equipos).

## Criterios de aceptación

### CHU-01 — `/me` de un usuario cliente

**Dado** la Solicitante `ana.perez@empresa-sintetica-01.test` de la empresa A con sesión iniciada  
**Cuando** consulta `GET /api/auth/me`  
**Entonces** recibe `200` con su `id`, `displayName`, `email`, `surface = "client"`, `roles = ["Requester"]`, `teams = []` y `company` con el id y el nombre de la empresa A.

### CHU-02 — `/me` de un usuario interno

**Dado** una usuaria interna con rol `ProductManager` y equipo Producción  
**Cuando** consulta `GET /api/auth/me`  
**Entonces** recibe `200` con `surface = "internal"`, `roles = ["ProductManager"]`, `teams = ["Production"]` y `company = null`.

### CHU-03 — `/me` no expone datos de otros (prueba negativa A↔B)

**Dado** la Solicitante de la empresa A y un usuario `luis.gomez@empresa-sintetica-02.test` de la empresa B  
**Cuando** la Solicitante de A consulta `GET /api/auth/me?userId=<id de Luis>` con la cabecera adicional `X-Company-Id: <id de B>`  
**Entonces** la respuesta es la de CHU-01 (su propio usuario y la empresa A); no aparece ningún dato de Luis ni de la empresa B; el JSON solo contiene los campos del contrato.

### CHU-04 — Sin sesión no hay contexto

**Dado** una petición sin cookie o con una cookie de una sesión cerrada  
**Cuando** consulta `GET /api/auth/me`  
**Entonces** recibe `401` `application/problem+json` sin cabecera `Location`.

### CHU-05 — Matriz de políticas por rol

**Dado** principales con cada perfil (`Requester`, `CompanyCoordinator`, `ProductManager`, `Administrator`, miembro solo de equipo Desarrollo)  
**Cuando** se evalúan las políticas con `IAuthorizationService`  
**Entonces** `ClientUser` solo autoriza a `Requester` y `CompanyCoordinator`; `InternalUser` solo a `ProductManager`, `Administrator` y miembros de equipo; `Administrator` solo a `Administrator`; `ProductManagerOrAdministrator` solo a esos dos roles; y un miembro de equipo Desarrollo sin otros roles no satisface `Administrator` ni `ProductManagerOrAdministrator`.

### CHU-06 — Autenticación por defecto en todos los endpoints

**Dado** todos los endpoints registrados en la aplicación  
**Cuando** se ejecuta la prueba de recorrido  
**Entonces** los únicos endpoints que admiten acceso anónimo son los de la lista explícita del contrato; si alguien añade un endpoint sin autorización (en la prueba, uno de control registrado sin metadatos), la política de reserva lo protege y una petición anónima recibe `401`.

### CHU-07 — `company_id` solo desde la sesión

**Dado** un principal de cliente cuyo claim `company_id` es la empresa A  
**Cuando** un caso de uso lee `ICurrentUser.CompanyId`  
**Entonces** obtiene A; un principal de cliente sin claim `company_id` es tratado como no autorizado (respuesta `403`); y la prueba de convención confirma que ningún DTO de entrada de un endpoint con política `ClientUser` declara `CompanyId`.

### CHU-08 — El rol de administrador no da acceso a contenido

**Dado** un principal con rol `Administrator`  
**Cuando** se evalúan las políticas `ClientUser` y las previstas para contenido de tickets  
**Entonces** `ClientUser` falla y ninguna política registrada en esta HU concede acceso a contenido de tickets por el solo hecho de ser administrador (las de ticket se definen por participación en HU posteriores).

### CHU-09 — Arquitectura hexagonal intacta

**Dado** el puerto `ICurrentUser` y su adaptador  
**Cuando** se ejecuta `cd backend && dotnet test`  
**Entonces** `DataTicket.ArchitectureTests` pasa: `ICurrentUser` está en `DataTicket.Application` sin referencias a `Microsoft.AspNetCore*`, y el adaptador que lee `HttpContext` está en `DataTicket.Api`.

### CHU-10 — El frontend tolera el contrato

**Dado** los dos ejemplos JSON de esta HU y uno con `surface = "partner"`  
**Cuando** el normalizador del módulo `session` los procesa  
**Entonces** los dos primeros producen un `CurrentUser` con la superficie correcta y el tercero produce «sin acceso», y el shell muestra la vista «sin permiso».

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-10 validados con evidencia.
- [ ] **DoD-02** — Pruebas escritas primero y en verde, con nombres por comportamiento: `Me_de_cliente_incluye_solo_su_empresa`, `Me_de_interno_no_tiene_empresa`, `Me_ignora_userId_y_cabeceras_de_empresa`, `Me_sin_sesion_responde_401`, `Politicas_por_rol_cumplen_la_matriz`, `Solo_la_lista_publica_admite_anonimos`, `Cliente_sin_company_id_falla_cerrado`, `DTOs_de_cliente_no_declaran_CompanyId`.
- [ ] **DoD-03** — `cd backend && dotnet test` en verde, incluidas `DataTicket.ArchitectureTests`.
- [ ] **DoD-04** — `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build` en verde.
- [ ] **DoD-05** — Contrato de `/me` coherente con [[hu-002-shell-y-navegacion-por-rol|HU-002]]: el shell funciona contra el endpoint real en `docker compose up --build` (verificación manual documentada).
- [ ] **DoD-06** — Decisión sobre la representación de equipos registrada (ADR o entrada actualizada en [[pendientes]]) antes de fusionar.
- [ ] **DoD-07** — Sin cambios de esquema (si la decisión de equipos los exige, se trasladan a HU-009 con su migración).
- [ ] **DoD-08** — Wiki actualizada vía Notas para la wiki: [[autenticacion-identity]], [[roles-y-permisos]], [[backend-hexagonal]], [[glosario]], [[pendientes]].
- [ ] **DoD-09** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (*fallback*, lista de públicos, origen de `company_id`).
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

- **Riesgo — roles frente a equipos**: [[autenticacion-identity]] lista `Development` y `Production` como roles de Identity; [[glosario]], [[roles-y-permisos]] y [[equipo-data-global]] usan `Team.Development` / `Team.Production`. El contrato de `/me` separa `roles` y `teams` para no prejuzgar; la representación interna se decide en T-01 (y la usa [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]]).
- **Frescura de claims**: si un administrador cambia roles, equipos o desactiva una cuenta, los claims de la cookie se actualizan cuando el sello de seguridad se revalida; el intervalo se ratifica en T-01 de HU-003.
- Nombres propuestos (ratificar en T-01 y registrar en el [[glosario]]): políticas `ClientUser`, `InternalUser`, `Administrator`, `ProductManagerOrAdministrator`; claim `team`; campo `surface`.
- Responder `404` (no `403`) ante tickets de otra empresa es propuesta para las HU de tickets; aquí solo se fija el `403` por rol.

## Relacionado

- [[ep-002-identidad-y-acceso]] · [[tablero-scrum]] · [[autenticacion-identity]] · [[roles-y-permisos]] · [[backend-hexagonal]] · [[adr-0004-autenticacion-cookie-mismo-origen]] · [[criterios-de-aceptacion]] · [[equipo-data-global]] · [[glosario]]
