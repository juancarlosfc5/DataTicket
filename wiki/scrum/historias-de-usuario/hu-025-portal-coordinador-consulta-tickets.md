---
title: "HU-025 — Portal: el coordinador consulta los tickets de su empresa"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/portal, producto/seguridad, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.4", "PRD.md §10", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-025", "Portal: el coordinador consulta los tickets de su empresa"]
epica: "[[ep-008-bandejas-y-portal-del-cliente]]"
criterios_prd: [CA-01, CA-02, CA-10]
componentes: [Backend Application, Backend Infrastructure, Backend Api, Frontend models, Frontend controllers, Frontend views]
dificultad: "Medio"
sprint_sugerido: "Sprint 4"
dependencias: ["[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-008-administrar-usuarios-cliente]]", "[[hu-010-datos-sinteticos-de-desarrollo]]"]
relacionadas: ["[[hu-013-radicar-ticket]]", "[[hu-034-portal-respuesta-formal]]", "[[hu-042-resumen-de-estados-en-portal]]", "[[hu-022-registrar-url-de-pr]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-025 — Portal: el coordinador consulta los tickets de su empresa

El coordinador de empresa lista y abre los tickets de **todos** los solicitantes de su empresa, nunca los de otra (404 y fuera de listados y conteos), con las mismas restricciones de exposición que el solicitante (PRD §5, §10, §14.1–§14.2).

## Historia de usuario

**COMO** coordinador de una empresa cliente (`CompanyCoordinator`)  
**QUIERO** consultar todos los tickets radicados por las personas de mi empresa  
**PARA** hacer seguimiento del soporte que recibe mi organización sin preguntar a cada solicitante

## Contexto

"El coordinador consulta los tickets de toda su empresa" y además puede radicar (PRD §5, §10). Reutiliza los endpoints, casos de uso (`ListClientTickets`, `GetClientTicket`) y el puerto `IClientTicketQueries` de [[hu-024-portal-solicitante-consulta-tickets|HU-024]]; lo nuevo es el alcance por empresa según el rol y sus pruebas negativas. La radicación del coordinador está en [[hu-013-radicar-ticket|HU-013]]. Se entrega en Fase 1 por [[adr-0008-portal-cliente-en-fase-1]].

## Alcance

- Alcance `company` en `IClientTicketQueries` para el rol `CompanyCoordinator`: todos los tickets con su `CompanyId`.
- Indicación del solicitante de cada ticket en lista y detalle.
- Filtro opcional por `ClientStatus` y paginación (mismo contrato que HU-024).
- Ajustes de vista: columna "Solicitante" y título "Solicitudes de mi empresa".

## Fuera de alcance

- Administración de usuarios de la empresa ([[hu-008-administrar-usuarios-cliente|HU-008]]).
- Respuesta formal ([[hu-034-portal-respuesta-formal|HU-034]]) y resumen por estados ([[hu-042-resumen-de-estados-en-portal|HU-042]]).
- Filtro por solicitante, exportación y vistas guardadas (no exigidos; PRD §10, §16.4).
- Coordinadores de varias empresas (el modelo propone una sola empresa por usuario cliente, [[modelo-de-dominio]]).

## Requisitos y reglas de negocio

- El coordinador consulta los tickets de su empresa completa y puede radicar (PRD §5, §10).
- Los usuarios cliente solo consultan tickets de la empresa a la que pertenece su cuenta (PRD §5 regla 1, §14.1).
- Ambos perfiles ven solo estado resumido, datos de radicación y respuesta formal final (PRD §14.2).
- Los clientes nunca ven mensajes internos, estados técnicos, URL de PR, nombres de colaboradores internos ni actividad privada de otros clientes (PRD §5 regla 6).
- (propuesta) Mostrar al coordinador el nombre del solicitante de su misma empresa; "otros clientes" se interpreta como otras empresas.
- (propuesta) Ticket de otra empresa → 404 con el mismo cuerpo que uno inexistente.

## Invariantes en juego

- AGENTS §7.1 — el coordinador ve los tickets de su empresa y solo de ella; filtrado en backend.
- AGENTS §7.2 — mismas prohibiciones de exposición que el solicitante.

## Criterios del PRD cubiertos

- PRD CA-01 (parcial: alcance del coordinador; junto con HU-024 cubre listar y consultar) → [[criterios-de-aceptacion]]
- PRD CA-02 (parcial: alcance de empresa del coordinador; falta la respuesta formal, HU-034) → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial: superficie del portal del coordinador) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-008-bandejas-y-portal-del-cliente]]
- Dependencias: [[hu-024-portal-solicitante-consulta-tickets|HU-024]] (endpoints, DTO y allowlist), [[hu-008-administrar-usuarios-cliente|HU-008]] (rol coordinador), [[hu-010-datos-sinteticos-de-desarrollo|HU-010]] (empresas A y B con varios solicitantes).
- Relacionadas: [[hu-013-radicar-ticket|HU-013]], [[hu-022-registrar-url-de-pr|HU-022]] (no exposición), [[hu-034-portal-respuesta-formal|HU-034]], [[hu-042-resumen-de-estados-en-portal|HU-042]].
- Decisiones: [[adr-0008-portal-cliente-en-fase-1]]; Row-Level Security abierta en [[pendientes]] §4.

