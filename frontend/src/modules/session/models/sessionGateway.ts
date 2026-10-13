import { getJson, HttpError } from '../../../core/http/httpClient'
import { ResponseFormatError } from '../../../core/http/httpError'
import { toCurrentUser, type CurrentUser } from './currentUser'

const UNAUTHORIZED = 401

/**
 * `GET /api/auth/me`. Devuelve `null` si no hay sesión (401): al arrancar es una respuesta
 * esperada, por eso no dispara el manejador global de 401.
 */
export async function fetchCurrentUser(signal?: AbortSignal): Promise<CurrentUser | null> {
  try {
    const dto = await getJson<unknown>('/auth/me', { signal, notifyAuthErrors: false })
    const user = toCurrentUser(dto)
    if (!user) throw new ResponseFormatError()
    return user
  } catch (error) {
    if (error instanceof HttpError && error.status === UNAUTHORIZED) return null
    throw error
  }
}
