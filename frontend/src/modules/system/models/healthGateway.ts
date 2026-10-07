import { getJson } from '../../../core/http/httpClient'
import { toHealthSummary, type HealthReportDto, type HealthSummary } from './health'

const SERVICE_UNAVAILABLE = 503

export async function fetchHealth(signal?: AbortSignal): Promise<HealthSummary> {
  const dto = await getJson<HealthReportDto>('/health', {
    signal,
    acceptedStatuses: [SERVICE_UNAVAILABLE],
  })
  return toHealthSummary(dto)
}
