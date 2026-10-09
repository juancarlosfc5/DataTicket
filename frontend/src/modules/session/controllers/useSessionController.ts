import { useCallback, useEffect, useReducer, useRef, useState } from 'react'
import { invalidateAntiforgeryToken } from '../../../core/http/antiforgery'
import { setForbiddenHandler, setUnauthorizedHandler } from '../../../core/http/authEvents'
import { userMessageFor } from '../../../core/http/httpError'
import { fetchCurrentUser } from '../models/sessionGateway'
import { INITIAL_SESSION_STATE, sessionReducer, type SessionState } from '../models/sessionState'

export interface SessionControllerOptions {
  /** Se invoca cuando cualquier petición a `/api` responde 403 (la app navega a «sin permiso»). */
  readonly onForbidden?: () => void
}

export interface SessionController {
  readonly state: SessionState
  /** Vuelve a consultar `/api/auth/me` (p. ej. tras iniciar sesión o tras un error). */
  readonly refresh: () => void
  /** Descarta la sesión en memoria y el token antiforgery (tras el cierre de sesión en el backend). */
  readonly endSession: () => void
}

/**
 * Controlador de la sesión: carga el usuario actual y registra los manejadores globales.
 * Un 401 de cualquier petición deja la sesión anónima; la guarda de rutas lleva a `/ingresar`.
 */
export function useSessionController(options: SessionControllerOptions = {}): SessionController {
  const [state, dispatch] = useReducer(sessionReducer, INITIAL_SESSION_STATE)
  const [attempt, setAttempt] = useState(0)
  const onForbiddenRef = useRef(options.onForbidden)
  /** `/me` en curso: se aborta si la sesión termina antes, para que no la restaure. */
  const loadRef = useRef<AbortController | null>(null)

  useEffect(() => {
    onForbiddenRef.current = options.onForbidden
  })

  useEffect(() => {
    const controller = new AbortController()
    loadRef.current = controller

    fetchCurrentUser(controller.signal)
      .then((user) => {
        if (!controller.signal.aborted) dispatch({ type: 'loaded', user })
      })
      .catch((error: unknown) => {
        if (!controller.signal.aborted) dispatch({ type: 'loadFailed', message: userMessageFor(error) })
      })

    return () => {
      controller.abort()
      if (loadRef.current === controller) loadRef.current = null
    }
  }, [attempt])

  const discardSession = useCallback((event: 'sessionExpired' | 'signedOut') => {
    loadRef.current?.abort()
    invalidateAntiforgeryToken()
    dispatch({ type: event })
  }, [])

  useEffect(() => setUnauthorizedHandler(() => discardSession('sessionExpired')), [discardSession])

  useEffect(() => setForbiddenHandler(() => onForbiddenRef.current?.()), [])

  const refresh = useCallback(() => {
    dispatch({ type: 'loadStarted' })
    setAttempt((current) => current + 1)
  }, [])

  const endSession = useCallback(() => discardSession('signedOut'), [discardSession])

  return { state, refresh, endSession }
}
