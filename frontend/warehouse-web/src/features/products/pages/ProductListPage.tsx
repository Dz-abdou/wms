import { Alert, Empty, Input, Select, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import type { Product } from "../api/productTypes";
import { useProducts } from "../api/useProducts";
import { useProductCategories } from "../api/useProductCategories";
import { ProductStatusTag } from "../components/ProductStatusTag";
import { productRoutes } from "../productConstants";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import {
  ListPageLayout,
  ListFilter,
  ReturnAwareLink,
  NewPageAction,
} from "../../../shared/components/PageLayouts";

export function ProductListPage() {
  const listQuery = useUrlListQuery();
  const { t } = useTranslation();
  const activeValue = listQuery.get("active");
  const categories = useProductCategories({ page: 1, pageSize: 100 });
  const { data, error, isLoading, isFetching } = useProducts({
    ...listQuery.request,
    search: listQuery.get("q"),
    isActive:
      activeValue === "true"
        ? true
        : activeValue === "false"
          ? false
          : undefined,
    categoryId: listQuery.get("categoryId"),
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
      actions={<NewPageAction to={productRoutes.create} />}
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      filters={
        <>
          <ListFilter label={t("ui.search")} width="search">
            <Input.Search
              key={listQuery.get("q") ?? "q"}
              allowClear
              defaultValue={listQuery.get("q")}
              onSearch={(value) => listQuery.update({ q: value })}
              placeholder={t("products.searchPlaceholder")}
            />
          </ListFilter>
          <ListFilter label={t("products.table.status")} width="compact">
            <Select
              allowClear
              aria-label={t("products.table.status")}
              onChange={(value) => listQuery.update({ active: value })}
              options={[
                { value: "true", label: t("products.status.active") },
                { value: "false", label: t("products.status.inactive") },
              ]}
              placeholder={t("products.table.status")}
              value={activeValue}
            />
          </ListFilter>
          <ListFilter label={t("products.form.category")}>
            <Select
              allowClear
              aria-label={t("products.form.category")}
              onChange={(value) => listQuery.update({ categoryId: value })}
              options={(categories.data?.items ?? []).map((category) => ({
                value: category.id,
                label: `${category.code} — ${category.name}`,
              }))}
              placeholder={t("products.form.category")}
              value={listQuery.get("categoryId")}
            />
          </ListFilter>
        </>
      }
      subtitle={t("products.subtitle")}
      title={t("products.title")}
    >
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
          pagination={listQuery.toTablePagination(data)}
          rowKey="id"
        />
      ) : null}
    </ListPageLayout>
  );
}
