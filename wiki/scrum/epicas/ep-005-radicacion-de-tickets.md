---
title: "EP-005 — Radicación de tickets"
type: epica
status: vigente
estado: Aprobada
tags: [scrum, scrum/epica, producto/ticket, producto/archivos, producto/prioridad]
sources: ["PRD.md §6.1", "PRD.md §8", "PRD.md §9", "PRD.md §5", "PRD.md §12", "PRD.md §14"]
aliases: ["EP-005", "Radicación de tickets"]
fase_prd: "1"
criterios_prd: ["PRD CA-15", "PRD CA-09", "PRD CA-14", "PRD CA-01"]
historias: ["[[hu-013-radicar-ticket]]", "[[hu-014-adjuntos-en-radicacion]]", "[[hu-015-ajustar-prioridad-manualmente]]"]
dependencias: ["[[ep-001-fundaciones-tecnicas]]", "[[ep-002-identidad-y-acceso]]", "[[ep-003-empresas-usuarios-y-equipos]]", "[[ep-004-auditoria-append-only]]"]
created: 2026-10-07
updated: 2026-10-09
---

# EP-005 — Radicación de tickets

Los usuarios cliente radican tickets con el formulario común del piloto, con prioridad calculada por la matriz urgencia × impacto y adjuntos validados en backend; Data Global puede ajustar la prioridad dejando rastro auditable (PRD §6.1, §8, §9).

## Objetivo

Permitir que un solicitante o coordinador autenticado cree un ticket de su empresa —empresa y solicitante derivados de la cuenta— con categoría, título, descripción, urgencia, impacto y archivos, de modo que el ticket nazca en `New`, entre en la cola de triage y quede auditado; y permitir el ajuste manual de prioridad con motivo obligatorio.

## Valor esperado

- Las solicitudes dejan de llegar dispersas: un único punto de entrada con contexto y archivos (PRD §3, §4 objetivo 1).
- La prioridad inicial es objetiva y reproducible (matriz de 9 combinaciones), y cualquier excepción queda justificada (PRD §6.1).
- El piloto opera con el formulario común sin esperar a los formularios configurables (PRD §9, CA-15).

## Fase del PRD

Fase 1 — Piloto operativo (PRD §13): "formulario común, radicación, adjuntos".

## Actores

- Solicitante de cliente (`Requester`): radica para sí mismo (PRD §5, §6.1).
- Coordinador de empresa (`CompanyCoordinator`): también puede radicar (PRD §5).
- Elizabeth — PM (`ProductManager`) y administrador (`Administrator`): ajustan la prioridad (propuesta, V-03).

## Alcance

- Caso de uso `SubmitTicket`, endpoint de radicación y vista del formulario común en el portal del cliente.
- Catálogo de categorías de solo lectura con semilla inicial provisional (V-12).
- Cálculo de `CalculatedPriority` en el dominio con la matriz del PRD.
- Número de ticket único asignado por el servidor (formato pendiente, V-16).
- Adjuntos en la radicación: tipos permitidos, ≤ 10 MB por archivo, validación de extensión + MIME + firma, binario en Blob (`IFileStorage`, Azurite en local) y metadatos en PostgreSQL.
- Ajuste manual de prioridad (`PriorityOverride`) con motivo obligatorio y auditoría.

## Fuera de alcance

- Administración del catálogo de categorías (V-12; sin dueño definido).
- Formularios configurables por empresa (Fase 4, [[ep-013-formularios-configurables]]; [[hu-046-radicar-con-formulario-de-empresa|HU-046]]).
- Consulta posterior del ticket por el cliente (portal: [[ep-008-bandejas-y-portal-del-cliente]], [[adr-0008-portal-cliente-en-fase-1]]).
- Selector genérico de producto (PRD §9).
- Radicación por correo entrante o automática (PRD §13).
- Radicación por personas internas en nombre de un cliente (el PRD no la contempla; propuesta: no permitida).
- URLs firmadas (SAS) o acceso autenticado a archivos (ADR-0006 vigente).
- Notificación por correo al radicar (no está en PRD §11).

## Requisitos y reglas de negocio

- La cuenta determina empresa y usuario que radica; no se eligen en el formulario (PRD §6.1, §9).
- Formulario común: categoría, título ≤ 120 caracteres, descripción, urgencia (baja/media/alta), impacto (bajo/medio/alto), prioridad calculada y adjuntos opcionales (PRD §6.1).
- Matriz de prioridad de 9 combinaciones (PRD §6.1; [[matriz-de-prioridad]]).
- Al radicarse, el ticket entra en la cola de Elizabeth con estado `New` (PRD §6.2); para el cliente, `Received` (PRD §6.4).
- Ajuste manual: calculada, nueva, quién, cuándo y motivo (PRD §6.1).
- Adjuntos: PDF, imágenes, XML y Excel; 10 MB por archivo; validados en backend; metadatos en PostgreSQL y binario en Azure Blob (PRD §8).
- URL pública permanente: riesgo aceptado (PRD §8; [[adr-0006-urls-publicas-azure-blob]]).
- La radicación y el cambio de prioridad son eventos auditables (PRD §12).

