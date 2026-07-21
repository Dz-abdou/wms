import { render, screen } from '@testing-library/react'
import { App } from './App'

describe('App', () => {
  it('renders the Phase 0 application shell', () => {
    render(<App />)

    expect(screen.getByRole('heading', { name: 'Warehouse Management System' })).toBeInTheDocument()
    expect(screen.getByText('Foundation ready')).toBeInTheDocument()
  })
})
