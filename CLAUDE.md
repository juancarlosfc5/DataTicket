# DataTicket — Claude Code

@AGENTS.md

## Específico de Claude Code

- **Hilo principal = `orchestrator`.** `.claude/settings.json` fija `"agent": "orchestrator"`, así que toda sesión abierta en este repositorio corre con `.claude/agents/orchestrator.md`. Para una sesión puntual con otro agente: `claude --agent <nombre>`.
- **Subagentes del proyecto** en `.claude/agents/`: `backend-engineer`, `frontend-engineer`, `quality-reviewer`, `wiki-keeper`. Están versionados para que todo el equipo tenga los mismos.
- **Hook `Stop`** (`.claude/hooks/wiki-guard.mjs`): si quedan cambios fuera de `wiki/` posteriores a la última entrada de `wiki/log.md`, devuelve el turno una vez para cerrar la wiki. Requiere Node 24 en el PATH y, en Windows, Git for Windows (Claude Code ejecuta los hooks con Git Bash; el comando usa `$CLAUDE_PROJECT_DIR`).
- **Configuración personal** (no versionada): `.claude/settings.local.json` y `CLAUDE.local.md`.
