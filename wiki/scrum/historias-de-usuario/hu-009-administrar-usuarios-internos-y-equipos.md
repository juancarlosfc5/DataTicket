---
title: "HU-009 — Administrar usuarios internos, equipos y roles"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/roles, personas, identity, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §6.3", "PRD.md §10", "PRD.md §12"]
aliases: ["HU-009", "Administrar usuarios internos, equipos y roles", "Administrar usuarios internos y equipos"]
epica: "[[ep-003-empresas-usuarios-y-equipos]]"
criterios_prd: []
componentes: ["Backend (Domain: User, equipos)", "Backend (Application: casos de uso de usuarios internos)", "Backend (Infrastructure: Identity, EF Core)", "Backend (Api: /api/admin/internal-users)", "Persistencia PostgreSQL", "Frontend (models, controllers, views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 1"
dependencias: ["[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-005-invitar-y-activar-cuentas]]", "[[hu-008-administrar-usuarios-cliente]]", "[[hu-011-registrar-eventos-auditables]]"]
relacionadas: ["[[hu-010-datos-sinteticos-de-desarrollo]]", "[[hu-016-cola-global-de-triage]]", "[[hu-018-asignar-y-agregar-participantes]]", "[[hu-020-detalle-interno-del-ticket]]", "[[hu-023-bandeja-de-mis-tickets-y-equipo]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-009 — Administrar usuarios internos, equipos y roles

El administrador invita a las personas de Data Global, les asigna los roles `ProductManager` y `Administrator` y la pertenencia a los equipos Desarrollo y Producción, cambia esas asignaciones y desactiva cuentas internas. Un usuario interno no pertenece a ninguna empresa cliente (propuesta) y pertenecer a un equipo **no** da acceso a tickets.

## Historia de usuario

**COMO** administrador de DataTicket  
**QUIERO** invitar a las personas de Data Global y gestionar sus roles (PM, administrador) y su pertenencia a los equipos Desarrollo y Producción  
**PARA** que triage, asignación, bandejas por equipo y chat funcionen con las personas correctas sin conceder acceso a tickets por el solo hecho de pertenecer a un equipo

## Contexto

PRD §5 define los equipos iniciales (Desarrollo: Juan David, Laura, Brayan, Cristian, Kevin; Producción: Julián, Elizabeth, Gerardo), que Elizabeth es PM y pertenece a Producción, que Julián lidera Producción y que «usuarios y asignaciones futuras se administran dentro de DataTicket». PRD §6.2.5 y §10: la asignación a un equipo no sustituye la lista de participantes; el acceso al detalle y al chat requiere asociación explícita, salvo Elizabeth. [[equipo-data-global]] recoge el piloto. **Inconsistencia abierta**: [[autenticacion-identity]] trata `Development` y `Production` como roles de Identity, mientras [[glosario]] y [[roles-y-permisos]] los nombran `Team.Development` / `Team.Production`; esta HU la registra y la resuelve solo vía T-01 con decisión explícita.

## Alcance

- Modelo de pertenencia a equipos de un usuario interno (forma decidida en T-01: rol de Identity o entidad/claim `Team.*`).
- Casos de uso (nombres propuestos, ratificar en T-01 y registrar en el [[glosario]]): `CreateInternalUser` (crea la cuenta `Invited` y dispara `InviteUser`), `ChangeInternalUserMembership`, `ListInternalUsers`; reglas adicionales para `DeactivateUser` sobre cuentas internas.
- Endpoints `/api/admin/internal-users` con política `Administrator`.
- Reglas de protección (propuestas): un administrador no puede quitarse su propio rol `Administrator` ni desactivarse; siempre queda al menos un `Administrator` activo.
- Marca informativa de liderazgo de equipo (propuesta, sin permisos asociados) para reflejar que Julián lidera Producción.
- Refresco de claims al cambiar roles o equipos.
- Auditoría de alta y cambios de roles/equipos (propuesta).
- Frontend: listado de usuarios internos con filtros por equipo, rol y estado; invitación; edición de roles y equipos.

## Fuera de alcance

- Usuarios cliente ([[hu-008-administrar-usuarios-cliente|HU-008]]).
- Bandejas por equipo y responsabilidad ([[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]]).
- Determinar la «ausencia» de Elizabeth para la cobertura de triage (V-05, [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]]).
- Permisos derivados del liderazgo de equipo (el PRD no los define).
- Nombrar a los administradores del piloto (V-15).

## Requisitos y reglas de negocio

- Roles internos: PM (`ProductManager`) con triage, asignación, respuesta formal y cierre; Administrador (`Administrator`) que administra el sistema y gestiona participantes sin acceso automático al contenido (PRD §5).
- Equipos Desarrollo (`Team.Development`) y Producción (`Team.Production`); Elizabeth ejerce de PM y pertenece a Producción; Julián lidera Producción (PRD §5).
- Pertenecer a un equipo no da acceso al detalle ni al chat: el acceso es por asociación explícita, excepto Elizabeth (PRD §6.2.5, §10).
- Un administrador no obtiene acceso general a mensajes y archivos por serlo (PRD §5 regla 5).
- Usuarios y asignaciones futuras se administran dentro de DataTicket (PRD §5).
- Propuestas de esta HU: un interno no tiene empresa; un interno tiene al menos un rol o un equipo; protección contra quedarse sin administradores.

## Invariantes en juego

- Invariante 3 (`AGENTS.md` §7): la pertenencia a un equipo no concede lectura ni escritura del chat; solo la participación vigente (y la PM).
- Invariante 5: el rol `Administrator` no concede contenido de tickets.
- Invariante 1: un usuario interno no tiene `company_id`, así que no entra en el filtro multiempresa del portal.

## Criterios del PRD cubiertos

- Ninguno de forma directa. Prepara PRD CA-03 y PRD CA-04 (la PM y los administradores existen con sus roles) y PRD CA-06 (los equipos no dan acceso al chat) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-003-empresas-usuarios-y-equipos]]
- Dependencias: [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]] (políticas, claims, decisión roles/equipos), [[hu-005-invitar-y-activar-cuentas|HU-005]] (`InviteUser`), [[hu-008-administrar-usuarios-cliente|HU-008]] (endpoints genéricos de desactivar/reactivar), [[hu-011-registrar-eventos-auditables|HU-011]] (`IAuditLog`; HU-011 adelantada al Sprint 1).
- Relacionadas: [[hu-010-datos-sinteticos-de-desarrollo|HU-010]] (siembra el equipo del piloto), [[hu-016-cola-global-de-triage|HU-016]], [[hu-018-asignar-y-agregar-participantes|HU-018]], [[hu-020-detalle-interno-del-ticket|HU-020]] (prueba de regresión: equipo sin participación no ve el detalle), [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]].
- Decisiones: inconsistencia roles frente a equipos ([[pendientes]]); [[modelo-de-dominio]] (propuesta).

