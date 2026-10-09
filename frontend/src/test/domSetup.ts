/**
 * Preparación común de las pruebas de vistas (jsdom). Cada `*.test.tsx` lo importa tras
 * declarar `// @vitest-environment jsdom`: así modelos y controladores siguen en `node`.
 */
import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach } from 'vitest'

afterEach(() => {
  cleanup()
})
