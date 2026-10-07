---
title: Dashboard y métricas
type: concepto
status: vigente
tags: [producto, producto/dashboard, producto/metricas]
sources: ["PRD.md §4", "PRD.md §10", "PRD.md §13", "PRD.md §14", "PRD.md §16"]
aliases: [Dashboard, Métricas, Bandejas, Paneles]
created: 2026-10-07
updated: 2026-10-07
---

# Dashboard y métricas

Métricas operativas que el MVP mide y muestra, la regla de cómputo de tiempos, y las bandejas y paneles de cada superficie. El MVP **mide** pero **no promete SLA** ni metas numéricas (PRD §4, §10).

## Métricas del MVP (PRD §4)

| Métrica | Definición inicial |
|---|---|
| Tickets recibidos | Tickets creados en un periodo, filtrables por estado, empresa y área |
| Tickets sin asignar | Tickets en cola de triage sin responsable operativo |
| Antigüedad | Tiempo desde la creación hasta la consulta o el cierre |
| Carga por responsable | Tickets activos asociados a cada persona |
| Tiempo hasta respuesta formal | Desde la radicación hasta el envío de la respuesta formal final |
| Tiempo hasta cierre | Desde la radicación hasta el cierre manual |

## Regla de tiempo hábil (PRD §4)

| Aspecto | Regla |
|---|---|
| Zona horaria | `America/Bogota` |
| Días que cuentan | Lunes a viernes |
| Días excluidos | Sábados y domingos |
| Festivos | **Cuentan** (no se excluyen) |

> [!info] Decisión
> Los festivos **sí cuentan**: solo se excluyen sábados y domingos (PRD §4). Es fácil implementarlo al revés por costumbre; conviene cubrir la regla con pruebas unitarias explícitas.

## Sin SLA

- No hay semáforos SLA ni metas numéricas; las metas se definirán tras obtener una línea base operativa (PRD §4, §10).
- El cliente no recibe metas de SLA no acordadas (PRD §14.13).
- Automatizaciones de SLA y recordatorios están fuera del MVP (PRD §13).

## Bandejas y paneles internos (PRD §10)

| Bandeja / panel | Quién | Contenido |
|---|---|---|
| Bandeja global (`Inbox`) | Elizabeth | Triage y seguimiento de todos los tickets |
| Cola de triage (`TriageQueue`) en cobertura | Administrador | Solo campos de cola; detalle tras asociarse. Ver [[roles-y-permisos]] |
| Bandejas por equipo | Desarrollo, Producción | Por equipo y responsabilidad; detalle y chat requieren asociación |
| Paneles | PM y equipos | Tickets por estado y área, sin asignar, antigüedad, carga por responsable, tiempos hasta respuesta formal y cierre |

El panel debe diferenciar datos **globales de PM**, datos del **equipo interno** y tickets **visibles a una empresa cliente** (PRD §10).

## Portal del cliente (PRD §10)

- Solicitante: sus propios tickets. Coordinador: los de toda su empresa.
- Cada ticket muestra estado resumido, datos de radicación y la respuesta formal final cuando exista.
- Nunca muestra chat, notas internas, adjuntos internos, participantes de Data Global ni estados de Desarrollo/PR/Producción. Ver [[estados-del-ticket]].

## Diferido

- Listados exportables y vistas guardadas: propuestos en el borrador del desarrollador, no bloqueantes; se priorizarán tras el piloto (PRD §10, §16).
- El dashboard es la Fase 3 (P1) (PRD §13). Ver [[fases-y-alcance]].

> [!question] Pendiente
> - El PRD fija días hábiles pero no un **horario** hábil (horas del día): ¿los tiempos se cuentan 24 h en días hábiles?
> - La Fase 3 habla de medir "primera respuesta" (PRD §13), métrica que §4 no define; §4 solo define "tiempo hasta respuesta formal".
> - "Responsable operativo" (métrica de sin asignar) no se distingue claramente de "participante".
> Ver [[pendientes]].

## Relacionado

- [[fases-y-alcance]] · [[estados-del-ticket]] · [[roles-y-permisos]] · [[flujo-del-ticket]]
- [[elizabeth-pm]] · [[equipo-data-global]] · [[criterios-de-aceptacion]] · [[glosario]]