## Componentes afectados

- Backend `Domain`: `User` (roles internos, equipos, sin empresa), enumeración de equipos (`Team.Development`, `Team.Production`).
- Backend `Application`: casos de uso de usuarios internos; puertos de cuentas (HU-005), `IAuditLog`, `ICurrentUser`, `IClock`.
- Backend `Infrastructure`: Identity (roles), persistencia de equipos según T-01, sello de seguridad.
- Backend `Api`: `/api/admin/internal-users`.
- Persistencia PostgreSQL: tabla de pertenencia a equipos si T-01 la elige; migración.
- Frontend: módulo de usuarios internos (nombre propuesto `internalUsers`), ruta `/interno/administracion/usuarios-internos` (propuesta).

## Dificultad

**Nivel:** Alto

**Justificación:** Además de atravesar todas las capas, obliga a cerrar la inconsistencia de modelado roles/equipos, introduce reglas de protección del sistema (último administrador, autoprotección) y fija la base de las bandejas por equipo y del acceso al chat.

## Contrato backend ↔ frontend

Propuesta REST; ratificar en T-01. Política `Administrator`; antiforgery en mutaciones; errores `application/problem+json`.

**`GET /api/admin/internal-users?team=Production&role=&status=all&page=1&pageSize=25`**

```json
// 200
{
  "items": [
    { "id": "9a2e…", "displayName": "Elizabeth", "email": "elizabeth@dataticket.local", "roles": ["ProductManager"], "teams": ["Production"], "leads": [], "accountStatus": "Active" },
    { "id": "7c4b…", "displayName": "Julián", "email": "julian@dataticket.local", "roles": [], "teams": ["Production"], "leads": ["Production"], "accountStatus": "Active" }
  ],
  "page": 1, "pageSize": 25, "totalCount": 3
}
```

