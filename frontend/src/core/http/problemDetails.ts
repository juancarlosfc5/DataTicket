/** Cuerpo de error RFC 9457 (`application/problem+json`) que expone la API. */
export interface ProblemDetails {
  readonly type?: string
  readonly title?: string
  readonly status?: number
  readonly detail?: string
  /** Errores de validación por campo (nombre del campo en camelCase → mensajes). */
  readonly errors?: Readonly<Record<string, readonly string[]>>
  /** Código estable de error de negocio, si el backend lo envía. */
  readonly code?: string
  readonly traceId?: string
}

function optionalString(value: unknown): string | undefined {
  return typeof value === 'string' ? value : undefined
}

function toFieldErrors(value: unknown): Record<string, readonly string[]> | undefined {
  if (typeof value !== 'object' || value === null || Array.isArray(value)) return undefined

  const entries = Object.entries(value)
    .filter((entry): entry is [string, unknown[]] => Array.isArray(entry[1]))
    .map(([field, messages]) => [field, messages.filter((m): m is string => typeof m === 'string')] as const)
    .filter(([, messages]) => messages.length > 0)

  return entries.length > 0 ? Object.fromEntries(entries) : undefined
}

/** Normaliza un JSON externo: los campos con tipo inesperado se descartan. */
export function toProblemDetails(value: unknown): ProblemDetails | undefined {
  if (typeof value !== 'object' || value === null || Array.isArray(value)) return undefined
  const raw = value as Record<string, unknown>

  return {
    type: optionalString(raw.type),
    title: optionalString(raw.title),
    status: typeof raw.status === 'number' ? raw.status : undefined,
    detail: optionalString(raw.detail),
    errors: toFieldErrors(raw.errors),
    code: optionalString(raw.code),
    traceId: optionalString(raw.traceId),
  }
}
