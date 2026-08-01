import {
  Alert,
  Button,
  Card,
  Form,
  Input,
  InputNumber,
  Select,
  Spin,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import {
  EditableFormListTable,
  type EditableFormListTableRow,
} from "../../../shared/components/EditableFormListTable";
import { FormPageActions } from "../../../shared/components/FormPageActions";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import { useProducts } from "../../products/api/useProducts";
import type { Product } from "../../products/api/productTypes";
import { useWarehouses } from "../../warehouses/api/useWarehouses";
import { ReloadableQuantityField } from "../../../shared/components/ReloadableQuantityField";
import { getCycleCountCandidate } from "../api/inventoryApi";
import { useCreateCycleCount } from "../api/useInventory";
import type {
  CycleCountInput,
  CycleCountLineInput,
} from "../api/inventoryTypes";
import {
  fractionalBaseUnitCodes,
  inventoryPageSize,
  inventoryRoutes,
} from "../inventoryConstants";

type CycleCountRow = object;

export function CycleCountPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [form] = Form.useForm<CycleCountInput>();
  const products = useProducts({ page: 1, pageSize: inventoryPageSize });
  const warehouses = useWarehouses({ page: 1, pageSize: inventoryPageSize });
  const feedback = useApiFeedback();
  const createCycleCount = useCreateCycleCount();
  const { goBack, returnTo } = useReturnDestination(
    inventoryRoutes.cycleCounts,
  );
  const selectedWarehouseId = Form.useWatch("warehouseId", form);
  const lines: CycleCountLineInput[] = Form.useWatch("lines", form) ?? [];

  const productOptions = products.data?.items
    .filter((product) => product.isActive)
    .map((product) => ({
      value: product.id,
      label: `${product.sku} — ${product.name}`,
    }));
  const warehouseOptions = warehouses.data?.items
    .filter((warehouse) => warehouse.isActive)
    .map((warehouse) => ({
      value: warehouse.id,
      label: `${warehouse.code} — ${warehouse.name}`,
    }));

  async function loadCandidate(rowIndex: number, productId: string) {
    if (!selectedWarehouseId) return;

    try {
      const candidate = await getCycleCountCandidate(
        selectedWarehouseId,
        productId,
      );
      form.setFields([
        {
          name: ["lines", rowIndex, "systemQuantityInBase"],
          value: candidate.systemQuantityInBase,
          errors: [],
        },
        {
          name: ["lines", rowIndex, "systemBalanceVersion"],
          value: candidate.systemBalanceVersion,
          errors: [],
        },
        {
          name: ["lines", rowIndex, "countedUnitOfMeasure"],
          value: candidate.baseUnitOfMeasure,
          errors: [],
        },
        {
          name: ["lines", rowIndex, "countedQuantityInUnit"],
          value: undefined,
          errors: [],
        },
      ]);
    } catch (error) {
      form.setFields([
        {
          name: ["lines", rowIndex, "productId"],
          errors: [
            getErrorMessage(t, error, "inventory.errors.loadCountCandidate"),
          ],
        },
      ]);
    }
  }

  const lineColumns = (
    remove: (fieldName: number) => void,
  ): ColumnsType<CycleCountRow & EditableFormListTableRow> => [
    {
      title: t("inventory.table.product"),
      key: "product",
      width: 290,
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "productId"]}
          rules={[
            { required: true, message: t("inventory.form.productRequired") },
          ]}
          style={{ marginBottom: 0 }}
        >
          <Select
            aria-label={t("inventory.table.product")}
            disabled={!selectedWarehouseId}
            onChange={(productId) =>
              void loadCandidate(row.fieldName, productId)
            }
            options={productOptions}
            showSearch
            optionFilterProp="label"
          />
        </Form.Item>
      ),
    },
    {
      title: t("inventory.cycleCounts.systemQuantity"),
      key: "systemQuantity",
      width: 330,
      render: (_, row) => {
        const line = lines[row.fieldName];
        const product = products.data?.items.find(
          (item) => item.id === line?.productId,
        );
        return (
          <>
            <Form.Item
              name={[row.fieldName, "systemQuantityInBase"]}
              style={{ marginBottom: 0 }}
            >
              <ReloadableQuantityField
                disabled={!line?.productId}
                label={t("inventory.cycleCounts.systemQuantity")}
                onReload={() => {
                  if (line?.productId) {
                    void loadCandidate(row.fieldName, line.productId);
                  }
                }}
                reloadLabel={t("inventory.cycleCounts.reloadLine")}
                unitOfMeasure={product?.baseUnitOfMeasure}
              />
            </Form.Item>
            <Form.Item hidden name={[row.fieldName, "systemBalanceVersion"]}>
              <Input />
            </Form.Item>
          </>
        );
      },
    },
    {
      title: t("inventory.cycleCounts.unit"),
      key: "unit",
      width: 120,
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "countedUnitOfMeasure"]}
          rules={[
            {
              required: true,
              message: t("inventory.form.unitOfMeasureRequired"),
            },
          ]}
          style={{ marginBottom: 0 }}
        >
          <Select
            aria-label={t("inventory.cycleCounts.unit")}
            disabled={
              !products.data?.items.find(
                (item) => item.id === lines[row.fieldName]?.productId,
              )
            }
            options={unitOptions(
              products.data?.items.find(
                (item) => item.id === lines[row.fieldName]?.productId,
              ),
            ).map((unit) => ({ value: unit, label: unit }))}
          />
        </Form.Item>
      ),
    },
    {
      title: t("inventory.cycleCounts.countedQuantity"),
      key: "countedQuantity",
      width: 180,
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "countedQuantityInUnit"]}
          rules={[
            {
              required: true,
              message: t("inventory.cycleCounts.countedQuantityRequired"),
            },
            {
              type: "number",
              min: 0,
              message: t("inventory.cycleCounts.countedQuantityMinimum"),
            },
          ]}
          style={{ marginBottom: 0 }}
        >
          <InputNumber
            aria-label={t("inventory.cycleCounts.countedQuantity")}
            min={0}
            precision={3}
          />
        </Form.Item>
      ),
    },
    {
      title: t("inventory.table.actions"),
      key: "actions",
      render: (_, row) => (
        <Button danger onClick={() => remove(row.fieldName)} type="text">
          {t("inventory.removeLine")}
        </Button>
      ),
    },
  ];

  async function submit(input: CycleCountInput) {
    try {
      const result = await createCycleCount.mutateAsync(input);
      navigate(inventoryRoutes.cycleCountDetail(result.id));
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed")) {
        feedback.notifyError(error, "inventory.errors.createCycleCount");
      }
    }
  }

  if (products.isLoading || warehouses.isLoading) {
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("inventory.loadingSources")}
      />
    );
  }

  if (products.error || warehouses.error) {
    return (
      <Alert
        message={getErrorMessage(
          t,
          products.error ?? warehouses.error,
          "inventory.errors.loadSources",
        )}
        showIcon
        type="error"
      />
    );
  }

  return (
    <FormPageLayout
      backLabel={t("inventory.cycleCounts.title")}
      backTo={returnTo}
      subtitle={t("inventory.cycleCounts.createSubtitle")}
      title={t("inventory.cycleCounts.createTitle")}
    >
      <Card>
        <Form
          form={form}
          initialValues={{ lines: [] }}
          layout="vertical"
          onFinish={submit}
          onValuesChange={(changedValues) => {
            if ("warehouseId" in changedValues) form.setFieldValue("lines", []);
          }}
        >
          <Form.Item
            label={t("inventory.form.warehouse")}
            name="warehouseId"
            rules={[
              {
                required: true,
                message: t("inventory.form.warehouseRequired"),
              },
            ]}
          >
            <Select
              aria-label={t("inventory.form.warehouse")}
              disabled={lines.length > 0}
              options={warehouseOptions}
              showSearch
              optionFilterProp="label"
            />
          </Form.Item>
          <Form.Item label={t("inventory.form.reference")} name="reference">
            <Input maxLength={100} />
          </Form.Item>
          <Form.Item label={t("inventory.form.note")} name="note">
            <Input.TextArea maxLength={1000} />
          </Form.Item>
          <EditableFormListTable<CycleCountRow>
            addDisabled={!selectedWarehouseId}
            addLabel={t("inventory.addLine")}
            columns={lineColumns}
            createRow={() => ({})}
            name="lines"
            scroll={{ x: 1060 }}
          />
          <FormPageActions
            cancelLabel={t("inventory.cancel")}
            isSubmitting={createCycleCount.isPending}
            onCancel={goBack}
            submitLabel={t("inventory.cycleCounts.post")}
          />
        </Form>
      </Card>
    </FormPageLayout>
  );
}

function unitOptions(product: Product | undefined): string[] {
  if (!product) return [];

  return [
    product.baseUnitOfMeasure,
    ...product.unitConversions.map((conversion) => conversion.unitOfMeasure),
  ]
    .filter((unit, index, units) => units.indexOf(unit) === index)
    .filter((unit) => fractionalBaseUnitCodes.has(unit) || unit.length > 0);
}
