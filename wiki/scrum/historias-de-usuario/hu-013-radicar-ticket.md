---
title: "HU-013 — Radicar ticket"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/ticket, producto/prioridad, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §6.1", "PRD.md §6.2", "PRD.md §6.4", "PRD.md §5", "PRD.md §9", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-013", "Radicar ticket", "Radicar ticket con formulario común"]
epica: "[[ep-005-radicacion-de-tickets]]"
criterios_prd: ["PRD CA-15", "PRD CA-01", "PRD CA-14", "PRD CA-10"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 2"
dependencias: ["[[hu-003-iniciar-y-cerrar-sesion]]", "[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-008-administrar-usuarios-cliente]]", "[[hu-011-registrar-eventos-auditables]]", "[[hu-002-shell-y-navegacion-por-rol]]"]
relacionadas: ["[[hu-014-adjuntos-en-radicacion]]", "[[hu-016-cola-global-de-triage]]", "[[hu-015-ajustar-prioridad-manualmente]]", "[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-046-radicar-con-formulario-de-empresa]]", "[[hu-010-datos-sinteticos-de-desarrollo]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-013 — Radicar ticket

Un solicitante o coordinador autenticado crea un ticket con el formulario común del piloto (categoría, título, descripción, urgencia, impacto); el servidor deriva empresa y solicitante de la cuenta, calcula la prioridad con la matriz, asigna un número, deja el ticket en `New` (cliente: `Received`), lo pone en la cola de triage y lo audita (PRD §6.1, §6.2, §12).

## Historia de usuario

**COMO** solicitante (o coordinador) de una empresa cliente  
**QUIERO** radicar un ticket con categoría, título, descripción, urgencia e impacto, y ver la prioridad que resulta  
**PARA** que Data Global reciba mi solicitud con contexto suficiente, la clasifique y la atienda, y yo tenga un número para darle seguimiento

## Contexto

El piloto opera con un formulario común (PRD §9, CA-15). La cuenta determina la empresa y el usuario que radica; esos datos no se eligen en el formulario (PRD §6.1). La prioridad se calcula con la matriz urgencia × impacto (PRD §6.1; [[matriz-de-prioridad]]). Al radicarse, el ticket entra en la cola general de Elizabeth con estado `New` (PRD §6.2) y el cliente lo ve como `Received` (PRD §6.4). Es la primera funcionalidad vertical completa: crea el agregado `Ticket` y su tabla. Los adjuntos se añaden en [[hu-014-adjuntos-en-radicacion|HU-014]] (mismo sprint).

## Alcance

- Agregado `Ticket` (campos de radicación) y función de dominio de la matriz de prioridad (`CalculatedPriority`).
- Catálogo `Category` de solo lectura con semilla provisional y endpoint para listarlo (V-12).
- Número de ticket único generado por el servidor (puerto propuesto `ITicketNumberGenerator`; formato V-16 abierto).
- Caso de uso `SubmitTicket` con autorización por rol y derivación de empresa/solicitante desde `ICurrentUser`.
- Endpoint `POST /api/tickets` (JSON) y `GET /api/categories`.
- Evento de auditoría `TicketSubmitted` vía `IAuditLog`.
- Formulario de radicación en el portal del cliente con vista previa de la prioridad, validación de ayuda y pantalla de confirmación con el número.
- Migración EF Core de `tickets` y `categories`.

## Fuera de alcance

- Adjuntos: [[hu-014-adjuntos-en-radicacion|HU-014]].
- Consulta posterior del ticket en el portal: [[hu-024-portal-solicitante-consulta-tickets|HU-024]] y [[hu-025-portal-coordinador-consulta-tickets|HU-025]].
- Ajuste manual de prioridad: [[hu-015-ajustar-prioridad-manualmente|HU-015]].
- Administración del catálogo de categorías (V-12).
- Formularios por empresa (Fase 4, [[hu-046-radicar-con-formulario-de-empresa|HU-046]]).
- Radicación por personas internas en nombre de un cliente (propuesta: no permitida; el PRD solo describe radicación por usuarios cliente, PRD §5, §6.1).
- Correo de acuse de recibo (no figura en PRD §11).

## Requisitos y reglas de negocio

- Radican el solicitante (PRD §5, §6.1) y el coordinador ("además de poder radicar", PRD §5).
- Empresa y solicitante se derivan de la cuenta; no se eligen en el formulario (PRD §6.1, §9).
- Campos: categoría; título de hasta 120 caracteres; descripción del caso; urgencia baja/media/alta; impacto bajo/medio/alto; prioridad calculada (PRD §6.1).
- (propuesta) Título obligatorio de 1 a 120 caracteres tras recortar espacios, contados como `string.Length` en .NET y `.length` en TypeScript (coinciden para texto habitual); descripción obligatoria con límite técnico a fijar en T-01 (propuesta: 10 000 caracteres).
- Matriz de prioridad (PRD §6.1):

  | `Impact` \ `Urgency` | `Low` | `Medium` | `High` |
  |---|---|---|---|
  | `Low` | `VeryLow` | `Low` | `Medium` |
  | `Medium` | `Low` | `Medium` | `High` |
  | `High` | `Medium` | `High` | `Critical` |

- La prioridad la calcula el servidor; un valor enviado por el cliente no se usa (propuesta derivada de PRD §6.1).
- El ticket nace en `New` y entra en la cola de triage; para el cliente es `Received` (PRD §6.2, §6.4).
- La radicación es un evento auditable (PRD §12).
- No se crea selector genérico de producto (PRD §9).

## Invariantes en juego

- Invariante 1: el ticket pertenece a la empresa de la cuenta; filtrado y derivado en backend.
- Invariante 2: la respuesta al cliente no expone el estado interno (`New`) ni datos internos.
- Invariante 8: la radicación queda en la auditoría.

## Criterios del PRD cubiertos

- PRD CA-15 (total: el piloto opera con el formulario común) → [[criterios-de-aceptacion]]
- PRD CA-01 (parcial: la empresa del ticket no es manipulable por el cliente) → [[criterios-de-aceptacion]]
- PRD CA-14 (parcial: radicación auditada con actor y fecha/hora) → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial: la respuesta de radicación no expone estados técnicos) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-005-radicacion-de-tickets|EP-005 — Radicación de tickets]]
- Dependencias: [[hu-003-iniciar-y-cerrar-sesion|HU-003 — Iniciar y cerrar sesión]] · [[hu-004-contexto-de-usuario-y-autorizacion|HU-004 — Contexto de usuario y autorización]] (`ICurrentUser` con rol y `company_id`) · [[hu-008-administrar-usuarios-cliente|HU-008 — Administrar usuarios cliente]] · [[hu-011-registrar-eventos-auditables|HU-011 — Registrar eventos auditables]] · [[hu-002-shell-y-navegacion-por-rol|HU-002 — Shell y navegación por rol]]
- Relacionadas: [[hu-014-adjuntos-en-radicacion|HU-014]] · [[hu-016-cola-global-de-triage|HU-016]] · [[hu-015-ajustar-prioridad-manualmente|HU-015]] · [[hu-024-portal-solicitante-consulta-tickets|HU-024]] · [[hu-046-radicar-con-formulario-de-empresa|HU-046]] · [[hu-010-datos-sinteticos-de-desarrollo|HU-010]]
- Decisiones: V-12 (categorías), V-16 (número), [[adr-0004-autenticacion-cookie-mismo-origen]] (antiforgery, pendiente de aceptar), [[adr-0008-portal-cliente-en-fase-1]]

