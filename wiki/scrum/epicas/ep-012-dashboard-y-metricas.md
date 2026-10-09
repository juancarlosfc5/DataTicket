---
title: "EP-012 — Dashboard y métricas"
type: epica
status: vigente
estado: Aprobada
tags: [scrum, scrum/epica, producto/dashboard, producto/metricas]
sources: ["PRD.md §4", "PRD.md §5", "PRD.md §10", "PRD.md §13", "PRD.md §14", "PRD.md §16"]
aliases: ["EP-012", "Dashboard y métricas"]
fase_prd: "3"
criterios_prd: [CA-13, CA-02, CA-10]
historias: ["[[hu-039-calculo-de-tiempos-habiles]]", "[[hu-040-panel-global-de-la-pm]]", "[[hu-041-panel-del-equipo-interno]]", "[[hu-042-resumen-de-estados-en-portal]]"]
dependencias: ["[[ep-006-triage-asignacion-y-participantes]]", "[[ep-007-trabajo-interno-y-estados]]", "[[ep-008-bandejas-y-portal-del-cliente]]", "[[ep-010-respuesta-formal-y-cierre]]"]
created: 2026-10-07
updated: 2026-10-09
---

# EP-012 — Dashboard y métricas

Medición operativa del MVP: cálculo de tiempos en días hábiles (`America/Bogota`, lunes a viernes, festivos incluidos), panel global de la PM, panel del equipo interno y resumen de estados en el portal del cliente, sin semáforos SLA ni metas (PRD §4, §10).

## Objetivo

Dar a la PM y a los equipos una vista de carga, antigüedad, estado y tiempos de los tickets (PRD §4 objetivo 6), diferenciando datos globales de PM, datos del equipo interno y datos visibles para una empresa cliente (PRD §10).

## Valor esperado

- Línea base operativa para fijar metas más adelante (PRD §4).
- La PM detecta tickets sin asignar y carga desbalanceada sin revisar ticket por ticket (PRD §4, §10).
- El cliente ve un resumen de sus tickets por estado resumido sin recibir promesas de SLA (PRD §14.13).

## Fase del PRD

Fase 3 — Operación medible (P1) (PRD §13).

## Actores

- Elizabeth — PM (rol `ProductManager`): panel global.
- Integrantes de `Team.Development` y `Team.Production`: panel del equipo.
- Solicitante (`Requester`) y coordinador (`CompanyCoordinator`): resumen del portal.
- Administrador (`Administrator`): acceso al panel global sin definir (propuesta: sin acceso, ver [[hu-040-panel-global-de-la-pm|HU-040]]).

## Alcance

- Cálculo de tiempos en días hábiles y métricas de antigüedad, tiempo hasta respuesta formal y tiempo hasta cierre ([[hu-039-calculo-de-tiempos-habiles|HU-039]]).
- Panel global de la PM: recibidos por periodo con filtros, sin asignar, antigüedad, carga por responsable, tiempos ([[hu-040-panel-global-de-la-pm|HU-040]]).
- Panel del equipo interno limitado a sus tickets ([[hu-041-panel-del-equipo-interno|HU-041]]).
- Resumen por `ClientStatus` en el portal ([[hu-042-resumen-de-estados-en-portal|HU-042]]).

## Fuera de alcance

- SLA, semáforos, metas numéricas, automatizaciones de escalamiento y recordatorios (PRD §4, §10, §13).
- Listados exportables y vistas guardadas (PRD §10, §16.4).
- Medición de "primera respuesta" hasta que se defina (contradicción en [[pendientes]] §1).
- Filtro por "área" hasta que el concepto exista en el modelo (hallazgo 3).
- Calendario de festivos: no se excluyen, así que no hace falta (PRD §4).

## Requisitos y reglas de negocio

- Métricas del MVP: tickets recibidos (con filtros por estado, empresa y área), sin asignar, antigüedad, carga por responsable, tiempo hasta respuesta formal y tiempo hasta cierre (PRD §4).
- Cómputo en `America/Bogota`, de lunes a viernes, excluyendo sábados y domingos y **contando los festivos** (PRD §4).
- Sin SLA ni metas numéricas; el cliente no recibe metas no acordadas (PRD §4, §10, §14.13).
- El panel diferencia datos globales de PM, datos del equipo interno y tickets visibles a una empresa cliente (PRD §10).
- El detalle/chat requiere asociación además de la pertenencia al equipo (PRD §10): los paneles no son una vía para leer contenido de tickets no asociados.

