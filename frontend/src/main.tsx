// Los estilos base van ANTES de `App`: el CSS de cada módulo se emite en el orden de importación,
// y a igual especificidad gana el último. Si `global.css` se cargara después, sus clases base
// (`.btn`, …) pisarían los ajustes de los módulos (p. ej. el alto de «Ingresar» en `/ingresar`).
import '@fontsource-variable/archivo/wdth.css'
import './styles/tokens.css'
import './styles/global.css'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router'
import { App } from './app/App'

const rootElement = document.getElementById('root')
if (!rootElement) {
  throw new Error('No se encontró el elemento #root en index.html')
}

createRoot(rootElement).render(
  <StrictMode>
    <BrowserRouter>
      <App />
    </BrowserRouter>
  </StrictMode>,
)
