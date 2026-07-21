import { Alert, Card, Spin, Typography } from 'antd'
import { useNavigate, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../../shared/errors/problemDetails'
import type { WarehouseInput } from '../api/warehouseTypes'
import { useWarehouse, useUpdateWarehouse } from '../api/useWarehouses'
import { WarehouseForm } from '../components/WarehouseForm'
import { warehouseRoutes } from '../warehouseConstants'
export function WarehouseEditPage(){const {id}=useParams();const n=useNavigate();const {t}=useTranslation();const q=useWarehouse(id);const update=useUpdateWarehouse(id??'');if(q.isLoading)return <Spin className="page-spinner" size="large" tip={t('warehouses.loadingOne')}/>;if(q.error||!q.data||!id)return <Alert message={getErrorMessage(t,q.error,'warehouses.errors.load')} showIcon type="error"/>;async function submit(v:WarehouseInput){await update.mutateAsync(v);n(warehouseRoutes.detail(id!))}return <section><Typography.Title level={2}>{t('warehouses.editTitle')}</Typography.Title>{update.error?<Alert className="page-alert" message={getErrorMessage(t,update.error,'warehouses.errors.update')} showIcon type="error"/>:null}<Card><WarehouseForm initialValues={{code:q.data.code,name:q.data.name,description:q.data.description??undefined}} isSubmitting={update.isPending} onSubmit={submit} submitLabel={t('warehouses.save')}/></Card></section>}