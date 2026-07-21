import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { ApiError } from '../../../shared/api/apiClient'
import { i18n } from '../../../shared/i18n/i18n'
import { ProductForm } from './ProductForm'

describe('ProductForm', () => {
  it('blocks a blank SKU and name before submitting', async () => {
    const user = userEvent.setup()
    const onSubmit = vi.fn()

    render(<ProductForm isSubmitting={false} onSubmit={onSubmit} submitLabel="Create product" />)
    await user.click(screen.getByRole('button', { name: 'Create product' }))

    expect(await screen.findByText('SKU is required.')).toBeInTheDocument()
    expect(await screen.findByText('Name is required.')).toBeInTheDocument()
    expect(onSubmit).not.toHaveBeenCalled()
  })

  it('translates server validation error codes on their fields', async () => {
    await i18n.changeLanguage('fr')
    const user = userEvent.setup()
    const onSubmit = vi.fn().mockRejectedValue(
      new ApiError(400, {
        errorCodes: { Sku: ['validation.required'] },
        errors: { Sku: ['SKU is required.'] }
      })
    )

    render(<ProductForm isSubmitting={false} onSubmit={onSubmit} submitLabel="Créer le produit" />)
    await user.type(screen.getByLabelText('Référence'), 'SKU-001')
    await user.type(screen.getByLabelText('Nom'), 'Sample product')
    await user.click(screen.getByRole('button', { name: 'Créer le produit' }))

    await waitFor(() => expect(screen.getByText('Ce champ est obligatoire.')).toBeInTheDocument())
  })
})