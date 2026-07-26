import {
  Alert,
  Empty,
  Input,
  Popconfirm,
  Select,
  Space,
  Spin,
  Table,
  Tag,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import {
  ListFilter,
  NewPageAction,
  ReturnAwareLink,
  ListPageLayout,
} from "../../../shared/components/PageLayouts";
import { useUrlListQuery } from "../../../shared/pagination/pagination";
import type { AdministrationUser } from "../api/administrationTypes";
import {
  useAdministrationUsers,
  useDeleteAdministrationUser,
} from "../api/useAdministration";
import {
  administrationRoles,
  administrationRoutes,
} from "../administrationConstants";

export function UsersPage() {
  const { t } = useTranslation();
  const listQuery = useUrlListQuery();
  const users = useAdministrationUsers({
    ...listQuery.request,
    email: listQuery.get("q"),
    role: listQuery.get("role"),
  });
  const deleteUser = useDeleteAdministrationUser();
  const columns: ColumnsType<AdministrationUser> = [
    {
      title: t("administration.users.table.email"),
      dataIndex: "email",
      key: "email",
    },
    {
      title: t("administration.users.table.roles"),
      dataIndex: "roles",
      key: "roles",
      render: (roles: string[]) =>
        roles.map((role) => (
          <Tag key={role}>{t(`administration.roles.names.${role}`)}</Tag>
        )),
    },
    {
      title: t("administration.users.table.actions"),
      key: "actions",
      render: (_, user) => (
        <Space>
          <ReturnAwareLink to={administrationRoutes.userEdit(user.id)}>
            {t("administration.users.edit")}
          </ReturnAwareLink>
          <Popconfirm
            cancelText={t("ui.cancel")}
            okText={t("administration.users.delete")}
            onConfirm={() => deleteUser.mutate(user.id)}
            title={t("administration.users.deleteConfirm")}
          >
            <a>{t("administration.users.delete")}</a>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <ListPageLayout
      actions={<NewPageAction to={administrationRoutes.userCreate} />}
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
              placeholder={t("administration.users.searchPlaceholder")}
            />
          </ListFilter>
          <ListFilter
            label={t("administration.users.table.roles")}
            width="regular"
          >
            <Select
              allowClear
              aria-label={t("administration.users.table.roles")}
              onChange={(value) => listQuery.update({ role: value })}
              options={administrationRoles.map((role) => ({
                value: role,
                label: t(`administration.roles.names.${role}`),
              }))}
              placeholder={t("administration.users.table.roles")}
              value={listQuery.get("role")}
            />
          </ListFilter>
        </>
      }
      subtitle={t("administration.users.subtitle")}
      title={t("administration.users.title")}
    >
      {users.isLoading ? <Spin className="page-spinner" size="large" /> : null}
      {users.error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            users.error,
            "administration.users.errors.load",
          )}
          showIcon
          type="error"
        />
      ) : null}
      {users.data?.items.length === 0 ? (
        <Empty
          className="page-empty"
          description={t("administration.users.empty")}
        />
      ) : null}
      {users.data && users.data.items.length > 0 ? (
        <Table
          columns={columns}
          dataSource={users.data.items}
          loading={users.isFetching}
          pagination={listQuery.toTablePagination(users.data)}
          rowKey="id"
        />
      ) : null}
    </ListPageLayout>
  );
}
