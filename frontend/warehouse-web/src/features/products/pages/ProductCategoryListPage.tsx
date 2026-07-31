import { Alert, Empty, Input, Spin, Table } from "antd";
import { useTranslation } from "react-i18next";
import {
  ListPageLayout,
  ListFilter,
  NewPageAction,
  ReturnAwareLink,
} from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useProductCategories } from "../api/useProductCategories";
import { productRoutes } from "../productConstants";
import { useUrlListQuery } from "../../../shared/pagination/pagination";

export function ProductCategoryListPage() {
  const { t } = useTranslation();
  const listQuery = useUrlListQuery();
  const categories = useProductCategories({
    ...listQuery.request,
    search: listQuery.get("q"),
  });
  return (
    <ListPageLayout
      actions={<NewPageAction to={productRoutes.categoryCreate} />}
      hasActiveFilters={listQuery.hasFilters}
      onClearFilters={listQuery.clearFilters}
      filters={
        <ListFilter label={t("ui.search")} width="search">
          <Input.Search
            key={listQuery.get("q") ?? "q"}
            allowClear
            defaultValue={listQuery.get("q")}
            onSearch={(value) => listQuery.update({ q: value })}
            placeholder={t("masterData.categories.searchPlaceholder")}
          />
        </ListFilter>
      }
      subtitle={t("masterData.categories.subtitle")}
      title={t("masterData.categories.title")}
    >
      {categories.isLoading ? (
        <Spin
          className="page-spinner"
          size="large"
          tip={t("masterData.categories.loading")}
        />
      ) : null}
      {categories.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            categories.error,
            "masterData.categories.errors.save",
          )}
          showIcon
          type="error"
        />
      ) : null}
      {categories.data?.items.length === 0 ? (
        <Empty
          className="page-empty"
          description={t("masterData.categories.empty")}
        />
      ) : null}
      {categories.data && categories.data.items.length > 0 ? (
        <Table
          rowKey="id"
          dataSource={categories.data.items}
          loading={categories.isFetching}
          pagination={listQuery.toTablePagination(categories.data)}
          columns={[
            { title: t("masterData.code"), dataIndex: "code" },
            { title: t("masterData.name"), dataIndex: "name" },
            {
              title: t("masterData.actions"),
              key: "actions",
              fixed: "right",
              width: 120,
              render: (_, item) => (
                <ReturnAwareLink to={productRoutes.categoryEdit(item.id)}>
                  {t("masterData.edit")}
                </ReturnAwareLink>
              ),
            },
          ]}
          scroll={{ x: 600 }}
        />
      ) : null}
    </ListPageLayout>
  );
}
