---
title: "HU-024 — Portal: el solicitante consulta sus tickets"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/portal, producto/seguridad, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.1", "PRD.md §6.4", "PRD.md §8", "PRD.md §10", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-024", "Portal: el solicitante consulta sus tickets", "ListClientTickets", "GetClientTicket"]
epica: "[[ep-008-bandejas-y-portal-del-cliente]]"
criterios_prd: [CA-01, CA-02, CA-10]
componentes: [Backend Application, Backend Infrastructure, Backend Api, Frontend models, Frontend controllers, Frontend views, Persistencia PostgreSQL]
dificultad: "Alto"
sprint_sugerido: "Sprint 4"
dependencias: ["[[hu-002-shell-y-navegacion-por-rol]]", "[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-010-datos-sinteticos-de-desarrollo]]", "[[hu-013-radicar-ticket]]", "[[hu-014-adjuntos-en-radicacion]]", "[[hu-021-cambiar-estado-interno]]"]
relacionadas: ["[[hu-025-portal-coordinador-consulta-tickets]]", "[[hu-022-registrar-url-de-pr]]", "[[hu-034-portal-respuesta-formal]]", "[[hu-042-resumen-de-estados-en-portal]]", "[[hu-015-ajustar-prioridad-manualmente]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-024 — Portal: el solicitante consulta sus tickets

El solicitante lista y abre los tickets que él mismo radicó, con su `ClientStatus` y sus datos de radicación; nunca ve chat, estado interno, URL de PR, participantes internos ni ajustes internos, y cualquier ticket ajeno (de su empresa o de otra) responde 404 (PRD §5, §10, §14.1–§14.2, §14.10).

## Historia de usuario

**COMO** solicitante de una empresa cliente (`Requester`)  
**QUIERO** consultar la lista y el detalle de los tickets que radiqué  
**PARA** saber si mi caso fue recibido, si sigue en atención y qué datos envié

## Contexto

El portal se entrega en la Fase 1 por [[adr-0008-portal-cliente-en-fase-1]] para validar con el piloto CA-01, CA-02 y CA-10. Casos de uso del [[glosario]]: `ListClientTickets`, `GetClientTicket`; puerto de salida `IClientTicketQueries` ([[backend-hexagonal]]). La respuesta formal visible se añade en [[hu-034-portal-respuesta-formal|HU-034]] (Fase 2) y el resumen por estados en [[hu-042-resumen-de-estados-en-portal|HU-042]] (Fase 3). El alcance del coordinador está en [[hu-025-portal-coordinador-consulta-tickets|HU-025]]; ambas comparten endpoints y se diferencian por el alcance que calcula el backend.

## Alcance

- Lista paginada de los tickets cuyo `RequesterId` es el usuario actual, con filtro opcional por `ClientStatus`.
- Detalle de un ticket propio con datos de radicación y adjuntos de radicación.
- DTO de portal con lista de campos permitidos (allowlist), independiente del DTO interno.
- Vistas del portal: lista y detalle, con estados vacío y error.

## Fuera de alcance

- Respuesta formal en el portal ([[hu-034-portal-respuesta-formal|HU-034]]).
- Resumen/conteos por estado ([[hu-042-resumen-de-estados-en-portal|HU-042]]).
- Editar, cancelar o comentar un ticket (no está en el PRD); chat visible al cliente (PRD §13).
- Row-Level Security de PostgreSQL (decisión abierta, ver Riesgos en la épica).
- Exportación y vistas guardadas (PRD §10, §16.4).

## Requisitos y reglas de negocio

- El solicitante consulta los tickets creados por sí mismo (PRD §5, §10).
- Cada ticket muestra estado resumido y datos de radicación (PRD §10, §14.2).
- No se exponen chat, notas internas, adjuntos internos, participantes de Data Global ni estados de Desarrollo/PR/Producción (PRD §5 regla 6, §10, §14.10).
- Toda consulta del portal se restringe por empresa y permisos en el servidor (PRD §5 regla 2, §12).
- Un cliente de la empresa A no puede listar, consultar ni descargar desde el portal un ticket de B (PRD §14.1).
- Correspondencia de estados: `New → Received`; `InDevelopment`, `PullRequestReview`, `InProduction → InProgress`; `SolutionDelivered`; `Closed` (PRD §6.4).
- (inferencia de la wiki, no literal del PRD) Los adjuntos de radicación se muestran al cliente como parte de sus datos de radicación ("se entiende que sí", [[archivos-adjuntos]]).
- (propuesta) Se muestra la prioridad **calculada** (deriva de la urgencia y el impacto que el cliente indicó, PRD §6.1); el ajuste manual (`PriorityOverride`) y su motivo son internos y no se exponen.
- (propuesta) Ticket ajeno o inexistente → 404 con el mismo cuerpo, para no revelar existencia.

## Invariantes en juego

- AGENTS §7.1 — el solicitante solo ve sus tickets; filtrado en backend.
- AGENTS §7.2 — nunca chat, estados técnicos, URL de PR ni nombres de colaboradores internos.
- AGENTS §7.7 — los adjuntos mostrados ya fueron validados al radicar ([[hu-014-adjuntos-en-radicacion|HU-014]]).

## Criterios del PRD cubiertos

- PRD CA-01 (parcial: alcance del solicitante; el coordinador en HU-025; la descarga fuera del portal queda bajo el riesgo V-17) → [[criterios-de-aceptacion]]
- PRD CA-02 (parcial: estado resumido y datos de radicación del solicitante; la respuesta formal llega con HU-034) → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial: superficie del portal; los correos se verifican en HU-035) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-008-bandejas-y-portal-del-cliente]]
- Dependencias: [[hu-002-shell-y-navegacion-por-rol|HU-002]] (superficie del portal), [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]] (`ICurrentUser` con `CompanyId` y rol), [[hu-010-datos-sinteticos-de-desarrollo|HU-010]] (empresas A y B, solicitantes A1 y A2), [[hu-013-radicar-ticket|HU-013]], [[hu-014-adjuntos-en-radicacion|HU-014]] (adjuntos con contexto de radicación, hallazgo 4), [[hu-021-cambiar-estado-interno|HU-021]] (estados a mapear).
- Relacionadas: [[hu-025-portal-coordinador-consulta-tickets|HU-025]], [[hu-022-registrar-url-de-pr|HU-022]] (prueba de no exposición), [[hu-015-ajustar-prioridad-manualmente|HU-015]] (ajuste que no se expone), [[hu-034-portal-respuesta-formal|HU-034]], [[hu-042-resumen-de-estados-en-portal|HU-042]].
- Decisiones: [[adr-0008-portal-cliente-en-fase-1]], [[adr-0006-urls-publicas-azure-blob]] (V-17); Row-Level Security abierta en [[pendientes]] §4.

