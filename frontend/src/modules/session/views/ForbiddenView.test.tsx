// @vitest-environment jsdom
import '../../../test/domSetup'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { ForbiddenView } from './ForbiddenView'

describe('ForbiddenView', () => {
  it('anuncia que no hay permiso y enlaza a la superficie propia', async () => {
    const onNavigate = vi.fn()
    render(<ForbiddenView homePath="/portal" homeLabel="Ir a mi portal" onNavigate={onNavigate} />)

    expect(screen.getByRole('alert')).toHaveTextContent('No tienes permiso para ver esta página.')
    const link = screen.getByRole('link', { name: 'Ir a mi portal' })
    expect(link).toHaveAttribute('href', '/portal')
    await userEvent.click(link)
    expect(onNavigate).toHaveBeenCalledWith('/portal')
  })

  it('sin superficie propia no ofrece enlace', () => {
    render(<ForbiddenView homePath={null} homeLabel="" onNavigate={vi.fn()} />)

    expect(screen.getByRole('alert')).toHaveTextContent('No tienes permiso para ver esta página.')
    expect(screen.queryByRole('link')).not.toBeInTheDocument()
  })
})
