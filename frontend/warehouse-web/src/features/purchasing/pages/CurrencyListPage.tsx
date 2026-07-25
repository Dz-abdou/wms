import {
  Button,
  Form,
  Input,
  InputNumber,
  Modal,
  Table,
  Tag,
  Typography,
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
    <section>
      <div className="page-heading">
        <div>
          <Typography.Title level={2}>
            {t("masterData.currencies.title")}
          </Typography.Title>
          <Typography.Paragraph type="secondary">
            {t("masterData.currencies.subtitle")}
          </Typography.Paragraph>
        </div>
        <Button type="primary" onClick={() => setEditing({} as Currency)}>
          {t("masterData.new")}
        </Button>
      </div>
      <Table
        rowKey="id"
        loading={currencies.isLoading}
        dataSource={currencies.data?.items}
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
      />
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
          onSubmit={submit}
          loading={create.isPending || update.isPending}
        />
      </Modal>
    </section>
  );
}
function CurrencyForm({
  initial,
  onSubmit,
  loading,
}: {
  initial?: Currency;
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
      <Button htmlType="submit" loading={loading} type="primary">
        {t("masterData.save")}
      </Button>
    </Form>
  );
}
