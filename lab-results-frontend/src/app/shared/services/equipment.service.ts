import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, map, catchError, of } from 'rxjs';
import { EnvironmentService } from './environment.service';
import {
    Equipment,
    EquipmentSelection,
    EquipmentCalibration,
    EquipmentValidationResult,
    EquipmentFilter
} from '../models/equipment.model';

@Injectable({
    providedIn: 'root'
})
export class EquipmentService {
    private readonly http = inject(HttpClient);
    private readonly environment = inject(EnvironmentService);
    private readonly baseUrl = this.environment.getApiEndpoint('equipment');

    // Signals for reactive state management
    private readonly _selectedEquipment = signal<Map<string, EquipmentSelection>>(new Map());
    private readonly _equipmentCache = signal<Map<string, EquipmentSelection[]>>(new Map());
    private readonly _isLoading = signal(false);
    private readonly _error = signal<string | null>(null);

    // Public readonly signals
    readonly selectedEquipment = this._selectedEquipment.asReadonly();
    readonly equipmentCache = this._equipmentCache.asReadonly();
    readonly isLoading = this._isLoading.asReadonly();
    readonly error = this._error.asReadonly();

    // Computed signals
    readonly hasSelectedEquipment = computed(() => this._selectedEquipment().size > 0);
    readonly selectedEquipmentList = computed(() => Array.from(this._selectedEquipment().values()));

    // Equipment type specific signals
    private readonly _thermometers = signal<EquipmentSelection[]>([]);
    private readonly _stopwatches = signal<EquipmentSelection[]>([]);
    private readonly _barometers = signal<EquipmentSelection[]>([]);
    private readonly _tubes = signal<EquipmentSelection[]>([]);

    readonly thermometers = this._thermometers.asReadonly();
    readonly stopwatches = this._stopwatches.asReadonly();
    readonly barometers = this._barometers.asReadonly();
    readonly tubes = this._tubes.asReadonly();

    /**
     * Get equipment by type for dropdown selection
     */
    getEquipmentByType(equipType: string, testId?: number, lubeType?: string): Observable<EquipmentSelection[]> {
        this._isLoading.set(true);
        this._error.set(null);

        // Check cache first
        const cacheKey = `${equipType}_${testId || 'all'}_${lubeType || 'all'}`;
        const cached = this._equipmentCache().get(cacheKey);
        if (cached) {
            this._isLoading.set(false);
            return of(cached);
        }

        let params = new HttpParams();
        if (testId) {
            params = params.set('testId', testId.toString());
        }
        if (lubeType) {
            params = params.set('lubeType', lubeType);
        }

        return this.http.get<EquipmentSelection[]>(`${this.baseUrl}/by-type/${equipType}`, { params })
            .pipe(
                map(equipment => {
                    // Convert date strings to Date objects
                    const processedEquipment = equipment.map(item => ({
                        ...item,
                        dueDate: item.dueDate ? new Date(item.dueDate) : undefined
                    }));

                    // Cache the result
                    this._equipmentCache.update(cache => {
                        const newCache = new Map(cache);
                        newCache.set(cacheKey, processedEquipment);
                        return newCache;
                    });

                    this._isLoading.set(false);
                    return processedEquipment;
                }),
                catchError(error => {
                    this._error.set(`Failed to load ${equipType} equipment: ${error.message}`);
                    this._isLoading.set(false);
                    return of([]);
                })
            );
    }

    /**
     * Get equipment calibration information
     */
    getEquipmentCalibration(equipmentId: number): Observable<EquipmentCalibration | null> {
        return this.http.get<EquipmentCalibration>(`${this.baseUrl}/${equipmentId}/calibration`)
            .pipe(
                map(calibration => ({
                    ...calibration,
                    dueDate: calibration.dueDate ? new Date(calibration.dueDate) : undefined
                })),
                catchError(error => {
                    console.error('Failed to get equipment calibration:', error);
                    return of(null);
                })
            );
    }

    /**
     * Validate equipment status and due dates
     */
    validateEquipment(equipmentId: number): Observable<EquipmentValidationResult> {
        return this.http.get<EquipmentValidationResult>(`${this.baseUrl}/${equipmentId}/validate`)
            .pipe(
                map(result => ({
                    ...result,
                    dueDate: result.dueDate ? new Date(result.dueDate) : undefined
                })),
                catchError(error => {
                    console.error('Failed to validate equipment:', error);
                    return of({
                        isValid: false,
                        isDueSoon: false,
                        isOverdue: false,
                        message: 'Validation failed',
                        daysUntilDue: 0
                    });
                })
            );
    }

