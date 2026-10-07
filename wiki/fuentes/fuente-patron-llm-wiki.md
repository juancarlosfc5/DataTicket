---
title: "Fuente: patrón LLM Wiki"
type: fuente
status: vigente
tags: [fuente, wiki, metodologia]
sources: ["Documento de idea 'LLM Wiki' compartido por la persona líder (2026-10-07)"]
aliases: [LLM Wiki, Metodología de la wiki]
created: 2026-10-07
updated: 2026-10-07
source_date: 2026-10-07
source_author: Compartido por la persona líder del proyecto
---

# Fuente: patrón LLM Wiki

Documento de idea que define cómo se construye y mantiene esta wiki. En lugar de que el LLM redescubra el conocimiento en cada pregunta (RAG), **el LLM mantiene una wiki persistente y enlazada** que acumula lo aprendido: cada fuente o pregunta nueva enriquece páginas existentes, actualiza síntesis y marca contradicciones. Las personas curan fuentes y hacen preguntas; el LLM hace el trabajo de mantenimiento.

## Puntos clave

- **Tres capas**: fuentes crudas inmutables, la wiki (escrita por el LLM) y el esquema (instrucciones de cómo mantenerla).
- **Operaciones**: ingesta (una fuente puede tocar 10–15 páginas), consulta (las buenas respuestas se archivan como páginas nuevas) y lint (contradicciones, huérfanas, enlaces faltantes, vacíos).
- **Dos archivos especiales**: `index.md` (catálogo por categorías, se lee primero) y `log.md` (bitácora cronológica append-only con encabezados parseables `## [fecha] tipo | título`).
- Obsidian como visor (grafo, plantillas, Dataview sobre frontmatter); la wiki es un repositorio git de markdown.
- El documento es deliberadamente abstracto: la estructura concreta se acuerda con el equipo.

## Cómo se instanció en DataTicket

| Concepto del patrón | Implementación aquí |
|---|---|
| Fuentes crudas | `PRD.md` (raíz) y `wiki/raw/` (+ `raw/assets/`) |
| Wiki | `wiki/` como bóveda Obsidian, carpetas por tipo de conocimiento |
| Esquema | `AGENTS.md` (cargado por `CLAUDE.md`) |
| Mantenedor | Agente `orchestrator` en cada interacción; `wiki-keeper` para ingestas y lint |
| Garantía de mantenimiento | Hook `Stop` `wiki-guard.mjs` + plantilla de PR con "wiki actualizada" |
| Colaboración | Git + `merge=union` en `log.md` |
| Primera consulta archivada | [[analisis-prd-vs-docker-compose]] |

Detalle de uso en [[trabajar-con-el-agente]].

## Relacionado

- [[trabajar-con-el-agente]] · [[fuente-prd-v0-1]] · [[index]]
