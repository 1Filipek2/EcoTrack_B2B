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
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Building2, Leaf, Lock, Mail, Code } from 'lucide-react';
import axios from 'axios';
import { authApi } from '../api';
import { useI18n } from '../i18n';
import LanguageSwitch from '../components/LanguageSwitch';
var resendVerificationCode = function (email) {
    return authApi.resendVerificationCode
        ? authApi.resendVerificationCode(email)
        : Promise.reject('Not implemented');
};
export default function RegisterPage() {
    var _this = this;
    var _a = useState(''), email = _a[0], setEmail = _a[1];
    var _b = useState(''), password = _b[0], setPassword = _b[1];
    var _c = useState(''), companyId = _c[0], setCompanyId = _c[1];
    var _d = useState(''), verificationCode = _d[0], setVerificationCode = _d[1];
    var _e = useState(''), error = _e[0], setError = _e[1];
    var _f = useState(''), success = _f[0], setSuccess = _f[1];
    var _g = useState(false), loading = _g[0], setLoading = _g[1];
    var _h = useState('register'), step = _h[0], setStep = _h[1];
    var _j = useState(false), resendLoading = _j[0], setResendLoading = _j[1];
    var _k = useState(''), resendMessage = _k[0], setResendMessage = _k[1];
    var navigate = useNavigate();
    var t = useI18n().t;
    var handleRegister = function (e) { return __awaiter(_this, void 0, void 0, function () {
        var response, err_1;
        var _a, _b;
        return __generator(this, function (_c) {
            switch (_c.label) {
                case 0:
                    e.preventDefault();
                    setError('');
                    setSuccess('');
                    setLoading(true);
                    _c.label = 1;
                case 1:
                    _c.trys.push([1, 3, 4, 5]);
                    return [4 /*yield*/, authApi.register({
                            email: email,
                            password: password,
                            companyId: companyId.trim() ? companyId.trim() : undefined,
                        })];
                case 2:
                    response = _c.sent();
                    if (response.message.includes('Dev mode') || response.message.includes('verification skipped')) {
                        setSuccess(t('registerSuccess'));
                        setTimeout(function () { return navigate('/login'); }, 1500);
                    }
                    else {
                        setSuccess(t('checkEmailForCode'));
                        setStep('verify');
                    }
                    return [3 /*break*/, 5];
                case 3:
                    err_1 = _c.sent();
                    if (axios.isAxiosError(err_1)) {
                        setError(((_b = (_a = err_1.response) === null || _a === void 0 ? void 0 : _a.data) === null || _b === void 0 ? void 0 : _b.error) || t('registerFailed'));
                    }
                    else {
                        setError(t('registerFailed'));
                    }
                    return [3 /*break*/, 5];
                case 4:
                    setLoading(false);
                    return [7 /*endfinally*/];
                case 5: return [2 /*return*/];
            }
        });
    }); };
    var handleVerify = function (e) { return __awaiter(_this, void 0, void 0, function () {
        var err_2;
        var _a, _b;
        return __generator(this, function (_c) {
            switch (_c.label) {
                case 0:
                    e.preventDefault();
                    setError('');
                    setSuccess('');
                    setLoading(true);
                    _c.label = 1;
                case 1:
                    _c.trys.push([1, 3, 4, 5]);
                    return [4 /*yield*/, authApi.verifyEmail(email, verificationCode)];
                case 2:
                    _c.sent();
                    setSuccess(t('verifySuccess'));
                    setTimeout(function () { return navigate('/login'); }, 1500);
                    return [3 /*break*/, 5];
                case 3:
                    err_2 = _c.sent();
                    if (axios.isAxiosError(err_2)) {
                        setError(((_b = (_a = err_2.response) === null || _a === void 0 ? void 0 : _a.data) === null || _b === void 0 ? void 0 : _b.error) || t('verifyFailed'));
                    }
                    else {
                        setError(t('verifyFailed'));
                    }
                    return [3 /*break*/, 5];
                case 4:
                    setLoading(false);
                    return [7 /*endfinally*/];
                case 5: return [2 /*return*/];
            }
        });
    }); };
    var handleResendCode = function () { return __awaiter(_this, void 0, void 0, function () {
        var _a;
        return __generator(this, function (_b) {
            switch (_b.label) {
                case 0:
                    setResendLoading(true);
                    setResendMessage('');
                    setError('');
                    _b.label = 1;
                case 1:
                    _b.trys.push([1, 3, 4, 5]);
                    return [4 /*yield*/, resendVerificationCode(email)];
                case 2:
                    _b.sent();
                    setResendMessage(t('resendCodeSuccess'));
                    return [3 /*break*/, 5];
                case 3:
                    _a = _b.sent();
                    setResendMessage(t('resendCodeFailed'));
                    return [3 /*break*/, 5];
                case 4:
                    setResendLoading(false);
                    return [7 /*endfinally*/];
                case 5: return [2 /*return*/];
            }
        });
    }); };
    return (_jsx("div", { className: "min-h-screen flex items-center justify-center bg-linear-to-br from-emerald-50 to-gray-100 px-4 py-8 sm:px-6 lg:px-8", children: _jsxs("div", { className: "w-full max-w-md", children: [_jsx("div", { className: "flex justify-end mb-3", children: _jsx(LanguageSwitch, {}) }), _jsxs("div", { className: "text-center mb-10 sm:mb-12", children: [_jsx("div", { className: "inline-flex items-center justify-center w-16 h-16 bg-emerald-600 rounded-2xl mb-4 sm:mb-6", children: _jsx(Leaf, { className: "w-10 h-10 text-white" }) }), _jsx("h1", { className: "text-3xl sm:text-4xl font-bold text-gray-900", children: step === 'register' ? t('createAccount') : t('verifyEmailTitle') }), _jsx("p", { className: "text-gray-600 text-sm sm:text-base mt-3", children: step === 'register' ? t('startMeasuring') : t('verifyEmailDesc') })] }), _jsxs("div", { className: "bg-white rounded-2xl shadow-sm border border-gray-200 p-6 sm:p-8", children: [error && (_jsx("div", { className: "mb-4 p-3 sm:p-4 bg-red-50 border border-red-200 rounded-lg text-xs sm:text-sm text-red-700", children: error })), success && (_jsx("div", { className: "mb-4 p-3 sm:p-4 bg-green-50 border border-green-200 rounded-lg text-xs sm:text-sm text-green-700", children: success })), step === 'register' ? (_jsxs("form", { onSubmit: handleRegister, className: "space-y-5 sm:space-y-6", children: [_jsxs("div", { children: [_jsx("label", { className: "block text-xs sm:text-sm font-medium text-gray-700 mb-2", children: t('email') }), _jsxs("div", { className: "relative", children: [_jsx(Mail, { className: "absolute left-3 top-1/2 -translate-y-1/2 w-4 sm:w-5 h-4 sm:h-5 text-gray-400" }), _jsx("input", { type: "email", value: email, onChange: function (e) { return setEmail(e.target.value); }, className: "w-full pl-10 pr-4 py-2.5 sm:py-3 text-sm sm:text-base border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition", placeholder: "you@company.com", required: true })] })] }), _jsxs("div", { children: [_jsx("label", { className: "block text-xs sm:text-sm font-medium text-gray-700 mb-2", children: t('password') }), _jsxs("div", { className: "relative", children: [_jsx(Lock, { className: "absolute left-3 top-1/2 -translate-y-1/2 w-4 sm:w-5 h-4 sm:h-5 text-gray-400" }), _jsx("input", { type: "password", value: password, onChange: function (e) { return setPassword(e.target.value); }, className: "w-full pl-10 pr-4 py-2.5 sm:py-3 text-sm sm:text-base border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition", placeholder: t('passwordHint'), minLength: 6, required: true })] })] }), _jsxs("div", { children: [_jsx("label", { className: "block text-xs sm:text-sm font-medium text-gray-700 mb-2", children: t('companyIdOptional') }), _jsxs("div", { className: "relative", children: [_jsx(Building2, { className: "absolute left-3 top-1/2 -translate-y-1/2 w-4 sm:w-5 h-4 sm:h-5 text-gray-400" }), _jsx("input", { type: "text", value: companyId, onChange: function (e) { return setCompanyId(e.target.value); }, className: "w-full pl-10 pr-4 py-2.5 sm:py-3 text-sm sm:text-base border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition", placeholder: t('companyIdHint') })] })] }), _jsx("button", { type: "submit", disabled: loading, className: "w-full py-2.5 sm:py-3 bg-emerald-600 hover:bg-emerald-700 text-white text-sm sm:text-base font-medium rounded-lg transition-colors disabled:opacity-50", children: loading ? t('creatingAccount') : t('signUp') })] })) : (_jsxs("form", { onSubmit: handleVerify, className: "space-y-5 sm:space-y-6", children: [_jsxs("div", { children: [_jsx("label", { className: "block text-xs sm:text-sm font-medium text-gray-700 mb-2", children: t('verificationCode') }), _jsxs("div", { className: "relative", children: [_jsx(Code, { className: "absolute left-3 top-1/2 -translate-y-1/2 w-4 sm:w-5 h-4 sm:h-5 text-gray-400" }), _jsx("input", { type: "text", value: verificationCode, onChange: function (e) { return setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6)); }, className: "w-full pl-10 pr-4 py-2.5 sm:py-3 text-lg border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition text-center tracking-widest font-mono", placeholder: "000000", maxLength: 6, required: true })] }), _jsx("p", { className: "text-xs text-gray-500 mt-2", children: t('checkEmailForCode') }), _jsx("p", { className: "text-xs text-yellow-500 mt-1", children: t('spamWarning') }), resendMessage && (_jsx("div", { className: "mt-2 text-xs text-green-600", children: resendMessage })), _jsx("button", { type: "button", onClick: handleResendCode, disabled: resendLoading, className: "mt-2 py-2 px-4 bg-gray-200 hover:bg-gray-300 text-gray-700 text-xs font-medium rounded-lg transition-colors disabled:opacity-50", children: resendLoading ? t('verifying') : t('resendCode') })] }), _jsx("button", { type: "submit", disabled: loading || verificationCode.length !== 6, className: "w-full py-2.5 sm:py-3 bg-emerald-600 hover:bg-emerald-700 text-white text-sm sm:text-base font-medium rounded-lg transition-colors disabled:opacity-50", children: loading ? t('verifying') : t('verifyEmailButton') }), _jsx("button", { type: "button", onClick: function () {
                                        setStep('register');
                                        setVerificationCode('');
                                        setError('');
                                        setSuccess('');
                                    }, className: "w-full py-2.5 sm:py-3 bg-gray-200 hover:bg-gray-300 text-gray-700 text-sm sm:text-base font-medium rounded-lg transition-colors", children: t('back') })] })), _jsxs("p", { className: "text-xs sm:text-sm text-center text-gray-600 mt-8 sm:mt-10", children: [t('alreadyHaveAccount'), ' ', _jsx(Link, { to: "/login", className: "text-emerald-700 font-semibold hover:underline", children: t('signIn') })] })] })] }) }));
}
