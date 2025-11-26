import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { Sample, SampleHistory, SampleFilter } from '../models/sample.model';
import { EnvironmentService } from './environment.service';

@Injectable({
    providedIn: 'root'
})
export class SampleService {
    private http = inject(HttpClient);
    private environment = inject(EnvironmentService);
    private readonly baseUrl = this.environment.getApiEndpoint('samples');

    // Signals for reactive state management
    private _selectedSample = signal<Sample | null>(null);
    private _samples = signal<Sample[]>([]);
    private _sampleHistory = signal<SampleHistory[]>([]);
    private _isLoading = signal(false);
    private _error = signal<string | null>(null);

    // Public readonly signals
    readonly selectedSample = this._selectedSample.asReadonly();
    readonly samples = this._samples.asReadonly();
    readonly sampleHistory = this._sampleHistory.asReadonly();
    readonly isLoading = this._isLoading.asReadonly();
    readonly error = this._error.asReadonly();

    // Computed signals
    readonly hasSelectedSample = computed(() => this._selectedSample() !== null);
    readonly hasError = computed(() => this._error() !== null);

    /**
     * Get samples available for a specific test type
     */
    getSamplesByTest(testId: number): Observable<Sample[]> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<Sample[]>(`${this.baseUrl}/by-test/${testId}`).pipe(
            map(samples => samples.map(this.mapSampleDates)),
            tap(samples => {
                this._samples.set(samples);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load samples for test ${testId}: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get samples with optional filtering
     */
    getSamples(filter?: SampleFilter): Observable<Sample[]> {
        this._isLoading.set(true);
        this._error.set(null);

        let params = new HttpParams();
        if (filter) {
            if (filter.tagNumber) params = params.set('tagNumber', filter.tagNumber);
            if (filter.component) params = params.set('component', filter.component);
            if (filter.location) params = params.set('location', filter.location);
            if (filter.lubeType) params = params.set('lubeType', filter.lubeType);
            if (filter.fromDate) params = params.set('fromDate', filter.fromDate.toISOString());
            if (filter.toDate) params = params.set('toDate', filter.toDate.toISOString());
            if (filter.status !== undefined) params = params.set('status', filter.status.toString());
        }

        return this.http.get<Sample[]>(this.baseUrl, { params }).pipe(
            map(samples => samples.map(this.mapSampleDates)),
            tap(samples => {
                this._samples.set(samples);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load samples: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get detailed information for a specific sample
     */
    getSample(sampleId: number): Observable<Sample> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<Sample>(`${this.baseUrl}/${sampleId}`).pipe(
            map(this.mapSampleDates),
            tap(sample => {
                this._selectedSample.set(sample);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load sample ${sampleId}: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get historical test results for a sample
     */
    getSampleHistory(sampleId: number, testId: number): Observable<SampleHistory[]> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<SampleHistory[]>(`${this.baseUrl}/${sampleId}/history/${testId}`).pipe(
            map(history => history.map(this.mapHistoryDates)),
            tap(history => {
                this._sampleHistory.set(history);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load sample history: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Select a sample for test entry
     */
    selectSample(sample: Sample | null): void {
        this._selectedSample.set(sample);
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
        this._selectedSample.set(null);
        this._samples.set([]);
        this._sampleHistory.set([]);
        this._error.set(null);
    }

    /**
     * Map sample dates from string to Date objects
     */
    private mapSampleDates(sample: any): Sample {
        return {
            ...sample,
            sampleDate: new Date(sample.sampleDate)
        };
    }

    /**
     * Map history dates from string to Date objects
     */
    private mapHistoryDates(history: any): SampleHistory {
        return {
            ...history,
            sampleDate: new Date(history.sampleDate),
            entryDate: history.entryDate ? new Date(history.entryDate) : undefined
        };
    }
}