## Componentes afectados

- Backend (Domain): `Ticket`, `Category`, `Urgency`, `Impact`, `CalculatedPriority`, `TicketStatus`, `ClientStatus`, matriz de prioridad.
- Backend (Application): `SubmitTicket`, `ITicketRepository`, `IAuditLog`, `ICurrentUser`, `IClock`, puertos propuestos `ITicketNumberGenerator` y `ICategoryCatalog`.
- Backend (Infrastructure): configuraciones EF Core, repositorio, generador de número (secuencia PostgreSQL, propuesta), semilla de categorías.
- Backend (Api): `POST /api/tickets`, `GET /api/categories`.
- Persistencia PostgreSQL: tablas `tickets` y `categories`, índice único de número.
- Frontend: módulo propuesto `src/modules/tickets` (models, controllers, views).

## Dificultad

**Nivel:** Alto

**Justificación:** primera funcionalidad vertical completa (dominio, persistencia, API, frontend), con reglas de autorización y multiempresa críticas, generación concurrente de números únicos, auditoría transaccional y dos decisiones abiertas (V-12, V-16) que hay que aislar sin bloquear.

## Contrato backend ↔ frontend

Propuesta, ratificar en T-01 (base `/api`, ProblemDetails RFC 9457, cookie de sesión y encabezado antiforgery según [[adr-0004-autenticacion-cookie-mismo-origen]]).

