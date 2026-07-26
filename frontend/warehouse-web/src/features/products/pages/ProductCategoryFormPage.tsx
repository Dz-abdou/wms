import { Alert, Card, Form, Input, Select, Spin } from "antd";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { FormPageActions } from "../../../shared/components/FormPageActions";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import {
  useCreateProductCategory,
  useProductCategories,
  useProductCategory,
  useUpdateProductCategory,
} from "../api/useProductCategories";
import type { ProductCategory } from "../api/productTypes";
import { productRoutes } from "../productConstants";
type Values = Pick<ProductCategory, "code" | "name" | "parentCategoryId">;
export function ProductCategoryFormPage({ editing }: { editing: boolean }) {
  const { id } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const feedback = useApiFeedback();
  const categories = useProductCategories({ page: 1, pageSize: 100 });
  const categoryQuery = useProductCategory(editing ? id : undefined);
  const create = useCreateProductCategory();
  const update = useUpdateProductCategory();
  const [form] = Form.useForm<Values>();
  const { goBack, returnTo } = useReturnDestination(productRoutes.categories);
  const category = categoryQuery.data;
  if (categories.isLoading || categoryQuery.isLoading)
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("masterData.categories.loading")}
      />
    );
  if (editing && !category)
    return (
      <Alert
        message={t("errors.codes.product_category.not_found")}
        showIcon
        type="error"
      />
    );
  async function submit(values: Values) {
    try {
      if (category)
        await update.mutateAsync({ id: category.id, input: values });
      else await create.mutateAsync(values);
      navigate(returnTo);
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed"))
        feedback.notifyError(error, "masterData.categories.errors.save");
    }
  }
  return (
    <FormPageLayout
      backLabel={t("masterData.categories.title")}
      backTo={returnTo}
      title={t(
        category ? "masterData.categories.edit" : "masterData.categories.new",
      )}
    >
      <Card>
        <Form
          form={form}
          initialValues={category}
          layout="vertical"
          onFinish={submit}
        >
          <Form.Item
            label={t("masterData.code")}
            name="code"
            rules={[{ required: true }]}
          >
            <Input />
          </Form.Item>
          <Form.Item
            label={t("masterData.name")}
            name="name"
            rules={[{ required: true }]}
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
                .filter((x) => x.id !== category?.id)
                .map((x) => ({ value: x.id, label: `${x.code} — ${x.name}` }))}
            />
          </Form.Item>
          <FormPageActions
            cancelLabel={t("ui.cancel")}
            isSubmitting={create.isPending || update.isPending}
            onCancel={goBack}
            submitLabel={t("masterData.save")}
          />
        </Form>
      </Card>
    </FormPageLayout>
  );
}