**`POST /api/admin/internal-users`** → `201` + `Location`; crea la cuenta `Invited` y envía la invitación (HU-005)

```json
// Petición
{ "displayName": "Kevin", "email": "kevin@dataticket.local", "roles": [], "teams": ["Development"], "leads": [] }
```

Un `companyId` en el cuerpo se ignora: los internos no tienen empresa.

**`PUT /api/admin/internal-users/{userId}/membership`** → `200` con el usuario actualizado

```json
{ "roles": ["Administrator"], "teams": ["Development"], "leads": [] }
```

Desactivar/reactivar: `POST /api/admin/users/{userId}/deactivate|reactivate` de [[hu-008-administrar-usuarios-cliente|HU-008]], con las reglas internas de esta HU.

| Código | Cuándo |
|---|---|
| `400` | `roles` con valores fuera de `ProductManager`/`Administrator` (p. ej. `Requester`); `teams` fuera de `Development`/`Production`; `roles` y `teams` vacíos a la vez; `leads` con un equipo al que no pertenece; `displayName` vacío o > 100; `email` inválido o > 256 |
| `401` / `403` | Sin sesión / sin rol `Administrator` |
| `404` | El usuario no existe o es un usuario cliente |
| `409` | Correo ya registrado; el administrador se quita su propio rol `Administrator` o se desactiva; la operación dejaría el sistema sin ningún `Administrator` activo |

## Tareas de desarrollo

- [ ] **T-01 — Contrato y modelado de equipos** · Capa: Transversal · Dificultad: Alto  
  Descripción: decidir y registrar (ADR o [[pendientes]]) si los equipos son roles de Identity o una pertenencia propia `Team.*`; ratificar rutas, DTOs, `leads` (o descartarlo), reglas de protección y acciones de auditoría (propuestas: `InternalUserCreated`, `UserRolesChanged`, `UserTeamsChanged`). Mantener coherente el campo `teams` de `/me` (HU-004).
- [ ] **T-02 — Dominio (pruebas primero)** · Capa: Backend (Domain) · Dificultad: Medio  
  Descripción: pruebas unitarias: un interno no tiene empresa; exige al menos un rol o equipo; no admite perfiles cliente; `leads` ⊆ `teams`; pertenecer a un equipo no cambia ninguna capacidad sobre tickets (el dominio no expone permisos por equipo).
- [ ] **T-03 — Casos de uso (pruebas primero)** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: con dobles: autorización `Administrator` en el caso de uso; autoprotección y último administrador activo → conflicto; correo duplicado → conflicto; `CreateInternalUser` invoca `InviteUser`; auditoría con valores anterior/nuevo de roles y equipos.
- [ ] **T-04 — Identity, persistencia y migración** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: persistir roles y equipos según T-01; actualizar el sello de seguridad al cambiar la pertenencia; claims de equipo coherentes con HU-004. Migración si T-01 introduce tabla de equipos (propuesta: `AddTeamMembership`).
- [ ] **T-05 — Endpoints** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: pruebas de integración primero (CHU-01 a CHU-09); `/api/admin/internal-users` con política `Administrator`; reglas internas aplicadas también en `/api/admin/users/{userId}/deactivate`.
- [ ] **T-06 — Frontend: modelos y gateway** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: pruebas Vitest primero: tipos `InternalUser`, `InternalRole`, `TeamName`; validación local (al menos un rol o equipo, `leads` ⊆ `teams`, nombre y correo); `internalUsersGateway`.
- [ ] **T-07 — Frontend: controladores y vistas** · Capa: Frontend (controllers, views) · Dificultad: Medio  
  Descripción: `useInternalUsersController` (filtros por equipo, rol y estado) y `useInternalUserFormController`; vistas puras con casillas de roles y equipos en español («PM», «Administrador», «Desarrollo», «Producción»); el administrador actual no puede desmarcar su propio rol en la interfaz (además del `409` del backend). Pruebas con Testing Library.
