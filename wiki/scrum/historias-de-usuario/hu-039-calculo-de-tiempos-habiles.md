---
title: "HU-039 — Cálculo de tiempos en días hábiles"
type: historia-de-usuario
status: propuesta
estado: Pendiente de aprobación
tags: [scrum, scrum/historia, producto/metricas, arquitectura/backend, arquitectura/frontend]
sources: ["PRD.md §4", "PRD.md §6.5", "PRD.md §10", "PRD.md §13", "PRD.md §14"]
aliases: ["HU-039", "Cálculo de tiempos en días hábiles", "Días hábiles"]
epica: "[[ep-012-dashboard-y-metricas]]"
criterios_prd: [CA-13]
componentes: [Backend Domain, Backend Application, Frontend models, Docker/Compose]
dificultad: "Medio"
sprint_sugerido: "Sprint 8"
dependencias: ["[[hu-033-emitir-respuesta-formal]]", "[[hu-036-cerrar-ticket-manualmente]]"]
relacionadas: ["[[hu-040-panel-global-de-la-pm]]", "[[hu-041-panel-del-equipo-interno]]", "[[hu-042-resumen-de-estados-en-portal]]"]
created: 2026-10-07
updated: 2026-10-07
---

# HU-039 — Cálculo de tiempos en días hábiles

Regla de cómputo de tiempos del MVP: zona `America/Bogota`, de lunes a viernes, excluyendo sábados y domingos y **contando los festivos**; base de las métricas de antigüedad, tiempo hasta respuesta formal y tiempo hasta cierre (PRD §4).

## Historia de usuario

**COMO** Elizabeth (PM)  
**QUIERO** que los tiempos de los tickets se midan en tiempo hábil con una regla única y verificable  
**PARA** construir una línea base operativa fiable antes de fijar metas

## Contexto

El PRD fija la regla: "El cómputo usa zona horaria `America/Bogota`, cuenta de lunes a viernes, excluye sábados y domingos y **cuenta los festivos**" (PRD §4). Es fácil implementarlo al revés por costumbre (excluyendo festivos), por eso la regla se cubre con pruebas sobre fechas concretas, incluidos festivos colombianos entre semana. No define un horario hábil (V-09). Las fechas se guardan en UTC ([[persistencia-postgresql]]) y el "ahora" sale de `IClock`. Esta HU no expone un endpoint propio: la consumen [[hu-040-panel-global-de-la-pm|HU-040]] y [[hu-041-panel-del-equipo-interno|HU-041]].

## Alcance

- Cálculo puro del tiempo hábil transcurrido entre dos instantes UTC.
- Métricas por ticket: antigüedad, tiempo hasta respuesta formal y tiempo hasta cierre.
- Formato de presentación en el frontend.
- Verificación de los datos de zona horaria en las imágenes de contenedor del backend.

## Fuera de alcance

- Calendario de festivos (no se excluyen, PRD §4).
- Horario hábil por horas del día hasta resolver V-09 (propuesta provisional abajo).
- SLA, metas, semáforos y alertas (PRD §4, §13).
- "Primera respuesta" (no definida en PRD §4; [[pendientes]] §1).
- Agregaciones (promedios, medianas) por panel: van en HU-040/HU-041.

## Requisitos y reglas de negocio

- Zona horaria `America/Bogota`; cuentan lunes a viernes; se excluyen sábados y domingos; los festivos cuentan (PRD §4).
- Antigüedad: desde la creación hasta el momento de consulta o de cierre (PRD §4).
- Tiempo hasta respuesta formal: entre la radicación y el envío de la respuesta formal final (PRD §4).
- Tiempo hasta cierre: entre la radicación y el cierre manual (PRD §4).
- Se miden y muestran tiempos sin prometer SLA ni metas (PRD §4, §10).
- (propuesta, V-09) Se cuentan las 24 horas de cada día hábil local; el resultado se expresa en minutos hábiles enteros y se presenta en días hábiles (minutos / 1440) con un decimal.
- (propuesta) Si el fin es anterior al inicio, es un error de programación (excepción), no un valor negativo.

## Invariantes en juego

- Ninguno de AGENTS §7 directamente. Regla de producto de PRD §4 con riesgo alto de implementarse al revés.

## Criterios del PRD cubiertos

- PRD CA-13 (parcial: aporta antigüedad y tiempos que el dashboard presenta) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-012-dashboard-y-metricas]]
- Dependencias: [[hu-033-emitir-respuesta-formal|HU-033]] (fecha de envío de `FormalResponse`), [[hu-036-cerrar-ticket-manualmente|HU-036]] (fecha de cierre; hallazgo 5: `Ticket` no tiene aún campos de cierre).
- Relacionadas: [[hu-040-panel-global-de-la-pm|HU-040]], [[hu-041-panel-del-equipo-interno|HU-041]], [[hu-042-resumen-de-estados-en-portal|HU-042]] (no muestra tiempos, propuesta).
- Decisiones: V-09 (horario hábil) y V-13 (varias respuestas formales) en [[pendientes]].

