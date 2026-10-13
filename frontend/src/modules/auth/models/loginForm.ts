import type { ProblemDetails } from '../../../core/http/problemDetails'

export type LoginField = 'email' | 'password'

export interface LoginInput {
  readonly email: string
  readonly password: string
}

/** Primer mensaje de error por campo; un campo sin clave no tiene error. */
export type LoginFieldErrors = Readonly<Partial<Record<LoginField, string>>>

/** Mismo límite que el backend (`POST /api/auth/login`, contrato de HU-003). */
export const EMAIL_MAX_LENGTH = 256
/** Una sola `@`, partes no vacías y sin espacios. El formato definitivo lo valida el backend. */
const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+$/

export const LOGIN_FIELD_MESSAGES = {
  emailRequired: 'Escribe tu correo.',
  emailFormat: 'Escribe un correo válido, como nombre@empresa.com.',
  emailTooLong: `El correo no puede superar ${EMAIL_MAX_LENGTH} caracteres.`,
  passwordRequired: 'Escribe tu contraseña.',
} as const

const FIELDS: readonly LoginField[] = ['email', 'password']

/** Cuerpo de la petición: el correo sin espacios alrededor; la contraseña tal cual. */
export function toLoginRequest(input: LoginInput): LoginInput {
  return { email: input.email.trim(), password: input.password }
}

function emailError(rawEmail: string): string | undefined {
  const email = rawEmail.trim()
  if (email.length === 0) return LOGIN_FIELD_MESSAGES.emailRequired
  if (email.length > EMAIL_MAX_LENGTH) return LOGIN_FIELD_MESSAGES.emailTooLong
  if (!EMAIL_PATTERN.test(email)) return LOGIN_FIELD_MESSAGES.emailFormat
  return undefined
}

/** Validación local antes de llamar al backend (no sustituye a la del servidor). */
export function validateLogin(input: LoginInput): LoginFieldErrors {
  const email = emailError(input.email)
  const password = input.password.length === 0 ? LOGIN_FIELD_MESSAGES.passwordRequired : undefined
  return {
    ...(email ? { email } : {}),
    ...(password ? { password } : {}),
  }
}

export function hasFieldErrors(errors: LoginFieldErrors): boolean {
  return FIELDS.some((field) => errors[field] !== undefined)
}

/** Primer campo con error, en el orden del formulario. */
export function firstInvalidField(errors: LoginFieldErrors): LoginField | null {
  return FIELDS.find((field) => errors[field] !== undefined) ?? null
}

/** Errores por campo de un 400 ProblemDetails (`errors.email`, `errors.Email`…). */
export function fieldErrorsFromProblem(problem: ProblemDetails | undefined): LoginFieldErrors {
  const entries = Object.entries(problem?.errors ?? {})
  const result: Partial<Record<LoginField, string>> = {}
  for (const field of FIELDS) {
    const match = entries.find(([key, messages]) => key.toLowerCase() === field && messages.length > 0)
    if (match) result[field] = match[1][0]
  }
  return result
}
