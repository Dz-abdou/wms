import {
  Alert,
  Button,
  Card,
  Descriptions,
  Form,
  Input,
  InputNumber,
  Spin,
  Table,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { useEffect, useMemo } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { FormPageActions } from "../../../shared/components/FormPageActions";
import { FormPageLayout } from "../../../shared/components/PageLayouts";
import {
  hasProblemCode,
  getErrorMessage,
} from "../../../shared/errors/problemDetails";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import { useReturnDestination } from "../../../shared/navigation/returnNavigation";
import {
  useCreateGoodsReceipt,
  useGoodsReceiptCandidate,
} from "../api/useReceiving";
import type {
  GoodsReceiptCandidateLine,
  GoodsReceiptInput,
} from "../api/receivingTypes";
import { receivingRoutes, receivingValidation } from "../receivingConstants";

type ReceiptFormValues = Omit<
  GoodsReceiptInput,
  "purchaseOrderId" | "purchaseOrderVersion"
>;

export function GoodsReceiptCreatePage() {
  const { purchaseOrderId } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const feedback = useApiFeedback();
  const [form] = Form.useForm<ReceiptFormValues>();
  const candidateQuery = useGoodsReceiptCandidate(purchaseOrderId);
  const createReceipt = useCreateGoodsReceipt();
  const { goBack, returnTo } = useReturnDestination(receivingRoutes.list);
  const candidate = candidateQuery.data;
  const lines = Form.useWatch("lines", form);

  useEffect(() => {
    if (!candidate) return;
    const previousLines = new Map<string, number>(
      ((form.getFieldValue("lines") ?? []) as ReceiptFormValues["lines"]).map(
        (line) => [line.purchaseOrderLineId, line.acceptedQuantity] as const,
      ),
    );
    form.setFieldsValue({
      receivedAtUtc: form.getFieldValue("receivedAtUtc") ?? toLocalDateTime(),
      lines: candidate.lines.map((line) => ({
        purchaseOrderLineId: line.purchaseOrderLineId,
        acceptedQuantity:
          previousLines.get(line.purchaseOrderLineId) ??
          line.outstandingQuantity,
      })),
    });
  }, [candidate, form]);

  const columns = useMemo<ColumnsType<GoodsReceiptCandidateLine>>(
    () => [
      {
        title: t("receiving.lineNumber"),
        dataIndex: "lineNumber",
        key: "lineNumber",
        width: 80,
      },
      {
        title: t("receiving.product"),
        key: "product",
        width: 270,
        render: (_, line) => `${line.productSku} — ${line.productName}`,
      },
      {
        title: t("receiving.unit"),
        dataIndex: "unitOfMeasure",
        key: "unitOfMeasure",
        width: 110,
      },
      {
        title: t("receiving.orderedQuantity"),
        key: "orderedQuantity",
        width: 130,
        render: (_, line) => `${line.orderedQuantity} ${line.unitOfMeasure}`,
      },
      {
        title: t("receiving.previouslyReceived"),
        key: "receivedQuantity",
        width: 150,
        render: (_, line) => `${line.receivedQuantity} ${line.unitOfMeasure}`,
      },
      {
        title: t("receiving.outstandingQuantity"),
        key: "outstandingQuantity",
        width: 140,
        render: (_, line) =>
          `${line.outstandingQuantity} ${line.unitOfMeasure}`,
      },
      {
        title: t("receiving.acceptedQuantity"),
        key: "acceptedQuantity",
        width: 180,
        render: (_, line, index) => (
          <>
            <Form.Item hidden name={["lines", index, "purchaseOrderLineId"]}>
              <Input />
            </Form.Item>
            <Form.Item
              name={["lines", index, "acceptedQuantity"]}
              rules={[
                {
                  required: true,
                  message: t("receiving.acceptedQuantityRequired"),
                },
                {
                  validator: (_, value: number | null | undefined) =>
                    typeof value === "number" &&
                    value > 0 &&
                    value <= line.outstandingQuantity
                      ? Promise.resolve()
                      : Promise.reject(
                          new Error(
                            t("receiving.acceptedQuantityMaximum", {
                              maximum: line.outstandingQuantity,
                            }),
                          ),
                        ),
                },
              ]}
              style={{ marginBottom: 0 }}
            >
              <InputNumber
                aria-label={t("receiving.acceptedQuantity")}
                min={0}
                precision={receivingValidation.acceptedQuantityPrecision}
              />
            </Form.Item>
          </>
        ),
      },
      {
        title: t("receiving.baseQuantity"),
        key: "baseQuantity",
        width: 140,
        render: (_, line, index) => {
          const quantity = lines?.[index]?.acceptedQuantity ?? 0;
          return quantity * line.outstandingQuantity === 0
            ? 0
            : quantity * line.conversionFactorToBaseUnit;
        },
      },
    ],
    [lines, t],
  );

  async function submit(values: ReceiptFormValues) {
    if (!candidate || !purchaseOrderId) return;
    try {
      const receipt = await createReceipt.mutateAsync({
        ...values,
        purchaseOrderId,
        purchaseOrderVersion: candidate.version,
        receivedAtUtc: new Date(values.receivedAtUtc).toISOString(),
      });
      navigate(receivingRoutes.detail(receipt.id));
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed")) {
        feedback.notifyError(error, "receiving.errors.create");
      }
    }
  }

  if (candidateQuery.isLoading) {
    return (
      <Spin
        className="page-spinner"
        size="large"
        tip={t("receiving.loadingCandidate")}
      />
    );
  }
  if (!candidate || candidateQuery.error || !purchaseOrderId) {
    return (
      <Alert
        message={getErrorMessage(
          t,
          candidateQuery.error,
          "receiving.errors.loadCandidate",
        )}
        showIcon
        type="error"
      />
    );
  }

  return (
    <FormPageLayout
      backLabel={t("receiving.title")}
      backTo={returnTo}
      subtitle={t("receiving.createSubtitle")}
      title={t("receiving.createTitle")}
    >
      {hasProblemCode(
        createReceipt.error,
        "goods_receipt.purchase_order_concurrency_conflict",
      ) ? (
        <Alert
          action={
            <Button onClick={() => void candidateQuery.refetch()}>
              {t("receiving.refresh")}
            </Button>
          }
          className="page-alert"
          message={t("receiving.concurrencyHint")}
          showIcon
          type="warning"
        />
      ) : null}
      <Card>
        <Descriptions bordered column={1}>
          <Descriptions.Item label={t("receiving.purchaseOrder")}>
            {candidate.purchaseOrderNumber}
          </Descriptions.Item>
          <Descriptions.Item label={t("receiving.warehouse")}>
            {candidate.warehouseCode} — {candidate.warehouseName}
          </Descriptions.Item>
          <Descriptions.Item label={t("receiving.currency")}>
            {candidate.currencyCode ?? "—"}
          </Descriptions.Item>
        </Descriptions>
        <Form form={form} layout="vertical" onFinish={submit}>
          <div className="form-grid">
            <Form.Item
              label={t("receiving.receivedAt")}
              name="receivedAtUtc"
              rules={[
                { required: true, message: t("receiving.receivedAtRequired") },
              ]}
            >
              <Input type="datetime-local" />
            </Form.Item>
            <Form.Item
              label={t("receiving.supplierDeliveryNote")}
              name="supplierDeliveryNote"
            >
              <Input
                maxLength={receivingValidation.maxSupplierDeliveryNoteLength}
              />
            </Form.Item>
          </div>
          <Form.Item label={t("receiving.notes")} name="notes">
            <Input.TextArea maxLength={receivingValidation.maxNotesLength} />
          </Form.Item>
          <Table
            columns={columns}
            dataSource={candidate.lines}
            pagination={false}
            rowKey="purchaseOrderLineId"
            scroll={{ x: 1300 }}
          />
          <FormPageActions
            cancelLabel={t("receiving.cancel")}
            isSubmitting={createReceipt.isPending}
            onCancel={goBack}
            submitLabel={t("receiving.post")}
          />
        </Form>
      </Card>
    </FormPageLayout>
  );
}

function toLocalDateTime() {
  const now = new Date();
  const offset = now.getTimezoneOffset() * 60_000;
  return new Date(now.getTime() - offset).toISOString().slice(0, 16);
}
