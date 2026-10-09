---
title: "HU-046 — Radicar con el formulario de la empresa"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/formularios, producto/ticket, producto/portal, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.1", "PRD.md §8", "PRD.md §9", "PRD.md §10", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-046", "Radicar con el formulario de la empresa"]
epica: "[[ep-013-formularios-configurables]]"
criterios_prd: ["CA-15", "CA-01", "CA-02"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 9"
dependencias: ["[[hu-045-versionar-plantillas]]", "[[hu-044-asociar-plantilla-a-empresa]]", "[[hu-013-radicar-ticket]]", "[[hu-014-adjuntos-en-radicacion]]", "[[hu-020-detalle-interno-del-ticket]]", "[[hu-024-portal-solicitante-consulta-tickets]]"]
relacionadas: ["[[hu-043-constructor-de-plantillas]]", "[[hu-047-consultar-por-campos-variables]]", "[[hu-025-portal-coordinador-consulta-tickets]]", "[[hu-017-cola-de-cobertura-y-tomar-ticket]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-046 — Radicar con el formulario de la empresa

Cuando la empresa del usuario tiene una versión publicada de plantilla, el portal muestra el formulario común más los campos de esa versión; el backend valida obligatoriedad, tipo y opciones, guarda las respuestas variables en PostgreSQL y el ticket conserva la versión usada. Las empresas sin plantilla siguen con el formulario común (PRD §9, §14.15).

## Historia de usuario

**COMO** solicitante o coordinador de una empresa cliente con formulario propio  
**QUIERO** radicar mi ticket con los campos específicos de mi empresa además de los comunes  
**PARA** aportar desde el inicio la información que Data Global necesita para atender mi caso

## Contexto

PRD §9 exige "conservación de campos núcleo y soporte para almacenar respuestas variables en PostgreSQL" y que cada ticket conserve los campos y la versión con que se creó. Empresa y solicitante se derivan de la cuenta (PRD §6.1). La radicación con formulario común existe desde la Fase 1 ([[hu-013-radicar-ticket|HU-013]], [[hu-014-adjuntos-en-radicacion|HU-014]]); esta HU la extiende sin romperla.

Hallazgo del plan de backlog: la forma de almacenar las respuestas variables (columna `jsonb` en el ticket frente a tablas de valores por campo) no está decidida y condiciona esta HU y [[hu-047-consultar-por-campos-variables|HU-047]]. Esta HU exige un ADR aceptado antes de implementar la persistencia; no lo resuelve.

## Alcance

- Endpoint que entrega al portal el formulario aplicable (campos núcleo + versión publicada vigente o `null`).
- Renderizado dinámico de los campos de la versión en el orden publicado, junto a los campos núcleo.
- Ampliar `SubmitTicket` con `formTemplateVersionId` y `customFields`, validados en backend.
- Persistir la versión usada y las respuestas variables según el ADR de almacenamiento.
- Mostrar las respuestas con las etiquetas de su versión en el detalle interno ([[hu-020-detalle-interno-del-ticket|HU-020]]) y en el del portal ([[hu-024-portal-solicitante-consulta-tickets|HU-024]], [[hu-025-portal-coordinador-consulta-tickets|HU-025]]).
- Incluir la versión del formulario en la auditoría de radicación (propuesta).

## Fuera de alcance

- Diseñar, asociar o publicar plantillas → HU-043, HU-044, HU-045.
- Filtrar por campos variables → [[hu-047-consultar-por-campos-variables|HU-047]].
- Editar las respuestas variables después de radicar (no aparece en el PRD).
- Mostrar campos variables en la cola de cobertura del administrador (PRD §5.5 limita los campos de esa cola).
- Cambiar el catálogo de categorías (V-12) o la matriz de prioridad.
- Campos de archivo dentro de la plantilla: los adjuntos siguen siendo el campo núcleo (PRD §8).

## Requisitos y reglas de negocio

- Se conservan los campos núcleo: categoría, título (≤ 120), descripción, urgencia, impacto y adjuntos; la prioridad sigue calculándose con la matriz (PRD §6.1, §9).
- Respuestas variables almacenadas en PostgreSQL (PRD §9).
- Cada ticket conserva los campos y la versión con que se creó (PRD §9).
- Empresa y solicitante salen de la cuenta autenticada (PRD §6.1).
- Toda consulta y operación se restringe por empresa en el servidor (PRD §12).
- El piloto no se bloquea: empresas sin plantilla radican con el formulario común (PRD §9, §14.15).
- Reglas de validación propuestas: obligatorio = presente y no vacío tras recortar; `ShortText` ≤ 200 caracteres; `LongText` ≤ 4 000; `Number` decimal con punto (`.`), sin separador de miles; `Date` en formato `AAAA-MM-DD` válido; `SingleChoice` = una opción exacta de la lista; `MultipleChoice` = lista sin repetidos de opciones válidas; claves desconocidas → error.
- Propuesta: si `formTemplateVersionId` no es la versión vigente de la empresa del usuario (versión antigua o de otra empresa) → 409 "el formulario cambió", sin revelar a qué empresa pertenece.

## Invariantes en juego

- Invariante 1 (`AGENTS.md` §7): el usuario de la empresa A nunca recibe ni usa la plantilla de B; las respuestas variables solo se ven en tickets visibles para él.
- Invariante 7: los adjuntos siguen validándose en backend (sin cambios respecto de HU-014).
- Invariante 8: la radicación sigue auditada.
- Invariante 2: el cliente ve sus propias respuestas (datos de radicación), nunca la configuración interna ni campos de otras empresas.

## Criterios del PRD cubiertos

- PRD CA-15 (total en conjunto con la épica: el piloto opera con el formulario común y la personalización es separada) → [[criterios-de-aceptacion]]
- PRD CA-01 (parcial: aislamiento A↔B en plantillas y respuestas) → [[criterios-de-aceptacion]]
- PRD CA-02 (parcial: el cliente ve sus propios datos de radicación, incluidas sus respuestas variables) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-013-formularios-configurables]]
- Dependencias: [[hu-045-versionar-plantillas|HU-045]] (versión vigente), [[hu-044-asociar-plantilla-a-empresa|HU-044]] (empresa ↔ plantilla), [[hu-013-radicar-ticket|HU-013]] y [[hu-014-adjuntos-en-radicacion|HU-014]] (radicación base), [[hu-020-detalle-interno-del-ticket|HU-020]] y [[hu-024-portal-solicitante-consulta-tickets|HU-024]] (detalles a ampliar).
- Relacionadas: [[hu-043-constructor-de-plantillas|HU-043]] (renderizador de vista previa reutilizado), [[hu-047-consultar-por-campos-variables|HU-047]], [[hu-025-portal-coordinador-consulta-tickets|HU-025]], [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]] (la cola de cobertura no muestra campos variables).
- Decisiones: **bloqueante:** ADR de almacenamiento de respuestas variables (`jsonb` frente a tablas); abiertas: V-12, V-16, Row-Level Security, enrutador del frontend → [[pendientes]].

