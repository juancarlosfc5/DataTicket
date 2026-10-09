---
title: "HU-047 — Consultar tickets por campos variables"
type: historia-de-usuario
status: vigente
estado: Aprobada
tags: [scrum, scrum/historia, producto/formularios, producto/dashboard, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §9", "PRD.md §10", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-047", "Consultar tickets por campos variables"]
epica: "[[ep-013-formularios-configurables]]"
criterios_prd: ["CA-01", "CA-15"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Alto"
sprint_sugerido: "Sprint 9"
dependencias: ["[[hu-046-radicar-con-formulario-de-empresa]]", "[[hu-043-constructor-de-plantillas]]", "[[hu-045-versionar-plantillas]]", "[[hu-016-cola-global-de-triage]]", "[[hu-023-bandeja-de-mis-tickets-y-equipo]]"]
relacionadas: ["[[hu-017-cola-de-cobertura-y-tomar-ticket]]", "[[hu-040-panel-global-de-la-pm]]", "[[hu-041-panel-del-equipo-interno]]", "[[hu-010-datos-sinteticos-de-desarrollo]]"]
created: 2026-10-07
updated: 2026-10-09
---

# HU-047 — Consultar tickets por campos variables

Los equipos internos filtran y ven en sus bandejas los tickets de una empresa por los campos variables que la plantilla marca como consultables (p. ej. sede o centro de costo), respetando el aislamiento por empresa y las reglas de visibilidad de cada bandeja (PRD §9, §10, §12).

## Historia de usuario

**COMO** Elizabeth (PM) o integrante de Desarrollo o Producción  
**QUIERO** filtrar los tickets de una empresa por los campos variables que usamos en la operación  
**PARA** encontrar rápido los casos de una sede, centro de costo u otro dato propio del cliente

## Contexto

PRD §9 pide "soporte para almacenar respuestas variables en PostgreSQL, con capacidad de consultar campos usados operativamente". El PRD no dice qué campos se consultan ni dónde; se propone que el administrador marque campos como consultables en la plantilla (propiedad propuesta `isFilterable`) y que el filtro viva en las bandejas internas ([[hu-016-cola-global-de-triage|HU-016]], [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]]). La viabilidad y el rendimiento dependen del ADR de almacenamiento de respuestas variables exigido por [[hu-046-radicar-con-formulario-de-empresa|HU-046]].

## Alcance

- Propiedad "consultable" por campo en el constructor, congelada en cada versión (propuesta).
- Endpoint que lista los campos consultables de una empresa (unión de todas sus versiones por clave, con la etiqueta más reciente).
- Filtros por campos variables en las bandejas internas, siempre acotados a una empresa.
- Columnas opcionales con los valores de los campos consultables en los resultados.
- Índices de base de datos según el ADR para que los filtros no recorran toda la tabla.

## Fuera de alcance

- Filtros por campos variables en el portal del cliente (propuesta: fuera hasta que se pida).
- Exportaciones y vistas guardadas (PRD §10, §16.4).
- Búsqueda de texto completo sobre `LongText` (propuesta: no consultable).
- Filtros que crucen varias empresas a la vez por un campo variable.
- Métricas o gráficos por campo variable en el dashboard ([[ep-012-dashboard-y-metricas]]).

## Requisitos y reglas de negocio

- Capacidad de consultar los campos usados operativamente (PRD §9).
- Desarrollo y Producción usan bandejas por equipo y responsabilidad; el detalle requiere asociación (PRD §10).
- El administrador de cobertura ve solo los campos de la cola (número, empresa, título, categoría, urgencia, impacto, prioridad, fecha, estado resumido) antes de asociarse (PRD §5.5): no ve ni filtra por campos variables sin estar asociado.
- Toda consulta se restringe por empresa y permisos en el servidor (PRD §12).
- Propuestas de semántica: igualdad exacta para `SingleChoice`, `Number` y `Date`; "contiene" sin distinguir mayúsculas para `ShortText`; "incluye la opción" para `MultipleChoice`; `LongText` no puede marcarse como consultable.
- Propuesta: un filtro por campo variable exige `companyId`; como máximo 5 filtros por consulta.
- Propuesta: en la bandeja del equipo (HU-023), los tickets donde la persona no es participante no muestran valores variables ni se incluyen en resultados filtrados por ellos.

