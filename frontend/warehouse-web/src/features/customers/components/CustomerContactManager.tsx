import { Button, Form, Input, Modal, Popconfirm, Space, Table } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import {
  useCreateCustomerContact,
  useDeleteCustomerContact,
  useUpdateCustomerContact,
} from "../api/useCustomers";
import type {
  CustomerContact,
  CustomerContactInput,
} from "../api/customerTypes";
import { customerValidation } from "../customerConstants";

type Props = { contacts: CustomerContact[]; customerId: string };

export function CustomerContactManager({ contacts, customerId }: Props) {
  const { t } = useTranslation();
  const feedback = useApiFeedback();
  const [editing, setEditing] = useState<CustomerContact | null | undefined>(
    undefined,
  );
  const [form] = Form.useForm<CustomerContactInput>();
  const create = useCreateCustomerContact(customerId);
  const update = useUpdateCustomerContact(customerId, editing?.id ?? "");
  const remove = useDeleteCustomerContact(customerId);
  const isOpen = editing !== undefined;

  const columns: ColumnsType<CustomerContact> = [
    { title: t("customers.contacts.name"), dataIndex: "name", key: "name" },
    {
      title: t("customers.contacts.role"),
      dataIndex: "role",
      key: "role",
      render: (value) => value ?? t("customers.missingValue"),
    },
    {
      title: t("customers.contacts.email"),
      dataIndex: "email",
      key: "email",
      render: (value) => value ?? t("customers.missingValue"),
    },
    {
      title: t("customers.contacts.phone"),
      dataIndex: "phoneNumber",
      key: "phoneNumber",
      render: (value) => value ?? t("customers.missingValue"),
    },
    {
      title: t("customers.table.actions"),
      key: "actions",
      fixed: "right",
      width: 170,
      render: (_, contact) => (
        <Space>
          <Button onClick={() => setEditing(contact)} type="link">
            {t("customers.edit")}
          </Button>
          <Popconfirm
            cancelText={t("customers.cancel")}
            description={t("customers.contacts.deleteDescription", {
              name: contact.name,
            })}
            okButtonProps={{ danger: true }}
            okText={t("customers.delete")}
            onConfirm={() => deleteContact(contact.id)}
            title={t("customers.contacts.deleteTitle")}
          >
            <Button danger loading={remove.isPending} type="link">
              {t("customers.delete")}
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  async function submit(values: CustomerContactInput) {
    try {
      if (editing) await update.mutateAsync(values);
      else await create.mutateAsync(values);
      setEditing(undefined);
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed")) {
        feedback.notifyError(error, "customers.errors.contactSave");
      }
    }
  }

  async function deleteContact(contactId: string) {
    try {
      await remove.mutateAsync(contactId);
    } catch (error) {
      feedback.notifyError(error, "customers.errors.contactDelete");
    }
  }

  return (
    <>
      <Space direction="vertical" size="middle" style={{ width: "100%" }}>
        <Button onClick={() => setEditing(null)} type="primary">
          {t("customers.contacts.add")}
        </Button>
        <Table
          columns={columns}
          dataSource={contacts}
          locale={{ emptyText: t("customers.contacts.empty") }}
          pagination={false}
          rowKey="id"
          scroll={{ x: 800 }}
          size="small"
        />
      </Space>
      <Modal
        destroyOnHidden
        onCancel={() => setEditing(undefined)}
        onOk={() => form.submit()}
        okButtonProps={{ loading: create.isPending || update.isPending }}
        okText={editing ? t("customers.save") : t("customers.contacts.add")}
        open={isOpen}
        title={t(
          editing
            ? "customers.contacts.editTitle"
            : "customers.contacts.addTitle",
        )}
      >
        <Form
          form={form}
          initialValues={editing ?? undefined}
          layout="vertical"
          onFinish={submit}
          requiredMark="optional"
        >
          <Form.Item
            label={t("customers.contacts.name")}
            name="name"
            rules={[
              {
                required: true,
                whitespace: true,
                message: t("customers.contacts.nameRequired"),
              },
            ]}
          >
            <Input maxLength={customerValidation.maxContactNameLength} />
          </Form.Item>
          <Form.Item label={t("customers.contacts.role")} name="role">
            <Input maxLength={customerValidation.maxContactRoleLength} />
          </Form.Item>
          <Form.Item label={t("customers.contacts.email")} name="email">
            <Input maxLength={customerValidation.maxEmailLength} type="email" />
          </Form.Item>
          <Form.Item label={t("customers.contacts.phone")} name="phoneNumber">
            <Input maxLength={customerValidation.maxPhoneNumberLength} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
