import { Button, Layout, Menu, Typography } from "antd";
import { Link, Outlet, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
  administrationRoutes,
  administratorRole,
} from "../features/administration/administrationConstants";
import { warehouseRoutes } from "../features/warehouses/warehouseConstants";
import { productRoutes } from "../features/products/productConstants";
import { inventoryRoutes } from "../features/inventory/inventoryConstants";
import { supplierRoutes } from "../features/suppliers/supplierConstants";
import { customerRoutes } from "../features/customers/customerConstants";
import { purchasingRoutes } from "../features/purchasing/purchasingConstants";
import { receivingRoutes } from "../features/receiving/receivingConstants";
import { LanguageSelector } from "../shared/components/LanguageSelector";
import { useAuth } from "../features/auth/AuthContext";

const { Content, Header } = Layout;

export function ApplicationLayout() {
  const { session, signOut } = useAuth();
  const location = useLocation();
  const { t } = useTranslation();
  const selectedKey = location.pathname.startsWith(productRoutes.list)
    ? "products"
    : location.pathname.startsWith(productRoutes.categories)
      ? "categories"
      : location.pathname.startsWith(warehouseRoutes.list)
        ? "warehouses"
        : location.pathname.startsWith(supplierRoutes.list)
          ? "suppliers"
          : location.pathname.startsWith(customerRoutes.list)
            ? "customers"
            : location.pathname.startsWith(purchasingRoutes.catalogue)
              ? "catalogue"
              : location.pathname.startsWith(purchasingRoutes.currencies)
                ? "currencies"
                : location.pathname.startsWith(purchasingRoutes.orders)
                  ? "purchase-orders"
                  : location.pathname.startsWith(receivingRoutes.list)
                    ? "goods-receipts"
                    : location.pathname.startsWith(inventoryRoutes.overview) ||
                        location.pathname === inventoryRoutes.root
                      ? "inventory-overview"
                      : location.pathname.startsWith(
                            inventoryRoutes.adjustments,
                          )
                        ? "inventory-adjustments"
                        : location.pathname.startsWith(
                              inventoryRoutes.transfers,
                            )
                          ? "inventory-transfers"
                          : location.pathname.startsWith(
                                inventoryRoutes.cycleCounts,
                              )
                            ? "inventory-cycle-counts"
                            : location.pathname.startsWith(
                                  inventoryRoutes.movementHistory,
                                ) ||
                                location.pathname.startsWith(
                                  inventoryRoutes.root,
                                )
                              ? "inventory-movements"
                              : location.pathname.startsWith(
                                    administrationRoutes.users,
                                  )
                                ? "users"
                                : location.pathname.startsWith(
                                      administrationRoutes.roles,
                                    )
                                  ? "roles"
                                  : "home";
  const isAdministrator = session?.roles.includes(administratorRole);

  return (
    <Layout className="application-layout">
      <Header className="application-header">
        <Link className="application-title" to="/">
          <Typography.Title level={3}>{t("app.brand")}</Typography.Title>
        </Link>
        <Menu
          className="application-menu"
          items={[
            { key: "home", label: <Link to="/">{t("navigation.home")}</Link> },
            {
              key: "master-data",
              label: t("navigation.masterData"),
              children: [
                {
                  key: "products",
                  label: (
                    <Link to={productRoutes.list}>
                      {t("navigation.products")}
                    </Link>
                  ),
                },
                {
                  key: "categories",
                  label: (
                    <Link to={productRoutes.categories}>
                      {t("navigation.categories")}
                    </Link>
                  ),
                },
                {
                  key: "warehouses",
                  label: (
                    <Link to={warehouseRoutes.list}>
                      {t("navigation.warehouses")}
                    </Link>
                  ),
                },
                {
                  key: "suppliers",
                  label: (
                    <Link to={supplierRoutes.list}>
                      {t("navigation.suppliers")}
                    </Link>
                  ),
                },
                {
                  key: "customers",
                  label: (
                    <Link to={customerRoutes.list}>
                      {t("navigation.customers")}
                    </Link>
                  ),
                },
                {
                  key: "currencies",
                  label: (
                    <Link to={purchasingRoutes.currencies}>
                      {t("navigation.currencies")}
                    </Link>
                  ),
                },
              ],
            },
            {
              key: "inbound",
              label: t("navigation.inbound"),
              children: [
                {
                  key: "catalogue",
                  label: (
                    <Link to={purchasingRoutes.catalogue}>
                      {t("navigation.supplierCatalogue")}
                    </Link>
                  ),
                },
                {
                  key: "purchase-orders",
                  label: (
                    <Link to={purchasingRoutes.orders}>
                      {t("navigation.purchaseOrders")}
                    </Link>
                  ),
                },
                {
                  key: "goods-receipts",
                  label: (
                    <Link to={receivingRoutes.list}>
                      {t("navigation.goodsReceipts")}
                    </Link>
                  ),
                },
              ],
            },
            {
              key: "inventory",
              label: t("navigation.inventory"),
              children: [
                {
                  key: "inventory-overview",
                  label: (
                    <Link to={inventoryRoutes.overview}>
                      {t("navigation.inventoryOverview")}
                    </Link>
                  ),
                },
                {
                  key: "inventory-movements",
                  label: (
                    <Link to={inventoryRoutes.movementHistory}>
                      {t("navigation.movementHistory")}
                    </Link>
                  ),
                },
                {
                  key: "inventory-adjustments",
                  label: (
                    <Link to={inventoryRoutes.adjustments}>
                      {t("navigation.adjustments")}
                    </Link>
                  ),
                },
                {
                  key: "inventory-cycle-counts",
                  label: (
                    <Link to={inventoryRoutes.cycleCounts}>
                      {t("navigation.cycleCounts")}
                    </Link>
                  ),
                },
                {
                  key: "inventory-transfers",
                  label: (
                    <Link to={inventoryRoutes.transfers}>
                      {t("navigation.transfers")}
                    </Link>
                  ),
                },
              ],
            },
            ...(isAdministrator
              ? [
                  {
                    key: "administration",
                    label: t("navigation.administration"),
                    children: [
                      {
                        key: "users",
                        label: (
                          <Link to={administrationRoutes.users}>
                            {t("navigation.users")}
                          </Link>
                        ),
                      },
                      {
                        key: "roles",
                        label: (
                          <Link to={administrationRoutes.roles}>
                            {t("navigation.roles")}
                          </Link>
                        ),
                      },
                    ],
                  },
                ]
              : []),
          ]}
          mode="horizontal"
          selectedKeys={[selectedKey]}
          theme="dark"
        />
        <Button onClick={() => void signOut()}>{t("auth.logout")}</Button>
        <LanguageSelector />
      </Header>
      <Content className="application-content">
        <Outlet />
      </Content>
    </Layout>
  );
}
