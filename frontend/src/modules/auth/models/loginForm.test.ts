import { describe, expect, it } from 'vitest'
import { fieldErrorsFromProblem, hasFieldErrors, toLoginRequest, validateLogin } from './loginForm'

const VALID = { email: 'ana.perez@empresa-sintetica-01.test', password: 'Clave-Sintetica-2026' }

function emailOfLength(length: number): string {
  const domain = '@empresa.test'
  return `${'a'.repeat(length - domain.length)}${domain}`
}

describe('validateLogin', () => {
  it('acepta un correo y una contraseña válidos', () => {
    expect(validateLogin(VALID)).toEqual({})
    expect(hasFieldErrors(validateLogin(VALID))).toBe(false)
  })

  it('exige el correo y la contraseña', () => {
    expect(validateLogin({ email: '   ', password: '' })).toEqual({
      email: 'Escribe tu correo.',
      password: 'Escribe tu contraseña.',
    })
  })

  it('rechaza un correo sin dominio o con espacios', () => {
    expect(validateLogin({ ...VALID, email: 'ana.perez' }).email).toBe(
      'Escribe un correo válido, como nombre@empresa.com.',
    )
    expect(validateLogin({ ...VALID, email: 'ana perez@empresa.test' }).email).toBeDefined()
    expect(validateLogin({ ...VALID, email: '@empresa.test' }).email).toBeDefined()
    expect(validateLogin({ ...VALID, email: 'ana@' }).email).toBeDefined()
  })

  it('admite 256 caracteres de correo y rechaza 257', () => {
    expect(validateLogin({ ...VALID, email: emailOfLength(256) })).toEqual({})
    expect(validateLogin({ ...VALID, email: emailOfLength(257) }).email).toBe(
      'El correo no puede superar 256 caracteres.',
    )
  })

  it('no recorta ni valida el contenido de la contraseña', () => {
    expect(validateLogin({ ...VALID, password: ' ' })).toEqual({})
  })
})

describe('toLoginRequest', () => {
  it('recorta los espacios del correo y conserva la contraseña tal cual', () => {
    expect(toLoginRequest({ email: '  ana@empresa.test ', password: ' clave ' })).toEqual({
      email: 'ana@empresa.test',
      password: ' clave ',
    })
  })
})

describe('fieldErrorsFromProblem', () => {
  it('toma el primer mensaje de cada campo sin distinguir mayúsculas', () => {
    expect(
      fieldErrorsFromProblem({
        title: 'Uno o más campos no son válidos.',
        errors: { Email: ['El correo no tiene un formato válido.', 'otro'], password: ['Escribe la contraseña.'] },
      }),
    ).toEqual({ email: 'El correo no tiene un formato válido.', password: 'Escribe la contraseña.' })
  })

  it('ignora campos ajenos al formulario y problemas sin errores', () => {
    expect(fieldErrorsFromProblem({ errors: { tenant: ['x'] } })).toEqual({})
    expect(fieldErrorsFromProblem(undefined)).toEqual({})
  })
})
