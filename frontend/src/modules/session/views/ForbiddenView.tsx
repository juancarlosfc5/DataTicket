import { InternalLink } from '../../../shared/ui/InternalLink'

interface ForbiddenViewProps {
  /** Inicio de la superficie propia; `null` si el usuario no tiene ninguna. */
  readonly homePath: string | null
  readonly homeLabel: string
  readonly onNavigate: (to: string) => void
}

/** «Sin permiso»: no revela nada del recurso solicitado. */
export function ForbiddenView({ homePath, homeLabel, onNavigate }: ForbiddenViewProps) {
  return (
    <section className="notice" aria-labelledby="notice-forbidden-title">
      <span className="notice__badge">403</span>
      <h1 id="notice-forbidden-title" className="notice__title">
        Sin permiso
      </h1>
      <p className="notice__text" role="alert">
        No tienes permiso para ver esta página.
      </p>
      {homePath ? (
        <InternalLink className="btn btn--primary" to={homePath} onNavigate={onNavigate}>
          {homeLabel}
        </InternalLink>
      ) : null}
    </section>
  )
}