- [ ] **T-08 — Notas para la wiki** · Capa: Documentación · Dificultad: Bajo  
  Descripción: [[equipo-data-global]] (cómo se administran los equipos), [[roles-y-permisos]], [[autenticacion-identity]] (corregir roles/equipos según T-01), [[modelo-de-dominio]], [[glosario]], [[pendientes]] (inconsistencia cerrada).

## Criterios de aceptación

### CHU-01 — Invitar a una persona interna con rol y equipo

**Dado** un administrador  
**Cuando** envía `POST /api/admin/internal-users` con `{ "displayName": "Elizabeth", "email": "elizabeth@dataticket.local", "roles": ["ProductManager"], "teams": ["Production"], "leads": [] }`  
**Entonces** recibe `201` con `accountStatus = "Invited"`; se envía una invitación; y tras activarla, `GET /api/auth/me` muestra `surface = "internal"`, `roles = ["ProductManager"]`, `teams = ["Production"]` y `company = null`.

### CHU-02 — Validación de roles, equipos y límites

**Dado** un administrador  
**Cuando** crea internos con `roles = ["Requester"]`, con `teams = ["QA"]`, con `roles = []` y `teams = []`, con `leads = ["Development"]` y `teams = ["Production"]`, con `displayName` de 101 caracteres y con un correo ya registrado  
**Entonces** los cinco primeros responden `400` con `errors` en el campo correspondiente y el último `409`; ninguno crea cuenta; y un `displayName` de 100 caracteres válido responde `201`.

### CHU-03 — Un interno no tiene empresa (propuesta)

**Dado** la empresa A  
**Cuando** el administrador crea un interno con un cuerpo que incluye `"companyId": "<id de A>"`  
**Entonces** la cuenta se crea sin empresa, su `/api/auth/me` devuelve `company = null` y su principal no contiene el claim `company_id`; y la prueba de dominio confirma que un interno con empresa no puede construirse.

### CHU-04 — Cambiar roles y equipos refresca el acceso

**Dado** Kevin, integrante de Desarrollo, con sesión abierta  
**Cuando** el administrador envía `PUT /api/admin/internal-users/{Kevin}/membership` con `{ "roles": [], "teams": ["Production"], "leads": [] }`  
**Entonces** recibe `200`; y cumplido el intervalo de revalidación del sello (en pruebas, cero), `/api/auth/me` de Kevin muestra `teams = ["Production"]`.

### CHU-05 — Pertenecer a un equipo no da acceso a tickets ni a administración

**Dado** un usuario solo con equipo Desarrollo y otro solo con equipo Producción (sin roles)  
**Cuando** llaman a `/api/admin/internal-users`, `/api/admin/companies` o se evalúan las políticas `Administrator` y `ProductManagerOrAdministrator`  
**Entonces** reciben `403` y las políticas fallan; y [[hu-020-detalle-interno-del-ticket|HU-020]] incluye la prueba de regresión «miembro de equipo sin participación no carga el detalle ni el chat» enlazada a este criterio (PRD §6.2.5).

### CHU-06 — Solo el administrador gestiona internos

**Dado** la PM (sin rol `Administrator`), un miembro de Desarrollo, la Coordinadora de la empresa A y una petición anónima  
**Cuando** llaman a `GET` o `POST /api/admin/internal-users`  
**Entonces** los autenticados reciben `403`, la anónima `401`, y la comprobación existe también en el caso de uso.

### CHU-07 — Autoprotección y último administrador (propuesta)

**Dado** dos administradores activos, A1 y A2  
**Cuando** A1 intenta quitarse el rol `Administrator`, A1 intenta desactivarse a sí mismo, A1 desactiva a A2 y luego A1 vuelve a intentar quitarse el rol  
**Entonces** el primer y segundo intento responden `409`; la desactivación de A2 responde `204`; y el último intento responde `409` «Debe existir al menos un administrador activo.».

### CHU-08 — Listado por equipo con el liderazgo informativo

**Dado** los internos sintéticos Julián (Producción, `leads = ["Production"]`), Elizabeth (`ProductManager`, Producción), Gerardo (Producción) y Kevin (Desarrollo)  
**Cuando** el administrador consulta `GET /api/admin/internal-users?team=Production`  
**Entonces** obtiene exactamente Julián, Elizabeth y Gerardo; Elizabeth figura con `roles = ["ProductManager"]` y Julián con `leads = ["Production"]`; y `leads` no aparece en ninguna política de autorización.

