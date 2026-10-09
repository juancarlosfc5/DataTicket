import { describe, expect, it } from 'vitest'
import { sanitizeReturnUrl, signInPathFor } from './returnUrl'

const FALLBACK = '/portal'

describe('sanitizeReturnUrl', () => {
  it('sanitizeReturnUrl rechaza //host y URL absolutas', () => {
    expect(sanitizeReturnUrl('//evil.example/x', FALLBACK)).toBe(FALLBACK)
    expect(sanitizeReturnUrl('https://evil.example', FALLBACK)).toBe(FALLBACK)
    expect(sanitizeReturnUrl('http://localhost:8080/api', FALLBACK)).toBe(FALLBACK)
    expect(sanitizeReturnUrl('javascript:alert(1)', FALLBACK)).toBe(FALLBACK)
    expect(sanitizeReturnUrl('/\\evil.example', FALLBACK)).toBe(FALLBACK)
  })

  it('acepta rutas internas relativas con query y fragmento', () => {
    expect(sanitizeReturnUrl('/interno/administracion', FALLBACK)).toBe('/interno/administracion')
    expect(sanitizeReturnUrl('/portal/tickets?estado=abierto#arriba', FALLBACK)).toBe(
      '/portal/tickets?estado=abierto#arriba',
    )
  })

  it('rechaza caracteres de control, barras invertidas y valores que no son texto', () => {
    expect(sanitizeReturnUrl('/\t/evil.example', FALLBACK)).toBe(FALLBACK)
    expect(sanitizeReturnUrl('/\n/evil.example', FALLBACK)).toBe(FALLBACK)
    expect(sanitizeReturnUrl('/ruta\\otra', FALLBACK)).toBe(FALLBACK)
    expect(sanitizeReturnUrl('', FALLBACK)).toBe(FALLBACK)
    expect(sanitizeReturnUrl(null, FALLBACK)).toBe(FALLBACK)
    expect(sanitizeReturnUrl(undefined, FALLBACK)).toBe(FALLBACK)
    expect(sanitizeReturnUrl('interno', FALLBACK)).toBe(FALLBACK)
  })

  it('no devuelve a la propia entrada para evitar bucles', () => {
    expect(sanitizeReturnUrl('/ingresar', FALLBACK)).toBe(FALLBACK)
    expect(sanitizeReturnUrl('/ingresar?returnUrl=%2Fportal', FALLBACK)).toBe(FALLBACK)
  })

  it('rechaza valores desmesurados', () => {
    expect(sanitizeReturnUrl(`/${'a'.repeat(3000)}`, FALLBACK)).toBe(FALLBACK)
  })
})

describe('signInPathFor', () => {
  it('codifica la ruta actual en returnUrl', () => {
    expect(signInPathFor('/interno/administracion')).toBe('/ingresar?returnUrl=%2Finterno%2Fadministracion')
  })

  it('omite returnUrl para la raíz o rutas no válidas', () => {
    expect(signInPathFor('/')).toBe('/ingresar')
    expect(signInPathFor('//evil.example')).toBe('/ingresar')
  })
})
