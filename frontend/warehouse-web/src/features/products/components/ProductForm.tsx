import { Button, Form, Input } from 'antd'
import type { FormInstance } from 'antd'
import { useTranslation } from 'react-i18next'
import { ApiError } from '../../../shared/api/apiClient'
import { getFieldErrorMessages } from '../../../shared/errors/problemDetails'
import type { ProductFormValues } from '../api/productTypes'
import { productValidation } from '../productConstants'

type ProductFormProps = {
  initialValues?: ProductFormValues
  isSubmitting: boolean
  onSubmit: (values: ProductFormValues) => Promise<void>
  submitLabel: string
}

type ProductFormField = keyof ProductFormValues

export function ProductForm({ initialValues, isSubmitting, onSubmit, submitLabel }: ProductFormProps) {
  const [form] = Form.useForm<ProductFormValues>()
  const { t } = useTranslation()

  async function handleSubmit(values: ProductFormValues) {
    try {
      await onSubmit(values)
    } catch (error) {
      if (error instanceof ApiError && error.problem.errors) {
        applyServerErrors(form, error.problem.errors, error.problem.errorCodes, t)
      }
    }
  }

  return (
    <Form<ProductFormValues>
      form={form}
      initialValues={initialValues}
      layout="vertical"
      onFinish={handleSubmit}
      requiredMark="optional"
    >
      <Form.Item
        label={t('products.form.sku')}
        name="sku"
        rules={[
          { required: true, whitespace: true, message: t('products.form.skuRequired') },
          { max: productValidation.maxSkuLength, message: t('products.form.skuMax', { max: productValidation.maxSkuLength }) }
        ]}
      >
        <Input autoComplete="off" maxLength={productValidation.maxSkuLength} placeholder={t('products.form.skuPlaceholder')} />
      </Form.Item>
      <Form.Item
        label={t('products.form.name')}
        name="name"
        rules={[
          { required: true, whitespace: true, message: t('products.form.nameRequired') },
          { max: productValidation.maxNameLength, message: t('products.form.nameMax', { max: productValidation.maxNameLength }) }
        ]}
      >
        <Input maxLength={productValidation.maxNameLength} placeholder={t('products.form.namePlaceholder')} />
      </Form.Item>
      <Form.Item
        label={t('products.form.description')}
        name="description"
        rules={[
          {
            max: productValidation.maxDescriptionLength,
            message: t('products.form.descriptionMax', { max: productValidation.maxDescriptionLength })
          }
        ]}
      >
        <Input.TextArea maxLength={productValidation.maxDescriptionLength} rows={4} showCount />
      </Form.Item>
      <Button htmlType="submit" loading={isSubmitting} type="primary">
        {submitLabel}
      </Button>
    </Form>
  )
}

function applyServerErrors(
  form: FormInstance<ProductFormValues>,
  errors: Record<string, string[]>,
  errorCodes: Record<string, string[]> | undefined,
  t: ReturnType<typeof useTranslation>['t']
) {
  const fields = Object.entries(errors)
    .map(([key]) => {
      const field = toFormField(key)
      return field
        ? {
            name: field,
            errors: getFieldErrorMessages(t, getErrorCodes(errorCodes, key), 'errors.validationFailed')
          }
        : null
    })
    .filter((field): field is { name: ProductFormField; errors: string[] } => field !== null)

  form.setFields(fields)
}

function getErrorCodes(errorCodes: Record<string, string[]> | undefined, fieldName: string): string[] | undefined {
  return errorCodes?.[fieldName] ?? errorCodes?.[capitalize(fieldName)]
}

function capitalize(value: string): string {
  return `${value.charAt(0).toUpperCase()}${value.slice(1)}`
}

function toFormField(key: string): ProductFormField | null {
  switch (key.toLowerCase()) {
    case 'sku':
      return 'sku'
    case 'name':
      return 'name'
    case 'description':
      return 'description'
    default:
      return null
  }
}