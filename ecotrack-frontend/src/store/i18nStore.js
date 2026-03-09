import { create } from 'zustand';
import { persist } from 'zustand/middleware';
export var useI18nStore = create()(persist(function (set) { return ({
    locale: 'en',
    setLocale: function (locale) { return set({ locale: locale }); },
}); }, {
    name: 'i18n-storage',
}));
