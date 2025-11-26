import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, throwError, timeout, of } from 'rxjs';
import { EnvironmentService } from './environment.service';
import { LoggingService } from './logging.service';
import { map, catchError, tap, finalize } from 'rxjs/operators';
import {
    LoginRequest,
    LoginResponse,
    UserInfo,
    UserQualification,
    TestAccessRequest,
    TestAccessResponse,
    ValidateTokenRequest,
    ValidateTokenResponse,
    AuthState,
    LogoutOptions,
    LogoutReason
} from '../models/auth.model';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly http = inject(HttpClient);
    private readonly router = inject(Router);
    private readonly environment = inject(EnvironmentService);
    private readonly logger = inject(LoggingService);
    private readonly apiUrl = this.environment.getApiEndpoint('auth');
    private readonly tokenKey = 'lab_results_token';
    private readonly userKey = 'lab_results_user';
    private readonly serverLogoutTimeout = 5000; // 5 seconds timeout for server logout

    // Safeguards for concurrent operations
    private isLogoutInProgress = false;
    private logoutPromise: Promise<void> | null = null;
    private tokenCleanupAttempts = 0;
    private readonly maxTokenCleanupAttempts = 3;

    // Signals for reactive state management
    private _authState = signal<AuthState>({
        isAuthenticated: false,
        user: null,
        token: null,
        loading: false,
        error: null,
        lastLogoutReason: undefined
    });

    // Public readonly signals
    readonly authState = this._authState.asReadonly();
    readonly isAuthenticated = computed(() => this._authState().isAuthenticated);
    readonly currentUser = computed(() => this._authState().user);
    readonly isLoading = computed(() => this._authState().loading);
    readonly error = computed(() => this._authState().error);
    readonly lastLogoutReason = computed(() => this._authState().lastLogoutReason);

    constructor() {
        this.logger.info('AUTH', 'AuthService initialized');
        this.initializeAuthState();
    }

    private initializeAuthState(): void {
        const token = localStorage.getItem(this.tokenKey);
        const userJson = localStorage.getItem(this.userKey);

        this.logger.debug('AUTH', 'Initializing auth state', {
            hasToken: !!token,
            hasUserData: !!userJson
        });

        if (token && userJson) {
            try {
                const user = JSON.parse(userJson) as UserInfo;

                this.logger.info('AUTH', 'Found stored credentials, restoring session', {
                    userId: user.employeeId,
                    role: user.role,
                    qualificationCount: user.qualifications?.length || 0
                });

                // TEMPORARILY SKIP TOKEN VALIDATION - ASSUME VALID IF EXISTS
                // Batch update for better performance
                this._authState.update(state => ({
                    ...state,
                    isAuthenticated: true,
                    user,
                    token,
                    loading: false,
                    error: null
                }));

                this.logger.logAuthStateChange('unauthenticated', 'authenticated', 'session-restored', user.employeeId);

                // TODO: Re-enable token validation later
                // this.validateStoredToken(token).subscribe({
                //     next: (isValid) => {
                //         if (isValid) {
                //             this._authState.update(state => ({
                //                 ...state,
                //                 isAuthenticated: true,
                //                 user,
                //                 token
                //             }));
                //             this.logger.logAuthStateChange('unauthenticated', 'authenticated', 'token-validated', user.employeeId);
                //         } else {
                //             this.logger.logAuthError('token-validation-failed', 'Stored token is invalid', user.employeeId);
                //             this.clearAuthState(true, 'token-invalid');
                //         }
                //     },
                //     error: (error) => {
                //         this.logger.logAuthError('token-validation-error', error, user.employeeId);
                //         this.clearAuthState(true, 'error');
                //     }
                // });
            } catch (error) {
                this.logger.logAuthError('session-restore-failed', error);
                this.clearAuthState(true, 'error');
            }
        } else {
            this.logger.debug('AUTH', 'No stored credentials found, starting unauthenticated');
        }
    }

    login(credentials: LoginRequest): Observable<LoginResponse> {
        this.logger.logAuthEvent('login-attempt', { employeeId: credentials.employeeId });
        this._authState.update(state => ({ ...state, loading: true, error: null }));

        return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
            tap(response => {
                this.logger.logAuthEvent('login-success', {
                    employeeId: response.employeeId,
                    role: response.role,
                    qualificationCount: response.qualifications?.length || 0
                }, response.employeeId);

                // Store token and user info
                try {
                    localStorage.setItem(this.tokenKey, response.token);
                    localStorage.setItem(this.userKey, JSON.stringify({
                        employeeId: response.employeeId,
                        fullName: response.fullName,
                        role: response.role,
                        qualifications: response.qualifications
                    }));

                    this.logger.debug('AUTH', 'Credentials stored successfully', undefined, response.employeeId);
                } catch (error) {
                    this.logger.error('AUTH', 'Failed to store credentials in localStorage', error, response.employeeId);
                    throw new Error('Failed to store authentication data');
                }

                // Update auth state in a single batch operation for better performance
                const newUser: UserInfo = {
                    employeeId: response.employeeId,
                    fullName: response.fullName,
                    role: response.role,
                    qualifications: response.qualifications
                };

                this._authState.update(state => ({
                    ...state,
                    isAuthenticated: true,
                    user: newUser,
                    token: response.token,
                    loading: false,
                    error: null,
                    lastLogoutReason: undefined  // Clear any previous logout reason on successful login
                }));

                this.logger.logAuthStateChange('unauthenticated', 'authenticated', 'login-success', response.employeeId);
            }),
            catchError(error => {
                this.logger.logAuthError('login-failed', error, credentials.employeeId);

                const errorMessage = this.extractErrorMessage(error);
                this._authState.update(state => ({
                    ...state,
                    loading: false,
                    error: errorMessage
                }));

                return throwError(() => error);
            })
        );
    }

    logout(options: LogoutOptions = {}): Promise<void> {
        const { silent = false, reason = 'user', skipRedirect = false } = options;
        const currentUser = this.currentUser();
        const userId = currentUser?.employeeId;

        this.logger.logLogoutEvent(reason, silent, userId);

        // Return existing logout promise if logout is already in progress
        if (this.isLogoutInProgress && this.logoutPromise) {
            this.logger.warn('AUTH', 'Logout already in progress, returning existing promise', undefined, userId);
            return this.logoutPromise;
        }

        // Prevent multiple simultaneous logout operations
        if (this.isLogoutInProgress) {
            this.logger.warn('AUTH', 'Logout already in progress, ignoring duplicate request', undefined, userId);
            return Promise.resolve();
        }

        this.isLogoutInProgress = true;
        this._authState.update(state => ({ ...state, loading: true }));

        // Create and store the logout promise
        this.logoutPromise = this.performLogout(silent, reason, skipRedirect, userId);

        return this.logoutPromise.finally(() => {
            this.isLogoutInProgress = false;
            this.logoutPromise = null;
        });
    }

    private async performLogout(silent: boolean, reason: LogoutReason, skipRedirect: boolean, userId?: string): Promise<void> {
        try {
            // If silent mode, skip server logout and clear state immediately
            if (silent) {
                this.logger.info('AUTH', 'Performing silent logout (skipping server request)', { reason }, userId);
                await this.clearAuthState(!skipRedirect, reason);
                return;
            }

            // For non-silent logout, attempt server logout first
            const token = this.getToken();
            if (token) {
                this.logger.debug('AUTH', 'Attempting server logout', undefined, userId);

                try {
                    await this.performServerLogout().toPromise();
                    this.logger.logServerLogoutResult(true, undefined, userId);
                } catch (error) {
                    this.logger.logServerLogoutResult(false, error, userId);
                    this.logger.warn('AUTH', 'Server logout failed, proceeding with local cleanup', undefined, userId);
                }
            } else {
                this.logger.info('AUTH', 'No token found, performing local logout only', undefined, userId);
            }

            await this.clearAuthState(!skipRedirect, reason);
        } catch (error) {
            this.logger.error('AUTH', 'Error during logout process', error, userId);
            // Ensure state is cleared even if there's an error
            await this.clearAuthState(!skipRedirect, reason);
            throw error;
        }
    }

    private async clearAuthState(redirectToLogin: boolean = true, reason?: LogoutReason): Promise<void> {
        const currentUser = this.currentUser();
        const userId = currentUser?.employeeId;

        this.logger.info('AUTH', 'Clearing authentication state', {
            redirectToLogin,
            reason,
            currentUrl: this.router.url
        }, userId);

        // Ensure complete token removal with retry mechanism
        await this.ensureCompleteTokenRemoval(userId);

        // Update reactive signals in a single batch operation for better performance
        const previousState = this._authState();
        this._authState.update(state => ({
            ...state,
            isAuthenticated: false,
            user: null,
            token: null,
            loading: false,
            error: null,
            lastLogoutReason: reason
        }));

        this.logger.logAuthStateChange(
            previousState.isAuthenticated ? 'authenticated' : 'unauthenticated',
            'unauthenticated',
            reason,
            userId
        );

        // Handle navigation with improved edge case handling
        if (redirectToLogin) {
            await this.handlePostLogoutNavigation(userId);
        }
    }

    private async ensureCompleteTokenRemoval(userId?: string): Promise<void> {
        this.tokenCleanupAttempts = 0;

        while (this.tokenCleanupAttempts < this.maxTokenCleanupAttempts) {
            this.tokenCleanupAttempts++;

            try {
                // Remove from localStorage
                localStorage.removeItem(this.tokenKey);
                localStorage.removeItem(this.userKey);

                // Clear any cached tokens in memory
                // Force garbage collection of any token references
                const currentState = this._authState();
                if (currentState.token) {
                    // Explicitly null out the token reference
                    this._authState.update(state => ({ ...state, token: null }));
                }

                // Verify removal was successful
                const remainingToken = localStorage.getItem(this.tokenKey);
                const remainingUser = localStorage.getItem(this.userKey);

                if (!remainingToken && !remainingUser) {
                    this.logger.debug('AUTH', `Token cleanup successful on attempt ${this.tokenCleanupAttempts}`, undefined, userId);
                    return;
                }

                this.logger.warn('AUTH', `Token cleanup incomplete on attempt ${this.tokenCleanupAttempts}`, {
                    hasToken: !!remainingToken,
                    hasUser: !!remainingUser
                }, userId);

                // Wait before retry
                if (this.tokenCleanupAttempts < this.maxTokenCleanupAttempts) {
                    await new Promise(resolve => setTimeout(resolve, 100));
                }
            } catch (error) {
                this.logger.error('AUTH', `Token cleanup failed on attempt ${this.tokenCleanupAttempts}`, error, userId);

                if (this.tokenCleanupAttempts >= this.maxTokenCleanupAttempts) {
                    throw error;
                }

                // Wait before retry
                await new Promise(resolve => setTimeout(resolve, 100));
            }
        }

        this.logger.error('AUTH', 'Failed to completely remove tokens after maximum attempts', {
            attempts: this.maxTokenCleanupAttempts
        }, userId);
    }

    private performServerLogout(): Observable<void> {
        const userId = this.currentUser()?.employeeId;

        this.logger.debug('AUTH', 'Initiating server logout request', undefined, userId);

        return this.http.post<void>(`${this.apiUrl}/logout`, {}, {
            headers: this.getAuthHeaders()
        }).pipe(
            timeout(this.serverLogoutTimeout),
            tap(() => {
                this.logger.debug('AUTH', 'Server logout request completed successfully', undefined, userId);
            }),
            catchError(error => {
                const errorInfo = this.extractErrorInfo(error);
                this.logger.warn('AUTH', 'Server logout request failed', errorInfo, userId);

                // Don't throw error - we still want to clear local state
                return throwError(() => error);
            })
        );
    }

    getCurrentUser(): Observable<UserInfo> {
        const userId = this.currentUser()?.employeeId;

        this.logger.debug('AUTH', 'Fetching current user info', undefined, userId);

        return this.http.get<UserInfo>(`${this.apiUrl}/me`, {
            headers: this.getAuthHeaders()
        }).pipe(
            tap(user => {
                this.logger.debug('AUTH', 'User info retrieved successfully', {
                    employeeId: user.employeeId,
                    role: user.role
                }, user.employeeId);

                this._authState.update(state => ({ ...state, user }));

                try {
                    localStorage.setItem(this.userKey, JSON.stringify(user));
                } catch (error) {
                    this.logger.error('AUTH', 'Failed to update user info in localStorage', error, user.employeeId);
                }
            }),
            catchError(error => {
                this.logger.logAuthError('get-current-user-failed', error, userId);

                if (error.status === 401) {
                    this.logger.info('AUTH', 'Token invalid during user info fetch, triggering logout', undefined, userId);
                    this.clearAuthState(true, 'token-invalid');
                }

                return throwError(() => error);
            })
        );
    }

    checkTestAccess(testStandId: number, requiredLevel: string): Observable<TestAccessResponse> {
        const currentUser = this.currentUser();
        const userId = currentUser?.employeeId;

        if (!currentUser) {
            this.logger.warn('AUTH', 'Test access check attempted without authenticated user', {
                testStandId,
                requiredLevel
            });
            return throwError(() => new Error('User not authenticated'));
        }

        const request: TestAccessRequest = {
            employeeId: currentUser.employeeId,
            testStandId,
            requiredLevel
        };

        this.logger.debug('AUTH', 'Checking test access', {
            testStandId,
            requiredLevel
        }, userId);

        return this.http.post<TestAccessResponse>(`${this.apiUrl}/check-test-access`, request, {
            headers: this.getAuthHeaders()
        }).pipe(
            tap(response => {
                this.logger.debug('AUTH', 'Test access check completed', {
                    testStandId,
                    requiredLevel,
                    hasAccess: response.hasAccess
                }, userId);
            }),
            catchError(error => {
                this.logger.logAuthError('test-access-check-failed', error, userId);

                if (error.status === 401) {
                    this.logger.info('AUTH', 'Token invalid during test access check', undefined, userId);
                    this.clearAuthState(true, 'token-invalid');
                }

                return throwError(() => error);
            })
        );
    }

    getUserQualifications(): Observable<UserQualification[]> {
        const userId = this.currentUser()?.employeeId;

        this.logger.debug('AUTH', 'Fetching user qualifications', undefined, userId);

        return this.http.get<UserQualification[]>(`${this.apiUrl}/qualifications`, {
            headers: this.getAuthHeaders()
        }).pipe(
            tap(qualifications => {
                this.logger.debug('AUTH', 'User qualifications retrieved', {
                    count: qualifications.length
                }, userId);
            }),
            catchError(error => {
                this.logger.logAuthError('get-qualifications-failed', error, userId);

                if (error.status === 401) {
                    this.logger.info('AUTH', 'Token invalid during qualifications fetch', undefined, userId);
                    this.clearAuthState(true, 'token-invalid');
                }

                return throwError(() => error);
            })
        );
    }

    private validateStoredToken(token: string): Observable<boolean> {
        const request: ValidateTokenRequest = { token };
        return this.http.post<ValidateTokenResponse>(`${this.apiUrl}/validate-token`, request).pipe(
            map(response => response.isValid),
            catchError(() => {
                // If validation fails, consider token invalid
                return [false];
            })
        );
    }

    getToken(): string | null {
        // Prioritize in-memory token for better performance
        const memoryToken = this._authState().token;
        if (memoryToken) {
            return memoryToken;
        }

        // Fallback to localStorage only if memory token is not available
        try {
            return localStorage.getItem(this.tokenKey);
        } catch (error) {
            this.logger.error('AUTH', 'Failed to retrieve token from localStorage', error);
            return null;
        }
    }

    // Public method to check if logout is in progress
    getLogoutStatus(): boolean {
        return this.isLogoutInProgress;
    }

    // Emergency method to force clear all authentication data
    forceLogout(): Promise<void> {
        this.logger.warn('AUTH', 'Force logout initiated - clearing all authentication data');

        // Reset logout flags
        this.isLogoutInProgress = false;
        this.logoutPromise = null;
        this.tokenCleanupAttempts = 0;

        return this.clearAuthState(true, 'error');
    }

    // Added by SSO migration - clears all authentication tokens
    clearAuthenticationTokens(): void {
        this.logger.info('AUTH', 'Clearing all authentication tokens for SSO migration');

        try {
            // Clear localStorage
            localStorage.removeItem(this.tokenKey);
            localStorage.removeItem(this.userKey);
            localStorage.removeItem('token');
            localStorage.removeItem('refreshToken');
            localStorage.removeItem('user');

            // Clear sessionStorage
            sessionStorage.removeItem(this.tokenKey);
            sessionStorage.removeItem(this.userKey);
            sessionStorage.removeItem('token');
            sessionStorage.removeItem('refreshToken');
            sessionStorage.removeItem('user');

            // Clear in-memory state
            this._authState.update(state => ({
                ...state,
                isAuthenticated: false,
                user: null,
                token: null,
                loading: false,
                error: null,
                lastLogoutReason: 'sso-migration'
            }));

            this.logger.info('AUTH', 'Successfully cleared all authentication tokens');
        } catch (error) {
            this.logger.error('AUTH', 'Error clearing authentication tokens', error);
        }
    }

    private getAuthHeaders(): HttpHeaders {
        const token = this.getToken();
        return new HttpHeaders({
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        });
    }

    // Helper method to check if user has specific qualification
    hasQualification(testStandId: number, requiredLevel: string): boolean {
        const user = this.currentUser();
        if (!user) return false;

        const qualification = user.qualifications.find(q => q.testStandId === testStandId);
        if (!qualification) return false;

        // Check qualification level hierarchy: MicrE > Q/QAG > TRAIN
        const userLevel = qualification.qualificationLevel;

        switch (requiredLevel) {
            case 'TRAIN':
                return ['TRAIN', 'Q/QAG', 'MicrE'].includes(userLevel || '');
            case 'Q/QAG':
                return ['Q/QAG', 'MicrE'].includes(userLevel || '');
            case 'MicrE':
                return userLevel === 'MicrE';
            default:
                return false;
        }
    }

    // Helper method to get user's qualification level for a specific test
    getUserQualificationLevel(testStandId: number): string | null {
        const user = this.currentUser();
        if (!user) return null;

        const qualification = user.qualifications.find(q => q.testStandId === testStandId);
        return qualification?.qualificationLevel || null;
    }

    // Helper method to check if user is a reviewer
    isReviewer(): boolean {
        return this.currentUser()?.role === 'Reviewer';
    }

    // Helper method to check if user is a technician
    isTechnician(): boolean {
        return this.currentUser()?.role === 'Technician';
    }

    // Private helper methods for error handling and logging

    private async handlePostLogoutNavigation(userId?: string): Promise<void> {
        const currentUrl = this.router.url;
        const publicRoutes = ['/login', '/unauthorized', '/error', '/maintenance'];

        // Check if we're already on a public route
        const isOnPublicRoute = publicRoutes.some(route => currentUrl.includes(route));

        if (isOnPublicRoute) {
            this.logger.debug('AUTH', 'Skipping redirect - already on public page', {
                currentUrl
            }, userId);
            return;
        }

        // Handle edge cases for navigation
        try {
            this.logger.info('AUTH', 'Redirecting to login page after logout', {
                fromUrl: currentUrl
            }, userId);

            // Use replaceUrl to prevent back navigation to authenticated routes
            const navigationSuccess = await this.router.navigate(['/login'], {
                replaceUrl: true,
                queryParams: { returnUrl: currentUrl !== '/' ? currentUrl : null }
            });

            if (!navigationSuccess) {
                this.logger.warn('AUTH', 'Navigation to login page was blocked or failed', {
                    fromUrl: currentUrl
                }, userId);

                // Fallback: try to navigate to root and then login
                await this.router.navigate(['/'], { replaceUrl: true });
                setTimeout(() => {
                    this.router.navigate(['/login'], { replaceUrl: true });
                }, 100);
            }
        } catch (error) {
            this.logger.error('AUTH', 'Failed to navigate to login page', error, userId);

            // Last resort: force page reload to login
            try {
                window.location.href = '/login';
            } catch (reloadError) {
                this.logger.error('AUTH', 'Failed to force navigation via window.location', reloadError, userId);
            }
        }
    }

    private extractErrorMessage(error: any): string {
        if (error?.error?.message) {
            return error.error.message;
        }

        if (error?.message) {
            return error.message;
        }

        if (error?.status) {
            switch (error.status) {
                case 401:
                    return 'Invalid credentials';
                case 403:
                    return 'Access denied';
                case 404:
                    return 'Service not found';
                case 500:
                    return 'Server error occurred';
                default:
                    return `Request failed (${error.status})`;
            }
        }

        return 'An unexpected error occurred';
    }

    private extractErrorInfo(error: any): any {
        return {
            status: error?.status,
            statusText: error?.statusText,
            message: error?.message || error?.error?.message,
            url: error?.url,
            name: error?.name,
            timestamp: new Date().toISOString()
        };
    }
}