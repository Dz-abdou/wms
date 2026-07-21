import { ConfigProvider } from 'antd'
import enUs from 'antd/locale/en_US'
import frFr from 'antd/locale/fr_FR'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ProductCreatePage } from '../features/products/pages/ProductCreatePage'
import { ProductDetailPage } from '../features/products/pages/ProductDetailPage'
import { ProductEditPage } from '../features/products/pages/ProductEditPage'
import { ProductListPage } from '../features/products/pages/ProductListPage'
import { productRoutes } from '../features/products/productConstants'
import { toAppLanguage } from '../shared/i18n/constants'
import '../shared/i18n/i18n'
import { ApplicationLayout } from '../layouts/ApplicationLayout'
import { HomePage } from '../pages/home/HomePage'
import { AppProviders } from './AppProviders'
import { applicationTheme } from './theme'

export function App() {
  const { i18n } = useTranslation()
  const antdLocale = toAppLanguage(i18n.resolvedLanguage) === 'fr' ? frFr : enUs

  return (
    <ConfigProvider locale={antdLocale} theme={applicationTheme}>
      <AppProviders>
        <BrowserRouter>
          <Routes>
            <Route element={<ApplicationLayout />}>
              <Route index element={<HomePage />} />
              <Route path={productRoutes.listPattern} element={<ProductListPage />} />
              <Route path={productRoutes.create} element={<ProductCreatePage />} />
              <Route path={productRoutes.detailPattern} element={<ProductDetailPage />} />
              <Route path={productRoutes.editPattern} element={<ProductEditPage />} />
            </Route>
          </Routes>
        </BrowserRouter>
      </AppProviders>
    </ConfigProvider>
  )
}