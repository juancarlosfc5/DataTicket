---
title: "HU-007 — Administrar empresas cliente"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/roles, dominio, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §12", "PRD.md §13"]
aliases: ["HU-007", "Administrar empresas cliente"]
epica: "[[ep-003-empresas-usuarios-y-equipos]]"
criterios_prd: []
componentes: ["Backend (Domain: Company)", "Backend (Application: casos de uso de empresas)", "Backend (Infrastructure: EF Core)", "Backend (Api: /api/admin/companies)", "Persistencia PostgreSQL", "Identity", "Frontend (models, controllers, views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 1"
dependencias: ["[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-002-shell-y-navegacion-por-rol]]", "[[hu-011-registrar-eventos-auditables]]"]
relacionadas: ["[[hu-008-administrar-usuarios-cliente]]", "[[hu-010-datos-sinteticos-de-desarrollo]]", "[[hu-003-iniciar-y-cerrar-sesion]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-007 — Administrar empresas cliente

El administrador crea, renombra, lista, desactiva y reactiva las empresas cliente de Data Global. Desactivar una empresa impide que sus usuarios inicien sesión (propuesta) sin borrar sus tickets ni su historial.

## Historia de usuario

**COMO** administrador de DataTicket  
**QUIERO** crear, renombrar, consultar, desactivar y reactivar empresas cliente  
**PARA** que cada usuario cliente pueda vincularse a su empresa y que Data Global controle qué organizaciones tienen acceso al portal

## Contexto

PRD §12: «Cada ticket pertenece a una empresa cliente» y toda consulta del portal se restringe por empresa. PRD §5: las cuentas de empresas cliente las crea, invita y desactiva Data Global, y «usuarios y asignaciones futuras se administran dentro de DataTicket». [[modelo-de-dominio]] propone el agregado `Company` (`Id`, `Name`, `IsActive`). Hoy no existe ninguna entidad de dominio ni migración de negocio. Esta HU es requisito de [[hu-008-administrar-usuarios-cliente|HU-008]], que vincula usuarios a una empresa.

## Alcance

- Entidad `Company` en `Domain` con nombre normalizado (sin espacios al inicio/fin), estado activo y fecha de creación.
- Casos de uso (nombres propuestos, ratificar en T-01 y registrar en el [[glosario]]): `CreateCompany`, `RenameCompany`, `DeactivateCompany`, `ReactivateCompany`, `ListCompanies`.
- Puerto de salida `ICompanyRepository` (nombre propuesto) y adaptador EF Core; migración con índice único sobre el nombre normalizado sin distinguir mayúsculas (propuesta).
- Endpoints `/api/admin/companies` con política `Administrator`.
- Efecto de la desactivación sobre el acceso: los usuarios de una empresa inactiva no inician sesión y sus sesiones vigentes se invalidan (propuesta).
- Entrada de auditoría por cada alta, cambio de nombre, desactivación y reactivación (propuesta; usa `IAuditLog`).
- Frontend: listado con búsqueda, filtro por estado y paginación; formulario de alta/edición; confirmación antes de desactivar.

## Fuera de alcance

- Usuarios de la empresa ([[hu-008-administrar-usuarios-cliente|HU-008]]).
- Borrado físico de empresas (propuesta: no se borra, para conservar tickets y auditoría).
- Datos adicionales de la empresa (NIT, dirección, contactos): el PRD no los define.
- Plantillas de formulario por empresa ([[ep-013-formularios-configurables]]) y catálogo de categorías (V-12).
- Que un cliente vea el listado de empresas.

## Requisitos y reglas de negocio

- Cada ticket pertenece a exactamente una empresa cliente (PRD §12).
- Data Global crea, invita y desactiva las cuentas de empresas cliente (PRD §5).
- Usuarios y asignaciones futuras se administran dentro de DataTicket (PRD §5).
- Los usuarios cliente solo consultan tickets de su empresa (PRD §5 regla 1): un cliente no puede conocer el nombre de otras empresas.
- Propuestas de esta HU (no están en el PRD): nombre obligatorio de 1 a 150 caracteres tras recortar espacios; único sin distinguir mayúsculas; desactivar bloquea el acceso de sus usuarios; no hay borrado físico; las acciones administrativas se auditan.

## Invariantes en juego

- Invariante 1 (`AGENTS.md` §7): el aislamiento multiempresa empieza en que la empresa es un dato administrado por Data Global; el listado de empresas no se expone a clientes.
- Invariante 8: si la propuesta de auditar acciones administrativas se acepta, las entradas son append-only con actor, fecha, acción, objeto y valores anterior/nuevo.

## Criterios del PRD cubiertos

- Ninguno de forma directa. Prepara PRD CA-01 (la empresa que delimita el aislamiento) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-003-empresas-usuarios-y-equipos]]
- Dependencias: [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]] (política `Administrator`, `ICurrentUser`), [[hu-002-shell-y-navegacion-por-rol|HU-002]] (shell del Espacio interno), [[hu-011-registrar-eventos-auditables|HU-011]] (puerto `IAuditLog` y tabla `AuditEntry`; HU-011 se adelantó al Sprint 1 para resolver el orden).
- Relacionadas: [[hu-008-administrar-usuarios-cliente|HU-008]], [[hu-010-datos-sinteticos-de-desarrollo|HU-010]] (siembra ~50 empresas), [[hu-003-iniciar-y-cerrar-sesion|HU-003]] (rechazo de login).
- Decisiones: ninguna aceptada específica; [[modelo-de-dominio]] (propuesta).

