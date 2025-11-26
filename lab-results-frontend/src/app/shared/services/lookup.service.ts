import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError, of } from 'rxjs';
import { catchError, tap, shareReplay, map } from 'rxjs/operators';
import { EnvironmentService } from './environment.service';

// Interfaces for lookup data
export interface NasLookupRequest {
    particleCounts: { [channel: number]: number };
}

export interface NasLookupResult {
    isValid: boolean;
    highestNAS: number;
    channelNASValues: { [channel: number]: number };
    errorMessage?: string;
}

export interface NasLookupDto {
    channel: number;
    valLo: number;
    valHi: number;
    nas: number;
}

export interface NlgiLookupDto {
    lowerValue: number;
    upperValue: number;
    nlgiValue: string;
}

export interface EquipmentSelectionDto {
    id: number;
    equipName: string;
    equipType: string;
    dueDate?: Date;
    isDueSoon: boolean;
    isOverdue: boolean;
    calibrationValue?: number;
    displayText: string;
}

export interface EquipmentCalibrationDto {
    equipmentId: number;
    equipName: string;
    equipType: string;
    calibrationValue?: number;
    dueDate?: Date;
    isValid: boolean;
    validationMessage?: string;
}

export interface ParticleTypeDefinitionDto {
    id: number;
    type: string;
    description: string;
    image1: string;
    image2: string;
    active?: string;
    sortOrder?: number;
}

export interface ParticleSubTypeDefinitionDto {
    particleSubTypeCategoryId: number;
    value: number;
    description: string;
    active?: string;
    sortOrder?: number;
}

export interface ParticleSubTypeCategoryDefinitionDto {
    id: number;
    description: string;
    active?: string;
    sortOrder?: number;
}

export interface CommentDto {
    id: number;
    area: string;
    type?: string;
    remark: string;
}

export interface CacheStatusDto {
    cacheEntries: { [key: string]: CacheInfo };
}

export interface CacheInfo {
    isLoaded: boolean;
    lastRefreshed?: Date;
    itemCount: number;
    expiresIn?: string;
}

@Injectable({
    providedIn: 'root'
})
export class LookupService {
    private http = inject(HttpClient);
    private environment = inject(EnvironmentService);
    private readonly baseUrl = this.environment.getApiEndpoint('lookups');

    // Loading states using signals
    private _isLoading = signal(false);
    private _error = signal<string | null>(null);
    private _cacheStatus = signal<CacheStatusDto | null>(null);

    // Public readonly signals
    readonly isLoading = this._isLoading.asReadonly();
    readonly error = this._error.asReadonly();
    readonly cacheStatus = this._cacheStatus.asReadonly();
    readonly hasError = computed(() => this._error() !== null);

    // Cache for frequently accessed data
    private equipmentCache = new Map<string, Observable<EquipmentSelectionDto[]>>();
    private particleTypesCache: Observable<ParticleTypeDefinitionDto[]> | null = null;
    private commentAreasCache: Observable<string[]> | null = null;