## Componentes afectados

- Backend Domain: calculadora de tiempo hábil (nombre propuesto `BusinessTimeCalculator`, ratificar en T-01 y registrar en el glosario).
- Backend Application: métricas por ticket con `IClock` (nombre propuesto `TicketTimeMetrics`).
- Frontend models: formateo de duraciones.
- Docker/Compose: datos de zona horaria en las imágenes `sdk` (etapa `test`) y `aspnet` (etapa `runtime`) de `backend/Dockerfile`.

## Dificultad

**Nivel:** Medio

**Justificación:** el algoritmo es pequeño, pero mezcla conversión UTC ↔ hora local, cortes de día que no coinciden con UTC, una regla contraintuitiva (festivos cuentan) y una decisión abierta (V-09).

## Contrato backend ↔ frontend

No hay endpoint propio. Se fija el **formato de las duraciones** que usarán los DTO de HU-040 y HU-041 (propuesta, ratificar en T-01):

```json
{
  "ageBusinessMinutes": 960,
  "timeToFormalResponseBusinessMinutes": null,
  "timeToCloseBusinessMinutes": null
}
```

- Enteros en minutos hábiles; `null` cuando el hito no ha ocurrido (sin respuesta formal o sin cierre).
- El frontend presenta días hábiles con un decimal: `960 → "0,7 días hábiles"`, `2880 → "2,0 días hábiles"`.
- Errores: no aplica (sin endpoint).

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: Bajo  
  Descripción: ratificar unidad (minutos hábiles), presentación (días con un decimal), regla provisional de 24 h por día hábil (V-09) y nombres propuestos; registrar en el [[glosario]] y en [[dashboard-y-metricas]].
- [ ] **T-02 — Calculadora de tiempo hábil** · Capa: Backend Domain · Dificultad: Medio  
  Descripción: primero pruebas xUnit con los casos de los CHU-01 a CHU-06 (fechas concretas); luego la implementación que convierte a `America/Bogota` con `TimeZoneInfo` (sin desplazamiento fijo `-05:00` codificado) y suma solo los tramos de lunes a viernes.
- [ ] **T-03 — Métricas por ticket** · Capa: Backend Application · Dificultad: Bajo  
  Descripción: primero pruebas unitarias con `IClock` fijo: ticket abierto (antigüedad hasta ahora), con respuesta formal, cerrado (antigüedad hasta el cierre); luego `TicketTimeMetrics`.
- [ ] **T-04 — Zona horaria en contenedores** · Capa: Docker/Compose · Dificultad: Bajo  
  Descripción: ejecutar las pruebas en `docker build --target test ./backend` para confirmar que `America/Bogota` se resuelve en la imagen; si la imagen de `runtime` no trae datos tz, documentarlo y proponer el ajuste al orquestador (no se modifica el Dockerfile en esta HU sin acuerdo).
- [ ] **T-05 — Formateo** · Capa: Frontend models · Dificultad: Bajo  
  Descripción: `formatBusinessDuration(minutes: number | null)` en un modelo compartido del módulo de dashboard; pruebas Vitest (`null → "—"`, `0 → "0,0 días hábiles"`, `960 → "0,7 días hábiles"`, `2880 → "2,0 días hábiles"`). Controladores y vistas: no aplica en esta HU (los usan HU-040/HU-041).
- [ ] **T-06 — Migración** · Capa: Persistencia · Dificultad: Bajo  
  Descripción: no aplica: los campos de fecha de respuesta formal y cierre los crean HU-033 y HU-036.

## Criterios de aceptación

### CHU-01 — Un festivo entre semana cuenta como hábil (lunes festivo)

**Dado** inicio `2026-10-09T22:00:00Z` (viernes 9 de octubre, 17:00 en Bogotá) y fin `2026-10-12T14:00:00Z` (lunes 12 de octubre, 09:00 en Bogotá, festivo del Día de la Raza)  
**Cuando** se calcula el tiempo hábil  
**Entonces** el resultado es 960 minutos (7 h del viernes + 9 h del lunes festivo), no 420.

### CHU-02 — Un festivo en martes cuenta como hábil

**Dado** inicio `2026-12-07T17:00:00Z` (lunes 7 de diciembre, 12:00 en Bogotá) y fin `2026-12-09T17:00:00Z` (miércoles 9, 12:00), con el martes 8 de diciembre festivo (Inmaculada Concepción)  
**Cuando** se calcula  
**Entonces** el resultado es 2880 minutos (48 h), no 1440.

### CHU-03 — Sábados y domingos no cuentan

**Dado** inicio `2026-10-10T15:00:00Z` (sábado 10:00 en Bogotá) y fin `2026-10-11T23:00:00Z` (domingo 18:00)  
**Cuando** se calcula  
**Entonces** el resultado es 0; **y** del lunes `2026-10-05T05:00:00Z` (00:00 en Bogotá) al lunes `2026-10-12T05:00:00Z` el resultado es 7200 minutos (5 días × 24 h).

