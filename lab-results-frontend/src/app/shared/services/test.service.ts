import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { EnvironmentService } from './environment.service';
import {
    Test,
    TestTemplate,
    TestResult,
    SaveTestResultRequest,
    TestCalculationRequest,
    TestCalculationResult
} from '../models/test.model';

@Injectable({
    providedIn: 'root'
})
export class TestService {
    private http = inject(HttpClient);
    private environment = inject(EnvironmentService);
    private readonly baseUrl = this.environment.getApiEndpoint('tests');

    // Signals for reactive state management
    private _selectedTest = signal<Test | null>(null);
    private _testTemplate = signal<TestTemplate | null>(null);
    private _testResult = signal<TestResult | null>(null);
    private _testResultHistory = signal<TestResult[]>([]);
    private _isLoading = signal(false);
    private _error = signal<string | null>(null);

    // Public readonly signals
    readonly selectedTest = this._selectedTest.asReadonly();
    readonly testTemplate = this._testTemplate.asReadonly();
    readonly testResult = this._testResult.asReadonly();
    readonly testResultHistory = this._testResultHistory.asReadonly();
    readonly isLoading = this._isLoading.asReadonly();
    readonly error = this._error.asReadonly();

    // Computed signals
    readonly hasSelectedTest = computed(() => this._selectedTest() !== null);
    readonly hasTestTemplate = computed(() => this._testTemplate() !== null);
    readonly hasTestResult = computed(() => this._testResult() !== null);
    readonly hasError = computed(() => this._error() !== null);

