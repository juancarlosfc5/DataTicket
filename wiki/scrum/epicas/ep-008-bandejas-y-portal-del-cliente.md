---
title: "EP-008 — Bandejas internas y portal del cliente"
type: epica
status: vigente
estado: Aprobada
tags: [scrum, scrum/epica, producto/portal, producto/dashboard, producto/seguridad]
sources: ["PRD.md §5", "PRD.md §6.2", "PRD.md §6.4", "PRD.md §10", "PRD.md §12", "PRD.md §13", "PRD.md §14"]
aliases: ["EP-008", "Bandejas internas y portal del cliente", "Bandejas y portal del cliente"]
fase_prd: "1"
criterios_prd: [CA-01, CA-02, CA-10]
historias: ["[[hu-023-bandeja-de-mis-tickets-y-equipo]]", "[[hu-024-portal-solicitante-consulta-tickets]]", "[[hu-025-portal-coordinador-consulta-tickets]]"]
dependencias: ["[[ep-002-identidad-y-acceso]]", "[[ep-003-empresas-usuarios-y-equipos]]", "[[ep-005-radicacion-de-tickets]]", "[[ep-006-triage-asignacion-y-participantes]]", "[[ep-007-trabajo-interno-y-estados]]"]
created: 2026-10-07
updated: 2026-10-09
---

# EP-008 — Bandejas internas y portal del cliente

Listas de trabajo para el espacio interno ("mis tickets" y bandeja por equipo) y la consulta básica del portal del cliente (solicitante y coordinador) con aislamiento multiempresa verificado en backend. El portal se adelanta a la Fase 1 por [[adr-0008-portal-cliente-en-fase-1]].

## Objetivo

Que cada persona vea exactamente los tickets que le corresponden: los internos, los tickets donde participan y una vista limitada de su equipo; los clientes, sus tickets (solicitante) o los de su empresa (coordinador), siempre con `ClientStatus` y sin ningún dato interno.

## Valor esperado

- Desarrollo y Producción encuentran su trabajo sin depender de la PM (PRD §10).
- El cliente sabe que su caso fue recibido y si sigue en atención (PRD §3).
- Los invariantes de aislamiento (CA-01) y no exposición (CA-02, CA-10) tienen evidencia automatizada desde el piloto ([[adr-0008-portal-cliente-en-fase-1]]).

## Fase del PRD

Fase 1 — Piloto operativo ("bandejas") (PRD §13). La consulta básica del portal se entrega en la Fase 1 por decisión de la persona usuaria ([[adr-0008-portal-cliente-en-fase-1]]); el PRD §13 la ubicaba en la Fase 3.

## Actores

- Integrantes de Desarrollo (`Team.Development`) y Producción (`Team.Production`).
- Elizabeth — PM (rol `ProductManager`) y administradores (`Administrator`) en su bandeja "mis tickets".
- Solicitante (`Requester`).
- Coordinador de empresa (`CompanyCoordinator`).

## Alcance

- Bandeja "mis tickets" y bandeja de equipo (`Inbox`) con filtros por estado interno y campos de lista limitados ([[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023]]).
- Portal del solicitante: lista y detalle de sus propios tickets (`ListClientTickets`, `GetClientTicket`) ([[hu-024-portal-solicitante-consulta-tickets|HU-024]]).
- Portal del coordinador: lista y detalle de los tickets de toda su empresa ([[hu-025-portal-coordinador-consulta-tickets|HU-025]]).

## Fuera de alcance

- Bandeja global y cola de triage de la PM y del admin de cobertura ([[hu-016-cola-global-de-triage|HU-016]], [[hu-017-cola-de-cobertura-y-tomar-ticket|HU-017]]).
- Respuesta formal visible en el portal ([[hu-034-portal-respuesta-formal|HU-034]], Fase 2).
- Resumen por estado y métricas del portal ([[hu-042-resumen-de-estados-en-portal|HU-042]], Fase 3).
- Listados exportables y vistas guardadas (PRD §10, §16.4).
- Chat visible al cliente (PRD §13).
- Row-Level Security de PostgreSQL (decisión abierta en [[pendientes]] §4).

## Requisitos y reglas de negocio

- Desarrollo y Producción usan bandejas por equipo y responsabilidad; el detalle requiere asociación además de la pertenencia al equipo (PRD §10).
- El solicitante consulta los tickets creados por sí mismo; el coordinador, los de toda su empresa (PRD §5 regla 1, §10).
- Cada ticket del portal muestra estado resumido, datos de radicación y la respuesta formal cuando exista (PRD §10).
- No se exponen chat, notas internas, adjuntos internos, participantes de Data Global ni estados de Desarrollo/PR/Producción (PRD §5 regla 6, §10).
- Toda consulta del portal se restringe por empresa y permisos en el servidor (PRD §5 regla 2, §12).

