export const SIGN_IN_PATH = '/ingresar'

const MAX_RETURN_URL_LENGTH = 2048
const FIRST_PRINTABLE = 0x20
const DELETE = 0x7f

/** Caracteres de control (incluidos tabulador y saltos de línea, que el navegador elimina de las URL). */
function hasControlCharacters(value: string): boolean {
  return Array.from(value).some((char) => {
    const code = char.codePointAt(0) ?? 0
    return code < FIRST_PRINTABLE || code === DELETE
  })
}

function isSafeInternalPath(value: string): boolean {
  if (value.length === 0 || value.length > MAX_RETURN_URL_LENGTH) return false
  if (!value.startsWith('/') || value.startsWith('//')) return false
  // `\` lo normalizan los navegadores a `/` (`/\host` → `//host`); los controles se eliminan (`/\t/host`).
  if (value.includes('\\') || hasControlCharacters(value)) return false
  // Evita bucles: volver a la entrada no tiene sentido tras iniciar sesión.
  const pathname = value.split(/[?#]/, 1)[0]
  return pathname !== SIGN_IN_PATH && !pathname.startsWith(`${SIGN_IN_PATH}/`)
}

/**
 * Sanea el `returnUrl` de `/ingresar`: solo admite rutas internas relativas (empiezan por `/`,
 * no por `//` ni `/\`, sin esquema ni caracteres de control). Si no, devuelve `fallback`
 * (la superficie por defecto del usuario). Evita redirecciones abiertas.
 */
export function sanitizeReturnUrl(value: unknown, fallback: string): string {
  return typeof value === 'string' && isSafeInternalPath(value) ? value : fallback
}

/** Ruta de entrada que recuerda adónde volver: `/ingresar?returnUrl=%2Finterno%2F…`. */
export function signInPathFor(returnTo: string): string {
  if (returnTo === '/' || !isSafeInternalPath(returnTo)) return SIGN_IN_PATH
  return `${SIGN_IN_PATH}?returnUrl=${encodeURIComponent(returnTo)}`
}