    /**
     * Get all available tests
     */
    getTests(): Observable<Test[]> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<Test[]>(this.baseUrl).pipe(
            tap(() => this._isLoading.set(false)),
            catchError(error => {
                this._error.set(`Failed to load tests: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get tests that the current user is qualified to perform
     */
    getQualifiedTests(): Observable<Test[]> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<Test[]>(`${this.baseUrl}/qualified`).pipe(
            tap(() => this._isLoading.set(false)),
            catchError(error => {
                this._error.set(`Failed to load qualified tests: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get specific test information
     */
    getTest(testId: number): Observable<Test> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<Test>(`${this.baseUrl}/${testId}`).pipe(
            tap(test => {
                this._selectedTest.set(test);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load test ${testId}: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get test template configuration
     */
    getTestTemplate(testId: number): Observable<TestTemplate> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<TestTemplate>(`${this.baseUrl}/${testId}/template`).pipe(
            tap(template => {
                this._testTemplate.set(template);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load test template for test ${testId}: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get test results for a specific sample and test
     */
    getTestResults(testId: number, sampleId: number): Observable<TestResult> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<TestResult>(`${this.baseUrl}/${testId}/results/${sampleId}`).pipe(
            map(this.mapTestResultDates),
            tap(result => {
                this._testResult.set(result);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load test results: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get historical test results for a sample
     */
    getTestResultsHistory(testId: number, sampleId: number, count: number = 12): Observable<TestResult[]> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<TestResult[]>(`${this.baseUrl}/${testId}/results/${sampleId}/history?count=${count}`).pipe(
            map(results => results.map(this.mapTestResultDates)),
            tap(history => {
                this._testResultHistory.set(history);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load test results history: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Save test results
     */
    saveTestResults(testId: number, request: SaveTestResultRequest): Observable<{ message: string; recordsSaved: number }> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.post<{ message: string; recordsSaved: number }>(`${this.baseUrl}/${testId}/results`, request).pipe(
            tap(() => this._isLoading.set(false)),
            catchError(error => {
                this._error.set(`Failed to save test results: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Update test results
     */
    updateTestResults(testId: number, sampleId: number, request: SaveTestResultRequest): Observable<{ message: string; recordsUpdated: number }> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.put<{ message: string; recordsUpdated: number }>(`${this.baseUrl}/${testId}/results/${sampleId}`, request).pipe(
            tap(() => this._isLoading.set(false)),
            catchError(error => {
                this._error.set(`Failed to update test results: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Delete test results
     */
    deleteTestResults(testId: number, sampleId: number): Observable<{ message: string; recordsDeleted: number }> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.delete<{ message: string; recordsDeleted: number }>(`${this.baseUrl}/${testId}/results/${sampleId}`).pipe(
            tap(() => {
                this._testResult.set(null);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to delete test results: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Calculate test result based on input values
     */
    calculateTestResult(testId: number, request: TestCalculationRequest): Observable<TestCalculationResult> {
        return this.http.post<TestCalculationResult>(`${this.baseUrl}/${testId}/calculate`, request).pipe(
            catchError(error => {
                this._error.set(`Failed to calculate test result: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Calculate NAS values from particle counts
     */
    calculateNAS(request: { particleCounts: { [channel: number]: number } }): Observable<{ highestNAS: number; channelNASValues: { [channel: number]: number }; isValid: boolean; errorMessage?: string }> {
        return this.http.post<{ highestNAS: number; channelNASValues: { [channel: number]: number }; isValid: boolean; errorMessage?: string }>(this.environment.getApiEndpoint('lookups/nas/calculate'), request).pipe(
            catchError(error => {
                this._error.set(`Failed to calculate NAS: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Get NLGI grade for penetration value
     */
    getNLGIForPenetration(penetrationValue: number): Observable<{ penetrationValue: number; nlgi: string }> {
        return this.http.get<{ penetrationValue: number; nlgi: string }>(this.environment.getApiEndpoint(`lookups/nlgi/penetration/${penetrationValue}`)).pipe(
            catchError(error => {
                this._error.set(`Failed to get NLGI for penetration value: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Get particle type definitions for particle analysis
     */
    getParticleTypes(): Observable<any[]> {
        return this.http.get<any[]>(this.environment.getApiEndpoint('particle-analysis/particle-types')).pipe(
            catchError(error => {
                this._error.set(`Failed to get particle types: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Get particle sub-type categories with their definitions
     */
    getSubTypeCategories(): Observable<any[]> {
        return this.http.get<any[]>(this.environment.getApiEndpoint('particle-analysis/sub-type-categories')).pipe(
            catchError(error => {
                this._error.set(`Failed to get sub-type categories: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Get Inspect Filter results for a sample
     */
    getInspectFilterResults(sampleId: number, testId: number): Observable<any> {
        return this.http.get<any>(this.environment.getApiEndpoint(`particle-analysis/inspect-filter/${sampleId}/${testId}`)).pipe(
            catchError(error => {
                this._error.set(`Failed to get Inspect Filter results: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Save Inspect Filter results
     */
    saveInspectFilterResults(request: any): Observable<{ message: string; recordsSaved: number }> {
        return this.http.post<{ message: string; recordsSaved: number }>(this.environment.getApiEndpoint('particle-analysis/inspect-filter'), request).pipe(
            catchError(error => {
                this._error.set(`Failed to save Inspect Filter results: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Delete Inspect Filter results
     */
    deleteInspectFilterResults(sampleId: number, testId: number): Observable<{ message: string; recordsDeleted: number }> {
        return this.http.delete<{ message: string; recordsDeleted: number }>(this.environment.getApiEndpoint(`particle-analysis/inspect-filter/${sampleId}/${testId}`)).pipe(
            catchError(error => {
                this._error.set(`Failed to delete Inspect Filter results: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Get Ferrography results for a sample
     */
    getFerrographyResults(sampleId: number, testId: number): Observable<any> {
        return this.http.get<any>(this.environment.getApiEndpoint(`particle-analysis/ferrography/${sampleId}/${testId}`)).pipe(
            catchError(error => {
                this._error.set(`Failed to get Ferrography results: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Save Ferrography results
     */
    saveFerrographyResults(request: any): Observable<{ message: string; recordsSaved: number }> {
        return this.http.post<{ message: string; recordsSaved: number }>(this.environment.getApiEndpoint('particle-analysis/ferrography'), request).pipe(
            catchError(error => {
                this._error.set(`Failed to save Ferrography results: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Save partial Ferrography results (dilution factor only)
     */
    savePartialFerrographyResults(request: any): Observable<{ message: string; recordsSaved: number }> {
        return this.http.post<{ message: string; recordsSaved: number }>(this.environment.getApiEndpoint('particle-analysis/ferrography/partial'), request).pipe(
            catchError(error => {
                this._error.set(`Failed to save partial Ferrography results: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Delete Ferrography results
     */
    deleteFerrographyResults(sampleId: number, testId: number): Observable<{ message: string; recordsDeleted: number }> {
        return this.http.delete<{ message: string; recordsDeleted: number }>(this.environment.getApiEndpoint(`particle-analysis/ferrography/${sampleId}/${testId}`)).pipe(
            catchError(error => {
                this._error.set(`Failed to delete Ferrography results: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Select a test for entry
     */
    selectTest(test: Test | null): void {
        this._selectedTest.set(test);
        if (test) {
            this.getTestTemplate(test.testId).subscribe();
        } else {
            this._testTemplate.set(null);
        }
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
        this._selectedTest.set(null);
        this._testTemplate.set(null);
        this._testResult.set(null);
        this._testResultHistory.set([]);
        this._error.set(null);
    }

    /**
     * Map test result dates from string to Date objects
     */
    private mapTestResultDates(result: any): TestResult {
        return {
            ...result,
            entryDate: result.entryDate ? new Date(result.entryDate) : undefined
        };
    }
}