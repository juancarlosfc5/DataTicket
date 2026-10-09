---
title: "HU-008 — Administrar usuarios de empresas cliente"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/roles, seguridad, identity, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.1", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-008", "Administrar usuarios de empresas cliente", "Administrar usuarios cliente"]
epica: "[[ep-003-empresas-usuarios-y-equipos]]"
criterios_prd: ["CA-01"]
componentes: ["Backend (Domain: User)", "Backend (Application: casos de uso de usuarios cliente)", "Backend (Infrastructure: Identity, EF Core)", "Backend (Api: /api/admin/companies/{companyId}/users, /api/admin/users)", "Persistencia PostgreSQL", "Frontend (models, controllers, views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 1"
dependencias: ["[[hu-007-administrar-empresas-cliente]]", "[[hu-005-invitar-y-activar-cuentas]]", "[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-011-registrar-eventos-auditables]]"]
relacionadas: ["[[hu-009-administrar-usuarios-internos-y-equipos]]", "[[hu-003-iniciar-y-cerrar-sesion]]", "[[hu-010-datos-sinteticos-de-desarrollo]]", "[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-025-portal-coordinador-consulta-tickets]]", "[[hu-032-revocar-acceso-al-retirar-participante]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-008 — Administrar usuarios de empresas cliente

El administrador da de alta (por invitación) a los usuarios de una empresa cliente con perfil Solicitante (`Requester`) o Coordinador (`CompanyCoordinator`), cambia su perfil, los desactiva y los reactiva. Cada usuario cliente pertenece a exactamente una empresa; desactivarlo revoca su acceso.

## Historia de usuario

**COMO** administrador de DataTicket  
**QUIERO** invitar usuarios a una empresa cliente como Solicitantes o Coordinadores, cambiar su perfil y desactivarlos cuando corresponda  
**PARA** que cada persona del cliente vea exactamente lo que su perfil y su empresa le permiten, y deje de tener acceso en cuanto Data Global lo decida

## Contexto

PRD §5: el Solicitante crea tickets y consulta los propios; el Coordinador consulta los de toda su empresa y puede radicar; «las cuentas de empresas cliente las crea, invita y desactiva Data Global». PRD §6.1: «La cuenta determina la empresa y el usuario que radica». [[modelo-de-dominio]] propone que un usuario cliente pertenece a una sola empresa. El mecanismo de invitación está en [[hu-005-invitar-y-activar-cuentas|HU-005]]; las empresas, en [[hu-007-administrar-empresas-cliente|HU-007]].

## Alcance

- Vinculación de usuarios cliente a exactamente una `Company` (columna de empresa en la cuenta, clave foránea).
- Casos de uso (nombres propuestos, ratificar en T-01 y registrar en el [[glosario]]): `CreateClientUser` (crea la cuenta en estado `Invited` y dispara `InviteUser`), `ChangeClientUserProfile`, `DeactivateUser`, `ReactivateUser`, `ListCompanyUsers`.
- Endpoints bajo `/api/admin/companies/{companyId}/users` y operaciones genéricas de cuenta en `/api/admin/users/{userId}/deactivate|reactivate` (reutilizadas por [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]]).
- Revocación del acceso al desactivar: inicio de sesión rechazado y sesiones vigentes invalidadas (sello de seguridad).
- Refresco de claims al cambiar de perfil.
- Auditoría de alta, cambio de perfil, desactivación y reactivación (propuesta).
- Frontend: gestión de usuarios dentro del detalle de la empresa, con reenvío de invitación (contrato de HU-005).

## Fuera de alcance

- Usuarios internos, roles `ProductManager`/`Administrator` y equipos ([[hu-009-administrar-usuarios-internos-y-equipos|HU-009]]).
- Mover un usuario a otra empresa (propuesta: no permitido; se desactiva y se crea otra cuenta).
- Que el Coordinador administre usuarios de su empresa (el PRD asigna esa tarea a Data Global).
- Borrado físico de cuentas.
- Desconexión de conexiones SignalR del usuario desactivado: se integra cuando exista el hub ([[hu-032-revocar-acceso-al-retirar-participante|HU-032]]).

## Requisitos y reglas de negocio

- Perfiles cliente: Solicitante (sus tickets) y Coordinador (tickets de toda su empresa, puede radicar) (PRD §5).
- Los usuarios cliente solo consultan tickets de la empresa a la que pertenece su cuenta (PRD §5 regla 1).
- La cuenta determina la empresa; no se elige libremente (PRD §6.1).
- Data Global crea, invita y desactiva las cuentas de empresas cliente; no hay registro público (PRD §5).
- Propuestas de esta HU: un usuario cliente tiene exactamente un perfil cliente; el correo es único en todo el sistema; no se crea un usuario en una empresa inactiva; nombre visible de 1 a 100 caracteres; auditoría de acciones administrativas.

## Invariantes en juego

- Invariante 1 (`AGENTS.md` §7): cada usuario cliente pertenece a exactamente una empresa, fijada por Data Global; la empresa del claim `company_id` ([[hu-004-contexto-de-usuario-y-autorizacion|HU-004]]) sale de esta vinculación.
- Invariante 9: no hay registro público; el alta es siempre por invitación del administrador.
- Invariante 8: si se acepta auditar, entradas append-only con actor, fecha, acción, objeto y valores anterior/nuevo.

## Criterios del PRD cubiertos

- PRD CA-01 (parcial): vinculación única usuario cliente ↔ empresa y pruebas negativas A↔B en la administración; las pruebas sobre tickets llegan con [[hu-024-portal-solicitante-consulta-tickets|HU-024]] y [[hu-025-portal-coordinador-consulta-tickets|HU-025]] → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-003-empresas-usuarios-y-equipos]]
- Dependencias: [[hu-007-administrar-empresas-cliente|HU-007]] (empresas), [[hu-005-invitar-y-activar-cuentas|HU-005]] (`InviteUser`), [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]] (política `Administrator`, claim `company_id`), [[hu-011-registrar-eventos-auditables|HU-011]] (`IAuditLog`; HU-011 adelantada al Sprint 1).
- Relacionadas: [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]], [[hu-003-iniciar-y-cerrar-sesion|HU-003]], [[hu-010-datos-sinteticos-de-desarrollo|HU-010]], [[hu-024-portal-solicitante-consulta-tickets|HU-024]], [[hu-025-portal-coordinador-consulta-tickets|HU-025]], [[hu-032-revocar-acceso-al-retirar-participante|HU-032]].
- Decisiones: [[adr-0004-autenticacion-cookie-mismo-origen]] (propuesta); [[modelo-de-dominio]] (propuesta).

