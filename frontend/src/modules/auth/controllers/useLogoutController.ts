import { useCallback, useEffect, useRef, useState } from 'react'
import { GENERIC_ERROR_MESSAGE, HttpError } from '../../../core/http/httpError'
import { logout } from '../models/authGateway'

const UNAUTHORIZED = 401

export interface LogoutControllerOptions {
  /** Descarta la sesión en memoria (la guarda lleva entonces a `/ingresar`). */
  readonly onSignedOut: () => void
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
export function useLogoutController({ onSignedOut }: LogoutControllerOptions): LogoutController {
  const [isSigningOut, setIsSigningOut] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
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
