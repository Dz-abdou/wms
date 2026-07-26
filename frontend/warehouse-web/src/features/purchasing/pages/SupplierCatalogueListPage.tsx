import { Alert, Empty, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { ProductStatusTag } from "../../products/components/ProductStatusTag";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useListPagination } from "../../../shared/pagination/pagination";
import { useSupplierProducts } from "../api/usePurchasing";
import type { SupplierProduct } from "../api/purchasingTypes";
import { purchasingRoutes } from "../purchasingConstants";
import {
  ListPageLayout,
  ReturnAwareLink,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";

export function SupplierCatalogueListPage() {
  const { t } = useTranslation();
  const pagination = useListPagination();
  const catalogue = useSupplierProducts(pagination.page, pagination.pageSize);
  const columns = useMemo<ColumnsType<SupplierProduct>>(() => [
    { title: t("purchasing.catalogue.supplier"), key: "supplier", render: (_, item) => `${item.supplierCode} — ${item.supplierName}` },
    { title: t("purchasing.catalogue.product"), key: "product", render: (_, item) => `${item.productSku} — ${item.productName}` },
    { title: t("purchasing.catalogue.purchaseUnit"), dataIndex: "purchaseUnitOfMeasure", key: "purchaseUnitOfMeasure" },
    { title: t("purchasing.catalogue.unitPrice"), key: "unitPrice", render: (_, item) => `${item.unitPrice} ${item.currencyCode}` },
    { title: t("purchasing.catalogue.status"), dataIndex: "isActive", key: "isActive", render: (isActive: boolean) => <ProductStatusTag isActive={isActive} /> },
    { title: t("purchasing.catalogue.actions"), key: "actions", render: (_, item) => <ReturnAwareLink to={purchasingRoutes.catalogueEdit(item.id)}>{t("purchasing.edit")}</ReturnAwareLink> },
  ], [t]);
  return <ListPageLayout
    actions={<RouteActionButton to={purchasingRoutes.catalogueCreate} type="primary">{t("purchasing.catalogue.new")}</RouteActionButton>}
    subtitle={t("purchasing.catalogue.subtitle")}
    title={t("purchasing.catalogue.title")}
  >
    {catalogue.isLoading ? <Spin className="page-spinner" size="large" tip={t("purchasing.catalogue.loading")} /> : null}
    {catalogue.error ? <Alert className="page-alert" message={getErrorMessage(t, catalogue.error, "purchasing.catalogue.errors.load")} showIcon type="error" /> : null}
    {catalogue.data && catalogue.data.items.length === 0 ? <Empty description={t("purchasing.catalogue.empty")} /> : null}
    {catalogue.data && catalogue.data.items.length > 0 ? <Table columns={columns} dataSource={catalogue.data.items} loading={catalogue.isFetching} pagination={pagination.toTablePagination(catalogue.data)} rowKey="id" /> : null}
  </ListPageLayout>;
}
