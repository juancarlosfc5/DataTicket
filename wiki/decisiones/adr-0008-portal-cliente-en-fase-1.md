---
title: "ADR-0008: Consulta básica del portal del cliente en la Fase 1"
type: decision
status: aceptada
tags: [decision, producto, producto/fases, producto/portal]
sources: ["PRD.md §10", "PRD.md §13", "PRD.md §14 (CA-01, CA-02, CA-10)"]
aliases: [ADR-0008, Portal del cliente en Fase 1]
created: 2026-10-07
updated: 2026-10-07
decision_date: 2026-10-07
deciders: [Juan David (equipo del repositorio), al aprobar el plan del backlog Scrum]
---

# ADR-0008: Consulta básica del portal del cliente en la Fase 1

El portal del cliente se entrega en la Fase 1 con su consulta básica: listar los tickets y ver el detalle con el estado resumido. Las métricas y los resúmenes del portal quedan para la Fase 3. Así se resuelve la contradicción "fase del portal" registrada en [[pendientes]].

## Contexto

El PRD §13 ubica el "portal con estados resumidos" en la Fase 3 (P1). Sin embargo:

- CA-02 exige en el MVP que el solicitante vea sus tickets y el coordinador los de su empresa.
- CA-01 y CA-10 piden probar el aislamiento y la no exposición desde el portal.
- La Fase 1 ya incluye "estados" y "bandejas" (PRD §13).

Sin una vista de consulta, el piloto no puede validar con clientes los invariantes 1 y 2 de `AGENTS.md` §7.

## Opciones consideradas

1. **Consulta básica en la Fase 1 y métricas en la Fase 3.**
   - A favor: permite validar CA-01, CA-02 y CA-10 desde el piloto, y el cliente ve que su caso fue recibido (PRD §3).
   - En contra: adelanta trabajo de frontend del portal.
2. **Todo el portal en la Fase 3, como dice literalmente el PRD §13.**
   - A favor: menos alcance en la Fase 1.
   - En contra: en el piloto el cliente solo radica, sin poder consultar, y los CA de aislamiento quedan sin evidencia hasta la Fase 3.

## Decisión

Se elige la opción 1:

- **Fase 1:** el solicitante y el coordinador listan sus tickets y ven el detalle con el estado resumido ([[hu-024-portal-solicitante-consulta-tickets]], [[hu-025-portal-coordinador-consulta-tickets]]).
- **Fase 2:** la respuesta formal aparece en el portal ([[hu-034-portal-respuesta-formal]]).
- **Fase 3:** el resumen por estado y las métricas ([[hu-042-resumen-de-estados-en-portal]]).

## Consecuencias

- Positivas:
  - Los CA de aislamiento multiempresa (CA-01, CA-02, CA-10) tienen pruebas desde la Fase 1.
  - El piloto ofrece al cliente visibilidad sobre sus tickets.
- Negativas / riesgos:
  - La Fase 1 crece.
  - Sigue abierta la ambigüedad paralela sobre el dashboard: CA-13 está en el MVP, pero el dashboard está en la Fase 3 (ver [[pendientes]]).
- Qué hay que hacer en la wiki: actualizar [[fases-y-alcance]] y [[pendientes]], y reflejar la decisión en [[tablero-scrum]].
- Qué hay que hacer en el PRD: reflejar el cambio en el §13 en su próxima versión (no se edita por ser fuente inmutable).

## Relacionado

- [[fases-y-alcance]] · [[criterios-de-aceptacion]] · [[roles-y-permisos]] · [[pendientes]] · [[tablero-scrum]] · [[ep-008-bandejas-y-portal-del-cliente]]
