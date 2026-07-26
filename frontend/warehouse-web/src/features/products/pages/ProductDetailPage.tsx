import {
  Alert,
  Button,
  Descriptions,
  Popconfirm,
  Spin,
} from "antd";
import { useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { formatDateTime } from "../../../shared/formatting/dateTime";
import { toAppLanguage } from "../../../shared/i18n/constants";
import { useProduct, useSetProductStatus } from "../api/useProducts";
import { ProductStatusTag } from "../components/ProductStatusTag";
import { productRoutes } from "../productConstants";
import {
  DetailPageLayout,
  RouteActionButton,
} from "../../../shared/components/PageLayouts";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";

export function ProductDetailPage() {
  const { id } = useParams();
  const { i18n, t } = useTranslation();
  const feedback = useApiFeedback();
  const productQuery = useProduct(id);
  const setStatus = useSetProductStatus(id ?? "");
  const { returnTo } = useReturnDestination(productRoutes.list);

  if (productQuery.isLoading)
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("products.loadingOne")}
      />
    );
  if (productQuery.error || !productQuery.data || !id)
    return (
      <Alert
        message={getErrorMessage(t, productQuery.error, "products.errors.load")}
        showIcon
        type="error"
      />
    );

  const product = productQuery.data;
  const actionKey = product.isActive
    ? "products.deactivate"
    : "products.activate";
  const actionLabel = t(actionKey);
  const actionInSentence = actionLabel.toLocaleLowerCase(i18n.language);
  const language = toAppLanguage(i18n.resolvedLanguage);

  async function handleStatusChange() {
    try {
      await setStatus.mutateAsync(!product.isActive);
    } catch (error) {
      feedback.notifyError(error, "products.errors.status");
    }
  }

  return (
    <DetailPageLayout
      actions={
        <>
          <RouteActionButton to={productRoutes.edit(id)}>
            {t("products.edit")}
          </RouteActionButton>
          <Popconfirm
            cancelText={t("products.cancel")}
            description={t("products.confirmStatusDescription", {
              action: actionInSentence,
            })}
            okText={actionLabel}
            onConfirm={handleStatusChange}
            title={t("products.confirmStatusTitle", { action: actionLabel })}
          >
            <Button danger={product.isActive} loading={setStatus.isPending}>
              {actionLabel}
            </Button>
          </Popconfirm>
        </>
      }
      backLabel={t("products.title")}
      backTo={returnTo}
      title={product.name}
    >
      <Descriptions bordered column={1}>
        <Descriptions.Item label={t("products.table.sku")}>
          {product.sku}
        </Descriptions.Item>
        <Descriptions.Item label={t("products.table.name")}>
          {product.name}
        </Descriptions.Item>
        <Descriptions.Item label={t("products.table.description")}>
          {product.description ?? t("products.missingDescription")}
        </Descriptions.Item>
        <Descriptions.Item label={t("products.table.status")}>
          <ProductStatusTag isActive={product.isActive} />
        </Descriptions.Item>
        <Descriptions.Item label={t("products.table.created")}>
          {formatDateTime(product.createdAtUtc, language)}
        </Descriptions.Item>
        <Descriptions.Item label={t("products.table.updated")}>
          {formatDateTime(product.updatedAtUtc, language)}
        </Descriptions.Item>
      </Descriptions>
    </DetailPageLayout>
  );
}