## Componentes afectados

- Backend `Domain`: `User` (empresa obligatoria para perfiles cliente; sin empresa para internos).
- Backend `Application`: casos de uso de usuarios cliente; puertos de cuentas (HU-005), `ICompanyRepository`, `IAuditLog`, `ICurrentUser`, `IClock`.
- Backend `Infrastructure`: `ApplicationUser` con referencia a la empresa, asignación de roles cliente, sello de seguridad.
- Backend `Api`: endpoints administrativos.
- Persistencia PostgreSQL: columna y clave foránea de empresa en la tabla de usuarios; migración.
- Frontend: módulo de usuarios cliente (nombre propuesto `clientUsers`), ruta `/interno/administracion/empresas/{companyId}/usuarios` (propuesta).

## Dificultad

**Nivel:** Alto

**Justificación:** Toca dominio, Identity (roles, sello de seguridad, claims), persistencia con migración, API y frontend; contiene reglas de seguridad críticas (vinculación única a empresa, revocación de acceso, pruebas A↔B) y depende de tres HU del mismo sprint o posteriores.

## Contrato backend ↔ frontend

Propuesta REST; ratificar en T-01. Política `Administrator` en todas las rutas; antiforgery en mutaciones; errores `application/problem+json`.

**`GET /api/admin/companies/{companyId}/users?status=all&page=1&pageSize=25`**

```json
// 200
{
  "items": [
    {
      "id": "6f1c2a9e-4c1d-4b8e-9a51-1f0e2d3c4b5a",
      "displayName": "Ana Pérez",
      "email": "ana.perez@empresa-sintetica-01.test",
      "profile": "Requester",
      "accountStatus": "Active",
      "createdAt": "2026-10-07T14:05:00Z"
    }
  ],
  "page": 1, "pageSize": 25, "totalCount": 3
}
```

**`POST /api/admin/companies/{companyId}/users`** → `201` + `Location`; crea la cuenta en `Invited` y envía la invitación (HU-005)

