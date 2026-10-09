# Plan de trabajo de DataTicket

Plan de ejecución del MVP, dividido en las fases del PRD (§13). Aquí se registra **qué está hecho y quién lo hizo**.
El detalle de cada épica e historia de usuario (HU) está en la wiki: [tablero Scrum](wiki/scrum/tablero-scrum.md), [épicas](wiki/scrum/epicas/) e [historias](wiki/scrum/historias-de-usuario/).

## Cómo usar este archivo

1. Al **empezar** una HU, escribe tu nombre en `Responsable`. Abre la rama `feature/<area>-<descripcion>` desde `main`.
2. Marca la HU con `[x]` y escribe la `Fecha` (AAAA-MM-DD) **solo cuando esté `Completada`**:
   - todos sus criterios `CHU-xx` y su Definition of Done tienen evidencia en la matriz de la HU;
   - el PR está fusionado.
3. Marca una **fase** con `[x]` cuando todas sus HU estén marcadas y se haya revisado su criterio de completitud. `Responsable` de la fase es quien verifica y cierra la fase; `Fecha` es la del cierre.
4. Actualiza también:
   - el campo `estado` de la nota de la HU;
   - la tabla de avance de este archivo;
   - la entrada en `wiki/log.md`.
5. No reordenes ni renumeres HU. Una HU nueva toma el siguiente número libre (HU-048…) y se añade a su fase.

Estados Scrum de una HU: `Pendiente de aprobación` → `Aprobada` → `En desarrollo` → `En validación` → `Completada` (o `Bloqueada`).
Al crear el backlog (2026-10-07), todas las HU quedaron en `Pendiente de aprobación`.

## Avance por fase

| Fase | Prioridad | HU | Completadas | Estado |
|---|---|---:|---:|---|
| 0 — Base técnica | Bloqueante | 10 | 0 | Pendiente |
| 1 — Piloto operativo | P0 | 15 | 0 | Pendiente |
| 2 — Colaboración y entrega | P0 | 13 | 0 | Pendiente |
| 3 — Operación medible | P1 | 4 | 0 | Pendiente |
| 4 — Formularios configurables | Última fase comprometida | 5 | 0 | Pendiente |
| **Total** | | **47** | **0** | |

---

## Fase 0 — Base técnica (Bloqueante)

Resultado esperado: monolito modular .NET 10, React/TypeScript, PostgreSQL, autenticación, empresas, usuarios y permisos (PRD §13).

- [ ] **Fase 0 completada** · Responsable: ______ · Fecha: ______

### EP-001 — Fundaciones técnicas y calidad continua · [épica](wiki/scrum/epicas/ep-001-fundaciones-tecnicas.md)

- [ ] [HU-001 — Integración continua en cada PR](wiki/scrum/historias-de-usuario/hu-001-integracion-continua.md) · Sprint 0 · Responsable: ______ · Fecha: ______
- [ ] [HU-002 — Shell de la aplicación y navegación por rol](wiki/scrum/historias-de-usuario/hu-002-shell-y-navegacion-por-rol.md) · Sprint 0 · Responsable: ______ · Fecha: ______

### EP-002 — Identidad y acceso · [épica](wiki/scrum/epicas/ep-002-identidad-y-acceso.md)

- [ ] [HU-003 — Iniciar y cerrar sesión](wiki/scrum/historias-de-usuario/hu-003-iniciar-y-cerrar-sesion.md) · Sprint 0 · Responsable: ______ · Fecha: ______
- [ ] [HU-004 — Contexto del usuario y autorización por rol](wiki/scrum/historias-de-usuario/hu-004-contexto-de-usuario-y-autorizacion.md) · Sprint 0 · Responsable: ______ · Fecha: ______
- [ ] [HU-005 — Invitar y activar cuentas](wiki/scrum/historias-de-usuario/hu-005-invitar-y-activar-cuentas.md) · Sprint 1 · Responsable: ______ · Fecha: ______
- [ ] [HU-006 — Restablecer contraseña sin enumeración](wiki/scrum/historias-de-usuario/hu-006-restablecer-contrasena.md) · Sprint 1 · Responsable: ______ · Fecha: ______

### EP-003 — Empresas, usuarios y equipos · [épica](wiki/scrum/epicas/ep-003-empresas-usuarios-y-equipos.md)