## Componentes afectados

- Backend Application: `ListClientTickets`, `GetClientTicket`, `IClientTicketQueries`, `ICurrentUser`.
- Backend Infrastructure: adaptador EF Core de `IClientTicketQueries` con filtro de alcance obligatorio (propuesta: filtro global de EF Core por `CompanyId` además del predicado por `RequesterId`).
- Backend Api: `GET /api/portal/tickets`, `GET /api/portal/tickets/{ticketId}` con política de usuario cliente.
- Frontend models/controllers/views: módulo `portal` (nombre propuesto).
- Persistencia PostgreSQL: índice `Ticket(CompanyId, RequesterId, CreatedAt)` (propuesta).

## Dificultad

**Nivel:** Alto

**Justificación:** es la superficie más sensible del producto: combina aislamiento multiempresa e intraempresa, una allowlist de campos que debe resistir cambios futuros del modelo, la inferencia sobre adjuntos y una decisión abierta (RLS); todo con backend y frontend coordinados.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01. Compartido con [[hu-025-portal-coordinador-consulta-tickets|HU-025]].

**`GET /api/portal/tickets?clientStatus=InProgress&page=1&pageSize=20`**

Respuesta `200 OK`:

```json
{
  "items": [
    {
      "id": "6f1c2b9e-0d4a-4c1e-9a51-3c2f8e7b1a20",
      "number": "DT-000123",
      "title": "No genera la factura electrónica",
      "category": { "id": "c-01", "name": "Error en módulo" },
      "urgency": "High",
      "impact": "Medium",
      "calculatedPriority": "High",
      "clientStatus": "InProgress",
      "requester": { "id": "9a7d...", "displayName": "Solicitante A1" },
      "createdAt": "2026-10-07T14:03:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1
}
```

