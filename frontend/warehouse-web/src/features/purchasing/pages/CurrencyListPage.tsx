import { Button, Form, Input, InputNumber, Modal, Table, Tag, Typography } from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useCreateCurrency, useCurrencies, useSetCurrencyStatus, useSetDefaultCurrency } from "../api/usePurchasing";
import type { CurrencyInput } from "../api/purchasingTypes";

export function CurrencyListPage() {
  const { t } = useTranslation(); const [open, setOpen] = useState(false); const currencies = useCurrencies(); const create = useCreateCurrency(); const status = useSetCurrencyStatus(); const setDefault = useSetDefaultCurrency();
  async function submit(values: CurrencyInput) { await create.mutateAsync(values); setOpen(false); }
  return <section><div className="page-heading"><div><Typography.Title level={2}>{t("masterData.currencies.title")}</Typography.Title><Typography.Paragraph type="secondary">{t("masterData.currencies.subtitle")}</Typography.Paragraph></div><Button type="primary" onClick={() => setOpen(true)}>{t("masterData.new")}</Button></div>
    <Table rowKey="id" loading={currencies.isLoading} dataSource={currencies.data?.items} pagination={false} columns={[
      { title: t("masterData.code"), dataIndex: "code" }, { title: t("masterData.name"), dataIndex: "name" }, { title: t("masterData.symbol"), dataIndex: "symbol" }, { title: t("masterData.decimals"), dataIndex: "decimalPlaces" },
      { title: t("masterData.status"), render: (_, currency) => currency.isActive ? <Tag color="green">{t("masterData.active")}</Tag> : <Tag>{t("masterData.inactive")}</Tag> },
      { title: t("masterData.actions"), render: (_, currency) => <><Button type="link" disabled={currency.isDefault} onClick={() => void setDefault.mutateAsync(currency.id)}>{t("masterData.default")}</Button><Button type="link" disabled={currency.isDefault} onClick={() => void status.mutateAsync({ id: currency.id, isActive: !currency.isActive })}>{currency.isActive ? t("masterData.deactivate") : t("masterData.activate")}</Button></> },
    ]} />
    <Modal open={open} footer={null} title={t("masterData.currencies.new")} onCancel={() => setOpen(false)}><CurrencyForm onSubmit={submit} loading={create.isPending} /></Modal></section>;
}
function CurrencyForm({ onSubmit, loading }: { onSubmit: (values: CurrencyInput) => Promise<void>; loading: boolean }) { const { t } = useTranslation(); return <Form layout="vertical" onFinish={onSubmit} initialValues={{ decimalPlaces: 2 }}><Form.Item label={t("masterData.code")} name="code" rules={[{ required: true }]}><Input maxLength={3} /></Form.Item><Form.Item label={t("masterData.name")} name="name" rules={[{ required: true }]}><Input /></Form.Item><Form.Item label={t("masterData.symbol")} name="symbol"><Input /></Form.Item><Form.Item label={t("masterData.decimals")} name="decimalPlaces" rules={[{ required: true }]}><InputNumber min={0} max={4} /></Form.Item><Button htmlType="submit" loading={loading} type="primary">{t("masterData.save")}</Button></Form>; }
