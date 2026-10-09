/**
 * Manejadores globales de 401 y 403. Los registra el controlador de sesión; el cliente HTTP
 * los invoca ante cualquier respuesta 401/403 de `/api` (salvo que la petición lo desactive).
 */
type AuthEventHandler = () => void

let unauthorizedHandler: AuthEventHandler | null = null
let forbiddenHandler: AuthEventHandler | null = null

/** Registra el manejador de 401. Devuelve la baja, que solo lo retira si sigue vigente. */
export function setUnauthorizedHandler(handler: AuthEventHandler | null): () => void {
  unauthorizedHandler = handler
  return () => {
    if (unauthorizedHandler === handler) unauthorizedHandler = null
  }
}

/** Registra el manejador de 403. Devuelve la baja, que solo lo retira si sigue vigente. */
export function setForbiddenHandler(handler: AuthEventHandler | null): () => void {
  forbiddenHandler = handler
  return () => {
    if (forbiddenHandler === handler) forbiddenHandler = null
  }
}

export function notifyUnauthorized(): void {
  unauthorizedHandler?.()
}

export function notifyForbidden(): void {
  forbiddenHandler?.()
}
