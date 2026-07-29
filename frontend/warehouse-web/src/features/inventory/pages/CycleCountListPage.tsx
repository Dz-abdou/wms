import { Alert, Empty, Input, Select, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useTranslation } from "react-i18next";
import {
  ListFilter,
  ListPageLayout,
  NewPageAction,
  ReturnAwareLink,
} from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { useWarehouses } from "../../warehouses/api/useWarehouses";
import { useCycleCounts } from "../api/useInventory";
import type { CycleCountListItem } from "../api/inventoryTypes";
import { inventoryPageSize, inventoryRoutes } from "../inventoryConstants";

export function CycleCountListPage() {
  const { i18n, t } = useTranslation();
  const listQuery = useUrlListQuery();
  const warehouses = useWarehouses({ page: 1, pageSize: inventoryPageSize });
  const cycleCounts = useCycleCounts({
    ...listQuery.request,
    warehouseId: listQuery.get("warehouseId"),
    reference: listQuery.get("reference"),
    fromUtc: listQuery.get("fromUtc"),
    toUtc: listQuery.get("toUtc"),
  });
  const columns: ColumnsType<CycleCountListItem> = [
    {
      title: t("inventory.table.warehouse"),
      key: "warehouse",
      render: (_, item) => `${item.warehouseCode} — ${item.warehouseName}`,
    },
    {
      title: t("inventory.table.reference"),
      dataIndex: "reference",
      render: (value) => value ?? "—",
    },
    { title: t("inventory.table.lines"), dataIndex: "lineCount" },
    {
      title: t("inventory.cycleCounts.varianceLines"),
      dataIndex: "varianceLineCount",
    },
    {
      title: t("inventory.cycleCounts.countedAt"),
      dataIndex: "countedAtUtc",
      render: (value) =>
        formatDateTime(value, toAppLanguage(i18n.resolvedLanguage)),
    },
    {
      title: t("inventory.table.actions"),
      key: "actions",
      render: (_, item) => (
        <ReturnAwareLink to={inventoryRoutes.cycleCountDetail(item.id)}>
          {t("inventory.view")}
        </ReturnAwareLink>
      ),
    },
  ];

  return (
    <ListPageLayout
      actions={<NewPageAction to={inventoryRoutes.cycleCountCreate} />}
      filters={
        <>
          <ListFilter label={t("inventory.table.warehouse")} width="regular">
            <Select
              allowClear
              aria-label={t("inventory.table.warehouse")}
              onChange={(value) => listQuery.update({ warehouseId: value })}
              options={warehouses.data?.items
                .filter((warehouse) => warehouse.isActive)
                .map((warehouse) => ({
                  value: warehouse.id,
                  label: `${warehouse.code} — ${warehouse.name}`,
                }))}
              placeholder={t("inventory.table.warehouse")}
              showSearch
              optionFilterProp="label"
              value={listQuery.get("warehouseId")}
            />
          </ListFilter>
          <ListFilter label={t("inventory.table.reference")} width="regular">
            <Input
              allowClear
              aria-label={t("inventory.table.reference")}
              defaultValue={listQuery.get("reference")}
              key={listQuery.get("reference") ?? "reference"}
              onPressEnter={(event) =>
                listQuery.update({ reference: event.currentTarget.value })
              }
              placeholder={t("inventory.table.reference")}
            />
          </ListFilter>
          <ListFilter label={t("inventory.filters.fromDate")} width="compact">
            <Input
              aria-label={t("inventory.filters.fromDate")}
              onChange={(event) =>
                listQuery.update({
                  fromUtc: event.target.value
                    ? `${event.target.value}T00:00:00.000Z`
                    : undefined,
                })
              }
              type="date"
              value={listQuery.get("fromUtc")?.slice(0, 10)}
            />
          </ListFilter>
          <ListFilter label={t("inventory.filters.toDate")} width="compact">
            <Input
              aria-label={t("inventory.filters.toDate")}
              onChange={(event) =>
                listQuery.update({
                  toUtc: event.target.value
                    ? `${event.target.value}T23:59:59.999Z`
                    : undefined,
                })
              }
              type="date"
              value={listQuery.get("toUtc")?.slice(0, 10)}
            />
          </ListFilter>
        </>
      }
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      subtitle={t("inventory.cycleCounts.subtitle")}
      title={t("inventory.cycleCounts.title")}
    >
      {cycleCounts.isLoading || warehouses.isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("inventory.cycleCounts.loading")}
        />
      ) : cycleCounts.error || warehouses.error ? (
        <Alert
          message={getErrorMessage(
            t,
            cycleCounts.error ?? warehouses.error,
            "inventory.errors.loadCycleCounts",
          )}
          showIcon
          type="error"
        />
      ) : cycleCounts.data?.items.length === 0 ? (
        <Empty description={t("inventory.cycleCounts.empty")} />
      ) : (
        <Table
          columns={columns}
          dataSource={cycleCounts.data?.items}
          loading={cycleCounts.isFetching}
          pagination={
            cycleCounts.data
              ? listQuery.toTablePagination(cycleCounts.data)
              : false
          }
          rowKey="id"
        />
      )}
    </ListPageLayout>
  );
}
