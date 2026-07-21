import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createProduct, getProduct, getProducts, setProductStatus, updateProduct } from './productsApi'
import type { Product, ProductFormValues, ProductListQuery } from './productTypes'

export const productKeys = {
  all: ['products'] as const,
  detail: (id: string) => [...productKeys.all, 'detail', id] as const,
  list: (query: ProductListQuery) => [...productKeys.all, 'list', query] as const,
}

export function useProducts(query: ProductListQuery) {
  return useQuery({
    queryKey: productKeys.list(query),
    queryFn: ({ signal }) => getProducts(query, signal),
  })
}

export function useProduct(id: string | undefined) {
  return useQuery({
    queryKey: productKeys.detail(id ?? ''),
    queryFn: ({ signal }) => getProduct(id ?? '', signal),
    enabled: Boolean(id),
  })
}

export function useCreateProduct() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createProduct,
    onSuccess: () => invalidateProductLists(queryClient),
  })
}

export function useUpdateProduct(id: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: ProductFormValues) => updateProduct(id, values),
    onSuccess: (product) => refreshProductCache(queryClient, product),
  })
}

export function useSetProductStatus(id: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (isActive: boolean) => setProductStatus(id, isActive),
    onSuccess: (product) => refreshProductCache(queryClient, product),
  })
}

function refreshProductCache(queryClient: ReturnType<typeof useQueryClient>, product: Product) {
  queryClient.setQueryData(productKeys.detail(product.id), product)
  return invalidateProductLists(queryClient)
}

function invalidateProductLists(queryClient: ReturnType<typeof useQueryClient>) {
  return queryClient.invalidateQueries({ queryKey: productKeys.all })
}