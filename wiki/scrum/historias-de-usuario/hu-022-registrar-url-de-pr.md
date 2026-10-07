---
title: "HU-022 — Registrar URL de PR"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/ticket, producto/seguridad, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.3", "PRD.md §6.4", "PRD.md §10", "PRD.md §12", "PRD.md §13", "PRD.md §14"]
aliases: ["HU-022", "Registrar URL de PR", "Registrar URL manual de PR"]
epica: "[[ep-007-trabajo-interno-y-estados]]"
criterios_prd: [CA-10]
componentes: [Backend Domain, Backend Application, Backend Infrastructure, Backend Api, Frontend models, Frontend controllers, Frontend views, Persistencia PostgreSQL]
dificultad: "Medio"
sprint_sugerido: "Sprint 4"
dependencias: ["[[hu-011-registrar-eventos-auditables]]", "[[hu-020-detalle-interno-del-ticket]]"]
relacionadas: ["[[hu-021-cambiar-estado-interno]]", "[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-025-portal-coordinador-consulta-tickets]]", "[[hu-035-correo-de-respuesta-formal]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-022 — Registrar URL de PR

Permite a una persona interna autorizada registrar o corregir a mano la URL `https` del pull request de un ticket, visible solo para internos autorizados y nunca en el portal del cliente; no hay integración con GitHub ni GitLab (PRD §6.3, §13).

## Historia de usuario

**COMO** participante vigente de un ticket que generó un pull request (o Elizabeth)  
**QUIERO** registrar la URL del PR en el ticket  
**PARA** que Julián y los demás participantes encuentren el cambio a revisar y desplegar sin buscarlo fuera de DataTicket

## Contexto

"Si se genera un pull request, una persona autorizada agrega manualmente su URL" y "la URL se muestra a participantes internos autorizados" (PRD §6.3). Los repositorios son privados y la integración automática está fuera del MVP (PRD §6.3, §13). Quién es la "persona autorizada" está abierto (V-04). El glosario tiene `PullRequestUrl`, pero no un caso de uso para registrarla (hallazgo 7): se propone `SetPullRequestUrl` (nombre propuesto, ratificar en T-01 y registrar en el glosario).

## Alcance

- Value object `PullRequestUrl` con validación.
- Caso de uso para registrar o reemplazar la URL, con autorización y auditoría.
- Visualización como enlace en el detalle interno ([[hu-020-detalle-interno-del-ticket|HU-020]]).
- Prueba de no exposición en las respuestas del portal.

## Fuera de alcance

- Integración, verificación o sincronización con GitHub/GitLab; el backend no hace ninguna petición a la URL (PRD §6.3, §13).
- Varias URL de PR por ticket (el modelo propuesto tiene un único campo, [[modelo-de-dominio]]).
- Borrar la URL (propuesta: solo reemplazar; ver Notas).
- Cambio automático de estado al registrar la URL (lo decide la persona con [[hu-021-cambiar-estado-interno|HU-021]]).

## Requisitos y reglas de negocio

- La URL se agrega manualmente y la ven participantes internos autorizados (PRD §6.3).
- El cliente nunca ve la URL de PR (PRD §5 regla 6, §10, §14.10).
- Sin integración automática con proveedores de repositorios (PRD §6.3, §13).
- (propuesta) Solo se aceptan URL absolutas con esquema `https`, sin espacios y de hasta 2048 caracteres.
- (propuesta, V-04) Pueden registrarla: participantes vigentes, `ProductManager` y `Administrator` asociado.
- (propuesta) Se audita con valor anterior y nuevo. El PRD §12 no lista la URL de PR entre los eventos relevantes, pero §6.4 pide registrar "los movimientos internos" en la bitácora.

## Invariantes en juego

- AGENTS §7.2 — el cliente nunca ve la URL de PR.
- AGENTS §7.3 — solo participantes vigentes (y Elizabeth) operan sobre el contenido del ticket.
- AGENTS §7.5 — un administrador no asociado no puede registrarla.
- AGENTS §7.8 — auditoría append-only (propuesta para este evento).

## Criterios del PRD cubiertos

- PRD CA-10 (parcial: la URL de PR no aparece en el portal; los correos al cliente se verifican en [[hu-035-correo-de-respuesta-formal|HU-035]]) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-007-trabajo-interno-y-estados]]
- Dependencias: [[hu-011-registrar-eventos-auditables|HU-011]] (`IAuditLog`), [[hu-020-detalle-interno-del-ticket|HU-020]] (regla de acceso y vista de detalle).
- Relacionadas: [[hu-021-cambiar-estado-interno|HU-021]], [[hu-024-portal-solicitante-consulta-tickets|HU-024]] y [[hu-025-portal-coordinador-consulta-tickets|HU-025]] (pruebas de no exposición), [[hu-035-correo-de-respuesta-formal|HU-035]].
- Decisiones: V-04 en [[pendientes]]; hallazgo 7 (nombre del caso de uso).

## Componentes afectados

