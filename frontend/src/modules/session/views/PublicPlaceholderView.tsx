import { BrandMark } from '../../../shared/ui/BrandMark'

interface PublicPlaceholderViewProps {
  readonly title: string
}

/** Marcador de las páginas públicas de cuenta hasta que llegue su HU. */
export function PublicPlaceholderView({ title }: PublicPlaceholderViewProps) {
  return (
    <main className="public-page">
      <section className="public-card" aria-labelledby="public-title">
        <div className="public-card__brand">
          <BrandMark />
          <span className="public-card__brand-name">DataTicket</span>
        </div>
        <h1 id="public-title" className="public-card__title">
          {title}
        </h1>
        <p className="public-card__text">Disponible próximamente.</p>
      </section>
    </main>
  )
}
