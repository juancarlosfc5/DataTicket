import { firstInvalidField, type LoginField, type LoginFieldErrors } from './loginForm'

/** Mensaje único ante cualquier rechazo de credenciales: no revela si la cuenta existe (PRD §5). */
export const INVALID_CREDENTIALS_MESSAGE = 'Correo o contraseña incorrectos.'

export type LoginStatus =
  | { readonly kind: 'idle' }
  | { readonly kind: 'submitting' }
  | { readonly kind: 'error'; readonly message: string | null; readonly fieldErrors: LoginFieldErrors }

/** Adónde llevar el foco tras un fallo; `attempt` distingue peticiones sucesivas al mismo destino. */
export interface FocusRequest {
  readonly target: LoginField | 'alert'
  readonly attempt: number
}

export interface LoginState {
  readonly email: string
  readonly password: string
  readonly status: LoginStatus
  /** Envíos realizados (incluidos los que la validación local detuvo). */
  readonly attempt: number
  readonly focus: FocusRequest | null
}

export type LoginEvent =
  | { readonly type: 'emailChanged'; readonly value: string }
  | { readonly type: 'passwordChanged'; readonly value: string }
  | { readonly type: 'validationFailed'; readonly fieldErrors: LoginFieldErrors }
  | { readonly type: 'submitStarted' }
  /** 401: credenciales rechazadas. */
  | { readonly type: 'rejected' }
  /** 400 con errores por campo. */
  | { readonly type: 'invalid'; readonly fieldErrors: LoginFieldErrors }
  /** Cualquier otro fallo (5xx, red, 400 sin campos). */
  | { readonly type: 'failed'; readonly message: string }

export const INITIAL_LOGIN_STATE: LoginState = {
  email: '',
  password: '',
  status: { kind: 'idle' },
  attempt: 0,
  focus: null,
}

function withoutFieldError(status: LoginStatus, field: LoginField): LoginStatus {
  if (status.kind !== 'error' || status.fieldErrors[field] === undefined) return status
  const fieldErrors = Object.fromEntries(Object.entries(status.fieldErrors).filter(([key]) => key !== field))
  return { ...status, fieldErrors }
}

function fieldErrorState(state: LoginState, fieldErrors: LoginFieldErrors, attempt: number): LoginState {
  const target = firstInvalidField(fieldErrors)
  return {
    ...state,
    attempt,
    status: { kind: 'error', message: null, fieldErrors },
    focus: target ? { target, attempt } : null,
  }
}

/** Reductor puro del formulario de entrada. */
export function loginReducer(state: LoginState, event: LoginEvent): LoginState {
  switch (event.type) {
    case 'emailChanged':
      return { ...state, email: event.value, status: withoutFieldError(state.status, 'email') }
    case 'passwordChanged':
      return { ...state, password: event.value, status: withoutFieldError(state.status, 'password') }
    case 'validationFailed':
      return fieldErrorState(state, event.fieldErrors, state.attempt + 1)
    case 'submitStarted':
      return { ...state, attempt: state.attempt + 1, status: { kind: 'submitting' }, focus: null }
    case 'rejected':
      return {
        ...state,
        password: '',
        status: { kind: 'error', message: INVALID_CREDENTIALS_MESSAGE, fieldErrors: {} },
        focus: { target: 'password', attempt: state.attempt },
      }
    case 'invalid':
      return fieldErrorState(state, event.fieldErrors, state.attempt)
    case 'failed':
      return {
        ...state,
        status: { kind: 'error', message: event.message, fieldErrors: {} },
        focus: { target: 'alert', attempt: state.attempt },
      }
  }
}
