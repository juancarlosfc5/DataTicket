---
title: "EP-013 — Formularios configurables por cliente"
type: epica
status: vigente
estado: Aprobada
tags: [scrum, scrum/epica, producto/formularios]
sources: ["PRD.md §1", "PRD.md §2", "PRD.md §4", "PRD.md §6.1", "PRD.md §9", "PRD.md §10", "PRD.md §12", "PRD.md §13", "PRD.md §14"]
aliases: ["EP-013", "Formularios configurables por cliente"]
fase_prd: "4"
criterios_prd: ["CA-15", "CA-01"]
historias: ["[[hu-043-constructor-de-plantillas]]", "[[hu-044-asociar-plantilla-a-empresa]]", "[[hu-045-versionar-plantillas]]", "[[hu-046-radicar-con-formulario-de-empresa]]", "[[hu-047-consultar-por-campos-variables]]"]
dependencias: ["[[ep-003-empresas-usuarios-y-equipos]]", "[[ep-004-auditoria-append-only]]", "[[ep-005-radicacion-de-tickets]]", "[[ep-006-triage-asignacion-y-participantes]]", "[[ep-008-bandejas-y-portal-del-cliente]]"]
created: 2026-10-07
updated: 2026-10-09
---

# EP-013 — Formularios configurables por cliente

Personaliza la radicación por empresa cliente: Data Global diseña plantillas de campos adicionales, las asocia a una empresa, las versiona y las publica; los clientes radican con su formulario y los equipos internos consultan los campos usados operativamente. Es la última fase comprometida y no bloquea el piloto (PRD §9, §13, §14.15).

## Objetivo

Capturar en la radicación la información específica de cada cliente (p. ej. sede, centro de costo) sin perder los campos núcleo ni la trazabilidad de qué formulario se usó en cada ticket (PRD §9).

## Valor esperado

- Tickets con el contexto que cada cliente necesita aportar, sin un formulario común sobrecargado.
- Historial fiable: cada ticket conserva la versión y los campos con que se creó (PRD §9).
- Equipos internos capaces de filtrar por los campos variables que usan en la operación (PRD §9).

## Fase del PRD

Fase 4 — Formularios configurables, "última fase comprometida" (PRD §13). Sprint 9 en [[tablero-scrum]].

## Actores

- Administrador de DataTicket (`Administrator`): diseña, asocia, versiona y publica plantillas.
- Elizabeth — PM (rol `ProductManager`): consulta por campos variables; su participación en el constructor es propuesta.
- Solicitante (`Requester`) y coordinador (`CompanyCoordinator`): radican con el formulario de su empresa.
- Desarrollo y Producción: consultan y filtran tickets por campos variables en sus bandejas.

## Alcance

- Constructor de plantillas (`FormTemplate`) con campos de tipo, etiqueta, obligatoriedad, ayuda y opciones → [[hu-043-constructor-de-plantillas|HU-043]].
- Asociación de una plantilla a una empresa y orden visible de sus campos → [[hu-044-asociar-plantilla-a-empresa|HU-044]].
- Versionado y publicación inmutable; cada ticket conserva su versión → [[hu-045-versionar-plantillas|HU-045]].
- Radicación con el formulario de la empresa, con validación de respuestas variables en backend y persistencia en PostgreSQL → [[hu-046-radicar-con-formulario-de-empresa|HU-046]].
- Consulta y filtrado interno por campos marcados como consultables → [[hu-047-consultar-por-campos-variables|HU-047]].

## Fuera de alcance

- Que los clientes creen o modifiquen sus plantillas (PRD §9).
- Reemplazar o eliminar los campos núcleo del formulario común (PRD §9).
- Selector genérico de producto (PRD §9).
- Usar como campos reales los ejemplos del prototipo (sede, centro de costo, placa…): son semillas de diseño (PRD §9).
- Listados exportables y vistas guardadas (PRD §10, §16.4).
- Lógica condicional entre campos, cálculos o validaciones con expresiones (no aparecen en el PRD).
- Filtrar por campos variables desde el portal del cliente (propuesta: queda fuera hasta que se pida).

## Requisitos y reglas de negocio

