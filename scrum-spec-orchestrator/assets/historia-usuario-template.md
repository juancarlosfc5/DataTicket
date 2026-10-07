---
title: "HU-{{NNN}} — {{TITULO}}"
type: historia-de-usuario
status: propuesta
estado: Borrador
tags: [scrum, scrum/historia, {{AREA}}]
sources: ["PRD.md §{{N}}"]
aliases: ["HU-{{NNN}}", "{{TITULO}}"]
epica: "[[ep-{{NNN}}-{{slug-epica}}]]"
criterios_prd: []
componentes: []
dificultad: "{{Bajo|Medio|Alto|Muy alto}}"
sprint_sugerido: "Sprint {{N}}"
dependencias: []
relacionadas: []
created: {{YYYY-MM-DD}}
updated: {{YYYY-MM-DD}}
---

# HU-{{NNN}} — {{TITULO}}

{{RESUMEN_DE_1_A_3_LINEAS}}

## Historia de usuario

**COMO** {{PERFIL}}  
**QUIERO** {{CAPACIDAD}}  
**PARA** {{VALOR}}

## Contexto

{{DESCRIPCION}}

## Alcance

- {{ITEM}}

## Fuera de alcance

- {{ITEM_O_NO_APLICA}}

## Requisitos y reglas de negocio

- {{REQUISITO_O_REGLA}} (PRD §{{N}})

## Invariantes en juego

- {{INVARIANTE_DE_AGENTS_7_O_NINGUNO}}

## Criterios del PRD cubiertos

- PRD CA-{{xx}} ({{total|parcial}}) → [[criterios-de-aceptacion]]

## Dependencias y relaciones

- Épica: [[ep-{{NNN}}-{{slug-epica}}]]
- Dependencias: {{WIKILINKS_O_NINGUNA}}
- Relacionadas: {{WIKILINKS_O_NINGUNA}}
- Decisiones: {{ADR_O_PENDIENTE}}

## Componentes afectados

- {{Backend (Domain|Application|Infrastructure|Api)|Hub SignalR|Frontend (models|controllers|views)|Persistencia PostgreSQL|Archivos (Blob)|Identity|Docker/Compose}}

## Dificultad

**Nivel:** {{Bajo|Medio|Alto|Muy alto}}

**Justificación:** {{JUSTIFICACION_SIN_TIEMPO}}

## Contrato backend ↔ frontend

{{RUTAS_REST_DTOS_EVENTOS_DEL_HUB_Y_CODIGOS_DE_ERROR_O_NO_APLICA}}

## Tareas de desarrollo

- [ ] **T-01 — Contrato** · Capa: Transversal · Dificultad: {{NIVEL}}  
  Descripción: {{FIJAR_RUTAS_DTOS_EVENTOS_ERRORES}}
- [ ] **T-02 — {{TAREA_BACKEND}}** · Capa: Backend · Dificultad: {{NIVEL}}  
  Descripción: {{PRUEBA_PRIMERO_Y_LUEGO_IMPLEMENTACION}}
- [ ] **T-03 — {{TAREA_FRONTEND}}** · Capa: Frontend · Dificultad: {{NIVEL}}  
  Descripción: {{DESCRIPCION}}

## Criterios de aceptación

### CHU-01 — {{NOMBRE}}

**Dado** {{PRECONDICION}}  
**Cuando** {{ACCION}}  
**Entonces** {{RESULTADO_OBSERVABLE}}

## Definition of Done

- [ ] Todos los `CHU` obligatorios están validados con evidencia.
- [ ] {{CONDICION_DOD_APLICABLE}}
- [ ] La trazabilidad de la HU y su épica está actualizada.

## Evidencia de validación

| Elemento | Resultado | Evidencia | Observación |
|---|---|---|---|
| CHU-01 | Pendiente | — | — |
| DoD-01 | Pendiente | — | — |

## Historial

- {{YYYY-MM-DD}} — HU creada en estado `Borrador` tras aprobación para persistirla.

## Notas y decisiones

- {{NOTA_O_NINGUNA}}

## Relacionado

- [[ep-{{NNN}}-{{slug-epica}}]] · [[tablero-scrum]] · {{PAGINAS_DE_PRODUCTO_O_ARQUITECTURA}}
