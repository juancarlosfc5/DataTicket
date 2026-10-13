import { ResponseFormatError, toHttpError } from './httpError'

const ANTIFORGERY_PATH = '/api/auth/antiforgery'
/** Nombre de cabecera HTTP válido (token RFC 9110). */
const HEADER_NAME_PATTERN = /^[A-Za-z0-9!#$%&'*+.^_`|~-]+$/

export interface AntiforgeryToken {
  readonly headerName: string
  readonly token: string
}

/** Caché solo en memoria (nunca en Storage). Depende de la identidad: se invalida tras login y logout. */
let cachedToken: Promise<AntiforgeryToken> | null = null

function toAntiforgeryToken(value: unknown): AntiforgeryToken {
  if (typeof value !== 'object' || value === null) throw new ResponseFormatError()
  const { headerName, token } = value as { headerName?: unknown; token?: unknown }
  if (typeof headerName !== 'string' || !HEADER_NAME_PATTERN.test(headerName)) throw new ResponseFormatError()
  if (typeof token !== 'string' || token.length === 0) throw new ResponseFormatError()
  return { headerName, token }
}

async function requestToken(): Promise<AntiforgeryToken> {
  const response = await fetch(ANTIFORGERY_PATH, {
    method: 'GET',
    headers: { Accept: 'application/json' },
    credentials: 'same-origin',
    cache: 'no-store',
  })
  if (!response.ok) throw await toHttpError(response)

  try {
    return toAntiforgeryToken(await response.json())
  } catch {
    throw new ResponseFormatError()
  }
}

/** Devuelve el token vigente; lo pide una sola vez y lo reutiliza mientras no se invalide. */
export function getAntiforgeryToken(): Promise<AntiforgeryToken> {
  if (!cachedToken) {
    const pending = requestToken()
    cachedToken = pending
    pending.catch(() => {
      if (cachedToken === pending) cachedToken = null
    })
  }
  return cachedToken
}

/** Olvida el token: llámalo tras iniciar o cerrar sesión, porque el token depende de la identidad. */
export function invalidateAntiforgeryToken(): void {
  cachedToken = null
}