```json
// Petición
{ "displayName": "Carlos Ruiz", "email": "carlos.ruiz@empresa-sintetica-01.test", "profile": "CompanyCoordinator" }
// 201
{ "id": "c2d3…", "displayName": "Carlos Ruiz", "email": "carlos.ruiz@empresa-sintetica-01.test", "profile": "CompanyCoordinator", "accountStatus": "Invited", "createdAt": "2026-10-07T15:20:00Z" }
```

Un `companyId` dentro del cuerpo se ignora: la empresa es siempre la de la ruta.

**`PUT /api/admin/companies/{companyId}/users/{userId}/profile`** → `200` con el usuario actualizado

```json
{ "profile": "Requester" }
```

**`POST /api/admin/users/{userId}/deactivate`** → `204` · **`POST /api/admin/users/{userId}/reactivate`** → `204` (genéricos para cualquier cuenta; HU-009 añade reglas para internos)

Reenvío de invitación: `POST /api/auth/invitations` de [[hu-005-invitar-y-activar-cuentas|HU-005]].

| Código | Cuándo |
|---|---|
| `400` | `displayName` vacío o de más de 100 caracteres; `email` vacío, inválido o de más de 256; `profile` distinto de `Requester`/`CompanyCoordinator` |
| `401` / `403` | Sin sesión / sin rol `Administrator` |
| `404` | La empresa no existe, o el `userId` no pertenece a la empresa de la ruta |
| `409` | Correo ya registrado; empresa inactiva al crear o reactivar; desactivar una cuenta ya desactivada o reactivar una no desactivada |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: ratificar rutas, DTOs, límites, estado al reactivar (propuesta: vuelve a `Active` si ya tenía contraseña, a `Invited` si no), nombres de casos de uso y de acciones de auditoría (propuestas: `ClientUserCreated`, `UserProfileChanged`, `UserDeactivated`, `UserReactivated`) y `AccountStatus`. Confirmar que el administrador no puede cambiar la empresa de un usuario.
- [ ] **T-02 — Dominio (pruebas primero)** · Capa: Backend (Domain) · Dificultad: Medio  
  Descripción: pruebas unitarias de `User`: perfil cliente exige empresa; un usuario cliente no puede tener a la vez `Requester` y `CompanyCoordinator` ni roles internos; transiciones `Invited → Active → Deactivated → Active` y `Invited → Deactivated → Invited`.
- [ ] **T-03 — Casos de uso (pruebas primero)** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: con dobles: autorización `Administrator` en el caso de uso; empresa inexistente → 404, inactiva → 409; correo duplicado → 409; `CreateClientUser` invoca `InviteUser` una vez; usuario de otra empresa → 404; auditoría con valores anterior/nuevo.
- [ ] **T-04 — Identity, persistencia y migración** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: referencia a la empresa en `ApplicationUser` con clave foránea e índice; asignación del rol cliente; actualizar el sello de seguridad al cambiar de perfil y al desactivar; el login rechaza cuentas `Deactivated` (HU-003). Migración (propuesta: `AddClientUserCompany`).
- [ ] **T-05 — Endpoints** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: pruebas de integración primero (CHU-01 a CHU-09); endpoints con política `Administrator`, antiforgery y mapeo de errores.
- [ ] **T-06 — Frontend: modelos y gateway** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: pruebas Vitest primero: tipos `ClientUser`, `ClientProfile`, `AccountStatus`; validación local (nombre 1–100, correo con formato y ≤ 256, perfil válido); `clientUsersGateway` (`list`, `create`, `changeProfile`, `deactivate`, `reactivate`) y reenvío con `invitationGateway.resend` de HU-005.
- [ ] **T-07 — Frontend: controladores y vistas** · Capa: Frontend (controllers, views) · Dificultad: Medio  
  Descripción: `useClientUsersController` y `useInviteClientUserController`; vistas puras de listado (perfil y estado legibles en español), formulario de invitación, selector de perfil y confirmación de desactivación; acción «Reenviar invitación» solo para `Invited`. Pruebas con Testing Library.
- [ ] **T-08 — Notas para la wiki** · Capa: Documentación · Dificultad: Bajo  
  Descripción: [[modelo-de-dominio]] (`User`↔`Company`, estado de la cuenta), [[roles-y-permisos]] (ciclo de vida de cuentas cliente), [[autenticacion-identity]] (revocación por sello de seguridad), [[persistencia-postgresql]], [[auditoria]], [[glosario]].

