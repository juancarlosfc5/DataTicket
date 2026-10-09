---
title: "HU-044 — Asociar plantilla a empresa y ordenar campos"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/formularios, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §5", "PRD.md §9", "PRD.md §12", "PRD.md §14"]
aliases: ["HU-044", "Asociar plantilla a empresa y ordenar campos", "Asociar plantilla a empresa"]
epica: "[[ep-013-formularios-configurables]]"
criterios_prd: ["CA-15", "CA-01"]
componentes: ["Backend (Domain)", "Backend (Application)", "Backend (Infrastructure)", "Backend (Api)", "Persistencia PostgreSQL", "Frontend (models)", "Frontend (controllers)", "Frontend (views)"]
dificultad: "Medio"
sprint_sugerido: "Sprint 9"
dependencias: ["[[hu-043-constructor-de-plantillas]]", "[[hu-007-administrar-empresas-cliente]]", "[[hu-011-registrar-eventos-auditables]]"]
relacionadas: ["[[hu-045-versionar-plantillas]]", "[[hu-046-radicar-con-formulario-de-empresa]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-044 — Asociar plantilla a empresa y ordenar campos

El administrador asocia una plantilla a una empresa cliente y define el orden visible de sus campos. Cada empresa tiene como máximo una plantilla asociada; una empresa sin plantilla publicada sigue usando el formulario común (PRD §9).

## Historia de usuario

**COMO** administrador de DataTicket  
**QUIERO** asociar una plantilla a una empresa cliente y ordenar sus campos  
**PARA** que esa empresa radique con su formulario y vea los campos en el orden adecuado

## Contexto

PRD §9 incluye la "asociación de plantilla a empresa y orden visible de sus campos". El [[modelo-de-dominio]] propone `FormTemplate` con `CompanyId`. La asociación solo tiene efecto para el cliente cuando existe una versión publicada ([[hu-045-versionar-plantillas|HU-045]]); hasta entonces la empresa sigue con el formulario común (PRD §14.15).

## Alcance

- Asociar una plantilla a una empresa (una plantilla pertenece a una sola empresa; una empresa tiene como máximo una plantilla asociada; propuesta).
- Desasociar: la empresa vuelve al formulario común sin afectar tickets existentes.
- Reordenar los campos del borrador; el nuevo orden se ve en la vista previa y llega al cliente al publicar.
- Ver desde la ficha de la empresa qué plantilla tiene y su versión publicada.
- Auditoría de asociación, desasociación y reordenamiento (propuesta).

## Fuera de alcance

- Crear o editar campos → [[hu-043-constructor-de-plantillas|HU-043]].
- Publicar versiones → [[hu-045-versionar-plantillas|HU-045]].
- Compartir una misma plantilla entre varias empresas (propuesta: no; se duplica si hace falta).
- Plantillas distintas por categoría o por usuario dentro de una empresa (no aparecen en el PRD).
- Que el cliente vea o cambie la asociación (PRD §9).

## Requisitos y reglas de negocio

- Asociación de plantilla a empresa y orden visible de sus campos (PRD §9).
- La construcción de esta fase no bloquea el piloto con formulario común (PRD §9, §14.15).
- Cada ticket pertenece a una empresa y las consultas se restringen por empresa (PRD §12).
- Propuesta: relación 1 empresa ↔ 0..1 plantilla; asociar a una empresa que ya tiene plantilla → 409 (primero se desasocia).
- Propuesta: no se asocia una plantilla a una empresa inactiva (409).
- Propuesta: el orden se guarda como parte del borrador y se congela en cada versión publicada.

## Invariantes en juego

- Invariante 1 (`AGENTS.md` §7): la plantilla de una empresa nunca se sirve a usuarios de otra (verificado en HU-046; aquí se garantiza la relación única).
- Invariante 8: auditoría append-only de la asociación (propuesta).

## Criterios del PRD cubiertos

- PRD CA-15 (parcial: una empresa sin plantilla publicada sigue con el formulario común) → [[criterios-de-aceptacion]]
- PRD CA-01 (parcial: la asociación única por empresa, reforzada en la base de datos, evita mezclar formularios entre empresas) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-013-formularios-configurables]]
- Dependencias: [[hu-043-constructor-de-plantillas|HU-043]] (plantillas), [[hu-007-administrar-empresas-cliente|HU-007]] (empresas y su estado activo), [[hu-011-registrar-eventos-auditables|HU-011]].
- Relacionadas: [[hu-045-versionar-plantillas|HU-045]] (el orden se congela al publicar), [[hu-046-radicar-con-formulario-de-empresa|HU-046]] (consume la asociación).
- Decisiones: abierta: almacenamiento de definiciones ([[pendientes]]).