**`GET /api/categories`** — roles `Requester`, `CompanyCoordinator` (y roles internos para lectura). `200`:

```json
[
  { "id": "c1a2…", "name": "Incidente" },
  { "id": "c3b4…", "name": "Requerimiento" },
  { "id": "c5d6…", "name": "Consulta" }
]
```

Los nombres son una **semilla provisional** (propuesta) hasta resolver V-12; solo se listan categorías activas.

**`POST /api/tickets`** — `Content-Type: application/json` (HU-014 añade `multipart/form-data` con archivos). Roles: `Requester`, `CompanyCoordinator`.

```json
{
  "categoryId": "c1a2…",
  "title": "Error al generar la factura electrónica",
  "description": "Desde ayer el módulo de facturación devuelve un error 500 al emitir…",
  "urgency": "High",
  "impact": "Medium"
}
```

Respuesta `201 Created` (encabezado `Location` hacia el recurso de consulta del portal definido en HU-024):

```json
{
  "id": "3f4e…",
  "number": "DT-000123",
  "title": "Error al generar la factura electrónica",
  "category": { "id": "c1a2…", "name": "Incidente" },
  "urgency": "High",
  "impact": "Medium",
  "calculatedPriority": "High",
  "clientStatus": "Received",
  "createdAt": "2026-10-07T15:00:00Z",
  "attachments": []
}
```

- `number`: formato **ilustrativo**; el definitivo depende de V-16.
- La respuesta no incluye `status` interno, empresa, participantes ni datos internos.
- Propiedades no admitidas en el cuerpo (`companyId`, `requesterId`, `calculatedPriority`, `status`…) → `400` (propuesta: `JsonUnmappedMemberHandling.Disallow`; si T-01 decide ignorarlas, el servidor nunca las usa).

| Código | Cuándo |
|---|---|
| 400 | Validación: título vacío o > 120, descripción vacía o > límite técnico, `urgency`/`impact` fuera de `Low`/`Medium`/`High`, categoría inexistente o inactiva, propiedades no admitidas. `errors` por campo |
| 401 | Sin sesión |
| 403 | Rol interno (`ProductManager`, `Administrator`, `Development`, `Production`) o antiforgery inválido |
| 415 | `Content-Type` no admitido |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: fijar rutas, DTOs, nombres de enums en JSON, política ante propiedades desconocidas, límite técnico de la descripción, semilla provisional de categorías y formato provisional del número (marcado V-16). Registrar `ITicketNumberGenerator` y `ICategoryCatalog` (nombres propuestos) en [[glosario]] y [[backend-hexagonal]].
- [ ] **T-02 — Domain: matriz de prioridad** · Capa: Backend (Domain) · Dificultad: Bajo  
  Descripción: prueba primero `[Theory]` `Calculate_ReturnsMatrixValue` con las 9 combinaciones del PRD y `Calculate_IsSymmetric`. Implementar la función pura.
- [ ] **T-03 — Domain: agregado `Ticket` (radicación)** · Capa: Backend (Domain) · Dificultad: Medio  
  Descripción: pruebas primero: `Submit_WithValidData_StartsInNew`, `Submit_ClientStatusIsReceived`, `Submit_WithTitleOf121Chars_Throws`, `Submit_WithTitleOf120Chars_Succeeds`, `Submit_WithBlankTitle_Throws`, `Submit_WithBlankDescription_Throws`, `Submit_SetsCalculatedPriorityFromMatrix`. Implementar la factoría `Ticket.Submit(...)` y la derivación `ClientStatus` desde `TicketStatus` (propuesta del modelo).
