import {
  Button,
  Checkbox,
  Form,
  Input,
  Modal,
  Popconfirm,
  Space,
  Table,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import {
  useCreateCustomerAddress,
  useDeleteCustomerAddress,
  useUpdateCustomerAddress,
} from "../api/useCustomers";
import type {
  CustomerAddress,
  CustomerAddressInput,
} from "../api/customerTypes";
import { customerValidation } from "../customerConstants";

type Props = { addresses: CustomerAddress[]; customerId: string };

export function CustomerAddressManager({ addresses, customerId }: Props) {
  const { t } = useTranslation();
  const feedback = useApiFeedback();
  const [editing, setEditing] = useState<CustomerAddress | null | undefined>(
    undefined,
  );
  const [form] = Form.useForm<CustomerAddressInput>();
  const create = useCreateCustomerAddress(customerId);
  const update = useUpdateCustomerAddress(customerId, editing?.id ?? "");
  const remove = useDeleteCustomerAddress(customerId);
  const isOpen = editing !== undefined;

  const columns: ColumnsType<CustomerAddress> = [
    { title: t("customers.addresses.label"), dataIndex: "label", key: "label" },
    {
      title: t("customers.addresses.address"),
      key: "address",
      render: (_, address) =>
        `${address.addressLine1}, ${address.city}, ${address.countryCode}`,
    },
    {
      title: t("customers.addresses.purpose"),
      key: "purpose",
      render: (_, address) =>
        [
          address.isShippingAddress ? t("customers.addresses.shipping") : null,
          address.isBillingAddress ? t("customers.addresses.billing") : null,
        ]
          .filter(Boolean)
          .join(", "),
    },
    {
      title: t("customers.table.actions"),
      key: "actions",
      fixed: "right",
      width: 170,
      render: (_, address) => (
        <Space>
          <Button onClick={() => setEditing(address)} type="link">
            {t("customers.edit")}
          </Button>
          <Popconfirm
            cancelText={t("customers.cancel")}
            description={t("customers.addresses.deleteDescription", {
              label: address.label,
            })}
            okButtonProps={{ danger: true }}
            okText={t("customers.delete")}
            onConfirm={() => deleteAddress(address.id)}
            title={t("customers.addresses.deleteTitle")}
          >
            <Button danger loading={remove.isPending} type="link">
              {t("customers.delete")}
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  async function submit(values: CustomerAddressInput) {
    if (!values.isShippingAddress && !values.isBillingAddress) {
      form.setFields([
        {
          name: "isShippingAddress",
          errors: [t("customers.addresses.purposeRequired")],
        },
      ]);
      return;
    }

    try {
      if (editing) await update.mutateAsync(values);
      else await create.mutateAsync(values);
      setEditing(undefined);
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed")) {
        feedback.notifyError(error, "customers.errors.addressSave");
      }
    }
  }

  async function deleteAddress(addressId: string) {
    try {
      await remove.mutateAsync(addressId);
    } catch (error) {
      feedback.notifyError(error, "customers.errors.addressDelete");
    }
  }

  return (
    <>
      <Space direction="vertical" size="middle" style={{ width: "100%" }}>
        <Button onClick={() => setEditing(null)} type="primary">
          {t("customers.addresses.add")}
        </Button>
        <Table
          columns={columns}
          dataSource={addresses}
          locale={{ emptyText: t("customers.addresses.empty") }}
          pagination={false}
          rowKey="id"
          scroll={{ x: 900 }}
          size="small"
        />
      </Space>
      <Modal
        destroyOnHidden
        onCancel={() => setEditing(undefined)}
        onOk={() => form.submit()}
        okButtonProps={{ loading: create.isPending || update.isPending }}
        okText={editing ? t("customers.save") : t("customers.addresses.add")}
        open={isOpen}
        title={t(
          editing
            ? "customers.addresses.editTitle"
            : "customers.addresses.addTitle",
        )}
      >
        <Form
          form={form}
          initialValues={editing ?? { isShippingAddress: true }}
          layout="vertical"
          onFinish={submit}
          requiredMark="optional"
        >
          <Form.Item
            label={t("customers.addresses.label")}
            name="label"
            rules={[
              {
                required: true,
                whitespace: true,
                message: t("customers.addresses.labelRequired"),
              },
            ]}
          >
            <Input maxLength={customerValidation.maxAddressLabelLength} />
          </Form.Item>
          <Form.Item
            label={t("customers.addresses.addressLine1")}
            name="addressLine1"
            rules={[
              {
                required: true,
                whitespace: true,
                message: t("customers.addresses.addressLine1Required"),
              },
            ]}
          >
            <Input maxLength={customerValidation.maxAddressLineLength} />
          </Form.Item>
          <Form.Item
            label={t("customers.addresses.addressLine2")}
            name="addressLine2"
          >
            <Input maxLength={customerValidation.maxAddressLineLength} />
          </Form.Item>
          <Form.Item
            label={t("customers.addresses.city")}
            name="city"
            rules={[
              {
                required: true,
                whitespace: true,
                message: t("customers.addresses.cityRequired"),
              },
            ]}
          >
            <Input maxLength={customerValidation.maxCityLength} />
          </Form.Item>
          <Form.Item
            label={t("customers.addresses.postalCode")}
            name="postalCode"
          >
            <Input maxLength={customerValidation.maxPostalCodeLength} />
          </Form.Item>
          <Form.Item
            label={t("customers.addresses.countryCode")}
            name="countryCode"
            rules={[
              {
                required: true,
                whitespace: true,
                len: 2,
                message: t("customers.addresses.countryCodeRequired"),
              },
            ]}
          >
            <Input maxLength={2} />
          </Form.Item>
          <Form.Item
            label={t("customers.addresses.purpose")}
            name="isShippingAddress"
            valuePropName="checked"
          >
            <Checkbox>{t("customers.addresses.shipping")}</Checkbox>
          </Form.Item>
          <Form.Item name="isBillingAddress" valuePropName="checked">
            <Checkbox>{t("customers.addresses.billing")}</Checkbox>
          </Form.Item>
          <Form.Item
            label={t("customers.addresses.deliveryInstructions")}
            name="deliveryInstructions"
          >
            <Input.TextArea
              maxLength={customerValidation.maxAddressInstructionsLength}
              rows={3}
              showCount
            />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
