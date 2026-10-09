import { toProblemDetails, type ProblemDetails } from './problemDetails'

export const GENERIC_ERROR_MESSAGE = 'No pudimos completar la operación. Inténtalo de nuevo.'

const PROBLEM_CONTENT_TYPE = 'application/problem+json'
const FIRST_SERVER_ERROR = 500

/**
 * Error de una respuesta no exitosa de `/api`.
 * `problem` solo existe si la respuesta 4xx trae `application/problem+json` válido;
 * en 5xx o cuerpos vacíos/no JSON el error es genérico (no se filtran detalles del servidor).
 */
export class HttpError extends Error {
  readonly status: number
  readonly problem?: ProblemDetails

  constructor(status: number, problem?: ProblemDetails) {
    super(problem?.title ?? GENERIC_ERROR_MESSAGE)
    this.name = 'HttpError'
    this.status = status
    this.problem = problem
  }
}

/** La respuesta 2xx no tenía el formato esperado (p. ej. HTML en lugar de JSON). */
export class ResponseFormatError extends Error {
  constructor() {
    super(GENERIC_ERROR_MESSAGE)
    this.name = 'ResponseFormatError'
  }
}

async function readProblem(response: Response): Promise<ProblemDetails | undefined> {
  if (response.status >= FIRST_SERVER_ERROR) return undefined
  const contentType = response.headers.get('Content-Type') ?? ''
  if (!contentType.toLowerCase().includes(PROBLEM_CONTENT_TYPE)) return undefined

  try {
    const text = await response.text()
    return text.trim().length === 0 ? undefined : toProblemDetails(JSON.parse(text))
  } catch {
    return undefined
  }
}

export async function toHttpError(response: Response): Promise<HttpError> {
  return new HttpError(response.status, await readProblem(response))
}

/** Texto para mostrar al usuario: el detalle del ProblemDetails si existe; si no, el genérico. */
export function userMessageFor(error: unknown): string {
  if (error instanceof HttpError && error.problem) {
    return error.problem.detail ?? error.problem.title ?? GENERIC_ERROR_MESSAGE
  }
  return GENERIC_ERROR_MESSAGE
}
