import type { CurrentUser } from './currentUser'

/**
 * Por qué no hay sesión: `noSession` (al arrancar), `expired` (un 401 posterior) o `signedOut`
 * (cierre explícito). Tras un cierre explícito la entrada no recuerda la ruta anterior.
 */
export type AnonymousReason = 'noSession' | 'expired' | 'signedOut'

export type SessionState =
  | { readonly kind: 'loading' }
  | { readonly kind: 'anonymous'; readonly reason: AnonymousReason }
  | { readonly kind: 'authenticated'; readonly user: CurrentUser }
  | { readonly kind: 'error'; readonly message: string }

export type SessionEvent =
  | { readonly type: 'loadStarted' }
  | { readonly type: 'loaded'; readonly user: CurrentUser | null }
  | { readonly type: 'loadFailed'; readonly message: string }
  /** Cualquier petición respondió 401: la cookie ya no es válida. */
  | { readonly type: 'sessionExpired' }
  | { readonly type: 'signedOut' }

export const INITIAL_SESSION_STATE: SessionState = { kind: 'loading' }

/** Reductor puro de la sesión en memoria. */
export function sessionReducer(state: SessionState, event: SessionEvent): SessionState {
  switch (event.type) {
    case 'loadStarted':
      // Revalidar una sesión vigente no la oculta (evita parpadeos del shell).
      return state.kind === 'authenticated' ? state : INITIAL_SESSION_STATE
    case 'loaded':
      return event.user ? { kind: 'authenticated', user: event.user } : { kind: 'anonymous', reason: 'noSession' }
    case 'loadFailed':
      return { kind: 'error', message: event.message }
    case 'sessionExpired':
      return { kind: 'anonymous', reason: 'expired' }
    case 'signedOut':
      return { kind: 'anonymous', reason: 'signedOut' }
  }
}
