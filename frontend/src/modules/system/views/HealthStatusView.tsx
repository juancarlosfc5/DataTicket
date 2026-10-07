import type { HealthViewState } from '../controllers/useHealthController'
import type { HealthStatus } from '../models/health'

interface HealthStatusViewProps {
  state: HealthViewState
  onRetry: () => void
}

const STATUS_LABEL: Record<HealthStatus, string> = {
  Healthy: 'Operativo',
  Degraded: 'Degradado',
  Unhealthy: 'Sin servicio',
}

/** Vista pura: solo renderiza lo que recibe del controlador. */
export function HealthStatusView({ state, onRetry }: HealthStatusViewProps) {
  if (state.kind === 'loading') {
    return <p className="health health--loading" role="status">Consultando el estado del backend…</p>
  }

  if (state.kind === 'error') {
    return (
      <div className="health health--down" role="alert">
        <p>{state.message}</p>
        <button type="button" onClick={onRetry}>Reintentar</button>
      </div>
    )
  }

  const { summary } = state
  return (
    <section className={`health health--${summary.isOperational ? 'up' : 'down'}`} aria-label="Estado del sistema">
      <p className="health__status">
        Backend: <strong>{STATUS_LABEL[summary.status]}</strong>
      </p>
      <ul className="health__dependencies">
        {summary.dependencies.map((dependency) => (
          <li key={dependency.name}>
            {dependency.name}: {STATUS_LABEL[dependency.status]}
          </li>
        ))}
      </ul>
      <button type="button" onClick={onRetry}>Actualizar</button>
    </section>
  )
}
