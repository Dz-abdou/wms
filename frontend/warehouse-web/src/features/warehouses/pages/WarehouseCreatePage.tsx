import { Alert, Card, Typography } from 'antd'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../../shared/errors/problemDetails'
import type { WarehouseInput } from '../api/warehouseTypes'
import { useCreateWarehouse } from '../api/useWarehouses'
import { WarehouseForm } from '../components/WarehouseForm'
import { warehouseRoutes } from '../warehouseConstants'
export function WarehouseCreatePage(){const n=useNavigate();const {t}=useTranslation();const create=useCreateWarehouse();async function submit(v:WarehouseInput){const w=await create.mutateAsync(v);n(warehouseRoutes.detail(w.id))}return <section><Typography.Title level={2}>{t('warehouses.createTitle')}</Typography.Title>{create.error?<Alert className="page-alert" message={getErrorMessage(t,create.error,'warehouses.errors.create')} showIcon type="error"/>:null}<Card><WarehouseForm isSubmitting={create.isPending} onSubmit={submit} submitLabel={t('warehouses.create')}/></Card></section>}