// @vitest-environment jsdom
import '../test/domSetup'
import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, useLocation } from 'react-router'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { invalidateAntiforgeryToken } from '../core/http/antiforgery'
import { App } from './App'

type Handler = (init: RequestInit) => Response

const CLIENT_ME = {
  id: 'b0000000-0000-7000-8000-000001000001',
  displayName: 'Ana Pérez',
  email: 'ana.perez@empresa-sintetica-01.test',
  surface: 'client',
  roles: ['Requester'],
  teams: [],
  company: { id: 'c0000000-0000-7000-8000-000000000001', name: 'Empresa Sintética 01' },
}

const INTERNAL_ME = {
  id: 'a0000000-0000-7000-8000-000000000001',
  displayName: 'Elizabeth',
  email: 'elizabeth@dataticket.local',
  surface: 'internal',
  roles: ['ProductManager'],
  teams: ['Production'],
  company: null,
}

const ok = (body: unknown): Handler => () => Response.json(body)
const problem = (status: number, title: string): Handler => () =>
  new Response(JSON.stringify({ status, title }), { status, headers: { 'Content-Type': 'application/problem+json' } })

/** Doble de `fetch` que respeta el contrato de la API; cualquier ruta no prevista falla la prueba. */
function mockApi(routes: Record<string, Handler>) {
  const fetchMock = vi.fn(async (input: RequestInfo | URL, init: RequestInit = {}) => {
    const url = String(input)
    const handler = routes[url]
    if (!handler) throw new Error(`Petición no prevista en la prueba: ${url}`)
    return handler(init)
  })
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

function requestedUrls(fetchMock: ReturnType<typeof mockApi>): string[] {
  return fetchMock.mock.calls.map(([input]) => String(input))
}

function LocationProbe() {
  const location = useLocation()
  return <output data-testid="location">{`${location.pathname}${location.search}`}</output>
}

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <App />
      <LocationProbe />
    </MemoryRouter>,
  )
}

const currentLocation = () => screen.getByTestId('location').textContent