## Componentes afectados

- Backend (Domain): `Ticket` con referencia a `FormTemplateVersion` y respuestas variables (value object propuesto `CustomFieldAnswer`); validador de respuestas por tipo.
- Backend (Application): `SubmitTicket` ampliado; consulta `GetApplicableTicketForm` (nombre propuesto); `GetClientTicket` y detalle interno ampliados.
- Backend (Infrastructure): persistencia de respuestas según el ADR; migración.
- Backend (Api): endpoint del formulario aplicable y ampliación del endpoint de radicación.
- Frontend (models, controllers, views): formulario de radicación del portal y detalles.

## Dificultad

**Nivel:** Alto

**Justificación:** extiende el flujo central de radicación con validación dinámica por tipo, depende de una decisión de persistencia abierta, toca tres superficies (radicación, detalle interno y portal) y exige pruebas de regresión y de aislamiento.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01 sobre el contrato de [[hu-013-radicar-ticket|HU-013]] y [[hu-014-adjuntos-en-radicacion|HU-014]] (radicación en `POST /api/tickets`, JSON o `multipart/form-data`).

**`GET /api/portal/ticket-form`** (roles `Requester`, `CompanyCoordinator`) → `200`:

```json
{
  "template": {
    "formTemplateVersionId": "51ac…",
    "versionNumber": 3,
    "fields": [
      { "key": "centro_costo", "label": "Centro de costo", "type": "ShortText", "required": false, "helpText": "", "options": [], "order": 1 },
      { "key": "sede", "label": "Sede", "type": "SingleChoice", "required": true, "helpText": "Sede donde ocurre el caso", "options": ["Norte", "Sur", "Centro"], "order": 2 },
      { "key": "fecha_evento", "label": "Fecha del evento", "type": "Date", "required": false, "helpText": "Formato AAAA-MM-DD", "options": [], "order": 3 }
    ]
  }
}
```

