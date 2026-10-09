---
title: Tablero Scrum de DataTicket
type: indice
status: vigente
tags: [scrum, proceso]
sources: ["PRD.md §13", "PRD.md §14"]
aliases: [Tablero Scrum, Backlog, Product Backlog]
created: 2026-10-07
updated: 2026-10-09
---

# Tablero Scrum de DataTicket

Mapa del backlog: 13 épicas y 47 historias de usuario (HU), alineadas con las fases 0–4 del PRD (§13). Se generó con la skill `scrum-spec-orchestrator/`. El seguimiento de ejecución (checkmark, responsable y fecha) está en `PLAN-DE-TRABAJO.md`, en la raíz del repositorio. El detalle de cada HU vive en su propia nota y aquí no se duplica.

> [!info] Estado del backlog
> El 2026-10-09 la persona líder del proyecto (juancarlosfc5) aprobó explícitamente en bloque las 13 épicas y las 47 HU: todas están en estado `Aprobada` (`status: vigente`). La aprobación no cierra las decisiones abiertas de la sección final: cada HU las sigue ratificando en su tarea T-01 antes de implementar lo que dependa de ellas.

## Objetivo

Construir el MVP de DataTicket en el orden de las fases del PRD. La Fase 0 es bloqueante. Las fases 1 y 2 son P0, la Fase 3 es P1 y la Fase 4 es la última comprometida. Cada fase deja un incremento verificable y conserva los invariantes de `AGENTS.md` §7.

Stack y restricciones: [[stack-y-versiones]] · [[backend-hexagonal]] · [[frontend-mvc]]. Fases y fuera de alcance: [[fases-y-alcance]].

## Épicas

| Fase | Épica | HU |
|---|---|---|
| 0 — Base técnica (bloqueante) | [[ep-001-fundaciones-tecnicas\|EP-001 — Fundaciones técnicas y calidad continua]] | HU-001, HU-002 |
| 0 | [[ep-002-identidad-y-acceso\|EP-002 — Identidad y acceso]] | HU-003 a HU-006 |
| 0 | [[ep-003-empresas-usuarios-y-equipos\|EP-003 — Empresas, usuarios y equipos]] | HU-007 a HU-010 |
| 1 — Piloto operativo (P0) | [[ep-004-auditoria-append-only\|EP-004 — Auditoría append-only]] | HU-011, HU-012 |
| 1 | [[ep-005-radicacion-de-tickets\|EP-005 — Radicación de tickets]] | HU-013 a HU-015 |
| 1 | [[ep-006-triage-asignacion-y-participantes\|EP-006 — Triage, asignación y participantes]] | HU-016 a HU-019 |
| 1 | [[ep-007-trabajo-interno-y-estados\|EP-007 — Trabajo interno y estados]] | HU-020 a HU-022 |
| 1 | [[ep-008-bandejas-y-portal-del-cliente\|EP-008 — Bandejas internas y portal del cliente]] | HU-023 a HU-025 |
| 2 — Colaboración y entrega (P0) | [[ep-009-chat-interno-en-tiempo-real\|EP-009 — Chat interno en tiempo real]] | HU-026 a HU-032 |
| 2 | [[ep-010-respuesta-formal-y-cierre\|EP-010 — Respuesta formal y cierre manual]] | HU-033 a HU-036 |
| 2 | [[ep-011-notificaciones\|EP-011 — Notificaciones acordadas]] | HU-037, HU-038 |
| 3 — Operación medible (P1) | [[ep-012-dashboard-y-metricas\|EP-012 — Dashboard y métricas]] | HU-039 a HU-042 |
| 4 — Formularios configurables (última) | [[ep-013-formularios-configurables\|EP-013 — Formularios configurables por cliente]] | HU-043 a HU-047 |

## Sprints sugeridos

HU-011 pertenece a la Fase 1 pero se adelanta al Sprint 1: las acciones administrativas de HU-007, HU-008 y HU-009 se auditan con `IAuditLog`.

Los sprints son incrementos ordenados por dependencias. No estiman duración ni capacidad y no asignan personas. Una vez fijado el contrato (tarea T-01 de cada HU), las tareas de backend y frontend pueden avanzar en paralelo.

