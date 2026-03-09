var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g = Object.create((typeof Iterator === "function" ? Iterator : Object).prototype);
    return g.next = verb(0), g["throw"] = verb(1), g["return"] = verb(2), typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (g && (g = 0, op[0] && (_ = 0)), _) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
import { jsx as _jsx, jsxs as _jsxs } from "react/jsx-runtime";
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { BarChart3, TrendingUp, Leaf, Plus, Building2, Info, ChevronDown, } from 'lucide-react';
import axios from 'axios';
import { authApi, companiesApi, emissionsApi } from '../api';
import { useAuthStore } from '../store/authStore';
import LanguageSwitch from '../components/LanguageSwitch';
import MobileMenu from '../components/MobileMenu';
import { useI18n } from '../i18n';
export default function DashboardPage() {
    var _this = this;
    var _a = useAuthStore(), user = _a.user, logout = _a.logout, updateAuth = _a.updateAuth;
    var navigate = useNavigate();
    var t = useI18n().t;
    var _b = useState(null), report = _b[0], setReport = _b[1];
    var _c = useState(null), emissions = _c[0], setEmissions = _c[1];
    var _d = useState(true), loading = _d[0], setLoading = _d[1];
    var _e = useState(''), linkError = _e[0], setLinkError = _e[1];
    var _f = useState(''), companyName = _f[0], setCompanyName = _f[1];
    var _g = useState(''), vatNumber = _g[0], setVatNumber = _g[1];
    var _h = useState(false), linking = _h[0], setLinking = _h[1];
    var _j = useState(null), company = _j[0], setCompany = _j[1];
    var _k = useState(null), expandedEmissionId = _k[0], setExpandedEmissionId = _k[1];
    var _l = useState(false), deletingAccount = _l[0], setDeletingAccount = _l[1];
    var _m = useState(false), showWelcome = _m[0], setShowWelcome = _m[1];
    var _o = useState(false), hideWelcome = _o[0], setHideWelcome = _o[1];
    useEffect(function () {
        var loadData = function () { return __awaiter(_this, void 0, void 0, function () {
            var emissionsData, _a, reportData, companyData, error_1;
            return __generator(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        _b.trys.push([0, 5, 6, 7]);
                        return [4 /*yield*/, emissionsApi.getAll({ pageNumber: 1, pageSize: 5 })];
                    case 1:
                        emissionsData = _b.sent();
                        setEmissions(emissionsData);
                        if (!(user === null || user === void 0 ? void 0 : user.companyId)) return [3 /*break*/, 3];
                        return [4 /*yield*/, Promise.all([
                                companiesApi.getSustainabilityReport(user.companyId),
                                companiesApi.getById(user.companyId),
                            ])];
                    case 2:
                        _a = _b.sent(), reportData = _a[0], companyData = _a[1];
                        setReport(reportData);
                        setCompany(companyData);
                        return [3 /*break*/, 4];
                    case 3:
                        // Admin users may not be linked to a specific company.
                        setReport({
                            companyId: '',
                            totalEmissions: 0,
                            scope1: 0,
                            scope2: 0,
                            scope3: 0,
                        });
                        setCompany(null);
                        _b.label = 4;
                    case 4: return [3 /*break*/, 7];
                    case 5:
                        error_1 = _b.sent();
                        console.error('Failed to load data:', error_1);
                        return [3 /*break*/, 7];
                    case 6:
                        setLoading(false);
                        return [7 /*endfinally*/];
                    case 7: return [2 /*return*/];
                }
            });
        }); };
        loadData();
    }, [user === null || user === void 0 ? void 0 : user.companyId]);
    useEffect(function () {
        if (sessionStorage.getItem('showWelcomeOnce') === '1') {
            setShowWelcome(true);
            sessionStorage.removeItem('showWelcomeOnce');
            var hideTimer_1 = setTimeout(function () {
                setHideWelcome(true);
            }, 1500);
            var removeTimer_1 = setTimeout(function () {
                setShowWelcome(false);
                setHideWelcome(false);
            }, 2000);
            return function () {
                clearTimeout(hideTimer_1);
                clearTimeout(removeTimer_1);
            };
        }
    }, []);
    var handleLogout = function () {
        logout();
        navigate('/login');
    };
    var handleCreateAndLinkCompany = function (e) { return __awaiter(_this, void 0, void 0, function () {
        var createdCompany, refreshedAuth, err_1;
        var _a, _b;
        return __generator(this, function (_c) {
            switch (_c.label) {
                case 0:
                    e.preventDefault();
                    setLinkError('');
                    setLinking(true);
                    _c.label = 1;
                case 1:
                    _c.trys.push([1, 4, 5, 6]);
                    return [4 /*yield*/, companiesApi.create({ name: companyName, vatNumber: vatNumber })];
                case 2:
                    createdCompany = _c.sent();
                    return [4 /*yield*/, authApi.linkCompany({ companyId: createdCompany.id })];
                case 3:
                    refreshedAuth = _c.sent();
                    updateAuth(refreshedAuth);
                    setCompanyName('');
                    setVatNumber('');
                    return [3 /*break*/, 6];
                case 4:
                    err_1 = _c.sent();
                    if (axios.isAxiosError(err_1)) {
                        setLinkError(((_b = (_a = err_1.response) === null || _a === void 0 ? void 0 : _a.data) === null || _b === void 0 ? void 0 : _b.error) || t('failedLinkCompany'));
                    }
                    else {
                        setLinkError(t('failedLinkCompany'));
                    }
                    return [3 /*break*/, 6];
                case 5:
                    setLinking(false);
                    return [7 /*endfinally*/];
                case 6: return [2 /*return*/];
            }
        });
    }); };
    var handleDeleteAccount = function () { return __awaiter(_this, void 0, void 0, function () {
        var err_2;
        var _a, _b;
        return __generator(this, function (_c) {
            switch (_c.label) {
                case 0:
                    if (!window.confirm(t('confirmDeleteAccount')))
                        return [2 /*return*/];
                    setDeletingAccount(true);
                    _c.label = 1;
                case 1:
                    _c.trys.push([1, 3, 4, 5]);
                    return [4 /*yield*/, authApi.deleteMe()];
                case 2:
                    _c.sent();
                    logout();
                    navigate('/login');
                    return [3 /*break*/, 5];
                case 3:
                    err_2 = _c.sent();
                    if (axios.isAxiosError(err_2)) {
                        alert(((_b = (_a = err_2.response) === null || _a === void 0 ? void 0 : _a.data) === null || _b === void 0 ? void 0 : _b.error) || t('failedDeleteAccount'));
                    }
                    else {
                        alert(t('failedDeleteAccount'));
                    }
                    return [3 /*break*/, 5];
                case 4:
                    setDeletingAccount(false);
                    return [7 /*endfinally*/];
                case 5: return [2 /*return*/];
            }
        });
    }); };
    if (loading) {
        return (_jsx("div", { className: "min-h-screen flex items-center justify-center", children: _jsx("div", { className: "animate-spin rounded-full h-12 w-12 border-b-2 border-emerald-600" }) }));
    }
    return (_jsxs("div", { className: "min-h-screen bg-gray-50", children: [showWelcome && (_jsx("div", { className: "fixed top-4 sm:top-6 left-1/2 -translate-x-1/2 z-50 ".concat(hideWelcome ? 'animate-slide-out-top' : 'animate-slide-in-top'), children: _jsx("div", { className: "px-4 sm:px-6 py-2 sm:py-3 rounded-full bg-emerald-600 text-white shadow-lg font-semibold text-sm sm:text-base whitespace-nowrap", children: t('welcome') }) })), _jsx("header", { className: "bg-white border-b border-gray-200 sticky top-0 z-40", children: _jsx("div", { className: "max-w-7xl mx-auto px-4 sm:px-6 lg:px-8", children: _jsxs("div", { className: "flex justify-between items-center py-3 sm:py-4", children: [_jsxs("div", { className: "flex items-center gap-2 sm:gap-3 flex-1", children: [_jsx("div", { className: "w-8 sm:w-10 h-8 sm:h-10 bg-emerald-600 rounded-xl flex items-center justify-center flex-shrink-0", children: _jsx(Leaf, { className: "w-5 sm:w-6 h-5 sm:h-6 text-white" }) }), _jsxs("div", { className: "min-w-0", children: [_jsx("h1", { className: "text-lg sm:text-xl font-bold text-gray-900", children: t('appName') }), _jsx("p", { className: "text-xs sm:text-sm text-gray-600 truncate", children: user === null || user === void 0 ? void 0 : user.email })] })] }), _jsxs("div", { className: "hidden sm:flex items-center gap-2", children: [_jsx(LanguageSwitch, {}), _jsx("button", { type: "button", onClick: handleDeleteAccount, disabled: deletingAccount, className: "flex items-center gap-1 px-3 sm:px-4 py-2 text-xs sm:text-sm text-red-700 hover:bg-red-50 rounded-lg transition-colors cursor-pointer disabled:opacity-50 whitespace-nowrap", children: _jsx("span", { children: deletingAccount ? t('deleting') : t('deleteAccount') }) }), _jsx("button", { onClick: handleLogout, className: "flex items-center gap-1 px-2 sm:px-4 py-2 text-xs sm:text-sm text-gray-700 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer", children: _jsx("span", { children: t('logout') }) })] }), _jsx(MobileMenu, { onLogout: handleLogout, onDelete: handleDeleteAccount, deletingAccount: deletingAccount })] }) }) }), _jsxs("main", { className: "max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4 sm:py-8", children: [company && (_jsxs("div", { className: "card mb-4 sm:mb-6 border-emerald-200 bg-emerald-50/40", children: [_jsx("p", { className: "text-xs sm:text-sm text-gray-600", children: t('linkedCompany') }), _jsx("p", { className: "text-base sm:text-lg font-semibold text-gray-900 mt-1", children: company.name }), _jsxs("p", { className: "text-xs sm:text-sm text-gray-600 mt-1", children: [t('vat'), ": ", company.vatNumber] })] })), !(user === null || user === void 0 ? void 0 : user.companyId) && (_jsxs("div", { className: "card mb-6 sm:mb-8 border-emerald-200 bg-emerald-50/40", children: [_jsxs("div", { className: "flex gap-2 sm:gap-3 mb-3 sm:mb-4", children: [_jsx("div", { className: "p-2 rounded-lg bg-emerald-100 text-emerald-700 flex-shrink-0", children: _jsx(Building2, { className: "w-4 sm:w-5 h-4 sm:h-5" }) }), _jsxs("div", { className: "min-w-0", children: [_jsx("h2", { className: "text-sm sm:text-lg font-semibold text-gray-900", children: t('linkAccountTitle') }), _jsx("p", { className: "text-xs sm:text-sm text-gray-600 mt-0.5", children: t('linkAccountDesc') })] })] }), linkError && (_jsx("div", { className: "mb-3 p-2 sm:p-3 bg-red-50 border border-red-200 rounded-lg text-xs sm:text-sm text-red-700", children: linkError })), _jsxs("form", { onSubmit: handleCreateAndLinkCompany, className: "grid grid-cols-1 sm:grid-cols-3 gap-2 sm:gap-3", children: [_jsx("input", { className: "input-field text-xs sm:text-sm py-2 sm:py-2.5", placeholder: t('companyName'), value: companyName, onChange: function (e) { return setCompanyName(e.target.value); }, required: true }), _jsx("input", { className: "input-field text-xs sm:text-sm py-2 sm:py-2.5", placeholder: t('vatNumber'), value: vatNumber, onChange: function (e) { return setVatNumber(e.target.value); }, required: true }), _jsx("button", { type: "submit", className: "btn-primary text-xs sm:text-sm py-2 sm:py-2.5 disabled:opacity-50", disabled: linking, children: linking ? t('linking') : t('createAndLink') })] })] })), _jsxs("div", { className: "grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4 sm:gap-6 mb-4 sm:mb-6", children: [_jsx(StatsCard, { title: t('totalEmissions'), value: "".concat((report === null || report === void 0 ? void 0 : report.totalEmissions.toFixed(2)) || 0, " kg"), icon: _jsx(TrendingUp, { className: "w-4 sm:w-6 h-4 sm:h-6" }), color: "bg-blue-500" }), _jsx(StatsCard, { title: t('scope1'), value: "".concat((report === null || report === void 0 ? void 0 : report.scope1.toFixed(2)) || 0, " kg"), icon: _jsx(BarChart3, { className: "w-4 sm:w-6 h-4 sm:h-6" }), color: "bg-red-500" }), _jsx(StatsCard, { title: t('scope2'), value: "".concat((report === null || report === void 0 ? void 0 : report.scope2.toFixed(2)) || 0, " kg"), icon: _jsx(BarChart3, { className: "w-4 sm:w-6 h-4 sm:h-6" }), color: "bg-yellow-500" }), _jsx(StatsCard, { title: t('scope3'), value: "".concat((report === null || report === void 0 ? void 0 : report.scope3.toFixed(2)) || 0, " kg"), icon: _jsx(BarChart3, { className: "w-4 sm:w-6 h-4 sm:h-6" }), color: "bg-green-500" })] }), _jsx("div", { className: "card mb-6 sm:mb-8 border-blue-100 bg-blue-50/40", children: _jsxs("div", { className: "flex gap-2 sm:gap-3", children: [_jsx(Info, { className: "w-4 sm:w-5 h-4 sm:h-5 text-blue-700 mt-0.5 flex-shrink-0" }), _jsxs("div", { className: "text-xs sm:text-sm text-gray-700 space-y-1", children: [_jsxs("p", { children: [_jsxs("strong", { children: [t('scope1'), ":"] }), " ", t('scope1Help')] }), _jsxs("p", { children: [_jsxs("strong", { children: [t('scope2'), ":"] }), " ", t('scope2Help')] }), _jsxs("p", { children: [_jsxs("strong", { children: [t('scope3'), ":"] }), " ", t('scope3Help')] })] })] }) }), _jsxs("div", { className: "grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-6 mb-6 sm:mb-8", children: [_jsx(ActionCard, { title: t('addEmissionEntry'), description: t('addEmissionDesc'), icon: _jsx(Plus, { className: "w-5 sm:w-6 h-5 sm:h-6" }), onClick: function () { return navigate('/emissions/create'); } }), _jsx(ActionCard, { title: t('aiExtraction'), description: t('aiExtractionDesc'), icon: _jsx(Leaf, { className: "w-5 sm:w-6 h-5 sm:h-6" }), onClick: function () { return navigate('/emissions/ai'); } })] }), _jsxs("div", { className: "card", children: [_jsx("h2", { className: "text-lg sm:text-xl font-semibold text-gray-900 mb-3 sm:mb-4", children: t('recentEmissions') }), emissions && emissions.items.length > 0 ? (_jsx("div", { className: "space-y-2 sm:space-y-3", children: emissions.items.map(function (emission) {
                                    var expanded = expandedEmissionId === emission.id;
                                    return (_jsxs("button", { type: "button", onClick: function () { return setExpandedEmissionId(expanded ? null : emission.id); }, className: "w-full text-left flex flex-col gap-2 p-3 sm:p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors cursor-pointer", children: [_jsxs("div", { className: "flex justify-between items-center gap-3", children: [_jsx("div", { className: "min-w-0 flex-1", children: _jsx("p", { className: "font-medium text-sm sm:text-base text-gray-900 truncate", children: emission.category }) }), _jsxs("div", { className: "flex items-center gap-2 flex-shrink-0", children: [_jsxs("p", { className: "text-xs sm:text-sm font-medium text-gray-700 whitespace-nowrap", children: [emission.amount, " ", t('units')] }), _jsx(ChevronDown, { className: "w-4 sm:w-5 h-4 sm:h-5 text-gray-500 transition-transform duration-300 ".concat(expanded ? 'rotate-180' : '') })] })] }), _jsxs("div", { className: "flex justify-between items-center gap-2", children: [_jsx("p", { className: "text-xs sm:text-sm text-gray-600", children: new Date(emission.reportDate).toLocaleDateString() }), _jsxs("p", { className: "font-semibold text-sm sm:text-base text-gray-900 whitespace-nowrap", children: [emission.co2Equivalent.toFixed(2), " kg CO\u2082"] })] }), _jsx("div", { className: "overflow-hidden transition-all duration-300 ease-in-out ".concat(expanded ? 'max-h-40 opacity-100' : 'max-h-0 opacity-0'), children: _jsx("div", { className: "pt-2 text-xs sm:text-sm text-gray-600 border-t border-gray-200 mt-1", children: emission.rawData }) })] }, emission.id));
                                }) })) : (_jsx("p", { className: "text-gray-600 text-center py-6 sm:py-8 text-sm sm:text-base", children: t('noEmissions') }))] })] })] }));
}
function StatsCard(_a) {
    var title = _a.title, value = _a.value, icon = _a.icon, color = _a.color;
    return (_jsxs("div", { className: "card p-4 sm:p-6", children: [_jsxs("div", { className: "flex items-center justify-between gap-2 mb-2 sm:mb-3", children: [_jsx("p", { className: "text-xs sm:text-sm font-medium text-gray-600 line-clamp-2", children: title }), _jsx("div", { className: "".concat(color, " p-1.5 sm:p-2 rounded-lg text-white flex-shrink-0"), children: icon })] }), _jsx("p", { className: "text-lg sm:text-2xl font-bold text-gray-900", children: value })] }));
}
function ActionCard(_a) {
    var title = _a.title, description = _a.description, icon = _a.icon, onClick = _a.onClick;
    return (_jsx("button", { onClick: onClick, className: "card text-left hover:shadow-md transition-shadow group cursor-pointer p-4 sm:p-6", children: _jsxs("div", { className: "flex gap-3 sm:gap-4", children: [_jsx("div", { className: "bg-emerald-100 p-2 sm:p-3 rounded-xl text-emerald-600 group-hover:bg-emerald-600 group-hover:text-white transition-colors flex-shrink-0", children: icon }), _jsxs("div", { className: "min-w-0", children: [_jsx("h3", { className: "text-sm sm:text-lg font-semibold text-gray-900 mb-0.5 sm:mb-1", children: title }), _jsx("p", { className: "text-xs sm:text-sm text-gray-600", children: description })] })] }) }));
}
