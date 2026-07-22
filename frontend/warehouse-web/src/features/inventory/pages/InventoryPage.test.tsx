import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { InventoryPage } from './InventoryPage'

const { useProductsMock, useWarehousesMock, useMovementHistoryMock, useAdjustInventoryMock } = vi.hoisted(() => ({
  useProductsMock: vi.fn(),
  useWarehousesMock: vi.fn(),
  useMovementHistoryMock: vi.fn(),
  useAdjustInventoryMock: vi.fn(),
}))

vi.mock('../../products/api/useProducts', () => ({ useProducts: useProductsMock }))
vi.mock('../../warehouses/api/useWarehouses', () => ({ useWarehouses: useWarehousesMock }))
vi.mock('../api/useInventory', () => ({
  useMovementHistory: useMovementHistoryMock,
  useAdjustInventory: useAdjustInventoryMock,
}))

describe('InventoryPage', () => {
  beforeEach(() => {
    useProductsMock.mockReturnValue({ data: { items: [], page: 1, pageSize: 100, totalCount: 0 }, error: null, isLoading: false })
    useWarehousesMock.mockReturnValue({ data: { items: [], page: 1, pageSize: 100, totalCount: 0 }, error: null, isLoading: false })
    useMovementHistoryMock.mockReturnValue({ data: undefined, error: null, isLoading: false })
    useAdjustInventoryMock.mockReturnValue({ data: undefined, error: null, isPending: false, mutateAsync: vi.fn() })
  })

  it('renders the adjustment form and initial history state', () => {
    render(<InventoryPage />)

    expect(screen.getByRole('heading', { name: 'Inventory' })).toBeInTheDocument()
    expect(screen.getByText('Manual adjustment')).toBeInTheDocument()
    expect(screen.getByText('Select a product and warehouse to view history.')).toBeInTheDocument()
  })

  it('renders a source loading state', () => {
    useProductsMock.mockReturnValue({ data: undefined, error: null, isLoading: true })

    render(<InventoryPage />)

    expect(screen.getByText('Loading products and warehouses…')).toBeInTheDocument()
  })
})
