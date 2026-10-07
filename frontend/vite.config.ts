/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// En Docker el backend se resuelve por nombre de servicio (http://backend:8080);
// fuera de Docker, por localhost. El navegador solo habla con el origen del frontend,
// lo que permite cookies de Identity same-origin y WebSockets de SignalR sin CORS.
const apiTarget = process.env.VITE_API_PROXY_TARGET ?? 'http://localhost:8080'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': { target: apiTarget },
      '/hubs': { target: apiTarget, ws: true },
    },
  },
  test: {
    environment: 'node',
    include: ['src/**/*.test.{ts,tsx}'],
  },
})
