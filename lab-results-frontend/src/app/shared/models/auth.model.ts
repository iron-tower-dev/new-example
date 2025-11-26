export interface LoginRequest {
    employeeId: string;
    password: string;
}

export interface LoginResponse {
    token: string;
    employeeId: string;
    fullName: string;
    role: string;
    qualifications: UserQualification[];
    expiresAt: string;
}

export interface UserQualification {
    testStandId?: number;
    testStand?: string;
    qualificationLevel?: string;
}

export interface UserInfo {
    employeeId: string;
    fullName: string;
    role: string;
    qualifications: UserQualification[];
}

export interface TestAccessRequest {
    employeeId: string;
    testStandId: number;
    requiredLevel: string;
}

export interface TestAccessResponse {
    hasAccess: boolean;
    userQualificationLevel?: string;
    requiredLevel: string;
    message?: string;
}

export interface ValidateTokenRequest {
    token: string;
}

export interface ValidateTokenResponse {
    isValid: boolean;
}

export enum QualificationLevel {
    TRAIN = 'TRAIN',
    Q_QAG = 'Q/QAG',
    MicrE = 'MicrE'
}

export interface LogoutOptions {
    silent?: boolean;           // Default: false - skip server-side logout request
    reason?: LogoutReason;      // Default: 'user' - reason for logout
    skipRedirect?: boolean;     // Default: false - skip navigation to login
}

export type LogoutReason = 'user' | 'token-invalid' | 'error' | 'session-expired' | 'sso-migration';

export interface AuthState {
    isAuthenticated: boolean;
    user: UserInfo | null;
    token: string | null;
    loading: boolean;
    error: string | null;
    lastLogoutReason?: LogoutReason;  // For debugging purposes
}