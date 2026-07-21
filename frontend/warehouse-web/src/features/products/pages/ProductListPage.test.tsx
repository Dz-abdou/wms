import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import { ApiError } from '../../../shared/api/apiClient'
import { ProductListPage } from './ProductListPage'

const { useProductsMock } = vi.hoisted(() => ({ useProductsMock: vi.fn() }))

vi.mock('../api/useProducts', () => ({
  useProducts: useProductsMock,
}))

describe('ProductListPage', () => {
  beforeEach(() => {
    useProductsMock.mockReset()
  })

  it('renders a loading state', () => {
    useProductsMock.mockReturnValue({ data: undefined, error: null, isFetching: true, isLoading: true })

    renderPage()

    expect(screen.getByText('Loading products…')).toBeInTheDocument()
  })

  it('renders an empty state', () => {
    useProductsMock.mockReturnValue({
      data: { items: [], page: 1, pageSize: 20, totalCount: 0 },
      error: null,
      isFetching: false,
      isLoading: false,
    })

    renderPage()

    expect(screen.getByText('No products match this search.')).toBeInTheDocument()
  })

  it('renders an API error state', () => {
    useProductsMock.mockReturnValue({
      data: undefined,
      error: new ApiError(500, { title: 'Server error' }),
      isFetching: false,
      isLoading: false,
    })

    renderPage()

    expect(screen.getByText('Products could not be loaded.')).toBeInTheDocument()
  })

  it('renders a populated product table', () => {
    useProductsMock.mockReturnValue({
      data: {
        items: [
          {
            id: 'a9e05574-335d-4b32-afad-b449d05009fd',
            sku: 'SKU-001',
            name: 'Sample product',
            description: null,
            isActive: true,
            createdAtUtc: '2026-07-20T12:00:00Z',
            updatedAtUtc: '2026-07-20T12:00:00Z',
          },
        ],
        page: 1,
        pageSize: 20,
        totalCount: 1,
      },
      error: null,
      isFetching: false,
      isLoading: false,
    })

    renderPage()

    expect(screen.getByText('SKU-001')).toBeInTheDocument()
    expect(screen.getByText('Sample product')).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
  })
})

function renderPage() {
  return render(
    <MemoryRouter>
      <ProductListPage />
    </MemoryRouter>,
  )
}