## Criterios de aceptación

### CHU-01 — Invitar a un usuario cliente

**Dado** la empresa activa A y un administrador  
**Cuando** envía `POST /api/admin/companies/{A}/users` con `{ "displayName": "Carlos Ruiz", "email": "carlos.ruiz@empresa-sintetica-01.test", "profile": "CompanyCoordinator" }`  
**Entonces** recibe `201` con `accountStatus = "Invited"`; la cuenta queda vinculada a A con el rol `CompanyCoordinator`; se envía exactamente una invitación (doble de `IEmailSender` o Mailpit); y tras activarla ([[hu-005-invitar-y-activar-cuentas|HU-005]]) su `GET /api/auth/me` muestra `company.id = A`.

### CHU-02 — No hay usuario cliente sin empresa ni con perfil interno

**Dado** un administrador  
**Cuando** intenta crear un usuario en `/api/admin/companies/{empresa inexistente}/users`, con `profile = "ProductManager"` o `"Administrator"`, o un usuario cliente mediante el endpoint de internos de [[hu-009-administrar-usuarios-internos-y-equipos|HU-009]]  
**Entonces** recibe `404`, `400` y `400` respectivamente; ninguna cuenta se crea; y la prueba de dominio confirma que un `User` con perfil cliente y sin empresa no puede construirse.

### CHU-03 — La empresa sale de la ruta, nunca del cuerpo

**Dado** las empresas A y B  
**Cuando** el administrador envía `POST /api/admin/companies/{A}/users` con un cuerpo que además incluye `"companyId": "<id de B>"`  
**Entonces** la cuenta creada pertenece a A y no a B.

### CHU-04 — Validación con valores límite

**Dado** la empresa activa A  
**Cuando** se crean usuarios con `displayName` de 100 y de 101 caracteres, con `email` de 257 caracteres, con `email = "carlos.ruiz"` y con un correo ya registrado en la empresa B  
**Entonces** el de 100 caracteres responde `201`; los de 101 caracteres, 257 caracteres y formato inválido responden `400` con `errors` en el campo; el duplicado responde `409` «Ya existe una cuenta con ese correo.»; y crear en una empresa inactiva responde `409`.

### CHU-05 — Cambiar el perfil actualiza el acceso

**Dado** el Solicitante Ana de la empresa A con sesión abierta  
**Cuando** el administrador envía `PUT /api/admin/companies/{A}/users/{Ana}/profile` con `{ "profile": "CompanyCoordinator" }`  
**Entonces** recibe `200`; la cuenta tiene solo el rol `CompanyCoordinator`; y, cumplido el intervalo de revalidación del sello de seguridad (en pruebas, cero), `GET /api/auth/me` de Ana muestra `roles = ["CompanyCoordinator"]`.

### CHU-06 — Prueba negativa A↔B en la administración

**Dado** el usuario Luis de la empresa B  
**Cuando** el administrador llama a `PUT /api/admin/companies/{A}/users/{Luis}/profile` o lista `/api/admin/companies/{A}/users`  
**Entonces** el cambio responde `404` y no modifica a Luis; el listado de A no contiene a Luis; y la Coordinadora de A que llama a `GET /api/admin/companies/{A}/users` o `/{B}/users` recibe `403`.

### CHU-07 — Desactivar revoca el acceso

**Dado** el Solicitante Ana activa con una sesión abierta (cookie C1)  
**Cuando** el administrador envía `POST /api/admin/users/{Ana}/deactivate`  
**Entonces** recibe `204`; `accountStatus = "Deactivated"`; el inicio de sesión de Ana responde `401` genérico de HU-003; una petición con C1, cumplido el intervalo de revalidación (en pruebas, cero), responde `401`; sus datos y tickets se conservan; y desactivarla de nuevo responde `409`.

### CHU-08 — Reactivar restaura el estado previo

**Dado** Ana desactivada (había activado su cuenta) y Pedro desactivado sin haber aceptado nunca su invitación  
**Cuando** el administrador reactiva a ambos  
**Entonces** Ana queda `Active` y vuelve a iniciar sesión con su contraseña; Pedro queda `Invited` y necesita una invitación nueva; y reactivar a un usuario de una empresa inactiva responde `409`.

