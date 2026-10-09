import { describe, expect, it } from 'vitest'
import { toCurrentUser, userInitials } from './currentUser'

const clientDto = {
  id: 'b0000000-0000-7000-8000-000001000001',
  displayName: 'Ana Pérez',
  email: 'ana.perez@empresa-sintetica-01.test',
  surface: 'client',
  roles: ['Requester'],
  teams: [],
  company: { id: 'c0000000-0000-7000-8000-000000000001', name: 'Empresa Sintética 01' },
}

const internalDto = {
  id: 'a0000000-0000-7000-8000-000000000001',
  displayName: 'Elizabeth',
  email: 'elizabeth@dataticket.local',
  surface: 'internal',
  roles: ['ProductManager'],
  teams: ['Production'],
  company: null,
}

describe('toCurrentUser (contrato de GET /api/auth/me)', () => {
  it('normaliza un usuario cliente con su empresa', () => {
    expect(toCurrentUser(clientDto)).toEqual(clientDto)
  })

  it('normaliza un usuario interno sin empresa', () => {
    expect(toCurrentUser(internalDto)).toEqual(internalDto)
  })

  it('trata un surface desconocido como sin acceso', () => {
    const user = toCurrentUser({ ...internalDto, surface: 'superadmin' })

    expect(user?.surface).toBe('none')
    expect(user?.roles).toEqual([])
    expect(user?.teams).toEqual([])
    expect(user?.company).toBeNull()
  })

  it('descarta roles y equipos desconocidos o ajenos a la superficie', () => {
    const client = toCurrentUser({ ...clientDto, roles: ['Requester', 'Administrator', 'Root', 7], teams: ['Development'] })
    const internal = toCurrentUser({
      ...internalDto,
      roles: ['Administrator', 'Requester'],
      teams: ['Development', 'Marketing'],
    })

    expect(client?.roles).toEqual(['Requester'])
    expect(client?.teams).toEqual([])
    expect(internal?.roles).toEqual(['Administrator'])
    expect(internal?.teams).toEqual(['Development'])
  })

  it('nunca asigna empresa a un usuario interno', () => {
    const user = toCurrentUser({ ...internalDto, company: { id: 'c1', name: 'Empresa' } })

    expect(user?.company).toBeNull()
  })

  it('ignora una empresa mal formada', () => {
    expect(toCurrentUser({ ...clientDto, company: { id: 3 } })?.company).toBeNull()
  })

  it('usa el correo si el nombre visible llega vacío', () => {
    expect(toCurrentUser({ ...clientDto, displayName: '   ' })?.displayName).toBe(clientDto.email)
  })

  it('rechaza respuestas que no cumplen el contrato', () => {
    expect(toCurrentUser(null)).toBeNull()
    expect(toCurrentUser('texto')).toBeNull()
    expect(toCurrentUser({ ...clientDto, id: '' })).toBeNull()
    expect(toCurrentUser({ ...clientDto, email: 42 })).toBeNull()
  })
})

describe('userInitials', () => {
  it('toma la inicial del nombre y del último apellido', () => {
    expect(userInitials('Ana Pérez')).toBe('AP')
    expect(userInitials('María José de la Torre')).toBe('MT')
    expect(userInitials('Elizabeth')).toBe('E')
  })

  it('devuelve un valor seguro con nombres vacíos o con símbolos', () => {
    expect(userInitials('   ')).toBe('?')
    expect(userInitials('ñandú')).toBe('Ñ')
  })
})

/** CHU-10 de HU-004: los dos ejemplos literales de la HU y un `surface` desconocido. */
describe('contrato de /me (HU-004 CHU-10)', () => {
  const HU004_CLIENT = {
    id: '6f1c2a9e-4c1d-4b8e-9a51-1f0e2d3c4b5a',
    displayName: 'Ana Pérez',
    email: 'ana.perez@empresa-sintetica-01.test',
    surface: 'client',
    roles: ['Requester'],
    teams: [],
    company: { id: '0b7d6c1e-2f3a-4b5c-8d9e-0a1b2c3d4e5f', name: 'Empresa Sintética 01' },
  }

  const HU004_INTERNAL = {
    id: '9a2e4f60-7b1c-4d2e-8f3a-5b6c7d8e9f01',
    displayName: 'Elizabeth',
    email: 'elizabeth@dataticket.local',
    surface: 'internal',
    roles: ['ProductManager'],
    teams: ['Production'],
    company: null,
  }

  it('el ejemplo de cliente produce un CurrentUser del portal con su empresa', () => {
    expect(toCurrentUser(HU004_CLIENT)).toEqual({
      id: '6f1c2a9e-4c1d-4b8e-9a51-1f0e2d3c4b5a',
      displayName: 'Ana Pérez',
      email: 'ana.perez@empresa-sintetica-01.test',
      surface: 'client',
      roles: ['Requester'],
      teams: [],
      company: { id: '0b7d6c1e-2f3a-4b5c-8d9e-0a1b2c3d4e5f', name: 'Empresa Sintética 01' },
    })
  })

  it('el ejemplo interno produce un CurrentUser interno sin empresa', () => {
    expect(toCurrentUser(HU004_INTERNAL)).toEqual({
      id: '9a2e4f60-7b1c-4d2e-8f3a-5b6c7d8e9f01',
      displayName: 'Elizabeth',
      email: 'elizabeth@dataticket.local',
      surface: 'internal',
      roles: ['ProductManager'],
      teams: ['Production'],
      company: null,
    })
  })

  it('surface "partner" produce un usuario sin acceso', () => {
    const user = toCurrentUser({ ...HU004_CLIENT, surface: 'partner' })

    expect(user?.surface).toBe('none')
    expect(user?.roles).toEqual([])
    expect(user?.company).toBeNull()
  })
})