| Sprint | Fase | HU | Incremento |
|---|---|---|---|
| 0 | 0 | [[hu-001-integracion-continua\|HU-001]], [[hu-002-shell-y-navegacion-por-rol\|HU-002]], [[hu-003-iniciar-y-cerrar-sesion\|HU-003]], [[hu-004-contexto-de-usuario-y-autorizacion\|HU-004]] | CI, shell, login y autorización base |
| 1 | 0 (+ HU-011 de la Fase 1) | [[hu-005-invitar-y-activar-cuentas\|HU-005]], [[hu-006-restablecer-contrasena\|HU-006]], [[hu-007-administrar-empresas-cliente\|HU-007]], [[hu-008-administrar-usuarios-cliente\|HU-008]], [[hu-009-administrar-usuarios-internos-y-equipos\|HU-009]], [[hu-010-datos-sinteticos-de-desarrollo\|HU-010]], [[hu-011-registrar-eventos-auditables\|HU-011]] | Cuentas, empresas, equipos, datos sintéticos y puerto de auditoría |
| 2 | 1 | [[hu-013-radicar-ticket\|HU-013]], [[hu-014-adjuntos-en-radicacion\|HU-014]], [[hu-016-cola-global-de-triage\|HU-016]] | Radicar con adjuntos y verlo en la cola de la PM |
| 3 | 1 | [[hu-015-ajustar-prioridad-manualmente\|HU-015]], [[hu-017-cola-de-cobertura-y-tomar-ticket\|HU-017]], [[hu-018-asignar-y-agregar-participantes\|HU-018]], [[hu-019-reasignar-y-retirar-participantes\|HU-019]], [[hu-020-detalle-interno-del-ticket\|HU-020]] | Triage completo y detalle interno |
| 4 | 1 | [[hu-012-consultar-bitacora-del-ticket\|HU-012]], [[hu-021-cambiar-estado-interno\|HU-021]], [[hu-022-registrar-url-de-pr\|HU-022]], [[hu-023-bandeja-de-mis-tickets-y-equipo\|HU-023]], [[hu-024-portal-solicitante-consulta-tickets\|HU-024]], [[hu-025-portal-coordinador-consulta-tickets\|HU-025]] | Estados, bandejas y portal del cliente (piloto operativo) |
| 5 | 2 | [[hu-026-historial-del-chat-por-cursor\|HU-026]], [[hu-027-unirse-al-chat-del-ticket\|HU-027]], [[hu-028-enviar-y-recibir-mensajes\|HU-028]], [[hu-032-revocar-acceso-al-retirar-participante\|HU-032]] | Chat seguro en tiempo real |
| 6 | 2 | [[hu-029-adjuntos-e-imagenes-en-chat\|HU-029]], [[hu-030-confirmaciones-de-lectura\|HU-030]], [[hu-031-recuperar-mensajes-tras-reconexion\|HU-031]], [[hu-037-notificaciones-en-la-app\|HU-037]], [[hu-038-correo-de-vinculacion\|HU-038]] | Chat completo y notificaciones |
| 7 | 2 | [[hu-033-emitir-respuesta-formal\|HU-033]], [[hu-034-portal-respuesta-formal\|HU-034]], [[hu-035-correo-de-respuesta-formal\|HU-035]], [[hu-036-cerrar-ticket-manualmente\|HU-036]] | Entrega formal y cierre (MVP P0 completo) |
| 8 | 3 | [[hu-039-calculo-de-tiempos-habiles\|HU-039]], [[hu-040-panel-global-de-la-pm\|HU-040]], [[hu-041-panel-del-equipo-interno\|HU-041]], [[hu-042-resumen-de-estados-en-portal\|HU-042]] | Operación medible |
| 9 | 4 | [[hu-043-constructor-de-plantillas\|HU-043]], [[hu-044-asociar-plantilla-a-empresa\|HU-044]], [[hu-045-versionar-plantillas\|HU-045]], [[hu-046-radicar-con-formulario-de-empresa\|HU-046]], [[hu-047-consultar-por-campos-variables\|HU-047]] | Formularios por cliente |

