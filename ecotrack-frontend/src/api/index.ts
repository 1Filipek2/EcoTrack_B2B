import { apiClient } from './client';
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  Company,
  EmissionCategory,
  EmissionEntry,
  CreateEmissionRequest,
  SustainabilityReport,
  PagedResult,
  PaginationParams,
  LinkCompanyRequest,
} from '../types/api';

// Auth API
export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/auth/login', data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<{ message: string }> => {
    const response = await apiClient.post('/auth/register', data);
    return response.data;
  },

  linkCompany: async (data: LinkCompanyRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/auth/link-company', data);
    return response.data;
  },

  deleteMe: async (): Promise<{ message: string }> => {
    const response = await apiClient.delete('/auth/me');
    return response.data;
  },

  verifyEmail: async (email: string, verificationCode: string): Promise<{ message: string }> => {
    const response = await apiClient.post('/auth/verify-email', { email, verificationCode });
    return response.data;
  },
};

// Companies API
export const companiesApi = {
  getAll: async (params?: PaginationParams): Promise<PagedResult<Company>> => {
    const response = await apiClient.get<PagedResult<Company>>('/companies', { params });
    return response.data;
  },

  getById: async (id: string): Promise<Company> => {
    const response = await apiClient.get<Company>(`/companies/${id}`);
    return response.data;
  },

  create: async (data: { name: string; vatNumber: string }): Promise<Company> => {
    const response = await apiClient.post<Company>('/companies', data);
    return response.data;
  },

  getSustainabilityReport: async (
    id: string,
    startDate?: string,
    endDate?: string
  ): Promise<SustainabilityReport> => {
    const response = await apiClient.get<SustainabilityReport>(
      `/companies/${id}/sustainability-report`,
      {
        params: { startDate, endDate },
      }
    );
    return response.data;
  },
};

// Emission Categories API
export const categoriesApi = {
  getAll: async (): Promise<EmissionCategory[]> => {
    const response = await apiClient.get<EmissionCategory[]>('/emissioncategories');
    return response.data;
  },

  getById: async (id: string): Promise<EmissionCategory> => {
    const response = await apiClient.get<EmissionCategory>(`/emissioncategories/${id}`);
    return response.data;
  },

  create: async (data: {
    name: string;
    description: string;
    scope: number;
  }): Promise<EmissionCategory> => {
    const response = await apiClient.post<EmissionCategory>('/emissioncategories', data);
    return response.data;
  },
};

// Emissions API
export const emissionsApi = {
  getAll: async (params?: PaginationParams): Promise<PagedResult<EmissionEntry>> => {
    const response = await apiClient.get<PagedResult<EmissionEntry>>('/emissions', { params });
    return response.data;
  },

  getById: async (id: string): Promise<EmissionEntry> => {
    const response = await apiClient.get<EmissionEntry>(`/emissions/${id}`);
    return response.data;
  },

  create: async (data: CreateEmissionRequest): Promise<EmissionEntry> => {
    const response = await apiClient.post<EmissionEntry>('/emissions', data);
    return response.data;
  },

  processUnstructured: async (data: {
    companyId: string;
    rawText: string;
    reportedDate?: string;
  }): Promise<string> => {
    const response = await apiClient.post<string>('/emissions/process-unstructured', data);
    return response.data;
  },
};