## Criterios del PRD cubiertos

- PRD CA-13 — (total, salvo el filtro por "área" bloqueado por el hallazgo 3) dashboard interno con cola, estado, asignaciones, antigüedad, carga y tiempos, sin metas SLA para el cliente → [[criterios-de-aceptacion]]
- PRD CA-02 — (parcial) el resumen del portal respeta el alcance solicitante/coordinador → [[criterios-de-aceptacion]]
- PRD CA-10 — (parcial) el resumen del portal no expone estados técnicos → [[criterios-de-aceptacion]]

## Dependencias

- [[ep-006-triage-asignacion-y-participantes]] — `Assignment` y `TicketParticipant` para "sin asignar" y "carga por responsable".
- [[ep-007-trabajo-interno-y-estados]] — `TicketStatus` y sus transiciones.
- [[ep-008-bandejas-y-portal-del-cliente]] — alcance del portal (`IClientTicketQueries`) reutilizado por HU-042.
- [[ep-010-respuesta-formal-y-cierre]] — fecha de envío de `FormalResponse` y fecha de cierre (hallazgo 5: `Ticket` aún no tiene campos de cierre).
- [[hu-010-datos-sinteticos-de-desarrollo|HU-010]] — volumen sintético (~50 empresas, PRD §16.5) para probar consultas agregadas.

## Historias de usuario

- [[hu-039-calculo-de-tiempos-habiles|HU-039 — Cálculo de tiempos en días hábiles]] (Sprint 8)
- [[hu-040-panel-global-de-la-pm|HU-040 — Panel global de la PM]] (Sprint 8)
- [[hu-041-panel-del-equipo-interno|HU-041 — Panel del equipo interno]] (Sprint 8)
- [[hu-042-resumen-de-estados-en-portal|HU-042 — Resumen de estados en el portal]] (Sprint 8)

## Criterio de completitud

- [ ] HU-039 a HU-042 están `Completada` con su matriz de evidencia.
- [ ] Pruebas unitarias con fechas concretas demuestran que un festivo colombiano entre semana cuenta como hábil y que sábados y domingos no cuentan.
- [ ] Ninguna respuesta de los paneles ni del resumen del portal contiene campos de SLA, metas o semáforos (inspección de JSON).
- [ ] Pruebas negativas: el panel del equipo no cuenta tickets ajenos; el resumen del portal no cuenta tickets de otra empresa ni de otro solicitante.
- [ ] Hallazgo 2 (alcance del "MVP" frente a CA-13), hallazgo 3 ("área") y la métrica "primera respuesta" están resueltos o aceptados como riesgo por la persona responsable del producto.

## Riesgos e incógnitas

- **Hallazgo 2:** CA-13 forma parte de los criterios del MVP, pero el dashboard está en la Fase 3; no se define si "MVP" = fases 0–2 o 0–3. Si el MVP termina en la Fase 2, CA-13 quedaría sin cumplir al cierre del MVP.
- **Hallazgo 3:** "área" aparece en las métricas (PRD §4, §10, §14.13) pero no existe en el modelo de dominio; ¿es el equipo, la categoría u otro catálogo?
- **"Primera respuesta"** (PRD §13) no está definida en §4 ([[pendientes]] §1).
- **V-09:** horario hábil sin definir; se propone contar 24 h de cada día hábil.
- **V-10:** "responsable operativo" frente a participante, necesario para "sin asignar" y "carga por responsable".
- **Zona horaria en contenedores:** `America/Bogota` exige datos tz (tzdata/ICU) en las imágenes `sdk` y `aspnet` de `backend/Dockerfile`; hay que comprobarlo con `docker build --target test ./backend`.
- **Rendimiento:** agregaciones sobre ~50 empresas sintéticas; conviene calcularlas en la consulta SQL y no en memoria.

## Relacionado

- [[tablero-scrum]] · [[fases-y-alcance]] · [[dashboard-y-metricas]] · [[estados-del-ticket]] · [[roles-y-permisos]]
- [[adr-0008-portal-cliente-en-fase-1]] · [[persistencia-postgresql]] · [[criterios-de-aceptacion]] · [[pendientes]]
- [[ep-008-bandejas-y-portal-del-cliente]] · [[ep-010-respuesta-formal-y-cierre]]