    /**
     * Get all equipment with optional filtering
     */
    getAllEquipment(filter?: EquipmentFilter): Observable<Equipment[]> {
        let params = new HttpParams();

        if (filter) {
            if (filter.equipType) {
                params = params.set('equipType', filter.equipType);
            }
            if (filter.testId) {
                params = params.set('testId', filter.testId.toString());
            }
            if (filter.lubeType) {
                params = params.set('lubeType', filter.lubeType);
            }
            if (filter.excludeInactive !== undefined) {
                params = params.set('excludeInactive', filter.excludeInactive.toString());
            }
            if (filter.includeOverdue !== undefined) {
                params = params.set('includeOverdue', filter.includeOverdue.toString());
            }
        }

        return this.http.get<Equipment[]>(this.baseUrl, { params })
            .pipe(
                map(equipment => equipment.map(item => ({
                    ...item,
                    dueDate: item.dueDate ? new Date(item.dueDate) : undefined
                }))),
                catchError(error => {
                    console.error('Failed to get all equipment:', error);
                    return of([]);
                })
            );
    }

    /**
     * Get equipment by ID
     */
    getEquipmentById(id: number): Observable<Equipment | null> {
        return this.http.get<Equipment>(`${this.baseUrl}/${id}`)
            .pipe(
                map(equipment => ({
                    ...equipment,
                    dueDate: equipment.dueDate ? new Date(equipment.dueDate) : undefined
                })),
                catchError(error => {
                    console.error('Failed to get equipment by ID:', error);
                    return of(null);
                })
            );
    }

    /**
     * Get available equipment types
     */
    getEquipmentTypes(): Observable<string[]> {
        return this.http.get<string[]>(`${this.baseUrl}/types`)
            .pipe(
                catchError(error => {
                    console.error('Failed to get equipment types:', error);
                    return of([]);
                })
            );
    }

    /**
     * Check if equipment is due soon
     */
    isEquipmentDueSoon(equipmentId: number, warningDays: number = 30): Observable<boolean> {
        const params = new HttpParams().set('warningDays', warningDays.toString());

        return this.http.get<{ equipmentId: number; isDueSoon: boolean; warningDays: number }>
            (`${this.baseUrl}/${equipmentId}/due-soon`, { params })
            .pipe(
                map(result => result.isDueSoon),
                catchError(error => {
                    console.error('Failed to check if equipment is due soon:', error);
                    return of(false);
                })
            );
    }

    /**
     * Select equipment for a specific field/type
     */
    selectEquipment(fieldKey: string, equipment: EquipmentSelection): void {
        this._selectedEquipment.update(selected => {
            const newSelected = new Map(selected);
            newSelected.set(fieldKey, equipment);
            return newSelected;
        });
    }

    /**
     * Clear equipment selection for a specific field
     */
    clearEquipmentSelection(fieldKey: string): void {
        this._selectedEquipment.update(selected => {
            const newSelected = new Map(selected);
            newSelected.delete(fieldKey);
            return newSelected;
        });
    }

    /**
     * Clear all equipment selections
     */
    clearAllSelections(): void {
        this._selectedEquipment.set(new Map());
    }

    /**
     * Get selected equipment for a specific field
     */
    getSelectedEquipment(fieldKey: string): EquipmentSelection | undefined {
        return this._selectedEquipment().get(fieldKey);
    }

    /**
     * Clear equipment cache
     */
    clearCache(): void {
        this._equipmentCache.set(new Map());
    }

    /**
     * Clear error state
     */
    clearError(): void {
        this._error.set(null);
    }

    /**
     * Get thermometers for test entry
     */
    getThermometers(): Observable<EquipmentSelection[]> {
        return this.getEquipmentByType('Thermometer').pipe(
            map(equipment => {
                this._thermometers.set(equipment);
                return equipment;
            })
        );
    }

    /**
     * Get stopwatches for test entry
     */
    getStopwatches(): Observable<EquipmentSelection[]> {
        return this.getEquipmentByType('Stopwatch').pipe(
            map(equipment => {
                this._stopwatches.set(equipment);
                return equipment;
            })
        );
    }

    /**
     * Get barometers for test entry
     */
    getBarometers(): Observable<EquipmentSelection[]> {
        return this.getEquipmentByType('Barometer').pipe(
            map(equipment => {
                this._barometers.set(equipment);
                return equipment;
            })
        );
    }

    /**
     * Get tubes for viscosity tests
     */
    getTubes(): Observable<EquipmentSelection[]> {
        return this.getEquipmentByType('Tube').pipe(
            map(equipment => {
                this._tubes.set(equipment);
                return equipment;
            })
        );
    }
}