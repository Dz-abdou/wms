import { Alert, Card, Spin } from "antd";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import type { ProductFormValues } from "../api/productTypes";
import { useProduct, useUpdateProduct } from "../api/useProducts";
import { ProductForm } from "../components/ProductForm";
import { productRoutes } from "../productConstants";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";

export function ProductEditPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const productQuery = useProduct(id);
  const updateProduct = useUpdateProduct(id ?? "");
  const { goBack, returnTo } = useReturnDestination(productRoutes.detail(id ?? ""));

  if (productQuery.isLoading) {
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("products.loadingOne")}
      />
    );
  }

  if (productQuery.error || !productQuery.data || !id) {
    return (
      <Alert
        message={getErrorMessage(t, productQuery.error, "products.errors.load")}
        showIcon
        type="error"
      />
    );
  }

  const productId = id;

  async function handleSubmit(values: ProductFormValues) {
    await updateProduct.mutateAsync(values);
    navigate(productRoutes.detail(productId));
  }

  return (
    <FormPageLayout
      backLabel={productQuery.data.name}
      backTo={returnTo}
      title={t("products.editTitle")}
    >
      <Card>
        <ProductForm
          cancelLabel={t("products.cancel")}
          initialValues={{
            sku: productQuery.data.sku,
            name: productQuery.data.name,
            description: productQuery.data.description ?? undefined,
            baseUnitOfMeasure: productQuery.data.baseUnitOfMeasure,
            categoryId: productQuery.data.categoryId ?? undefined,
            unitConversions: productQuery.data.unitConversions,
            measurements: productQuery.data.measurements ?? undefined,
          }}
          isSubmitting={updateProduct.isPending}
          onCancel={goBack}
          onSubmit={handleSubmit}
          submitLabel={t("products.save")}
        />
      </Card>
    </FormPageLayout>
  );
}
