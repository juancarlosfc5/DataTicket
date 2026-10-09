---
title: "HU-045 — Versionar y publicar plantillas"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/formularios, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §9", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-045", "Versionar y publicar plantillas", "Versionado de plantillas"]
epica: "[[ep-013-formularios-configurables]]"
criterios_prd: ["CA-15"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 9"
dependencias: ["[[hu-043-constructor-de-plantillas]]", "[[hu-044-asociar-plantilla-a-empresa]]", "[[hu-011-registrar-eventos-auditables]]"]
relacionadas: ["[[hu-046-radicar-con-formulario-de-empresa]]", "[[hu-047-consultar-por-campos-variables]]", "[[hu-012-consultar-bitacora-del-ticket]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-045 — Versionar y publicar plantillas

El administrador publica el borrador de una plantilla y el sistema crea una versión inmutable y numerada. Editar después crea una nueva versión al publicar; cada ticket conserva la versión y los campos con que se creó, así que cambiar la plantilla no altera tickets existentes (PRD §9).

## Historia de usuario

**COMO** administrador de DataTicket  
**QUIERO** publicar versiones numeradas e inmutables de una plantilla  
**PARA** cambiar el formulario de una empresa sin alterar los tickets ya radicados

## Contexto

PRD §9: "Versionado: cada ticket conserva los campos y la versión de formulario con que se creó". La plantilla tiene un borrador editable ([[hu-043-constructor-de-plantillas|HU-043]], [[hu-044-asociar-plantilla-a-empresa|HU-044]]); publicar congela su contenido (campos, tipos, opciones, ayuda, obligatoriedad y orden) en una versión. La versión vigente de la empresa es la última publicada de su plantilla asociada; la usa [[hu-046-radicar-con-formulario-de-empresa|HU-046]].

## Alcance

- Publicar el borrador como versión `n+1` (entidad propuesta `FormTemplateVersion`, ratificar en T-01 y registrar en el glosario).
- Inmutabilidad de las versiones publicadas.
- Listar versiones y consultar el contenido de cualquiera.
- Garantizar que los tickets referencian una versión concreta y siguen mostrando sus etiquetas aunque se publique otra.
- Regla de estabilidad de claves: una clave conserva su tipo en todas las versiones de la plantilla (propuesta, facilita [[hu-047-consultar-por-campos-variables|HU-047]]).
- Auditoría de la publicación.

## Fuera de alcance

- Programar publicaciones a futuro o revertir automáticamente a una versión anterior (no aparecen en el PRD; para volver atrás se edita el borrador y se publica de nuevo).
- Migrar respuestas de tickets antiguos a la nueva versión (contradice PRD §9).
- Radicar con la versión → [[hu-046-radicar-con-formulario-de-empresa|HU-046]].
- Comparador visual entre versiones (propuesta: mejora posterior).

## Requisitos y reglas de negocio

- Cada ticket conserva los campos y la versión de formulario con que se creó (PRD §9).
- Constructor administrado desde DataTicket; los clientes no modifican plantillas (PRD §9).
- Propuesta: publicar exige al menos un campo y un borrador válido; publicar sin cambios respecto de la última versión → 409.
- Propuesta: cambiar el tipo de una clave ya publicada → 400 (usar una clave nueva).
- Propuesta: la publicación es un evento auditable (`FormTemplatePublished`).
- La versión vigente para la empresa es la última publicada de su plantilla asociada.

## Invariantes en juego

- Invariante 8 (`AGENTS.md` §7): auditoría append-only de la publicación.
- Invariante 1 (indirecto): la versión pertenece a la plantilla de una sola empresa.

## Criterios del PRD cubiertos

- PRD CA-15 (parcial: la personalización evoluciona sin alterar tickets ni el formulario común) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-013-formularios-configurables]]
- Dependencias: [[hu-043-constructor-de-plantillas|HU-043]], [[hu-044-asociar-plantilla-a-empresa|HU-044]], [[hu-011-registrar-eventos-auditables|HU-011]].
- Relacionadas: [[hu-046-radicar-con-formulario-de-empresa|HU-046]], [[hu-047-consultar-por-campos-variables|HU-047]], [[hu-012-consultar-bitacora-del-ticket|HU-012]].
- Decisiones: abierta: almacenamiento de definiciones y respuestas variables (`jsonb` frente a tablas) → [[pendientes]].

## Componentes afectados

