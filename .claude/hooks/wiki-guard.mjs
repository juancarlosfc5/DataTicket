#!/usr/bin/env node
// Hook "Stop" de Claude Code para DataTicket.
// Si hay archivos fuera de wiki/ modificados después de la última entrada de wiki/log.md
// (o si wiki/log.md falta), devuelve el turno UNA vez para que el agente actualice la wiki
// (AGENTS.md §4, paso 6). Falla en abierto: ante cualquier error deja terminar al agente.

import { execFileSync } from 'node:child_process'
import { existsSync, readFileSync, readdirSync, realpathSync, statSync } from 'node:fs'
import path from 'node:path'

const IGNORED_DIRS = new Set([
  '.git', '.vs', '.idea', '.vscode', '.vite', '.obsidian',
  'node_modules', 'bin', 'obj', 'dist', 'coverage', 'TestResults',
])
const IGNORED_PATHS = new Set(['.claude/settings.local.json', 'CLAUDE.local.md'])
const MTIME_TOLERANCE_MS = 1000
const MAX_LISTED = 8

/** Entrada JSON del hook; null si no se puede leer (en ese caso no se bloquea). */
function readHookInput() {
  try {
    return JSON.parse(readFileSync(0, 'utf8'))
  } catch {
    return null
  }
}

function git(root, args) {
  return execFileSync('git', args, { cwd: root, encoding: 'utf8', stdio: ['ignore', 'pipe', 'ignore'] })
}

/** Rutas absolutas modificadas/no rastreadas según git; null si no es un repositorio git. */
function changedPathsFromGit(root) {
  try {
    const topLevel = realpathSync.native(git(root, ['rev-parse', '--show-toplevel']).trim())
    const entries = git(root, ['status', '--porcelain', '--untracked-files=all', '-z']).split('\0').filter(Boolean)
    const paths = []
    for (let i = 0; i < entries.length; i++) {
      const status = entries[i].slice(0, 2)
      paths.push(path.resolve(topLevel, entries[i].slice(3))) // git reporta rutas relativas al toplevel
      if (status.startsWith('R') || status.startsWith('C')) i++ // omite la ruta de origen
    }
    return paths
  } catch {
    return null
  }
}

/** Recorrido del árbol cuando aún no hay git. */
function walk(dir, acc = []) {
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    if (IGNORED_DIRS.has(entry.name)) continue
    const fullPath = path.join(dir, entry.name)
    if (entry.isDirectory()) walk(fullPath, acc)
    else acc.push(fullPath)
  }
  return acc
}

function isIgnoredFileName(name) {
  if (name === 'Thumbs.db' || name === '.DS_Store' || name.endsWith('.log')) return true
  return name.startsWith('.env') && name !== '.env.example' // secretos locales, nunca versionados
}

function isRelevant(relativePath) {
  if (relativePath.startsWith('..') || relativePath.startsWith('wiki/')) return false
  if (IGNORED_PATHS.has(relativePath)) return false
  const segments = relativePath.split('/')
  if (isIgnoredFileName(segments.at(-1))) return false
  return !segments.some((segment) => IGNORED_DIRS.has(segment))
}

function modifiedAfter(absolutePath, referenceMs) {
  try {
    return statSync(absolutePath).mtimeMs > referenceMs + MTIME_TOLERANCE_MS
  } catch {
    return false // archivo eliminado: no hay fecha que comparar
  }
}

function main() {
  const input = readHookInput()
  if (!input || input.stop_hook_active) return // sin entrada fiable o ya se devolvió el turno: no bloquear

  const declaredRoot = process.env.CLAUDE_PROJECT_DIR || input.cwd || process.cwd()
  if (!existsSync(path.join(declaredRoot, 'wiki'))) return
  // Ruta canónica: git devuelve rutas largas; la raíz puede llegar en formato corto 8.3 o por un enlace.
  const root = realpathSync.native(declaredRoot)

  const logPath = path.join(root, 'wiki', 'log.md')
  const logMtime = existsSync(logPath) ? statSync(logPath).mtimeMs : 0

  const absolutePaths = changedPathsFromGit(root) ?? walk(root)
  const pending = absolutePaths
    .map((absolutePath) => ({ absolutePath, relativePath: path.relative(root, absolutePath).split(path.sep).join('/') }))
    .filter(({ relativePath }) => isRelevant(relativePath))
    .filter(({ absolutePath }) => modifiedAfter(absolutePath, logMtime))
    .map(({ relativePath }) => relativePath)
  if (pending.length === 0) return

  const extra = pending.length > MAX_LISTED ? ` y ${pending.length - MAX_LISTED} más` : ''
  const listed = pending.slice(0, MAX_LISTED).join(', ') + extra
  const reason =
    `Wiki pendiente: hay cambios posteriores a la última entrada de wiki/log.md (${listed}). ` +
    'Antes de terminar aplica AGENTS.md §4 paso 6: actualiza las páginas afectadas de wiki/ (tabla §5.6), ' +
    'index.md si hay páginas nuevas y añade la entrada al final de wiki/log.md. ' +
    'Si el cambio no amerita documentación, registra igualmente una entrada breve en el log.'

  process.stdout.write(JSON.stringify({ decision: 'block', reason }))
}

try {
  main()
} catch {
  // Falla en abierto: el hook nunca debe impedir que el agente termine por un error propio.
}
