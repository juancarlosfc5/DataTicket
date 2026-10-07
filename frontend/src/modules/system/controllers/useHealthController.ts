import { useCallback, useEffect, useState } from 'react'
import type { HealthSummary } from '../models/health'
import { fetchHealth } from '../models/healthGateway'

export type HealthViewState =
  | { readonly kind: 'loading' }
  | { readonly kind: 'ready'; readonly summary: HealthSummary }
  | { readonly kind: 'error'; readonly message: string }

const UNREACHABLE_MESSAGE = 'No fue posible contactar el backend. Verifica que el servicio esté en ejecución.'

/** Controlador: obtiene el estado del backend y expone estado + acciones para la vista. */
export function useHealthController() {
  const [state, setState] = useState<HealthViewState>({ kind: 'loading' })
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    fetchHealth(controller.signal)
      .then((summary) => setState({ kind: 'ready', summary }))
      .catch(() => {
        if (!controller.signal.aborted) setState({ kind: 'error', message: UNREACHABLE_MESSAGE })
      })

    return () => controller.abort()
  }, [attempt])

  const retry = useCallback(() => {
    setState({ kind: 'loading' })
    setAttempt((current) => current + 1)
  }, [])

  return { state, retry }
}
