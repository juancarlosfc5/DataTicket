---
title: "HU-004 — Contexto del usuario y autorización por rol"
type: historia-de-usuario
status: vigente
estado: Completada
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
updated: 2026-10-14
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
- Decisiones: [[adr-0004-autenticacion-cookie-mismo-origen]] (aceptada el 2026-10-09), [[adr-0002-backend-hexagonal]]. Riesgo abierto: roles frente a equipos (ver Notas).

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

- [x] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: fijar el JSON de `/me`, la regla de `surface`, los nombres de políticas y claims (`company_id`, claim de equipo propuesto `team`), la lista de públicos y **cómo se representan los equipos** (rol Identity frente a claim/tabla `Team.*`), registrando la decisión en un ADR o en [[pendientes]] sin resolverla en silencio.
- [x] **T-02 — Puerto `ICurrentUser` (pruebas primero)** · Capa: Backend (Application) · Dificultad: Bajo  
  Descripción: interfaz en `Application/Ports/Out` (`UserId`, `IsAuthenticated`, `Roles`, `Teams`, `CompanyId?`, `IsInRole`), sin dependencias de ASP.NET Core; las `ArchitectureTests` lo verifican.
- [x] **T-03 — Adaptador de `ICurrentUser` y fábrica de claims** · Capa: Backend (Api / Infrastructure) · Dificultad: Medio  
  Descripción: pruebas unitarias primero: principal de cliente con `company_id` → `CompanyId` correcto; principal de cliente sin `company_id` → falla cerrada (no autorizado); principal interno → `CompanyId = null`. La fábrica de claims añade roles, equipos y `company_id` solo a cuentas cliente; una cuenta con rol cliente e interno a la vez se rechaza (propuesta).
- [x] **T-04 — Políticas y *fallback* (pruebas primero)** · Capa: Backend (Api) · Dificultad: Medio  
  Descripción: pruebas con `IAuthorizationService` y principales construidos para la matriz de CHU-05; luego registrar las políticas, la política de reserva `RequireAuthenticatedUser` y `AllowAnonymous` en los públicos existentes (`/api/health`, `/api/health/live`) y de HU-003.
- [x] **T-05 — Endpoint `/api/auth/me`** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: prueba de integración primero para cliente e interno; el endpoint compone la respuesta desde `ICurrentUser` y el nombre de la empresa, sin aceptar parámetros.
- [x] **T-06 — Prueba de recorrido de endpoints** · Capa: Backend (pruebas) · Dificultad: Medio  
  Descripción: prueba de integración que obtiene `EndpointDataSource`, recorre todos los `RouteEndpoint` y falla si alguno con metadatos `IAllowAnonymous` no está en la lista explícita; complementa con peticiones anónimas a los endpoints `GET` no públicos esperando `401`.
- [x] **T-07 — Prueba de convención de DTOs de cliente** · Capa: Backend (pruebas) · Dificultad: Bajo  
  Descripción: (propuesta) prueba que inspecciona los tipos de entrada de los endpoints con política `ClientUser` y falla si alguno declara una propiedad `CompanyId`.
- [x] **T-08 — Frontend: prueba de contrato de `/me`** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: añadir al módulo `session` de HU-002 pruebas Vitest que normalizan los dos ejemplos JSON de esta HU y un `surface` desconocido (→ sin acceso).
- [x] **T-09 — Notas para la wiki** · Capa: Documentación · Dificultad: Bajo  
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

