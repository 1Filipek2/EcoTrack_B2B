import axios from 'axios';
var rawApiBaseUrl = (typeof import.meta !== 'undefined' && import.meta.env && import.meta.env.VITE_API_URL) ? import.meta.env.VITE_API_URL : 'http://localhost:5222/api';
function normalizeApiBaseUrl(url) {
    var trimmed = url.trim().replace(/\/+$/, '');
    return trimmed.endsWith('/api') ? trimmed : "".concat(trimmed, "/api");
}
var API_BASE_URL = normalizeApiBaseUrl(rawApiBaseUrl);
export var apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});
// Request interceptor to add auth token
apiClient.interceptors.request.use(function (config) {
    var token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = "Bearer ".concat(token);
    }
    return config;
}, function (error) {
    return Promise.reject(error);
});
// Response interceptor for error handling
apiClient.interceptors.response.use(function (response) { return response; }, function (error) {
    var _a;
    if (((_a = error.response) === null || _a === void 0 ? void 0 : _a.status) === 401) {
        // Unauthorized - clear token and redirect to login
        localStorage.removeItem('token');
        window.location.href = '/login';
    }
    return Promise.reject(error);
});
// No custom ImportMeta interface found. Confirmed correct usage of import.meta.env for Vite.
