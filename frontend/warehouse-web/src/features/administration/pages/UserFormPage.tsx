import { Alert, Card, Form, Input, Spin } from "antd";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { FormPageActions } from "../../../shared/components/FormPageActions";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import type { CreateUserValues } from "../api/administrationTypes";
import {
  useAdministrationUser,
  useCreateAdministrationUser,
  useUpdateAdministrationUser,
} from "../api/useAdministration";
import { administrationRoutes } from "../administrationConstants";

export function UserFormPage({ editing }: { editing: boolean }) {
  const { id } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const feedback = useApiFeedback();
  const user = useAdministrationUser(editing ? id : undefined);
  const create = useCreateAdministrationUser();
  const update = useUpdateAdministrationUser();
  const [form] = Form.useForm<CreateUserValues>();
  const { goBack, returnTo } = useReturnDestination(administrationRoutes.users);

  if (user.isLoading) return <Spin className="page-spinner" size="large" />;
  if (editing && (user.error || !user.data))
    return (
      <Alert
        message={getErrorMessage(
          t,
          user.error,
          "administration.users.errors.load",
        )}
        showIcon
        type="error"
      />
    );

  async function submit(values: CreateUserValues) {
    try {
      if (editing && id)
        await update.mutateAsync({ id, values: { email: values.email } });
      else await create.mutateAsync(values);
      navigate(returnTo);
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed"))
        feedback.notifyError(error, "administration.users.errors.save");
    }
  }

  return (
    <FormPageLayout
      backLabel={t("administration.users.title")}
      backTo={returnTo}
      title={t(
        editing
          ? "administration.users.editTitle"
          : "administration.users.createTitle",
      )}
    >
      <Card>
        <Form
          form={form}
          initialValues={user.data}
          layout="vertical"
          onFinish={submit}
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
          {!editing ? (
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
          <FormPageActions
            cancelLabel={t("ui.cancel")}
            isSubmitting={create.isPending || update.isPending}
            onCancel={goBack}
            submitLabel={t("ui.save")}
          />
        </Form>
      </Card>
    </FormPageLayout>
  );
}
