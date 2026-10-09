/**
 * Usuario de la sesión, tal como lo describe `GET /api/auth/me` (contrato ratificado en
 * `wiki/arquitectura/autenticacion-identity.md`). Vive solo en memoria: nunca en Storage.
 */
export type Surface = 'client' | 'internal'
/** `none`: el backend envió una superficie desconocida → el usuario no tiene acceso a ninguna. */
export type SurfaceAccess = Surface | 'none'

export type ClientRole = 'Requester' | 'CompanyCoordinator'
export type InternalRole = 'ProductManager' | 'Administrator'
export type Role = ClientRole | InternalRole
export type Team = 'Development' | 'Production'

export interface Company {
  readonly id: string
  readonly name: string
}

export interface CurrentUser {
  readonly id: string
  readonly displayName: string
  readonly email: string
  readonly surface: SurfaceAccess
  readonly roles: readonly Role[]
  readonly teams: readonly Team[]
  /** Solo para usuarios cliente; `null` para internos. */
  readonly company: Company | null
}

const CLIENT_ROLES: readonly ClientRole[] = ['Requester', 'CompanyCoordinator']
const INTERNAL_ROLES: readonly InternalRole[] = ['ProductManager', 'Administrator']
const TEAMS: readonly Team[] = ['Development', 'Production']

function nonEmptyString(value: unknown): string | null {
  return typeof value === 'string' && value.trim().length > 0 ? value : null
}

function toSurface(value: unknown): SurfaceAccess {
  return value === 'client' || value === 'internal' ? value : 'none'
}

function pickKnown<T extends string>(value: unknown, allowed: readonly T[]): readonly T[] {
  if (!Array.isArray(value)) return []
  return allowed.filter((item) => value.includes(item))
}

function toCompany(value: unknown): Company | null {
  if (typeof value !== 'object' || value === null) return null
  const { id, name } = value as { id?: unknown; name?: unknown }
  const companyId = nonEmptyString(id)
  const companyName = nonEmptyString(name)
  return companyId && companyName ? { id: companyId, name: companyName } : null
}

function rolesFor(surface: SurfaceAccess, value: unknown): readonly Role[] {
  if (surface === 'client') return pickKnown(value, CLIENT_ROLES)
  if (surface === 'internal') return pickKnown(value, INTERNAL_ROLES)
  return []
}

/**
 * Normaliza el JSON de `/me`. Devuelve `null` si no cumple el contrato (sin `id` o sin correo).
 * Roles, equipos y empresa ajenos a la superficie se descartan: el shell nunca amplía permisos.
 */
export function toCurrentUser(value: unknown): CurrentUser | null {
  if (typeof value !== 'object' || value === null) return null
  const raw = value as Record<string, unknown>

  const id = nonEmptyString(raw.id)
  const email = nonEmptyString(raw.email)
  if (!id || !email) return null

  const surface = toSurface(raw.surface)
  return {
    id,
    displayName: nonEmptyString(raw.displayName) ?? email,
    email,
    surface,
    roles: rolesFor(surface, raw.roles),
    teams: surface === 'internal' ? pickKnown(raw.teams, TEAMS) : [],
    company: surface === 'client' ? toCompany(raw.company) : null,
  }
}

/** Iniciales para el avatar: nombre y último apellido. `?` si no hay letras. */
export function userInitials(displayName: string): string {
  const words = displayName
    .trim()
    .split(/\s+/)
    .filter((word) => /\p{L}/u.test(word))
  if (words.length === 0) return '?'

  const initial = (word: string) => (word.match(/\p{L}/u)?.[0] ?? '').toLocaleUpperCase('es-CO')
  const first = initial(words[0])
  return words.length === 1 ? first : `${first}${initial(words[words.length - 1])}`
}
