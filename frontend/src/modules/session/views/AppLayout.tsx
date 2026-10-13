import type { ReactNode } from 'react'
import { BrandMark } from '../../../shared/ui/BrandMark'
import type { NavigationItem, SectionId } from '../models/navigation'
import { ShellNavigation } from './ShellNavigation'
import { ShellUser } from './ShellUser'
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
  /** Fallo al cerrar sesión (la sesión sigue abierta). */
  readonly signOutError?: string | null
  readonly children: ReactNode
}

const NAVIGATION_LABEL: Record<AppLayoutVariant, string> = {
  internal: 'Navegación del espacio interno',
  portal: 'Navegación del portal del cliente',
}

/** Shell autenticado: cabecera (marca, usuario, «Cerrar sesión»), navegación y contenido. */
export function AppLayout(props: AppLayoutProps) {
  const { variant, navigation, currentItemId, onNavigate, signOutError, children } = props
  const nav = (
    <ShellNavigation
      label={NAVIGATION_LABEL[variant]}
      items={navigation}
      currentItemId={currentItemId}
      onNavigate={onNavigate}
    />
  )

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
            <span className="shell-brand__surface">{props.surfaceLabel}</span>
          </div>
        </div>
        {variant === 'portal' ? nav : null}
        <ShellUser
          name={props.userName}
          detail={props.userDetail}
          initials={props.userInitials}
          onSignOut={props.onSignOut}
          isSigningOut={props.isSigningOut}
        />
      </header>
      {variant === 'internal' ? nav : null}
      <main id="contenido" className="shell-main" tabIndex={-1}>
        {signOutError ? (
          <p className="shell-alert" role="alert">
            {signOutError}
          </p>
        ) : null}
        {children}
      </main>
    </div>
  )
}
