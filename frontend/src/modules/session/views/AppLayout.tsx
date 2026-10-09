import type { ReactNode } from 'react'
import { BrandMark } from '../../../shared/ui/BrandMark'
import { InternalLink } from '../../../shared/ui/InternalLink'
import type { NavigationItem, SectionId } from '../models/navigation'
import './AppLayout.css'

export type AppLayoutVariant = 'internal' | 'portal'

export interface AppLayoutProps {
  /** `internal`: barra lateral; `portal`: navegación en la cabecera. */
  readonly variant: AppLayoutVariant
  readonly surfaceLabel: string
  readonly userName: string
  /** Segunda línea del usuario (empresa del cliente o «Data Global»). */
  readonly userDetail: string
  readonly userInitials: string
  readonly navigation: readonly NavigationItem[]
  readonly currentItemId: SectionId | null
  readonly onNavigate: (to: string) => void
  readonly onSignOut: () => void
  readonly isSigningOut: boolean
  readonly children: ReactNode
}

const NAVIGATION_LABEL: Record<AppLayoutVariant, string> = {
  internal: 'Navegación del espacio interno',
  portal: 'Navegación del portal del cliente',
}

/** Shell autenticado: cabecera (marca, usuario, «Cerrar sesión»), navegación y contenido. */
export function AppLayout({
  variant,
  surfaceLabel,
  userName,
  userDetail,
  userInitials,
  navigation,
  currentItemId,
  onNavigate,
  onSignOut,
  isSigningOut,
  children,
}: AppLayoutProps) {
  const nav =
    navigation.length > 0 ? (
      <nav className="shell-nav" aria-label={NAVIGATION_LABEL[variant]}>
        <ul className="shell-nav__list">
          {navigation.map((item) => (
            <li key={item.id}>
              <InternalLink
                className="shell-nav__link"
                to={item.to}
                onNavigate={onNavigate}
                aria-current={item.id === currentItemId ? 'page' : undefined}
              >
                {item.label}
              </InternalLink>
            </li>
          ))}
        </ul>
      </nav>
    ) : null

  return (
    <div className={`shell shell--${variant}`}>
      <a className="skip-link" href="#contenido">
        Saltar al contenido
      </a>
      <header className="shell-bar">
        <div className="shell-brand">
          <BrandMark />
          <div className="shell-brand__text">
            <span className="shell-brand__name">DataTicket</span>
            <span className="shell-brand__surface">{surfaceLabel}</span>
          </div>
        </div>
        {variant === 'portal' ? nav : null}
        <div className="shell-user">
          <span className="avatar" aria-hidden="true">
            {userInitials}
          </span>
          <div className="shell-user__text">
            <span className="shell-user__name">{userName}</span>
            <span className="shell-user__detail">{userDetail}</span>
          </div>
          <button type="button" className="btn btn--quiet" onClick={onSignOut} disabled={isSigningOut}>
            {isSigningOut ? 'Cerrando sesión…' : 'Cerrar sesión'}
          </button>
        </div>
      </header>
      {variant === 'internal' ? nav : null}
      <main id="contenido" className="shell-main" tabIndex={-1}>
        {children}
      </main>
    </div>
  )
}
