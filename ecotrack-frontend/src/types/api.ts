// API Response Types
export interface User {
  id: string;
  email: string;
  role: string;
  companyId?: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  role: string;
  companyId?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  companyId?: string;
  companyName?: string;
  vatNumber?: string;
}

export interface Company {
  id: string;
  name: string;
  vatNumber: string;
  createdAt: string;
}

export interface EmissionCategory {
  id: string;
  name: string;
  description: string;
  scope: EmissionScope;
  createdAt: string;
}

export type EmissionScope = 1 | 2 | 3;

export const EmissionScopeLabels: Record<EmissionScope, string> = {
  1: 'Scope 1',
  2: 'Scope 2',
  3: 'Scope 3',
};

export interface EmissionEntry {
  id: string;
  companyId: string;
  category: string;
  amount: number;
  co2Equivalent: number;
  reportDate: string;
  rawData: string;
}

export interface CreateEmissionRequest {
  companyId: string;
  categoryId: string;
  amount: number;
  reportedDate: string;
  rawData: string;
}

export interface SustainabilityReport {
  companyId: string;
  totalEmissions: number;
  scope1: number;
  scope2: number;
  scope3: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
}

export interface ApiError {
  error: string;
  message?: string;
}

export interface LinkCompanyRequest {
  companyId: string;
}
