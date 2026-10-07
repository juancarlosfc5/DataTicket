---
title: Matriz de prioridad
type: concepto
status: vigente
tags: [producto, producto/prioridad]
sources: ["PRD.md §6.1", "PRD.md §6.2", "PRD.md §12", "PRD.md §14"]
aliases: [Prioridad calculada, Urgencia e impacto, CalculatedPriority]
created: 2026-10-07
updated: 2026-10-07
---

# Matriz de prioridad

La prioridad de un ticket se **calcula** a partir de la urgencia y el impacto que indica el cliente al radicar. Un integrante de Data Global puede ajustarla a mano, dejando rastro auditable (PRD §6.1).

## Entradas

| Campo | Valores (negocio) | Código |
|---|---|---|
| Urgencia | baja, media, alta | `Urgency`: `Low`, `Medium`, `High` |
| Impacto | bajo, medio, alto | `Impact`: `Low`, `Medium`, `High` |

Ambos forman parte del formulario común del piloto y alimentan la prioridad calculada (PRD §6.1, §9).

## Matriz

| Impacto \ Urgencia | Baja | Media | Alta |
|---|---|---|---|
| **Bajo** | Muy baja | Baja | Media |
| **Medio** | Baja | Media | Alta |
| **Alto** | Media | Alta | Crítica |

Fuente: (PRD §6.1), tomada del prototipo y del borrador del desarrollador.

Equivalente en código (`CalculatedPriority`):

| `Impact` \ `Urgency` | `Low` | `Medium` | `High` |
|---|---|---|---|
| `Low` | `VeryLow` | `Low` | `Medium` |
| `Medium` | `Low` | `Medium` | `High` |
| `High` | `Medium` | `High` | `Critical` |

La matriz es simétrica: intercambiar urgencia e impacto da la misma prioridad. Es una función pura, apta para pruebas unitarias exhaustivas (9 combinaciones).

## Ajuste manual (`PriorityOverride`)

Si un integrante de Data Global cambia la prioridad, se registra obligatoriamente (PRD §6.1):

| Dato | Obligatorio |
|---|---|
| Prioridad calculada original | Sí |
| Nueva prioridad | Sí |
| Quién hizo el cambio | Sí |
| Cuándo | Sí |
| Motivo | Sí |

- El cambio de prioridad es un evento auditado (PRD §12, §14.14). Ver [[auditoria]].
- El registro conserva la prioridad calculada junto a la nueva, de modo que siempre se sabe de dónde partió el ajuste (PRD §6.1).
- La cola de triage muestra la **prioridad calculada** (PRD §5, §6.2).

> [!question] Pendiente
> - El PRD dice "un integrante de Data Global" sin acotar el rol: ¿cualquier participante interno, o solo PM y administradores?
> - No se indica si las bandejas y el dashboard ordenan por la prioridad calculada o por la ajustada.
> Ver [[pendientes]].

## Relacionado

- [[flujo-del-ticket]] · [[estados-del-ticket]] · [[formularios-configurables]]
- [[auditoria]] · [[dashboard-y-metricas]] · [[modelo-de-dominio]] · [[glosario]]
