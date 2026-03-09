import { create } from 'zustand';
import { persist } from 'zustand/middleware';
export var useAuthStore = create()(persist(function (set) { return ({
    token: null,
    user: null,
    isAuthenticated: false,
    login: function (authData) {
        localStorage.setItem('token', authData.token);
        set({
            token: authData.token,
            user: {
                id: '',
                email: authData.email,
                role: authData.role,
                companyId: authData.companyId,
            },
            isAuthenticated: true,
        });
    },
    updateAuth: function (authData) {
        localStorage.setItem('token', authData.token);
        set({
            token: authData.token,
            user: {
                id: '',
                email: authData.email,
                role: authData.role,
                companyId: authData.companyId,
            },
            isAuthenticated: true,
        });
    },
    logout: function () {
        localStorage.removeItem('token');
        set({
            token: null,
            user: null,
            isAuthenticated: false,
        });
    },
}); }, {
    name: 'auth-storage',
}));