`template` es `null` si la empresa no tiene versión publicada. Los campos núcleo y el catálogo de categorías siguen el contrato de HU-013. Usuarios internos → `403`.

**`POST /api/tickets`** (`application/json` o `multipart/form-data`, roles `Requester` y `CompanyCoordinator`): campos de HU-013/HU-014 (`category`, `title`, `description`, `urgency`, `impact`, `files`) más:

| Parte | Tipo | Regla |
|---|---|---|
| `formTemplateVersionId` | GUID | Obligatoria si la empresa tiene versión vigente; prohibida si no |
| `customFields` | JSON (propiedad en JSON; parte con contenido JSON en multipart) | Objeto `clave → valor` |

```json
{ "sede": "Norte", "centro_costo": "CC-100", "fecha_evento": "2026-10-01" }
```

Respuesta `201`: la de HU-013 más `formTemplateVersion: 3`.

**Detalle (portal e interno)** — campo añadido:

```json
"customFields": {
  "versionNumber": 3,
  "values": [
    { "key": "centro_costo", "label": "Centro de costo", "value": "CC-100" },
    { "key": "sede", "label": "Sede", "value": "Norte" },
    { "key": "fecha_evento", "label": "Fecha del evento", "value": "2026-10-01" }
  ]
}
```

`customFields` es `null` en tickets radicados con el formulario común.

| Código | Caso | `type` |
|---|---|---|
| 400 | Obligatorio vacío; tipo inválido (`"12,5"` en `Number`, `"2026-02-30"` en `Date`); opción fuera de la lista; clave desconocida; `customFields` enviado sin plantilla vigente; errores de campos núcleo (HU-013) | `urn:dataticket:validation` (con `errors["customFields.sede"]`) |
| 401 | Sin sesión | — |
| 403 | Usuario interno en ruta del portal | `urn:dataticket:forbidden` |
| 409 | `formTemplateVersionId` distinto de la versión vigente de su empresa | `urn:dataticket:form-version-outdated` |
| 413 / 415 | Adjuntos (sin cambios respecto de HU-014) | — |

## Tareas de desarrollo

- [ ] **T-01 — Contrato y ADR de almacenamiento** · Capa: Transversal · Dificultad: Alto  
  Descripción: ratificar contrato y reglas de validación; redactar y someter a aceptación el ADR "almacenamiento de respuestas variables" (`jsonb` con índice GIN frente a tabla de valores por campo), considerando las consultas de HU-047. Sin ADR aceptado no se inicia T-04.
- [ ] **T-02 — Dominio: validación de respuestas** · Capa: Backend (Domain) · Dificultad: Medio  
  Descripción: primero pruebas: `Answers_MissingRequired_Fails`, `Answers_WhitespaceOnlyRequired_Fails`, `Number_WithComma_Fails`, `Date_Feb30_Fails`, `SingleChoice_OutOfList_Fails`, `MultipleChoice_Duplicated_Fails`, `ShortText_201Chars_Fails`, `ShortText_200Chars_Passes`, `UnknownKey_Fails`. Luego el validador y `CustomFieldAnswer`.
