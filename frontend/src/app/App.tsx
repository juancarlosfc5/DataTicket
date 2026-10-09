import { useNavigate } from 'react-router'
import { useSessionController } from '../modules/session/controllers/useSessionController'
import { FORBIDDEN_PATH } from '../modules/session/models/navigation'
import { AppRoutes } from './AppRoutes'

/** Shell de DataTicket. Necesita un enrutador por encima (`BrowserRouter` en `main.tsx`). */
export function App() {
  const navigate = useNavigate()
  const session = useSessionController({
    onForbidden: () => navigate(FORBIDDEN_PATH, { replace: true }),
  })

  // Punto de conexión del cierre de sesión. B5 solo descarta la sesión en memoria (la guarda lleva
  // a /ingresar); HU-003 (bloque B6) lo sustituye por el controlador de `auth`, que llama antes a
  // POST /api/auth/logout y expone `isSigningOut`.
  return <AppRoutes session={session} onSignOut={session.endSession} isSigningOut={false} />
}
