import { jsxs as _jsxs, jsx as _jsx } from "react/jsx-runtime";
import { useI18n } from '../i18n';
export default function LanguageSwitch() {
    var _a = useI18n(), locale = _a.locale, setLocale = _a.setLocale, t = _a.t;
    return (_jsxs("div", { className: "inline-flex items-center gap-2 text-xs sm:text-sm", children: [_jsxs("span", { className: "text-gray-500", children: [t('language'), ":"] }), _jsxs("div", { className: "inline-flex rounded-md border border-gray-300 overflow-hidden", children: [_jsx("button", { type: "button", onClick: function () { return setLocale('en'); }, className: "px-2 py-1 cursor-pointer ".concat(locale === 'en' ? 'bg-emerald-600 text-white' : 'bg-white text-gray-700 hover:bg-gray-100'), children: "EN" }), _jsx("button", { type: "button", onClick: function () { return setLocale('sk'); }, className: "px-2 py-1 cursor-pointer ".concat(locale === 'sk' ? 'bg-emerald-600 text-white' : 'bg-white text-gray-700 hover:bg-gray-100'), children: "SK" })] })] }));
}
