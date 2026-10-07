import { useHealthController } from '../modules/system/controllers/useHealthController'
import { HealthStatusView } from '../modules/system/views/HealthStatusView'

export function App() {
  const health = useHealthController()

  return (
    <main className="shell">
      <header className="shell__header">
        <h1>DataTicket</h1>
        <p>Portal de soporte de Data Global</p>
      </header>
      <HealthStatusView state={health.state} onRetry={health.retry} />
    </main>
  )
}