### CHU-09 — Auditoría de acciones sobre usuarios (propuesta)

**Dado** un administrador con id X  
**Cuando** crea a Carlos, le cambia el perfil de `CompanyCoordinator` a `Requester` y lo desactiva  
**Entonces** existen tres `AuditEntry` con actor X, fecha/hora UTC, acciones `ClientUserCreated`, `UserProfileChanged`, `UserDeactivated`, objeto `User:{id de Carlos}` y valores anterior/nuevo (`null` → `CompanyCoordinator`/A; `CompanyCoordinator` → `Requester`; `Invited` → `Deactivated`), sin contraseñas ni tokens en ningún valor.

### CHU-10 — La interfaz guía y maneja errores

**Dado** la vista de usuarios de la empresa A  
**Cuando** el administrador invita con un correo ya registrado, o pulsa «Desactivar»  
**Entonces** ve «Ya existe una cuenta con ese correo.» junto al campo y conserva lo escrito; la desactivación pide confirmación («La persona perderá el acceso de inmediato.»); el perfil y el estado se muestran en español (Solicitante/Coordinador; Invitación pendiente/Activa/Desactivada); y «Reenviar invitación» solo aparece para cuentas `Invited`.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-10 validados con evidencia (CHU-09 solo si se acepta auditar acciones administrativas).
- [ ] **DoD-02** — Pruebas escritas primero y en verde, con nombres por comportamiento: `Crear_usuario_cliente_lo_vincula_a_la_empresa_de_la_ruta_e_invita`, `Usuario_cliente_sin_empresa_no_puede_existir`, `Perfil_interno_no_se_acepta_para_cliente`, `CompanyId_del_cuerpo_se_ignora`, `Validacion_displayName_100_y_101`, `Correo_duplicado_responde_409`, `Cambio_de_perfil_refresca_claims`, `Usuario_de_B_no_se_modifica_por_ruta_de_A`, `Coordinador_no_administra_usuarios`, `Desactivar_usuario_revoca_login_y_sesion`, `Reactivar_restaura_Active_o_Invited`, `Acciones_sobre_usuarios_quedan_auditadas`.
- [ ] **DoD-03** — Pruebas Vitest del módulo de usuarios cliente en verde.
- [ ] **DoD-04** — `cd backend && dotnet test` en verde, incluidas `DataTicket.ArchitectureTests`.
- [ ] **DoD-05** — `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build` en verde.
- [ ] **DoD-06** — Migración EF Core (empresa del usuario, clave foránea e índice) creada, aplicada en local y revisada.
- [ ] **DoD-07** — `docker compose up --build`: alta, invitación en Mailpit, activación, cambio de perfil y desactivación verificados manualmente en `http://localhost:5173` (documentado en el PR).
- [ ] **DoD-08** — Wiki actualizada vía Notas para la wiki: [[modelo-de-dominio]], [[roles-y-permisos]], [[autenticacion-identity]], [[persistencia-postgresql]], [[auditoria]], [[glosario]].
- [ ] **DoD-09** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (vinculación a empresa, A↔B, revocación).
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
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- **Revocación no instantánea**: con cookies, la desactivación se hace efectiva al revalidar el sello de seguridad. El intervalo se ratifica en T-01 de HU-003; si debe ser inmediata, hace falta comprobar el estado en cada petición o un almacén de sesiones en servidor.
- **Conexiones SignalR** del usuario desactivado: cuando exista el hub, la desactivación debe cortarlas (mismo mecanismo que `IChatConnectionRevoker` de [[hu-032-revocar-acceso-al-retirar-participante|HU-032]]). Registrar como dependencia futura.
- **Cambio de perfil Coordinador → Solicitante**: deja de ver los tickets de otros de su empresa en cuanto se refrescan los claims; el PRD no lo trata.
- Propuestas no respaldadas por el PRD: unicidad global del correo, límite de 100 caracteres del nombre, prohibición de mover usuarios entre empresas, auditoría de acciones administrativas.

## Relacionado

- [[ep-003-empresas-usuarios-y-equipos]] · [[tablero-scrum]] · [[roles-y-permisos]] · [[modelo-de-dominio]] · [[autenticacion-identity]] · [[auditoria]] · [[criterios-de-aceptacion]] · [[glosario]]