- [x] **DoD-01** — CHU-01 a CHU-10 validados con evidencia.
- [x] **DoD-02** — Pruebas escritas primero y en verde, con nombres por comportamiento: `Me_de_cliente_incluye_solo_su_empresa`, `Me_de_interno_no_tiene_empresa`, `Me_ignora_userId_y_cabeceras_de_empresa`, `Me_sin_sesion_responde_401`, `Politicas_por_rol_cumplen_la_matriz`, `Solo_la_lista_publica_admite_anonimos`, `Cliente_sin_company_id_falla_cerrado`, `DTOs_de_cliente_no_declaran_CompanyId`.
- [x] **DoD-03** — `cd backend && dotnet test` en verde, incluidas `DataTicket.ArchitectureTests`.
- [x] **DoD-04** — `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build` en verde.
- [x] **DoD-05** — Contrato de `/me` coherente con [[hu-002-shell-y-navegacion-por-rol|HU-002]]: el shell funciona contra el endpoint real en `docker compose up --build` (verificación manual documentada).
- [x] **DoD-06** — Decisión sobre la representación de equipos registrada (ADR o entrada actualizada en [[pendientes]]) antes de fusionar.
- [x] **DoD-07** — Sin cambios de esquema (si la decisión de equipos los exige, se trasladan a HU-009 con su migración).
- [x] **DoD-08** — Wiki actualizada vía Notas para la wiki: [[autenticacion-identity]], [[roles-y-permisos]], [[backend-hexagonal]], [[glosario]], [[pendientes]].
- [x] **DoD-09** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (*fallback*, lista de públicos, origen de `company_id`).
- [x] **DoD-10** — PR revisado y aprobado por otra persona del equipo; trazabilidad de HU y épica actualizada. *(dispensado por la persona líder el 2026-10-14)*

## Evidencia de validación

Validación del 2026-10-12 sobre la rama `feature/auth-login` (bloques B0–B7 de `goal_login.md`, hasta el commit `70137ab`). Base de la evidencia:

- **Suite backend** (ejecutada por el orquestador el 2026-10-12): `cd backend && dotnet build` → 0 advertencias / 0 errores; `cd backend && dotnet test` → total 169, error 0, correcto 169 (exit 0). Salvo que se indique otro proyecto, las clases citadas están en `backend/tests/DataTicket.IntegrationTests/`.
- **Suite frontend** (orquestador, 2026-10-12): `npm --prefix frontend run lint` → 0 advertencias / 0 errores en 58 archivos; `npm --prefix frontend test` → 17 archivos / 152 pruebas en verde; `npm --prefix frontend run build` → exit 0. Rutas relativas a `frontend/src/`.
- **Verificación manual** (orquestador, 2026-10-12): stack reconstruido con `docker compose up --build -d backend frontend` (exit 0); `curl` por el proxy `http://localhost:5173` y navegador integrado, con una cuenta interna (Elizabeth) y una cliente (Ana Pérez) del sembrador de Development. Detalle en `goal_login.md` §3 (B7) y en [[log]].
- **Resultados**: `Cumple`, `No cumple` y `No verificable` según la skill `scrum-spec-orchestrator`; `Pendiente` indica que todavía no ocurrió lo que hay que validar.

> [!warning] Desviaciones respecto al texto de la HU (2026-10-12)
> - **`/me` responde también `403`.** El contrato lista `200` y `401`; el código devuelve además `403` «No tienes permiso para realizar esta acción.» cuando hay sesión pero el perfil no es válido (cliente sin `company_id`, perfil mixto, interno con empresa…). En esos casos `surface` no vale `"internal"` «en otro caso», como proponía la tabla del contrato: no hay superficie. Por ratificar: [[pendientes]] §1 y P-23.
> - **El claim `team` no existe.** Los equipos viajan en la cookie como `ClaimTypes.Role` (roles de Identity `Development` y `Production`, `RoleNames.Teams`) y se exponen aparte en `teams` y en `ICurrentUser.Teams` ([[autenticacion-identity]]).
> - **El nombre visible viaja en el claim `display_name`.** El contrato T-01 no fijaba el tipo del claim; por ratificar (P-23).
> - **Prueba negativa A↔B (CHU-03).** El usuario de la empresa B es `TestAccounts.Company02Requester1`: el correo de ejemplo `luis.gomez@empresa-sintetica-02.test` no está en el catálogo de `db.sql` §13.
> - **Prueba de convención antiforgery.** Se llama `Toda_mutacion_lleva_el_filtro_antiforgery` (hallazgo BAJO-8 de la revisión de B3); no figura en el DoD-02 de esta HU.
> - **T-02 y T-03.** `ICurrentUser` expone además `DisplayName`, `Email` y `Surface`. Las pruebas del adaptador (`Auth/CurrentUserAdapterTests`) están en `DataTicket.IntegrationTests`, sin host ni Docker, porque `DataTicket.UnitTests` no referencia `Api`.
> - **T-07.** La convención de DTO es más amplia que la propuesta: revisa todo endpoint que no sea solo interno, no únicamente los que llevan la política `ClientUser`.