## Componentes afectados

- Backend `Domain`: `Company`.
- Backend `Application`: casos de uso de empresas, puertos `ICompanyRepository`, `IAuditLog`, `ICurrentUser`, `IClock`.
- Backend `Infrastructure`: configuración EF Core de `Company`, repositorio, invalidación de sesiones de usuarios de la empresa.
- Backend `Api`: grupo `/api/admin/companies`.
- Persistencia PostgreSQL: tabla de empresas e índice único.
- Identity: comprobación de empresa activa al iniciar sesión.
- Frontend: módulo `companies` (nombre propuesto) en el Espacio interno, ruta `/interno/administracion/empresas` (propuesta).

## Dificultad

**Nivel:** Medio

**Justificación:** CRUD acotado con reglas claras, pero atraviesa todas las capas, introduce la primera migración de negocio y conecta con Identity (bloqueo de acceso) y con la auditoría, que podría no estar lista en el mismo sprint.

## Contrato backend ↔ frontend

Propuesta REST; ratificar en T-01. Todas las rutas exigen política `Administrator`; las mutaciones, cabecera antiforgery. Errores `application/problem+json`.

**`GET /api/admin/companies?q=sintetica&status=active&page=1&pageSize=25`**

```json
// 200
{
  "items": [
    { "id": "0b7d6c1e-2f3a-4b5c-8d9e-0a1b2c3d4e5f", "name": "Empresa Sintética 01", "isActive": true, "userCount": 3, "createdAt": "2026-10-07T14:00:00Z" }
  ],
  "page": 1,
  "pageSize": 25,
  "totalCount": 50
}
```

- `status` ∈ `active` | `inactive` | `all` (por defecto `all`); `pageSize` de 1 a 100 (por defecto 25); orden por nombre ascendente.

**`POST /api/admin/companies`** → `201` + `Location: /api/admin/companies/{id}`

```json
// Petición
{ "name": "Empresa Sintética 51" }
// 201
{ "id": "5a1f…", "name": "Empresa Sintética 51", "isActive": true, "userCount": 0, "createdAt": "2026-10-07T15:10:00Z" }
```

**`GET /api/admin/companies/{companyId}`** → `200` con el mismo objeto.

**`PUT /api/admin/companies/{companyId}`** → `200` con el objeto actualizado

```json
{ "name": "Empresa Sintética 51 S.A.S." }
```

**`POST /api/admin/companies/{companyId}/deactivate`** → `204` · **`POST /api/admin/companies/{companyId}/reactivate`** → `204`

| Código | Cuándo |
|---|---|
| `400` | `name` vacío o solo espacios, de más de 150 caracteres tras recortar; `pageSize` fuera de 1..100; `status` desconocido |
| `401` / `403` | Sin sesión / sin rol `Administrator` |
| `404` | La empresa no existe |
| `409` | Nombre duplicado (sin distinguir mayúsculas ni espacios extremos); desactivar una empresa ya inactiva o reactivar una activa |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar rutas, DTOs, límites (150 caracteres, `pageSize` máximo 100), unicidad, efectos de la desactivación, nombres de casos de uso y acciones de auditoría (propuestas: `CompanyCreated`, `CompanyRenamed`, `CompanyDeactivated`, `CompanyReactivated`). Si [[hu-011-registrar-eventos-auditables|HU-011]] no está lista, acordar adelantar el mínimo de `IAuditLog`/`AuditEntry` o diferir CHU-07.
- [ ] **T-02 — Dominio (pruebas primero)** · Capa: Backend (Domain) · Dificultad: Bajo  
  Descripción: pruebas unitarias de `Company`: recorta espacios, rechaza vacío y 151 caracteres, acepta 150; `Deactivate()` sobre inactiva y `Reactivate()` sobre activa lanzan error de dominio.
