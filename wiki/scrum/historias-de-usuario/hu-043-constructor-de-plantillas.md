---
title: "HU-043 — Constructor de plantillas de formulario"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/formularios, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.1", "PRD.md §9", "PRD.md §12", "PRD.md §13", "PRD.md §14"]
aliases: ["HU-043", "Constructor de plantillas de formulario", "Constructor de plantillas"]
epica: "[[ep-013-formularios-configurables]]"
criterios_prd: ["CA-15"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 9"
dependencias: ["[[hu-004-contexto-de-usuario-y-autorizacion]]", "[[hu-007-administrar-empresas-cliente]]", "[[hu-011-registrar-eventos-auditables]]", "[[hu-013-radicar-ticket]]"]
relacionadas: ["[[hu-044-asociar-plantilla-a-empresa]]", "[[hu-045-versionar-plantillas]]", "[[hu-046-radicar-con-formulario-de-empresa]]", "[[hu-047-consultar-por-campos-variables]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-043 — Constructor de plantillas de formulario

Un administrador de DataTicket crea y edita, desde el espacio interno, plantillas de formulario (`FormTemplate`) en borrador con campos adicionales configurables: tipo, etiqueta, obligatoriedad, ayuda y opciones. Los clientes no crean ni modifican plantillas (PRD §9).

## Historia de usuario

**COMO** administrador de DataTicket  
**QUIERO** diseñar plantillas de formulario con campos adicionales tipados  
**PARA** preparar el formulario de radicación que necesita cada empresa cliente sin tocar código

## Contexto

La Fase 4 incluye un "constructor de formularios administrado desde DataTicket" con "campos configurables por cliente, con tipo, etiqueta, obligatoriedad, ayuda y opciones" (PRD §9). No se requiere que cada cliente cree o modifique sus plantillas (PRD §9). Los ejemplos del prototipo (sede, servicio afectado, planta, centro de costo, bodega, número de pedido, sección o periodo académico, placa de vehículo) son semillas de diseño, no campos reales (PRD §9). Esta HU cubre el borrador; la asociación y el orden van en [[hu-044-asociar-plantilla-a-empresa|HU-044]] y la publicación en [[hu-045-versionar-plantillas|HU-045]].

## Alcance

- Crear, listar, consultar y editar plantillas en estado borrador.
- Definir campos con clave estable, tipo, etiqueta, obligatoriedad, ayuda y opciones.
- Tipos de campo propuestos (`FormFieldType`, nombre propuesto): `ShortText`, `LongText`, `Number`, `Date`, `SingleChoice`, `MultipleChoice`.
- Validación en backend de la definición (límites y claves reservadas).
- Eliminar una plantilla que nunca se publicó.
- Vista previa del formulario tal como lo vería el cliente (sin enviar).
- Auditoría de creación, edición y eliminación de plantillas (propuesta).

## Fuera de alcance

- Asociar a una empresa y reordenar → [[hu-044-asociar-plantilla-a-empresa|HU-044]].
- Publicar y versionar → [[hu-045-versionar-plantillas|HU-045]].
- Marcar campos como consultables → [[hu-047-consultar-por-campos-variables|HU-047]].
- Edición de plantillas por clientes (PRD §9).
- Modificar o eliminar campos núcleo (categoría, título, descripción, urgencia, impacto, adjuntos) (PRD §9).
- Lógica condicional, cálculos, campos de archivo dentro de la plantilla y catálogo de categorías (V-12).

## Requisitos y reglas de negocio

- Constructor administrado desde DataTicket (PRD §9).
- Campos con tipo, etiqueta, obligatoriedad, ayuda y opciones (PRD §9).
- Los clientes no crean ni modifican plantillas (PRD §9).
- Se conservan los campos núcleo (PRD §9); claves reservadas: `category`, `title`, `description`, `urgency`, `impact`, `priority`, `attachments`, `company`, `requester`.
- Autorización en servidor en cada operación (PRD §5.2).
- Propuesta: solo `Administrator` usa el constructor; si la PM también debe hacerlo, se habilita al resolver la duda.
- Límites propuestos: nombre de plantilla 1–100 caracteres; máximo 30 campos; clave `^[a-z][a-z0-9_]{1,39}$` única en la plantilla; etiqueta 1–80; ayuda 0–300; opciones solo en `SingleChoice`/`MultipleChoice`, entre 2 y 50, de 1–80 caracteres, únicas sin distinguir mayúsculas.

## Invariantes en juego

- Invariante 2 (`AGENTS.md` §7): la configuración interna no se expone a clientes; solo verán el formulario publicado de su empresa (HU-046).
- Invariante 8: auditoría append-only de los cambios de plantilla (propuesta).
- Ningún otro invariante en juego directo.

## Criterios del PRD cubiertos

- PRD CA-15 (parcial: la personalización se construye como fase separada sin tocar el formulario común) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-013-formularios-configurables]]
- Dependencias: [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]] (rol `Administrator`), [[hu-007-administrar-empresas-cliente|HU-007]] (espacio de administración), [[hu-011-registrar-eventos-auditables|HU-011]] (`IAuditLog`), [[hu-013-radicar-ticket|HU-013]] (campos núcleo que no se tocan).
- Relacionadas: [[hu-044-asociar-plantilla-a-empresa|HU-044]], [[hu-045-versionar-plantillas|HU-045]], [[hu-046-radicar-con-formulario-de-empresa|HU-046]] (reutiliza el renderizador de la vista previa), [[hu-047-consultar-por-campos-variables|HU-047]].
- Decisiones: abiertas V-12 (categorías), enrutador y estado del frontend, almacenamiento de definiciones y respuestas → [[pendientes]].

