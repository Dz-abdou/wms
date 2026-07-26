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
  filters?: ReactNode;
  filterActions?: ReactNode;
  onBack?: () => void;
  subtitle?: ReactNode;
  title: ReactNode;
};

function PageLayout({
  actions,
  backTo,
  backLabel,
  children,
  filters,
  filterActions,
  onBack,
  subtitle,
  title,
}: PageLayoutProps) {
  const { t } = useTranslation();

  return (
    <section className="page-layout">
      {backTo && backLabel && onBack ? (
        <Button className="page-back-link" onClick={onBack} type="link">
          ← {t("ui.backTo", { destination: backLabel })}
        </Button>
      ) : backTo && backLabel ? (
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
      {filters ? (
        <div className="page-filter-toolbar">
          <div className="list-filter-controls">{filters}</div>
          {filterActions ? (
            <div className="list-filter-actions">{filterActions}</div>
          ) : null}
        </div>
      ) : null}
      {children}
    </section>
  );
}

type ListPageLayoutProps = Omit<
  PageLayoutProps,
  "backLabel" | "backTo" | "filterActions"
> & {
  hasActiveFilters?: boolean;
  onClearFilters?: () => void;
};

export function ListPageLayout({
  hasActiveFilters = false,
  onClearFilters,
  ...props
}: ListPageLayoutProps) {
  const { t } = useTranslation();
  const filterActions = onClearFilters ? (
    <Button disabled={!hasActiveFilters} onClick={onClearFilters} type="link">
      {t("ui.clearFilters")}
    </Button>
  ) : undefined;

  return <PageLayout {...props} filterActions={filterActions} />;
}

export function FormPageLayout(props: PageLayoutProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const confirmBack = () => {
    if (
      window.confirm(
        `${t("ui.discardChangesTitle")}\n\n${t("ui.discardChangesDescription")}`,
      )
    ) {
      navigate(props.backTo ?? "");
    }
  };

  return <PageLayout {...props} onBack={confirmBack} />;
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
      onClick={() =>
        navigate(to, { state: { returnTo: locationTarget(location) } })
      }
      type={type}
    >
      {children}
    </Button>
  );
}

/** The single, consistent primary action on a list page. */
export function NewPageAction({ to }: { to: string }) {
  const { t } = useTranslation();
  return (
    <RouteActionButton to={to} type="primary">
      {t("ui.new")}
    </RouteActionButton>
  );
}

type ListFilterProps = {
  children: ReactNode;
  label: ReactNode;
  width?: "compact" | "regular" | "search";
};

/** A labeled, compact filter field for a shared list toolbar. */
export function ListFilter({
  children,
  label,
  width = "regular",
}: ListFilterProps) {
  return (
    <label className={`list-filter list-filter-${width}`}>
      <span className="list-filter-label">{label}</span>
      {children}
    </label>
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
