import { describe, expect, it } from 'vitest'
import { INITIAL_SESSION_STATE, sessionReducer, type SessionState } from './sessionState'
import { clientUser } from './testUsers'

const authenticated: SessionState = { kind: 'authenticated', user: clientUser() }

describe('sessionReducer', () => {
  it('empieza cargando', () => {
    expect(INITIAL_SESSION_STATE).toEqual({ kind: 'loading' })
  })

  it('pasa a autenticado o anónimo según /me', () => {
    expect(sessionReducer(INITIAL_SESSION_STATE, { type: 'loaded', user: clientUser() })).toEqual(authenticated)
    expect(sessionReducer(INITIAL_SESSION_STATE, { type: 'loaded', user: null })).toEqual({
      kind: 'anonymous',
      reason: 'noSession',
    })
  })

  it('un fallo de carga deja un error con mensaje', () => {
    expect(sessionReducer(INITIAL_SESSION_STATE, { type: 'loadFailed', message: 'x' })).toEqual({
      kind: 'error',
      message: 'x',
    })
  })

  it('recargar desde error o anónimo vuelve a cargando, pero no oculta una sesión vigente', () => {
    expect(sessionReducer({ kind: 'error', message: 'x' }, { type: 'loadStarted' })).toEqual({ kind: 'loading' })
    expect(sessionReducer({ kind: 'anonymous', reason: 'expired' }, { type: 'loadStarted' })).toEqual({ kind: 'loading' })
    expect(sessionReducer(authenticated, { type: 'loadStarted' })).toBe(authenticated)
  })

  it('una sesión caducada o cerrada descarta el usuario en memoria y recuerda el motivo', () => {
    expect(sessionReducer(authenticated, { type: 'sessionExpired' })).toEqual({ kind: 'anonymous', reason: 'expired' })
    expect(sessionReducer(authenticated, { type: 'signedOut' })).toEqual({ kind: 'anonymous', reason: 'signedOut' })
  })
})
