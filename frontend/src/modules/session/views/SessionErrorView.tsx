interface SessionErrorViewProps {
  readonly message: string
  readonly onRetry: () => void
}

/** No se pudo consultar la sesión (5xx, red, respuesta fuera de contrato). */
export function SessionErrorView({ message, onRetry }: SessionErrorViewProps) {
  return (
    <main className="public-page">
      <section className="notice" aria-labelledby="notice-error-title">
        <h1 id="notice-error-title" className="notice__title">
          Algo salió mal
        </h1>
        <p className="notice__text" role="alert">
          {message}
        </p>
        <button type="button" className="btn btn--primary" onClick={onRetry}>
          Reintentar
        </button>
      </section>
    </main>
  )
}