## Componentes afectados

- Backend (Domain): `FormTemplate` (glosario), `FormField` y `FormFieldType` (nombres propuestos).
- Backend (Application): casos de uso `CreateFormTemplate`, `UpdateFormTemplateDraft`, `DeleteFormTemplate`, `GetFormTemplate`, `ListFormTemplates` (nombres propuestos); repositorio `IFormTemplateRepository` (propuesto).
- Backend (Infrastructure): configuración EF Core y migración.
- Backend (Api): endpoints `/api/admin/form-templates`.
- Frontend (models, controllers, views): módulo de administración de plantillas (nombre propuesto `form-templates`).

## Dificultad

**Nivel:** Alto

**Justificación:** introduce un agregado nuevo con validación estructural rica (tipos, opciones, claves reservadas), persistencia de una definición flexible y un editor visual con vista previa. Se acotó a borradores para no llegar a `Muy alto`.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01.

| Método | Ruta | Uso | Éxito |
|---|---|---|---|
| POST | `/api/admin/form-templates` | Crear borrador | 201 |
| GET | `/api/admin/form-templates` | Listar (id, nombre, estado, empresa asociada, versión publicada) | 200 |
| GET | `/api/admin/form-templates/{formTemplateId}` | Detalle con el borrador | 200 |
| PUT | `/api/admin/form-templates/{formTemplateId}/draft` | Reemplazar nombre y campos del borrador | 200 |
| DELETE | `/api/admin/form-templates/{formTemplateId}` | Eliminar si nunca se publicó | 204 |

`PUT /api/admin/form-templates/{formTemplateId}/draft`:

```json
{
  "name": "Formulario Empresa Demo Norte",
  "fields": [
    { "key": "sede", "label": "Sede", "type": "SingleChoice", "required": true, "helpText": "Sede donde ocurre el caso", "options": ["Norte", "Sur", "Centro"] },
    { "key": "centro_costo", "label": "Centro de costo", "type": "ShortText", "required": false, "helpText": "", "options": [] },
    { "key": "fecha_evento", "label": "Fecha del evento", "type": "Date", "required": false, "helpText": "Formato AAAA-MM-DD", "options": [] }
  ]
}
```

Respuesta `200`:

```json
{
  "formTemplateId": "e2b7…",
  "name": "Formulario Empresa Demo Norte",
  "status": "Draft",
  "companyId": null,
  "latestPublishedVersion": null,
  "draftUpdatedAt": "2026-10-07T16:40:00Z",
  "fields": [ { "key": "sede", "label": "Sede", "type": "SingleChoice", "required": true, "helpText": "Sede donde ocurre el caso", "options": ["Norte", "Sur", "Centro"] } ]
}
```

(Los valores del ejemplo son datos sintéticos de diseño, no campos reales de ningún cliente.)

