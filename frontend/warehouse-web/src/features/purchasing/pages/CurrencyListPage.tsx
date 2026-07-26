import {
  Alert,
  Button,
  Empty,
  Form,
  Input,
  InputNumber,
  Modal,
  Table,
  Tag,
} from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { applyServerFieldErrors } from "../../../shared/errors/serverFieldErrors";
import { useApiFeedback } from "../../../shared/feedback/ApiFeedbackProvider";
import {
  useCreateCurrency,
  useCurrencies,
  useSetCurrencyStatus,
  useSetDefaultCurrency,
  useUpdateCurrency,
} from "../api/usePurchasing";
import type { Currency, CurrencyInput } from "../api/purchasingTypes";
import { getErrorMessage } from "../../../shared/errors/problemDetails";
import { ListPageLayout } from "../../../shared/components/PageLayouts";
import { ModalFormActions } from "../../../shared/components/ModalFormActions";

export function CurrencyListPage() {
  const { t } = useTranslation();
  const feedback = useApiFeedback();
  const [editing, setEditing] = useState<Currency | undefined>();
  const currencies = useCurrencies();
  const create = useCreateCurrency();
  const update = useUpdateCurrency();
  const status = useSetCurrencyStatus();
  const setDefault = useSetDefaultCurrency();
  async function submit(values: CurrencyInput) {
    if (editing?.id)
      await update.mutateAsync({
        id: editing.id,
        input: {
          name: values.name,
          symbol: values.symbol,
          decimalPlaces: values.decimalPlaces,
        },
      });
    else await create.mutateAsync(values);
    setEditing(undefined);
  }
  async function run(action: () => Promise<unknown>) {
    try {
      await action();
    } catch (error) {
      feedback.notifyError(error, "masterData.currencies.errors.save");
    }
  }
  return (
    <ListPageLayout
      actions={<Button type="primary" onClick={() => setEditing({} as Currency)}>
          {t("masterData.new")}
        </Button>}
      subtitle={t("masterData.currencies.subtitle")}
      title={t("masterData.currencies.title")}
    >
      {currencies.error ? <Alert className="page-alert" message={getErrorMessage(t, currencies.error, "masterData.currencies.errors.save")} showIcon type="error" /> : null}
      {currencies.isLoading ? <Empty className="page-empty" description={t("masterData.currencies.loading")} /> : null}
      {!currencies.isLoading && currencies.data?.items.length === 0 ? <Empty className="page-empty" description={t("masterData.currencies.empty")} /> : null}
      {currencies.data && currencies.data.items.length > 0 ? <Table
        rowKey="id"
        loading={currencies.isLoading}
        dataSource={currencies.data.items}
        pagination={false}
        columns={[
          { title: t("masterData.code"), dataIndex: "code" },
          { title: t("masterData.name"), dataIndex: "name" },
          { title: t("masterData.symbol"), dataIndex: "symbol" },
          { title: t("masterData.decimals"), dataIndex: "decimalPlaces" },
          {
            title: t("masterData.status"),
            render: (_, currency) =>
              currency.isActive ? (
                <Tag color="green">{t("masterData.active")}</Tag>
              ) : (
                <Tag>{t("masterData.inactive")}</Tag>
              ),
          },
          {
            title: t("masterData.actions"),
            render: (_, currency) => (
              <>
                <Button type="link" onClick={() => setEditing(currency)}>
                  {t("masterData.edit")}
                </Button>
                <Button
                  type="link"
                  disabled={currency.isDefault}
                  onClick={() =>
                    void run(() => setDefault.mutateAsync(currency.id))
                  }
                >
                  {t("masterData.default")}
                </Button>
                <Button
                  type="link"
                  disabled={currency.isDefault}
                  onClick={() =>
                    void run(() =>
                      status.mutateAsync({
                        id: currency.id,
                        isActive: !currency.isActive,
                      }),
                    )
                  }
                >
                  {currency.isActive
                    ? t("masterData.deactivate")
                    : t("masterData.activate")}
                </Button>
              </>
            ),
          },
        ]}
      /> : null}
      <Modal
        open={editing !== undefined}
        footer={null}
        title={
          editing?.id ? t("masterData.edit") : t("masterData.currencies.new")
        }
        onCancel={() => setEditing(undefined)}
      >
        <CurrencyForm
          initial={editing?.id ? editing : undefined}
          onCancel={() => setEditing(undefined)}
          onSubmit={submit}
          loading={create.isPending || update.isPending}
        />
      </Modal>
    </ListPageLayout>
  );
}
function CurrencyForm({
  initial,
  onCancel,
  onSubmit,
  loading,
}: {
  initial?: Currency;
  onCancel: () => void;
  onSubmit: (values: CurrencyInput) => Promise<void>;
  loading: boolean;
}) {
  const { t } = useTranslation();
  const feedback = useApiFeedback();
  const [form] = Form.useForm<CurrencyInput>();
  async function submit(values: CurrencyInput) {
    try {
      await onSubmit(values);
    } catch (error) {
      if (!applyServerFieldErrors(form, error, t, "errors.validationFailed"))
        feedback.notifyError(error, "masterData.currencies.errors.save");
    }
  }
  return (
    <Form
      form={form}
      layout="vertical"
      onFinish={submit}
      initialValues={initial ?? { decimalPlaces: 2 }}
    >
      <Form.Item
        label={t("masterData.code")}
        name="code"
        rules={[{ required: true }]}
      >
        <Input disabled={Boolean(initial)} maxLength={3} />
      </Form.Item>
      <Form.Item
        label={t("masterData.name")}
        name="name"
        rules={[{ required: true }]}
      >
        <Input />
      </Form.Item>
      <Form.Item label={t("masterData.symbol")} name="symbol">
        <Input />
      </Form.Item>
      <Form.Item
        label={t("masterData.decimals")}
        name="decimalPlaces"
        rules={[{ required: true }]}
      >
        <InputNumber min={0} max={4} />
      </Form.Item>
      <ModalFormActions
        cancelLabel={t("ui.cancel")}
        isSubmitting={loading}
        onCancel={onCancel}
        submitLabel={t("masterData.save")}
      />
    </Form>
  );
}