### CHU-04 — El corte del día es el de Bogotá, no el de UTC

**Dado** inicio `2026-10-10T03:00:00Z` y fin `2026-10-10T05:00:00Z` (sábado en UTC, pero viernes 22:00–24:00 en Bogotá)  
**Cuando** se calcula  
**Entonces** el resultado es 120 minutos, no 0.

### CHU-05 — Mismo día y límites

**Dado** inicio `2026-10-07T13:00:00Z` (miércoles 08:00 en Bogotá) y fin `2026-10-07T22:30:00Z` (17:30)  
**Cuando** se calcula  
**Entonces** el resultado es 570 minutos; **con** inicio igual al fin el resultado es 0; **y** con fin anterior al inicio se lanza un error de argumento.

### CHU-06 — Métricas por ticket con `IClock`

**Dado** `IClock.UtcNow = 2026-10-12T14:00:00Z` y un ticket creado `2026-10-09T22:00:00Z`, sin respuesta formal ni cierre  
**Cuando** se calculan sus métricas  
**Entonces** `ageBusinessMinutes = 960`, `timeToFormalResponseBusinessMinutes = null` y `timeToCloseBusinessMinutes = null`; **y** si el ticket se cerró en `2026-10-12T14:00:00Z`, la antigüedad ya no crece aunque `IClock` avance un día.

### CHU-07 — Presentación en el frontend

**Dado** duraciones de `null`, `0`, `960` y `2880` minutos  
**Cuando** se formatean  
**Entonces** se muestran como `—`, `0,0 días hábiles`, `0,7 días hábiles` y `2,0 días hábiles`, sin colores ni etiquetas de cumplimiento.

## Definition of Done

- [ ] CHU-01 a CHU-07 validados con evidencia.
- [ ] Pruebas de dominio en verde con nombres explícitos: `TiempoHabil_LunesFestivo_Cuenta`, `TiempoHabil_MartesFestivo_Cuenta`, `TiempoHabil_FinDeSemana_NoCuenta`, `TiempoHabil_CorteDeDiaEnBogota_NoEnUtc`, `TiempoHabil_FinAnteriorAlInicio_Lanza` (`cd backend && dotnet test`).
- [ ] Las mismas pruebas en verde dentro del contenedor: `docker build --target test ./backend`.
- [ ] Ningún desplazamiento horario fijo en el código (revisión de `quality-reviewer`).
- [ ] `DataTicket.ArchitectureTests` en verde.
- [ ] Vitest de `formatBusinessDuration`, `npm --prefix frontend run lint` y `npm --prefix frontend run build` en verde.
- [ ] Sin migración (justificado en T-06).
- [ ] Wiki actualizada: [[dashboard-y-metricas]] (unidad, regla provisional de V-09 y casos de prueba de referencia), [[glosario]] (nombres ratificados), [[pendientes]] (estado de V-09).
- [ ] `quality-reviewer` sin hallazgos CRÍTICO/ALTO abiertos.
- [ ] PR revisado y aprobado por otra persona del equipo.

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
| DoD-01 Pruebas de dominio nombradas | Pendiente | — | — |
| DoD-02 Pruebas en contenedor | Pendiente | — | — |
| DoD-03 Sin desplazamiento fijo | Pendiente | — | — |
| DoD-04 Pruebas de arquitectura | Pendiente | — | — |
| DoD-05 Vitest, lint y build | Pendiente | — | — |
| DoD-06 Migración (no aplica) | Pendiente | — | — |
| DoD-07 Wiki actualizada | Pendiente | — | — |
| DoD-08 `quality-reviewer` | Pendiente | — | — |
| DoD-09 PR revisado | Pendiente | — | — |

## Historial

- 2026-10-07 — HU creada en estado `Pendiente de aprobación` tras aprobación del plan de backlog por Juan David.

## Notas y decisiones

- **Dependencia V-09:** si se define un horario hábil (p. ej. 8:00–18:00), cambian los resultados de todos los CHU; la regla provisional de 24 h debe confirmarse antes de implementar.
- **Dependencia V-13:** si puede haber más de una respuesta formal, hay que decidir cuál cuenta para "tiempo hasta respuesta formal" (el PRD habla de la "final").
- **Hallazgo 5:** `Ticket` no tiene campos de cierre en el modelo propuesto; [[hu-036-cerrar-ticket-manualmente|HU-036]] debe añadirlos.
- Los festivos usados en los CHU (12 de octubre y 8 de diciembre de 2026) son festivos nacionales de Colombia; se eligieron justamente para comprobar que **no** se excluyen.

## Relacionado

- [[ep-012-dashboard-y-metricas]] · [[tablero-scrum]] · [[dashboard-y-metricas]] · [[persistencia-postgresql]]
- [[entorno-docker]] · [[backend-hexagonal]] · [[criterios-de-aceptacion]] · [[pendientes]]
