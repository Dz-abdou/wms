import { ConfigProvider } from "antd";
import enUs from "antd/locale/en_US";
import frFr from "antd/locale/fr_FR";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { RolesPage } from "../features/administration/pages/RolesPage";
import { UsersPage } from "../features/administration/pages/UsersPage";
import { UserFormPage } from "../features/administration/pages/UserFormPage";
import {
  administrationRoutes,
  administratorRole,
} from "../features/administration/administrationConstants";
import { WarehouseCreatePage } from "../features/warehouses/pages/WarehouseCreatePage";
import { WarehouseDetailPage } from "../features/warehouses/pages/WarehouseDetailPage";
import { WarehouseEditPage } from "../features/warehouses/pages/WarehouseEditPage";
import { WarehouseListPage } from "../features/warehouses/pages/WarehouseListPage";
import { warehouseRoutes } from "../features/warehouses/warehouseConstants";
import { SupplierCreatePage } from "../features/suppliers/pages/SupplierCreatePage";
import { SupplierDetailPage } from "../features/suppliers/pages/SupplierDetailPage";
import { SupplierEditPage } from "../features/suppliers/pages/SupplierEditPage";
import { SupplierListPage } from "../features/suppliers/pages/SupplierListPage";
import { supplierRoutes } from "../features/suppliers/supplierConstants";
import { SupplierCatalogueCreatePage } from "../features/purchasing/pages/SupplierCatalogueCreatePage";
import { SupplierCatalogueEditPage } from "../features/purchasing/pages/SupplierCatalogueEditPage";
import { SupplierCatalogueListPage } from "../features/purchasing/pages/SupplierCatalogueListPage";
import { PurchaseOrderCreatePage } from "../features/purchasing/pages/PurchaseOrderCreatePage";
import { PurchaseOrderDetailPage } from "../features/purchasing/pages/PurchaseOrderDetailPage";
import { PurchaseOrderEditPage } from "../features/purchasing/pages/PurchaseOrderEditPage";
import { PurchaseOrderListPage } from "../features/purchasing/pages/PurchaseOrderListPage";
import { purchasingRoutes } from "../features/purchasing/purchasingConstants";
import { GoodsReceiptCreatePage } from "../features/receiving/pages/GoodsReceiptCreatePage";
import { GoodsReceiptDetailPage } from "../features/receiving/pages/GoodsReceiptDetailPage";
import { GoodsReceiptListPage } from "../features/receiving/pages/GoodsReceiptListPage";
import { receivingRoutes } from "../features/receiving/receivingConstants";
import { InventoryAdjustmentPage } from "../features/inventory/pages/InventoryAdjustmentPage";
import { InventoryAdjustmentDetailPage } from "../features/inventory/pages/InventoryAdjustmentDetailPage";
import { InventoryAdjustmentListPage } from "../features/inventory/pages/InventoryAdjustmentListPage";
import { InventoryMovementHistoryPage } from "../features/inventory/pages/InventoryMovementHistoryPage";
import { InventoryOverviewPage } from "../features/inventory/pages/InventoryOverviewPage";
import { CycleCountPage } from "../features/inventory/pages/CycleCountPage";
import { CycleCountListPage } from "../features/inventory/pages/CycleCountListPage";
import { CycleCountDetailPage } from "../features/inventory/pages/CycleCountDetailPage";
import { InventoryTransferPage } from "../features/inventory/pages/InventoryTransferPage";
import { InventoryTransferListPage } from "../features/inventory/pages/InventoryTransferListPage";
import { InventoryTransferDetailPage } from "../features/inventory/pages/InventoryTransferDetailPage";
import { inventoryRoutes } from "../features/inventory/inventoryConstants";
import { LoginPage } from "../features/auth/pages/LoginPage";
import { AuthProvider } from "../features/auth/AuthContext";
import { ProtectedRoute } from "../features/auth/components/ProtectedRoute";
import { AccessDeniedPage } from "../features/auth/pages/AccessDeniedPage";
import { ProductCreatePage } from "../features/products/pages/ProductCreatePage";
import { ProductDetailPage } from "../features/products/pages/ProductDetailPage";
import { ProductEditPage } from "../features/products/pages/ProductEditPage";
import { ProductListPage } from "../features/products/pages/ProductListPage";
import { ProductCategoryListPage } from "../features/products/pages/ProductCategoryListPage";
import { ProductCategoryFormPage } from "../features/products/pages/ProductCategoryFormPage";
import { CurrencyListPage } from "../features/purchasing/pages/CurrencyListPage";
import { CurrencyFormPage } from "../features/purchasing/pages/CurrencyFormPage";
import { productRoutes } from "../features/products/productConstants";
import { toAppLanguage } from "../shared/i18n/constants";
import "../shared/i18n/i18n";
import { ApplicationLayout } from "../layouts/ApplicationLayout";
import { ApiFeedbackProvider } from "../shared/feedback/ApiFeedbackProvider";
import { HomePage } from "../pages/home/HomePage";
import { AppProviders } from "./AppProviders";
import { applicationTheme } from "./theme";

