import { Alert, Button, Card, Form, Input, Typography } from "antd";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ApiError } from "../../../shared/api/apiClient";
import { useAuth } from "../AuthContext";

export function LoginPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { signIn } = useAuth();
  const [error, setError] = useState<string>();
  const [isSubmitting, setIsSubmitting] = useState(false);
  async function submit(values: { email: string; password: string }) {
    setIsSubmitting(true);
    setError(undefined);
    try {
      await signIn(values.email, values.password);
      navigate("/");
    } catch (exception) {
      setError(
        exception instanceof ApiError ? exception.problem.code : undefined,
      );
    } finally {
      setIsSubmitting(false);
    }
  }
  return (
    <main className="login-page">
      <Card className="login-card">
        <Typography.Title level={2}>{t("auth.title")}</Typography.Title>
        {error ? (
          <Alert
            message={t(`errors.codes.${error}`, {
              defaultValue: t("errors.requestFailed"),
            })}
            type="error"
          />
        ) : null}
        <Form layout="vertical" onFinish={submit}>
          <Form.Item
            label={t("auth.email")}
            name="email"
            rules={[{ required: true, message: t("auth.emailRequired") }]}
          >
            <Input type="email" />
          </Form.Item>
          <Form.Item
            label={t("auth.password")}
            name="password"
            rules={[{ required: true, message: t("auth.passwordRequired") }]}
          >
            <Input.Password />
          </Form.Item>
          <Button block htmlType="submit" loading={isSubmitting} type="primary">
            {t("auth.submit")}
          </Button>
        </Form>
      </Card>
    </main>
  );
}
