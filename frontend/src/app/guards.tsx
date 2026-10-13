import type { ReactNode } from 'react'
import { Navigate, Outlet, useLocation } from 'react-router'
import type { SessionController } from '../modules/session/controllers/useSessionController'
import { canAccessSection, type SectionId } from '../modules/session/models/navigation'
import { SIGN_IN_PATH, signInPathFor } from '../modules/session/models/returnUrl'
import { SessionErrorView } from '../modules/session/views/SessionErrorView'
import { LoadingView } from '../shared/ui/LoadingView'
import { ForbiddenPage } from './pages'
import { useShellUser, type ShellContext } from './shellContext'

interface RequireSessionProps {
  readonly session: SessionController
}

/**
 * Guarda de las rutas autenticadas. Sin sesión (o tras un 401) lleva a
 * `/ingresar?returnUrl=<ruta actual>`. Es UX: la autorización real la aplica el backend.
 */
export function RequireSession({ session }: RequireSessionProps) {
  const location = useLocation()
  const { state } = session

  switch (state.kind) {
    case 'loading':
      return <LoadingView message="Cargando tu sesión…" />
    case 'error':
      return <SessionErrorView message={state.message} onRetry={session.refresh} />
    case 'anonymous': {
      // Tras un cierre de sesión explícito no se recuerda la ruta; tras un 401, sí.
      const target =
        state.reason === 'signedOut'
          ? SIGN_IN_PATH
          : signInPathFor(`${location.pathname}${location.search}${location.hash}`)
      return <Navigate to={target} replace />
    }
    case 'authenticated': {
      const context: ShellContext = { user: state.user }
      return <Outlet context={context} />
    }
  }
}

interface RequireAccessProps {
  /** Sección declarada por la ruta; nunca se deduce de la URL (que puede venir codificada). */
  readonly section: SectionId
  readonly children: ReactNode
}

/** Superficie ajena o sección restringida por rol → «sin permiso», sin montar la sección. */
export function RequireAccess({ section, children }: RequireAccessProps) {
  const user = useShellUser()
  return canAccessSection(user, section) ? children : <ForbiddenPage />
}
