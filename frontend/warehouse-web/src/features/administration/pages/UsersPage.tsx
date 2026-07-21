import {
  Alert,
  Button,
  Empty,
  Form,
  Input,
  Modal,
  Popconfirm,
  Space,
  Spin,
  Table,
  Tag,
  Typography,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import type {
  AdministrationUser,
  CreateUserValues,
} from "../api/administrationTypes";
import {
  useAdministrationUsers,
  useCreateAdministrationUser,
  useDeleteAdministrationUser,
  useUpdateAdministrationUser,
} from "../api/useAdministration";

export function UsersPage() {
  const { t } = useTranslation();
  const { data: users, error, isLoading } = useAdministrationUsers();
  const createUser = useCreateAdministrationUser();
  const updateUser = useUpdateAdministrationUser();
  const deleteUser = useDeleteAdministrationUser();
  const [editingUser, setEditingUser] = useState<AdministrationUser>();
  const [isModalOpen, setIsModalOpen] = useState(false);

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
          <Button
            onClick={() => {
              setEditingUser(user);
              setIsModalOpen(true);
            }}
            type="link"
          >
            {t("administration.users.edit")}
          </Button>
          <Popconfirm
            cancelText={t("administration.users.cancel")}
            okText={t("administration.users.delete")}
            onConfirm={() => deleteUser.mutate(user.id)}
            title={t("administration.users.deleteConfirm")}
          >
            <Button danger type="link">
              {t("administration.users.delete")}
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  function closeModal() {
    setIsModalOpen(false);
    setEditingUser(undefined);
  }

  async function saveUser(values: CreateUserValues) {
    try {
      if (editingUser) {
        await updateUser.mutateAsync({
          id: editingUser.id,
          values: { email: values.email },
        });
      } else {
        await createUser.mutateAsync(values);
      }
      closeModal();
    } catch {
      // Mutation state displays the localized error in the dialog.
    }
  }

  return (
    <section>
      <div className="page-heading">
        <div>
          <Typography.Title level={2}>
            {t("administration.users.title")}
          </Typography.Title>
          <Typography.Paragraph type="secondary">
            {t("administration.users.subtitle")}
          </Typography.Paragraph>
        </div>
        <Button
          onClick={() => {
            setEditingUser(undefined);
            setIsModalOpen(true);
          }}
          type="primary"
        >
          {t("administration.users.new")}
        </Button>
      </div>

      {isLoading ? <Spin className="page-spinner" size="large" /> : null}
      {error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            error,
            "administration.users.errors.load",
          )}
          showIcon
          type="error"
        />
      ) : null}
      {users && users.length === 0 ? (
        <Empty
          className="page-empty"
          description={t("administration.users.empty")}
        />
      ) : null}
      {users && users.length > 0 ? (
        <Table columns={columns} dataSource={users} rowKey="id" />
      ) : null}

      <UserModal
        error={createUser.error ?? updateUser.error}
        isEditing={Boolean(editingUser)}
        onCancel={closeModal}
        onSubmit={saveUser}
        open={isModalOpen}
        user={editingUser}
      />
    </section>
  );
}

type UserModalProps = {
  error: unknown;
  isEditing: boolean;
  onCancel: () => void;
  onSubmit: (values: CreateUserValues) => Promise<void>;
  open: boolean;
  user: AdministrationUser | undefined;
};

function UserModal({
  error,
  isEditing,
  onCancel,
  onSubmit,
  open,
  user,
}: UserModalProps) {
  const { t } = useTranslation();
  const [form] = Form.useForm<CreateUserValues>();

  return (
    <Modal
      footer={null}
      onCancel={onCancel}
      open={open}
      title={t(
        isEditing
          ? "administration.users.editTitle"
          : "administration.users.createTitle",
      )}
    >
      {error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            error,
            "administration.users.errors.save",
          )}
          showIcon
          type="error"
        />
      ) : null}
      <Form
        form={form}
        initialValues={{ email: user?.email }}
        key={user?.id ?? "new-user"}
        layout="vertical"
        onFinish={(values) => void onSubmit(values)}
      >
        <Form.Item
          label={t("administration.users.email")}
          name="email"
          rules={[
            {
              required: true,
              message: t("administration.users.emailRequired"),
            },
          ]}
        >
          <Input type="email" />
        </Form.Item>
        {!isEditing ? (
          <Form.Item
            label={t("administration.users.password")}
            name="password"
            rules={[
              {
                required: true,
                message: t("administration.users.passwordRequired"),
              },
            ]}
          >
            <Input.Password />
          </Form.Item>
        ) : null}
        <Space>
          <Button htmlType="submit" type="primary">
            {t("administration.users.save")}
          </Button>
          <Button onClick={onCancel}>{t("administration.users.cancel")}</Button>
        </Space>
      </Form>
    </Modal>
  );
}
