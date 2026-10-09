import { Navigate, useNavigate, useSearchParams } from 'react-router'
import type { SessionController } from '../modules/session/controllers/useSessionController'
import type { CurrentUser } from '../modules/session/models/currentUser'
import { homePathFor, SURFACE_HOME, sectionTitle, type SectionId } from '../modules/session/models/navigation'
import { sanitizeReturnUrl } from '../modules/session/models/returnUrl'
import { ForbiddenView } from '../modules/session/views/ForbiddenView'
import { PublicPlaceholderView } from '../modules/session/views/PublicPlaceholderView'
import { SectionPlaceholderView } from '../modules/session/views/SectionPlaceholderView'
import { useHealthController } from '../modules/system/controllers/useHealthController'
import { HealthStatusView } from '../modules/system/views/HealthStatusView'
import { LoadingView } from '../shared/ui/LoadingView'
import { useShellUser } from './shellContext'

/** Textos de los marcadores de posición; cada sección real llega con su HU. */
const SECTION_DESCRIPTIONS: Readonly<Record<SectionId, string>> = {
  'portal-home': 'Desde aquí consultarás tus solicitudes de soporte y radicarás nuevas.',
  'portal-tickets': 'Aquí verás tus tickets y su estado.',
  'portal-submit': 'Aquí podrás radicar un ticket nuevo con sus adjuntos.',
  'internal-home': 'Resumen del trabajo de soporte de Data Global.',
  'internal-inbox': 'Tickets asignados y en curso.',
  'internal-triage': 'Tickets nuevos pendientes de clasificar y asignar.',
  'internal-admin': 'Empresas, usuarios, equipos y plantillas.',
  'internal-status': 'Estado del backend y sus dependencias.',
}

const HOME_LINK_LABEL = { client: 'Ir a mi portal', internal: 'Ir al espacio interno' } as const

function homeLinkFor(user: CurrentUser): { readonly path: string | null; readonly label: string } {
  if (user.surface === 'none') return { path: null, label: '' }
  return { path: SURFACE_HOME[user.surface], label: HOME_LINK_LABEL[user.surface] }
}

/** `/`: cada usuario va a su superficie. */
export function SurfaceRedirect() {
  return <Navigate to={homePathFor(useShellUser())} replace />
}

export function ForbiddenPage() {
  const user = useShellUser()
  const navigate = useNavigate()
  const home = homeLinkFor(user)
  return <ForbiddenView homePath={home.path} homeLabel={home.label} onNavigate={navigate} />
}

export function SectionPage({ section }: { readonly section: SectionId }) {
  const user = useShellUser()
  return <SectionPlaceholderView title={sectionTitle(section, user)} description={SECTION_DESCRIPTIONS[section]} />
}

/** Conserva el módulo `system` (salud del backend) dentro del espacio interno. */
export function SystemStatusPage() {
  const health = useHealthController()
  return (
    <section className="page" aria-labelledby="page-title">
      <div className="page__head">
        <h1 id="page-title" className="page__title">
          Estado del sistema
        </h1>
      </div>
      <HealthStatusView state={health.state} onRetry={health.retry} />
    </section>
  )
}

export function PublicPlaceholderPage({ title }: { readonly title: string }) {
  return <PublicPlaceholderView title={title} />
}

/**
 * `/ingresar`. Con sesión activa vuelve al `returnUrl` saneado (o a la superficie por defecto).
 * Sin sesión muestra la entrada: HU-003 (bloque B6) sustituye este marcador por la vista real.
 */
export function SignInPage({ session }: { readonly session: SessionController }) {
  const [searchParams] = useSearchParams()
  const { state } = session

  if (state.kind === 'loading') return <LoadingView message="Cargando tu sesión…" />
  if (state.kind === 'authenticated') {
    const target = sanitizeReturnUrl(searchParams.get('returnUrl'), homePathFor(state.user))
    return <Navigate to={target} replace />
  }
  return <PublicPlaceholderView title="Ingresar" />
}
