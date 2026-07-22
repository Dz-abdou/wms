export type ProductUnitConversion = {
  unitOfMeasure: string
  quantityInBaseUnit: number
}

export type ProductMeasurements = {
  netWeight?: number
  grossWeight?: number
  weightUnitOfMeasure?: string
  length?: number
  width?: number
  height?: number
  dimensionUnitOfMeasure?: string
  volumeCubicMetres?: number | null
}

export type Product = {
  id: string
  sku: string
  name: string
  description: string | null
  baseUnitOfMeasure: string
  unitConversions: ProductUnitConversion[]
  measurements: ProductMeasurements | null
  categoryId: string | null
  isActive: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export type ProductCategory = {
  id: string
  code: string
  name: string
  parentCategoryId: string | null
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
  baseUnitOfMeasure: string
  categoryId?: string
  unitConversions: ProductUnitConversion[]
  measurements?: ProductMeasurements
}
