import { Alert, Empty, Select, Spin, Table, Tag } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import type { AdministrationUser } from "../api/administrationTypes";
import {
  useAdministrationRoles,
  useAdministrationUsers,
  useSetAdministrationUserRoles,
} from "../api/useAdministration";
import { ListPageLayout } from "../../../shared/components/PageLayouts";
import { useUrlListQuery } from "../../../shared/pagination/pagination";

export function RolesPage() {
  const { t } = useTranslation();
  const listQuery = useUrlListQuery();
  const {
    data: roles,
    error: rolesError,
    isLoading: isLoadingRoles,
  } = useAdministrationRoles();
  const {
    data: users,
    error: usersError,
    isLoading: isLoadingUsers,
  } = useAdministrationUsers(listQuery.request);
  const setRoles = useSetAdministrationUserRoles();

  const columns = useMemo<ColumnsType<AdministrationUser>>(
    () => [
      {
        title: t("administration.roles.table.email"),
        dataIndex: "email",
        key: "email",
      },
      {
        title: t("administration.roles.table.assignments"),
        key: "roles",
        render: (_, user) => (
          <Select
            mode="multiple"
            onChange={(selectedRoles: string[]) =>
              setRoles.mutate({ id: user.id, roles: selectedRoles })
            }
            options={(roles ?? []).map((role) => ({
              label: t(`administration.roles.names.${role}`),
              value: role,
            }))}
            value={user.roles}
          />
        ),
      },
    ],
    [roles, setRoles, t],
  );

  const error = rolesError ?? usersError ?? setRoles.error;

  return (
    <ListPageLayout
      subtitle={t("administration.roles.subtitle")}
      title={t("administration.roles.title")}
    >
      {isLoadingRoles || isLoadingUsers ? (
        <Spin className="page-spinner" size="large" />
      ) : null}
      {error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            error,
            "administration.roles.errors.load",
          )}
          showIcon
          type="error"
        />
      ) : null}
      {roles ? (
        <div className="role-list">
          {roles.map((role) => (
            <Tag key={role}>{t(`administration.roles.names.${role}`)}</Tag>
          ))}
        </div>
      ) : null}
      {users && users.items.length === 0 ? (
        <Empty
          className="page-empty"
          description={t("administration.roles.empty")}
        />
      ) : null}
      {users && users.items.length > 0 ? (
        <Table
          columns={columns}
          dataSource={users.items}
          pagination={listQuery.toTablePagination(users)}
          rowKey="id"
        />
      ) : null}
    </ListPageLayout>
  );
}
