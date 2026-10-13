import { describe, expect, it } from 'vitest'
import { canAccessSection, currentNavigationId, homePathFor, navigationFor } from './navigation'
import { clientUser, internalUser } from './testUsers'

const INTERNAL_TEXTS = ['Triage', 'Chat', 'Administración', 'Equipo']

/**
 * La navegación por rol es AYUDA de interfaz: ocultar un enlace no autoriza nada.
 * La barrera real es el backend (PRD §5 regla 2): cada endpoint aplica su política.
 */
describe('navigationFor', () => {
  it('navigationFor de Requester no incluye secciones internas', () => {
    for (const user of [clientUser({ roles: ['Requester'] }), clientUser({ roles: ['CompanyCoordinator'] })]) {
      const items = navigationFor(user)

      expect(items.length).toBeGreaterThan(0)
      for (const item of items) {
        expect(item.to.startsWith('/portal')).toBe(true)
        expect(item.to.startsWith('/interno')).toBe(false)
        for (const text of INTERNAL_TEXTS) expect(item.label).not.toContain(text)
      }
    }
  })

  it('navigationFor solo muestra Administración al Administrator', () => {
    const labelsOf = (roles: Parameters<typeof internalUser>[0]) => navigationFor(internalUser(roles)).map((i) => i.label)

    expect(labelsOf({ roles: ['Administrator'], teams: [] })).toContain('Administración')
    expect(labelsOf({ roles: ['ProductManager'], teams: [] })).not.toContain('Administración')
    expect(labelsOf({ roles: [], teams: ['Development'] })).not.toContain('Administración')
  })

  it('navigationFor solo muestra Cola de triage al ProductManager', () => {
    const labelsOf = (roles: Parameters<typeof internalUser>[0]) => navigationFor(internalUser(roles)).map((i) => i.label)

    expect(labelsOf({ roles: ['ProductManager'], teams: [] })).toContain('Cola de triage')
    expect(labelsOf({ roles: ['Administrator'], teams: [] })).not.toContain('Cola de triage')
  })

  it('un miembro solo de Development no ve Cola de triage ni Administración', () => {
    const labels = navigationFor(internalUser({ roles: [], teams: ['Development'] })).map((i) => i.label)

    expect(labels).not.toContain('Cola de triage')
    expect(labels).not.toContain('Administración')
    expect(labels).toContain('Bandeja')
  })

  it('la navegación interna nunca apunta al portal', () => {
    const items = navigationFor(internalUser({ roles: ['ProductManager', 'Administrator'] }))

    expect(items.every((item) => item.to.startsWith('/interno'))).toBe(true)
  })

  it('un usuario sin superficie no tiene navegación', () => {
    expect(navigationFor(clientUser({ surface: 'none', roles: [], company: null }))).toEqual([])
  })

  it('el coordinador ve los tickets de su empresa y el solicitante los suyos', () => {
    expect(navigationFor(clientUser({ roles: ['CompanyCoordinator'] })).map((i) => i.label)).toContain(
      'Tickets de la empresa',
    )
    expect(navigationFor(clientUser({ roles: ['Requester'] })).map((i) => i.label)).toContain('Mis tickets')
  })
})

describe('currentNavigationId', () => {
  it('marca la sección más específica y no confunde Inicio con sus hijas', () => {
    const items = navigationFor(internalUser({ roles: ['Administrator'] }))

    expect(currentNavigationId(items, '/interno')).toBe('internal-home')
    expect(currentNavigationId(items, '/interno/')).toBe('internal-home')
    expect(currentNavigationId(items, '/interno/administracion')).toBe('internal-admin')
    expect(currentNavigationId(items, '/interno/administracion/usuarios')).toBe('internal-admin')
    expect(currentNavigationId(items, '/sin-permiso')).toBeNull()
  })

  it('tolera rutas codificadas o en mayúsculas como las resuelve el enrutador', () => {
    const items = navigationFor(internalUser({ roles: ['Administrator'] }))

    expect(currentNavigationId(items, '/interno/%61dministracion')).toBe('internal-admin')
    expect(currentNavigationId(items, '/INTERNO/Administracion')).toBe('internal-admin')
    expect(currentNavigationId(items, '/interno/%E0%A4%A')).toBe('internal-home')
  })
})

describe('homePathFor y canAccessSection', () => {
  it('cada superficie tiene su inicio', () => {
    expect(homePathFor(clientUser())).toBe('/portal')
    expect(homePathFor(internalUser())).toBe('/interno')
    expect(homePathFor(clientUser({ surface: 'none' }))).toBe('/sin-permiso')
  })

  it('un cliente no accede a la superficie interna ni un interno al portal', () => {
    expect(canAccessSection(clientUser(), 'portal-tickets')).toBe(true)
    expect(canAccessSection(clientUser(), 'internal-admin')).toBe(false)
    expect(canAccessSection(clientUser(), 'internal-home')).toBe(false)
    expect(canAccessSection(internalUser(), 'internal-inbox')).toBe(true)
    expect(canAccessSection(internalUser(), 'portal-home')).toBe(false)
  })

  it('las secciones restringidas por rol se niegan a quien no tiene el rol', () => {
    const developer = internalUser({ roles: [], teams: ['Development'] })

    expect(canAccessSection(developer, 'internal-triage')).toBe(false)
    expect(canAccessSection(developer, 'internal-admin')).toBe(false)
    expect(canAccessSection(internalUser({ roles: ['Administrator'] }), 'internal-admin')).toBe(true)
  })

  it('un usuario sin superficie no accede a ninguna sección', () => {
    expect(canAccessSection(clientUser({ surface: 'none' }), 'portal-home')).toBe(false)
    expect(canAccessSection(internalUser({ surface: 'none' }), 'internal-home')).toBe(false)
  })
})
