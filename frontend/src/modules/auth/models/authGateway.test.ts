import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { invalidateAntiforgeryToken } from '../../../core/http/antiforgery'
import { setUnauthorizedHandler } from '../../../core/http/authEvents'
import { HttpError } from '../../../core/http/httpError'
import { login, logout } from './authGateway'

let tokenCounter = 0

function stubApi(respond: (url: string) => Response) {
  const fetchMock = vi.fn(async (input: RequestInfo | URL, _init?: RequestInit) => {
    const url = String(input)
    if (url === '/api/auth/antiforgery') {
      tokenCounter += 1
      return Response.json({ headerName: 'X-XSRF-TOKEN', token: `t-${tokenCounter}` })
    }
    return respond(url)
  })
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

function problem(status: number, body: Record<string, unknown> = {}): Response {
  return new Response(JSON.stringify({ status, ...body }), {
    status,
    headers: { 'Content-Type': 'application/problem+json' },
  })
}

function tokensSentTo(fetchMock: ReturnType<typeof stubApi>, url: string): (string | null)[] {
  return fetchMock.mock.calls
    .filter(([input]) => String(input) === url)
    .map(([, init]) => new Headers(init?.headers).get('X-XSRF-TOKEN'))
}

beforeEach(() => {
  tokenCounter = 0
  invalidateAntiforgeryToken()
})

afterEach(() => {
  setUnauthorizedHandler(null)
  vi.unstubAllGlobals()
})

describe('authGateway.login', () => {
  it('envía POST /api/auth/login con el correo y la contraseña', async () => {
    const fetchMock = stubApi(() => new Response(null, { status: 204 }))

    await login({ email: 'ana@empresa.test', password: 'secreta' })

    const call = fetchMock.mock.calls.find(([input]) => String(input) === '/api/auth/login')
    expect(call?.[1]?.method).toBe('POST')
    expect(call?.[1]?.body).toBe(JSON.stringify({ email: 'ana@empresa.test', password: 'secreta' }))
  })

  it('invalida el token antiforgery tras iniciar sesión', async () => {
    const fetchMock = stubApi(() => new Response(null, { status: 204 }))

    await login({ email: 'ana@empresa.test', password: 'secreta' })
    await logout()

    expect(tokensSentTo(fetchMock, '/api/auth/login')).toEqual(['t-1'])
    expect(tokensSentTo(fetchMock, '/api/auth/logout')).toEqual(['t-2'])
  })

  it('un 401 se propaga como HttpError sin disparar el manejador global', async () => {
    const onUnauthorized = vi.fn()
    setUnauthorizedHandler(onUnauthorized)
    stubApi(() => problem(401, { title: 'Correo o contraseña incorrectos.' }))

    const error = await login({ email: 'ana@empresa.test', password: 'x' }).catch((caught: unknown) => caught)

    expect(error).toBeInstanceOf(HttpError)
    expect((error as HttpError).status).toBe(401)
    expect(onUnauthorized).not.toHaveBeenCalled()
  })
})

describe('authGateway.logout', () => {
  it('envía POST /api/auth/logout e invalida el token después', async () => {
    const fetchMock = stubApi(() => new Response(null, { status: 204 }))

    await logout()
    await logout()

    expect(tokensSentTo(fetchMock, '/api/auth/logout')).toEqual(['t-1', 't-2'])
  })

  it('un 401 (sesión ya caducada) no dispara el manejador global', async () => {
    const onUnauthorized = vi.fn()
    setUnauthorizedHandler(onUnauthorized)
    stubApi(() => problem(401))

    await expect(logout()).rejects.toBeInstanceOf(HttpError)
    expect(onUnauthorized).not.toHaveBeenCalled()
  })
})
