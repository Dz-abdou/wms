import { ConfigProvider } from "antd";
import enUs from "antd/locale/en_US";
import frFr from "antd/locale/fr_FR";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { RolesPage } from "../features/administration/pages/RolesPage";
import { UsersPage } from "../features/administration/pages/UsersPage";
import {
  administrationRoutes,
  administratorRole,
} from "../features/administration/administrationConstants";
import { WarehouseCreatePage } from "../features/warehouses/pages/WarehouseCreatePage";
import { WarehouseDetailPage } from "../features/warehouses/pages/WarehouseDetailPage";
import { WarehouseEditPage } from "../features/warehouses/pages/WarehouseEditPage";
import { WarehouseListPage } from "../features/warehouses/pages/WarehouseListPage";
import { warehouseRoutes } from "../features/warehouses/warehouseConstants";
import { InventoryPage } from "../features/inventory/pages/InventoryPage";
import { inventoryRoutes } from "../features/inventory/inventoryConstants";
import { LoginPage } from "../features/auth/pages/LoginPage";
import { AuthProvider } from "../features/auth/AuthContext";
import { ProtectedRoute } from "../features/auth/components/ProtectedRoute";
import { AccessDeniedPage } from "../features/auth/pages/AccessDeniedPage";
import { ProductCreatePage } from "../features/products/pages/ProductCreatePage";
import { ProductDetailPage } from "../features/products/pages/ProductDetailPage";
import { ProductEditPage } from "../features/products/pages/ProductEditPage";
import { ProductListPage } from "../features/products/pages/ProductListPage";
import { productRoutes } from "../features/products/productConstants";
import { toAppLanguage } from "../shared/i18n/constants";
import "../shared/i18n/i18n";
import { ApplicationLayout } from "../layouts/ApplicationLayout";
import { HomePage } from "../pages/home/HomePage";
import { AppProviders } from "./AppProviders";
import { applicationTheme } from "./theme";

export function App() {
  const { i18n } = useTranslation();
  const antdLocale =
    toAppLanguage(i18n.resolvedLanguage) === "fr" ? frFr : enUs;

  return (
    <ConfigProvider locale={antdLocale} theme={applicationTheme}>
      <AppProviders>
        <BrowserRouter>
          <AuthProvider>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route element={<ProtectedRoute />}>
                <Route path="/access-denied" element={<AccessDeniedPage />} />
                <Route element={<ApplicationLayout />}>
                  <Route index element={<HomePage />} />
                  <Route
                    path={productRoutes.listPattern}
                    element={<ProductListPage />}
                  />
                  <Route
                    path={productRoutes.create}
                    element={<ProductCreatePage />}
                  />
                  <Route
                    path={productRoutes.detailPattern}
                    element={<ProductDetailPage />}
                  />
                  <Route
                    path={productRoutes.editPattern}
                    element={<ProductEditPage />}
                  />
                  <Route
                    path={warehouseRoutes.listPattern}
                    element={<WarehouseListPage />}
                  />
                  <Route
                    path={inventoryRoutes.dashboardPattern}
                    element={<InventoryPage />}
                  />
                  <Route
                    path={warehouseRoutes.create}
                    element={<WarehouseCreatePage />}
                  />
                  <Route
                    path={warehouseRoutes.detailPattern}
                    element={<WarehouseDetailPage />}
                  />
                  <Route
                    path={warehouseRoutes.editPattern}
                    element={<WarehouseEditPage />}
                  />
                  <Route
                    element={<ProtectedRoute roles={[administratorRole]} />}
                  >
                    <Route
                      path={administrationRoutes.usersPattern}
                      element={<UsersPage />}
                    />
                    <Route
                      path={administrationRoutes.rolesPattern}
                      element={<RolesPage />}
                    />
                  </Route>
                </Route>
              </Route>
            </Routes>
          </AuthProvider>
        </BrowserRouter>
      </AppProviders>
    </ConfigProvider>
  );
}