- [ ] **T-03 — Casos de uso (pruebas primero)** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: con dobles de `ICompanyRepository`, `IAuditLog`, `ICurrentUser` e `IClock`: autorización `Administrator` en el caso de uso (no solo en el endpoint), duplicados → conflicto, una entrada de auditoría por acción con valores anterior/nuevo, consulta paginada.
- [ ] **T-04 — Persistencia y migración** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: configuración EF Core de `Company` (`timestamptz` en UTC), índice único sobre el nombre normalizado en minúsculas, repositorio; migración (propuesta: `AddCompanies`).
- [ ] **T-05 — Efecto sobre el acceso** · Capa: Backend (Infrastructure/Identity) · Dificultad: Medio  
  Descripción: el inicio de sesión rechaza (401 genérico de HU-003) a usuarios de empresas inactivas; al desactivar, se actualiza el sello de seguridad de los usuarios de la empresa para invalidar sus sesiones (propuesta). Pruebas de integración.
- [ ] **T-06 — Endpoints** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: pruebas de integración primero (CHU-01 a CHU-09); grupo `/api/admin/companies` con política `Administrator`, antiforgery en mutaciones y mapeo de errores de dominio a 400/404/409.
- [ ] **T-07 — Frontend: modelos y gateway** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: pruebas Vitest primero: tipos `Company` y `CompanyPage`, validación local del nombre (1–150 tras recortar), `companiesGateway` (`list`, `get`, `create`, `rename`, `deactivate`, `reactivate`).
- [ ] **T-08 — Frontend: controladores y vistas** · Capa: Frontend (controllers, views) · Dificultad: Medio  
  Descripción: `useCompanyListController` (búsqueda, filtro, paginación, recarga tras mutación) y `useCompanyFormController`; vistas puras `CompanyListView`, `CompanyFormView` y diálogo de confirmación de desactivación. Pruebas con Testing Library.
- [ ] **T-09 — Notas para la wiki** · Capa: Documentación · Dificultad: Bajo  
  Descripción: [[modelo-de-dominio]] (`Company` implementado), [[persistencia-postgresql]] (migración), [[roles-y-permisos]] (administración de empresas), [[auditoria]] (eventos administrativos, si se aceptan), [[glosario]] (casos de uso).

## Criterios de aceptación

### CHU-01 — Alta de empresa

**Dado** un administrador con sesión iniciada  
**Cuando** envía `POST /api/admin/companies` con `{ "name": "  Empresa Sintética 51  " }`  
**Entonces** recibe `201` con `Location`, `name = "Empresa Sintética 51"` (espacios recortados), `isActive = true`, `userCount = 0`; y la empresa aparece en `GET /api/admin/companies?q=51`.

### CHU-02 — Validación del nombre con valores límite

**Dado** un administrador  
**Cuando** crea empresas con un nombre de 150 caracteres, uno de 151, uno vacío y uno de solo espacios  
**Entonces** el de 150 responde `201` y los otros tres responden `400` con `errors.name`; lo mismo aplica a `PUT /api/admin/companies/{companyId}`.

### CHU-03 — Nombres únicos

**Dado** la empresa existente «Empresa Sintética 01»  
**Cuando** el administrador crea « empresa sintética 01 » o renombra otra empresa a «EMPRESA SINTÉTICA 01»  
**Entonces** ambas operaciones responden `409` «Ya existe una empresa con ese nombre.» y no cambian datos.

### CHU-04 — Solo el administrador administra empresas (A↔B)

**Dado** la Coordinadora de la empresa A, un Solicitante de A, la PM, un miembro del equipo Desarrollo y una petición anónima  
**Cuando** llaman a `GET /api/admin/companies`, `GET /api/admin/companies/{id de B}` o `POST /api/admin/companies`  
**Entonces** los autenticados reciben `403`, la anónima `401`; ningún cliente obtiene el nombre de la empresa B ni de ninguna otra; y la comprobación también existe en el caso de uso (prueba unitaria con `ICurrentUser` sin rol `Administrator`).

### CHU-05 — Desactivar bloquea el acceso de sus usuarios (propuesta)

**Dado** la empresa A activa con un Solicitante activo que tiene sesión abierta (cookie C1)  
**Cuando** el administrador envía `POST /api/admin/companies/{A}/deactivate`  
**Entonces** recibe `204`; el listado muestra `isActive = false`; un nuevo inicio de sesión del Solicitante responde `401` genérico de HU-003; una petición con C1, cumplido el intervalo de revalidación (en pruebas, cero), responde `401`; y ningún ticket ni dato de la empresa se borra.

### CHU-06 — Reactivar restaura el acceso y los conflictos de estado se rechazan