**`GET /api/portal/tickets/{ticketId}`**

Respuesta `200 OK`:

```json
{
  "id": "6f1c2b9e-0d4a-4c1e-9a51-3c2f8e7b1a20",
  "number": "DT-000123",
  "title": "No genera la factura electrónica",
  "description": "Desde el lunes el módulo de facturación…",
  "category": { "id": "c-01", "name": "Error en módulo" },
  "urgency": "High",
  "impact": "Medium",
  "calculatedPriority": "High",
  "clientStatus": "InProgress",
  "requester": { "id": "9a7d...", "displayName": "Solicitante A1" },
  "createdAt": "2026-10-07T14:03:00Z",
  "attachments": [
    { "id": "a1b2...", "fileName": "error.png", "contentType": "image/png", "sizeBytes": 245760, "url": "https://<cuenta>.blob.core.windows.net/..." }
  ]
}
```

- Claves permitidas (allowlist): las de los ejemplos. [[hu-034-portal-respuesta-formal|HU-034]] añadirá `formalResponse`.
- Prohibidas siempre: `status`, `pullRequestUrl`, `participants`, `assignments`, `priorityOverride`, `effectivePriority`, mensajes de chat y adjuntos que no sean de radicación.

Errores (ProblemDetails):

| Código | Cuándo |
|---|---|
| 400 | `clientStatus` fuera de `ClientStatus`; `page < 1`; `pageSize` 0 o mayor que 50; `ticketId` no es GUID |
| 401 | Sin sesión |
| 403 | Usuario interno (propuesta: el portal es solo para roles cliente) |
| 404 | Ticket inexistente, de otra empresa o de otro solicitante de la misma empresa (mismo cuerpo) |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: ratificar rutas `/api/portal/tickets`, allowlist de campos, exposición de `calculatedPriority` y de adjuntos de radicación, 403 para internos, 404 uniforme y límites de paginación; confirmar con la persona responsable del producto la inferencia sobre adjuntos.
- [ ] **T-02 — Casos de uso** · Capa: Backend Application · Dificultad: Medio  
  Descripción: primero pruebas unitarias con dobles de `IClientTicketQueries` y `ICurrentUser`: el alcance (`RequesterId`, `CompanyId`) se calcula del usuario autenticado y nunca de parámetros de la petición; usuario interno → prohibido; no encontrado → resultado vacío. Luego `ListClientTickets` y `GetClientTicket`.
- [ ] **T-03 — Adaptador de consultas** · Capa: Backend Infrastructure · Dificultad: Alto  
  Descripción: implementar `IClientTicketQueries` con proyección directa a DTO de portal (nunca serializar la entidad), mapeo `TicketStatus → ClientStatus`, solo adjuntos de radicación y filtro de empresa obligatorio; pruebas de integración con PostgreSQL real y datos de dos empresas.
- [ ] **T-04 — Índice y migración** · Capa: Persistencia · Dificultad: Bajo  
  Descripción: migración EF Core del índice propuesto si no existe.
