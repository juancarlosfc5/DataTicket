const API_BASE_PATH = '/api'

export class HttpError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'HttpError'
    this.status = status
  }
}

interface GetJsonOptions {
  signal?: AbortSignal
  /** Códigos no-2xx cuyo cuerpo JSON también es una respuesta válida (p. ej. 503 en /health). */
  acceptedStatuses?: readonly number[]
}

/**
 * GET contra el backend a través del proxy same-origin (/api).
 * `credentials: 'same-origin'` envía la cookie de Identity cuando exista.
 */
export async function getJson<T>(path: string, options: GetJsonOptions = {}): Promise<T> {
  const { signal, acceptedStatuses = [] } = options

  const response = await fetch(`${API_BASE_PATH}${path}`, {
    method: 'GET',
    headers: { Accept: 'application/json' },
    credentials: 'same-origin',
    signal,
  })

  if (!response.ok && !acceptedStatuses.includes(response.status)) {
    throw new HttpError(response.status, `GET ${path} respondió ${response.status}`)
  }

  return (await response.json()) as T
}
