import { requestJson } from '../../../shared/api/apiClient'
import { productApiPaths } from '../productConstants'
import type { PagedResult, Product, ProductFormValues, ProductListQuery } from './productTypes'

function toQueryString(query: ProductListQuery): string {
  const parameters = new URLSearchParams({
    page: String(query.page),
    pageSize: String(query.pageSize),
  })

  if (query.search?.trim()) {
    parameters.set('search', query.search.trim())
  }

  return parameters.toString()
}

export function getProducts(query: ProductListQuery, signal?: AbortSignal) {
  return requestJson<PagedResult<Product>>(`${productApiPaths.base}?${toQueryString(query)}`, { signal })
}

export function getProduct(id: string, signal?: AbortSignal) {
  return requestJson<Product>(productApiPaths.byId(id), { signal })
}

export function createProduct(values: ProductFormValues) {
  return requestJson<Product>(productApiPaths.base, jsonRequest('POST', values))
}

export function updateProduct(id: string, values: ProductFormValues) {
  return requestJson<Product>(productApiPaths.byId(id), jsonRequest('PUT', values))
}

export function setProductStatus(id: string, isActive: boolean) {
  return requestJson<Product>(productApiPaths.status(id), jsonRequest('PATCH', { isActive }))
}

function jsonRequest(method: 'PATCH' | 'POST' | 'PUT', body: object): RequestInit {
  return {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  }
}