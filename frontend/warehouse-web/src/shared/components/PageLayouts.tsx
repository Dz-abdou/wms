import { Button, Space, Typography } from "antd";
import type { ReactNode } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { locationTarget } from "../navigation/returnNavigation";

type PageLayoutProps = {
  actions?: ReactNode;
  backTo?: string;
  backLabel?: string;
  children: ReactNode;
  subtitle?: ReactNode;
  title: ReactNode;
};

function PageLayout({
  actions,
  backTo,
  backLabel,
  children,
  subtitle,
  title,
}: PageLayoutProps) {
  const { t } = useTranslation();

  return (
    <section className="page-layout">
      {backTo && backLabel ? (
        <Link className="page-back-link" to={backTo}>
          ← {t("ui.backTo", { destination: backLabel })}
        </Link>
      ) : null}
      <div className="page-heading">
        <div>
          <Typography.Title level={2}>{title}</Typography.Title>
          {subtitle ? (
            <Typography.Paragraph type="secondary">
              {subtitle}
            </Typography.Paragraph>
          ) : null}
        </div>
        {actions ? <Space className="page-actions">{actions}</Space> : null}
      </div>
      {children}
    </section>
  );
}

export function ListPageLayout(
  props: Omit<PageLayoutProps, "backLabel" | "backTo">,
) {
  return <PageLayout {...props} />;
}

export function FormPageLayout(props: PageLayoutProps) {
  return <PageLayout {...props} />;
}

export function DetailPageLayout(props: PageLayoutProps) {
  return <PageLayout {...props} />;
}

type RouteActionButtonProps = {
  children: ReactNode;
  to: string;
  type?: "default" | "primary" | "text" | "link";
};

/** Navigates accessibly while preserving the current page as the safe return path. */
export function RouteActionButton({
  children,
  to,
  type = "default",
}: RouteActionButtonProps) {
  const location = useLocation();
  const navigate = useNavigate();

  return (
    <Button
      onClick={() => navigate(to, { state: { returnTo: locationTarget(location) } })}
      type={type}
    >
      {children}
    </Button>
  );
}

type ReturnAwareLinkProps = {
  children: ReactNode;
  to: string;
};

/** Use for a row/detail link that should return to the current list context. */
export function ReturnAwareLink({ children, to }: ReturnAwareLinkProps) {
  const location = useLocation();

  return (
    <Link state={{ returnTo: locationTarget(location) }} to={to}>
      {children}
    </Link>
  );
}