beforeEach(() => {
  invalidateAntiforgeryToken()
})

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('Shell: superficie por usuario (CHU-01)', () => {
  it('la raíz lleva al cliente a /portal y muestra su nombre en la cabecera', async () => {
    mockApi({ '/api/auth/me': ok(CLIENT_ME) })

    renderAt('/')

    await waitFor(() => expect(currentLocation()).toBe('/portal'))
    expect(within(screen.getByRole('banner')).getByText('Ana Pérez')).toBeInTheDocument()
    expect(screen.getByRole('navigation', { name: 'Navegación del portal del cliente' })).toBeInTheDocument()
  })

  it('la raíz lleva al interno a /interno y muestra su nombre en la cabecera', async () => {
    mockApi({ '/api/auth/me': ok(INTERNAL_ME) })

    renderAt('/')

    await waitFor(() => expect(currentLocation()).toBe('/interno'))
    expect(within(screen.getByRole('banner')).getByText('Elizabeth')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Cola de triage' })).toBeInTheDocument()
  })

  it('una ruta desconocida vuelve a la superficie del usuario', async () => {
    mockApi({ '/api/auth/me': ok(CLIENT_ME) })

    renderAt('/no-existe')

    await waitFor(() => expect(currentLocation()).toBe('/portal'))
  })

  it('un surface desconocido se trata como sin acceso', async () => {
    mockApi({ '/api/auth/me': ok({ ...INTERNAL_ME, surface: 'otro' }) })

    renderAt('/')

    expect(await screen.findByRole('alert')).toHaveTextContent('No tienes permiso para ver esta página.')
    expect(screen.queryByRole('navigation', { name: /Navegación/ })).not.toBeInTheDocument()
  })
})

describe('Shell: sesión caducada (CHU-04)', () => {
  it('sin sesión, una ruta protegida redirige a /ingresar con returnUrl', async () => {
    mockApi({ '/api/auth/me': problem(401, 'Se requiere iniciar sesión.') })

    renderAt('/interno/administracion')

    await waitFor(() => expect(currentLocation()).toBe('/ingresar?returnUrl=%2Finterno%2Fadministracion'))
  })

  it('un 401 de cualquier petición posterior descarta la sesión y redirige a /ingresar', async () => {
    mockApi({
      '/api/auth/me': ok(INTERNAL_ME),
      '/api/health': problem(401, 'Se requiere iniciar sesión.'),
    })

    renderAt('/interno/estado')

    await waitFor(() => expect(currentLocation()).toBe('/ingresar?returnUrl=%2Finterno%2Festado'))
    expect(screen.queryByText('Elizabeth')).not.toBeInTheDocument()
  })
})

describe('Shell: returnUrl saneado (CHU-05)', () => {
  it('con sesión activa, /ingresar con returnUrl externo lleva a la superficie por defecto', async () => {
    mockApi({ '/api/auth/me': ok(CLIENT_ME) })

    renderAt('/ingresar?returnUrl=%2F%2Fevil.example%2Fx')

    await waitFor(() => expect(currentLocation()).toBe('/portal'))
  })

  it('con sesión activa, /ingresar con returnUrl interno vuelve a esa ruta', async () => {
    mockApi({ '/api/auth/me': ok(INTERNAL_ME) })

    renderAt('/ingresar?returnUrl=%2Finterno%2Fbandeja')

    await waitFor(() => expect(currentLocation()).toBe('/interno/bandeja'))
  })
})

describe('Shell: sin permiso (CHU-06)', () => {
  it('un cliente que abre /interno/administracion ve sin permiso, sin pedir /api/admin', async () => {
    const fetchMock = mockApi({ '/api/auth/me': ok(CLIENT_ME) })

    renderAt('/interno/administracion')

    expect(await screen.findByRole('alert')).toHaveTextContent('No tienes permiso para ver esta página.')
    expect(screen.getByRole('link', { name: 'Ir a mi portal' })).toHaveAttribute('href', '/portal')
    expect(screen.queryByText('Administración')).not.toBeInTheDocument()
    expect(requestedUrls(fetchMock)).toEqual(['/api/auth/me'])
  })

  it('un interno que abre /portal ve sin permiso', async () => {
    mockApi({ '/api/auth/me': ok(INTERNAL_ME) })

    renderAt('/portal/radicar')

    expect(await screen.findByRole('alert')).toHaveTextContent('No tienes permiso para ver esta página.')
    expect(screen.getByRole('link', { name: 'Ir al espacio interno' })).toHaveAttribute('href', '/interno')
  })

  it('un miembro de Development que abre /interno/triage ve sin permiso', async () => {
    mockApi({ '/api/auth/me': ok({ ...INTERNAL_ME, roles: [], teams: ['Development'] }) })

    renderAt('/interno/triage')

    expect(await screen.findByRole('alert')).toHaveTextContent('No tienes permiso para ver esta página.')
  })

  it('un 403 de la API lleva a la vista sin permiso', async () => {
    mockApi({
      '/api/auth/me': ok(INTERNAL_ME),
      '/api/health': problem(403, 'No tienes permiso para realizar esta acción.'),
    })

    renderAt('/interno/estado')

    await waitFor(() => expect(currentLocation()).toBe('/sin-permiso'))
    expect(screen.getByRole('alert')).toHaveTextContent('No tienes permiso para ver esta página.')
  })
})

describe('Shell: errores y rutas públicas', () => {
  it('si /me responde 502 HTML muestra el mensaje genérico y permite reintentar', async () => {
    let attempts = 0
    mockApi({
      '/api/auth/me': () => {
        attempts += 1
        return attempts === 1
          ? new Response('<html>Bad Gateway</html>', { status: 502, headers: { 'Content-Type': 'text/html' } })
          : Response.json(CLIENT_ME)
      },
    })

    renderAt('/portal')

    expect(await screen.findByRole('alert')).toHaveTextContent('No pudimos completar la operación. Inténtalo de nuevo.')
    await userEvent.click(screen.getByRole('button', { name: 'Reintentar' }))
    expect(await screen.findByText('Ana Pérez')).toBeInTheDocument()
  })

  it('las rutas públicas de cuenta se muestran sin sesión', async () => {
    mockApi({ '/api/auth/me': problem(401, 'Se requiere iniciar sesión.') })

    renderAt('/olvide-mi-contrasena')

    expect(await screen.findByRole('heading', { name: 'Recuperar contraseña' })).toBeInTheDocument()
    expect(screen.getByText('Disponible próximamente.')).toBeInTheDocument()
    expect(currentLocation()).toBe('/olvide-mi-contrasena')
  })

  it('/ingresar sin sesión muestra la entrada', async () => {
    mockApi({ '/api/auth/me': problem(401, 'Se requiere iniciar sesión.') })

    renderAt('/ingresar?returnUrl=%2Fportal')

    expect(await screen.findByRole('heading', { name: 'Ingresar' })).toBeInTheDocument()
    expect(currentLocation()).toBe('/ingresar?returnUrl=%2Fportal')
  })

  it('Cerrar sesión descarta la sesión en memoria y lleva a /ingresar', async () => {
    mockApi({ '/api/auth/me': ok(CLIENT_ME) })
    renderAt('/portal')

    await userEvent.click(await screen.findByRole('button', { name: 'Cerrar sesión' }))

    await waitFor(() => expect(currentLocation()).toBe('/ingresar'))
    expect(screen.queryByText('Ana Pérez')).not.toBeInTheDocument()
  })

  it('el módulo system sigue accesible en /interno/estado', async () => {
    mockApi({
      '/api/auth/me': ok(INTERNAL_ME),
      '/api/health': ok({ status: 'Healthy', checks: [{ name: 'postgresql', status: 'Healthy' }] }),
    })

    renderAt('/interno/estado')

    expect(await screen.findByText('Operativo', { selector: 'strong' })).toBeInTheDocument()
  })
})
