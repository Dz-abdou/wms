import { Alert, Button, Card, Empty, Form, InputNumber, Radio, Select, Spin, Statistic, Table, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../../shared/errors/problemDetails'
import { formatDateTime } from '../../../shared/formatting/dateTime'
import { toAppLanguage } from '../../../shared/i18n/constants'
import { useProducts } from '../../products/api/useProducts'
import { useWarehouses } from '../../warehouses/api/useWarehouses'
import { inventoryPageSize } from '../inventoryConstants'
import { useAdjustInventory, useMovementHistory } from '../api/useInventory'
import type { InventoryAdjustmentInput, InventoryMovement } from '../api/inventoryTypes'

type InventorySelection = Pick<InventoryAdjustmentInput, 'productId' | 'warehouseId'>

export function InventoryPage() {
  const { i18n, t } = useTranslation()
  const [selection, setSelection] = useState<InventorySelection>()
  const products = useProducts({ page: 1, pageSize: inventoryPageSize })
  const warehouses = useWarehouses(1, inventoryPageSize)
  const movements = useMovementHistory(selection?.productId, selection?.warehouseId)
  const adjustment = useAdjustInventory()
  const language = toAppLanguage(i18n.resolvedLanguage)

  if (products.isLoading || warehouses.isLoading) {
    return <Spin className="page-spinner" size="large" tip={t('inventory.loadingSources')} />
  }

  if (products.error || warehouses.error) {
    return <Alert message={getErrorMessage(t, products.error ?? warehouses.error, 'inventory.errors.loadSources')} showIcon type="error" />
  }

  const columns: ColumnsType<InventoryMovement> = [
    { title: t('inventory.table.type'), dataIndex: 'type', key: 'type', render: (type) => t(type === 'ManualIncrease' ? 'inventory.types.increase' : 'inventory.types.decrease') },
    { title: t('inventory.table.delta'), dataIndex: 'quantityDelta', key: 'quantityDelta' },
    { title: t('inventory.table.balanceAfter'), dataIndex: 'balanceAfter', key: 'balanceAfter' },
    { title: t('inventory.table.created'), dataIndex: 'createdAtUtc', key: 'createdAtUtc', render: (value) => formatDateTime(value, language) },
  ]

  async function submit(input: InventoryAdjustmentInput) {
    const balance = await adjustment.mutateAsync(input)
    setSelection({ productId: input.productId, warehouseId: input.warehouseId })
    return balance
  }

  return <section>
    <div className="page-heading">
      <Typography.Title level={2}>{t('inventory.title')}</Typography.Title>
      <Typography.Paragraph>{t('inventory.subtitle')}</Typography.Paragraph>
    </div>
    {adjustment.error ? <Alert className="page-alert" message={getErrorMessage(t, adjustment.error, 'inventory.errors.adjust')} showIcon type="error" /> : null}
    <Card title={t('inventory.adjustTitle')}>
      <Form<InventoryAdjustmentInput> layout="vertical" onFinish={submit} onValuesChange={(values: Partial<InventorySelection>) => {
        if (values.productId || values.warehouseId) setSelection((current) => ({ ...current, ...values } as InventorySelection))
      }}>
        <Form.Item label={t('inventory.form.product')} name="productId" rules={[{ required: true, message: t('inventory.form.productRequired') }]}>
          <Select options={products.data?.items.filter((product) => product.isActive).map((product) => ({ value: product.id, label: `${product.sku} — ${product.name}` }))} />
        </Form.Item>
        <Form.Item label={t('inventory.form.warehouse')} name="warehouseId" rules={[{ required: true, message: t('inventory.form.warehouseRequired') }]}>
          <Select options={warehouses.data?.items.filter((warehouse) => warehouse.isActive).map((warehouse) => ({ value: warehouse.id, label: `${warehouse.code} — ${warehouse.name}` }))} />
        </Form.Item>
        <Form.Item label={t('inventory.form.direction')} name="direction" initialValue="Increase">
          <Radio.Group options={[{ value: 'Increase', label: t('inventory.types.increase') }, { value: 'Decrease', label: t('inventory.types.decrease') }]} />
        </Form.Item>
        <Form.Item label={t('inventory.form.quantity')} name="quantity" rules={[{ required: true, message: t('inventory.form.quantityRequired') }]}>
          <InputNumber min={0.001} precision={3} />
        </Form.Item>
        <Button htmlType="submit" loading={adjustment.isPending} type="primary">{t('inventory.adjust')}</Button>
      </Form>
    </Card>
    {adjustment.data ? <Card className="page-alert"><Statistic title={t('inventory.currentBalance')} value={adjustment.data.quantity} /></Card> : null}
    <Card title={t('inventory.historyTitle')}>
      {!selection ? <Empty description={t('inventory.selectForHistory')} /> : movements.isLoading ? <Spin tip={t('inventory.loadingHistory')} /> : movements.error ? <Alert message={getErrorMessage(t, movements.error, 'inventory.errors.loadHistory')} showIcon type="error" /> : movements.data?.items.length === 0 ? <Empty description={t('inventory.emptyHistory')} /> : <Table columns={columns} dataSource={movements.data?.items} pagination={false} rowKey="id" />}
    </Card>
  </section>
}
