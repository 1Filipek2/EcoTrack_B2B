import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { AuthResponse, User } from '../types/api';

interface AuthState {
  token: string | null;
  user: User | null;
  isAuthenticated: boolean;
  login: (authData: AuthResponse) => void;
  updateAuth: (authData: AuthResponse) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      isAuthenticated: false,

      login: (authData: AuthResponse) => {
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

      updateAuth: (authData: AuthResponse) => {
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

      logout: () => {
        localStorage.removeItem('token');
        set({
          token: null,
          user: null,
          isAuthenticated: false,
        });
      },
    }),
    {
      name: 'auth-storage',
    }
  )
);
