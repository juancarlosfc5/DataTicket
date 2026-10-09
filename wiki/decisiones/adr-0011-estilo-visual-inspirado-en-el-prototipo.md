---
title: "ADR-0011: Estilo visual inspirado en DataTicket.html; el orquestador opera el frontend"
type: decision
status: aceptada
tags: [decision, arquitectura/frontend, diseno]
sources: ["DataTicket.html", "Instrucción de la persona líder del proyecto (2026-10-09)"]
aliases: [ADR-0011, Estilo visual, Inspiración visual]
created: 2026-10-09
updated: 2026-10-09
decision_date: 2026-10-09
deciders: [juancarlosfc5 (persona líder del proyecto)]
---

# ADR-0011: Estilo visual inspirado en `DataTicket.html`; el orquestador opera el frontend

Todo el frontend sigue el **estilo** del prototipo `DataTicket.html` (estética, paleta, tipografía y densidad) combinado con el **estilo Apple** (movimiento físico, profundidad, tipografía cuidada). El prototipo es **solo referencia visual**: no se copian sus pantallas, flujos ni reglas. **El PRD y las HU de Scrum rigen** qué se construye y cómo se comporta. El orquestador (con `frontend-engineer`) toma y valida las decisiones visuales sin pedir supervisión a la persona líder.

## Contexto

- Faltaba la dirección visual: el agente frontend trabajaba con un «diseño provisional» a la espera del ZIP de diseño (PRD §2, [[pendientes]] §6).
- El prototipo trae un sistema coherente con la marca Data Global (azul `#004098`, verde `#7DBF00`), tema claro y oscuro, y componentes pensados para el dominio (prioridad, estado, empresa).
- Sus pantallas responden a reglas que contradicen el PRD (respuestas públicas en el hilo, estados distintos), así que no pueden copiarse tal cual ([[fuente-prototipo-dataticket-html]]).

## Opciones consideradas

1. **Copiar las pantallas del mockup**: rápido, pero arrastra reglas contrarias al PRD y datos que el backend no tiene.
2. **Adoptar su estilo como sistema de diseño y diseñar cada vista desde el contrato real**: coherente con la marca y con el dominio.
3. **Seguir con el diseño provisional hasta el ZIP**: retrasa la calidad visual y obliga a rehacer.

## Decisión

Opción 2:

- Los tokens, la tipografía, la anatomía de componentes y el movimiento quedan en [[sistema-de-diseno]], que es la referencia obligatoria. Su origen es `DataTicket.html`, fuente cruda inmutable.
- Cada pantalla se construye a partir del contrato API/SignalR y de la base de datos ([[adr-0009-db-sql-diseno-de-referencia]]); si el mockup muestra algo que el backend no expone, no se inventa.
- Si el prototipo contradice el PRD, prevalece el PRD (`AGENTS.md` §3).
- Se instalan localmente las 14 skills de Emil Kowalski (`emilkowalski/skill`) para el pulido, el movimiento y el estilo Apple. `frontend-engineer` precarga `apple-design` y `emil-design-eng`.
- Ampliación del 2026-10-09: la persona líder pidió el estilo Apple y confirmó que, ante cualquier diferencia, el prototipo no se tiene en cuenta.
- **Operación del frontend**: el orquestador decide los detalles visuales, verifica en el navegador integrado (tema claro y oscuro, 375/768/1440 px, teclado) y solo consulta a la persona líder ante decisiones de producto.

## Consecuencias

- `frontend/src/styles/tokens.css` se crea en HU-002 con los tokens del prototipo; los colores sueltos son un hallazgo de revisión.
- Si llega el ZIP de diseño, se ingiere y se contrasta con esta guía; las diferencias se resuelven con un ADR nuevo.
- Tipografía Archivo autoalojada (propuesta en [[sistema-de-diseno]]), pendiente de verificar el paquete en npm.

## Relacionado

- [[sistema-de-diseno]] · [[fuente-prototipo-dataticket-html]] · [[frontend-mvc]] · [[adr-0003-frontend-mvc]] · [[plan-goal-login-y-loop-chat]]