## Componentes afectados

- Backend Application: cálculo del alcance por rol en `ListClientTickets` / `GetClientTicket`.
- Backend Infrastructure: predicado de alcance `company` en el adaptador de `IClientTicketQueries`.
- Backend Api: sin rutas nuevas; mismas que HU-024.
- Frontend models/controllers/views: columna de solicitante y textos según rol.

## Dificultad

**Nivel:** Medio

**Justificación:** reutiliza el contrato y la allowlist de HU-024; el trabajo está en el alcance por rol y en una batería de pruebas negativas multiempresa sobre lista, detalle y conteos.

## Contrato backend ↔ frontend

Mismas rutas y DTO que [[hu-024-portal-solicitante-consulta-tickets|HU-024]] (propuesta, ratificar en T-01):

- `GET /api/portal/tickets?clientStatus=Received&page=1&pageSize=20`
- `GET /api/portal/tickets/{ticketId}`

Diferencia de comportamiento según el rol del usuario autenticado (nunca según parámetros de la petición):

| Rol | Alcance |
|---|---|
| `Requester` | `RequesterId = usuario actual` y `CompanyId = empresa del usuario` |
| `CompanyCoordinator` | `CompanyId = empresa del usuario` |

Ejemplo de ítem para el coordinador de A (ticket radicado por A2):

```json
{
  "id": "0d9e…",
  "number": "DT-000130",
  "title": "Reporte de cartera vacío",
  "category": { "id": "c-02", "name": "Reportes" },
  "urgency": "Medium",
  "impact": "Low",
  "calculatedPriority": "Low",
  "clientStatus": "Received",
  "requester": { "id": "a2…", "displayName": "Solicitante A2" },
  "createdAt": "2026-10-06T19:30:00Z"
}
```

Errores: idénticos a HU-024 (400, 401, 403 para internos, 404 para tickets de otra empresa o inexistentes).

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: confirmar que el coordinador comparte rutas y DTO con el solicitante y que puede ver el nombre de los solicitantes de su empresa.
- [ ] **T-02 — Alcance por rol** · Capa: Backend Application · Dificultad: Medio  
  Descripción: primero pruebas unitarias: con rol `CompanyCoordinator` el alcance es la empresa; con `Requester` es el propio usuario; un parámetro de query como `companyId` o `requesterId` se ignora o se rechaza con 400 (propuesta: rechazar); luego el cálculo del alcance en los casos de uso.
- [ ] **T-03 — Predicado de empresa** · Capa: Backend Infrastructure · Dificultad: Medio  
  Descripción: alcance `company` en el adaptador de `IClientTicketQueries`; pruebas de integración con PostgreSQL real y dos empresas con varios solicitantes cada una.
- [ ] **T-04 — Pruebas de endpoint** · Capa: Backend Api · Dificultad: Medio  
  Descripción: pruebas con `WebApplicationFactory<Program>`: lista, detalle, `totalCount` y filtro como coordinador de A frente a datos de B; allowlist de claves también para el coordinador.
- [ ] **T-05 — Modelo** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: exponer el ámbito (`own | company`) que el frontend deduce del rol de la sesión para textos y columnas; pruebas Vitest.
- [ ] **T-06 — Controlador** · Capa: Frontend controllers · Dificultad: Bajo  
  Descripción: `useClientTicketListController` muestra la columna de solicitante si el ámbito es `company`; pruebas Vitest.
- [ ] **T-07 — Vista** · Capa: Frontend views · Dificultad: Bajo  
  Descripción: `ClientTicketListView` con columna "Solicitante" y título "Solicitudes de mi empresa" para el coordinador.

## Criterios de aceptación

### CHU-01 — Lista de toda la empresa

