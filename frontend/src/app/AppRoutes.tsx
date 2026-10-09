import { Navigate, Route, Routes } from 'react-router'
import type { SessionController } from '../modules/session/controllers/useSessionController'
import { AuthenticatedShell } from './AuthenticatedShell'
import { RequireAccess, RequireSession } from './guards'
import {
  ForbiddenPage,
  PublicPlaceholderPage,
  SectionPage,
  SignInPage,
  SurfaceRedirect,
  SystemStatusPage,
} from './pages'

interface AppRoutesProps {
  readonly session: SessionController
  readonly onSignOut: () => void
  readonly isSigningOut: boolean
}

/**
 * Rutas del SPA (contrato en `wiki/arquitectura/autenticacion-identity.md` § «Rutas del SPA»).
 * React Router 8 en modo librería (ADR-0010): sin `loader`/`action`; los datos los piden los controladores.
 */
export function AppRoutes({ session, onSignOut, isSigningOut }: AppRoutesProps) {
  return (
    <Routes>
      <Route path="/ingresar" element={<SignInPage session={session} />} />
      <Route path="/activar-cuenta" element={<PublicPlaceholderPage title="Activar cuenta" />} />
      <Route path="/olvide-mi-contrasena" element={<PublicPlaceholderPage title="Recuperar contraseña" />} />
      <Route path="/restablecer-contrasena" element={<PublicPlaceholderPage title="Restablecer contraseña" />} />

      <Route element={<RequireSession session={session} />}>
        <Route element={<AuthenticatedShell onSignOut={onSignOut} isSigningOut={isSigningOut} />}>
          <Route index element={<SurfaceRedirect />} />
          <Route path="sin-permiso" element={<ForbiddenPage />} />
          <Route element={<RequireAccess />}>
            <Route path="portal" element={<SectionPage section="portal-home" />} />
            <Route path="portal/tickets" element={<SectionPage section="portal-tickets" />} />
            <Route path="portal/radicar" element={<SectionPage section="portal-submit" />} />
            <Route path="interno" element={<SectionPage section="internal-home" />} />
            <Route path="interno/bandeja" element={<SectionPage section="internal-inbox" />} />
            <Route path="interno/triage" element={<SectionPage section="internal-triage" />} />
            <Route path="interno/administracion" element={<SectionPage section="internal-admin" />} />
            <Route path="interno/estado" element={<SystemStatusPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
