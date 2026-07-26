import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createProductCategory, getProductCategories, updateProductCategory } from './productCategoriesApi'
import type { ProductCategory } from './productTypes'

export const productCategoryKeys = {
  all: ['product-categories'] as const,
}

export function useProductCategories() {
  return useQuery({
    queryKey: productCategoryKeys.all,
    queryFn: ({ signal }) => getProductCategories(signal),
  })
}

export function useCreateProductCategory() { const queryClient = useQueryClient(); return useMutation({ mutationFn: createProductCategory, onSuccess: () => queryClient.invalidateQueries({ queryKey: productCategoryKeys.all }) }) }
export function useUpdateProductCategory() { const queryClient = useQueryClient(); return useMutation({ mutationFn: ({ id, input }: { id: string; input: Pick<ProductCategory, 'code' | 'name' | 'parentCategoryId'> }) => updateProductCategory(id, input), onSuccess: () => queryClient.invalidateQueries({ queryKey: productCategoryKeys.all }) }) }