- [ ] [HU-007 — Administrar empresas cliente](wiki/scrum/historias-de-usuario/hu-007-administrar-empresas-cliente.md) · Sprint 1 · Responsable: ______ · Fecha: ______
- [ ] [HU-008 — Administrar usuarios de empresas cliente](wiki/scrum/historias-de-usuario/hu-008-administrar-usuarios-cliente.md) · Sprint 1 · Responsable: ______ · Fecha: ______
- [ ] [HU-009 — Administrar usuarios internos, equipos y roles](wiki/scrum/historias-de-usuario/hu-009-administrar-usuarios-internos-y-equipos.md) · Sprint 1 · Responsable: ______ · Fecha: ______
- [ ] [HU-010 — Datos sintéticos de desarrollo](wiki/scrum/historias-de-usuario/hu-010-datos-sinteticos-de-desarrollo.md) · Sprint 1 · Responsable: ______ · Fecha: ______

---

## Fase 1 — Piloto operativo (P0)

Resultado esperado: formulario común, radicación, adjuntos, triage, participantes, estados, auditoría, bandejas y consulta básica del portal del cliente (PRD §13; [ADR-0008](wiki/decisiones/adr-0008-portal-cliente-en-fase-1.md)).

- [ ] **Fase 1 completada** · Responsable: ______ · Fecha: ______

### EP-004 — Auditoría append-only · [épica](wiki/scrum/epicas/ep-004-auditoria-append-only.md)

- [ ] [HU-011 — Registrar eventos auditables](wiki/scrum/historias-de-usuario/hu-011-registrar-eventos-auditables.md) · Sprint 1 · Responsable: ______ · Fecha: ______
- [ ] [HU-012 — Consultar la bitácora de un ticket](wiki/scrum/historias-de-usuario/hu-012-consultar-bitacora-del-ticket.md) · Sprint 4 · Responsable: ______ · Fecha: ______

### EP-005 — Radicación de tickets · [épica](wiki/scrum/epicas/ep-005-radicacion-de-tickets.md)

- [ ] [HU-013 — Radicar ticket con formulario común y prioridad calculada](wiki/scrum/historias-de-usuario/hu-013-radicar-ticket.md) · Sprint 2 · Responsable: ______ · Fecha: ______
- [ ] [HU-014 — Adjuntar archivos al radicar](wiki/scrum/historias-de-usuario/hu-014-adjuntos-en-radicacion.md) · Sprint 2 · Responsable: ______ · Fecha: ______
- [ ] [HU-015 — Ajustar manualmente la prioridad](wiki/scrum/historias-de-usuario/hu-015-ajustar-prioridad-manualmente.md) · Sprint 3 · Responsable: ______ · Fecha: ______

### EP-006 — Triage, asignación y participantes · [épica](wiki/scrum/epicas/ep-006-triage-asignacion-y-participantes.md)

- [ ] [HU-016 — Cola global de triage de la PM](wiki/scrum/historias-de-usuario/hu-016-cola-global-de-triage.md) · Sprint 2 · Responsable: ______ · Fecha: ______
- [ ] [HU-017 — Cola de cobertura y tomar ticket (administrador)](wiki/scrum/historias-de-usuario/hu-017-cola-de-cobertura-y-tomar-ticket.md) · Sprint 3 · Responsable: ______ · Fecha: ______
- [ ] [HU-018 — Asignar y agregar participantes](wiki/scrum/historias-de-usuario/hu-018-asignar-y-agregar-participantes.md) · Sprint 3 · Responsable: ______ · Fecha: ______
- [ ] [HU-019 — Reasignar y retirar participantes](wiki/scrum/historias-de-usuario/hu-019-reasignar-y-retirar-participantes.md) · Sprint 3 · Responsable: ______ · Fecha: ______

### EP-007 — Trabajo interno y estados · [épica](wiki/scrum/epicas/ep-007-trabajo-interno-y-estados.md)

