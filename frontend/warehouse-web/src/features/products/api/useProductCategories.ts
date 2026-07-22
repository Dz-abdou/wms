import { useQuery } from '@tanstack/react-query'
import { getProductCategories } from './productCategoriesApi'

export const productCategoryKeys = {
  all: ['product-categories'] as const,
}

export function useProductCategories() {
  return useQuery({
    queryKey: productCategoryKeys.all,
    queryFn: ({ signal }) => getProductCategories(signal),
  })
}

