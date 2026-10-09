import { Outlet, useLocation, useNavigate } from 'react-router'
import type { CurrentUser } from '../modules/session/models/currentUser'
import { userInitials } from '../modules/session/models/currentUser'
import { currentNavigationId, navigationFor } from '../modules/session/models/navigation'
import { AppLayout } from '../modules/session/views/AppLayout'
import { useShellUser, type ShellContext } from './shellContext'

interface AuthenticatedShellProps {
  readonly onSignOut: () => void
  readonly isSigningOut: boolean
}

const SURFACE_LABEL = { client: 'Portal del cliente', internal: 'Espacio interno', none: 'Sin acceso' } as const

function userDetailFor(user: CurrentUser): string {
  if (user.surface === 'internal') return 'Data Global'
  return user.company?.name ?? ''
}

/** Conecta la sesión con el layout: la superficie del layout es siempre la del usuario. */
export function AuthenticatedShell({ onSignOut, isSigningOut }: AuthenticatedShellProps) {
  const user = useShellUser()
  const location = useLocation()
  const navigate = useNavigate()
  const navigation = navigationFor(user)
  const context: ShellContext = { user }

  return (
    <AppLayout
      variant={user.surface === 'internal' ? 'internal' : 'portal'}
      surfaceLabel={SURFACE_LABEL[user.surface]}
      userName={user.displayName}
      userDetail={userDetailFor(user)}
      userInitials={userInitials(user.displayName)}
      navigation={navigation}
      currentItemId={currentNavigationId(navigation, location.pathname)}
      onNavigate={navigate}
      onSignOut={onSignOut}
      isSigningOut={isSigningOut}
    >
      <Outlet context={context} />
    </AppLayout>
  )
}
