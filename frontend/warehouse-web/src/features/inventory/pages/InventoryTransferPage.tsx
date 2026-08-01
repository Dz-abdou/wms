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
import { ReloadableQuantityField } from "../components/ReloadableQuantityField";
import { getTransferCandidate } from "../api/inventoryApi";
import { useCreateTransfer } from "../api/useInventory";
import type {
  InventoryTransferInput,
  InventoryTransferLineInput,
} from "../api/inventoryTypes";
import {
  fractionalBaseUnitCodes,
  inventoryPageSize,
  inventoryRoutes,
} from "../inventoryConstants";

type TransferFormLine = InventoryTransferLineInput & {
  baseUnitOfMeasure?: string;
};
type TransferFormInput = Omit<InventoryTransferInput, "lines"> & {
  lines: TransferFormLine[];
};
type TransferRow = object;

export function InventoryTransferPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [form] = Form.useForm<TransferFormInput>();
  const products = useProducts({ page: 1, pageSize: inventoryPageSize });
  const warehouses = useWarehouses({ page: 1, pageSize: inventoryPageSize });
  const feedback = useApiFeedback();
  const createTransfer = useCreateTransfer();
  const { goBack, returnTo } = useReturnDestination(inventoryRoutes.transfers);
  const sourceWarehouseId = Form.useWatch("sourceWarehouseId", form);
  const destinationWarehouseId = Form.useWatch("destinationWarehouseId", form);
  const lines: TransferFormLine[] = Form.useWatch("lines", form) ?? [];
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

  async function loadAvailability(rowIndex: number, productId: string) {
    if (!sourceWarehouseId) return;

    try {
      const candidate = await getTransferCandidate(
        sourceWarehouseId,
        productId,
      );
      form.setFields([
        {
          name: ["lines", rowIndex, "sourceQuantityInBase"],
          value: candidate.availableQuantityInBase,
          errors: [],
        },
        {
          name: ["lines", rowIndex, "baseUnitOfMeasure"],
          value: candidate.baseUnitOfMeasure,
          errors: [],
        },
        {
          name: ["lines", rowIndex, "sourceBalanceVersion"],
          value: candidate.sourceBalanceVersion,
          errors: [],
        },
        {
          name: ["lines", rowIndex, "unitOfMeasure"],
          value: candidate.baseUnitOfMeasure,
          errors: [],
        },
      ]);
    } catch (error) {
      form.setFields([
        {
          name: ["lines", rowIndex, "productId"],
          errors: [
            getErrorMessage(t, error, "inventory.errors.loadTransferCandidate"),
          ],
        },
      ]);
    }
  }

  const lineColumns = (
    remove: (fieldName: number) => void,
  ): ColumnsType<TransferRow & EditableFormListTableRow> => [
    {
      title: t("inventory.form.product"),
      key: "product",
      width: 280,
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "productId"]}
          rules={[
            { required: true, message: t("inventory.form.productRequired") },
          ]}
          style={{ marginBottom: 0 }}
        >
          <Select
            aria-label={t("inventory.form.product")}
            disabled={!sourceWarehouseId || !destinationWarehouseId}
            onChange={(productId) =>
              void loadAvailability(row.fieldName, productId)
            }
            options={productOptions}
            optionFilterProp="label"
            showSearch
          />
        </Form.Item>
      ),
    },
    {
      title: t("inventory.transfers.availableAtSource"),
      key: "available",
      width: 190,
      render: (_, row) => {
        const line = lines[row.fieldName];
        return (
          <>
            <Form.Item hidden name={[row.fieldName, "baseUnitOfMeasure"]}>
              <Input />
            </Form.Item>
            <Form.Item hidden name={[row.fieldName, "sourceBalanceVersion"]}>
              <Input />
            </Form.Item>
            <Form.Item
              name={[row.fieldName, "sourceQuantityInBase"]}
              style={{ marginBottom: 0 }}
            >
              <ReloadableQuantityField
                disabled={!line?.productId}
                label={t("inventory.transfers.availableAtSource")}
                onReload={() => {
                  if (line?.productId) {
                    void loadAvailability(row.fieldName, line.productId);
                  }
                }}
                reloadLabel={t("inventory.transfers.reloadLine")}
                unitOfMeasure={line?.baseUnitOfMeasure}
              />
            </Form.Item>
          </>
        );
      },
    },
    {
      title: t("inventory.form.unitOfMeasure"),
      key: "unit",
      width: 150,
      render: (_, row) => {
        const product = products.data?.items.find(
          (item) => item.id === lines[row.fieldName]?.productId,
        );
        return (
          <Form.Item
            name={[row.fieldName, "unitOfMeasure"]}
            rules={[
              {
                required: true,
                message: t("inventory.form.unitOfMeasureRequired"),
              },
            ]}
            style={{ marginBottom: 0 }}
          >
            <Select
              aria-label={t("inventory.form.unitOfMeasure")}
              disabled={!product}
              options={unitOptions(product).map((unit) => ({
                value: unit,
                label: unit,
              }))}
            />
          </Form.Item>
        );
      },
    },
    {
      title: t("inventory.form.quantity"),
      key: "quantity",
      width: 170,
      render: (_, row) => (
        <Form.Item
          name={[row.fieldName, "quantity"]}
          rules={[
            { required: true, message: t("inventory.form.quantityRequired") },
          ]}
          style={{ marginBottom: 0 }}
        >
          <InputNumber
            aria-label={t("inventory.form.quantity")}
            min={0.001}
            precision={3}
          />
        </Form.Item>
      ),
    },
    {
      title: t("inventory.table.actions"),
      key: "actions",
      fixed: "right",
      width: 120,
      render: (_, row) => (
        <Button danger onClick={() => remove(row.fieldName)} type="text">
          {t("inventory.removeLine")}
        </Button>
      ),
    },
  ];

  async function submit(input: TransferFormInput) {
    const transferInput: InventoryTransferInput = {
      ...input,
      lines: input.lines.map(
        ({
          productId,
          quantity,
          unitOfMeasure,
          sourceQuantityInBase,
          sourceBalanceVersion,
        }) => ({
          productId,
          quantity,
          unitOfMeasure,
          sourceQuantityInBase,
          sourceBalanceVersion,
        }),
      ),
    };
    try {
      const result = await createTransfer.mutateAsync(transferInput);
      navigate(inventoryRoutes.transferDetail(result.id));
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed")) {
        feedback.notifyError(error, "inventory.errors.createTransfer");
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
      backLabel={t("inventory.transfers.title")}
      backTo={returnTo}
      subtitle={t("inventory.transfers.createSubtitle")}
      title={t("inventory.transfers.createTitle")}
    >
      <Card>
        <Form
          form={form}
          initialValues={{ lines: [] }}
          layout="vertical"
          onFinish={submit}
          onValuesChange={(changedValues) => {
            if (
              "sourceWarehouseId" in changedValues ||
              "destinationWarehouseId" in changedValues
            ) {
              form.setFieldValue("lines", []);
            }
          }}
        >
          <Form.Item
            label={t("inventory.transfers.sourceWarehouse")}
            name="sourceWarehouseId"
            rules={[
              {
                required: true,
                message: t("inventory.transfers.sourceRequired"),
              },
            ]}
          >
            <Select
              allowClear
              aria-label={t("inventory.transfers.sourceWarehouse")}
              options={warehouseOptions?.filter(
                (warehouse) => warehouse.value !== destinationWarehouseId,
              )}
              optionFilterProp="label"
              showSearch
            />
          </Form.Item>
          <Form.Item
            label={t("inventory.transfers.destinationWarehouse")}
            name="destinationWarehouseId"
            rules={[
              {
                required: true,
                message: t("inventory.transfers.destinationRequired"),
              },
            ]}
          >
            <Select
              allowClear
              aria-label={t("inventory.transfers.destinationWarehouse")}
              options={warehouseOptions?.filter(
                (warehouse) => warehouse.value !== sourceWarehouseId,
              )}
              optionFilterProp="label"
              showSearch
            />
          </Form.Item>
          <Form.Item label={t("inventory.form.reference")} name="reference">
            <Input maxLength={100} />
          </Form.Item>
          <Form.Item label={t("inventory.form.note")} name="note">
            <Input.TextArea maxLength={1000} />
          </Form.Item>
          <EditableFormListTable<TransferRow>
            addDisabled={!sourceWarehouseId || !destinationWarehouseId}
            addLabel={t("inventory.addLine")}
            columns={lineColumns}
            createRow={() => ({})}
            name="lines"
            scroll={{ x: 1000 }}
          />
          <FormPageActions
            cancelLabel={t("inventory.cancel")}
            isSubmitting={createTransfer.isPending}
            onCancel={goBack}
            submitLabel={t("inventory.transfers.post")}
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
  ].filter(
    (unit, index, units) =>
      units.indexOf(unit) === index &&
      (fractionalBaseUnitCodes.has(unit) || unit.length > 0),
  );
}