## Criterios del PRD cubiertos

- PRD CA-15 (total: el piloto opera con el formulario común) → [[criterios-de-aceptacion]]
- PRD CA-09 (parcial: radicación; el chat lo cubre [[hu-029-adjuntos-e-imagenes-en-chat|HU-029]] y la respuesta formal [[hu-033-emitir-respuesta-formal|HU-033]]) → [[criterios-de-aceptacion]]
- PRD CA-14 (parcial: prioridad) → [[criterios-de-aceptacion]]
- PRD CA-01 (parcial: la empresa del ticket siempre es la de la cuenta; la consulta y descarga desde el portal las cubren HU-024/HU-025) → [[criterios-de-aceptacion]]

## Dependencias

- [[ep-001-fundaciones-tecnicas]] — CI, migraciones, shell del frontend ([[hu-002-shell-y-navegacion-por-rol|HU-002]]).
- [[ep-002-identidad-y-acceso]] — sesión, `ICurrentUser` con rol y `company_id` ([[hu-003-iniciar-y-cerrar-sesion|HU-003]], [[hu-004-contexto-de-usuario-y-autorizacion|HU-004]]).
- [[ep-003-empresas-usuarios-y-equipos]] — empresas y usuarios cliente existentes ([[hu-007-administrar-empresas-cliente|HU-007]], [[hu-008-administrar-usuarios-cliente|HU-008]], [[hu-010-datos-sinteticos-de-desarrollo|HU-010]]).
- [[ep-004-auditoria-append-only]] — `IAuditLog` ([[hu-011-registrar-eventos-auditables|HU-011]]).

## Historias de usuario

- [[hu-013-radicar-ticket|HU-013 — Radicar ticket]] · Sprint 2
- [[hu-014-adjuntos-en-radicacion|HU-014 — Adjuntos en la radicación]] · Sprint 2
- [[hu-015-ajustar-prioridad-manualmente|HU-015 — Ajustar prioridad manualmente]] · Sprint 3

## Criterio de completitud

- [ ] HU-013, HU-014 y HU-015 están `Completada`.
- [ ] Prueba unitaria exhaustiva de las 9 combinaciones de la matriz en verde.
- [ ] Pruebas de integración demuestran que la empresa y el solicitante del ticket siempre salen de la cuenta autenticada.
- [ ] Pruebas de integración demuestran el rechazo en backend de tipo, firma y tamaño inválidos (límite 10 485 760 bytes).
- [ ] La radicación y el ajuste de prioridad dejan `AuditEntry` verificadas.
- [ ] PRD CA-15 puede marcarse en [[criterios-de-aceptacion]] con la evidencia de HU-013.
- [ ] No quedan dependencias bloqueantes dentro del alcance.

## Riesgos e incógnitas

- **V-12** — catálogo de categorías y su administrador: se usa una semilla provisional; cambiarla no debe exigir cambio de código.
- **V-16** — formato del número de ticket: la unicidad se implementa ya; el formato visible queda provisional.
- **V-03** — quién ajusta la prioridad y si las colas ordenan por la calculada o la ajustada.
- **V-06** — extensiones y MIME exactos de "imágenes" y "Excel"; validación de firma propuesta.
- **V-17 / ADR-0006** — la URL pública permite acceso entre empresas fuera del portal; riesgo aceptado pero pendiente de aprobación explícita de la dirección (PRD §16.6).
- **Máximo de archivos por solicitud** — decisión técnica abierta ([[pendientes]] §4); condiciona el límite de cuerpo del backend (nginx admite hoy 50 MB en `/api/`).
- **Hallazgo 4** — `Attachment` no tiene contexto (radicación/chat/respuesta formal) ni marca interno/externo en el modelo; HU-014 propone añadirlo.
- **Hallazgo 7** — no existe caso de uso para ajustar prioridad en el mapa de puertos; HU-015 propone uno.
- Portal del cliente: la consulta básica se entrega en Fase 1 según [[adr-0008-portal-cliente-en-fase-1]].

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[flujo-del-ticket]] · [[matriz-de-prioridad]] · [[archivos-adjuntos]]
- [[formularios-configurables]] · [[adr-0006-urls-publicas-azure-blob]] · [[modelo-de-dominio]] · [[criterios-de-aceptacion]]
- [[ep-004-auditoria-append-only]] · [[ep-006-triage-asignacion-y-participantes]] · [[ep-008-bandejas-y-portal-del-cliente]] · [[ep-013-formularios-configurables]]
