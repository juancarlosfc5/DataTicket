import type { CurrentUser } from './currentUser'

/** Usuarios sintéticos para pruebas (sin datos reales). */
export function clientUser(overrides: Partial<CurrentUser> = {}): CurrentUser {
  return {
    id: 'b0000000-0000-7000-8000-000001000001',
    displayName: 'Ana Pérez',
    email: 'ana.perez@empresa-sintetica-01.test',
    surface: 'client',
    roles: ['Requester'],
    teams: [],
    company: { id: 'c0000000-0000-7000-8000-000000000001', name: 'Empresa Sintética 01' },
    ...overrides,
  }
}

export function internalUser(overrides: Partial<CurrentUser> = {}): CurrentUser {
  return {
    id: 'a0000000-0000-7000-8000-000000000001',
    displayName: 'Elizabeth',
    email: 'elizabeth@dataticket.local',
    surface: 'internal',
    roles: ['ProductManager'],
    teams: ['Production'],
    company: null,
    ...overrides,
  }
}
