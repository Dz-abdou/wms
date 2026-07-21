import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { ApiError } from '../../../shared/api/apiClient'
import { WarehouseListPage } from './WarehouseListPage'

const { useWarehousesMock } = vi.hoisted(() => ({ useWarehousesMock: vi.fn() }))
vi.mock('../api/useWarehouses', () => ({ useWarehouses: useWarehousesMock }))

describe('WarehouseListPage', () => {
  beforeEach(() => useWarehousesMock.mockReset())

  it('renders loading, empty, error, and populated states', () => {
    useWarehousesMock.mockReturnValue({ data: undefined, error: null, isFetching: true, isLoading: true })
    const { rerender } = renderPage()
    expect(screen.getByText('Loading warehouses…')).toBeInTheDocument()

    useWarehousesMock.mockReturnValue({ data: { items: [], page: 1, pageSize: 20, totalCount: 0 }, error: null, isFetching: false, isLoading: false })
    rerender(<MemoryRouter><WarehouseListPage /></MemoryRouter>)
    expect(screen.getByText('No warehouses exist yet.')).toBeInTheDocument()

    useWarehousesMock.mockReturnValue({ data: undefined, error: new ApiError(500, {}), isFetching: false, isLoading: false })
    rerender(<MemoryRouter><WarehouseListPage /></MemoryRouter>)
    expect(screen.getByText('Warehouses could not be loaded.')).toBeInTheDocument()

    useWarehousesMock.mockReturnValue({ data: { items: [{ id: 'a', code: 'MAIN-01', name: 'Main', description: null, isActive: true, createdAtUtc: '2026-07-21T10:00:00Z', updatedAtUtc: '2026-07-21T10:00:00Z' }], page: 1, pageSize: 20, totalCount: 1 }, error: null, isFetching: false, isLoading: false })
    rerender(<MemoryRouter><WarehouseListPage /></MemoryRouter>)
    expect(screen.getByText('MAIN-01')).toBeInTheDocument()
  })
})

function renderPage() { return render(<MemoryRouter><WarehouseListPage /></MemoryRouter>) }