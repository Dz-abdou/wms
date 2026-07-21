import { Tag } from 'antd'
import { useTranslation } from 'react-i18next'

export function ProductStatusTag({ isActive }: { isActive: boolean }) {
  const { t } = useTranslation()
  const statusKey = isActive ? 'products.status.active' : 'products.status.inactive'

  return <Tag color={isActive ? 'green' : 'default'}>{t(statusKey)}</Tag>
}