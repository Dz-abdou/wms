export type Product = {
  id: string
  sku: string
  name: string
  description: string | null
  isActive: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export type ProductListQuery = {
  page: number
  pageSize: number
  search?: string
}

export type PagedResult<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export type ProductFormValues = {
  sku: string
  name: string
  description?: string
}