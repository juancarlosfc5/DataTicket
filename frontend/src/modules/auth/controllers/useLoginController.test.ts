// @vitest-environment jsdom
import '../../../test/domSetup'
import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { invalidateAntiforgeryToken } from '../../../core/http/antiforgery'
import { GENERIC_ERROR_MESSAGE } from '../../../core/http/httpError'
import { useLoginController } from './useLoginController'

function stubLogin(respond: () => Response | Promise<Response>) {
  const fetchMock = vi.fn(async (input: RequestInfo | URL, _init?: RequestInit) =>
    String(input) === '/api/auth/antiforgery'
      ? Response.json({ headerName: 'X-XSRF-TOKEN', token: 't' })
      : respond(),
  )
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

function problem(status: number, body: Record<string, unknown> = {}): Response {
  return new Response(JSON.stringify({ status, ...body }), {
    status,
    headers: { 'Content-Type': 'application/problem+json' },
  })
}

function fill(result: { current: ReturnType<typeof useLoginController> }, email: string, password: string) {
  act(() => {
    result.current.changeEmail(email)
    result.current.changePassword(password)
  })
}

beforeEach(() => invalidateAntiforgeryToken())
afterEach(() => vi.unstubAllGlobals())

describe('useLoginController', () => {
  it('no envía nada si la validación local falla', async () => {
    const fetchMock = stubLogin(() => new Response(null, { status: 204 }))
    const { result } = renderHook(() => useLoginController({ onSignedIn: vi.fn() }))

    await act(() => result.current.submit())

    expect(fetchMock).not.toHaveBeenCalled()
    expect(result.current.state.status).toEqual({
      kind: 'error',
      message: null,
      fieldErrors: { email: 'Escribe tu correo.', password: 'Escribe tu contraseña.' },
    })
  })

  it('está en submitting mientras la petición está en curso y avisa al iniciar sesión (204)', async () => {
    let release: (response: Response) => void = () => undefined
    stubLogin(() => new Promise<Response>((resolve) => (release = resolve)))
    const onSignedIn = vi.fn()
    const { result } = renderHook(() => useLoginController({ onSignedIn }))
    fill(result, 'ana@empresa.test', 'secreta')

    let pending: Promise<void> = Promise.resolve()
    act(() => {
      pending = result.current.submit()
    })
    await waitFor(() => expect(result.current.state.status.kind).toBe('submitting'))

    await act(async () => {
      release(new Response(null, { status: 204 }))
      await pending
    })
    expect(onSignedIn).toHaveBeenCalledTimes(1)
  })

  it('un 401 muestra el mensaje genérico y vacía la contraseña', async () => {
    stubLogin(() => problem(401, { title: 'Correo o contraseña incorrectos.' }))
    const onSignedIn = vi.fn()
    const { result } = renderHook(() => useLoginController({ onSignedIn }))
    fill(result, 'ana@empresa.test', 'mala')

    await act(() => result.current.submit())

    expect(result.current.state.status).toEqual({
      kind: 'error',
      message: 'Correo o contraseña incorrectos.',
      fieldErrors: {},
    })
    expect(result.current.state.email).toBe('ana@empresa.test')
    expect(result.current.state.password).toBe('')
    expect(onSignedIn).not.toHaveBeenCalled()
  })

  it('un 400 con errores por campo los muestra junto al campo', async () => {
    stubLogin(() => problem(400, { errors: { email: ['El correo no tiene un formato válido.'] } }))
    const { result } = renderHook(() => useLoginController({ onSignedIn: vi.fn() }))
    fill(result, 'ana@empresa.test', 'secreta')

    await act(() => result.current.submit())

    expect(result.current.state.status).toEqual({
      kind: 'error',
      message: null,
      fieldErrors: { email: 'El correo no tiene un formato válido.' },
    })
  })

  it('un 5xx o un 400 sin errores por campo muestran el mensaje genérico', async () => {
    stubLogin(() => new Response('<html></html>', { status: 502, headers: { 'Content-Type': 'text/html' } }))
    const { result } = renderHook(() => useLoginController({ onSignedIn: vi.fn() }))
    fill(result, 'ana@empresa.test', 'secreta')

    await act(() => result.current.submit())

    expect(result.current.state.status).toEqual({ kind: 'error', message: GENERIC_ERROR_MESSAGE, fieldErrors: {} })
  })

  it('ignora un segundo envío mientras el primero está en curso', async () => {
    let release: (response: Response) => void = () => undefined
    const fetchMock = stubLogin(() => new Promise<Response>((resolve) => (release = resolve)))
    const { result } = renderHook(() => useLoginController({ onSignedIn: vi.fn() }))
    fill(result, 'ana@empresa.test', 'secreta')

    let first: Promise<void> = Promise.resolve()
    act(() => {
      first = result.current.submit()
    })
    await waitFor(() => expect(result.current.state.status.kind).toBe('submitting'))
    await act(() => result.current.submit())
    await act(async () => {
      release(new Response(null, { status: 204 }))
      await first
    })

    expect(fetchMock.mock.calls.filter(([input]) => String(input) === '/api/auth/login')).toHaveLength(1)
  })
})
