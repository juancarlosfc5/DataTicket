import { useNavigate } from 'react-router'
import { useLogoutController } from '../modules/auth/controllers/useLogoutController'
import { useSessionController } from '../modules/session/controllers/useSessionController'
import { FORBIDDEN_PATH } from '../modules/session/models/navigation'
import { AppRoutes } from './AppRoutes'

/** Shell de DataTicket. Necesita un enrutador por encima (`BrowserRouter` en `main.tsx`). */
export function App() {
  const navigate = useNavigate()
  const session = useSessionController({
    onForbidden: () => navigate(FORBIDDEN_PATH, { replace: true }),
  })
  // «Cerrar sesión»: POST /api/auth/logout y, después, descartar la sesión en memoria
  // (la guarda lleva a /ingresar sin returnUrl).
  const signOut = useLogoutController({
    onSignedOut: session.endSession,
    userId: session.state.kind === 'authenticated' ? session.state.user.id : null,
  })

  return <AppRoutes session={session} signOut={signOut} />
}