## Cobertura de los criterios del PRD

| PRD CA | HU que lo cubren |
|---|---|
| CA-01 Aislamiento A↔B | HU-004, HU-008, HU-013, HU-014, HU-024, HU-025, HU-034, HU-042, HU-044, HU-046, HU-047 |
| CA-02 Solicitante y coordinador | HU-015, HU-024, HU-025, HU-034, HU-036, HU-042, HU-046 |
| CA-03 Cola de la PM y cobertura del admin | HU-016, HU-017, HU-020, HU-023 |
| CA-04 Asignar, reasignar y participantes | HU-011, HU-012, HU-018, HU-019 |
| CA-05 Tiempo real y recuperación | HU-026, HU-027, HU-028, HU-031 |
| CA-06 No participante y revocación | HU-019, HU-026, HU-027, HU-028, HU-030, HU-031, HU-032, HU-037 |
| CA-07 Confirmaciones de lectura | HU-030 |
| CA-08 Historial para incorporados y retirados | HU-018, HU-019, HU-026, HU-029, HU-031, HU-032 |
| CA-09 Tipos y tamaño de adjuntos | HU-014, HU-028, HU-029, HU-033 |
| CA-10 Nada interno al cliente | HU-012, HU-013, HU-015, HU-018, HU-020, HU-021, HU-022, HU-024, HU-025, HU-029, HU-034, HU-035, HU-042 |
| CA-11 Respuesta formal | HU-033, HU-034, HU-035 |
| CA-12 Cierre manual | HU-021, HU-036 |
| CA-13 Dashboard interno sin SLA | HU-039, HU-040, HU-041, HU-042 |
| CA-14 Auditoría con actor y fecha | HU-011, HU-012, HU-013, HU-015, HU-017, HU-018, HU-019, HU-021, HU-033, HU-036 |
| CA-15 Formulario común y fase separada | HU-013, HU-043 a HU-047 |

Un CA se marca en [[criterios-de-aceptacion]] solo con evidencia: la prueba o el PR que lo demuestra.

## Decisiones pendientes que condicionan el backlog

Las HU no resuelven estas decisiones; las registran como dependencias. Detalle en [[pendientes]].

| Decisión | HU afectadas |
|---|---|
| Aceptar [[adr-0004-autenticacion-cookie-mismo-origen]] | HU-003 a HU-006 |
| CI en GitHub Actions | HU-001 |
| Enrutador y librería de estado del frontend | HU-002 |
| Roles frente a equipos (`Development`/`Production` frente a `Team.*`) | HU-004, HU-009 |
| Catálogo de categorías (V-12) y formato del número de ticket (V-16) | HU-013 |
| Máximo de archivos por solicitud; MIME y firma (V-06) | HU-014, HU-029, HU-033 |
| Quién ajusta la prioridad (V-03) | HU-015 |
| "Ausencia" de Elizabeth (V-05) | HU-017 |
| Responsable frente a participante (V-10) | HU-018, HU-019 |
| Transiciones de estado (V-01, V-02) | HU-021, HU-033, HU-036 |
| Quién registra la URL del PR (V-04) | HU-022 |
| Row-Level Security | HU-024, HU-025 |
| Formato del cursor del chat | HU-026, HU-031 |
| Edición o borrado de mensajes (V-14) | HU-028 |
| Destinatarios de correo (V-07, V-08) | HU-035, HU-038 |
| Varias respuestas formales (V-13) | HU-033 |
| "Área" y "primera respuesta"; horario hábil (V-09) | HU-039, HU-040 |
| Almacenamiento de respuestas variables | HU-046, HU-047 |

## Decisiones tomadas al crear el backlog

- [[adr-0008-portal-cliente-en-fase-1]]: la consulta básica del portal del cliente se adelanta a la Fase 1.

## Relacionado

- [[fases-y-alcance]] · [[criterios-de-aceptacion]] · [[pendientes]] · [[estrategia-de-pruebas]] · [[flujo-de-trabajo-github]] · [[trabajar-con-el-agente]]