- [ ] **T-04 — Application: `SubmitTicket`** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: pruebas unitarias primero con dobles: `SubmitTicket_UsesCompanyAndRequesterFromCurrentUser`, `SubmitTicket_AsCoordinator_Succeeds`, `SubmitTicket_AsInternalRole_IsForbidden`, `SubmitTicket_WithInactiveCategory_IsRejected`, `SubmitTicket_AppendsTicketSubmittedAuditEntry`. Implementar el caso de uso con `ICurrentUser`, `ICategoryCatalog`, `ITicketNumberGenerator`, `ITicketRepository`, `IAuditLog`, `IClock`.
- [ ] **T-05 — Infrastructure: persistencia y migración** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: pruebas de integración primero: `TicketRepository_PersistsAndReloadsTicket`, `TicketNumbers_AreUniqueUnderConcurrency` (20 radicaciones en paralelo), `Categories_SeedIsAvailable`. Configuraciones EF Core, repositorio, generador de número (secuencia PostgreSQL, propuesta), semilla de categorías y migración `AddTicketsAndCategories` (índice único en `number`, índice `(company_id, created_at)`).
- [ ] **T-06 — Api: endpoints** · Capa: Backend (Api) · Dificultad: Medio  
  Descripción: pruebas de integración primero con `WebApplicationFactory<Program>`: `PostTicket_AsRequester_Returns201WithClientShape`, `PostTicket_WithCompanyIdOfOtherCompany_NeverCreatesTicketInThatCompany`, `PostTicket_AsDeveloper_Returns403`, `PostTicket_Anonymous_Returns401`, `PostTicket_WithTitle121_Returns400WithFieldError`, `GetCategories_ReturnsOnlyActive`. Mapear endpoints con políticas por rol y ProblemDetails.
- [ ] **T-07 — Frontend models** · Capa: Frontend (models) · Dificultad: Medio  
  Descripción: pruebas Vitest primero: `calculatePriority` (9 combinaciones, espejo de la matriz solo para la vista previa), `validateSubmission` (título 0/1/120/121, descripción vacía, urgencia/impacto obligatorios), `toSubmittedTicket` (normaliza la respuesta), `toFieldErrors` (ProblemDetails → errores por campo). Implementar `ticketSubmission.ts` y `ticketsGateway.ts` (`submitTicket`, `fetchCategories`) sobre `core/http`.
- [ ] **T-08 — Frontend controller** · Capa: Frontend (controllers) · Dificultad: Medio  
  Descripción: `useSubmitTicketController` con estado `editing/submitting/submitted/error`, carga de categorías, prioridad derivada en vivo, bloqueo de doble envío y conservación de los datos ante error. Prueba Vitest de la lógica de transición (reducer puro).
- [ ] **T-09 — Frontend views** · Capa: Frontend (views) · Dificultad: Medio  
  Descripción: `SubmitTicketFormView` (campos, contador 0/120, prioridad calculada visible, errores por campo con `aria-describedby`) y `TicketSubmittedView` (número y estado "Recibido"). Ruta en el shell del portal del cliente (HU-002; enrutador según decisión abierta). `npm --prefix frontend run lint` sin violaciones MVC.

## Criterios de aceptación

### CHU-01 — Radicación exitosa

**Dado** Ana, solicitante activa de la empresa A, con sesión iniciada  
**Cuando** envía `POST /api/tickets` con categoría activa, título "Error al generar la factura electrónica", descripción no vacía, `urgency: "High"`, `impact: "Medium"`  
**Entonces** recibe `201` con `number` no vacío, `calculatedPriority: "High"` y `clientStatus: "Received"`; en base de datos el ticket tiene `company_id` = A, `requester_id` = Ana, `status` = `New` y `created_at` en UTC; y la vista muestra el número y el estado "Recibido".

### CHU-02 — Matriz de prioridad completa