- [ ] **T-03 — Casos de uso** · Capa: Backend (Application) · Dificultad: Alto  
  Descripción: primero pruebas: `SubmitTicket_CompanyWithoutTemplate_UsesCommonFormUnchanged`, `SubmitTicket_WithOutdatedVersion_ReturnsConflict`, `SubmitTicket_WithOtherCompanyVersion_ReturnsConflict`, `SubmitTicket_StoresVersionAndAnswers`, `GetApplicableForm_ReturnsOnlyOwnCompanyTemplate`, `SubmitTicket_AuditIncludesFormVersion`. Luego ampliación de `SubmitTicket` y la consulta del formulario aplicable.
- [ ] **T-04 — Persistencia y migración** · Capa: Backend (Infrastructure) · Dificultad: Alto  
  Descripción: implementación según el ADR aceptado; referencia `form_template_version_id` en tickets; migración `AddTicketCustomFields`; prueba de integración ida y vuelta con PostgreSQL real.
- [ ] **T-05 — Endpoints y detalles** · Capa: Backend (Api) · Dificultad: Medio  
  Descripción: primero pruebas de integración: formulario aplicable por empresa (A recibe su plantilla, B sin plantilla recibe `null`), radicación válida e inválida, `customFields` en el detalle interno y del portal; coordinador de B → ticket de A = 404. Luego endpoints.
- [ ] **T-06 — Modelo** · Capa: Frontend (models) · Dificultad: Medio  
  Descripción: primero Vitest con las mismas reglas de validación por tipo (casos límite de T-02) y la serialización de `customFields`. Luego tipos y gateway.
- [ ] **T-07 — Controlador** · Capa: Frontend (controllers) · Dificultad: Medio  
  Descripción: ampliar el controlador de radicación de HU-013: carga del formulario aplicable, estado de respuestas, validación local, manejo de `409` (recargar formulario conservando núcleo y respuestas compatibles).
- [ ] **T-08 — Vistas** · Capa: Frontend (views) · Dificultad: Medio  
  Descripción: reutilizar `DynamicFormPreviewView` de HU-043 como renderizador de campos con etiquetas, ayuda y errores accesibles; `CustomFieldsView` de solo lectura para los detalles interno y del portal.

## Criterios de aceptación

### CHU-01 — Radicación con plantilla

**Dado** la empresa A con la versión 3 publicada (campos `centro_costo`, `sede` obligatorio, `fecha_evento`) y un solicitante de A  
**Cuando** radica con los campos núcleo válidos y `customFields = {"sede": "Norte", "centro_costo": "CC-100", "fecha_evento": "2026-10-01"}`  
**Entonces** recibe `201` con `formTemplateVersion: 3`, el ticket guarda la versión y las tres respuestas, y la prioridad calculada sigue la matriz del PRD §6.1.

### CHU-02 — Empresa sin plantilla (regresión CA-15)

**Dado** la empresa B sin plantilla publicada  
**Cuando** un solicitante de B consulta `GET /api/portal/ticket-form` y radica con el formulario común  
**Entonces** recibe `template: null`, la radicación responde igual que en HU-013 y el ticket tiene `customFields: null`; **y** si envía `customFields`, recibe `400`.

### CHU-03 — Validación por tipo con valores límite

**Dado** la empresa A con un `ShortText`, un `Number`, un `Date` y un `SingleChoice`  
**Cuando** envía 200 y 201 caracteres en el texto, `"12.5"` y `"12,5"` en el número, `"2026-02-28"` y `"2026-02-30"` en la fecha, `"Norte"` y `"Oeste"` en la opción  
**Entonces** se aceptan 200 caracteres, `"12.5"`, `"2026-02-28"` y `"Norte"`; los demás reciben `400` con el error en `customFields.<clave>`.

### CHU-04 — Obligatorios y claves desconocidas

**Dado** `sede` obligatorio  
**Cuando** se radica sin `sede`, con `sede = "   "` o con una clave `placa` que no existe en la versión  
**Entonces** cada caso recibe `400` y no se crea el ticket ni se suben adjuntos.

### CHU-05 — Aislamiento A↔B de plantillas

**Dado** la versión vigente de la empresa A  
**Cuando** un solicitante de la empresa B radica enviando el `formTemplateVersionId` de A  
**Entonces** recibe `409` sin datos de la plantilla de A, y `GET /api/portal/ticket-form` para B nunca devuelve campos de A.

