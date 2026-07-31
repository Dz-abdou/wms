import { Alert, Empty, Select, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { ProductStatusTag } from "../../products/components/ProductStatusTag";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { useSupplierProducts } from "../api/usePurchasing";
import { usePurchasingCurrencies } from "../api/usePurchasing";
import { useSuppliers } from "../../suppliers/api/useSuppliers";
import { useProducts } from "../../products/api/useProducts";
import type { SupplierProduct } from "../api/purchasingTypes";
import { purchasingRoutes } from "../purchasingConstants";
import {
  ListFilter,
  ListPageLayout,
  ReturnAwareLink,
  NewPageAction,
} from "../../../shared/components/PageLayouts";

export function SupplierCatalogueListPage() {
  const { t } = useTranslation();
  const listQuery = useUrlListQuery();
  const [supplierSearch, setSupplierSearch] = useState("");
  const [productSearch, setProductSearch] = useState("");
  const activeValue = listQuery.get("active");
  const catalogue = useSupplierProducts({
    ...listQuery.request,
    supplierId: listQuery.get("supplierId"),
    productId: listQuery.get("productId"),
    isActive:
      activeValue === "true"
        ? true
        : activeValue === "false"
          ? false
          : undefined,
    currencyCode: listQuery.get("currencyCode"),
  });
  const suppliers = useSuppliers({
    page: 1,
    pageSize: 20,
    search: supplierSearch,
  });
  const products = useProducts({
    page: 1,
    pageSize: 20,
    search: productSearch,
  });
  const currencies = usePurchasingCurrencies();
  const columns = useMemo<ColumnsType<SupplierProduct>>(
    () => [
      {
        title: t("purchasing.catalogue.supplier"),
        key: "supplier",
        render: (_, item) => `${item.supplierCode} — ${item.supplierName}`,
      },
      {
        title: t("purchasing.catalogue.product"),
        key: "product",
        render: (_, item) => `${item.productSku} — ${item.productName}`,
      },
      {
        title: t("purchasing.catalogue.purchaseUnit"),
        dataIndex: "purchaseUnitOfMeasure",
        key: "purchaseUnitOfMeasure",
      },
      {
        title: t("purchasing.catalogue.unitPrice"),
        key: "unitPrice",
        render: (_, item) => `${item.unitPrice} ${item.currencyCode}`,
      },
      {
        title: t("purchasing.catalogue.status"),
        dataIndex: "isActive",
        key: "isActive",
        render: (isActive: boolean) => <ProductStatusTag isActive={isActive} />,
      },
      {
        title: t("purchasing.catalogue.actions"),
        key: "actions",
        fixed: "right",
        width: 120,
        render: (_, item) => (
          <ReturnAwareLink to={purchasingRoutes.catalogueEdit(item.id)}>
            {t("purchasing.edit")}
          </ReturnAwareLink>
        ),
      },
    ],
    [t],
  );
  return (
    <ListPageLayout
      actions={<NewPageAction to={purchasingRoutes.catalogueCreate} />}
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      filters={
        <>
          <ListFilter
            label={t("purchasing.catalogue.supplier")}
            width="regular"
          >
            <Select
              allowClear
              aria-label={t("purchasing.catalogue.supplier")}
              filterOption={false}
              onChange={(value) => listQuery.update({ supplierId: value })}
              onSearch={setSupplierSearch}
              options={(suppliers.data?.items ?? []).map((supplier) => ({
                value: supplier.id,
                label: `${supplier.code} — ${supplier.name}`,
              }))}
              placeholder={t("purchasing.catalogue.supplier")}
              showSearch
              value={listQuery.get("supplierId")}
            />
          </ListFilter>
          <ListFilter label={t("purchasing.catalogue.product")} width="regular">
            <Select
              allowClear
              aria-label={t("purchasing.catalogue.product")}
              filterOption={false}
              onChange={(value) => listQuery.update({ productId: value })}
              onSearch={setProductSearch}
              options={(products.data?.items ?? []).map((product) => ({
                value: product.id,
                label: `${product.sku} — ${product.name}`,
              }))}
              placeholder={t("purchasing.catalogue.product")}
              showSearch
              value={listQuery.get("productId")}
            />
          </ListFilter>
          <ListFilter label={t("purchasing.catalogue.status")} width="compact">
            <Select
              allowClear
              aria-label={t("purchasing.catalogue.status")}
              onChange={(value) => listQuery.update({ active: value })}
              options={[
                { value: "true", label: t("products.status.active") },
                { value: "false", label: t("products.status.inactive") },
              ]}
              placeholder={t("purchasing.catalogue.status")}
              value={activeValue}
            />
          </ListFilter>
          <ListFilter
            label={t("purchasing.catalogue.currencyCode")}
            width="compact"
          >
            <Select
              allowClear
              aria-label={t("purchasing.catalogue.currencyCode")}
              onChange={(value) => listQuery.update({ currencyCode: value })}
              options={(currencies.data ?? []).map((currency) => ({
                value: currency.code,
                label: currency.code,
              }))}
              placeholder={t("purchasing.catalogue.currencyCode")}
              value={listQuery.get("currencyCode")}
            />
          </ListFilter>
        </>
      }
      subtitle={t("purchasing.catalogue.subtitle")}
      title={t("purchasing.catalogue.title")}
    >
      {catalogue.isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("purchasing.catalogue.loading")}
        />
      ) : null}
      {catalogue.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            catalogue.error,
            "purchasing.catalogue.errors.load",
          )}
          showIcon
          type="error"
        />
      ) : null}
      {catalogue.data && catalogue.data.items.length === 0 ? (
        <Empty description={t("purchasing.catalogue.empty")} />
      ) : null}
      {catalogue.data && catalogue.data.items.length > 0 ? (
        <Table
          columns={columns}
          dataSource={catalogue.data.items}
          loading={catalogue.isFetching}
          pagination={listQuery.toTablePagination(catalogue.data)}
          rowKey="id"
          scroll={{ x: 1000 }}
        />
      ) : null}
    </ListPageLayout>
  );
}
