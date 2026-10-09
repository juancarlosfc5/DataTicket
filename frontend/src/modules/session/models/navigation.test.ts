import { describe, expect, it } from 'vitest'
import { canAccessPath, currentNavigationId, homePathFor, navigationFor } from './navigation'
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
})

describe('homePathFor y canAccessPath', () => {
  it('cada superficie tiene su inicio', () => {
    expect(homePathFor(clientUser())).toBe('/portal')
    expect(homePathFor(internalUser())).toBe('/interno')
    expect(homePathFor(clientUser({ surface: 'none' }))).toBe('/sin-permiso')
  })

  it('un cliente no accede a la superficie interna ni un interno al portal', () => {
    expect(canAccessPath(clientUser(), '/portal/tickets')).toBe(true)
    expect(canAccessPath(clientUser(), '/interno/administracion')).toBe(false)
    expect(canAccessPath(clientUser(), '/interno')).toBe(false)
    expect(canAccessPath(internalUser(), '/interno/bandeja')).toBe(true)
    expect(canAccessPath(internalUser(), '/portal')).toBe(false)
  })

  it('las secciones restringidas por rol se niegan a quien no tiene el rol', () => {
    const developer = internalUser({ roles: [], teams: ['Development'] })

    expect(canAccessPath(developer, '/interno/triage')).toBe(false)
    expect(canAccessPath(developer, '/interno/administracion')).toBe(false)
    expect(canAccessPath(internalUser({ roles: ['Administrator'] }), '/interno/administracion')).toBe(true)
  })

  it('niega por defecto rutas desconocidas y usuarios sin superficie', () => {
    expect(canAccessPath(internalUser(), '/otra-cosa')).toBe(false)
    expect(canAccessPath(clientUser({ surface: 'none' }), '/portal')).toBe(false)
    expect(canAccessPath(clientUser(), '/portalfalso')).toBe(false)
  })
})
