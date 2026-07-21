import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

const defaultApiProxyTarget = 'http://localhost:5230'

export default defineConfig(({ mode }) => {
  const environment = loadEnv(mode, process.cwd(), '')
  const apiProxyTarget = environment.VITE_API_PROXY_TARGET || defaultApiProxyTarget

  return {
    plugins: [react()],
    server: {
      proxy: {
        '/api': {
          target: apiProxyTarget,
          changeOrigin: true,
        },
      },
    },
  }
})