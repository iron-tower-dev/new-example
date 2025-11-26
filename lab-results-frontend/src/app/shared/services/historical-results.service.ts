import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { EnvironmentService } from './environment.service';

export interface HistoricalResultSummary {
    sampleId: number;
    tagNumber: string;
    sampleDate: Date;
    status: string;
    entryDate?: Date;
    testName: string;
}

export interface HistoricalResultsResponse {
    sampleId: number;
    testId: number;
    count: number;
    results: any[];
}

export interface HistoricalResultsSummaryResponse {
    sampleId: number;
    testId: number;
    count: number;
    summary: HistoricalResultSummary[];
}

export interface ExtendedHistoricalResultsResponse {
    sampleId: number;
    testId: number;
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
    results: any[];
}

export interface HistoricalResultsFilter {
    fromDate?: Date;
    toDate?: Date;
    status?: string;
    page?: number;
    pageSize?: number;
}

@Injectable({
    providedIn: 'root'
})
export class HistoricalResultsService {
    private http = inject(HttpClient);
    private environment = inject(EnvironmentService);
    private readonly baseUrl = this.environment.getApiEndpoint('historical-results');

    // Signals for reactive state management
    private _historicalResults = signal<any[]>([]);
    private _historicalSummary = signal<HistoricalResultSummary[]>([]);
    private _extendedResults = signal<ExtendedHistoricalResultsResponse | null>(null);
    private _selectedHistoricalResult = signal<any | null>(null);
    private _isLoading = signal(false);
    private _error = signal<string | null>(null);
    private _currentSampleId = signal<number | null>(null);
    private _currentTestId = signal<number | null>(null);

    // Public readonly signals
    readonly historicalResults = this._historicalResults.asReadonly();
    readonly historicalSummary = this._historicalSummary.asReadonly();
    readonly extendedResults = this._extendedResults.asReadonly();
    readonly selectedHistoricalResult = this._selectedHistoricalResult.asReadonly();
    readonly isLoading = this._isLoading.asReadonly();
    readonly error = this._error.asReadonly();
    readonly currentSampleId = this._currentSampleId.asReadonly();
    readonly currentTestId = this._currentTestId.asReadonly();

    // Computed signals
    readonly hasHistoricalResults = computed(() => this._historicalResults().length > 0);
    readonly hasHistoricalSummary = computed(() => this._historicalSummary().length > 0);
    readonly hasExtendedResults = computed(() => this._extendedResults() !== null);
    readonly hasError = computed(() => this._error() !== null);
    readonly totalPages = computed(() => this._extendedResults()?.totalPages ?? 0);
    readonly currentPage = computed(() => this._extendedResults()?.page ?? 1);

    /**
     * Get historical results for a sample and test (last N results)
     */
    getHistoricalResults(sampleId: number, testId: number, count: number = 12): Observable<HistoricalResultsResponse> {
        this._isLoading.set(true);
        this._error.set(null);
        this._currentSampleId.set(sampleId);
        this._currentTestId.set(testId);

        const params = new HttpParams().set('count', count.toString());

        return this.http.get<HistoricalResultsResponse>(`${this.baseUrl}/samples/${sampleId}/tests/${testId}`, { params }).pipe(
            map(response => ({
                ...response,
                results: response.results.map(this.mapResultDates)
            })),
            tap(response => {
                this._historicalResults.set(response.results);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load historical results: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get historical results summary (lightweight data for display)
     */
    getHistoricalResultsSummary(sampleId: number, testId: number, count: number = 12): Observable<HistoricalResultsSummaryResponse> {
        this._isLoading.set(true);
        this._error.set(null);
        this._currentSampleId.set(sampleId);
        this._currentTestId.set(testId);

        const params = new HttpParams().set('count', count.toString());

        return this.http.get<HistoricalResultsSummaryResponse>(`${this.baseUrl}/samples/${sampleId}/tests/${testId}/summary`, { params }).pipe(
            map(response => ({
                ...response,
                summary: response.summary.map(this.mapSummaryDates)
            })),
            tap(response => {
                this._historicalSummary.set(response.summary);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load historical results summary: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get extended historical results with filtering and pagination
     */
    getExtendedHistoricalResults(sampleId: number, testId: number, filter?: HistoricalResultsFilter): Observable<ExtendedHistoricalResultsResponse> {
        this._isLoading.set(true);
        this._error.set(null);
        this._currentSampleId.set(sampleId);
        this._currentTestId.set(testId);

        let params = new HttpParams();

        if (filter?.fromDate) {
            params = params.set('fromDate', filter.fromDate.toISOString());
        }
        if (filter?.toDate) {
            params = params.set('toDate', filter.toDate.toISOString());
        }
        if (filter?.status) {
            params = params.set('status', filter.status);
        }
        if (filter?.page) {
            params = params.set('page', filter.page.toString());
        }
        if (filter?.pageSize) {
            params = params.set('pageSize', filter.pageSize.toString());
        }

        return this.http.get<ExtendedHistoricalResultsResponse>(`${this.baseUrl}/samples/${sampleId}/tests/${testId}/extended`, { params }).pipe(
            map(response => ({
                ...response,
                results: response.results.map(this.mapResultDates)
            })),
            tap(response => {
                this._extendedResults.set(response);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load extended historical results: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get detailed historical result for a specific sample
     */
    getHistoricalResultDetails(sampleId: number, testId: number, historicalSampleId: number): Observable<any> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<any>(`${this.baseUrl}/samples/${sampleId}/tests/${testId}/details/${historicalSampleId}`).pipe(
            map(this.mapResultDates),
            tap(result => {
                this._selectedHistoricalResult.set(result);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load historical result details: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Select a historical result for detailed view
     */
    selectHistoricalResult(result: any | null): void {
        this._selectedHistoricalResult.set(result);
    }

    /**
     * Clear error state
     */
    clearError(): void {
        this._error.set(null);
    }

    /**
     * Clear all data
     */
    clearData(): void {
        this._historicalResults.set([]);
        this._historicalSummary.set([]);
        this._extendedResults.set(null);
        this._selectedHistoricalResult.set(null);
        this._currentSampleId.set(null);
        this._currentTestId.set(null);
        this._error.set(null);
    }

    /**
     * Refresh current historical data
     */
    refresh(): void {
        const sampleId = this._currentSampleId();
        const testId = this._currentTestId();

        if (sampleId && testId) {
            this.getHistoricalResultsSummary(sampleId, testId).subscribe();
        }
    }

    /**
     * Map result dates from string to Date objects
     */
    private mapResultDates(result: any): any {
        return {
            ...result,
            entryDate: result.entryDate ? new Date(result.entryDate) : undefined
        };
    }

    /**
     * Map summary dates from string to Date objects
     */
    private mapSummaryDates(summary: any): HistoricalResultSummary {
        return {
            ...summary,
            sampleDate: new Date(summary.sampleDate),
            entryDate: summary.entryDate ? new Date(summary.entryDate) : undefined
        };
    }
}