import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export type Locale = 'en' | 'sk';

interface I18nState {
  locale: Locale;
  setLocale: (locale: Locale) => void;
}

export const useI18nStore = create<I18nState>()(
  persist(
    (set) => ({
      locale: 'en',
      setLocale: (locale) => set({ locale }),
    }),
    {
      name: 'i18n-storage',
    }
  )
);