| Código | Caso | `type` |
|---|---|---|
| 400 | Nombre vacío o > 100; > 30 campos; clave inválida, duplicada o reservada; etiqueta vacía o > 80; ayuda > 300; opciones en tipo no seleccionable; < 2 o > 50 opciones; opción duplicada | `urn:dataticket:validation` (con `errors` por ruta, p. ej. `fields[1].key`) |
| 401 | Sin sesión | — |
| 403 | PM (propuesta), Desarrollo, Producción o cliente | `urn:dataticket:forbidden` |
| 404 | Plantilla inexistente | `urn:dataticket:not-found` |
| 409 | Eliminar una plantilla con versiones publicadas; edición concurrente (token de concurrencia) | `urn:dataticket:conflict` |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: ratificar rutas, DTO, tipos de campo, límites y claves reservadas; confirmar con la persona usuaria si la PM usa el constructor; registrar `FormField`, `FormFieldType` en [[glosario]]; acordar con HU-045/HU-046 el almacenamiento de la definición (mismo ADR que las respuestas variables).
- [ ] **T-02 — Dominio** · Capa: Backend (Domain) · Dificultad: Medio  
  Descripción: primero pruebas: `Draft_WithReservedKey_Throws`, `Draft_With31Fields_Throws`, `Draft_With30Fields_IsValid`, `ChoiceField_WithOneOption_Throws`, `ChoiceField_With51Options_Throws`, `TextField_WithOptions_Throws`, `DuplicateOptionsIgnoringCase_Throws`. Luego `FormTemplate` con borrador, `FormField` y `FormFieldType`.
- [ ] **T-03 — Casos de uso** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: primero pruebas: `CreateFormTemplate_ByDeveloper_ReturnsForbidden`, `CreateFormTemplate_ByClient_ReturnsForbidden`, `UpdateDraft_ByAdmin_WritesAudit`, `DeletePublishedTemplate_ReturnsConflict`. Luego los casos de uso con `ICurrentUser`, `IClock` e `IAuditLog`.
- [ ] **T-04 — Persistencia y migración** · Capa: Backend (Infrastructure) · Dificultad: Medio  
  Descripción: configuración EF Core según el ADR de almacenamiento, token de concurrencia y migración `AddFormTemplates`; prueba de integración ida y vuelta con PostgreSQL real.
- [ ] **T-05 — Endpoints** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: primero pruebas de integración (201, 200, 400 con `errors` por ruta, 403 por rol, 409). Luego endpoints minimal API bajo `/api/admin/form-templates`.
- [ ] **T-06 — Modelo** · Capa: Frontend (models) · Dificultad: Medio  
  Descripción: primero Vitest con las mismas reglas que el dominio (claves reservadas, límites, opciones). Luego tipos, validación y `formTemplatesGateway.ts`.
- [ ] **T-07 — Controlador** · Capa: Frontend (controllers) · Dificultad: Medio  
  Descripción: `useFormTemplateBuilderController`: agregar, editar y quitar campos; cambiar tipo (limpia opciones si deja de ser seleccionable); guardar; errores del backend por ruta; aviso de cambios sin guardar.
- [ ] **T-08 — Vistas** · Capa: Frontend (views) · Dificultad: Medio  
  Descripción: `FormTemplateListView`, `FormTemplateBuilderView`, `FormFieldEditorView` y `DynamicFormPreviewView` (renderizador puro reutilizable por HU-046), con etiquetas accesibles.

## Criterios de aceptación

### CHU-01 — Crear y guardar un borrador

**Dado** un administrador autenticado  
**Cuando** crea la plantilla "Formulario Empresa Demo Norte" y guarda tres campos (`SingleChoice` con 3 opciones, `ShortText` y `Date`)  
**Entonces** recibe `201` y luego `200`, el detalle devuelve los tres campos en el orden enviado con `status: "Draft"` y `companyId: null`.

### CHU-02 — Límites de campos

**Dado** un administrador editando un borrador  
**Cuando** guarda 30 campos válidos y después 31  
**Entonces** el de 30 recibe `200` y el de 31 recibe `400` con el error en `fields`.

### CHU-03 — Claves válidas y reservadas

