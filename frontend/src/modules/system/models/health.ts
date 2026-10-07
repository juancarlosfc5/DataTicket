export type HealthStatus = 'Healthy' | 'Degraded' | 'Unhealthy'

/** Contrato JSON de GET /api/health (ver backend/src/DataTicket.Api/Health). */
export interface HealthReportDto {
  status?: unknown
  checks?: unknown
}

export interface DependencyHealth {
  readonly name: string
  readonly status: HealthStatus
}

export interface HealthSummary {
  readonly status: HealthStatus
  readonly isOperational: boolean
  readonly dependencies: readonly DependencyHealth[]
}

const KNOWN_STATUSES: readonly HealthStatus[] = ['Healthy', 'Degraded', 'Unhealthy']

function toHealthStatus(value: unknown): HealthStatus {
  return KNOWN_STATUSES.find((status) => status === value) ?? 'Unhealthy'
}

function toDependency(value: unknown): DependencyHealth | null {
  if (typeof value !== 'object' || value === null) return null
  const { name, status } = value as { name?: unknown; status?: unknown }
  if (typeof name !== 'string' || name.length === 0) return null
  return { name, status: toHealthStatus(status) }
}

/** Normaliza la respuesta del backend: cualquier dato inesperado se trata como no saludable. */
export function toHealthSummary(dto: HealthReportDto): HealthSummary {
  const status = toHealthStatus(dto.status)
  const dependencies = Array.isArray(dto.checks)
    ? dto.checks.map(toDependency).filter((dependency) => dependency !== null)
    : []

  return {
    status,
    isOperational: status !== 'Unhealthy',
    dependencies,
  }
}
