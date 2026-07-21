import { Alert, Card, Typography } from 'antd'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ApiError } from '../../../shared/api/apiClient'
import { getProblemMessage } from '../../../shared/errors/problemDetails'
import type { ProductFormValues } from '../api/productTypes'
import { useCreateProduct } from '../api/useProducts'
import { ProductForm } from '../components/ProductForm'
import { productRoutes } from '../productConstants'

export function ProductCreatePage() {
  const navigate = useNavigate()
  const createProduct = useCreateProduct()
  const { t } = useTranslation()

  async function handleSubmit(values: ProductFormValues) {
    const product = await createProduct.mutateAsync(values)
    navigate(productRoutes.detail(product.id))
  }

  const hasServerValidationErrors = createProduct.error instanceof ApiError && Boolean(createProduct.error.problem.errors)
  const errorMessage = createProduct.error instanceof ApiError
    ? getProblemMessage(t, createProduct.error.problem, 'products.errors.create')
    : t('products.errors.create')

  return (
    <section>
      <Typography.Title level={2}>{t('products.createTitle')}</Typography.Title>
      {createProduct.error && !hasServerValidationErrors ? (
        <Alert className="page-alert" message={errorMessage} showIcon type="error" />
      ) : null}
      <Card>
        <ProductForm isSubmitting={createProduct.isPending} onSubmit={handleSubmit} submitLabel={t('products.create')} />
      </Card>
    </section>
  )
}