| Elemento | Resultado | Evidencia | Observación |
|---|---|---|---|
| CHU-01 | Cumple | `Auth/MeEndpointTests.Me_de_cliente_incluye_solo_su_empresa` (`200` con `id`, `displayName`, `email`, `surface = "client"`, `roles = ["Requester"]`, `teams = []` y `company` con el id y el nombre de la Empresa Sintética 01; exactamente los siete campos del contrato) — suite backend. Verificación manual: `/me` de Ana Pérez → `surface: client`, `roles: [Requester]`, `company: {id, name: "Empresa Sintética 01"}` | Los identificadores son los de `db.sql` §13, no los del ejemplo de la HU |
| CHU-02 | Cumple | `Auth/MeEndpointTests`: `Me_de_interno_no_tiene_empresa` (`surface = "internal"`, `roles = ["ProductManager"]`, `teams = ["Production"]` y `company` presente con valor `null`) y `Me_separa_roles_y_equipos_en_cada_perfil`. Verificación manual: `/me` de Elizabeth → `surface: internal`, `roles: [ProductManager]`, `teams: [Production]`, `company: null` | — |
| CHU-03 | Cumple | `Auth/MeEndpointTests.Me_ignora_userId_y_cabeceras_de_empresa` (prueba negativa A↔B en los dos sentidos: con `?userId=`, `?companyId=`, `X-Company-Id`, `X-User-Id` y `X-Forwarded-User` la respuesta es idéntica byte a byte a la propia y no contiene el id, el correo ni la empresa del otro usuario) | El usuario de la empresa B es `TestAccounts.Company02Requester1` (ver desviaciones) |
| CHU-04 | Cumple | `Auth/MeEndpointTests.Me_sin_sesion_responde_401` (sin cookie, con una cookie que no emitió el servidor y tras el logout: `401` `application/problem+json` sin `Location`). Verificación manual: `/me` → 401 después de `POST /api/auth/logout` | «Sesión cerrada» se prueba en el mismo navegador, donde la cookie expira. Una copia de la cookie guardada antes del logout sigue valiendo hasta su expiración o hasta la revalidación del sello: riesgo aceptado en T-01 (P-22) |
| CHU-05 | Cumple | `Auth/AuthorizationPolicyTests.Politicas_por_rol_cumplen_la_matriz` (16 perfiles × 4 políticas con `IAuthorizationService`), `Auth/SessionAuthorizationTests.Politicas_con_sesion_real_cumplen_la_matriz` y `Auth/AuthorizationPolicyTests.Politicas_son_las_mismas_fuera_de_Development` | — |
| CHU-06 | Cumple | `Conventions/EndpointAuthorizationConventionTests`: `Solo_la_lista_publica_admite_anonimos` (igualdad exacta en Development y Production), `Endpoints_no_publicos_responden_401_sin_sesion`, `Endpoint_sin_metadatos_exige_sesion` (el endpoint de control sin metadatos responde `401`) y `Anonimo_nunca_recibe_2xx_fuera_de_la_lista_publica`; `Auth/AuthorizationPolicyTests.Politica_de_reserva_exige_usuario_autenticado` | Lista implementada: `GET /api/health`, `GET /api/health/live`, `GET /api/auth/antiforgery`, `POST /api/auth/login` y OpenAPI solo en Development. Las tres rutas públicas de HU-005 y HU-006 que lista el contrato aún no existen. El alcance de la reserva (`404` en rutas inexistentes) está por ratificar (P-23) |
| CHU-07 | Cumple | `Auth/CurrentUserAdapterTests.Principal_de_cliente_con_company_id_expone_su_empresa`; `Auth/SessionAuthorizationTests`: `ICurrentUser_entrega_la_empresa_de_la_sesion_y_no_la_de_la_peticion` y `Cliente_sin_company_id_falla_cerrado` (`403`); `Conventions/ClientInputConventionTests.DTOs_de_cliente_no_declaran_CompanyId`; `Auth/ClaimsFactoryTests.Company_id_sale_solo_de_la_cuenta_y_no_de_claims_almacenados` | La convención revisa cuerpo, consulta, ruta, cabeceras y `[AsParameters]` de todo endpoint que no sea solo interno |
| CHU-08 | Cumple | `Auth/AuthorizationPolicyTests.Administrador_no_obtiene_politicas_de_cliente_ni_de_contenido` (las políticas registradas son exactamente las cuatro del contrato, en Development y Production, y el administrador no satisface `ClientUser`) | — |
| CHU-09 | Cumple | `backend/tests/DataTicket.ArchitectureTests`: `CurrentUserPortTests.CurrentUser_port_lives_in_Application_Ports_Out`, `CurrentUser_port_does_not_expose_AspNetCore_types` y `CurrentUser_adapter_lives_only_in_Api`; `LayerDependencyTests.Application_DoesNotDependOnFrameworks` (suite backend) | — |
| CHU-10 | Cumple | `modules/session/models/currentUser.test.ts` › «contrato de /me (HU-004 CHU-10)»: `el ejemplo de cliente produce un CurrentUser del portal con su empresa`, `el ejemplo interno produce un CurrentUser interno sin empresa` y `surface "partner" produce un usuario sin acceso`; `app/App.test.tsx`: `un surface desconocido (partner) se trata como sin acceso y muestra sin permiso (HU-004 CHU-10)` (suite frontend) | — |
| DoD-01 | Cumple | CHU-01 a CHU-10 en `Cumple` (filas anteriores) | — |
| DoD-02 | Cumple | Las ocho pruebas existen con el nombre exacto y pasan (suite backend): `Me_de_cliente_incluye_solo_su_empresa`, `Me_de_interno_no_tiene_empresa`, `Me_ignora_userId_y_cabeceras_de_empresa`, `Me_sin_sesion_responde_401`, `Politicas_por_rol_cumplen_la_matriz`, `Solo_la_lista_publica_admite_anonimos`, `Cliente_sin_company_id_falla_cerrado` y `DTOs_de_cliente_no_declaran_CompanyId` | Rojo inicial registrado en B4 (`total: 140, error: 38`) y verde de B4 con 154/154 (`goal_login.md` §3) |
| DoD-03 | Cumple | `cd backend && dotnet test` → total 169, error 0, correcto 169 (exit 0) el 2026-10-12, con las 13 pruebas de `DataTicket.ArchitectureTests` | — |
| DoD-04 | Cumple | Suite frontend del 2026-10-12: lint 0/0 en 58 archivos, 152 pruebas en 17 archivos y build con exit 0 | — |
| DoD-05 | Cumple | Verificación manual del 2026-10-12 contra el stack de Compose: el shell consume el `/me` real; la interna entra a `/interno/bandeja` con su navegación y la cliente a `/portal` con la suya | El SPA no distingue el `403` de `/me`: acaba en el estado de error de carga y no en «sin permiso» ([[pendientes]] §1) |
| DoD-06 | Cumple | [[pendientes]] §1, «roles frente a equipos», con las actualizaciones del 2026-10-09 (B1) y del 2026-10-12 (B4): los equipos son los roles de Identity `Development` y `Production`, expuestos en `teams` | Decisión provisional: el modelo definitivo sigue abierto para HU-009 |
| DoD-07 | Cumple | B4 no añadió ninguna migración: la única sigue siendo `20261009164647_AddIdentityAndCompanies`, de HU-003; `dotnet ef migrations has-pending-model-changes` → sin cambios (B4, `goal_login.md` §3) | — |
| DoD-08 | Cumple | [[autenticacion-identity]], [[roles-y-permisos]], [[backend-hexagonal]], [[glosario]] y [[pendientes]], al día el 2026-10-12 | — |
| DoD-09 | Cumple | Revisión final de `quality-reviewer` sobre la rama `feature/auth-login` (B8, 2026-10-12): 0 CRÍTICO y 0 ALTO abiertos. Antes, por bloque: B4 con 0 CRÍTICO / 0 ALTO y sus 2 MEDIO y BAJO 1–6 corregidos con prueba; B6 y B7 con 0 CRÍTICO / 0 ALTO ([[log]]) | Veredicto final recibido el 2026-10-12. Sus 2 MEDIO eran de documentación (tablero y épicas sin actualizar, y estas matrices) y se atendieron en B8; los 8 BAJO quedaron registrados |
| DoD-10 | Dispensado | Decisión de la persona líder del 2026-10-14 | Depende de personas: la rama se fusionó en `main` el 2026-10-12 (`718e0a8`) por instrucción de la persona líder, sin Pull Request; sigue faltando la revisión y aprobación de otra persona del equipo, ahora sobre lo ya fusionado |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.
- 2026-10-09 — Se ejecuta en el goal de `goal_login.md` (bloques B1, B4, B6).
- 2026-10-09 — Estado `En desarrollo`: inicia el `/goal` de `goal_login.md` en la rama `feature/auth-login` (bloque B0).
- 2026-10-12 — Estado `En validación` (bloque B8 de `goal_login.md`): tareas T-02 a T-09 hechas en B4 y B6, y matriz de evidencia completada. Resultado: 19 `Cumple` y 1 `Pendiente` (DoD-10, revisión del PR por otra persona). Quedan por ratificar con el equipo P-23 (alcance de la política de reserva, claim `display_name` y `403` de `/me`) y el modelo definitivo de equipos (HU-009). Revisión final de `quality-reviewer` sobre la rama: 0 CRÍTICO y 0 ALTO abiertos.
- 2026-10-12 — La rama `feature/auth-login` se fusionó en `main` (`718e0a8`) y se publicó en `origin`, por instrucción de la persona líder y sin Pull Request. La HU **sigue `En validación`**: falta la revisión y aprobación de otra persona del equipo, ahora sobre lo ya fusionado ([[pendientes]] §1).
- 2026-10-14 — Estado `Completada` por decisión de la persona líder: dispensa la revisión por otra persona del equipo (filas `Dispensado` en la matriz) y ratifica P-18, P-22 y P-23 ([[pendientes]] §1).

