export const languageStorageKey = 'warehouse-web.language'

export const supportedLanguages = ['en', 'fr'] as const

export type AppLanguage = (typeof supportedLanguages)[number]

export function toAppLanguage(language: string | undefined): AppLanguage {
  return language?.startsWith('fr') ? 'fr' : 'en'
}