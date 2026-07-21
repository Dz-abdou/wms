import '@testing-library/jest-dom/vitest'
import { beforeEach } from 'vitest'
import { i18n } from '../shared/i18n/i18n'

class ResizeObserver {
  observe() {}

  unobserve() {}

  disconnect() {}
}

Object.defineProperty(globalThis, 'ResizeObserver', {
  writable: true,
  value: ResizeObserver
})

Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string) => ({
    addEventListener: () => {},
    addListener: () => {},
    dispatchEvent: () => false,
    matches: false,
    media: query,
    onchange: null,
    removeEventListener: () => {},
    removeListener: () => {}
  })
})

beforeEach(async () => {
  window.localStorage.clear()
  await i18n.changeLanguage('en')
})