### CHU-06 — Versión desactualizada

**Dado** que el solicitante cargó el formulario en la versión 3 y luego se publicó la 4  
**Cuando** envía la radicación con la versión 3  
**Entonces** recibe `409`; la vista recarga el formulario de la versión 4 y conserva los campos núcleo y las respuestas cuyas claves siguen existiendo.

### CHU-07 — El ticket conserva su versión y es visible solo a quien corresponde

**Dado** un ticket de la empresa A radicado con la versión 3  
**Cuando** se publica la versión 4, el solicitante y el coordinador de A abren el detalle del portal y el coordinador de B pide ese `ticketId`  
**Entonces** A ve `customFields` con las etiquetas de la versión 3; el coordinador de B recibe `404`.

### CHU-08 — Detalle interno y cola de cobertura

**Dado** el ticket anterior  
**Cuando** un participante interno abre el detalle interno y un administrador no asociado consulta la cola de cobertura  
**Entonces** el participante ve `customFields` y la cola de cobertura no incluye ningún campo variable.

### CHU-09 — Auditoría

**Dado** una radicación con plantilla  
**Cuando** se consulta la auditoría del ticket  
**Entonces** la entrada de radicación incluye actor, fecha/hora y la versión de formulario usada (`3`).

### CHU-10 — Interfaz del formulario dinámico

**Dado** un solicitante de A en el formulario de radicación  
**Cuando** deja `sede` vacío y pulsa enviar  
**Entonces** la vista marca el campo con "Este campo es obligatorio" antes de llamar al backend; si el backend devuelve `400`, cada error aparece junto a su campo y no se pierden los datos escritos.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-10 validados con evidencia en la matriz.
- [ ] **DoD-02** — ADR de almacenamiento de respuestas variables en estado `aceptada` antes de la migración.
- [ ] **DoD-03** — Pruebas escritas primero y en verde (`cd backend && dotnet test`): validador por tipo (`Answers_*`, `Number_*`, `Date_*`, `SingleChoice_*`, `ShortText_*`), `SubmitTicket_CompanyWithoutTemplate_UsesCommonFormUnchanged`, `SubmitTicket_WithOtherCompanyVersion_ReturnsConflict` e integración con PostgreSQL real.
- [ ] **DoD-04** — Regresión: todas las pruebas de HU-013 y HU-014 siguen en verde.
- [ ] **DoD-05** — `DataTicket.ArchitectureTests` en verde.
- [ ] **DoD-06** — Migración `AddTicketCustomFields` creada y aplicada en local.
- [ ] **DoD-07** — Frontend: Vitest del modelo en verde, `npm --prefix frontend run lint` y `npm --prefix frontend run build` correctos.
- [ ] **DoD-08** — `docker compose up --build` y verificación manual con una empresa con plantilla y otra sin ella.
- [ ] **DoD-09** — Wiki actualizada: [[formularios-configurables]], [[persistencia-postgresql]] (estrategia del ADR), [[modelo-de-dominio]], [[flujo-del-ticket]] (radicación), [[glosario]]; nota para [[criterios-de-aceptacion]] (CA-15 con prueba de regresión).
- [ ] **DoD-10** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO y PR revisado por otra persona del equipo.
- [ ] **DoD-11** — La trazabilidad de la HU y de [[ep-013-formularios-configurables]] está actualizada.

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

- **Bloqueante:** ADR de almacenamiento de respuestas variables (hallazgo del plan de backlog).
- **Propuesta:** reglas de formato (`Number` con punto, `Date` ISO, límites de texto).
- **Propuesta:** 409 ante versión no vigente o ajena, sin distinguir ambos casos.
- **Riesgo:** los clientes podrían capturar datos personales en campos libres; la retención está pendiente (PRD §12, §16.3).

## Relacionado

- [[ep-013-formularios-configurables]] · [[tablero-scrum]] · [[formularios-configurables]] · [[flujo-del-ticket]] · [[persistencia-postgresql]] · [[modelo-de-dominio]] · [[roles-y-permisos]] · [[matriz-de-prioridad]] · [[criterios-de-aceptacion]]
