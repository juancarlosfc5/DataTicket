import { useCallback, useEffect, useReducer, useRef } from 'react'
import { GENERIC_ERROR_MESSAGE, HttpError } from '../../../core/http/httpError'
import { login } from '../models/authGateway'
import { fieldErrorsFromProblem, hasFieldErrors, validateLogin } from '../models/loginForm'
import { INITIAL_LOGIN_STATE, loginReducer, type LoginEvent, type LoginState } from '../models/loginState'

const BAD_REQUEST = 400
const UNAUTHORIZED = 401

export interface LoginControllerOptions {
  /** Tras el 204: la app refresca la sesión y la ruta lleva al `returnUrl` saneado. */
  readonly onSignedIn: () => void
}

export interface LoginController {
  readonly state: LoginState
  readonly changeEmail: (value: string) => void
  readonly changePassword: (value: string) => void
  readonly submit: () => Promise<void>
}

/** Traduce el fallo del login a un evento del formulario, sin revelar si la cuenta existe. */
function failureEvent(error: unknown): LoginEvent {
  if (error instanceof HttpError && error.status === UNAUTHORIZED) return { type: 'rejected' }
  if (error instanceof HttpError && error.status === BAD_REQUEST) {
    const fieldErrors = fieldErrorsFromProblem(error.problem)
    if (hasFieldErrors(fieldErrors)) return { type: 'invalid', fieldErrors }
  }
  return { type: 'failed', message: GENERIC_ERROR_MESSAGE }
}

/** Controlador de `/ingresar`: validación local, envío y traducción de errores. */
export function useLoginController({ onSignedIn }: LoginControllerOptions): LoginController {
  const [state, dispatch] = useReducer(loginReducer, INITIAL_LOGIN_STATE)
  const stateRef = useRef(state)
  const inFlightRef = useRef<AbortController | null>(null)
  const onSignedInRef = useRef(onSignedIn)

  useEffect(() => {
    stateRef.current = state
    onSignedInRef.current = onSignedIn
  })

  useEffect(() => () => inFlightRef.current?.abort(), [])

  const changeEmail = useCallback((value: string) => dispatch({ type: 'emailChanged', value }), [])
  const changePassword = useCallback((value: string) => dispatch({ type: 'passwordChanged', value }), [])

  const submit = useCallback(async () => {
    if (inFlightRef.current) return
    const { email, password } = stateRef.current
    const fieldErrors = validateLogin({ email, password })
    if (hasFieldErrors(fieldErrors)) {
      dispatch({ type: 'validationFailed', fieldErrors })
      return
    }

    const controller = new AbortController()
    inFlightRef.current = controller
    dispatch({ type: 'submitStarted' })
    try {
      await login({ email, password }, controller.signal)
      if (!controller.signal.aborted) onSignedInRef.current()
    } catch (error) {
      if (!controller.signal.aborted) dispatch(failureEvent(error))
    } finally {
      if (inFlightRef.current === controller) inFlightRef.current = null
    }
  }, [])

  return { state, changeEmail, changePassword, submit }
}