## Invariantes en juego

- Invariante 1 (`AGENTS.md` §7): los filtros nunca devuelven tickets de otra empresa aunque compartan claves de campo.
- Invariante 5: el administrador no accede a contenido por serlo; los valores variables solo aparecen en tickets donde es participante.
- Invariante 3 (análogo para el detalle): Desarrollo y Producción solo ven en el filtro los tickets de su bandeja.

## Criterios del PRD cubiertos

- PRD CA-01 (parcial: aislamiento A↔B en consultas por campos variables) → [[criterios-de-aceptacion]]
- PRD CA-15 (parcial: completa la fase de personalización sin afectar el formulario común) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-013-formularios-configurables]]
- Dependencias: [[hu-046-radicar-con-formulario-de-empresa|HU-046]] (respuestas y ADR de almacenamiento), [[hu-043-constructor-de-plantillas|HU-043]] y [[hu-045-versionar-plantillas|HU-045]] (propiedad consultable versionada), [[hu-016-cola-global-de-triage|HU-016]] y [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]] (bandejas que se amplían).
- Relacionadas: [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]], [[hu-040-panel-global-de-la-pm|HU-040]], [[hu-041-panel-del-equipo-interno|HU-041]].
- Decisiones: **bloqueante:** ADR de almacenamiento de respuestas variables; abiertas: Row-Level Security, librería de estado del frontend → [[pendientes]].

## Componentes afectados

- Backend (Domain): propiedad `IsFilterable` en `FormField` (propuesta).
- Backend (Application): consulta de bandeja ampliada con filtros (`CustomFieldFilter`, nombre propuesto); `ListFilterableFields` (propuesto).
- Backend (Infrastructure): consultas EF Core/SQL e índices según el ADR (p. ej. GIN sobre `jsonb` o índice compuesto `(company_id, field_key, value)`); migración.
- Backend (Api): parámetros nuevos en los endpoints de bandeja y endpoint de campos consultables.
- Frontend (models, controllers, views): constructor (casilla "consultable") y filtros de bandejas.

## Dificultad

**Nivel:** Alto

**Justificación:** combina consultas dinámicas sobre datos flexibles, reglas de visibilidad de varias bandejas, aislamiento multiempresa e índices dependientes de una decisión de persistencia abierta.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01 sobre los contratos de bandeja de [[hu-016-cola-global-de-triage|HU-016]] (`GET /api/triage-queue`, solo `ProductManager`) y [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]] (`GET /api/inbox/mine`, `GET /api/inbox/team`).

**`GET /api/companies/{companyId}/filterable-fields`** (roles internos) → `200`:

```json
{
  "companyId": "7d41…",
  "fields": [
    { "key": "sede", "label": "Sede principal", "type": "SingleChoice", "options": ["Norte", "Sur", "Centro"] },
    { "key": "centro_costo", "label": "Centro de costo", "type": "ShortText", "options": [] }
  ]
}
```

**Filtro en las bandejas:** los mismos parámetros se aceptan en las tres rutas, p. ej. `GET /api/triage-queue?status=all&companyId=7d41…&cf.sede=Norte&cf.centro_costo=CC-1` o `GET /api/inbox/mine?companyId=7d41…&cf.sede=Norte` → `200`, mismo cuerpo de cada bandeja más, por fila visible:

```json
"customFields": { "sede": "Norte", "centro_costo": "CC-100" }
```

En `GET /api/inbox/team`, las filas con `isParticipant = false` (HU-023) **no** incluyen `customFields` y quedan **excluidas** de los resultados cuando hay filtros `cf.*`: los valores variables son datos de radicación que solo ven quienes acceden al detalle (propuesta).

**Constructor** ([[hu-043-constructor-de-plantillas|HU-043]]): cada campo del borrador acepta `"isFilterable": true|false`; `true` en `LongText` → 400.

| Código | Caso | `type` |
|---|---|---|
| 400 | `cf.*` sin `companyId`; clave inexistente o no consultable; más de 5 filtros; valor con formato inválido para el tipo (`cf.fecha_evento=2026-13-01`) | `urn:dataticket:validation` |
| 401 | Sin sesión | — |
| 403 | Cliente (solicitante o coordinador) | `urn:dataticket:forbidden` |

