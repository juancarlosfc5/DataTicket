import { useCallback, useEffect, useRef, useState } from 'react'
import { GENERIC_ERROR_MESSAGE, HttpError } from '../../../core/http/httpError'
import { logout } from '../models/authGateway'

const UNAUTHORIZED = 401

export interface LogoutControllerOptions {
  /** Descarta la sesión en memoria (la guarda lleva entonces a `/ingresar`). */
  readonly onSignedOut: () => void
  /** Usuario de la sesión actual (`null` sin sesión). Al cambiar, se olvida el aviso de un fallo anterior. */
  readonly userId: string | null
}

export interface LogoutController {
  readonly signOut: () => Promise<void>
  readonly isSigningOut: boolean
  readonly errorMessage: string | null
}

/**
 * «Cerrar sesión»: `POST /api/auth/logout` y, si responde 204 o 401 (ya no había sesión),
 * descarta la sesión local. Otros fallos conservan la sesión y muestran el mensaje genérico.
 */
export function useLogoutController({ onSignedOut, userId }: LogoutControllerOptions): LogoutController {
  const [isSigningOut, setIsSigningOut] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [sessionUserId, setSessionUserId] = useState(userId)

  // Cambio de sesión (cierre, caducidad o nueva entrada): el aviso anterior ya no aplica.
  // Ajuste de estado durante el render (patrón de React para reaccionar a una prop), sin efecto.
  if (sessionUserId !== userId) {
    setSessionUserId(userId)
    setErrorMessage(null)
  }
  const inFlightRef = useRef(false)
  const onSignedOutRef = useRef(onSignedOut)

  useEffect(() => {
    onSignedOutRef.current = onSignedOut
  })

  const signOut = useCallback(async () => {
    if (inFlightRef.current) return
    inFlightRef.current = true
    setIsSigningOut(true)
    setErrorMessage(null)
    try {
      await logout()
      onSignedOutRef.current()
    } catch (error) {
      if (error instanceof HttpError && error.status === UNAUTHORIZED) onSignedOutRef.current()
      else setErrorMessage(GENERIC_ERROR_MESSAGE)
    } finally {
      inFlightRef.current = false
      setIsSigningOut(false)
    }
  }, [])

  return { signOut, isSigningOut, errorMessage }
}
