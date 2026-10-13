import { getAntiforgeryToken, invalidateAntiforgeryToken } from './antiforgery'
import { notifyForbidden, notifyUnauthorized } from './authEvents'
import { ResponseFormatError, toHttpError, type HttpError } from './httpError'

export { HttpError, GENERIC_ERROR_MESSAGE, userMessageFor } from './httpError'
export type { ProblemDetails } from './problemDetails'

const API_BASE_PATH = '/api'
const UNAUTHORIZED = 401
const FORBIDDEN = 403
const BAD_REQUEST = 400
/** Extensión `code` del ProblemDetails cuando la cabecera antiforgery falta o caducó. */
const ANTIFORGERY_INVALID = 'antiforgery_invalid'

export interface RequestOptions {
  readonly signal?: AbortSignal
  /**
   * `false` evita invocar los manejadores globales de 401/403. Lo usan las peticiones cuyo
   * 401 es una respuesta esperada (p. ej. `/auth/me` al arrancar o el login con credenciales malas).
   */
  readonly notifyAuthErrors?: boolean
}

export interface GetJsonOptions extends RequestOptions {
  /** Códigos no-2xx cuyo cuerpo JSON también es una respuesta válida (p. ej. 503 en /health). */
  readonly acceptedStatuses?: readonly number[]
}

type MutationMethod = 'POST' | 'PUT' | 'PATCH' | 'DELETE'

function decodeSegment(segment: string): string {
  try {
    return decodeURIComponent(segment)
  } catch {
    return segment
  }
}

/** Valida solo el path (lo anterior a `?`/`#`): la query puede llevar cualquier valor. */
function isSafeApiPath(pathname: string): boolean {
  if (!pathname.startsWith('/') || pathname.startsWith('//')) return false
  if (pathname.includes('\\') || pathname.includes('://')) return false
  return pathname.split('/').every((segment) => {
    const decoded = decodeSegment(segment)
    return decoded !== '..' && decoded !== '.'
  })
}

/** Solo rutas relativas a `/api` (`/tickets`, no `http://…`, `//host` ni `..`): todo es same-origin. */
function toApiUrl(path: string): string {
  const pathname = path.split(/[?#]/, 1)[0]
  if (!isSafeApiPath(pathname)) {
    throw new TypeError(`Ruta de API no válida: debe ser relativa a ${API_BASE_PATH} y empezar por "/".`)
  }
  return `${API_BASE_PATH}${path}`
}

async function readJson<T>(response: Response): Promise<T> {
  const text = await response.text()
  if (text.trim().length === 0) return undefined as T
  try {
    return JSON.parse(text) as T
  } catch {
    throw new ResponseFormatError()
  }
}

function notifyAuthError(error: HttpError, options: RequestOptions): void {
  if (options.notifyAuthErrors === false) return
  if (error.status === UNAUTHORIZED) notifyUnauthorized()
  if (error.status === FORBIDDEN) notifyForbidden()
}

async function failWith(response: Response, options: RequestOptions): Promise<never> {
  const error = await toHttpError(response)
  notifyAuthError(error, options)
  throw error
}

function isAntiforgeryRejection(error: HttpError): boolean {
  return error.status === BAD_REQUEST && error.problem?.code === ANTIFORGERY_INVALID
}

/**
 * GET contra el backend a través del proxy same-origin (/api).
 * `credentials: 'same-origin'` envía la cookie de Identity; no lleva cabecera antiforgery.
 */
export async function getJson<T>(path: string, options: GetJsonOptions = {}): Promise<T> {
  const url = toApiUrl(path)
  const { signal, acceptedStatuses = [] } = options

  const response = await fetch(url, {
    method: 'GET',
    headers: { Accept: 'application/json' },
    credentials: 'same-origin',
    signal,
  })

  if (!response.ok && !acceptedStatuses.includes(response.status)) return failWith(response, options)
  return readJson<T>(response)
}

async function sendMutation<T>(
  method: MutationMethod,
  path: string,
  body: unknown,
  options: RequestOptions,
): Promise<T> {
  const url = toApiUrl(path)
  const payload = body === undefined ? undefined : JSON.stringify(body)

  const send = async (): Promise<Response> => {
    const antiforgery = await getAntiforgeryToken()
    const headers: Record<string, string> = {
      Accept: 'application/json',
      [antiforgery.headerName]: antiforgery.token,
    }
    if (payload !== undefined) headers['Content-Type'] = 'application/json'
    return fetch(url, { method, headers, credentials: 'same-origin', body: payload, signal: options.signal })
  }

  let response = await send()
  if (!response.ok) {
    const error = await toHttpError(response)
    if (!isAntiforgeryRejection(error)) {
      notifyAuthError(error, options)
      throw error
    }
    // Token obsoleto (p. ej. cambió la identidad en otra pestaña): token nuevo y un único reintento.
    invalidateAntiforgeryToken()
    response = await send()
    if (!response.ok) return failWith(response, options)
  }
  return readJson<T>(response)
}

/** POST con cuerpo JSON y cabecera antiforgery. Una respuesta 204 resuelve `undefined`. */
export function postJson<T = void>(path: string, body?: unknown, options: RequestOptions = {}): Promise<T> {
  return sendMutation<T>('POST', path, body, options)
}

export function putJson<T = void>(path: string, body: unknown, options: RequestOptions = {}): Promise<T> {
  return sendMutation<T>('PUT', path, body, options)
}

export function patchJson<T = void>(path: string, body: unknown, options: RequestOptions = {}): Promise<T> {
  return sendMutation<T>('PATCH', path, body, options)
}

export async function deleteRequest(path: string, options: RequestOptions = {}): Promise<void> {
  await sendMutation<void>('DELETE', path, undefined, options)
}
