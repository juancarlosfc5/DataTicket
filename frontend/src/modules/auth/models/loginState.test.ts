import { describe, expect, it } from 'vitest'
import { INITIAL_LOGIN_STATE, INVALID_CREDENTIALS_MESSAGE, loginReducer, type LoginState } from './loginState'

const filled: LoginState = { ...INITIAL_LOGIN_STATE, email: 'ana@empresa.test', password: 'secreta' }

describe('loginReducer', () => {
  it('empieza en idle con el formulario vacío', () => {
    expect(INITIAL_LOGIN_STATE).toEqual({
      email: '',
      password: '',
      status: { kind: 'idle' },
      attempt: 0,
      focus: null,
    })
  })

  it('actualiza los campos y limpia solo el error del campo editado', () => {
    const withErrors = loginReducer(filled, {
      type: 'validationFailed',
      fieldErrors: { email: 'a', password: 'b' },
    })
    const next = loginReducer(withErrors, { type: 'emailChanged', value: 'otro@empresa.test' })

    expect(next.email).toBe('otro@empresa.test')
    expect(next.status).toEqual({ kind: 'error', message: null, fieldErrors: { password: 'b' } })
  })

  it('la validación local enfoca el primer campo con error', () => {
    const next = loginReducer(filled, { type: 'validationFailed', fieldErrors: { password: 'b' } })

    expect(next.focus).toEqual({ target: 'password', attempt: 1 })
  })

  it('al enviar pasa a submitting y cuenta el intento', () => {
    const next = loginReducer(filled, { type: 'submitStarted' })

    expect(next.status).toEqual({ kind: 'submitting' })
    expect(next.attempt).toBe(1)
  })

  it('un 401 muestra el mensaje genérico, conserva el correo, vacía la contraseña y enfoca la contraseña', () => {
    const submitting = loginReducer(filled, { type: 'submitStarted' })
    const next = loginReducer(submitting, { type: 'rejected' })

    expect(next.status).toEqual({ kind: 'error', message: INVALID_CREDENTIALS_MESSAGE, fieldErrors: {} })
    expect(INVALID_CREDENTIALS_MESSAGE).toBe('Correo o contraseña incorrectos.')
    expect(next.email).toBe('ana@empresa.test')
    expect(next.password).toBe('')
    expect(next.focus).toEqual({ target: 'password', attempt: 1 })
  })

  it('un 400 con errores por campo los muestra y enfoca el primero', () => {
    const next = loginReducer(loginReducer(filled, { type: 'submitStarted' }), {
      type: 'invalid',
      fieldErrors: { email: 'El correo no tiene un formato válido.' },
    })

    expect(next.status).toEqual({
      kind: 'error',
      message: null,
      fieldErrors: { email: 'El correo no tiene un formato válido.' },
    })
    expect(next.password).toBe('secreta')
    expect(next.focus?.target).toBe('email')
  })

  it('otro fallo muestra un mensaje y enfoca el aviso', () => {
    const next = loginReducer(loginReducer(filled, { type: 'submitStarted' }), { type: 'failed', message: 'x' })

    expect(next.status).toEqual({ kind: 'error', message: 'x', fieldErrors: {} })
    expect(next.focus?.target).toBe('alert')
  })
})
