import { Card, Space, Tag, Typography } from 'antd'
import { useTranslation } from 'react-i18next'
import { ListPageLayout } from '../../shared/components/PageLayouts'

export function HomePage() {
  const { t } = useTranslation()

  return (
    <ListPageLayout title={t('home.title')}>
    <Card>
      <Space direction="vertical" size="middle">
        <Typography.Paragraph>{t('home.description')}</Typography.Paragraph>
        <Tag color="success">{t('home.phase')}</Tag>
      </Space>
    </Card>
    </ListPageLayout>
  )
}