- [ ] **T-05 — Endpoints** · Capa: Backend Api · Dificultad: Medio  
  Descripción: endpoints con política de usuario cliente; pruebas con `WebApplicationFactory<Program>` de 200/400/401/403/404 y prueba de allowlist que compara el conjunto de claves del JSON con el esperado y busca cadenas prohibidas.
- [ ] **T-06 — Modelos y gateway** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: tipos `ClientTicketSummary`, `ClientTicketDetail` (sin campos internos), etiquetas de `ClientStatus` ("Recibido", "En atención", "Solución entregada", "Cerrado"), normalización que descarta claves no previstas y `clientTicketGateway`; pruebas Vitest.
- [ ] **T-07 — Controladores** · Capa: Frontend controllers · Dificultad: Bajo  
  Descripción: `useClientTicketListController` (filtro, paginación, `loading | ready | empty | error`) y `useClientTicketDetailController` (`notFound`); pruebas Vitest.
- [ ] **T-08 — Vistas** · Capa: Frontend views · Dificultad: Medio  
  Descripción: `ClientTicketListView` y `ClientTicketDetailView` puras, responsivas (PRD §12), con enlaces de adjuntos y sin ningún elemento de la superficie interna.

## Criterios de aceptación

### CHU-01 — Lista de los tickets propios

**Dado** el solicitante A1 con 3 tickets radicados, el solicitante A2 (misma empresa A) con 2 y la empresa B con 4  
**Cuando** A1 pide `GET /api/portal/tickets`  
**Entonces** recibe 200 con exactamente sus 3 tickets, `totalCount = 3`, ordenados por `createdAt` descendente.

### CHU-02 — Detalle propio con datos de radicación

**Dado** un ticket de A1 con descripción y un adjunto PDF de radicación  
**Cuando** A1 pide su detalle  
**Entonces** recibe 200 con `title`, `description`, `category`, `urgency`, `impact`, `calculatedPriority`, `clientStatus`, `createdAt` y el adjunto con su `url`.

### CHU-03 — Prueba negativa intraempresa y multiempresa

**Dado** el ticket T-A2 del solicitante A2 (empresa A) y el ticket T-B de la empresa B  
**Cuando** A1 pide `GET /api/portal/tickets/{T-A2}` y `GET /api/portal/tickets/{T-B}`  
**Entonces** ambas responden 404 con el mismo cuerpo que un GUID inexistente, y ninguno aparece en la lista ni en `totalCount` de A1.

### CHU-04 — Estado resumido mapeado

**Dado** tickets de A1 en `New`, `InDevelopment`, `PullRequestReview`, `InProduction`, `SolutionDelivered` y `Closed`  
**Cuando** A1 consulta la lista  
**Entonces** ve `Received`, `InProgress`, `InProgress`, `InProgress`, `SolutionDelivered` y `Closed` respectivamente; **y** `?clientStatus=InProgress` devuelve exactamente los tres intermedios.

### CHU-05 — No exposición verificada sobre el JSON

**Dado** un ticket de A1 en `PullRequestReview` con URL de PR, Laura y Julián como participantes, un ajuste de prioridad con motivo "Cliente estratégico" y un adjunto en el chat  
**Cuando** A1 pide la lista y el detalle  
**Entonces** las claves de cada objeto coinciden exactamente con la allowlist del contrato, y el cuerpo crudo no contiene `PullRequestReview`, la URL del PR, `Laura`, `Julián`, `Cliente estratégico` ni el nombre del adjunto del chat.

### CHU-06 — Validación de parámetros con valores límite

**Dado** A1 autenticado  
**Cuando** pide `pageSize=0`, `pageSize=51`, `page=0` o `clientStatus=InDevelopment`  
**Entonces** recibe 400 ProblemDetails; **y** `pageSize=50` responde 200.

### CHU-07 — Autorización por rol

**Dado** un usuario interno (Laura) y una petición sin sesión  
**Cuando** llaman a `GET /api/portal/tickets`  
**Entonces** Laura recibe 403 (propuesta) y la petición sin sesión 401.

