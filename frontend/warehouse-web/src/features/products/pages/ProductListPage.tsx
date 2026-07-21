import { Alert, Button, Empty, Input, Spin, Table, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import type { Product } from '../api/productTypes'
import { useProducts } from '../api/useProducts'
import { ProductStatusTag } from '../components/ProductStatusTag'
import { productPagination, productRoutes } from '../productConstants'
import { getErrorMessage } from '../../../shared/errors/problemDetails'

export function ProductListPage() {
  const [page, setPage] = useState<number>(productPagination.defaultPage)
  const [pageSize, setPageSize] = useState<number>(productPagination.defaultPageSize)
  const [search, setSearch] = useState('')
  const { t } = useTranslation()
  const { data, error, isLoading, isFetching } = useProducts({ page, pageSize, search })

  const columns = useMemo<ColumnsType<Product>>(
    () => [
      { title: t('products.table.sku'), dataIndex: 'sku', key: 'sku' },
      { title: t('products.table.name'), dataIndex: 'name', key: 'name' },
      {
        title: t('products.table.status'),
        dataIndex: 'isActive',
        key: 'isActive',
        render: (isActive: boolean) => <ProductStatusTag isActive={isActive} />
      },
      {
        title: t('products.table.actions'),
        key: 'actions',
        render: (_, product) => <Link to={productRoutes.detail(product.id)}>{t('products.view')}</Link>
      }
    ],
    [t]
  )

  return (
    <section>
      <div className="page-heading">
        <div>
          <Typography.Title level={2}>{t('products.title')}</Typography.Title>
          <Typography.Paragraph type="secondary">{t('products.subtitle')}</Typography.Paragraph>
        </div>
        <Button type="primary">
          <Link to={productRoutes.create}>{t('products.new')}</Link>
        </Button>
      </div>

      <Input.Search
        allowClear
        className="product-search"
        onSearch={(value) => {
          setPage(productPagination.defaultPage)
          setSearch(value)
        }}
        placeholder={t('products.searchPlaceholder')}
      />

      {isLoading ? <Spin className="page-spinner" size="large" tip={t('products.loadingList')} /> : null}
      {error ? <Alert className="page-alert" message={getErrorMessage(t, error, 'products.errors.loadList')} showIcon type="error" /> : null}
      {data && data.items.length === 0 ? <Empty className="page-empty" description={t('products.empty')} /> : null}
      {data && data.items.length > 0 ? (
        <Table<Product>
          columns={columns}
          dataSource={data.items}
          loading={isFetching}
          pagination={{
            current: data.page,
            pageSize: data.pageSize,
            total: data.totalCount,
            showSizeChanger: true,
            onChange: (nextPage, nextPageSize) => {
              setPage(nextPage)
              setPageSize(nextPageSize)
            }
          }}
          rowKey="id"
        />
      ) : null}
    </section>
  )
}