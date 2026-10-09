import { invalidateAntiforgeryToken } from '../../../core/http/antiforgery'
import { postJson } from '../../../core/http/httpClient'
import { toLoginRequest, type LoginInput } from './loginForm'

/**
 * `POST /api/auth/login` (contrato de HU-003). Su 401 es una respuesta esperada (credenciales
 * malas), así que no dispara el manejador global. El token antiforgery depende de la identidad:
 * se invalida al terminar, para pedir uno nuevo en la siguiente mutación.
 */
export async function login(input: LoginInput, signal?: AbortSignal): Promise<void> {
  try {
    await postJson('/auth/login', toLoginRequest(input), { signal, notifyAuthErrors: false })
  } finally {
    invalidateAntiforgeryToken()
  }
}

/** `POST /api/auth/logout`. Un 401 (sesión ya caducada) lo resuelve el llamador. */
export async function logout(signal?: AbortSignal): Promise<void> {
  try {
    await postJson('/auth/logout', undefined, { signal, notifyAuthErrors: false })
  } finally {
    invalidateAntiforgeryToken()
  }
}