- [ ] [HU-020 — Detalle interno del ticket](wiki/scrum/historias-de-usuario/hu-020-detalle-interno-del-ticket.md) · Sprint 3 · Responsable: ______ · Fecha: ______
- [ ] [HU-021 — Cambiar estado interno](wiki/scrum/historias-de-usuario/hu-021-cambiar-estado-interno.md) · Sprint 4 · Responsable: ______ · Fecha: ______
- [ ] [HU-022 — Registrar URL manual de PR](wiki/scrum/historias-de-usuario/hu-022-registrar-url-de-pr.md) · Sprint 4 · Responsable: ______ · Fecha: ______

### EP-008 — Bandejas internas y portal del cliente · [épica](wiki/scrum/epicas/ep-008-bandejas-y-portal-del-cliente.md)

- [ ] [HU-023 — Bandeja "mis tickets" y de equipo](wiki/scrum/historias-de-usuario/hu-023-bandeja-de-mis-tickets-y-equipo.md) · Sprint 4 · Responsable: ______ · Fecha: ______
- [ ] [HU-024 — Portal: el solicitante consulta sus tickets](wiki/scrum/historias-de-usuario/hu-024-portal-solicitante-consulta-tickets.md) · Sprint 4 · Responsable: ______ · Fecha: ______
- [ ] [HU-025 — Portal: el coordinador consulta los tickets de su empresa](wiki/scrum/historias-de-usuario/hu-025-portal-coordinador-consulta-tickets.md) · Sprint 4 · Responsable: ______ · Fecha: ______

---

## Fase 2 — Colaboración y entrega (P0)

Resultado esperado: chat SignalR persistente con adjuntos, lecturas y reconexión; respuesta formal; cierre manual; notificaciones acordadas (PRD §13).

- [ ] **Fase 2 completada** · Responsable: ______ · Fecha: ______

### EP-009 — Chat interno en tiempo real · [épica](wiki/scrum/epicas/ep-009-chat-interno-en-tiempo-real.md)

- [ ] [HU-026 — Historial del chat paginado por cursor](wiki/scrum/historias-de-usuario/hu-026-historial-del-chat-por-cursor.md) · Sprint 5 · Responsable: ______ · Fecha: ______
- [ ] [HU-027 — Conectarse y unirse al chat del ticket](wiki/scrum/historias-de-usuario/hu-027-unirse-al-chat-del-ticket.md) · Sprint 5 · Responsable: ______ · Fecha: ______
- [ ] [HU-028 — Enviar y recibir mensajes en tiempo real](wiki/scrum/historias-de-usuario/hu-028-enviar-y-recibir-mensajes.md) · Sprint 5 · Responsable: ______ · Fecha: ______
- [ ] [HU-029 — Adjuntos e imágenes en el chat](wiki/scrum/historias-de-usuario/hu-029-adjuntos-e-imagenes-en-chat.md) · Sprint 6 · Responsable: ______ · Fecha: ______
- [ ] [HU-030 — Confirmaciones de lectura](wiki/scrum/historias-de-usuario/hu-030-confirmaciones-de-lectura.md) · Sprint 6 · Responsable: ______ · Fecha: ______
- [ ] [HU-031 — Recuperar mensajes tras reconexión](wiki/scrum/historias-de-usuario/hu-031-recuperar-mensajes-tras-reconexion.md) · Sprint 6 · Responsable: ______ · Fecha: ______
- [ ] [HU-032 — Revocar acceso al retirar un participante](wiki/scrum/historias-de-usuario/hu-032-revocar-acceso-al-retirar-participante.md) · Sprint 5 · Responsable: ______ · Fecha: ______

### EP-010 — Respuesta formal y cierre manual · [épica](wiki/scrum/epicas/ep-010-respuesta-formal-y-cierre.md)

- [ ] [HU-033 — Emitir respuesta formal](wiki/scrum/historias-de-usuario/hu-033-emitir-respuesta-formal.md) · Sprint 7 · Responsable: ______ · Fecha: ______
- [ ] [HU-034 — El cliente consulta la respuesta formal en el portal](wiki/scrum/historias-de-usuario/hu-034-portal-respuesta-formal.md) · Sprint 7 · Responsable: ______ · Fecha: ______
- [ ] [HU-035 — Enviar la respuesta formal por correo](wiki/scrum/historias-de-usuario/hu-035-correo-de-respuesta-formal.md) · Sprint 7 · Responsable: ______ · Fecha: ______
- [ ] [HU-036 — Cerrar ticket manualmente](wiki/scrum/historias-de-usuario/hu-036-cerrar-ticket-manualmente.md) · Sprint 7 · Responsable: ______ · Fecha: ______