**Dado** la empresa A con 3 tickets de A1, 2 de A2 y 1 del propio coordinador, y la empresa B con 4 tickets  
**Cuando** el coordinador de A pide `GET /api/portal/tickets`  
**Entonces** recibe 200 con los 6 tickets de A, `totalCount = 6`, y cada ítem indica su `requester.displayName`.

### CHU-02 — Detalle de un ticket de otro solicitante de su empresa

**Dado** el ticket T-A2 radicado por A2  
**Cuando** el coordinador de A pide su detalle  
**Entonces** recibe 200 con los datos de radicación y adjuntos de radicación de T-A2.

### CHU-03 — Prueba negativa multiempresa en detalle, lista y conteos

**Dado** el ticket T-B de la empresa B  
**Cuando** el coordinador de A pide `GET /api/portal/tickets/{T-B}` y la lista con `?clientStatus=Received` y sin filtro  
**Entonces** el detalle responde 404 con el mismo cuerpo que un GUID inexistente, T-B no aparece en ninguna lista y ningún `totalCount` incluye tickets de B.

### CHU-04 — El alcance no se puede forzar desde la petición

**Dado** el coordinador de A  
**Cuando** pide `GET /api/portal/tickets?companyId={idDeB}`  
**Entonces** recibe 400 (propuesta) y en ningún caso tickets de B.

### CHU-05 — Mismas restricciones de exposición

**Dado** un ticket de A2 en `InProduction` con URL de PR, participantes internos y un ajuste de prioridad  
**Cuando** el coordinador de A pide lista y detalle  
**Entonces** las claves coinciden con la allowlist de HU-024, `clientStatus = "InProgress"` y el cuerpo crudo no contiene `InProduction`, la URL de PR ni los nombres de los participantes internos.

### CHU-06 — El solicitante no hereda el alcance del coordinador

**Dado** el solicitante A1 de la misma empresa  
**Cuando** pide la lista  
**Entonces** sigue viendo solo sus 3 tickets (regresión de HU-024).

### CHU-07 — UI según el rol

**Dado** el coordinador de A en el portal  
**Cuando** abre la lista  
**Entonces** ve el título "Solicitudes de mi empresa" y la columna "Solicitante"; **con** una empresa sin tickets ve "Tu empresa aún no tiene solicitudes"; **y** ante un 500 ve un error con "Reintentar".

## Definition of Done

- [ ] CHU-01 a CHU-07 validados con evidencia.
- [ ] Pruebas en verde (`cd backend && dotnet test`): `ListClientTickets_Coordinador_IncluyeOtrosSolicitantesDeSuEmpresa`, `ListClientTickets_Coordinador_TotalNoIncluyeOtraEmpresa`, `GetClientTicket_CoordinadorOtraEmpresa_Devuelve404`, `ListClientTickets_ParametroCompanyId_Rechazado`, `ListClientTickets_Solicitante_NoHeredaAlcanceDeEmpresa`.
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Vitest, `npm --prefix frontend run lint` y `npm --prefix frontend run build` en verde.
- [ ] El stack levanta con `docker compose up --build` y el portal del coordinador sintético muestra los tickets de su empresa.
- [ ] Sin migración (no cambia el esquema); si se añade un índice por `CompanyId`, migración revisada.
- [ ] Wiki actualizada: [[roles-y-permisos]] (alcance del coordinador verificado), [[criterios-de-aceptacion]] (evidencia de CA-01 y CA-02 para el coordinador).
- [ ] `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos, con revisión explícita de aislamiento multiempresa.
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
| DoD-01 Pruebas backend nombradas | Pendiente | — | — |
| DoD-02 Pruebas de arquitectura | Pendiente | — | — |
| DoD-03 Vitest, lint y build | Pendiente | — | — |
| DoD-04 `docker compose up --build` | Pendiente | — | — |
| DoD-05 Migración (no aplica o revisada) | Pendiente | — | — |
| DoD-06 Wiki actualizada | Pendiente | — | — |
| DoD-07 `quality-reviewer` | Pendiente | — | — |
| DoD-08 PR revisado | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- Row-Level Security sigue abierta: mismo riesgo que en HU-024.
- (propuesta) Si un usuario tuviera a la vez los roles `Requester` y `CompanyCoordinator`, prevalece el alcance de empresa.
- (propuesta) Que el coordinador vea el nombre de los solicitantes de su empresa debe confirmarlo la persona responsable del producto.

## Relacionado

- [[ep-008-bandejas-y-portal-del-cliente]] · [[tablero-scrum]] · [[adr-0008-portal-cliente-en-fase-1]] · [[roles-y-permisos]] · [[estados-del-ticket]]
- [[persistencia-postgresql]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[criterios-de-aceptacion]]
