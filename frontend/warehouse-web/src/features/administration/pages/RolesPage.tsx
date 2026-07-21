import { Alert, Empty, Select, Spin, Table, Tag, Typography } from "antd";
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

export function RolesPage() {
  const { t } = useTranslation();
  const {
    data: roles,
    error: rolesError,
    isLoading: isLoadingRoles,
  } = useAdministrationRoles();
  const {
    data: users,
    error: usersError,
    isLoading: isLoadingUsers,
  } = useAdministrationUsers();
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
    <section>
      <div className="page-heading">
        <div>
          <Typography.Title level={2}>
            {t("administration.roles.title")}
          </Typography.Title>
          <Typography.Paragraph type="secondary">
            {t("administration.roles.subtitle")}
          </Typography.Paragraph>
        </div>
      </div>

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
      {users && users.length === 0 ? (
        <Empty
          className="page-empty"
          description={t("administration.roles.empty")}
        />
      ) : null}
      {users && users.length > 0 ? (
        <Table
          columns={columns}
          dataSource={users}
          pagination={false}
          rowKey="id"
        />
      ) : null}
    </section>
  );
}
