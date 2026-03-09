import { jsx as _jsx, Fragment as _Fragment, jsxs as _jsxs } from "react/jsx-runtime";
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { useAuthStore } from './store/authStore';
import AiExtractionPage from './pages/AiExtractionPage';
import CreateEmissionPage from './pages/CreateEmissionPage';
import DashboardPage from './pages/DashboardPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
function ProtectedRoute(_a) {
    var children = _a.children;
    var isAuthenticated = useAuthStore(function (state) { return state.isAuthenticated; });
    if (!isAuthenticated)
        return _jsx(Navigate, { to: "/login", replace: true });
    return _jsx(_Fragment, { children: children });
}
function PublicOnlyRoute(_a) {
    var children = _a.children;
    var isAuthenticated = useAuthStore(function (state) { return state.isAuthenticated; });
    if (isAuthenticated)
        return _jsx(Navigate, { to: "/dashboard", replace: true });
    return _jsx(_Fragment, { children: children });
}
function App() {
    return (_jsx(BrowserRouter, { children: _jsxs(Routes, { children: [_jsx(Route, { path: "/login", element: _jsx(PublicOnlyRoute, { children: _jsx(LoginPage, {}) }) }), _jsx(Route, { path: "/register", element: _jsx(PublicOnlyRoute, { children: _jsx(RegisterPage, {}) }) }), _jsx(Route, { path: "/dashboard", element: _jsx(ProtectedRoute, { children: _jsx(DashboardPage, {}) }) }), _jsx(Route, { path: "/emissions/create", element: _jsx(ProtectedRoute, { children: _jsx(CreateEmissionPage, {}) }) }), _jsx(Route, { path: "/emissions/ai", element: _jsx(ProtectedRoute, { children: _jsx(AiExtractionPage, {}) }) }), _jsx(Route, { path: "/", element: _jsx(Navigate, { to: "/dashboard", replace: true }) }), _jsx(Route, { path: "*", element: _jsx(Navigate, { to: "/dashboard", replace: true }) })] }) }));
}
export default App;
