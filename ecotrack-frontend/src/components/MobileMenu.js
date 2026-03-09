import { jsx as _jsx, jsxs as _jsxs } from "react/jsx-runtime";
import { Menu, X, LogOut, Trash2 } from 'lucide-react';
import { useState } from 'react';
import LanguageSwitch from './LanguageSwitch';
import { useI18n } from '../i18n';
export default function MobileMenu(_a) {
    var onLogout = _a.onLogout, onDelete = _a.onDelete, deletingAccount = _a.deletingAccount;
    var _b = useState(false), isOpen = _b[0], setIsOpen = _b[1];
    var t = useI18n().t;
    var handleClose = function () { return setIsOpen(false); };
    return (_jsxs("div", { className: "sm:hidden relative", children: [_jsx("button", { onClick: function () { return setIsOpen(function (v) { return !v; }); }, className: "p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer", children: isOpen ? (_jsx(X, { className: "w-6 h-6 text-gray-700" })) : (_jsx(Menu, { className: "w-6 h-6 text-gray-700" })) }), _jsx("div", { className: "fixed inset-0 z-40 bg-black/30 transition-opacity duration-300 ".concat(isOpen ? 'opacity-100 pointer-events-auto' : 'opacity-0 pointer-events-none'), onClick: handleClose }), _jsxs("div", { className: "fixed top-[61px] left-0 right-0 z-50 bg-white border-t border-gray-200 shadow-xl transition-all duration-300 ease-out ".concat(isOpen ? 'opacity-100 translate-y-0' : 'opacity-0 -translate-y-3 pointer-events-none'), children: [_jsx("div", { className: "p-4 border-b border-gray-200", children: _jsx(LanguageSwitch, {}) }), _jsxs("button", { onClick: function () {
                            onDelete();
                            handleClose();
                        }, disabled: deletingAccount, className: "w-full flex items-center gap-3 px-4 py-4 text-sm text-red-700 hover:bg-red-50 transition-colors cursor-pointer disabled:opacity-50 border-b border-gray-200", children: [_jsx(Trash2, { className: "w-4 h-4" }), _jsx("span", { children: deletingAccount ? t('deleting') : t('deleteAccount') })] }), _jsxs("button", { onClick: function () {
                            onLogout();
                            handleClose();
                        }, className: "w-full flex items-center gap-3 px-4 py-4 text-sm text-gray-700 hover:bg-gray-100 transition-colors cursor-pointer", children: [_jsx(LogOut, { className: "w-4 h-4" }), _jsx("span", { children: t('logout') })] })] })] }));
}
