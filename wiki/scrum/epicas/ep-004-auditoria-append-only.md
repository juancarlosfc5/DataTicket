---
title: "EP-004 — Auditoría append-only"
type: epica
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/epica, producto/auditoria, arquitectura/backend]
sources: ["PRD.md §12", "PRD.md §6.1", "PRD.md §6.2", "PRD.md §6.4", "PRD.md §6.5", "PRD.md §14"]
aliases: ["EP-004", "Auditoría append-only"]
fase_prd: "1"
criterios_prd: ["PRD CA-04", "PRD CA-14"]
historias: ["[[hu-011-registrar-eventos-auditables]]", "[[hu-012-consultar-bitacora-del-ticket]]"]
dependencias: ["[[ep-001-fundaciones-tecnicas]]", "[[ep-002-identidad-y-acceso]]"]
created: 2026-10-07
updated: 2026-10-07
---

# EP-004 — Auditoría append-only

Bitácora durable e inmutable desde la aplicación de quién hizo qué, cuándo y sobre qué objeto en la vida de un ticket. Es la fuente de trazabilidad del producto, por encima del correo (PRD §11, §12).

## Objetivo

Disponer de un mecanismo único (`AuditEntry` + puerto `IAuditLog`) con el que cada caso de uso auditable registre actor, fecha/hora, acción, objeto, valores anterior/nuevo y resultado en la misma transacción que la acción, y ofrecer a las personas internas autorizadas una vista cronológica de esa bitácora por ticket (PRD §12).

## Valor esperado

- Elizabeth (PM) y la dirección pueden reconstruir la historia de cada ticket: radicación, prioridad, estados, asignaciones, participantes, respuesta formal y cierre (PRD §4 objetivo 4, §12).
- Los criterios PRD CA-04 y CA-14 se pueden demostrar con pruebas automáticas, porque cada acción deja una entrada verificable.
- La participación histórica de una persona retirada permanece registrada aunque pierda el acceso (PRD §5.4, §12).

## Fase del PRD

Fase 1 — Piloto operativo (PRD §13): la auditoría forma parte explícita del resultado esperado de la fase.

## Actores

- Elizabeth — PM (`ProductManager`): consulta la bitácora y es actora de la mayoría de eventos.
- Administrador (`Administrator`): actor de eventos de cobertura, participantes, respuesta formal y cierre.
- Personas internas de Desarrollo y Producción participantes: consultan la bitácora del ticket (propuesta, ver Riesgos).
- Clientes (`Requester`, `CompanyCoordinator`): actores del evento de radicación; **nunca** consultan la bitácora (PRD §5.6).
- Responsable de trazabilidad / dirección de Data Global: beneficiario de la evidencia.

## Alcance

- Entidad `AuditEntry` y catálogo de acciones auditables (nombre propuesto `AuditAction`).
- Puerto `IAuditLog` (solo inserción) y su adaptador EF Core que comparte la transacción de la acción.
- Protección append-only a nivel de base de datos (propuesta: trigger que rechaza `UPDATE`/`DELETE`/`TRUNCATE`; revocación de privilegios cuando exista un rol de aplicación separado).
- Consulta cronológica de la bitácora de un ticket para personas internas autorizadas, con su vista en el espacio interno.

## Fuera de alcance

- Emisión de cada evento concreto: la hace la HU dueña de cada acción (radicación HU-013, prioridad HU-015, asignación y participantes HU-017/HU-018/HU-019, estado HU-021, respuesta formal HU-033, cierre HU-036). Esta épica provee el mecanismo y verifica el catálogo.
- Auditoría de intentos denegados hasta que se resuelva V-11.
- Retención, archivado, purga y respaldo de la bitácora (PRD §12, §16.3).
- Exportables o reportes de auditoría (PRD §10, §16.4).
- Bitácora global (multi-ticket) o de acciones administrativas sobre cuentas y empresas (no las enumera PRD §12; propuesta para una fase posterior).

## Requisitos y reglas de negocio

