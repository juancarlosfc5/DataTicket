# Modelo Scrum para Obsidian

La bóveda es la carpeta `wiki/`. Las notas Scrum siguen las convenciones de `AGENTS.md` §5.3.

## Rutas

```text
wiki/scrum/
├── tablero-scrum.md
├── epicas/ep-NNN-slug.md
└── historias-de-usuario/hu-NNN-slug.md
```

No crees notas durante la etapa de propuesta. Persiste únicamente después de la aprobación definida en `SKILL.md`.

## Identificadores

- épicas: `EP-001`, `EP-002`, ...;
- historias: `HU-001`, `HU-002`, ...;
- criterios propios de la HU: `CHU-01`, `CHU-02`, ...;
- criterios del PRD: `PRD CA-01`…`PRD CA-15` (no se renumeran);
- tareas: `T-01`, `T-02`, ... por HU.

Los IDs son estables y no se reutilizan. Los archivos usan `kebab-case` en minúsculas, sin tildes ni ñ y únicos en la bóveda: `ep-001-base-tecnica.md`, `hu-004-radicar-ticket.md`. El ID en mayúsculas va en el H1, en `title` y en `aliases`.

## Frontmatter

Obligatorio según `AGENTS.md` §5.3: `title`, `type`, `status`, `tags`, `sources`, `aliases`, `created`, `updated`.

- `status` usa el vocabulario de la wiki: `propuesta` mientras no esté aprobada, `vigente` una vez aprobada, `obsoleta` si se descarta.
- `estado` guarda el estado Scrum.
- `tags`: `scrum`, `scrum/epica` o `scrum/historia`, más el área (`producto/chat`, `arquitectura/backend`…).

## Enlaces

- Wikilinks sin carpeta: `[[hu-004-radicar-ticket|HU-004 — Radicar ticket]]`. Nunca enlaces markdown relativos.
- Cada épica enlaza todas sus HU; cada HU enlaza su épica.
- Las dependencias usan wikilinks y, cuando ayude al grafo, relación inversa.
- Se enlazan las páginas existentes: `[[criterios-de-aceptacion]]`, páginas de `producto/`, `arquitectura/` y ADR (`[[adr-0005-signalr-para-chat]]`).
- Cada nota termina con `## Relacionado`.

## Estados Scrum

`Borrador`, `Pendiente de aprobación`, `Aprobada`, `En desarrollo`, `En validación`, `Completada`, `Bloqueada`.

## Dificultad

`Bajo`, `Medio`, `Alto`, `Muy alto`. Nunca representa duración.

## Tablero

`wiki/scrum/tablero-scrum.md` contiene objetivo, enlaces al stack (`[[stack-y-versiones]]`) y a las fases (`[[fases-y-alcance]]`), enlaces a épicas, propuesta de sprints, cobertura de los CA del PRD y decisiones pendientes. No duplica el detalle de las HU.

## Integración con la wiki

La skill no edita `index.md`, `log.md` ni `pendientes.md`. Entrega al orquestador las notas para registrar las páginas nuevas en el índice (sección *Scrum*), la entrada del log y las contradicciones.
