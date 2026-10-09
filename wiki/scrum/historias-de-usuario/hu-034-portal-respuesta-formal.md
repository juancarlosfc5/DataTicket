---
title: "HU-034 — Consultar la respuesta formal en el portal"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/portal, producto/respuesta-formal, producto/seguridad]
sources: ["PRD.md §5", "PRD.md §6.4", "PRD.md §6.5", "PRD.md §8", "PRD.md §10", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-034", "Consultar la respuesta formal en el portal", "Portal: respuesta formal"]
epica: "[[ep-010-respuesta-formal-y-cierre]]"
criterios_prd: ["CA-01", "CA-02", "CA-10", "CA-11"]
componentes: ["Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 7"
dependencias: ["[[hu-033-emitir-respuesta-formal]]", "[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-025-portal-coordinador-consulta-tickets]]"]
relacionadas: ["[[hu-035-correo-de-respuesta-formal]]", "[[hu-036-cerrar-ticket-manualmente]]", "[[hu-042-resumen-de-estados-en-portal]]", "[[hu-022-registrar-url-de-pr]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-034 — Consultar la respuesta formal en el portal

El solicitante (sus tickets) y el coordinador (los de su empresa) ven en el detalle del portal el cuerpo y los adjuntos de la respuesta formal cuando existe, con estado resumido "Solución entregada", sin ningún dato interno de Data Global (PRD §6.5, §10, §14.2).

## Historia de usuario

**COMO** solicitante o coordinador de una empresa cliente  
**QUIERO** leer en el portal la respuesta formal de mi ticket y descargar sus archivos  
**PARA** conocer la solución entregada por Data Global y conservarla como referencia

## Contexto

La respuesta formal "queda disponible en el portal" (PRD §6.5) y cada ticket del portal muestra estado resumido, datos de radicación y la respuesta formal cuando esté disponible (PRD §10). La consulta básica del portal ya existe desde la Fase 1 ([[adr-0008-portal-cliente-en-fase-1]], [[hu-024-portal-solicitante-consulta-tickets|HU-024]], [[hu-025-portal-coordinador-consulta-tickets|HU-025]]); esta HU amplía el detalle con la respuesta.

El PRD prohíbe mostrar al cliente nombres de colaboradores internos (PRD §5.6). Elizabeth es colaboradora interna, así que el emisor se muestra de forma institucional (propuesta: "Data Global").

## Alcance

- Ampliar el DTO del detalle del portal (`GetClientTicket`) con `formalResponse` (cuerpo, fecha de entrega, emisor institucional y adjuntos con `context = FormalResponse`).
- Filtrar en backend por empresa y rol, igual que el detalle existente (`IClientTicketQueries`).
- Mostrar `ClientStatus` `SolutionDelivered` o `Closed` y la respuesta en ambos.
- Vista del portal con el texto (respetando saltos de línea) y la lista de archivos descargables.

## Fuera de alcance

- Emitir la respuesta → [[hu-033-emitir-respuesta-formal|HU-033]].
- Correo de la respuesta → [[hu-035-correo-de-respuesta-formal|HU-035]].
- Responder o comentar la respuesta formal desde el portal (PRD §13: sin chat visible al cliente).
- Encuesta de satisfacción (PRD §13).
- Resumen de estados del portal → [[hu-042-resumen-de-estados-en-portal|HU-042]].

## Requisitos y reglas de negocio

- El solicitante consulta los tickets creados por sí mismo; el coordinador, los de toda su empresa (PRD §5, §10).
- Cada ticket muestra estado resumido, datos de radicación y la respuesta formal final cuando esté disponible (PRD §10).
- No se exponen chat, notas internas, adjuntos internos, participantes de Data Global ni estados de Desarrollo/PR/Producción (PRD §5.6, §10).
- Toda consulta del portal se restringe por empresa y permisos en el servidor (PRD §12).
- Los archivos tienen URL pública permanente: la descarga desde el portal está protegida, pero la URL no (PRD §8, [[adr-0006-urls-publicas-azure-blob]]).
- Propuesta: el emisor se muestra como "Data Global", sin nombre de persona.
- Propuesta: un ticket ajeno o de otra empresa responde `404` (no `403`) para no confirmar su existencia.

## Invariantes en juego

- Invariante 1 (`AGENTS.md` §7): el cliente solo ve tickets de su empresa; el solicitante, los suyos.
- Invariante 2: el cliente nunca ve chat, estados técnicos, URL de PR ni nombres internos.

## Criterios del PRD cubiertos

- PRD CA-11 (parcial: la respuesta y sus adjuntos quedan en el portal) → [[criterios-de-aceptacion]]
- PRD CA-02 (parcial: la respuesta formal es visible al solicitante y al coordinador) → [[criterios-de-aceptacion]]
- PRD CA-01 (parcial: no se consulta ni descarga desde el portal la respuesta de otra empresa) → [[criterios-de-aceptacion]]
- PRD CA-10 (parcial: la respuesta no expone nombres internos ni estados técnicos) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-010-respuesta-formal-y-cierre]]
- Dependencias: [[hu-033-emitir-respuesta-formal|HU-033]] (respuesta y marca de contexto de adjuntos), [[hu-024-portal-solicitante-consulta-tickets|HU-024]] y [[hu-025-portal-coordinador-consulta-tickets|HU-025]] (detalle del portal filtrado).
- Relacionadas: [[hu-035-correo-de-respuesta-formal|HU-035]], [[hu-036-cerrar-ticket-manualmente|HU-036]] (estado `Closed`), [[hu-042-resumen-de-estados-en-portal|HU-042]], [[hu-022-registrar-url-de-pr|HU-022]] (URL de PR que nunca debe salir).
- Decisiones: [[adr-0008-portal-cliente-en-fase-1]], [[adr-0006-urls-publicas-azure-blob]]; abierta: Row-Level Security ([[pendientes]] §4), V-13.

## Componentes afectados

- Backend (Application): `GetClientTicket` y su DTO de portal.
- Backend (Infrastructure): consulta EF Core de `IClientTicketQueries` con proyección explícita.
- Backend (Api): endpoint del portal ya existente (ampliado).
- Frontend (models, controllers, views): módulo del portal.

## Dificultad

**Nivel:** Medio

**Justificación:** amplía un endpoint y una vista existentes, pero exige pruebas negativas multiempresa y por rol y una prueba de no exposición del JSON (lista blanca de campos).

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01 sobre el contrato de [[hu-024-portal-solicitante-consulta-tickets|HU-024]], que ya prevé añadir `formalResponse` a su lista blanca.

**`GET /api/portal/tickets/{ticketId}`** (roles `Requester`, `CompanyCoordinator`) → `200` (campos de HU-024 más `formalResponse`):

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
  "clientStatus": "SolutionDelivered",
  "requester": { "id": "9a7d…", "displayName": "Solicitante A1" },
  "createdAt": "2026-10-01T14:03:11Z",
  "attachments": [
    { "id": "a1b2…", "fileName": "error.png", "contentType": "image/png", "sizeBytes": 245760, "url": "http://localhost:10000/devstoreaccount1/attachments/…/error.png" }
  ],
  "formalResponse": {
    "body": "Se corrigió el cálculo de retenciones en el módulo de facturación…",
    "deliveredAt": "2026-10-07T20:15:42Z",
    "issuedBy": "Data Global",
    "attachments": [
      { "id": "a4e1…", "fileName": "acta-de-solucion.pdf", "contentType": "application/pdf", "sizeBytes": 248331, "url": "http://localhost:10000/devstoreaccount1/attachments/…/acta-de-solucion.pdf" }
    ]
  }
}
```

- `attachments` (raíz) son solo los de radicación, como en HU-024; los de la respuesta van anidados en `formalResponse.attachments`.
- `formalResponse` es `null` mientras no exista. `number` es un ejemplo: el formato está pendiente (V-16).
- Lista blanca: además de lo que prohíbe HU-024, el DTO **no** contiene `previousStatus`, `deliveredBy`, `emailDelivery` ni adjuntos con `context = Chat`.

Errores (ProblemDetails):

| Código | Caso |
|---|---|
| 401 | Sin sesión |
| 403 | Usuario interno (PM, administrador, Desarrollo, Producción) que llama a una ruta del portal (propuesta) |
| 404 | Ticket inexistente, de otra empresa o, para el solicitante, de otro solicitante de su empresa |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar el campo `formalResponse`, la lista blanca del DTO y el texto institucional del emisor ("Data Global", propuesta); confirmar 404 frente a 403 para tickets ajenos.
- [ ] **T-02 — Consulta del portal** · Capa: Backend (Application/Infrastructure) · Dificultad: Medio  
  Descripción: primero pruebas: `GetClientTicket_WithFormalResponse_ReturnsBodyAndAttachments`, `GetClientTicket_ExcludesChatAttachments`, `GetClientTicket_DoesNotExposeIssuerName`. Luego proyección explícita en `IClientTicketQueries` (sin cargar la entidad completa), filtrando adjuntos por `AttachmentContext` (`Submission`, `FormalResponse`).
- [ ] **T-03 — Pruebas de aislamiento** · Capa: Backend (Api) · Dificultad: Medio  
  Descripción: integración con `WebApplicationFactory<Program>` y PostgreSQL real: coordinador de A → ticket de B = 404; solicitante A1 → ticket de A2 = 404; desarrollador → 403; prueba de serialización que falla si aparece cualquier clave fuera de la lista blanca.
- [ ] **T-04 — Modelo** · Capa: Frontend (models) · Dificultad: Bajo  
  Descripción: primero Vitest: normalización de `formalResponse` (`null`, sin adjuntos, con adjuntos; formato de tamaño). Luego ampliar el tipo y el gateway del portal.
- [ ] **T-05 — Controlador y vista** · Capa: Frontend (controllers/views) · Dificultad: Bajo  
  Descripción: el controlador del detalle del portal expone `formalResponse`; `PortalFormalResponseView` muestra texto con saltos de línea, fecha en `America/Bogota`, "Data Global" como emisor y enlaces de descarga (`rel="noopener noreferrer"`); mensaje "Aún no hay respuesta formal" cuando es `null`.

## Criterios de aceptación

### CHU-01 — El solicitante ve la respuesta de su ticket

**Dado** un ticket del solicitante S (empresa A) con respuesta formal emitida y un PDF adjunto  
**Cuando** S abre el detalle en el portal  
**Entonces** ve el estado "Solución entregada", el cuerpo completo, la fecha de entrega, "Data Global" como emisor y el PDF descargable.

### CHU-02 — El coordinador ve la respuesta de cualquier ticket de su empresa

**Dado** un ticket radicado por otro solicitante de la empresa A con respuesta emitida  
**Cuando** el coordinador de A llama `GET /api/portal/tickets/{ticketId}`  
**Entonces** recibe `200` con `formalResponse` completo.

### CHU-03 — Aislamiento multiempresa A↔B

**Dado** un ticket de la empresa B con respuesta formal  
**Cuando** el coordinador de A y un solicitante de A llaman al detalle del portal con su `ticketId`  
**Entonces** ambos reciben `404` y la respuesta no contiene cuerpo ni URL de archivos de B.

### CHU-04 — Aislamiento entre solicitantes de la misma empresa

**Dado** un ticket radicado por el solicitante A2 con respuesta formal  
**Cuando** el solicitante A1 (misma empresa, sin rol de coordinador) pide su detalle  
**Entonces** recibe `404`.

### CHU-05 — Sin datos internos en la respuesta del portal

**Dado** un ticket con respuesta formal, chat con mensajes e imágenes, URL de PR registrada y tres participantes internos  
**Cuando** el solicitante obtiene el JSON del detalle  
**Entonces** el JSON no contiene `status`, `deliveredBy`, `participants`, `pullRequestUrl` ni mensajes, no aparece el nombre de la persona emisora, y solo lista adjuntos de radicación y de la respuesta formal (ningún adjunto del chat).

### CHU-06 — Respuesta aún no disponible

**Dado** un ticket en `InDevelopment` sin respuesta formal  
**Cuando** el solicitante abre el detalle  
**Entonces** el JSON trae `formalResponse: null` y `clientStatus: "InProgress"`, y la vista muestra "Aún no hay respuesta formal".

### CHU-07 — Ticket cerrado conserva la respuesta

**Dado** un ticket cerrado manualmente tras la respuesta ([[hu-036-cerrar-ticket-manualmente|HU-036]])  
**Cuando** el coordinador abre el detalle  
**Entonces** ve `clientStatus: "Closed"` y la misma respuesta formal con sus adjuntos.

### CHU-08 — Usuarios internos no usan el portal

**Dado** un desarrollador participante del ticket  
**Cuando** llama `GET /api/portal/tickets/{ticketId}`  
**Entonces** recibe `403` (propuesta) y debe usar el detalle interno.

### CHU-09 — Error de carga en la interfaz

**Dado** que el backend responde `404` o un error de red  
**Cuando** el cliente abre el detalle  
**Entonces** la vista muestra "No encontramos este ticket" o "No fue posible cargar el ticket. Intenta de nuevo" con opción de reintento, sin mostrar datos parciales de otra consulta.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-09 validados con evidencia en la matriz.
- [ ] **DoD-02** — Pruebas escritas primero y en verde (`cd backend && dotnet test`): `GetClientTicket_*` unitarias y de integración, incluidas `PortalTicket_FromOtherCompany_Returns404`, `PortalTicket_FromOtherRequester_Returns404`, `PortalTicketJson_ContainsOnlyWhitelistedFields`.
- [ ] **DoD-03** — `DataTicket.ArchitectureTests` en verde.
- [ ] **DoD-04** — Frontend: Vitest del modelo en verde, `npm --prefix frontend run lint` y `npm --prefix frontend run build` correctos.
- [ ] **DoD-05** — Contrato del portal coherente entre OpenAPI y el gateway; lista blanca documentada.
- [ ] **DoD-06** — `docker compose up --build` y verificación manual: el PDF de la respuesta se descarga desde el portal (Azurite).
- [ ] **DoD-07** — Wiki actualizada: [[roles-y-permisos]] (emisor institucional), [[estados-del-ticket]], [[dashboard-y-metricas]] (sección portal) y nota para [[criterios-de-aceptacion]] (CA-02, CA-10, CA-11 con prueba).
- [ ] **DoD-08** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO y PR revisado por otra persona del equipo.
- [ ] **DoD-09** — La trazabilidad de la HU y de [[ep-010-respuesta-formal-y-cierre]] está actualizada.

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

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.
- 2026-10-09 — HU aprobada (`Aprobada`) por juancarlosfc5, líder del proyecto, en la aprobación explícita en bloque de todas las épicas e HU del backlog. Las decisiones abiertas listadas en Riesgos siguen condicionando la implementación de las tareas afectadas.

## Notas y decisiones

- **Propuesta:** emisor mostrado como "Data Global" (PRD §5.6 prohíbe nombres de colaboradores internos).
- **Propuesta:** 404 ante ticket ajeno o de otra empresa; 403 para usuarios internos en rutas del portal.
- **Riesgo aceptado:** las URL de los adjuntos son públicas y permanentes ([[adr-0006-urls-publicas-azure-blob]]); CA-01 protege la descarga desde el portal, no la URL (V-17).
- Si V-13 permite varias respuestas, `formalResponse` pasaría a ser una lista; se revisaría el contrato.

## Relacionado

- [[ep-010-respuesta-formal-y-cierre]] · [[tablero-scrum]] · [[roles-y-permisos]] · [[estados-del-ticket]] · [[archivos-adjuntos]] · [[dashboard-y-metricas]] · [[frontend-mvc]] · [[criterios-de-aceptacion]] · [[adr-0008-portal-cliente-en-fase-1]] · [[adr-0006-urls-publicas-azure-blob]]
