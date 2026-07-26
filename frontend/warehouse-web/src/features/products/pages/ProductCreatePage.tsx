import { Card } from "antd";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import type { ProductFormValues } from "../api/productTypes";
import { useCreateProduct } from "../api/useProducts";
import { ProductForm } from "../components/ProductForm";
import { productRoutes } from "../productConstants";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";

export function ProductCreatePage() {
  const navigate = useNavigate();
  const createProduct = useCreateProduct();
  const { t } = useTranslation();
  const { goBack, returnTo } = useReturnDestination(productRoutes.list);

  async function handleSubmit(values: ProductFormValues) {
    const product = await createProduct.mutateAsync(values);
    navigate(productRoutes.detail(product.id));
  }

  return (
    <FormPageLayout
      backLabel={t("products.title")}
      backTo={returnTo}
      title={t("products.createTitle")}
    >
      <Card>
        <ProductForm
          cancelLabel={t("products.cancel")}
          isSubmitting={createProduct.isPending}
          onCancel={goBack}
          onSubmit={handleSubmit}
          submitLabel={t("products.create")}
        />
      </Card>
    </FormPageLayout>
  );
}