Para un administrador, el filtro no falla: los resultados excluyen los tickets donde no es participante vigente (propuesta, coherente con PRD §5.5).
| 404 | Empresa inexistente | `urn:dataticket:not-found` |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Medio  
  Descripción: ratificar sintaxis `cf.<clave>`, semántica por tipo, límite de filtros, campos consultables y visibilidad del administrador; alinear con el ADR de almacenamiento de HU-046.
- [ ] **T-02 — Dominio** · Capa: Backend (Domain) · Dificultad: Bajo  
  Descripción: primero pruebas: `LongText_MarkedFilterable_Throws`, `FilterableFlag_IsFrozenInVersion`. Luego la propiedad en `FormField` y su paso a la versión.
- [ ] **T-03 — Consulta de bandeja con filtros** · Capa: Backend (Application) · Dificultad: Alto  
  Descripción: primero pruebas: `Filter_WithoutCompanyId_ReturnsValidationError`, `Filter_NonFilterableKey_ReturnsValidationError`, `Filter_SameKeyInTwoCompanies_ReturnsOnlyRequestedCompany`, `Filter_ByDeveloper_ReturnsOnlyOwnInboxTickets`, `TeamInbox_NonParticipantRows_HaveNoCustomFields`, `Filter_ByAdmin_ExcludesTicketsWhereNotParticipant`, `Filter_ByClient_ReturnsForbidden`. Luego el objeto de filtro y su aplicación sobre la consulta base de cada bandeja.
- [ ] **T-04 — Persistencia, índices y migración** · Capa: Backend (Infrastructure) · Dificultad: Alto  
  Descripción: traducción de filtros a SQL según el ADR; índices; migración `AddCustomFieldFilterIndexes`; prueba de integración con PostgreSQL real y evidencia de `EXPLAIN` que muestre el uso del índice con datos sintéticos (PRD §16.5).
- [ ] **T-05 — Endpoints** · Capa: Backend (Api) · Dificultad: Medio  
  Descripción: primero pruebas de integración (200, 400, 403, 404 y prueba negativa A↔B). Luego parámetros en la bandeja y endpoint de campos consultables.
- [ ] **T-06 — Frontend** · Capa: Frontend (models/controllers/views) · Dificultad: Medio  
  Descripción: Vitest del modelo (construcción de la *query string* `cf.*`, validación por tipo, máximo 5); ampliación de los controladores de bandeja con `setCustomFieldFilter`; vistas `CustomFieldFiltersView` (aparece al elegir una empresa) y columnas opcionales; casilla "consultable" en el editor de campos.

## Criterios de aceptación

### CHU-01 — Filtro por opción para la PM

**Dado** la empresa A con 3 tickets `sede = Norte` y 2 `sede = Sur`, y `sede` marcado como consultable  
**Cuando** Elizabeth pide la bandeja global con `companyId=A&cf.sede=Norte`  
**Entonces** recibe exactamente los 3 tickets de Norte, cada fila con `customFields.sede = "Norte"`.

### CHU-02 — Aislamiento A↔B

**Dado** las empresas A y B, ambas con un campo `sede` y tickets con `sede = Norte`  
**Cuando** Elizabeth filtra con `companyId=A&cf.sede=Norte`  
**Entonces** ningún resultado pertenece a B; la prueba `Filter_SameKeyInTwoCompanies_ReturnsOnlyRequestedCompany` lo verifica en backend.

### CHU-03 — Visibilidad de Desarrollo

**Dado** Brayan (Desarrollo) participante en 1 de los 3 tickets de Norte, y los otros 2 visibles en la bandeja de su equipo con `isParticipant = false`  
**Cuando** filtra `GET /api/inbox/mine` y `GET /api/inbox/team` con `companyId=A&cf.sede=Norte`  
**Entonces** ambas devuelven solo el ticket donde participa, y ninguna fila de la bandeja del equipo sin filtro trae `customFields` cuando `isParticipant = false`.

### CHU-04 — Administrador y clientes