**Dado** la empresa A inactiva  
**Cuando** el administrador la reactiva, y después intenta reactivarla otra vez y desactivar una empresa ya inactiva B  
**Entonces** la primera responde `204` y el Solicitante de A vuelve a iniciar sesión; las otras dos responden `409`; y cualquier operación sobre un `companyId` inexistente responde `404`.

### CHU-07 — Auditoría de acciones administrativas (propuesta)

**Dado** un administrador con id X  
**Cuando** crea la empresa «Empresa Sintética 51», la renombra a «Empresa Sintética 51 S.A.S.» y la desactiva  
**Entonces** existen tres `AuditEntry` con actor X, fecha/hora UTC, acciones `CompanyCreated`, `CompanyRenamed`, `CompanyDeactivated`, objeto `Company:{id}`, valores anterior/nuevo (`null` → nombre; nombre anterior → nombre nuevo; `isActive: true` → `false`) y resultado exitoso; y la aplicación no expone ninguna operación que modifique o borre esas entradas.

### CHU-08 — Listado con búsqueda, filtro y paginación

**Dado** 50 empresas sintéticas, 5 de ellas inactivas  
**Cuando** el administrador consulta `?page=2&pageSize=25`, `?status=inactive` y `?pageSize=101`  
**Entonces** la primera devuelve 25 elementos con `totalCount = 50`, ordenados por nombre; la segunda devuelve exactamente las 5 inactivas; la tercera responde `400`.

### CHU-09 — La interfaz informa errores y confirma la desactivación

**Dado** la vista de empresas en `/interno/administracion/empresas`  
**Cuando** el backend responde `409` al guardar un nombre duplicado, o el administrador pulsa «Desactivar»  
**Entonces** la vista muestra «Ya existe una empresa con ese nombre.» junto al campo y conserva lo escrito; y antes de desactivar pide confirmación con el texto «Los usuarios de esta empresa no podrán ingresar mientras esté inactiva.»; si cancela, no se envía ninguna petición.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-09 validados con evidencia (CHU-05 y CHU-07 solo si T-01 confirma las propuestas; si no, se documenta la decisión).
- [ ] **DoD-02** — Pruebas escritas primero y en verde, con nombres por comportamiento: `Company_recorta_y_valida_nombre_1_a_150`, `Crear_empresa_duplicada_responde_409`, `Solo_Administrator_administra_empresas`, `Cliente_no_obtiene_empresas_ajenas`, `Desactivar_empresa_bloquea_login_y_sesiones_de_sus_usuarios`, `Reactivar_empresa_restaura_acceso`, `Acciones_sobre_empresas_quedan_auditadas`, `Listado_pagina_filtra_y_limita_pageSize`.
- [ ] **DoD-03** — Pruebas Vitest del módulo `companies` (modelos, controladores, vistas) en verde.
- [ ] **DoD-04** — `cd backend && dotnet test` en verde, incluidas `DataTicket.ArchitectureTests`.
- [ ] **DoD-05** — `npm --prefix frontend run lint`, `npm --prefix frontend test` y `npm --prefix frontend run build` en verde.
- [ ] **DoD-06** — Migración EF Core de empresas creada, aplicada en local y revisada (índice único incluido).
- [ ] **DoD-07** — `docker compose up --build` levanta y el flujo alta → edición → desactivación → reactivación funciona en `http://localhost:5173` (verificación manual documentada).
- [ ] **DoD-08** — Wiki actualizada vía Notas para la wiki: [[modelo-de-dominio]], [[persistencia-postgresql]], [[roles-y-permisos]], [[auditoria]], [[glosario]].
- [ ] **DoD-09** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos (autorización en caso de uso, exposición entre empresas, auditoría append-only).
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

- **Propuestas no respaldadas por el PRD**: longitud máxima de 150, unicidad del nombre, bloqueo de acceso al desactivar, ausencia de borrado físico y auditoría de acciones administrativas (el PRD §12 solo enumera eventos del ticket).
- **Orden con HU-011**: la auditoría depende de `IAuditLog`; HU-011 se adelantó al Sprint 1 (tablero, 2026-10-07).
- **¿La PM administra empresas?** Esta HU propone que no (`403`); ratificar con [[pendientes]] junto a «quién invita» de HU-005.
- Qué pasa con los tickets abiertos de una empresa desactivada (siguen en las colas internas) no lo define el PRD; propuesta: siguen visibles para Data Global.

## Relacionado

- [[ep-003-empresas-usuarios-y-equipos]] · [[tablero-scrum]] · [[modelo-de-dominio]] · [[roles-y-permisos]] · [[auditoria]] · [[persistencia-postgresql]] · [[autenticacion-identity]] · [[glosario]]