## Criterios del PRD cubiertos

- PRD CA-01 — (total para listar y consultar; la descarga queda sujeta al riesgo V-17) una cuenta de la empresa A no lista ni consulta tickets de B → [[criterios-de-aceptacion]]
- PRD CA-02 — (parcial: falta la respuesta formal, [[hu-034-portal-respuesta-formal|HU-034]]) solicitante y coordinador ven su alcance con estado resumido y datos de radicación → [[criterios-de-aceptacion]]
- PRD CA-10 — (parcial: el portal; los correos se cubren en EP-010) el cliente no ve estados técnicos, URL de PR, participantes internos ni chat → [[criterios-de-aceptacion]]

## Dependencias

- [[ep-002-identidad-y-acceso]] — sesión, roles de cliente e internos, `ICurrentUser` con `CompanyId`.
- [[ep-003-empresas-usuarios-y-equipos]] — empresas, usuarios cliente y equipos internos.
- [[ep-005-radicacion-de-tickets]] — tickets y adjuntos de radicación.
- [[ep-006-triage-asignacion-y-participantes]] — participantes vigentes que alimentan "mis tickets".
- [[ep-007-trabajo-interno-y-estados]] — estados internos y su mapeo a `ClientStatus`; URL de PR para las pruebas de no exposición.
- [[hu-010-datos-sinteticos-de-desarrollo|HU-010]] — empresas A y B sintéticas para las pruebas negativas.

## Historias de usuario

- [[hu-023-bandeja-de-mis-tickets-y-equipo|HU-023 — Bandeja de mis tickets y de equipo]] (Sprint 4)
- [[hu-024-portal-solicitante-consulta-tickets|HU-024 — Portal: el solicitante consulta sus tickets]] (Sprint 4)
- [[hu-025-portal-coordinador-consulta-tickets|HU-025 — Portal: el coordinador consulta los tickets de su empresa]] (Sprint 4)

## Criterio de completitud

- [ ] HU-023, HU-024 y HU-025 están `Completada` con su matriz de evidencia.
- [ ] Pruebas de integración negativas A↔B y A1↔A2 en verde para lista y detalle del portal (PRD CA-01).
- [ ] Prueba de integración que compara las claves del JSON del portal con una lista permitida y busca cadenas prohibidas (estados técnicos, URL de PR, nombres internos) (PRD CA-10 parcial).
- [ ] La bandeja de equipo nunca devuelve descripción, adjuntos ni participantes (inspección de JSON).
- [ ] La decisión sobre Row-Level Security sigue registrada como riesgo o se resolvió con ADR.

## Riesgos e incógnitas

- **Row-Level Security** es decisión abierta ([[pendientes]] §4, [[persistencia-postgresql]]): mientras tanto, el aislamiento depende del filtro en backend y de sus pruebas; un error en una consulta nueva puede filtrar datos.
- **V-17 / [[adr-0006-urls-publicas-azure-blob]]:** el portal solo entrega URL de adjuntos de tickets visibles, pero las URL públicas del Blob permiten abrir el archivo fuera del portal; CA-01 "descargar desde el portal" se cumple solo dentro del portal.
- **Hallazgo 4:** `Attachment` no tiene contexto ni marca interno/externo; sin ella no se puede garantizar que el portal liste solo adjuntos de radicación.
- **Inferencia de producto:** que el cliente vea los adjuntos de radicación es una inferencia de la wiki ("se entiende que sí", [[archivos-adjuntos]]), no una frase literal del PRD.
- **Hallazgo 1 y V-10:** la bandeja "por equipo y responsabilidad" no tiene definición operativa; los campos de lista de la bandeja de equipo son propuesta.
- **V-03:** orden de las bandejas por prioridad calculada o ajustada sin decidir.
- **Hallazgo 2:** el dashboard (Fase 3) frente a CA-13 en el MVP; afecta a la frontera con [[ep-012-dashboard-y-metricas]].

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[adr-0008-portal-cliente-en-fase-1]] · [[roles-y-permisos]] · [[estados-del-ticket]]
- [[dashboard-y-metricas]] · [[archivos-adjuntos]] · [[persistencia-postgresql]] · [[criterios-de-aceptacion]] · [[pendientes]]
- [[ep-007-trabajo-interno-y-estados]] · [[ep-010-respuesta-formal-y-cierre]] · [[ep-012-dashboard-y-metricas]]
