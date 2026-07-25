import { Card, Typography } from "antd";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import type { ProductFormValues } from "../api/productTypes";
import { useCreateProduct } from "../api/useProducts";
import { ProductForm } from "../components/ProductForm";
import { productRoutes } from "../productConstants";

export function ProductCreatePage() {
  const navigate = useNavigate();
  const createProduct = useCreateProduct();
  const { t } = useTranslation();

  async function handleSubmit(values: ProductFormValues) {
    const product = await createProduct.mutateAsync(values);
    navigate(productRoutes.detail(product.id));
  }

  return (
    <section>
      <Typography.Title level={2}>{t("products.createTitle")}</Typography.Title>
      <Card>
        <ProductForm
          cancelLabel={t("products.cancel")}
          isSubmitting={createProduct.isPending}
          onCancel={() => navigate(productRoutes.list)}
          onSubmit={handleSubmit}
          submitLabel={t("products.create")}
        />
      </Card>
    </section>
  );
}
