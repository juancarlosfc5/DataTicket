// @vitest-environment jsdom
import '../../../test/domSetup'
import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { invalidateAntiforgeryToken } from '../../../core/http/antiforgery'
import { GENERIC_ERROR_MESSAGE } from '../../../core/http/httpError'
import { useLogoutController } from './useLogoutController'

function stubLogout(respond: () => Response | Promise<Response>) {
  const fetchMock = vi.fn(async (input: RequestInfo | URL, _init?: RequestInit) =>
    String(input) === '/api/auth/antiforgery'
      ? Response.json({ headerName: 'X-XSRF-TOKEN', token: 't' })
      : respond(),
  )
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

beforeEach(() => invalidateAntiforgeryToken())
afterEach(() => vi.unstubAllGlobals())

describe('useLogoutController', () => {
  it('llama a POST /api/auth/logout, expone isSigningOut y descarta la sesión al terminar', async () => {
    let release: (response: Response) => void = () => undefined
    const fetchMock = stubLogout(() => new Promise<Response>((resolve) => (release = resolve)))
    const onSignedOut = vi.fn()
    const { result } = renderHook(() => useLogoutController({ onSignedOut }))

    let pending: Promise<void> = Promise.resolve()
    act(() => {
      pending = result.current.signOut()
    })
    await waitFor(() => expect(result.current.isSigningOut).toBe(true))
    await act(async () => {
      release(new Response(null, { status: 204 }))
      await pending
    })

    expect(fetchMock.mock.calls.some(([input, init]) => String(input) === '/api/auth/logout' && init?.method === 'POST')).toBe(true)
    expect(onSignedOut).toHaveBeenCalledTimes(1)
    expect(result.current.isSigningOut).toBe(false)
    expect(result.current.errorMessage).toBeNull()
  })

  it('si el backend responde 401 igual descarta la sesión local', async () => {
    stubLogout(() => new Response(JSON.stringify({ status: 401 }), { status: 401, headers: { 'Content-Type': 'application/problem+json' } }))
    const onSignedOut = vi.fn()
    const { result } = renderHook(() => useLogoutController({ onSignedOut }))

    await act(() => result.current.signOut())

    expect(onSignedOut).toHaveBeenCalledTimes(1)
    expect(result.current.errorMessage).toBeNull()
  })

  it('otros errores conservan la sesión y muestran el mensaje genérico', async () => {
    stubLogout(() => new Response('<html></html>', { status: 503, headers: { 'Content-Type': 'text/html' } }))
    const onSignedOut = vi.fn()
    const { result } = renderHook(() => useLogoutController({ onSignedOut }))

    await act(() => result.current.signOut())

    expect(onSignedOut).not.toHaveBeenCalled()
    expect(result.current.errorMessage).toBe(GENERIC_ERROR_MESSAGE)
    expect(result.current.isSigningOut).toBe(false)
  })
})
