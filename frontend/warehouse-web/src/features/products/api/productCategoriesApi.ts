import { requestJson } from '../../../shared/api/apiClient'
import type { PagedResult, ProductCategory } from './productTypes'

const productCategoriesPath = '/api/product-categories'

export function getProductCategories(signal?: AbortSignal) {
  return requestJson<PagedResult<ProductCategory>>(
    `${productCategoriesPath}?page=1&pageSize=100`,
    { signal },
  )
}