**Dado** un administrador editando un borrador  
**Cuando** usa las claves `title`, `Sede` (mayúscula), `1sede`, `sede` repetida y `centro_costo`  
**Entonces** `title`, `Sede`, `1sede` y la repetida reciben `400` con el error en `fields[n].key`, y `centro_costo` se acepta.

### CHU-04 — Opciones según el tipo

**Dado** un administrador editando un borrador  
**Cuando** guarda un `SingleChoice` con 1 opción, otro con 2, otro con 51, un `MultipleChoice` con las opciones "Norte" y "norte", y un `ShortText` con opciones  
**Entonces** solo el de 2 opciones se acepta; los demás reciben `400`.

### CHU-05 — Autorización en backend

**Dado** un desarrollador, Elizabeth (PM, mientras no se habilite) y un coordinador de empresa  
**Cuando** llaman `POST /api/admin/form-templates` directamente  
**Entonces** reciben `403` y no se crea ninguna plantilla.

### CHU-06 — El formulario común no cambia

**Dado** una plantilla en borrador sin asociar ni publicar  
**Cuando** un solicitante de cualquier empresa radica un ticket  
**Entonces** usa el formulario común sin cambios (PRD CA-15) y el borrador no aparece en ninguna respuesta del portal.

### CHU-07 — Eliminar solo lo nunca publicado

**Dado** una plantilla nunca publicada y otra con una versión publicada ([[hu-045-versionar-plantillas|HU-045]])  
**Cuando** el administrador elimina cada una  
**Entonces** la primera recibe `204` y la segunda `409`.

### CHU-08 — Auditoría

**Dado** un borrador guardado por un administrador  
**Cuando** se consulta la auditoría  
**Entonces** hay una entrada `FormTemplateDraftUpdated` (nombre propuesto) con actor, fecha/hora y objeto `FormTemplate:{id}`.

### CHU-09 — Interfaz del constructor

**Dado** el administrador en el constructor  
**Cuando** escribe una clave reservada o deja una etiqueta vacía  
**Entonces** la vista marca el campo con el mensaje en español antes de enviar; si el backend devuelve `400`, los errores se muestran junto a cada campo afectado y el borrador local no se pierde; la vista previa refleja los cambios sin guardar.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-09 validados con evidencia en la matriz.
- [ ] **DoD-02** — Pruebas escritas primero y en verde (`cd backend && dotnet test`): `Draft_*`, `ChoiceField_*`, `CreateFormTemplate_ByDeveloper_ReturnsForbidden`, `DeletePublishedTemplate_ReturnsConflict` e integración de endpoints.
- [ ] **DoD-03** — `DataTicket.ArchitectureTests` en verde.
- [ ] **DoD-04** — Migración `AddFormTemplates` creada y aplicada en local; almacenamiento conforme al ADR aceptado.
- [ ] **DoD-05** — Frontend: Vitest del modelo en verde, `npm --prefix frontend run lint` y `npm --prefix frontend run build` correctos.
- [ ] **DoD-06** — Regresión: las pruebas de radicación con formulario común ([[hu-013-radicar-ticket|HU-013]]) siguen en verde.
- [ ] **DoD-07** — `docker compose up --build` y verificación manual del constructor y la vista previa.
- [ ] **DoD-08** — Wiki actualizada: [[formularios-configurables]] (tipos y límites), [[modelo-de-dominio]] (`FormTemplate`, `FormField`), [[glosario]], [[persistencia-postgresql]], [[roles-y-permisos]] (quién administra plantillas).
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

## Notas y decisiones

- **Propuesta:** tipos de campo `ShortText`, `LongText`, `Number`, `Date`, `SingleChoice`, `MultipleChoice`; ampliar solo a pedido.
- **Propuesta:** constructor restringido a `Administrator`; confirmar si Elizabeth debe usarlo.
- **Propuesta:** límites numéricos (30 campos, 2–50 opciones, longitudes) a validar con el equipo.
- Las semillas del prototipo pueden usarse solo como datos sintéticos de ejemplo (PRD §9, §12).

## Relacionado

- [[ep-013-formularios-configurables]] · [[tablero-scrum]] · [[formularios-configurables]] · [[modelo-de-dominio]] · [[roles-y-permisos]] · [[persistencia-postgresql]] · [[frontend-mvc]] · [[criterios-de-aceptacion]]
