import type { ClientRole, CurrentUser, InternalRole, Surface } from './currentUser'

/**
 * Secciones del shell y quién puede abrirlas. Es AYUDA de navegación: ocultar o negar una
 * sección en la interfaz no autoriza nada. La barrera real es el backend (PRD §5 regla 2).
 */
export type SectionId =
  | 'portal-home'
  | 'portal-tickets'
  | 'portal-submit'
  | 'internal-home'
  | 'internal-inbox'
  | 'internal-triage'
  | 'internal-admin'
  | 'internal-status'

export interface NavigationItem {
  readonly id: SectionId
  readonly label: string
  readonly to: string
}

interface Section {
  readonly id: SectionId
  readonly path: string
  readonly surface: Surface
  readonly label: (user: CurrentUser) => string
  /** Si se indica, el usuario necesita al menos uno de estos roles. */
  readonly requiredRoles?: readonly (ClientRole | InternalRole)[]
}

export const SURFACE_HOME: Readonly<Record<Surface, string>> = { client: '/portal', internal: '/interno' }
export const FORBIDDEN_PATH = '/sin-permiso'

const SECTIONS: readonly Section[] = [
  { id: 'portal-home', path: '/portal', surface: 'client', label: () => 'Inicio' },
  {
    id: 'portal-tickets',
    path: '/portal/tickets',
    surface: 'client',
    label: (user) => (user.roles.includes('CompanyCoordinator') ? 'Tickets de la empresa' : 'Mis tickets'),
  },
  { id: 'portal-submit', path: '/portal/radicar', surface: 'client', label: () => 'Radicar ticket' },
  { id: 'internal-home', path: '/interno', surface: 'internal', label: () => 'Inicio' },
  { id: 'internal-inbox', path: '/interno/bandeja', surface: 'internal', label: () => 'Bandeja' },
  {
    id: 'internal-triage',
    path: '/interno/triage',
    surface: 'internal',
    label: () => 'Cola de triage',
    requiredRoles: ['ProductManager'],
  },
  {
    id: 'internal-admin',
    path: '/interno/administracion',
    surface: 'internal',
    label: () => 'Administración',
    requiredRoles: ['Administrator'],
  },
  { id: 'internal-status', path: '/interno/estado', surface: 'internal', label: () => 'Estado del sistema' },
]

function normalizePath(pathname: string): string {
  return pathname.length > 1 && pathname.endsWith('/') ? pathname.slice(0, -1) : pathname
}

function isWithin(pathname: string, base: string): boolean {
  return pathname === base || pathname.startsWith(`${base}/`)
}

/** Sección más específica que contiene la ruta (la de prefijo más largo). */
function sectionFor(pathname: string): Section | null {
  const path = normalizePath(pathname)
  return SECTIONS
    .filter((section) => isWithin(path, section.path))
    .reduce<Section | null>((best, section) => (!best || section.path.length > best.path.length ? section : best), null)
}

function isAllowed(user: CurrentUser, section: Section): boolean {
  if (user.surface !== section.surface) return false
  return !section.requiredRoles || section.requiredRoles.some((role) => user.roles.includes(role))
}

/** Inicio de la superficie del usuario; sin superficie conocida, la vista «sin permiso». */
export function homePathFor(user: CurrentUser): string {
  return user.surface === 'none' ? FORBIDDEN_PATH : SURFACE_HOME[user.surface]
}

/** Niega por defecto: rutas fuera de las secciones conocidas o de otra superficie → `false`. */
export function canAccessPath(user: CurrentUser, pathname: string): boolean {
  const section = sectionFor(pathname)
  return section !== null && isAllowed(user, section)
}

/** Navegación del usuario según su superficie y roles (función pura). */
export function navigationFor(user: CurrentUser): readonly NavigationItem[] {
  return SECTIONS.filter((section) => isAllowed(user, section)).map((section) => ({
    id: section.id,
    label: section.label(user),
    to: section.path,
  }))
}

/** Ítem de la navegación que corresponde a la ruta actual, o `null`. */
export function currentNavigationId(items: readonly NavigationItem[], pathname: string): SectionId | null {
  const path = normalizePath(pathname)
  const match = items
    .filter((item) => isWithin(path, item.to))
    .reduce<NavigationItem | null>((best, item) => (!best || item.to.length > best.to.length ? item : best), null)
  return match?.id ?? null
}

/** Título de una sección (para los marcadores de posición). */
export function sectionTitle(id: SectionId, user: CurrentUser): string {
  const section = SECTIONS.find((candidate) => candidate.id === id)
  return section ? section.label(user) : ''
}
