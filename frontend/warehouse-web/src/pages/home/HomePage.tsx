import { Card, Space, Tag, Typography } from 'antd'
import { useTranslation } from 'react-i18next'

export function HomePage() {
  const { t } = useTranslation()

  return (
    <Card title={t('home.title')}>
      <Space direction="vertical" size="middle">
        <Typography.Paragraph>{t('home.description')}</Typography.Paragraph>
        <Tag color="success">{t('home.phase')}</Tag>
      </Space>
    </Card>
  )
}