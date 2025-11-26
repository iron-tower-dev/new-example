import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EnvironmentService } from './environment.service';
import { Observable } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import {
    EmissionSpectroscopyData,
    EmissionSpectroscopyCreateRequest,
    EmissionSpectroscopyUpdateRequest
} from '../models/test.model';

@Injectable({
    providedIn: 'root'
})
export class EmissionSpectroscopyService {
    private http = inject(HttpClient);
    private environment = inject(EnvironmentService);
    private readonly baseUrl = this.environment.getApiEndpoint('emission-spectroscopy');

    // Signals for reactive state management
    private _emissionSpectroscopyData = signal<EmissionSpectroscopyData[]>([]);
    private _isLoading = signal(false);
    private _error = signal<string | null>(null);

    // Public readonly signals
    readonly emissionSpectroscopyData = this._emissionSpectroscopyData.asReadonly();
    readonly isLoading = this._isLoading.asReadonly();
    readonly error = this._error.asReadonly();

    // Computed signals
    readonly hasData = computed(() => this._emissionSpectroscopyData().length > 0);
    readonly hasError = computed(() => this._error() !== null);

    /**
     * Get emission spectroscopy data for a sample and test
     */
    getEmissionSpectroscopyData(sampleId: number, testId: number): Observable<EmissionSpectroscopyData[]> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<EmissionSpectroscopyData[]>(`${this.baseUrl}/${sampleId}/${testId}`).pipe(
            map(data => data.map(this.mapDates)),
            tap(data => {
                this._emissionSpectroscopyData.set(data);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load emission spectroscopy data: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Create new emission spectroscopy data
     */
    createEmissionSpectroscopyData(request: EmissionSpectroscopyCreateRequest): Observable<EmissionSpectroscopyData> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.post<EmissionSpectroscopyData>(this.baseUrl, request).pipe(
            map(this.mapDates),
            tap(data => {
                // Add to existing data
                const currentData = this._emissionSpectroscopyData();
                this._emissionSpectroscopyData.set([...currentData, data]);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to create emission spectroscopy data: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Update emission spectroscopy data
     */
    updateEmissionSpectroscopyData(
        sampleId: number,
        testId: number,
        trialNum: number,
        request: EmissionSpectroscopyUpdateRequest
    ): Observable<EmissionSpectroscopyData> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.put<EmissionSpectroscopyData>(`${this.baseUrl}/${sampleId}/${testId}/${trialNum}`, request).pipe(
            map(this.mapDates),
            tap(updatedData => {
                // Update existing data
                const currentData = this._emissionSpectroscopyData();
                const updatedArray = currentData.map(item =>
                    item.id === sampleId && item.testId === testId && item.trialNum === trialNum
                        ? updatedData
                        : item
                );
                this._emissionSpectroscopyData.set(updatedArray);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to update emission spectroscopy data: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Delete emission spectroscopy data
     */
    deleteEmissionSpectroscopyData(sampleId: number, testId: number, trialNum: number): Observable<void> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.delete<void>(`${this.baseUrl}/${sampleId}/${testId}/${trialNum}`).pipe(
            tap(() => {
                // Remove from existing data
                const currentData = this._emissionSpectroscopyData();
                const filteredData = currentData.filter(item =>
                    !(item.id === sampleId && item.testId === testId && item.trialNum === trialNum)
                );
                this._emissionSpectroscopyData.set(filteredData);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to delete emission spectroscopy data: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Schedule Ferrography test for a sample
     */
    scheduleFerrography(sampleId: number): Observable<{ message: string; sampleId: number }> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.post<{ message: string; sampleId: number }>(`${this.baseUrl}/${sampleId}/schedule-ferrography`, {}).pipe(
            tap(() => this._isLoading.set(false)),
            catchError(error => {
                this._error.set(`Failed to schedule Ferrography: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Clear any error state
     */
    clearError(): void {
        this._error.set(null);
    }

    /**
     * Clear all data
     */
    clearData(): void {
        this._emissionSpectroscopyData.set([]);
        this._error.set(null);
    }

    /**
     * Get data for a specific trial
     */
    getTrialData(trialNum: number): EmissionSpectroscopyData | undefined {
        return this._emissionSpectroscopyData().find(data => data.trialNum === trialNum);
    }

    /**
     * Check if data exists for a specific trial
     */
    hasTrialData(trialNum: number): boolean {
        return this._emissionSpectroscopyData().some(data => data.trialNum === trialNum);
    }

    /**
     * Map date strings to Date objects
     */
    private mapDates(data: any): EmissionSpectroscopyData {
        return {
            ...data,
            trialDate: data.trialDate ? new Date(data.trialDate) : undefined
        };
    }
}