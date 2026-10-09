interface LoadingViewProps {
  readonly message: string
}

/** Estado de carga de página completa, anunciado con `role="status"`. */
export function LoadingView({ message }: LoadingViewProps) {
  return (
    <div className="page-state" role="status">
      <span className="spinner" aria-hidden="true" />
      <p className="page-state__message">{message}</p>
    </div>
  )
}