## Componentes afectados

- Backend (Domain): `FormTemplate.CompanyId`, orden de `FormField`.
- Backend (Application): casos de uso `AssignFormTemplateToCompany`, `UnassignFormTemplateFromCompany`, `ReorderFormTemplateFields` (nombres propuestos).
- Backend (Infrastructure): índice único parcial por `company_id`; migración.
- Backend (Api): endpoints bajo `/api/admin/companies/{companyId}/form-template` y `/api/admin/form-templates/{id}/draft/field-order`.
- Frontend (models, controllers, views): ficha de empresa y constructor (ordenamiento).

## Dificultad

**Nivel:** Medio

**Justificación:** reglas de unicidad y orden sencillas, pero con restricción en base de datos, autorización y una interacción de ordenamiento accesible en la interfaz.

## Contrato backend ↔ frontend

Propuesta; se ratifica en T-01.

**`PUT /api/admin/companies/{companyId}/form-template`**

```json
{ "formTemplateId": "e2b7…" }
```

→ `200`:

```json
{ "companyId": "7d41…", "companyName": "Empresa Demo Norte", "formTemplateId": "e2b7…", "formTemplateName": "Formulario Empresa Demo Norte", "latestPublishedVersion": null, "effectiveForm": "Common" }
```

`effectiveForm` es `"Common"` mientras no haya versión publicada y `"Template"` cuando la hay.

**`GET /api/admin/companies/{companyId}/form-template`** → `200` (mismo cuerpo) o `404` si no tiene plantilla.

**`DELETE /api/admin/companies/{companyId}/form-template`** → `204`.

**`PUT /api/admin/form-templates/{formTemplateId}/draft/field-order`**

```json
{ "fieldKeys": ["centro_costo", "sede", "fecha_evento"] }
```

→ `200` con el borrador completo (contrato de HU-043) en el nuevo orden.

| Código | Caso | `type` |
|---|---|---|
| 400 | `fieldKeys` con claves faltantes, sobrantes o repetidas respecto del borrador | `urn:dataticket:validation` |
| 401 | Sin sesión | — |
| 403 | PM (propuesta), Desarrollo, Producción o cliente | `urn:dataticket:forbidden` |
| 404 | Empresa o plantilla inexistente | `urn:dataticket:not-found` |
| 409 | La empresa ya tiene plantilla; la plantilla ya está asociada a otra empresa; empresa inactiva | `urn:dataticket:conflict` |

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar rutas, relación 1↔0..1, regla de empresa inactiva y `effectiveForm`.
- [ ] **T-02 — Dominio** · Capa: Backend (Domain) · Dificultad: Bajo  
  Descripción: primero pruebas: `Reorder_WithMissingKey_Throws`, `Reorder_WithDuplicatedKey_Throws`, `Reorder_WithSameSet_ChangesOrder`, `Assign_WhenAlreadyAssignedToOtherCompany_Throws`. Luego métodos de `FormTemplate`.
- [ ] **T-03 — Casos de uso** · Capa: Backend (Application) · Dificultad: Medio  
  Descripción: primero pruebas: `AssignFormTemplate_ByDeveloper_ReturnsForbidden`, `AssignFormTemplate_ToCompanyWithTemplate_ReturnsConflict`, `AssignFormTemplate_ToInactiveCompany_ReturnsConflict`, `Unassign_DoesNotTouchExistingTickets`, `Assign_WritesAudit`. Luego implementación con `ICurrentUser` e `IAuditLog`.
- [ ] **T-04 — Persistencia y migración** · Capa: Backend (Infrastructure) · Dificultad: Bajo  
  Descripción: índice único parcial en `form_templates(company_id) WHERE company_id IS NOT NULL`; migración `AddFormTemplateCompanyAssignment`; prueba de integración de la restricción bajo concurrencia.
- [ ] **T-05 — Endpoints** · Capa: Backend (Api) · Dificultad: Bajo  
  Descripción: primero pruebas de integración (200, 204, 400, 403, 404, 409). Luego endpoints.
- [ ] **T-06 — Frontend** · Capa: Frontend (models/controllers/views) · Dificultad: Medio  
  Descripción: Vitest del modelo (validación de `fieldKeys`, `effectiveForm`); `useCompanyFormTemplateController` y ampliación de `useFormTemplateBuilderController` con `moveField(up/down)`; vistas `CompanyFormTemplateView` y controles de orden con botones "Subir/Bajar" accesibles por teclado (arrastrar es opcional).

## Criterios de aceptación

### CHU-01 — Asociar plantilla