### CHU-08 — UI vacía, 404 y error

**Dado** un solicitante sin tickets  
**Cuando** abre "Mis solicitudes"  
**Entonces** ve "Aún no has radicado solicitudes" con acceso a radicar ([[hu-013-radicar-ticket|HU-013]]); **al** abrir un ticket que responde 404 ve "Solicitud no encontrada" sin datos parciales; **y** ante un 500 ve un error con "Reintentar".

## Definition of Done

- [ ] CHU-01 a CHU-08 validados con evidencia.
- [ ] Pruebas en verde (`cd backend && dotnet test`): `GetClientTicket_SolicitanteDeMismaEmpresaAjeno_Devuelve404`, `GetClientTicket_OtraEmpresa_Devuelve404`, `ListClientTickets_NoCuentaTicketsAjenos`, `PortalTicket_ClavesCoincidenConAllowlist`, `PortalTicket_NoContieneCadenasInternas`, `ClientStatus_MapeaLosSeisEstados`.
- [ ] El alcance se calcula en el caso de uso a partir de `ICurrentUser` (prueba unitaria) y se refuerza en el adaptador (prueba de integración).
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Migración EF Core del índice creada y revisada (si aplica).
- [ ] Vitest, `npm --prefix frontend run lint` y `npm --prefix frontend run build` en verde.
- [ ] Contrato publicado en OpenAPI coherente con los tipos del frontend; el tipo del frontend no declara campos internos.
- [ ] El stack levanta con `docker compose up --build` y el portal muestra los tickets de un solicitante sintético.
- [ ] Wiki actualizada: [[roles-y-permisos]] (404 para tickets ajenos), [[estados-del-ticket]] (mapeo verificado), [[archivos-adjuntos]] (decisión sobre adjuntos de radicación en el portal), [[persistencia-postgresql]] (filtro multiempresa implementado), [[frontend-mvc]] (módulo `portal`); en [[criterios-de-aceptacion]] anotar la evidencia parcial de CA-01, CA-02 y CA-10.
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
| CHU-08 | Pendiente | — | — |
| DoD-01 Pruebas backend nombradas | Pendiente | — | — |
| DoD-02 Alcance en caso de uso y adaptador | Pendiente | — | — |
| DoD-03 Pruebas de arquitectura | Pendiente | — | — |
| DoD-04 Migración del índice | Pendiente | — | — |
| DoD-05 Vitest, lint y build | Pendiente | — | — |
| DoD-06 Contrato OpenAPI coherente | Pendiente | — | — |
| DoD-07 `docker compose up --build` | Pendiente | — | — |
| DoD-08 Wiki actualizada | Pendiente | — | — |
| DoD-09 `quality-reviewer` | Pendiente | — | — |
| DoD-10 PR revisado | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- **Riesgo (Row-Level Security):** decisión abierta; hasta resolverla, el aislamiento depende del filtro en backend y de estas pruebas negativas.
- **Riesgo V-17:** la `url` de los adjuntos es pública y permanente ([[adr-0006-urls-publicas-azure-blob]]); el portal solo la entrega para tickets visibles, pero quien la reciba puede abrirla fuera del portal.
- **Hallazgo 4:** sin contexto en `Attachment` no se puede separar adjuntos de radicación de los del chat; HU-014 debe definirlo antes de esta HU.
- (propuesta) Mostrar `requester.displayName` también al solicitante (es su propio nombre) simplifica compartir el DTO con el coordinador.

## Relacionado

- [[ep-008-bandejas-y-portal-del-cliente]] · [[tablero-scrum]] · [[adr-0008-portal-cliente-en-fase-1]] · [[roles-y-permisos]] · [[estados-del-ticket]]
- [[archivos-adjuntos]] · [[matriz-de-prioridad]] · [[persistencia-postgresql]] · [[backend-hexagonal]] · [[frontend-mvc]] · [[criterios-de-aceptacion]]
