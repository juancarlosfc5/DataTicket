import { Navigate, Route, Routes } from 'react-router'
import type { LogoutController } from '../modules/auth/controllers/useLogoutController'
import type { SessionController } from '../modules/session/controllers/useSessionController'
import type { SectionId } from '../modules/session/models/navigation'
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
  readonly signOut: LogoutController
}

/** Secciones protegidas: cada ruta declara su sección y la guarda decide sobre ella. */
const SECTION_ROUTES: readonly { readonly path: string; readonly section: SectionId }[] = [
  { path: 'portal', section: 'portal-home' },
  { path: 'portal/tickets', section: 'portal-tickets' },
  { path: 'portal/radicar', section: 'portal-submit' },
  { path: 'interno', section: 'internal-home' },
  { path: 'interno/bandeja', section: 'internal-inbox' },
  { path: 'interno/triage', section: 'internal-triage' },
  { path: 'interno/administracion', section: 'internal-admin' },
  { path: 'interno/estado', section: 'internal-status' },
]

function sectionElement(section: SectionId) {
  return section === 'internal-status' ? <SystemStatusPage /> : <SectionPage section={section} />
}

/**
 * Rutas del SPA (contrato en `wiki/arquitectura/autenticacion-identity.md` § «Rutas del SPA»).
 * React Router 8 en modo librería (ADR-0010): sin `loader`/`action`; los datos los piden los controladores.
 */
export function AppRoutes({ session, signOut }: AppRoutesProps) {
  return (
    <Routes>
      <Route path="/ingresar" element={<SignInPage session={session} />} />
      <Route path="/activar-cuenta" element={<PublicPlaceholderPage title="Activar cuenta" />} />
      <Route path="/olvide-mi-contrasena" element={<PublicPlaceholderPage title="Recuperar contraseña" />} />
      <Route path="/restablecer-contrasena" element={<PublicPlaceholderPage title="Restablecer contraseña" />} />

      <Route element={<RequireSession session={session} />}>
        <Route element={<AuthenticatedShell signOut={signOut} />}>
          <Route index element={<SurfaceRedirect />} />
          <Route path="sin-permiso" element={<ForbiddenPage />} />
          {SECTION_ROUTES.map(({ path, section }) => (
            <Route
              key={path}
              path={path}
              element={<RequireAccess section={section}>{sectionElement(section)}</RequireAccess>}
            />
          ))}
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
