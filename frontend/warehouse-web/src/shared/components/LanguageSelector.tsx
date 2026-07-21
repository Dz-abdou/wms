import { Select } from 'antd'
import { useTranslation } from 'react-i18next'
import { supportedLanguages, toAppLanguage } from '../i18n/constants'

export function LanguageSelector() {
  const { i18n, t } = useTranslation()
  const language = toAppLanguage(i18n.resolvedLanguage)

  return (
    <Select
      aria-label={t('language.label')}
      className="language-selector"
      onChange={(nextLanguage: string) => void i18n.changeLanguage(nextLanguage)}
      options={supportedLanguages.map((supportedLanguage) => ({
        label: t(`language.${supportedLanguage}`),
        value: supportedLanguage
      }))}
      value={language}
    />
  )
}