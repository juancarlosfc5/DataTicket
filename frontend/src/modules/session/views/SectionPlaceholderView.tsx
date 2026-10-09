interface SectionPlaceholderViewProps {
  readonly title: string
  readonly description: string
}

/** Marcador de una sección cuyo contenido llega con otra HU. */
export function SectionPlaceholderView({ title, description }: SectionPlaceholderViewProps) {
  return (
    <section className="page" aria-labelledby="page-title">
      <div className="page__head">
        <h1 id="page-title" className="page__title">
          {title}
        </h1>
      </div>
      <div className="placeholder">
        <span className="placeholder__pill">Disponible próximamente.</span>
        <p className="placeholder__text">{description}</p>
      </div>
    </section>
  )
}