export function App() {
  const { i18n } = useTranslation();
  const antdLocale =
    toAppLanguage(i18n.resolvedLanguage) === "fr" ? frFr : enUs;

  return (
    <ConfigProvider locale={antdLocale} theme={applicationTheme}>
      <ApiFeedbackProvider>
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
                      path={productRoutes.categoriesPattern}
                      element={<ProductCategoryListPage />}
                    />
                    <Route
                      path={productRoutes.categoryCreatePattern}
                      element={<ProductCategoryFormPage editing={false} />}
                    />
                    <Route
                      path={productRoutes.categoryEditPattern}
                      element={<ProductCategoryFormPage editing />}
                    />
                    <Route
                      path={warehouseRoutes.listPattern}
                      element={<WarehouseListPage />}
                    />
                    <Route
                      path={inventoryRoutes.rootPattern}
                      element={
                        <Navigate replace to={inventoryRoutes.overview} />
                      }
                    />
                    <Route
                      path={inventoryRoutes.overviewPattern}
                      element={<InventoryOverviewPage />}
                    />
                    <Route
                      path={inventoryRoutes.movementHistoryPattern}
                      element={<InventoryMovementHistoryPage />}
                    />
                    <Route
                      path={inventoryRoutes.adjustmentsPattern}
                      element={<InventoryAdjustmentListPage />}
                    />
                    <Route
                      path={inventoryRoutes.adjustmentCreatePattern}
                      element={<InventoryAdjustmentPage />}
                    />
                    <Route
                      path={inventoryRoutes.adjustmentDetailPattern}
                      element={<InventoryAdjustmentDetailPage />}
                    />
                    <Route
                      path={inventoryRoutes.cycleCountsPattern}
                      element={<CycleCountListPage />}
                    />
                    <Route
                      path={inventoryRoutes.cycleCountCreatePattern}
                      element={<CycleCountPage />}
                    />
                    <Route
                      path={inventoryRoutes.cycleCountDetailPattern}
                      element={<CycleCountDetailPage />}
                    />
                    <Route
                      path={inventoryRoutes.transfersPattern}
                      element={<InventoryTransferListPage />}
                    />
                    <Route
                      path={inventoryRoutes.transferCreatePattern}
                      element={<InventoryTransferPage />}
                    />
                    <Route
                      path={inventoryRoutes.transferDetailPattern}
                      element={<InventoryTransferDetailPage />}
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
                      path={supplierRoutes.listPattern}
                      element={<SupplierListPage />}
                    />
                    <Route
                      path={supplierRoutes.create}
                      element={<SupplierCreatePage />}
                    />
                    <Route
                      path={supplierRoutes.detailPattern}
                      element={<SupplierDetailPage />}
                    />
                    <Route
                      path={supplierRoutes.editPattern}
                      element={<SupplierEditPage />}
                    />
                    <Route
                      path={purchasingRoutes.cataloguePattern}
                      element={<SupplierCatalogueListPage />}
                    />
                    <Route
                      path={purchasingRoutes.currenciesPattern}
                      element={<CurrencyListPage />}
                    />
                    <Route
                      path={purchasingRoutes.currencyCreatePattern}
                      element={<CurrencyFormPage editing={false} />}
                    />
                    <Route
                      path={purchasingRoutes.currencyEditPattern}
                      element={<CurrencyFormPage editing />}
                    />
                    <Route
                      path={purchasingRoutes.catalogueCreatePattern}
                      element={<SupplierCatalogueCreatePage />}
                    />
                    <Route
                      path={purchasingRoutes.catalogueEditPattern}
                      element={<SupplierCatalogueEditPage />}
                    />
                    <Route
                      path={purchasingRoutes.ordersPattern}
                      element={<PurchaseOrderListPage />}
                    />
                    <Route
                      path={purchasingRoutes.orderCreate}
                      element={<PurchaseOrderCreatePage />}
                    />
                    <Route
                      path={purchasingRoutes.orderDetailPattern}
                      element={<PurchaseOrderDetailPage />}
                    />
                    <Route
                      path={purchasingRoutes.orderEditPattern}
                      element={<PurchaseOrderEditPage />}
                    />
                    <Route
                      path={receivingRoutes.listPattern}
                      element={<GoodsReceiptListPage />}
                    />
                    <Route
                      path={receivingRoutes.createPattern}
                      element={<GoodsReceiptCreatePage />}
                    />
                    <Route
                      path={receivingRoutes.detailPattern}
                      element={<GoodsReceiptDetailPage />}
                    />
                    <Route
                      element={<ProtectedRoute roles={[administratorRole]} />}
                    >
                      <Route
                        path={administrationRoutes.usersPattern}
                        element={<UsersPage />}
                      />
                      <Route
                        path={administrationRoutes.userCreatePattern}
                        element={<UserFormPage editing={false} />}
                      />
                      <Route
                        path={administrationRoutes.userEditPattern}
                        element={<UserFormPage editing />}
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
      </ApiFeedbackProvider>
    </ConfigProvider>
  );
}