**Dado** un administrador no asociado a ningún ticket de A y un coordinador de A  
**Cuando** el administrador filtra su bandeja por `cf.sede=Norte` y el coordinador llama al mismo endpoint interno  
**Entonces** el administrador no recibe tickets con valores variables (propuesta) y el coordinador recibe `403`.

### CHU-05 — Validaciones de filtro

**Dado** Elizabeth  
**Cuando** filtra con `cf.sede=Norte` sin `companyId`, con `cf.descripcion_larga=x` (campo `LongText`), con 6 filtros y con `cf.fecha_evento=2026-13-01`  
**Entonces** cada petición recibe `400` con el error correspondiente; con 5 filtros válidos recibe `200`.

### CHU-06 — Semántica por tipo

**Dado** tickets con `centro_costo = "CC-100"` y `"CC-200"`, y una `fecha_evento = 2026-10-01`  
**Cuando** Elizabeth filtra por `cf.centro_costo=cc-1` y por `cf.fecha_evento=2026-10-01`  
**Entonces** el primero devuelve el ticket con `"CC-100"` (contiene, sin distinguir mayúsculas) y el segundo solo el ticket de esa fecha exacta.

### CHU-07 — Campos de versiones anteriores

**Dado** tickets radicados con la versión 1 (`sede`) y con la versión 2 (`sede` con etiqueta "Sede principal")  
**Cuando** se filtra por `cf.sede=Norte`  
**Entonces** el resultado incluye tickets de ambas versiones y `filterable-fields` muestra la etiqueta más reciente.

### CHU-08 — Uso de índices

**Dado** la base local con datos sintéticos de varias empresas ([[hu-010-datos-sinteticos-de-desarrollo|HU-010]])  
**Cuando** se ejecuta `EXPLAIN` de la consulta filtrada por `cf.sede`  
**Entonces** el plan usa el índice definido en la migración y no un recorrido secuencial de la tabla de respuestas.

### CHU-09 — Interfaz de filtros

**Dado** la PM en su bandeja sin empresa seleccionada  
**Cuando** abre los filtros  
**Entonces** los filtros por campos variables aparecen deshabilitados con el texto "Selecciona una empresa"; al elegirla se cargan sus campos consultables y, si el backend responde `400`, el mensaje aparece junto al filtro y se conserva el resto.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-09 validados con evidencia en la matriz.
- [ ] **DoD-02** — Pruebas escritas primero y en verde (`cd backend && dotnet test`): `Filter_*`, `LongText_MarkedFilterable_Throws`, `FilterableFlag_IsFrozenInVersion` e integración con PostgreSQL real, incluida la prueba negativa A↔B.
- [ ] **DoD-03** — `DataTicket.ArchitectureTests` en verde.
- [ ] **DoD-04** — Migración `AddCustomFieldFilterIndexes` creada y aplicada; salida de `EXPLAIN` adjunta como evidencia.
- [ ] **DoD-05** — Regresión: pruebas de HU-016 y HU-023 sin filtros siguen en verde.
- [ ] **DoD-06** — Frontend: `npm --prefix frontend test`, `npm --prefix frontend run lint` y `npm --prefix frontend run build` correctos.
- [ ] **DoD-07** — `docker compose up --build` y verificación manual de filtros con dos empresas.
- [ ] **DoD-08** — Wiki actualizada: [[formularios-configurables]] (campos consultables), [[persistencia-postgresql]] (índices), [[dashboard-y-metricas]] (filtros de bandejas), [[roles-y-permisos]] (visibilidad del administrador).
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

- **Propuesta:** "campos usados operativamente" (PRD §9) = campos marcados como consultables por el administrador.
- **Propuesta:** filtros solo en bandejas internas; el portal queda fuera.
- **Propuesta:** semántica por tipo y máximo de 5 filtros.
- **Bloqueante:** ADR de almacenamiento de respuestas variables ([[hu-046-radicar-con-formulario-de-empresa|HU-046]]).

## Relacionado

- [[ep-013-formularios-configurables]] · [[tablero-scrum]] · [[formularios-configurables]] · [[dashboard-y-metricas]] · [[persistencia-postgresql]] · [[roles-y-permisos]] · [[criterios-de-aceptacion]]