**Dado** las 9 combinaciones de urgencia × impacto  
**Cuando** se radica un ticket con cada una  
**Entonces** la prioridad persistida, para cada par (`urgency`, `impact`), es: (`Low`,`Low`) `VeryLow`; (`Medium`,`Low`) `Low`; (`High`,`Low`) `Medium`; (`Low`,`Medium`) `Low`; (`Medium`,`Medium`) `Medium`; (`High`,`Medium`) `High`; (`Low`,`High`) `Medium`; (`Medium`,`High`) `High`; (`High`,`High`) `Critical`; y la vista previa del formulario muestra el mismo valor antes de enviar.

### CHU-03 — Validaciones con valores límite

**Dado** una solicitante de la empresa A  
**Cuando** envía títulos de 0 caracteres, solo espacios, 1, 120 y 121 caracteres; una descripción vacía; `urgency: "Critical"`; y un `categoryId` inexistente o inactivo  
**Entonces** 1 y 120 caracteres → `201`; 0, solo espacios y 121 → `400` con error en `title`; descripción vacía → `400` en `description`; `Critical` → `400` en `urgency`; categoría inválida → `400` en `categoryId`; y en los casos `400` no se crea ticket ni entrada de auditoría.

### CHU-04 — Empresa y solicitante derivados de la cuenta (A ↔ B)

**Dado** Ana (empresa A) y un ticket de prueba que intenta suplantar datos  
**Cuando** envía el cuerpo válido más `"companyId": "<id de B>"`, `"requesterId": "<usuario de B>"` y `"calculatedPriority": "Critical"` con `Low`/`Low`  
**Entonces** la respuesta es `400` (propuesta T-01) y no existe ningún ticket nuevo en la empresa B; si T-01 decide ignorar esas propiedades, la respuesta es `201` y el ticket queda en la empresa A, con Ana como solicitante y `VeryLow` como prioridad.

### CHU-05 — Autorización por rol en backend

**Dado** usuarios con roles `CompanyCoordinator` (empresa A), `ProductManager`, `Administrator`, `Development`, `Production` y una petición sin sesión  
**Cuando** cada uno envía una radicación válida  
**Entonces** el coordinador recibe `201` (ticket en A, coordinador como solicitante); los roles internos reciben `403`; sin sesión, `401`; y no se crea ningún ticket salvo el del coordinador.

### CHU-06 — Entra en la cola de triage

**Dado** un ticket recién radicado por Ana  
**Cuando** Elizabeth consulta la cola global de triage ([[hu-016-cola-global-de-triage|HU-016]])  
**Entonces** el ticket aparece con su número, empresa A, título, categoría, urgencia, impacto, prioridad calculada, fecha de creación y estado `New`.

### CHU-07 — Auditoría de la radicación

**Dado** la radicación exitosa de CHU-01 con `IClock` fijo en `2026-10-07T15:00:00Z`  
**Cuando** se consulta la tabla de auditoría  
**Entonces** existe una entrada `TicketSubmitted` con actor = Ana, `occurred_at` = `2026-10-07 15:00:00+00`, objeto `Ticket`/id del ticket, `old_value` nulo y `new_value` con número, empresa, solicitante, categoría, urgencia, impacto y prioridad calculada; confirmada en la misma transacción que el ticket.

### CHU-08 — Sin exposición de datos internos

**Dado** la respuesta `201` de una radicación  
**Cuando** se comparan sus claves JSON con el contrato  
**Entonces** no contiene `status`, `New`, participantes, asignaciones, `pullRequestUrl` ni nombres de personas internas; el único estado presente es `clientStatus: "Received"`.

### CHU-09 — Números únicos bajo concurrencia

**Dado** 20 radicaciones válidas enviadas en paralelo por usuarios de las empresas A y B  
**Cuando** todas terminan  
**Entonces** se crean 20 tickets con 20 números distintos y ninguna petición falla por colisión de número.

### CHU-10 — Comportamiento de la UI ante error

**Dado** el formulario completo en el navegador  
**Cuando** el backend responde `400` con error en `title`, o falla la red  
**Entonces** con `400` se muestra el mensaje junto al campo título (asociado por `aria-describedby`) y el resto de datos se conserva; con fallo de red se muestra un `role="alert"` con "Reintentar" sin perder lo escrito; durante el envío el botón está deshabilitado y un doble clic no crea dos tickets.