### EP-011 — Notificaciones acordadas · [épica](wiki/scrum/epicas/ep-011-notificaciones.md)

- [ ] [HU-037 — Notificaciones en la app por mensajes nuevos](wiki/scrum/historias-de-usuario/hu-037-notificaciones-en-la-app.md) · Sprint 6 · Responsable: ______ · Fecha: ______
- [ ] [HU-038 — Correo de vinculación en la primera asociación](wiki/scrum/historias-de-usuario/hu-038-correo-de-vinculacion.md) · Sprint 6 · Responsable: ______ · Fecha: ______

---

## Fase 3 — Operación medible (P1)

Resultado esperado: dashboard interno y resumen de estados en el portal del cliente; medición de tiempos hasta la respuesta formal y hasta el cierre, sin SLA (PRD §4, §13).

- [ ] **Fase 3 completada** · Responsable: ______ · Fecha: ______

### EP-012 — Dashboard y métricas · [épica](wiki/scrum/epicas/ep-012-dashboard-y-metricas.md)

- [ ] [HU-039 — Cálculo de tiempos en días hábiles](wiki/scrum/historias-de-usuario/hu-039-calculo-de-tiempos-habiles.md) · Sprint 8 · Responsable: ______ · Fecha: ______
- [ ] [HU-040 — Panel global de la PM](wiki/scrum/historias-de-usuario/hu-040-panel-global-de-la-pm.md) · Sprint 8 · Responsable: ______ · Fecha: ______
- [ ] [HU-041 — Panel del equipo interno](wiki/scrum/historias-de-usuario/hu-041-panel-del-equipo-interno.md) · Sprint 8 · Responsable: ______ · Fecha: ______
- [ ] [HU-042 — Resumen de estados en el portal del cliente](wiki/scrum/historias-de-usuario/hu-042-resumen-de-estados-en-portal.md) · Sprint 8 · Responsable: ______ · Fecha: ______

---

## Fase 4 — Formularios configurables (última fase comprometida)

Resultado esperado: constructor y plantillas de formulario por cliente, versionadas, sin bloquear el piloto (PRD §9, §13).

- [ ] **Fase 4 completada** · Responsable: ______ · Fecha: ______

### EP-013 — Formularios configurables por cliente · [épica](wiki/scrum/epicas/ep-013-formularios-configurables.md)

- [ ] [HU-043 — Constructor de plantillas](wiki/scrum/historias-de-usuario/hu-043-constructor-de-plantillas.md) · Sprint 9 · Responsable: ______ · Fecha: ______
- [ ] [HU-044 — Asociar plantilla a empresa y ordenar campos](wiki/scrum/historias-de-usuario/hu-044-asociar-plantilla-a-empresa.md) · Sprint 9 · Responsable: ______ · Fecha: ______
- [ ] [HU-045 — Versionar y publicar plantillas](wiki/scrum/historias-de-usuario/hu-045-versionar-plantillas.md) · Sprint 9 · Responsable: ______ · Fecha: ______
- [ ] [HU-046 — Radicar con el formulario de la empresa](wiki/scrum/historias-de-usuario/hu-046-radicar-con-formulario-de-empresa.md) · Sprint 9 · Responsable: ______ · Fecha: ______
- [ ] [HU-047 — Consultar tickets por campos variables](wiki/scrum/historias-de-usuario/hu-047-consultar-por-campos-variables.md) · Sprint 9 · Responsable: ______ · Fecha: ______

---

## Fuera del plan (PRD §13 y §16)

**No se planifica** (fuera del MVP):

- MFA, SSO y federación de identidad.
- Integración con GitHub o GitLab.
- Correo entrante.
- SLA, metas y recordatorios.
- Chat visible para el cliente.
- Exportables y vistas guardadas.
- Redis o backplane.
- App móvil nativa.

**Antes de operar con clientes reales** hay que resolver las decisiones del PRD §16:

- Infraestructura.
- Proveedor de correo.
- Retención y respaldos.
- Aprobación del riesgo de URL pública en Blob.

Su seguimiento está en [wiki/pendientes.md](wiki/pendientes.md).
