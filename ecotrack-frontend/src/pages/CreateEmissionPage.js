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
import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { categoriesApi, emissionsApi } from '../api';
import { useAuthStore } from '../store/authStore';
import { useI18n } from '../i18n';
var CATEGORY_UNIT_HINTS = [
    { match: /electric/i, unit: 'kWh' },
    { match: /natural gas|gas/i, unit: 'm3' },
    { match: /fleet fuel|fuel|diesel|petrol|benzin/i, unit: 'liters' },
    { match: /business travel|employee commuting|travel|transport/i, unit: 'km' },
    { match: /water/i, unit: 'm3' },
    { match: /waste/i, unit: 'kg' },
    { match: /purchased goods/i, unit: 'kg' },
];
function getUnitHint(categoryName, defaultHelper) {
    var _a;
    if (!categoryName)
        return { unit: 'units', helper: defaultHelper };
    var found = CATEGORY_UNIT_HINTS.find(function (x) { return x.match.test(categoryName); });
    return { unit: (_a = found === null || found === void 0 ? void 0 : found.unit) !== null && _a !== void 0 ? _a : 'units', helper: defaultHelper };
}
export default function CreateEmissionPage() {
    var _this = this;
    var navigate = useNavigate();
    var user = useAuthStore().user;
    var _a = useI18n(), t = _a.t, locale = _a.locale;
    var _b = useState([]), categories = _b[0], setCategories = _b[1];
    var _c = useState(''), categoryId = _c[0], setCategoryId = _c[1];
    var _d = useState(''), amount = _d[0], setAmount = _d[1];
    var _e = useState(new Date().toISOString().slice(0, 16)), reportedDate = _e[0], setReportedDate = _e[1];
    var _f = useState(''), rawData = _f[0], setRawData = _f[1];
    var _g = useState(''), error = _g[0], setError = _g[1];
    var _h = useState(false), loading = _h[0], setLoading = _h[1];
    var companyId = useMemo(function () { var _a; return (_a = user === null || user === void 0 ? void 0 : user.companyId) !== null && _a !== void 0 ? _a : ''; }, [user === null || user === void 0 ? void 0 : user.companyId]);
    var hasCategories = categories.length > 0;
    useEffect(function () {
        var load = function () { return __awaiter(_this, void 0, void 0, function () {
            var data_1, _a;
            return __generator(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        _b.trys.push([0, 2, , 3]);
                        return [4 /*yield*/, categoriesApi.getAll()];
                    case 1:
                        data_1 = _b.sent();
                        setCategories(data_1);
                        setCategoryId(function (prev) {
                            if (prev && data_1.some(function (c) { return c.id === prev; }))
                                return prev;
                            return data_1.length > 0 ? data_1[0].id : '';
                        });
                        return [3 /*break*/, 3];
                    case 2:
                        _a = _b.sent();
                        setError(t('failedLoadCategories'));
                        return [3 /*break*/, 3];
                    case 3: return [2 /*return*/];
                }
            });
        }); };
        load();
    }, [locale]);
    var submit = function (e) { return __awaiter(_this, void 0, void 0, function () {
        var created, err_1;
        var _a, _b;
        return __generator(this, function (_c) {
            switch (_c.label) {
                case 0:
                    e.preventDefault();
                    setError('');
                    if (!companyId) {
                        setError(t('accountNotLinked'));
                        return [2 /*return*/];
                    }
                    if (!hasCategories) {
                        setError(t('noCategoriesAvailable'));
                        return [2 /*return*/];
                    }
                    setLoading(true);
                    _c.label = 1;
                case 1:
                    _c.trys.push([1, 3, 4, 5]);
                    return [4 /*yield*/, emissionsApi.create({
                            companyId: companyId,
                            categoryId: categoryId,
                            amount: Number(amount),
                            reportedDate: new Date(reportedDate).toISOString(),
                            rawData: rawData,
                        })];
                case 2:
                    created = _c.sent();
                    navigate("/dashboard", { replace: true, state: { createdEmissionId: created.id } });
                    return [3 /*break*/, 5];
                case 3:
                    err_1 = _c.sent();
                    if (axios.isAxiosError(err_1)) {
                        setError(((_b = (_a = err_1.response) === null || _a === void 0 ? void 0 : _a.data) === null || _b === void 0 ? void 0 : _b.error) || t('failedCreateEmission'));
                    }
                    else {
                        setError(t('failedCreateEmission'));
                    }
                    return [3 /*break*/, 5];
                case 4:
                    setLoading(false);
                    return [7 /*endfinally*/];
                case 5: return [2 /*return*/];
            }
        });
    }); };
    var selectedCategory = categories.find(function (c) { return c.id === categoryId; });
    var unitHint = getUnitHint(selectedCategory === null || selectedCategory === void 0 ? void 0 : selectedCategory.name, t('unitHelperDefault'));
    var unitLabel = unitHint.unit === 'units' ? t('units') : unitHint.unit;
    return (_jsx("div", { className: "min-h-screen bg-gray-50 p-4 sm:p-6", children: _jsxs("div", { className: "max-w-3xl mx-auto card", children: [_jsx("h1", { className: "text-2xl font-bold text-gray-900 mb-6", children: t('createEmission') }), !hasCategories && (_jsx("div", { className: "mb-4 p-3 bg-amber-50 border border-amber-200 rounded-lg text-sm text-amber-800", children: t('categoriesEmptyRestart') })), error && _jsx("div", { className: "mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700", children: error }), _jsxs("form", { onSubmit: submit, className: "space-y-4", children: [_jsxs("div", { children: [_jsx("label", { className: "block text-sm font-medium text-gray-700 mb-2", children: t('category') }), _jsxs("select", { value: categoryId, onChange: function (e) { return setCategoryId(e.target.value); }, className: "input-field", disabled: !hasCategories, required: true, children: [!hasCategories && _jsx("option", { value: "", children: t('noCategoriesOption') }), categories.map(function (c) { return (_jsx("option", { value: c.id, children: c.name }, c.id)); })] })] }), _jsxs("div", { className: "grid grid-cols-1 sm:grid-cols-2 gap-4", children: [_jsxs("div", { children: [_jsxs("label", { className: "block text-sm font-medium text-gray-700 mb-2", children: [t('amount'), " (", unitLabel, ")"] }), _jsx("input", { className: "input-field", type: "number", min: "0.01", step: "0.01", value: amount, onChange: function (e) { return setAmount(e.target.value); }, placeholder: "".concat(t('enterValueIn'), " ").concat(unitLabel), required: true })] }), _jsxs("div", { children: [_jsx("label", { className: "block text-sm font-medium text-gray-700 mb-2", children: t('reportedDate') }), _jsx("input", { className: "input-field !w-auto min-w-[220px] max-w-full", type: "datetime-local", value: reportedDate, onChange: function (e) { return setReportedDate(e.target.value); }, required: true })] })] }), _jsxs("div", { children: [_jsx("label", { className: "block text-sm font-medium text-gray-700 mb-2", children: t('rawDataNote') }), _jsx("textarea", { className: "input-field min-h-32", value: rawData, onChange: function (e) { return setRawData(e.target.value); }, placeholder: t('aiTextExample'), required: true })] }), _jsxs("div", { className: "flex gap-3", children: [_jsx("button", { type: "submit", disabled: loading || !hasCategories, className: "btn-primary disabled:opacity-50", children: loading ? t('saving') : t('saveEmission') }), _jsx("button", { type: "button", className: "btn-secondary", onClick: function () { return navigate('/dashboard'); }, children: t('cancel') })] })] })] }) }));
}
