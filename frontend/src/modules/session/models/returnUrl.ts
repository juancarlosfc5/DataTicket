export const SIGN_IN_PATH = '/ingresar'
const NON_RETURNABLE_PATHS: readonly string[] = [SIGN_IN_PATH, '/sin-permiso']

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
  // Ni la entrada (bucle) ni «sin permiso» son destinos útiles tras iniciar sesión.
  const pathname = value.split(/[?#]/, 1)[0].toLowerCase()
  return !NON_RETURNABLE_PATHS.some((path) => pathname === path || pathname.startsWith(`${path}/`))
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