- Backend Domain: value object `PullRequestUrl` y método en `Ticket`.
- Backend Application: `SetPullRequestUrl` (nombre propuesto), `ITicketRepository`, `IAuditLog`, `ICurrentUser`, `IClock`.
- Backend Infrastructure: mapeo de la columna.
- Backend Api: `PUT /api/tickets/{ticketId}/pull-request-url`.
- Frontend models/controllers/views: formulario y enlace en el detalle interno.
- Persistencia PostgreSQL: columna `PullRequestUrl` si HU-013 no la creó.

## Dificultad

**Nivel:** Medio

**Justificación:** el cambio es localizado, pero exige validación estricta de URL (riesgo de enlaces `javascript:`), autorización por participación, auditoría y pruebas de no exposición que recorren el portal.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01.

**`PUT /api/tickets/{ticketId}/pull-request-url`**

Request:

```json
{ "pullRequestUrl": "https://github.com/dataglobal/facturacion/pull/42" }
```

Respuesta `200 OK`:

```json
{
  "ticketId": "6f1c2b9e-0d4a-4c1e-9a51-3c2f8e7b1a20",
  "pullRequestUrl": "https://github.com/dataglobal/facturacion/pull/42",
  "previousPullRequestUrl": null,
  "updatedAt": "2026-10-07T17:05:00Z"
}
```

Lectura: campo `pullRequestUrl` del detalle interno `GET /api/tickets/{ticketId}` ([[hu-020-detalle-interno-del-ticket|HU-020]]). Nunca existe en `/api/portal/**`.

Errores (ProblemDetails):

| Código | Cuándo |
|---|---|
| 400 | URL vacía, relativa, con esquema distinto de `https` (`http:`, `javascript:`, `ftp:`), con espacios o de más de 2048 caracteres |
| 401 | Sin sesión |
| 403 | Usuario cliente |
| 404 | Ticket inexistente o no visible (interno no asociado, admin no asociado, retirado) |
| 409 | (propuesta) Ticket en `Closed` |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar ruta, DTO, reglas de validación, quién registra (V-04), si se audita y si se bloquea en `Closed`; registrar `SetPullRequestUrl` en el [[glosario]].
- [ ] **T-02 — Value object** · Capa: Backend Domain · Dificultad: Bajo  
  Descripción: primero pruebas xUnit con casos límite (válida `https://github.com/org/repo/pull/1`; inválidas `""`, `"   "`, `/pull/1`, `http://github.com/...`, `javascript:alert(1)`, `https://exa mple.com`, 2048 caracteres válida y 2049 inválida); luego `PullRequestUrl` y el método de `Ticket` que devuelve el valor anterior.
- [ ] **T-03 — Caso de uso** · Capa: Backend Application · Dificultad: Medio  
  Descripción: primero pruebas unitarias con dobles: participante vigente registra; PM registra; admin no asociado → no encontrado; retirado → no encontrado; reemplazo deja `AuditEntry` con anterior/nuevo; el caso de uso no depende de ningún puerto HTTP saliente. Luego `SetPullRequestUrl`.
- [ ] **T-04 — Persistencia** · Capa: Backend Infrastructure · Dificultad: Bajo  
  Descripción: mapear la columna (`varchar(2048)`, nula); migración EF Core si no existe; prueba de integración de ida y vuelta.
- [ ] **T-05 — Endpoint** · Capa: Backend Api · Dificultad: Bajo  
  Descripción: `PUT` con mapeo a 200/400/403/404/409; pruebas de integración.
- [ ] **T-06 — Prueba de no exposición en el portal** · Capa: Backend Api · Dificultad: Bajo  
  Descripción: prueba de integración que registra una URL única y luego llama a `GET /api/portal/tickets` y `GET /api/portal/tickets/{id}` como solicitante y coordinador: la cadena de la URL y la clave `pullRequestUrl` no aparecen en el cuerpo crudo (se ejecuta cuando HU-024/HU-025 existan).
- [ ] **T-07 — Modelo y gateway** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: `validatePullRequestUrl()` como ayuda de interfaz (mismas reglas, sin sustituir al backend) y `setPullRequestUrl()` en el gateway; pruebas Vitest con los mismos casos límite.
- [ ] **T-08 — Controlador** · Capa: Frontend controllers · Dificultad: Bajo  
  Descripción: `usePullRequestUrlController` con estado `idle | editing | saving | error` y errores de validación del ProblemDetails; pruebas Vitest.
- [ ] **T-09 — Vista** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `PullRequestUrlView`: enlace con `target="_blank"` y `rel="noopener noreferrer"`, formulario de edición para quien puede editar, mensaje de error del campo.

## Criterios de aceptación

### CHU-01 — Registrar la URL

**Dado** Laura, participante vigente de un ticket sin URL de PR  
**Cuando** envía `PUT` con `https://github.com/dataglobal/facturacion/pull/42`  
**Entonces** responde 200 con `previousPullRequestUrl = null` y el detalle interno devuelve esa `pullRequestUrl`.

### CHU-02 — Validación con valores límite

