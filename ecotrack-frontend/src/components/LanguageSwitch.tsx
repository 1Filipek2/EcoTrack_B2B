import { useI18n } from '../i18n';

export default function LanguageSwitch() {
  const { locale, setLocale, t } = useI18n();

  return (
    <div className="inline-flex items-center gap-2 text-xs sm:text-sm">
      <span className="text-gray-500">{t('language')}:</span>
      <div className="inline-flex rounded-md border border-gray-300 overflow-hidden">
        <button
          type="button"
          onClick={() => setLocale('en')}
          className={`px-2 py-1 cursor-pointer ${locale === 'en' ? 'bg-emerald-600 text-white' : 'bg-white text-gray-700 hover:bg-gray-100'}`}
        >
          EN
        </button>
        <button
          type="button"
          onClick={() => setLocale('sk')}
          className={`px-2 py-1 cursor-pointer ${locale === 'sk' ? 'bg-emerald-600 text-white' : 'bg-white text-gray-700 hover:bg-gray-100'}`}
        >
          SK
        </button>
      </div>
    </div>
  );
}