- Backend (Domain): `FormTemplateVersion` (propuesto) y regla de estabilidad de claves.
- Backend (Application): `PublishFormTemplate`, `ListFormTemplateVersions`, `GetFormTemplateVersion` (nombres propuestos).
- Backend (Infrastructure): tabla de versiones con instantánea de campos; migración.
- Backend (Api): endpoints de publicación y consulta.
- Frontend (models, controllers, views): acción "Publicar" e historial de versiones.

## Dificultad

**Nivel:** Medio

**Justificación:** la lógica de publicación es acotada, pero la inmutabilidad, la estabilidad de claves y la garantía sobre tickets existentes requieren pruebas de integración cuidadosas.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01.

**`POST /api/admin/form-templates/{formTemplateId}/publish`** (sin cuerpo) → `201` (`Location: .../versions/3`):

```json
{
  "formTemplateId": "e2b7…",
  "formTemplateVersionId": "51ac…",
  "versionNumber": 3,
  "publishedAt": "2026-10-07T17:05:00Z",
  "publishedBy": { "userId": "8f2e…", "displayName": "Administrador Demo" },
  "fieldCount": 3
}
```

**`GET /api/admin/form-templates/{formTemplateId}/versions`** → `200`: lista de `{ versionNumber, formTemplateVersionId, publishedAt, publishedBy, fieldCount }` de la más reciente a la más antigua.

**`GET /api/admin/form-templates/{formTemplateId}/versions/{versionNumber}`** → `200`:

```json
{
  "formTemplateVersionId": "51ac…",
  "versionNumber": 3,
  "publishedAt": "2026-10-07T17:05:00Z",
  "fields": [
    { "key": "centro_costo", "label": "Centro de costo", "type": "ShortText", "required": false, "helpText": "", "options": [], "order": 1 },
    { "key": "sede", "label": "Sede", "type": "SingleChoice", "required": true, "helpText": "Sede donde ocurre el caso", "options": ["Norte", "Sur", "Centro"], "order": 2 }
  ]
}
```

No existen `PUT`, `PATCH` ni `DELETE` sobre versiones (`405`).

| Código | Caso | `type` |
|---|---|---|
| 400 | Borrador sin campos o inválido; clave publicada con tipo distinto | `urn:dataticket:validation` |
| 401 | Sin sesión | — |
| 403 | PM (propuesta), Desarrollo, Producción o cliente | `urn:dataticket:forbidden` |
| 404 | Plantilla o versión inexistente | `urn:dataticket:not-found` |
| 405 | Intento de modificar o borrar una versión | — |
| 409 | Sin cambios desde la última versión; publicación concurrente | `urn:dataticket:conflict` |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar rutas, la entidad `FormTemplateVersion`, la regla de estabilidad de claves y "sin cambios → 409"; registrar nombres en [[glosario]].
- [ ] **T-02 — Dominio** · Capa: Backend (Domain) · Dificultad: Medio  
  Descripción: primero pruebas: `Publish_EmptyDraft_Throws`, `Publish_FirstTime_CreatesVersion1`, `Publish_AfterEdit_CreatesNextVersion`, `Publish_WithoutChanges_Throws`, `Publish_ChangingTypeOfPublishedKey_Throws`, `PublishedVersion_IsImmutable`. Luego `FormTemplate.Publish(actor, now)` y `FormTemplateVersion` como instantánea.
- [ ] **T-03 — Caso de uso** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: primero pruebas: `PublishFormTemplate_ByDeveloper_ReturnsForbidden`, `PublishFormTemplate_WritesAuditWithVersionNumber`, `TicketWithVersion1_KeepsLabelsAfterVersion2`. Luego implementación con `IClock` e `IAuditLog`.
- [ ] **T-04 — Persistencia y migración** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: tabla de versiones con índice único `(form_template_id, version_number)` y la instantánea según el ADR de almacenamiento; migración `AddFormTemplateVersions`; prueba de integración de publicación concurrente (una gana, otra 409).
- [ ] **T-05 — Endpoints** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: primero pruebas de integración (201, 200, 400, 403, 404, 405, 409). Luego endpoints.
- [ ] **T-06 — Frontend** · Capa: Frontend (models/controllers/views) · Dificultad: Medio  
  Descripción: Vitest del modelo (normalización de versiones, orden descendente); `useFormTemplateVersionsController` con `publish()`; vistas: botón "Publicar versión" con confirmación ("Los tickets nuevos de la empresa usarán esta versión; los existentes no cambian"), `FormTemplateVersionsView` y vista de solo lectura de una versión.

## Criterios de aceptación

### CHU-01 — Primera publicación

