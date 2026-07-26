import { Alert, Button, Empty, Input, Select, Spin, Table, Tag } from "antd";
import { useTranslation } from "react-i18next";
import {
  ListFilter,
  ListPageLayout,
  NewPageAction,
  ReturnAwareLink,
} from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import {
  useCurrencies,
  useSetCurrencyStatus,
  useSetDefaultCurrency,
} from "../api/usePurchasing";
import { purchasingRoutes } from "../purchasingConstants";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
export function CurrencyListPage() {
  const { t } = useTranslation();
  const listQuery = useUrlListQuery();
  const feedback = useApiFeedback();
  const activeValue = listQuery.get("active");
  const currencies = useCurrencies({
    ...listQuery.request,
    search: listQuery.get("q"),
    isActive:
      activeValue === "true"
        ? true
        : activeValue === "false"
          ? false
          : undefined,
  });
  const status = useSetCurrencyStatus();
  const setDefault = useSetDefaultCurrency();
  const run = async (action: () => Promise<unknown>) => {
    try {
      await action();
    } catch (error) {
      feedback.notifyError(error, "masterData.currencies.errors.save");
    }
  };
  return (
    <ListPageLayout
      actions={<NewPageAction to={purchasingRoutes.currencyCreate} />}
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
              placeholder={t("masterData.currencies.searchPlaceholder")}
            />
          </ListFilter>
          <ListFilter label={t("masterData.status")} width="compact">
            <Select
              allowClear
              aria-label={t("masterData.status")}
              onChange={(value) => listQuery.update({ active: value })}
              options={[
                { value: "true", label: t("masterData.active") },
                { value: "false", label: t("masterData.inactive") },
              ]}
              placeholder={t("masterData.status")}
              value={activeValue}
            />
          </ListFilter>
        </>
      }
      subtitle={t("masterData.currencies.subtitle")}
      title={t("masterData.currencies.title")}
    >
      {currencies.isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("masterData.currencies.loading")}
        />
      ) : null}
      {currencies.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            currencies.error,
            "masterData.currencies.errors.save",
          )}
          showIcon
          type="error"
        />
      ) : null}
      {currencies.data?.items.length === 0 ? (
        <Empty
          className="page-empty"
          description={t("masterData.currencies.empty")}
        />
      ) : null}
      {currencies.data && currencies.data.items.length > 0 ? (
        <Table
          rowKey="id"
          dataSource={currencies.data.items}
          loading={currencies.isFetching}
          pagination={listQuery.toTablePagination(currencies.data)}
          columns={[
            { title: t("masterData.code"), dataIndex: "code" },
            { title: t("masterData.name"), dataIndex: "name" },
            { title: t("masterData.symbol"), dataIndex: "symbol" },
            { title: t("masterData.decimals"), dataIndex: "decimalPlaces" },
            {
              title: t("masterData.status"),
              render: (_, x) =>
                x.isActive ? (
                  <Tag color="green">{t("masterData.active")}</Tag>
                ) : (
                  <Tag>{t("masterData.inactive")}</Tag>
                ),
            },
            {
              title: t("masterData.actions"),
              render: (_, x) => (
                <>
                  <ReturnAwareLink to={purchasingRoutes.currencyEdit(x.id)}>
                    {t("masterData.edit")}
                  </ReturnAwareLink>
                  <Button
                    type="link"
                    disabled={x.isDefault}
                    onClick={() => void run(() => setDefault.mutateAsync(x.id))}
                  >
                    {t("masterData.default")}
                  </Button>
                  <Button
                    type="link"
                    disabled={x.isDefault}
                    onClick={() =>
                      void run(() =>
                        status.mutateAsync({ id: x.id, isActive: !x.isActive }),
                      )
                    }
                  >
                    {x.isActive
                      ? t("masterData.deactivate")
                      : t("masterData.activate")}
                  </Button>
                </>
              ),
            },
          ]}
        />
      ) : null}
    </ListPageLayout>
  );
}
