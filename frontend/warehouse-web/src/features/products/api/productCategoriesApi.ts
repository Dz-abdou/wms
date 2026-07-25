import { requestJson } from '../../../shared/api/apiClient'
import type { PagedResult, ProductCategory } from './productTypes'

const productCategoriesPath = '/api/product-categories'

export function getProductCategories(signal?: AbortSignal) {
  return requestJson<PagedResult<ProductCategory>>(
    `${productCategoriesPath}?page=1&pageSize=100`,
    { signal },
  )
}

export function createProductCategory(input: Pick<ProductCategory, 'code' | 'name' | 'parentCategoryId'>) {
  return requestJson<ProductCategory>(productCategoriesPath, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(input) })
}

export function updateProductCategory(id: string, input: Pick<ProductCategory, 'code' | 'name' | 'parentCategoryId'>) {
  return requestJson<ProductCategory>(`${productCategoriesPath}/${id}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(input) })
}

