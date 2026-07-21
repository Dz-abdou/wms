import type { AppLanguage } from '../i18n/constants'

const dateTimeLocales: Record<AppLanguage, string> = {
  en: 'en-US',
  fr: 'fr-FR'
}

export function formatDateTime(timestamp: string, language: AppLanguage): string {
  return new Intl.DateTimeFormat(dateTimeLocales[language], {
    dateStyle: 'medium',
    timeStyle: 'short'
  }).format(new Date(timestamp))
}