**Dado** una plantilla asociada a la empresa A con un borrador válido de tres campos  
**Cuando** el administrador la publica  
**Entonces** recibe `201` con `versionNumber: 1`, la versión contiene los tres campos en el orden del borrador y la empresa A pasa a `effectiveForm: "Template"`.

### CHU-02 — Nueva versión tras editar

**Dado** la versión 1 publicada  
**Cuando** el administrador cambia la etiqueta de `sede` a "Sede principal", agrega `placa` y publica  
**Entonces** recibe `201` con `versionNumber: 2`, y `GET .../versions/1` sigue devolviendo la etiqueta "Sede" y tres campos.

### CHU-03 — Tickets existentes no cambian

**Dado** un ticket de la empresa A radicado con la versión 1 y la respuesta `sede = "Norte"`  
**Cuando** se publica la versión 2  
**Entonces** el detalle interno y el del portal de ese ticket siguen mostrando "Sede: Norte" con la etiqueta de la versión 1 y referencian `versionNumber: 1`.

### CHU-04 — Versiones inmutables

**Dado** la versión 1 publicada  
**Cuando** se envía `PUT` o `DELETE` a `/api/admin/form-templates/{id}/versions/1`  
**Entonces** recibe `405` y la versión no cambia.

### CHU-05 — Validaciones de publicación

**Dado** una plantilla con borrador vacío, otra sin cambios desde la versión 2 y otra cuyo borrador cambió `sede` de `SingleChoice` a `ShortText`  
**Cuando** el administrador intenta publicarlas  
**Entonces** recibe `400`, `409` y `400` respectivamente, y no se crean versiones.

### CHU-06 — Autorización

**Dado** un desarrollador y un coordinador de empresa  
**Cuando** llaman `POST /api/admin/form-templates/{id}/publish`  
**Entonces** reciben `403` y no se crea versión.

### CHU-07 — Auditoría

**Dado** la publicación de la versión 2  
**Cuando** se consulta la auditoría  
**Entonces** existe una entrada `FormTemplatePublished` con actor, fecha/hora, plantilla, valor anterior `1` y valor nuevo `2`.

### CHU-08 — Publicación concurrente

**Dado** dos administradores que publican la misma plantilla a la vez  
**Cuando** se procesan ambas peticiones  
**Entonces** una recibe `201` y la otra `409`, y no hay números de versión duplicados.

### CHU-09 — Interfaz

**Dado** el administrador en el historial de versiones  
**Cuando** publica y el backend responde `409` por falta de cambios  
**Entonces** la vista muestra "No hay cambios desde la última versión publicada" y no altera el historial.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-09 validados con evidencia en la matriz.
- [ ] **DoD-02** — Pruebas escritas primero y en verde (`cd backend && dotnet test`): `Publish_*`, `PublishedVersion_IsImmutable`, `TicketWithVersion1_KeepsLabelsAfterVersion2` e integración con PostgreSQL real.
- [ ] **DoD-03** — `DataTicket.ArchitectureTests` en verde.
- [ ] **DoD-04** — Migración `AddFormTemplateVersions` creada y aplicada en local, conforme al ADR de almacenamiento.
- [ ] **DoD-05** — Entrada `FormTemplatePublished` verificada por prueba.
- [ ] **DoD-06** — Frontend: `npm --prefix frontend test`, `npm --prefix frontend run lint` y `npm --prefix frontend run build` correctos.
- [ ] **DoD-07** — `docker compose up --build` y verificación manual: publicar v1, editar, publicar v2, revisar ambas.
- [ ] **DoD-08** — Wiki actualizada: [[formularios-configurables]] (ciclo borrador → versión), [[modelo-de-dominio]] (`FormTemplateVersion`), [[glosario]], [[persistencia-postgresql]], [[auditoria]] (evento nuevo).
- [ ] **DoD-09** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO y PR revisado por otra persona del equipo.
- [ ] **DoD-10** — La trazabilidad de la HU y de [[ep-013-formularios-configurables]] está actualizada.

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

- **Propuesta:** "sin cambios → 409" y "clave publicada conserva su tipo".
- **Propuesta:** el orden de campos forma parte de la versión.
- CHU-03 necesita tickets con versión: se valida con datos sembrados en la prueba de integración o tras [[hu-046-radicar-con-formulario-de-empresa|HU-046]].

## Relacionado

- [[ep-013-formularios-configurables]] · [[tablero-scrum]] · [[formularios-configurables]] · [[modelo-de-dominio]] · [[persistencia-postgresql]] · [[auditoria]] · [[criterios-de-aceptacion]]
