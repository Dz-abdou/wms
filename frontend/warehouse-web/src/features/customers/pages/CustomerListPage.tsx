import { Alert, Empty, Input, Select, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { ProductStatusTag } from "../../products/components/ProductStatusTag";
import {
  ListFilter,
  ListPageLayout,
  NewPageAction,
  ReturnAwareLink,
} from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { useCustomers } from "../api/useCustomers";
import type { CustomerListItem } from "../api/customerTypes";
import { customerRoutes } from "../customerConstants";

export function CustomerListPage() {
  const listQuery = useUrlListQuery();
  const { t } = useTranslation();
  const activeValue = listQuery.get("active");
  const { data, error, isLoading, isFetching } = useCustomers({
    ...listQuery.request,
    search: listQuery.get("q"),
    isActive:
      activeValue === "true"
        ? true
        : activeValue === "false"
          ? false
          : undefined,
  });

  const columns = useMemo<ColumnsType<CustomerListItem>>(
    () => [
      { title: t("customers.table.code"), dataIndex: "code", key: "code" },
      {
        title: t("customers.table.legalName"),
        dataIndex: "legalName",
        key: "legalName",
      },
      {
        title: t("customers.table.tradingName"),
        dataIndex: "tradingName",
        key: "tradingName",
        render: (value: string | null) => value ?? t("customers.missingValue"),
      },
      {
        title: t("customers.table.defaultCurrency"),
        dataIndex: "defaultCurrencyCode",
        key: "defaultCurrencyCode",
        render: (value: string | null) => value ?? t("customers.missingValue"),
      },
      {
        title: t("customers.table.status"),
        dataIndex: "isActive",
        key: "isActive",
        render: (isActive: boolean) => <ProductStatusTag isActive={isActive} />,
      },
      {
        title: t("customers.table.actions"),
        key: "actions",
        fixed: "right",
        width: 120,
        render: (_, customer) => (
          <ReturnAwareLink to={customerRoutes.detail(customer.id)}>
            {t("customers.view")}
          </ReturnAwareLink>
        ),
      },
    ],
    [t],
  );

  return (
    <ListPageLayout
      actions={<NewPageAction to={customerRoutes.create} />}
      filters={
        <>
          <ListFilter label={t("ui.search")} width="search">
            <Input.Search
              key={listQuery.get("q") ?? "q"}
              allowClear
              defaultValue={listQuery.get("q")}
              onSearch={(value) => listQuery.update({ q: value })}
              placeholder={t("customers.searchPlaceholder")}
            />
          </ListFilter>
          <ListFilter label={t("customers.table.status")} width="compact">
            <Select
              allowClear
              aria-label={t("customers.table.status")}
              onChange={(value) => listQuery.update({ active: value })}
              options={[
                { value: "true", label: t("products.status.active") },
                { value: "false", label: t("products.status.inactive") },
              ]}
              placeholder={t("customers.table.status")}
              value={activeValue}
            />
          </ListFilter>
        </>
      }
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      subtitle={t("customers.subtitle")}
      title={t("customers.title")}
    >
      {isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("customers.loadingList")}
        />
      ) : null}
      {error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(t, error, "customers.errors.loadList")}
          showIcon
          type="error"
        />
      ) : null}
      {data && data.items.length === 0 ? (
        <Empty description={t("customers.empty")} />
      ) : null}
      {data && data.items.length > 0 ? (
        <Table
          columns={columns}
          dataSource={data.items}
          loading={isFetching}
          pagination={listQuery.toTablePagination(data)}
          rowKey="id"
          scroll={{ x: 900 }}
        />
      ) : null}
    </ListPageLayout>
  );
}
