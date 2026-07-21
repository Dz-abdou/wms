import { Spin } from "antd";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../AuthContext";
export function ProtectedRoute({ roles }: { roles?: string[] }) {
  const { isLoading, session } = useAuth();
  const location = useLocation();
  const { t } = useTranslation();
  if (isLoading) return <Spin tip={t("auth.loading")} />;
  if (!session)
    return <Navigate replace to="/login" state={{ from: location }} />;
  if (roles && !roles.some((role) => session.roles.includes(role)))
    return <Navigate replace to="/access-denied" />;
  return <Outlet />;
}
export const catalogManagerRoles = ["admin", "manager"];
