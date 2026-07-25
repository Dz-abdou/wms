import {
  Alert,
  Button,
  Form,
  Input,
  Modal,
  Select,
  Table,
  Typography,
} from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import {
  useCreateProductCategory,
  useProductCategories,
  useUpdateProductCategory,
} from "../api/useProductCategories";
import type { ProductCategory } from "../api/productTypes";

type Values = Pick<ProductCategory, "code" | "name" | "parentCategoryId">;
export function ProductCategoryListPage() {
  const { t } = useTranslation();
  const feedback = useApiFeedback();
  const [editing, setEditing] = useState<ProductCategory | undefined>();
  const categories = useProductCategories();
  const create = useCreateProductCategory();
  const update = useUpdateProductCategory();
  const [form] = Form.useForm<Values>();
  async function submit(values: Values) {
    try {
      if (editing?.id)
        await update.mutateAsync({ id: editing.id, input: values });
      else await create.mutateAsync(values);
      setEditing(undefined);
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed"))
        feedback.notifyError(error, "masterData.categories.errors.save");
    }
  }
  const error = categories.error ?? create.error ?? update.error;
  return (
    <section>
      <div className="page-heading">
        <div>
          <Typography.Title level={2}>
            {t("masterData.categories.title")}
          </Typography.Title>
          <Typography.Paragraph type="secondary">
            {t("masterData.categories.subtitle")}
          </Typography.Paragraph>
        </div>
        <Button
          type="primary"
          onClick={() => setEditing({} as ProductCategory)}
        >
          {t("masterData.new")}
        </Button>
      </div>
      {error ? (
        <Alert
          className="page-alert"
          message={getErrorMessage(
            t,
            error,
            "masterData.categories.errors.save",
          )}
          showIcon
          type="error"
        />
      ) : null}
      <Table
        rowKey="id"
        loading={categories.isLoading}
        dataSource={categories.data?.items}
        pagination={false}
        columns={[
          { title: t("masterData.code"), dataIndex: "code" },
          { title: t("masterData.name"), dataIndex: "name" },
          {
            title: t("masterData.actions"),
            render: (_, category) => (
              <Button type="link" onClick={() => setEditing(category)}>
                {t("masterData.edit")}
              </Button>
            ),
          },
        ]}
      />
      <Modal
        destroyOnHidden
        open={editing !== undefined}
        footer={null}
        title={
          editing?.id
            ? t("masterData.categories.edit")
            : t("masterData.categories.new")
        }
        onCancel={() => setEditing(undefined)}
      >
        <Form
          form={form}
          initialValues={editing?.id ? editing : undefined}
          layout="vertical"
          onFinish={submit}
        >
          <Form.Item
            label={t("masterData.code")}
            name="code"
            rules={[
              {
                required: true,
                message: t("errors.codes.validation.required"),
              },
            ]}
          >
            <Input />
          </Form.Item>
          <Form.Item
            label={t("masterData.name")}
            name="name"
            rules={[
              {
                required: true,
                message: t("errors.codes.validation.required"),
              },
            ]}
          >
            <Input />
          </Form.Item>
          <Form.Item
            extra={t("masterData.categories.parentHelp")}
            label={t("masterData.categories.parent")}
            name="parentCategoryId"
          >
            <Select
              allowClear
              options={(categories.data?.items ?? [])
                .filter((category) => category.id !== editing?.id)
                .map((category) => ({
                  value: category.id,
                  label: `${category.code} — ${category.name}`,
                }))}
            />
          </Form.Item>
          <Button
            htmlType="submit"
            loading={create.isPending || update.isPending}
            type="primary"
          >
            {t("masterData.save")}
          </Button>
        </Form>
      </Modal>
    </section>
  );
}
