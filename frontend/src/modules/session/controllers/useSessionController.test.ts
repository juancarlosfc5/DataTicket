// @vitest-environment jsdom
import '../../../test/domSetup'
import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { getJson } from '../../../core/http/httpClient'
import { useSessionController } from './useSessionController'

const ME = {
  id: 'u1',
  displayName: 'Elizabeth',
  email: 'elizabeth@dataticket.local',
  surface: 'internal',
  roles: ['ProductManager'],
  teams: [],
  company: null,
}

function problemResponse(status: number): Response {
  return new Response(JSON.stringify({ status }), { status, headers: { 'Content-Type': 'application/problem+json' } })
}

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useSessionController', () => {
  it('carga el usuario actual al montar', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => Response.json(ME)))

    const { result } = renderHook(() => useSessionController())

    expect(result.current.state.kind).toBe('loading')
    await waitFor(() => expect(result.current.state.kind).toBe('authenticated'))
  })

  it('sin sesión queda anónimo', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => problemResponse(401)))

    const { result } = renderHook(() => useSessionController())

    await waitFor(() => expect(result.current.state.kind).toBe('anonymous'))
  })

  it('registra el manejador global de 401: una petición rechazada descarta la sesión', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(async (input: RequestInfo | URL) => (String(input) === '/api/auth/me' ? Response.json(ME) : problemResponse(401))),
    )
    const { result } = renderHook(() => useSessionController())
    await waitFor(() => expect(result.current.state.kind).toBe('authenticated'))

    await act(async () => {
      await getJson('/tickets').catch(() => undefined)
    })

    expect(result.current.state.kind).toBe('anonymous')
  })

  it('invoca onForbidden ante un 403 y deja de escuchar al desmontar', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(async (input: RequestInfo | URL) => (String(input) === '/api/auth/me' ? Response.json(ME) : problemResponse(403))),
    )
    const onForbidden = vi.fn()
    const { result, unmount } = renderHook(() => useSessionController({ onForbidden }))
    await waitFor(() => expect(result.current.state.kind).toBe('authenticated'))

    await act(async () => {
      await getJson('/admin').catch(() => undefined)
    })
    expect(onForbidden).toHaveBeenCalledTimes(1)

    unmount()
    await getJson('/admin').catch(() => undefined)
    expect(onForbidden).toHaveBeenCalledTimes(1)
  })

  it('endSession descarta el usuario en memoria y refresh vuelve a consultar /me', async () => {
    const fetchMock = vi.fn(async () => Response.json(ME))
    vi.stubGlobal('fetch', fetchMock)
    const { result } = renderHook(() => useSessionController())
    await waitFor(() => expect(result.current.state.kind).toBe('authenticated'))

    act(() => result.current.endSession())
    expect(result.current.state.kind).toBe('anonymous')

    act(() => result.current.refresh())
    await waitFor(() => expect(result.current.state.kind).toBe('authenticated'))
    expect(fetchMock).toHaveBeenCalledTimes(2)
  })
})

describe('useSessionController — carrera de /me', () => {
  it('endSession aborta el /me en curso: una respuesta tardía no restaura la sesión', async () => {
    let release: (response: Response) => void = () => undefined
    let aborted = false
    vi.stubGlobal(
      'fetch',
      vi.fn(
        (_input: RequestInfo | URL, init?: RequestInit) =>
          new Promise<Response>((resolve) => {
            init?.signal?.addEventListener('abort', () => {
              aborted = true
            })
            release = resolve
          }),
      ),
    )
    const { result } = renderHook(() => useSessionController())

    act(() => result.current.endSession())
    await act(async () => {
      release(Response.json(ME))
    })

    expect(aborted).toBe(true)
    expect(result.current.state).toEqual({ kind: 'anonymous', reason: 'signedOut' })
  })

  it('un 401 global aborta el /me en curso', async () => {
    let releaseMe: (response: Response) => void = () => undefined
    vi.stubGlobal(
      'fetch',
      vi.fn((input: RequestInfo | URL) =>
        String(input) === '/api/auth/me'
          ? new Promise<Response>((resolve) => (releaseMe = resolve))
          : Promise.resolve(problemResponse(401)),
      ),
    )
    const { result } = renderHook(() => useSessionController())

    await act(async () => {
      await getJson('/tickets').catch(() => undefined)
    })
    await act(async () => {
      releaseMe(Response.json(ME))
    })

    expect(result.current.state).toEqual({ kind: 'anonymous', reason: 'expired' })
  })
})
