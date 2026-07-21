import { Button, Result } from "antd";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
export function AccessDeniedPage() {
  const { t } = useTranslation();
  return (
    <Result
      status="403"
      title={t("auth.accessDeniedTitle")}
      subTitle={t("auth.accessDeniedDescription")}
      extra={
        <Button type="primary">
          <Link to="/">{t("auth.backHome")}</Link>
        </Button>
      }
    />
  );
}