- La bitácora es append-only desde la aplicación (PRD §12).
- Campos mínimos: actor, fecha/hora, acción, objeto, valores anterior/nuevo cuando aplique y resultado (PRD §12).
- Eventos relevantes: radicación, cambio de prioridad/estado, asignación, reasignación, incorporación/retiro de participantes, respuesta formal y cierre manual (PRD §12).
- La participación histórica de una persona retirada permanece registrada (PRD §5.4, §12).
- Los movimientos internos, reasignaciones y cambios de participantes se registran en la bitácora (PRD §6.4).
- El ajuste manual de prioridad registra calculada, nueva, quién, cuándo y motivo (PRD §6.1).
- El cierre conserva quién cerró, cuándo y el estado anterior (PRD §6.5).
- El cliente nunca ve actividad interna ni nombres de colaboradores internos (PRD §5.6).

## Criterios del PRD cubiertos

- PRD CA-04 (parcial: aporta la auditoría de asignar, reasignar y agregar/quitar; la capacidad funcional está en [[ep-006-triage-asignacion-y-participantes]]) → [[criterios-de-aceptacion]]
- PRD CA-14 (parcial: aporta el mecanismo y la verificación del catálogo; se completa cuando HU-015, HU-018, HU-019, HU-021 y HU-036 emiten sus eventos) → [[criterios-de-aceptacion]]

## Dependencias

- [[ep-001-fundaciones-tecnicas]] — migraciones EF Core, CI y pruebas de integración con PostgreSQL real.
- [[ep-002-identidad-y-acceso]] — `ICurrentUser` para el actor y roles para autorizar la consulta ([[hu-004-contexto-de-usuario-y-autorizacion|HU-004 — Contexto de usuario y autorización]]).
- `IClock` disponible para la fecha/hora UTC.

## Historias de usuario

- [[hu-011-registrar-eventos-auditables|HU-011 — Registrar eventos auditables]] · Sprint 2
- [[hu-012-consultar-bitacora-del-ticket|HU-012 — Consultar la bitácora del ticket]] · Sprint 4

## Criterio de completitud

- [ ] HU-011 y HU-012 están `Completada`.
- [ ] Existe una prueba por cada acción del catálogo que verifica actor, fecha UTC, acción, objeto y valores anterior/nuevo (las acciones aún no implementadas quedan marcadas como pendientes de su HU dueña).
- [ ] Una prueba de integración demuestra que `UPDATE` y `DELETE` sobre la tabla de auditoría fallan desde la conexión de la aplicación.
- [ ] Una prueba de integración demuestra que un usuario cliente no puede consultar la bitácora (403) y que un interno no autorizado no la obtiene.
- [ ] PRD CA-04 y CA-14 tienen evidencia enlazada desde [[criterios-de-aceptacion]] cuando las HU emisoras estén completas.
- [ ] No quedan dependencias bloqueantes dentro del alcance.

## Riesgos e incógnitas

- **V-11** — significado del campo "resultado" y si se auditan intentos denegados: se registra solo el éxito hasta decidir ([[pendientes]]).
- **Quién consulta la bitácora** — el PRD no lo define; la propuesta (PM, participantes vigentes internos y administrador asociado) debe ratificarse (nueva duda para [[pendientes]]).
- **Protección en base de datos** — revocar `UPDATE`/`DELETE` exige separar el rol que ejecuta migraciones del rol de la aplicación; hoy Compose usa un único usuario `dataticket` propietario de las tablas. El trigger propuesto protege igualmente, pero la separación de roles es una decisión de infraestructura (PRD §16.1).
- **Retención** — un bloqueo absoluto de `DELETE` obligará a definir un procedimiento controlado cuando se decida la retención (PRD §12, §16.3).
- **Hallazgo 7** — faltan casos de uso para reasignar, ajustar prioridad, cambiar estado y registrar URL de PR; sus HU proponen nombres que deben entrar al catálogo de acciones.

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[auditoria]] · [[persistencia-postgresql]] · [[backend-hexagonal]]
- [[modelo-de-dominio]] · [[criterios-de-aceptacion]] · [[roles-y-permisos]] · [[pendientes]]
- [[ep-005-radicacion-de-tickets]] · [[ep-006-triage-asignacion-y-participantes]] · [[ep-007-trabajo-interno-y-estados]] · [[ep-010-respuesta-formal-y-cierre]]
