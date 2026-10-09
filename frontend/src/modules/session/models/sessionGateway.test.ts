import { afterEach, describe, expect, it, vi } from 'vitest'
import { setUnauthorizedHandler } from '../../../core/http/authEvents'
import { HttpError } from '../../../core/http/httpError'
import { fetchCurrentUser } from './sessionGateway'

function stubFetch(response: Response) {
  const fetchMock = vi.fn(async (_input: RequestInfo | URL, _init?: RequestInit) => response)
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

afterEach(() => {
  setUnauthorizedHandler(null)
  vi.unstubAllGlobals()
})

describe('sessionGateway.fetchCurrentUser', () => {
  it('pide /api/auth/me y normaliza el usuario', async () => {
    const fetchMock = stubFetch(
      Response.json({
        id: 'u1',
        displayName: 'Elizabeth',
        email: 'elizabeth@dataticket.local',
        surface: 'internal',
        roles: ['ProductManager'],
        teams: [],
        company: null,
      }),
    )

    const user = await fetchCurrentUser()

    expect(fetchMock.mock.calls[0]?.[0]).toBe('/api/auth/me')
    expect(user?.surface).toBe('internal')
  })

  it('un 401 significa sesión anónima y no dispara el manejador global', async () => {
    const onUnauthorized = vi.fn()
    setUnauthorizedHandler(onUnauthorized)
    stubFetch(new Response(JSON.stringify({ status: 401 }), { status: 401, headers: { 'Content-Type': 'application/problem+json' } }))

    await expect(fetchCurrentUser()).resolves.toBeNull()
    expect(onUnauthorized).not.toHaveBeenCalled()
  })

  it('una respuesta fuera de contrato es un error', async () => {
    stubFetch(Response.json({ hola: 'mundo' }))

    await expect(fetchCurrentUser()).rejects.toThrow()
  })

  it('un 5xx se propaga como HttpError', async () => {
    stubFetch(new Response('<html></html>', { status: 502, headers: { 'Content-Type': 'text/html' } }))

    await expect(fetchCurrentUser()).rejects.toBeInstanceOf(HttpError)
  })
})
