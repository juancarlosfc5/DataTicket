import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { invalidateAntiforgeryToken } from './antiforgery'
import { setForbiddenHandler, setUnauthorizedHandler } from './authEvents'
import { GENERIC_ERROR_MESSAGE, HttpError, userMessageFor } from './httpError'
import { deleteRequest, getJson, patchJson, postJson, putJson } from './httpClient'

const ANTIFORGERY = { headerName: 'X-XSRF-TOKEN', token: 'token-sintetico-1' }

function json(body: unknown, status = 200, contentType = 'application/json'): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': contentType } })
}

function problem(body: unknown, status: number): Response {
  return json(body, status, 'application/problem+json')
}

/** Simula el backend: responde el token antiforgery y delega el resto en `respond`. */
function mockFetch(respond: (url: string, init: RequestInit) => Response) {
  const fetchMock = vi.fn(async (input: RequestInfo | URL, init: RequestInit = {}) => {
    const url = String(input)
    if (url === '/api/auth/antiforgery') return json(ANTIFORGERY)
    return respond(url, init)
  })
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

function callsTo(fetchMock: ReturnType<typeof mockFetch>, url: string) {
  return fetchMock.mock.calls.filter(([input]) => String(input) === url)
}

function headersOf(init: RequestInit | undefined): Headers {
  return new Headers(init?.headers)
}

beforeEach(() => {
  invalidateAntiforgeryToken()
})

afterEach(() => {
  setUnauthorizedHandler(null)
  setForbiddenHandler(null)
  vi.unstubAllGlobals()
})

describe('core/http — mutaciones', () => {
  it('postJson envía cabecera antiforgery y credenciales same-origin', async () => {
    const fetchMock = mockFetch(() => json({ ok: true }))

    const result = await postJson<{ ok: boolean }>('/x', { a: 1 })

    expect(result).toEqual({ ok: true })
    const [[url, init]] = callsTo(fetchMock, '/api/x')
    expect(url).toBe('/api/x')
    expect(init?.method).toBe('POST')
    expect(init?.credentials).toBe('same-origin')
    expect(init?.body).toBe(JSON.stringify({ a: 1 }))
    const headers = headersOf(init)
    expect(headers.get('Content-Type')).toBe('application/json')
    expect(headers.get('X-XSRF-TOKEN')).toBe(ANTIFORGERY.token)
  })

  it('putJson, patchJson y deleteRequest también envían la cabecera antiforgery', async () => {
    const fetchMock = mockFetch(() => new Response(null, { status: 204 }))

    await putJson('/recurso/1', { b: 2 })
    await patchJson('/recurso/1', { c: 3 })
    await deleteRequest('/recurso/1')

    const calls = callsTo(fetchMock, '/api/recurso/1')
    expect(calls.map(([, init]) => init?.method)).toEqual(['PUT', 'PATCH', 'DELETE'])
    for (const [, init] of calls) {
      expect(init?.credentials).toBe('same-origin')
      expect(headersOf(init).get('X-XSRF-TOKEN')).toBe(ANTIFORGERY.token)
    }
    expect(headersOf(calls[0][1]).get('Content-Type')).toBe('application/json')
    expect(headersOf(calls[1][1]).get('Content-Type')).toBe('application/json')
  })

  it('usa el nombre de cabecera que indica /api/auth/antiforgery', async () => {
    const fetchMock = vi.fn(async (input: RequestInfo | URL, _init: RequestInit = {}) =>
      String(input) === '/api/auth/antiforgery'
        ? json({ headerName: 'X-Otro-Token', token: 't-2' })
        : new Response(null, { status: 204 }),
    )
    vi.stubGlobal('fetch', fetchMock)

    await postJson('/x', {})

    const [, init] = fetchMock.mock.calls[1]
    expect(headersOf(init).get('X-Otro-Token')).toBe('t-2')
    expect(headersOf(init).has('X-XSRF-TOKEN')).toBe(false)
  })

  it('el token antiforgery se pide una sola vez y se renueva tras invalidarlo', async () => {
    const fetchMock = mockFetch(() => new Response(null, { status: 204 }))

    await postJson('/a', {})
    await postJson('/b', {})
    expect(callsTo(fetchMock, '/api/auth/antiforgery')).toHaveLength(1)

    invalidateAntiforgeryToken()
    await postJson('/c', {})
    expect(callsTo(fetchMock, '/api/auth/antiforgery')).toHaveLength(2)
  })

  it('una respuesta 204 devuelve undefined', async () => {
    mockFetch(() => new Response(null, { status: 204 }))

    await expect(postJson('/auth/logout')).resolves.toBeUndefined()
  })
})

describe('core/http — lecturas', () => {
  it('getJson no envía cabecera antiforgery', async () => {
    const fetchMock = mockFetch(() => json({ id: 1 }))

    const result = await getJson<{ id: number }>('/tickets/1')

    expect(result).toEqual({ id: 1 })
    expect(callsTo(fetchMock, '/api/auth/antiforgery')).toHaveLength(0)
    const [[, init]] = callsTo(fetchMock, '/api/tickets/1')
    expect(init?.method).toBe('GET')
    expect(init?.credentials).toBe('same-origin')
    expect(headersOf(init).has('X-XSRF-TOKEN')).toBe(false)
  })

  it('getJson acepta códigos no-2xx declarados como válidos', async () => {
    mockFetch(() => json({ status: 'Unhealthy' }, 503))

    await expect(getJson('/health', { acceptedStatuses: [503] })).resolves.toEqual({ status: 'Unhealthy' })
  })

  it('las rutas son relativas a /api y nunca aceptan URL absolutas', async () => {
    const fetchMock = mockFetch(() => json({}))

    await expect(getJson('http://localhost:8080/api/x')).rejects.toThrow(TypeError)
    await expect(postJson('//evil.example/x', {})).rejects.toThrow(TypeError)
    await expect(getJson('sin-barra')).rejects.toThrow(TypeError)
    expect(fetchMock).not.toHaveBeenCalled()
  })
})

describe('core/http — errores', () => {
  it('problem+json se mapea a HttpError con errores por campo', async () => {
    mockFetch(() =>
      problem(
        {
          type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
          title: 'Uno o más campos no son válidos.',
          status: 400,
          errors: { email: ['El correo no tiene un formato válido.'] },
          traceId: '00-abc-01',
        },
        400,
      ),
    )

    const error = await postJson('/auth/login', { email: 'x' }).catch((caught: unknown) => caught)

    expect(error).toBeInstanceOf(HttpError)
    const httpError = error as HttpError
    expect(httpError.status).toBe(400)
    expect(httpError.problem?.title).toBe('Uno o más campos no son válidos.')
    expect(httpError.problem?.errors?.email?.[0]).toBe('El correo no tiene un formato válido.')
    expect(httpError.problem?.traceId).toBe('00-abc-01')
  })

  it('descarta campos mal formados del ProblemDetails', async () => {
    mockFetch(() => problem({ title: 42, errors: { email: 'no-es-lista', password: ['ok', 7] } }, 400))

    const error = (await getJson('/x').catch((caught: unknown) => caught)) as HttpError

    expect(error.problem?.title).toBeUndefined()
    expect(error.problem?.errors).toEqual({ password: ['ok'] })
  })

  it('502 con cuerpo HTML produce HttpError sin problem y mensaje genérico', async () => {
    mockFetch(
      () => new Response('<html><body>Bad Gateway</body></html>', { status: 502, headers: { 'Content-Type': 'text/html' } }),
    )

    const error = (await getJson('/x').catch((caught: unknown) => caught)) as HttpError

    expect(error).toBeInstanceOf(HttpError)
    expect(error.status).toBe(502)
    expect(error.problem).toBeUndefined()
    expect(error.message).toBe(GENERIC_ERROR_MESSAGE)
    expect(userMessageFor(error)).toBe('No pudimos completar la operación. Inténtalo de nuevo.')
  })

  it('un 5xx con problem+json también es genérico', async () => {
    mockFetch(() => problem({ title: 'NullReferenceException en Foo' }, 500))

    const error = (await getJson('/x').catch((caught: unknown) => caught)) as HttpError

    expect(error.problem).toBeUndefined()
    expect(userMessageFor(error)).toBe(GENERIC_ERROR_MESSAGE)
  })

  it('un 4xx con cuerpo vacío es genérico', async () => {
    mockFetch(() => new Response(null, { status: 404 }))

    const error = (await getJson('/x').catch((caught: unknown) => caught)) as HttpError

    expect(error.status).toBe(404)
    expect(error.problem).toBeUndefined()
    expect(userMessageFor(error)).toBe(GENERIC_ERROR_MESSAGE)
  })

  it('401 invoca el manejador global', async () => {
    const onUnauthorized = vi.fn()
    setUnauthorizedHandler(onUnauthorized)
    mockFetch(() => problem({ title: 'Se requiere iniciar sesión.', status: 401 }, 401))

    const error = (await getJson('/tickets').catch((caught: unknown) => caught)) as HttpError

    expect(onUnauthorized).toHaveBeenCalledTimes(1)
    expect(error.status).toBe(401)
  })

  it('401 no invoca el manejador global cuando la petición lo desactiva', async () => {
    const onUnauthorized = vi.fn()
    setUnauthorizedHandler(onUnauthorized)
    mockFetch(() => problem({ title: 'Correo o contraseña incorrectos.', status: 401 }, 401))

    const error = (await postJson('/auth/login', {}, { notifyAuthErrors: false }).catch(
      (caught: unknown) => caught,
    )) as HttpError

    expect(onUnauthorized).not.toHaveBeenCalled()
    expect(userMessageFor(error)).toBe('Correo o contraseña incorrectos.')
  })

  it('403 invoca el manejador global de acceso denegado', async () => {
    const onForbidden = vi.fn()
    const onUnauthorized = vi.fn()
    setForbiddenHandler(onForbidden)
    setUnauthorizedHandler(onUnauthorized)
    mockFetch(() => problem({ title: 'No tienes permiso para realizar esta acción.', status: 403 }, 403))

    await expect(getJson('/admin/usuarios')).rejects.toBeInstanceOf(HttpError)

    expect(onForbidden).toHaveBeenCalledTimes(1)
    expect(onUnauthorized).not.toHaveBeenCalled()
  })

  it('404 y 409 llegan como HttpError tipado sin invocar manejadores globales', async () => {
    const onForbidden = vi.fn()
    const onUnauthorized = vi.fn()
    setForbiddenHandler(onForbidden)
    setUnauthorizedHandler(onUnauthorized)
    mockFetch((url) =>
      url.endsWith('/404')
        ? problem({ title: 'No encontrado.', status: 404 }, 404)
        : problem({ title: 'Conflicto.', detail: 'El ticket ya está cerrado.', status: 409 }, 409),
    )

    const notFound = (await getJson('/404').catch((caught: unknown) => caught)) as HttpError
    const conflict = (await postJson('/409', {}).catch((caught: unknown) => caught)) as HttpError

    expect(notFound.status).toBe(404)
    expect(conflict.status).toBe(409)
    expect(userMessageFor(conflict)).toBe('El ticket ya está cerrado.')
    expect(onForbidden).not.toHaveBeenCalled()
    expect(onUnauthorized).not.toHaveBeenCalled()
  })

  it('al darse de baja, un manejador antiguo no borra al vigente', async () => {
    const first = vi.fn()
    const second = vi.fn()
    const unregisterFirst = setUnauthorizedHandler(first)
    setUnauthorizedHandler(second)
    unregisterFirst()
    mockFetch(() => problem({ status: 401 }, 401))

    await getJson('/x').catch(() => undefined)

    expect(first).not.toHaveBeenCalled()
    expect(second).toHaveBeenCalledTimes(1)
  })
})

describe('core/http — token antiforgery obsoleto', () => {
  function stubWithTokens(respond: (url: string, token: string | null, call: number) => Response) {
    let tokens = 0
    let calls = 0
    const fetchMock = vi.fn(async (input: RequestInfo | URL, init: RequestInit = {}) => {
      const url = String(input)
      if (url === '/api/auth/antiforgery') {
        tokens += 1
        return json({ headerName: 'X-XSRF-TOKEN', token: `t-${tokens}` })
      }
      calls += 1
      return respond(url, headersOf(init).get('X-XSRF-TOKEN'), calls)
    })
    vi.stubGlobal('fetch', fetchMock)
    return fetchMock
  }

  const antiforgeryInvalid = () =>
    problem({ title: 'La solicitud no es válida.', status: 400, code: 'antiforgery_invalid' }, 400)

  it('ante antiforgery_invalid pide un token nuevo y reintenta una sola vez', async () => {
    const fetchMock = stubWithTokens((_url, _token, call) =>
      call === 1 ? antiforgeryInvalid() : json({ ok: true }),
    )

    await expect(postJson('/x', { a: 1 })).resolves.toEqual({ ok: true })

    expect(callsTo(fetchMock, '/api/auth/antiforgery')).toHaveLength(2)
    expect(callsTo(fetchMock, '/api/x').map(([, init]) => headersOf(init).get('X-XSRF-TOKEN'))).toEqual(['t-1', 't-2'])
    expect(callsTo(fetchMock, '/api/x').map(([, init]) => init?.body)).toEqual([
      JSON.stringify({ a: 1 }),
      JSON.stringify({ a: 1 }),
    ])
  })

  it('si el reintento vuelve a fallar propaga el error sin más intentos', async () => {
    const fetchMock = stubWithTokens(() => antiforgeryInvalid())

    const error = (await postJson('/x', {}).catch((caught: unknown) => caught)) as HttpError

    expect(error.status).toBe(400)
    expect(error.problem?.code).toBe('antiforgery_invalid')
    expect(callsTo(fetchMock, '/api/x')).toHaveLength(2)
  })

  it('no reintenta otros 400', async () => {
    const fetchMock = stubWithTokens(() => problem({ status: 400, errors: { email: ['x'] } }, 400))

    await expect(postJson('/x', {})).rejects.toBeInstanceOf(HttpError)

    expect(callsTo(fetchMock, '/api/x')).toHaveLength(1)
    expect(callsTo(fetchMock, '/api/auth/antiforgery')).toHaveLength(1)
  })

  it('una petición fallida de antiforgery no queda en caché', async () => {
    let antiforgeryCalls = 0
    const fetchMock = vi.fn(async (input: RequestInfo | URL, _init?: RequestInit) => {
      if (String(input) === '/api/auth/antiforgery') {
        antiforgeryCalls += 1
        return antiforgeryCalls === 1
          ? new Response('<html></html>', { status: 503, headers: { 'Content-Type': 'text/html' } })
          : json(ANTIFORGERY)
      }
      return new Response(null, { status: 204 })
    })
    vi.stubGlobal('fetch', fetchMock)

    await expect(postJson('/x', {})).rejects.toBeInstanceOf(HttpError)
    await expect(postJson('/x', {})).resolves.toBeUndefined()

    expect(antiforgeryCalls).toBe(2)
  })
})

describe('core/http — validación de la ruta', () => {
  it('rechaza segmentos .. o . (también codificados) antes de llamar a fetch', async () => {
    const fetchMock = mockFetch(() => json({}))

    await expect(getJson('/tickets/../admin')).rejects.toThrow(TypeError)
    await expect(getJson('/tickets/%2e%2e/admin')).rejects.toThrow(TypeError)
    await expect(postJson('/./x', {})).rejects.toThrow(TypeError)
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('solo valida el path: la query puede contener URL o barras invertidas', async () => {
    const fetchMock = mockFetch(() => json({ ok: true }))

    await getJson('/tickets?origen=https://ejemplo.test/a\b#seccion')

    expect(String(fetchMock.mock.calls[0]?.[0])).toBe('/api/tickets?origen=https://ejemplo.test/a\b#seccion')
  })
})