**Dado** la empresa "Empresa Demo Norte" sin plantilla y una plantilla en borrador sin empresa  
**Cuando** el administrador la asocia  
**Entonces** recibe `200` con `effectiveForm: "Common"` (aún sin versión publicada) y la ficha de la empresa muestra la plantilla.

### CHU-02 — Una plantilla por empresa

**Dado** una empresa que ya tiene plantilla asociada  
**Cuando** el administrador intenta asociarle otra  
**Entonces** recibe `409` y la asociación original se mantiene.

### CHU-03 — Plantilla de otra empresa

**Dado** una plantilla asociada a la empresa A  
**Cuando** el administrador intenta asociarla también a la empresa B  
**Entonces** recibe `409` y B sigue sin plantilla.

### CHU-04 — Ordenar campos

**Dado** un borrador con campos `sede`, `centro_costo`, `fecha_evento`  
**Cuando** el administrador envía el orden `["centro_costo", "sede", "fecha_evento"]`  
**Entonces** recibe `200` y la vista previa muestra ese orden; **y cuando** envía `["sede", "sede", "fecha_evento"]` o `["sede", "fecha_evento"]`, recibe `400`.

### CHU-05 — Desasociar vuelve al formulario común

**Dado** una empresa con plantilla publicada y tickets radicados con ella  
**Cuando** el administrador la desasocia  
**Entonces** recibe `204`, el siguiente ticket de esa empresa se radica con el formulario común y los tickets anteriores conservan sus campos y versión.

### CHU-06 — Autorización

**Dado** un desarrollador y un coordinador de empresa  
**Cuando** llaman `PUT /api/admin/companies/{companyId}/form-template` directamente  
**Entonces** reciben `403` y la asociación no cambia.

### CHU-07 — Empresa inactiva

**Dado** una empresa desactivada ([[hu-007-administrar-empresas-cliente|HU-007]])  
**Cuando** el administrador intenta asociarle una plantilla  
**Entonces** recibe `409`.

### CHU-08 — Auditoría

**Dado** una asociación exitosa  
**Cuando** se consulta la auditoría  
**Entonces** existe una entrada `FormTemplateAssigned` (nombre propuesto) con actor, fecha/hora, empresa y plantilla.

### CHU-09 — Interfaz ante conflicto

**Dado** el administrador en la ficha de una empresa que otra persona acaba de asociar  
**Cuando** intenta asociar una plantilla y el backend responde `409`  
**Entonces** la vista muestra "Esta empresa ya tiene una plantilla asociada" y recarga la asociación vigente.

## Definition of Done

- [ ] **DoD-01** — CHU-01 a CHU-09 validados con evidencia en la matriz.
- [ ] **DoD-02** — Pruebas escritas primero y en verde (`cd backend && dotnet test`): `Reorder_*`, `AssignFormTemplate_*`, `Unassign_DoesNotTouchExistingTickets` e integración con PostgreSQL real (incluida la restricción única).
- [ ] **DoD-03** — `DataTicket.ArchitectureTests` en verde.
- [ ] **DoD-04** — Migración `AddFormTemplateCompanyAssignment` creada y aplicada en local.
- [ ] **DoD-05** — Frontend: `npm --prefix frontend test`, `npm --prefix frontend run lint` y `npm --prefix frontend run build` correctos.
- [ ] **DoD-06** — `docker compose up --build` y verificación manual: asociar, reordenar, desasociar.
- [ ] **DoD-07** — Wiki actualizada: [[formularios-configurables]] (relación empresa ↔ plantilla, orden), [[modelo-de-dominio]], [[persistencia-postgresql]].
- [ ] **DoD-08** — Revisión de `quality-reviewer` sin hallazgos CRÍTICO/ALTO y PR revisado por otra persona del equipo.
- [ ] **DoD-09** — La trazabilidad de la HU y de [[ep-013-formularios-configurables]] está actualizada.

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

## Notas y decisiones

- **Propuesta:** relación 1 empresa ↔ 0..1 plantilla y plantilla de una sola empresa (coherente con `FormTemplate.CompanyId` del [[modelo-de-dominio]]).
- **Propuesta:** el orden forma parte de la versión publicada.
- CHU-05 requiere tickets radicados con plantilla: se valida cuando [[hu-046-radicar-con-formulario-de-empresa|HU-046]] esté disponible o con datos sembrados en la prueba de integración.

## Relacionado

- [[ep-013-formularios-configurables]] · [[tablero-scrum]] · [[formularios-configurables]] · [[modelo-de-dominio]] · [[roles-y-permisos]] · [[persistencia-postgresql]] · [[criterios-de-aceptacion]]
