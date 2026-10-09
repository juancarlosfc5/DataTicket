// @vitest-environment jsdom
import '../../../test/domSetup'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import type { NavigationItem } from '../models/navigation'
import { AppLayout, type AppLayoutProps } from './AppLayout'

const portalNavigation: readonly NavigationItem[] = [
  { id: 'portal-home', label: 'Inicio', to: '/portal' },
  { id: 'portal-tickets', label: 'Mis tickets', to: '/portal/tickets' },
]

function renderLayout(overrides: Partial<AppLayoutProps> = {}) {
  const props: AppLayoutProps = {
    variant: 'portal',
    surfaceLabel: 'Portal del cliente',
    userName: 'Ana Pérez',
    userDetail: 'Empresa Sintética 01',
    userInitials: 'AP',
    navigation: portalNavigation,
    currentItemId: 'portal-tickets',
    onNavigate: vi.fn(),
    onSignOut: vi.fn(),
    isSigningOut: false,
    children: <h1>Contenido</h1>,
    ...overrides,
  }
  render(<AppLayout {...props} />)
  return props
}

describe('AppLayout', () => {
  it('AppLayout muestra el nombre del usuario y Cerrar sesión', async () => {
    const props = renderLayout()

    const banner = screen.getByRole('banner')
    expect(within(banner).getByText('Ana Pérez')).toBeInTheDocument()
    const signOut = within(banner).getByRole('button', { name: 'Cerrar sesión' })
    await userEvent.click(signOut)
    expect(props.onSignOut).toHaveBeenCalledTimes(1)
  })

  it('expone una navegación etiquetada con la sección actual marcada', () => {
    renderLayout()

    const nav = screen.getByRole('navigation', { name: 'Navegación del portal del cliente' })
    const current = within(nav).getByRole('link', { name: 'Mis tickets' })
    expect(current).toHaveAttribute('aria-current', 'page')
    expect(current).toHaveAttribute('href', '/portal/tickets')
    expect(within(nav).getByRole('link', { name: 'Inicio' })).not.toHaveAttribute('aria-current')
  })

  it('navega sin recargar con un clic simple y respeta Ctrl+clic', async () => {
    const props = renderLayout()
    const user = userEvent.setup()
    const link = screen.getByRole('link', { name: 'Inicio' })

    await user.click(link)
    expect(props.onNavigate).toHaveBeenCalledWith('/portal')

    vi.mocked(props.onNavigate).mockClear()
    await user.keyboard('{Control>}')
    await user.click(link)
    await user.keyboard('{/Control}')
    expect(props.onNavigate).not.toHaveBeenCalled()
  })

  it('deshabilita Cerrar sesión mientras se cierra', () => {
    renderLayout({ isSigningOut: true })

    expect(screen.getByRole('button', { name: 'Cerrando sesión…' })).toBeDisabled()
  })

  it('la variante interna etiqueta su navegación y muestra la superficie', () => {
    renderLayout({
      variant: 'internal',
      surfaceLabel: 'Espacio interno',
      userName: 'Elizabeth',
      userDetail: 'Data Global',
      navigation: [{ id: 'internal-home', label: 'Inicio', to: '/interno' }],
      currentItemId: 'internal-home',
    })

    expect(screen.getByRole('navigation', { name: 'Navegación del espacio interno' })).toBeInTheDocument()
    expect(screen.getByText('Espacio interno')).toBeInTheDocument()
  })

  it('ofrece un enlace para saltar al contenido principal', () => {
    renderLayout()

    expect(screen.getByRole('link', { name: 'Saltar al contenido' })).toHaveAttribute('href', '#contenido')
    expect(screen.getByRole('main')).toHaveAttribute('id', 'contenido')
  })
})