    // NAS Lookup Methods
    calculateNAS(request: NasLookupRequest): Observable<NasLookupResult> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.post<NasLookupResult>(`${this.baseUrl}/nas/calculate`, request).pipe(
            tap(() => this._isLoading.set(false)),
            catchError(error => {
                this._isLoading.set(false);
                this._error.set(error.error?.message || 'Error calculating NAS values');
                return throwError(() => error);
            })
        );
    }

    getNASLookupTable(): Observable<NasLookupDto[]> {
        return this.http.get<NasLookupDto[]>(`${this.baseUrl}/nas`).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error retrieving NAS lookup table');
                return throwError(() => error);
            })
        );
    }

    getNASForParticleCount(channel: number, count: number): Observable<{ channel: number; count: number; nas: number }> {
        return this.http.get<{ channel: number; count: number; nas: number }>(`${this.baseUrl}/nas/channel/${channel}/count/${count}`).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error retrieving NAS value');
                return throwError(() => error);
            })
        );
    }

    // NLGI Lookup Methods
    getNLGIForPenetration(penetrationValue: number): Observable<{ penetrationValue: number; nlgi: string }> {
        return this.http.get<{ penetrationValue: number; nlgi: string }>(`${this.baseUrl}/nlgi/penetration/${penetrationValue}`).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error retrieving NLGI value');
                return throwError(() => error);
            })
        );
    }

    getNLGILookupTable(): Observable<NlgiLookupDto[]> {
        return this.http.get<NlgiLookupDto[]>(`${this.baseUrl}/nlgi`).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error retrieving NLGI lookup table');
                return throwError(() => error);
            })
        );
    }

    // Equipment Lookup Methods with Caching
    getCachedEquipmentByType(equipType: string, testId?: number): Observable<EquipmentSelectionDto[]> {
        const cacheKey = `${equipType}_${testId || 0}`;

        if (this.equipmentCache.has(cacheKey)) {
            return this.equipmentCache.get(cacheKey)!;
        }

        this._isLoading.set(true);
        this._error.set(null);

        const url = testId
            ? `${this.baseUrl}/equipment/type/${equipType}/test/${testId}`
            : `${this.baseUrl}/equipment/type/${equipType}`;

        const request = this.http.get<EquipmentSelectionDto[]>(url).pipe(
            tap(() => this._isLoading.set(false)),
            catchError(error => {
                this._isLoading.set(false);
                this._error.set(error.error?.message || 'Error retrieving equipment');
                return throwError(() => error);
            }),
            shareReplay(1)
        );

        this.equipmentCache.set(cacheKey, request);
        return request;
    }

    getCachedEquipmentCalibration(equipmentId: number): Observable<EquipmentCalibrationDto> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<EquipmentCalibrationDto>(`${this.baseUrl}/equipment/calibration/${equipmentId}`).pipe(
            tap(() => this._isLoading.set(false)),
            catchError(error => {
                this._isLoading.set(false);
                this._error.set(error.error?.message || 'Error retrieving equipment calibration');
                return throwError(() => error);
            })
        );
    }

    // Particle Type Lookup Methods
    getParticleTypeDefinitions(): Observable<ParticleTypeDefinitionDto[]> {
        if (this.particleTypesCache) {
            return this.particleTypesCache;
        }

        this._isLoading.set(true);
        this._error.set(null);

        this.particleTypesCache = this.http.get<ParticleTypeDefinitionDto[]>(`${this.baseUrl}/particle-types`).pipe(
            tap(() => this._isLoading.set(false)),
            catchError(error => {
                this._isLoading.set(false);
                this._error.set(error.error?.message || 'Error retrieving particle types');
                return throwError(() => error);
            }),
            shareReplay(1)
        );

        return this.particleTypesCache;
    }

    getParticleSubTypeDefinitions(categoryId: number): Observable<ParticleSubTypeDefinitionDto[]> {
        return this.http.get<ParticleSubTypeDefinitionDto[]>(`${this.baseUrl}/particle-subtypes/category/${categoryId}`).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error retrieving particle sub-types');
                return throwError(() => error);
            })
        );
    }

    getParticleSubTypeCategories(): Observable<ParticleSubTypeCategoryDefinitionDto[]> {
        return this.http.get<ParticleSubTypeCategoryDefinitionDto[]>(`${this.baseUrl}/particle-categories`).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error retrieving particle categories');
                return throwError(() => error);
            })
        );
    }

    // Comment Lookup Methods
    getCommentsByArea(area: string): Observable<CommentDto[]> {
        return this.http.get<CommentDto[]>(`${this.baseUrl}/comments/area/${area}`).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error retrieving comments');
                return throwError(() => error);
            })
        );
    }

    getCommentsByAreaAndType(area: string, type: string): Observable<CommentDto[]> {
        return this.http.get<CommentDto[]>(`${this.baseUrl}/comments/area/${area}/type/${type}`).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error retrieving comments');
                return throwError(() => error);
            })
        );
    }

    getCommentAreas(): Observable<string[]> {
        if (this.commentAreasCache) {
            return this.commentAreasCache;
        }

        this.commentAreasCache = this.http.get<string[]>(`${this.baseUrl}/comments/areas`).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error retrieving comment areas');
                return throwError(() => error);
            }),
            shareReplay(1)
        );

        return this.commentAreasCache;
    }

    getCommentTypes(area: string): Observable<string[]> {
        return this.http.get<string[]>(`${this.baseUrl}/comments/types/${area}`).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error retrieving comment types');
                return throwError(() => error);
            })
        );
    }

    // Cache Management Methods
    refreshAllCaches(): Observable<{ message: string }> {
        this._isLoading.set(true);
        this._error.set(null);

        // Clear local caches
        this.equipmentCache.clear();
        this.particleTypesCache = null;
        this.commentAreasCache = null;

        return this.http.post<{ message: string }>(`${this.baseUrl}/refresh-all-caches`, {}).pipe(
            tap(() => {
                this._isLoading.set(false);
                this.loadCacheStatus();
            }),
            catchError(error => {
                this._isLoading.set(false);
                this._error.set(error.error?.message || 'Error refreshing caches');
                return throwError(() => error);
            })
        );
    }

    refreshEquipmentCache(): Observable<{ message: string }> {
        this.equipmentCache.clear();

        return this.http.post<{ message: string }>(`${this.baseUrl}/equipment/refresh-cache`, {}).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error refreshing equipment cache');
                return throwError(() => error);
            })
        );
    }

    refreshParticleTypeCache(): Observable<{ message: string }> {
        this.particleTypesCache = null;

        return this.http.post<{ message: string }>(`${this.baseUrl}/particle-types/refresh-cache`, {}).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error refreshing particle type cache');
                return throwError(() => error);
            })
        );
    }

    refreshCommentCache(): Observable<{ message: string }> {
        this.commentAreasCache = null;

        return this.http.post<{ message: string }>(`${this.baseUrl}/comments/refresh-cache`, {}).pipe(
            catchError(error => {
                this._error.set(error.error?.message || 'Error refreshing comment cache');
                return throwError(() => error);
            })
        );
    }

    getCacheStatus(): Observable<CacheStatusDto> {
        return this.http.get<CacheStatusDto>(`${this.baseUrl}/cache-status`).pipe(
            tap(status => this._cacheStatus.set(status)),
            catchError(error => {
                this._error.set(error.error?.message || 'Error retrieving cache status');
                return throwError(() => error);
            })
        );
    }

    // Utility Methods
    clearError(): void {
        this._error.set(null);
    }

    private loadCacheStatus(): void {
        this.getCacheStatus().subscribe();
    }

    // Automatic lookup triggers for field changes
    triggerNASLookup(particleCounts: { [channel: number]: number }): Observable<number> {
        if (!particleCounts || Object.keys(particleCounts).length === 0) {
            return of(0);
        }

        return this.calculateNAS({ particleCounts }).pipe(
            map(result => result.isValid ? result.highestNAS : 0),
            catchError(() => of(0))
        );
    }

    triggerNLGILookup(penetrationValue: number): Observable<string> {
        if (!penetrationValue || penetrationValue <= 0) {
            return of('');
        }

        return this.getNLGIForPenetration(penetrationValue).pipe(
            map(result => result.nlgi),
            catchError(() => of(''))
        );
    }

    triggerEquipmentCalibrationLookup(equipmentId: number): Observable<number | null> {
        if (!equipmentId || equipmentId <= 0) {
            return of(null);
        }

        return this.getCachedEquipmentCalibration(equipmentId).pipe(
            map(result => result.calibrationValue || null),
            catchError(() => of(null))
        );
    }
}