### CHU-09 — Auditoría de cambios de roles y equipos (propuesta)

**Dado** un administrador con id X  
**Cuando** crea a Kevin en Desarrollo y luego lo cambia a Producción con rol `Administrator`  
**Entonces** existen `AuditEntry` con actor X, fecha/hora UTC, acciones `InternalUserCreated`, `UserTeamsChanged` (`["Development"]` → `["Production"]`) y `UserRolesChanged` (`[]` → `["Administrator"]`), objeto `User:{id de Kevin}` y resultado exitoso.

### CHU-10 — La interfaz refleja roles y equipos y maneja errores

**Dado** la vista `/interno/administracion/usuarios-internos`  
**Cuando** el administrador abre su propio usuario, o el backend responde `409` al guardar  
**Entonces** la casilla «Administrador» de su propio usuario aparece deshabilitada con la explicación «No puedes quitarte tu propio rol de administrador.»; el `409` se muestra con su `title` sin perder los cambios del formulario; y los equipos y roles se muestran en español.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-10 validados con evidencia (CHU-03, CHU-07 y CHU-09 según lo que ratifique T-01).
- [ ] **DoD-02** — Pruebas escritas primero y en verde, con nombres por comportamiento: `Crear_interno_con_rol_y_equipo_e_invitar`, `Interno_rechaza_perfil_cliente_y_equipo_desconocido`, `Interno_exige_al_menos_un_rol_o_equipo`, `Interno_no_tiene_empresa_ni_claim_company_id`, `Cambio_de_equipo_refresca_claims`, `Miembro_de_equipo_no_satisface_politicas_de_PM_ni_admin`, `Solo_Administrator_gestiona_internos`, `Administrador_no_se_quita_su_rol_ni_se_desactiva`, `Siempre_queda_un_administrador_activo`, `Cambios_de_roles_y_equipos_quedan_auditados`.
- [ ] **DoD-03** — Pruebas Vitest del módulo de usuarios internos en verde.
- [ ] **DoD-04** — `cd backend && dotnet test` en verde, incluidas `DataTicket.ArchitectureTests`.
- [ ] **DoD-05** — `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build` en verde.
- [ ] **DoD-06** — Migración EF Core creada, aplicada en local y revisada si T-01 introduce persistencia propia de equipos; si no, documentado que no hubo cambio de esquema.
- [ ] **DoD-07** — Decisión roles frente a equipos registrada y [[autenticacion-identity]], [[glosario]] y [[roles-y-permisos]] coherentes entre sí.
- [ ] **DoD-08** — `docker compose up --build`: invitación de un interno, activación y edición de equipos verificadas en `http://localhost:5173` (documentado en el PR).
- [ ] **DoD-09** — Wiki actualizada vía Notas para la wiki: [[equipo-data-global]], [[roles-y-permisos]], [[autenticacion-identity]], [[modelo-de-dominio]], [[glosario]], [[pendientes]], [[auditoria]].
- [ ] **DoD-10** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (equipos sin acceso a tickets, autoprotección, autorización en caso de uso).
- [ ] **DoD-11** — PR revisado y aprobado por otra persona del equipo; trazabilidad de HU y épica actualizada.

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
| DoD-11 | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- **Riesgo — roles frente a equipos**: si los equipos fueran roles de Identity, una política mal escrita podría conceder acceso por equipo; separar `roles` de `teams` en el contrato reduce ese riesgo. Decisión en T-01, sin resolver aquí.
- **`leads`** (liderazgo de equipo) es propuesta: el PRD dice que Julián lidera Producción y la revisión de PR (PRD §6.3), pero no le asocia permisos. Si no aporta, se descarta en T-01.
- **¿Más de una PM?** El PRD habla de Elizabeth; no prohíbe varias cuentas `ProductManager`. Esta HU no impone unicidad.
- Autoprotección y último administrador son propuestas para evitar dejar el sistema sin administración.

## Relacionado

- [[ep-003-empresas-usuarios-y-equipos]] · [[tablero-scrum]] · [[equipo-data-global]] · [[elizabeth-pm]] · [[julian-produccion]] · [[roles-y-permisos]] · [[autenticacion-identity]] · [[modelo-de-dominio]] · [[glosario]] · [[pendientes]]
