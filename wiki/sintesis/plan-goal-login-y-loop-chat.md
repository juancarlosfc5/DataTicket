---
title: Plan de ejecución — goal del login y loop del chat
type: sintesis
status: vigente
tags: [sintesis, proceso, scrum, identity, signalr]
sources: ["PLAN-DE-TRABAJO.md", "wiki/scrum/tablero-scrum.md", "Decisiones D1–D6 de la persona líder del proyecto (2026-10-09)", "https://code.claude.com/docs/en/goal", "https://code.claude.com/docs/en/scheduled-tasks"]
aliases: [Plan goal y loop, goal_login, loop_chat]
created: 2026-10-09
updated: 2026-10-09
---

# Plan de ejecución — goal del login y loop del chat

Cómo se construyen con Claude Code el Sprint 0 de autenticación (HU-002 a HU-004) y el chat SignalR (EP-009). El primero va con un `/goal` y el segundo con un `/loop`. Cada uno tiene un **archivo de control en la raíz** (`goal_login.md`, `loop_chat.md`) con una lista de chequeo que Claude marca solo tras verificar y que el equipo consulta para saber qué se ejecutó.

## Diagnóstico de partida (2026-10-09)

- **Código:** solo hay esqueleto. Backend con salud, OpenAPI, `DataTicketDbContext` vacío y `ArchitectureTests`; frontend con el módulo `system` y `httpClient` solo GET. No hay CI (HU-001), Identity, pruebas de integración ni SignalR.
- **Backlog:** los commits de Juan David (`d6268f0`, `a81aad8`, `ce06599`) completaron las 47 HU y el tablero. El mismo día se aprobaron las 13 épicas y las 47 HU y se reescribieron `backend-engineer` y `frontend-engineer`. `db.sql` quedó aprobado como diseño de referencia ([[adr-0009-db-sql-diseno-de-referencia]]).
- **Dependencia crítica:** EP-009 (Fase 2) necesita `TicketParticipant` de HU-018/019 (Fase 1). Por eso el loop empieza con una rebanada habilitante (R0).

## Decisiones (persona líder del proyecto, 2026-10-09)

| # | Decisión | Resultado |
|---|---|---|
| D1 | Mecanismo de sesión | [[adr-0004-autenticacion-cookie-mismo-origen]] **aceptado** |
| D2 | Enrutador del frontend | [[adr-0010-enrutador-react-router]]: `react-router` 8.4.0, modo librería |
| D3 | Orden | Primero el goal del login y después el loop del chat. R0 adelanta lo mínimo de HU-018/019 |
| D4 | Aprobación del backlog | Todas las épicas y HU aprobadas (ver [[log]]) |
| D5 | Subagentes | Ya creados; ahora se complementan con [[sistema-de-diseno]] y las skills de diseño |
| D6 | Commits | Un commit local por rebanada del loop y por bloque del goal; nunca push ni merge |
| — | Estilo visual | [[adr-0011-estilo-visual-inspirado-en-el-prototipo]]: inspiración en `DataTicket.html` sin copiar pantallas; el orquestador opera el frontend |

## Mecánica

**`/goal`** (`goal_login.md`)
- Tras cada turno, un evaluador lee solo la conversación, sin abrir archivos ni ejecutar comandos. Por eso la condición exige pegar la lista de control final, la salida de las pruebas, el veredicto de `quality-reviewer` y el `git log`.
- Un solo goal por sesión. Se consulta con `/goal` y se borra con `/goal clear`.
- Límite de seguridad: 40 turnos.

**`/loop`** (`loop_chat.md`)
- Sin intervalo: Claude programa la siguiente iteración.
- El estado vive en el archivo, porque el loop no se restaura con `--resume`.
- Una rebanada por iteración (P0, R0 a R12), con verificación, commit y entrada en el log.
- Se detiene solo ante cualquiera de estos casos: termina, la misma rebanada falla 2 veces, falta una decisión o Docker no está disponible.

## Secuencia

```text
goal_login.md  B0 preparación → B1 contratos → B2 pruebas de integración → B3 HU-003 backend
               → B4 HU-004 backend → B5 HU-002 frontend → B6 entrada y sesión → B7 integración → B8 cierre
loop_chat.md   P0 → R0 dominio mínimo → R1–R3 HU-026 → R4–R5 HU-027 → R6–R7 HU-028
               → R8 HU-032 → R9 HU-031 → R10 HU-030 → R11 HU-029 → R12 cierre de EP-009
```

En paralelo, otra persona puede tomar HU-001 (CI): da evidencia en los PR y no choca con estos archivos.

## Riesgos

| Riesgo | Mitigación |
|---|---|
| R0 choca con quien desarrolle HU-018/019 | Si ya existen en `main`, R0 se marca «no aplica»; avisar en el tablero antes de tomarlas |
| Docker apagado (PostgreSQL de las pruebas) | Comprobado en B0 y P0; detiene la ejecución |
| El evaluador del goal no ve archivos | La condición exige la evidencia pegada en la conversación |
| Contexto largo en el loop | Estado en `loop_chat.md` y en las matrices de cada HU |
| HU-029 depende de HU-014 (`IFileStorage`) | R11 se marca bloqueada sin detener el resto |
| Sin CI | Verificación local obligatoria en cada bloque; HU-001 en paralelo |

## Relacionado

- [[tablero-scrum]] · [[ep-002-identidad-y-acceso]] · [[ep-009-chat-interno-en-tiempo-real]] · [[autenticacion-identity]] · [[tiempo-real-signalr]] · [[sistema-de-diseno]] · [[trabajar-con-el-agente]] · [[pendientes]]
