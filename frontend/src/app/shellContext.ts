import { useOutletContext } from 'react-router'
import type { CurrentUser } from '../modules/session/models/currentUser'

/** Contexto que las rutas autenticadas reciben de la guarda de sesión. */
export interface ShellContext {
  readonly user: CurrentUser
}

export function useShellUser(): CurrentUser {
  return useOutletContext<ShellContext>().user
}