- El piloto opera con el formulario común; esta fase no lo bloquea (PRD §9, §14.15).
- Constructor administrado desde DataTicket; campos con tipo, etiqueta, obligatoriedad, ayuda y opciones (PRD §9).
- Plantilla asociada a empresa con orden visible de campos (PRD §9).
- Versionado: cada ticket conserva los campos y la versión con que se creó (PRD §9).
- Se conservan los campos núcleo y las respuestas variables se guardan en PostgreSQL, con capacidad de consultar los campos usados operativamente (PRD §9).
- Empresa y solicitante se derivan de la cuenta, no del formulario (PRD §6.1, §9).
- Toda consulta se restringe por empresa y permisos en el servidor (PRD §12).

## Criterios del PRD cubiertos

- PRD CA-15 — Los formularios por cliente no bloquean el piloto: se construyen en fase separada y una empresa sin plantilla sigue con el formulario común (todas las HU) → [[criterios-de-aceptacion]]
- PRD CA-01 (parcial) — Las plantillas, respuestas y filtros respetan el aislamiento entre empresas (HU-046, HU-047) → [[criterios-de-aceptacion]]

## Dependencias

- [[ep-003-empresas-usuarios-y-equipos]]: empresas cliente ([[hu-007-administrar-empresas-cliente]]).
- [[ep-005-radicacion-de-tickets]]: formulario común y radicación ([[hu-013-radicar-ticket]], [[hu-014-adjuntos-en-radicacion]]).
- [[ep-006-triage-asignacion-y-participantes]] y [[ep-008-bandejas-y-portal-del-cliente]]: bandejas y detalle donde se consultan los campos ([[hu-016-cola-global-de-triage]], [[hu-023-bandeja-de-mis-tickets-y-equipo]], [[hu-024-portal-solicitante-consulta-tickets]]).
- [[ep-004-auditoria-append-only]]: auditoría de la publicación de plantillas (propuesta).
- Decisiones abiertas: almacenamiento de respuestas variables (`jsonb` frente a tablas), catálogo de categorías (V-12), enrutador y estado del frontend → [[pendientes]].

## Historias de usuario

- [[hu-043-constructor-de-plantillas|HU-043 — Constructor de plantillas de formulario]]
- [[hu-044-asociar-plantilla-a-empresa|HU-044 — Asociar plantilla a empresa y ordenar campos]]
- [[hu-045-versionar-plantillas|HU-045 — Versionar y publicar plantillas]]
- [[hu-046-radicar-con-formulario-de-empresa|HU-046 — Radicar con el formulario de la empresa]]
- [[hu-047-consultar-por-campos-variables|HU-047 — Consultar tickets por campos variables]]

## Criterio de completitud

- [ ] HU-043 a HU-047 están `Completada`, con su matriz de evidencia registrada.
- [ ] Una prueba de regresión demuestra que una empresa sin plantilla radica con el formulario común sin cambios (CA-15).
- [ ] Una prueba demuestra que un ticket creado con la versión N conserva sus campos y etiquetas tras publicar la versión N+1.
- [ ] Pruebas negativas A↔B: una empresa no recibe ni usa la plantilla de otra, y los filtros internos no mezclan empresas.
- [ ] La decisión de almacenamiento de respuestas variables está registrada como ADR aceptado y reflejada en [[persistencia-postgresql]].
- [ ] No quedan dependencias bloqueantes dentro del alcance.

## Riesgos e incógnitas

- **Almacenamiento de respuestas variables sin decidir** (`jsonb` frente a tablas por campo): condiciona la migración, la validación y el rendimiento de los filtros de HU-047. Requiere un ADR antes de implementar HU-046.
- **Catálogo de categorías (V-12):** la categoría es campo núcleo; si su catálogo se vuelve configurable por empresa, se cruzaría con esta épica.
- **Alcance del constructor:** tipos de campo y límites (número de campos, opciones) son propuesta; un constructor "abierto" puede derivar en dificultad `Muy alto`.
- **Rol del PM en el constructor:** el PRD solo dice "administrado desde DataTicket"; se propone restringirlo a `Administrator`.
- **Datos sensibles en campos variables:** un cliente podría capturar datos personales; la retención está pendiente (PRD §12, §16.3).

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[formularios-configurables]] · [[flujo-del-ticket]] · [[persistencia-postgresql]] · [[modelo-de-dominio]] · [[roles-y-permisos]] · [[criterios-de-aceptacion]] · [[ep-005-radicacion-de-tickets]]