**Dado** un participante vigente  
**Cuando** envía `""`, `"/pull/42"`, `"http://github.com/dataglobal/facturacion/pull/42"`, `"javascript:alert(1)"`, `"https://exa mple.com/pull/1"` o una URL `https` de 2049 caracteres  
**Entonces** cada petición responde 400 ProblemDetails sobre `pullRequestUrl` y el valor guardado no cambia; **y** una URL `https` de exactamente 2048 caracteres responde 200.

### CHU-03 — Autorización en backend

**Dado** un ticket con Laura como participante vigente  
**Cuando** intentan registrar la URL Kevin (Desarrollo, no asociado), un administrador no asociado o Brayan (retirado)  
**Entonces** reciben 404; **cuando** lo intenta el solicitante del ticket recibe 403; **y** la PM recibe 200.

### CHU-04 — Nunca aparece en el portal

**Dado** un ticket de la empresa A con `pullRequestUrl = "https://github.com/dataglobal/facturacion/pull/42"`  
**Cuando** el solicitante A1 y el coordinador de A llaman a `GET /api/portal/tickets` y `GET /api/portal/tickets/{ticketId}`  
**Entonces** el cuerpo crudo de las cuatro respuestas no contiene la clave `pullRequestUrl` ni la subcadena `github.com/dataglobal`.

### CHU-05 — Auditoría del reemplazo

**Dado** un ticket con URL `.../pull/42`  
**Cuando** la PM la reemplaza por `.../pull/43`  
**Entonces** existe un `AuditEntry` con actor la PM, fecha/hora UTC, `OldValue` `.../pull/42` y `NewValue` `.../pull/43` (propuesta, ver Notas).

### CHU-06 — Sin integración externa

**Dado** el registro de una URL de un repositorio privado inaccesible desde el servidor  
**Cuando** se envía el `PUT`  
**Entonces** responde 200 sin realizar ninguna petición saliente (el caso de uso no depende de ningún puerto HTTP y la prueba se ejecuta sin red).

### CHU-07 — Comportamiento de la UI

**Dado** el detalle interno  
**Cuando** el usuario escribe `http://github.com/...`  
**Entonces** la vista muestra "La URL debe empezar por https://" antes de enviar; **si** el backend devuelve 400 se muestra su mensaje junto al campo; **y** una URL guardada se muestra como enlace que abre en otra pestaña con `rel="noopener noreferrer"`.

## Definition of Done

- [ ] CHU-01 a CHU-07 validados con evidencia.
- [ ] Pruebas de dominio `PullRequestUrl_RechazaEsquemaNoHttps`, `PullRequestUrl_Acepta2048Rechaza2049` y de caso de uso `SetPullRequestUrl_AdminNoAsociado_DevuelveNoEncontrado`, `SetPullRequestUrl_Reemplazo_AuditaAnteriorYNuevo` en verde (`cd backend && dotnet test`).
- [ ] Prueba de integración `Portal_NoExponeUrlDePr` en verde una vez existan HU-024 y HU-025.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Migración EF Core creada y revisada (si la columna no existía).
- [ ] Vitest, `npm --prefix frontend run lint` y `npm --prefix frontend run build` en verde.
- [ ] El stack levanta con `docker compose up --build`.
- [ ] Wiki actualizada: [[flujo-del-ticket]] (quién registra la URL, cuando se resuelva V-04), [[auditoria]] (evento propuesto), [[glosario]] (`SetPullRequestUrl` si se ratifica), [[modelo-de-dominio]] (value object).
- [ ] `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos.
- [ ] PR revisado y aprobado por otra persona del equipo.

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
| DoD-01 Pruebas de dominio y caso de uso | Pendiente | — | — |
| DoD-02 Prueba de no exposición en el portal | Pendiente | — | — |
| DoD-03 Pruebas de arquitectura | Pendiente | — | — |
| DoD-04 Migración EF Core | Pendiente | — | — |
| DoD-05 Vitest, lint y build | Pendiente | — | — |
| DoD-06 `docker compose up --build` | Pendiente | — | — |
| DoD-07 Wiki actualizada | Pendiente | — | — |
| DoD-08 `quality-reviewer` | Pendiente | — | — |
| DoD-09 PR revisado | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- **V-04:** "persona autorizada" sin definir; la propuesta (participantes vigentes + PM + admin asociado) debe confirmarla la persona responsable del producto.
- (propuesta) Auditar el registro y el reemplazo de la URL, aunque PRD §12 no lo enumere; si se descarta, CHU-05 se elimina.
- (propuesta) No se permite borrar la URL, solo reemplazarla; y se bloquea en `Closed` (409).
- Restringir a dominios concretos (github.com, gitlab.com) no está en el PRD; no se propone.

## Relacionado

- [[ep-007-trabajo-interno-y-estados]] · [[tablero-scrum]] · [[flujo-del-ticket]] · [[estados-del-ticket]] · [[roles-y-permisos]]
- [[auditoria]] · [[modelo-de-dominio]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[criterios-de-aceptacion]] · [[pendientes]]