## Definition of Done

- [ ] CHU-01 a CHU-10 validados con evidencia.
- [ ] Pruebas backend escritas primero y en verde (`cd backend && dotnet test`): `Calculate_ReturnsMatrixValue` (9 casos), `Submit_WithTitleOf121Chars_Throws`, `SubmitTicket_UsesCompanyAndRequesterFromCurrentUser`, `SubmitTicket_AsInternalRole_IsForbidden`, `PostTicket_WithCompanyIdOfOtherCompany_NeverCreatesTicketInThatCompany`, `TicketNumbers_AreUniqueUnderConcurrency`, `SubmitTicket_AppendsTicketSubmittedAuditEntry`.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Migración EF Core `AddTicketsAndCategories` creada, aplicada en Compose y reversible.
- [ ] Pruebas Vitest en verde (`npm --prefix frontend test`): `calculatePriority`, `validateSubmission`, `toFieldErrors`, reducer del controlador.
- [ ] `npm --prefix frontend run lint` sin errores y `npm --prefix frontend run build` correcto.
- [ ] Contrato publicado en OpenAPI (`/openapi/v1.json`) coherente con los tipos del modelo frontend.
- [ ] `docker compose up --build`: un solicitante sintético radica desde `http://localhost:5173` y el ticket aparece en la cola de la PM.
- [ ] Wiki: [[flujo-del-ticket]] (radicación implementada), [[matriz-de-prioridad]] (ubicación de la función), [[formularios-configurables]] (categorías provisionales), [[modelo-de-dominio]] (campos de `Ticket` y `Category`), [[persistencia-postgresql]] (tablas), [[backend-hexagonal]] (puertos nuevos), [[frontend-mvc]] (módulo `tickets`), [[glosario]] (nombres nuevos) mediante Notas para la wiki; PRD CA-15 propuesto para marcar en [[criterios-de-aceptacion]].
- [ ] `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos.
- [ ] PR revisado y aprobado por otra persona del equipo.
- [ ] Trazabilidad de la HU y de [[ep-005-radicacion-de-tickets]] actualizada.

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
| DoD-01 Pruebas backend | Pendiente | — | — |
| DoD-02 ArchitectureTests | Pendiente | — | — |
| DoD-03 Migración `AddTicketsAndCategories` | Pendiente | — | — |
| DoD-04 Pruebas Vitest | Pendiente | — | — |
| DoD-05 Lint y build frontend | Pendiente | — | — |
| DoD-06 Contrato OpenAPI | Pendiente | — | — |
| DoD-07 `docker compose up --build` | Pendiente | — | — |
| DoD-08 Wiki actualizada | Pendiente | — | — |
| DoD-09 quality-reviewer | Pendiente | — | — |
| DoD-10 PR revisado | Pendiente | — | — |
| DoD-11 Trazabilidad | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- **V-12** — las categorías "Incidente", "Requerimiento" y "Consulta" son una semilla provisional para poder operar; no salen del PRD. Cambiarlas no debe requerir cambio de código (datos sembrados en tabla).
- **V-16** — el número se genera en servidor y es único; el formato `DT-000123` es solo ilustrativo.
- (propuesta) Personas internas no radican: el PRD describe la radicación solo para usuarios cliente; si Data Global necesita radicar en nombre de un cliente, sería una HU nueva.
- (propuesta) Límite técnico de la descripción (10 000 caracteres) para proteger el servidor; el PRD no fija uno.
- La vista previa de la prioridad en el frontend es solo ayuda de interfaz; el valor autoritativo lo calcula el dominio.

## Relacionado

- [[ep-005-radicacion-de-tickets]] · [[tablero-scrum]] · [[flujo-del-ticket]] · [[matriz-de-prioridad]] · [[formularios-configurables]]
- [[estados-del-ticket]] · [[roles-y-permisos]] · [[auditoria]] · [[modelo-de-dominio]] · [[backend-hexagonal]] · [[frontend-mvc]]
- [[criterios-de-aceptacion]] · [[adr-0008-portal-cliente-en-fase-1]] · [[pendientes]]
