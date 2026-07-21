import { Layout, Menu, Typography } from 'antd'
import { Link, Outlet, useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { productRoutes } from '../features/products/productConstants'
import { LanguageSelector } from '../shared/components/LanguageSelector'

const { Content, Header } = Layout

export function ApplicationLayout() {
  const location = useLocation()
  const { t } = useTranslation()
  const selectedKey = location.pathname.startsWith(productRoutes.list) ? 'products' : 'home'

  return (
    <Layout className="application-layout">
      <Header className="application-header">
        <Link className="application-title" to="/">
          <Typography.Title level={3}>{t('app.brand')}</Typography.Title>
        </Link>
        <Menu
          className="application-menu"
          items={[
            { key: 'home', label: <Link to="/">{t('navigation.home')}</Link> },
            { key: 'products', label: <Link to={productRoutes.list}>{t('navigation.products')}</Link> }
          ]}
          mode="horizontal"
          selectedKeys={[selectedKey]}
          theme="dark"
        />
        <LanguageSelector />
      </Header>
      <Content className="application-content">
        <Outlet />
      </Content>
    </Layout>
  )
}