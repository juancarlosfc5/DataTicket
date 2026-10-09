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