## Notas y decisiones

- **Riesgo — roles frente a equipos**: [[autenticacion-identity]] lista `Development` y `Production` como roles de Identity; [[glosario]], [[roles-y-permisos]] y [[equipo-data-global]] usan `Team.Development` / `Team.Production`. El contrato de `/me` separa `roles` y `teams` para no prejuzgar; la representación interna se decide en T-01 (y la usa [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]]).
- **Frescura de claims**: si un administrador cambia roles, equipos o desactiva una cuenta, los claims de la cookie se actualizan cuando el sello de seguridad se revalida; el intervalo se ratifica en T-01 de HU-003.
- Nombres propuestos (ratificar en T-01 y registrar en el [[glosario]]): políticas `ClientUser`, `InternalUser`, `Administrator`, `ProductManagerOrAdministrator`; claim `team`; campo `surface`.
- Responder `404` (no `403`) ante tickets de otra empresa es propuesta para las HU de tickets; aquí solo se fija el `403` por rol.

## Relacionado

- [[ep-002-identidad-y-acceso]] · [[tablero-scrum]] · [[autenticacion-identity]] · [[roles-y-permisos]] · [[backend-hexagonal]] · [[adr-0004-autenticacion-cookie-mismo-origen]] · [[criterios-de-aceptacion]] · [[equipo-data-global]] · [[glosario]]
