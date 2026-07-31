import { Alert, Empty, Input, Select, Spin, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import { useInventoryAdjustments } from "../api/useInventory";
import type { InventoryAdjustmentListItem } from "../api/inventoryTypes";
import { inventoryRoutes } from "../inventoryConstants";
import {
  ListFilter,
  ListPageLayout,
  ReturnAwareLink,
  NewPageAction,
} from "../../../shared/components/PageLayouts";

export function InventoryAdjustmentListPage() {
  const { i18n, t } = useTranslation();
  const listQuery = useUrlListQuery();
  const adjustments = useInventoryAdjustments({
    ...listQuery.request,
    reason: listQuery.get("reason") as
      InventoryAdjustmentListItem["reason"] | undefined,
    reference: listQuery.get("reference"),
    fromUtc: listQuery.get("fromUtc"),
    toUtc: listQuery.get("toUtc"),
  });
  const columns: ColumnsType<InventoryAdjustmentListItem> = [
    {
      title: t("inventory.table.reason"),
      dataIndex: "reason",
      render: (value) => t(`inventory.reasons.${value}`),
    },
    {
      title: t("inventory.table.reference"),
      dataIndex: "reference",
      render: (value) => value ?? "—",
    },
    { title: t("inventory.table.lines"), dataIndex: "lineCount" },
    {
      title: t("inventory.table.created"),
      dataIndex: "createdAtUtc",
      render: (value) =>
        formatDateTime(value, toAppLanguage(i18n.resolvedLanguage)),
    },
    {
      title: t("inventory.table.actions"),
      key: "actions",
      fixed: "right",
      width: 120,
      render: (_, item) => (
        <ReturnAwareLink to={inventoryRoutes.adjustmentDetail(item.id)}>
          {t("inventory.view")}
        </ReturnAwareLink>
      ),
    },
  ];
  return (
    <ListPageLayout
      actions={<NewPageAction to={inventoryRoutes.adjustmentCreate} />}
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      filters={
        <>
          <ListFilter label={t("inventory.table.reason")} width="regular">
            <Select
              allowClear
              aria-label={t("inventory.table.reason")}
              onChange={(value) => listQuery.update({ reason: value })}
              options={Object.keys({
                StockCorrection: true,
                Damage: true,
                WriteOff: true,
                FoundStock: true,
                InitialBalance: true,
              }).map((reason) => ({
                value: reason,
                label: t(`inventory.reasons.${reason}`),
              }))}
              placeholder={t("inventory.table.reason")}
              value={listQuery.get("reason")}
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
      subtitle={t("inventory.adjustmentsSubtitle")}
      title={t("inventory.adjustmentsTitle")}
    >
      {adjustments.isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("inventory.loadingAdjustments")}
        />
      ) : adjustments.error ? (
        <Alert
          message={getErrorMessage(
            t,
            adjustments.error,
            "inventory.errors.loadAdjustments",
          )}
          showIcon
          type="error"
        />
      ) : adjustments.data?.items.length === 0 ? (
        <Empty description={t("inventory.emptyAdjustments")} />
      ) : (
        <Table
          columns={columns}
          dataSource={adjustments.data?.items}
          loading={adjustments.isFetching}
          pagination={
            adjustments.data
              ? listQuery.toTablePagination(adjustments.data)
              : false
          }
          rowKey="id"
          scroll={{ x: 850 }}
        />
      )}
    </ListPageLayout>
  );
}
