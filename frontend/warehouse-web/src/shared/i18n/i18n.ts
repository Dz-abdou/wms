import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import { languageStorageKey, toAppLanguage } from './constants'
import en from './locales/en/common.json'
import fr from './locales/fr/common.json'

function getInitialLanguage(): string {
  const savedLanguage = window.localStorage.getItem(languageStorageKey)
  return toAppLanguage(savedLanguage ?? window.navigator.language)
}

void i18n
  .use(initReactI18next)
  .init({
    fallbackLng: 'en',
    lng: getInitialLanguage(),
    interpolation: { escapeValue: false },
    resources: {
      en: { translation: en },
      fr: { translation: fr }
    }
  })

i18n.on('languageChanged', (language) => {
  window.localStorage.setItem(languageStorageKey, toAppLanguage(language))
})

export { i18n }