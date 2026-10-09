// @vitest-environment jsdom
import '../../../test/domSetup'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { INITIAL_LOGIN_STATE, type LoginState } from '../models/loginState'
import { LoginView, type LoginViewProps } from './LoginView'

function renderView(state: Partial<LoginState> = {}, overrides: Partial<LoginViewProps> = {}) {
  const props: LoginViewProps = {
    state: { ...INITIAL_LOGIN_STATE, ...state },
    onEmailChange: vi.fn(),
    onPasswordChange: vi.fn(),
    onSubmit: vi.fn(),
    onNavigate: vi.fn(),
    ...overrides,
  }
  const view = render(<LoginView {...props} />)
  return { props, view }
}

describe('LoginView', () => {
  it('muestra correo, contraseña, Ingresar y el enlace de recuperación', () => {
    renderView()

    expect(screen.getByLabelText('Correo')).toHaveAttribute('autocomplete', 'username')
    expect(screen.getByLabelText('Contraseña')).toHaveAttribute('type', 'password')
    expect(screen.getByLabelText('Contraseña')).toHaveAttribute('autocomplete', 'current-password')
    expect(screen.getByRole('button', { name: 'Ingresar' })).toBeEnabled()
    expect(screen.getByRole('link', { name: '¿Olvidaste tu contraseña?' })).toHaveAttribute(
      'href',
      '/olvide-mi-contrasena',
    )
  })

  it('envía el formulario con Enter y notifica los cambios de los campos', async () => {
    const user = userEvent.setup()
    const { props } = renderView()

    await user.type(screen.getByLabelText('Correo'), 'a')
    await user.type(screen.getByLabelText('Contraseña'), 'b{Enter}')

    expect(props.onEmailChange).toHaveBeenCalledWith('a')
    expect(props.onPasswordChange).toHaveBeenCalledWith('b')
    expect(props.onSubmit).toHaveBeenCalledTimes(1)
  })

  it('deshabilita Ingresar mientras se envía', () => {
    renderView({ status: { kind: 'submitting' } })

    expect(screen.getByRole('button', { name: 'Ingresando…' })).toBeDisabled()
  })

  it('anuncia el error genérico en role=alert y enfoca la contraseña', () => {
    renderView({
      email: 'ana@empresa.test',
      password: '',
      status: { kind: 'error', message: 'Correo o contraseña incorrectos.', fieldErrors: {} },
      attempt: 1,
      focus: { target: 'password', attempt: 1 },
    })

    expect(screen.getByRole('alert')).toHaveTextContent('Correo o contraseña incorrectos.')
    expect(screen.getByLabelText('Correo')).toHaveValue('ana@empresa.test')
    expect(screen.getByLabelText('Contraseña')).toHaveValue('')
    expect(screen.getByLabelText('Contraseña')).toHaveFocus()
    expect(screen.getByRole('button', { name: 'Ingresar' })).toBeEnabled()
  })

  it('asocia los errores por campo con aria-describedby y aria-invalid', () => {
    renderView({
      status: { kind: 'error', message: null, fieldErrors: { email: 'Escribe tu correo.' } },
      focus: { target: 'email', attempt: 1 },
    })

    const email = screen.getByLabelText('Correo')
    expect(email).toHaveAttribute('aria-invalid', 'true')
    expect(email).toHaveAccessibleDescription('Escribe tu correo.')
    expect(email).toHaveFocus()
    expect(screen.getByLabelText('Contraseña')).not.toHaveAttribute('aria-invalid')
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('el enlace de recuperación navega sin recargar', async () => {
    const { props } = renderView()

    await userEvent.click(screen.getByRole('link', { name: '¿Olvidaste tu contraseña?' }))

    expect(props.onNavigate).toHaveBeenCalledWith('/olvide-mi-contrasena')
  })

  it('muestra los textos como texto plano', () => {
    renderView({ status: { kind: 'error', message: '<img src=x onerror=alert(1)>', fieldErrors: {} } })

    expect(screen.getByRole('alert')).toHaveTextContent('<img src=x onerror=alert(1)>')
    expect(document.querySelector('img')).toBeNull()
  })
})
