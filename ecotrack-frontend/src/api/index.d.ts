import type { AuthResponse, LoginRequest, RegisterRequest, Company, EmissionCategory, EmissionEntry, CreateEmissionRequest, SustainabilityReport, PagedResult, PaginationParams, LinkCompanyRequest } from '../types/api';
export declare const authApi: {
    login: (data: LoginRequest) => Promise<AuthResponse>;
    register: (data: RegisterRequest) => Promise<{
        message: string;
    }>;
    linkCompany: (data: LinkCompanyRequest) => Promise<AuthResponse>;
    deleteMe: () => Promise<{
        message: string;
    }>;
    verifyEmail: (email: string, verificationCode: string) => Promise<{
        message: string;
    }>;
};
export declare const companiesApi: {
    getAll: (params?: PaginationParams) => Promise<PagedResult<Company>>;
    getById: (id: string) => Promise<Company>;
    create: (data: {
        name: string;
        vatNumber: string;
    }) => Promise<Company>;
    getSustainabilityReport: (id: string, startDate?: string, endDate?: string) => Promise<SustainabilityReport>;
};
export declare const categoriesApi: {
    getAll: () => Promise<EmissionCategory[]>;
    getById: (id: string) => Promise<EmissionCategory>;
    create: (data: {
        name: string;
        description: string;
        scope: number;
    }) => Promise<EmissionCategory>;
};
export declare const emissionsApi: {
    getAll: (params?: PaginationParams) => Promise<PagedResult<EmissionEntry>>;
    getById: (id: string) => Promise<EmissionEntry>;
    create: (data: CreateEmissionRequest) => Promise<EmissionEntry>;
    processUnstructured: (data: {
        companyId: string;
        rawText: string;
        reportedDate?: string;
    }) => Promise<string>;
};
