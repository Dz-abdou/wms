import { Alert, Empty, Input, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import type { Product } from "../api/productTypes";
import { useProducts } from "../api/useProducts";
import { ProductStatusTag } from "../components/ProductStatusTag";
import { productRoutes } from "../productConstants";
import { useListPagination } from "../../../shared/pagination/pagination";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import {
  ListPageLayout,
  ReturnAwareLink,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";

export function ProductListPage() {
  const pagination = useListPagination();
  const [search, setSearch] = useState("");
  const { t } = useTranslation();
  const { data, error, isLoading, isFetching } = useProducts({
    ...pagination.request,
    search,
  });

  const columns = useMemo<ColumnsType<Product>>(
    () => [
      { title: t("products.table.sku"), dataIndex: "sku", key: "sku" },
      { title: t("products.table.name"), dataIndex: "name", key: "name" },
      {
        title: t("products.table.status"),
        dataIndex: "isActive",
        key: "isActive",
        render: (isActive: boolean) => <ProductStatusTag isActive={isActive} />,
      },
      {
        title: t("products.table.actions"),
        key: "actions",
        render: (_, product) => (
          <ReturnAwareLink to={productRoutes.detail(product.id)}>
            {t("products.view")}
          </ReturnAwareLink>
        ),
      },
    ],
    [t],
  );

  return (
    <ListPageLayout
      actions={
        <RouteActionButton to={productRoutes.create} type="primary">
          {t("products.new")}
        </RouteActionButton>
      }
      subtitle={t("products.subtitle")}
      title={t("products.title")}
    >

      <div className="page-filter-toolbar">
        <Input.Search
          allowClear
          className="product-search"
          onSearch={(value) => {
            pagination.resetPage();
            setSearch(value);
          }}
          placeholder={t("products.searchPlaceholder")}
        />
      </div>

      {isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("products.loadingList")}
        />
      ) : null}
      {error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(t, error, "products.errors.loadList")}
          showIcon
          type="error"
        />
      ) : null}
      {data && data.items.length === 0 ? (
        <Empty className="page-empty" description={t("products.empty")} />
      ) : null}
      {data && data.items.length > 0 ? (
        <Table<Product>
          columns={columns}
          dataSource={data.items}
          loading={isFetching}
          pagination={pagination.toTablePagination(data)}
          rowKey="id"
        />
      ) : null}
    </ListPageLayout>
  );
}
