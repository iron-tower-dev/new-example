import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, interval, switchMap, takeWhile } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface MigrationStatus {
    migrationId: string;
    status: string;
    startTime: Date;
    endTime?: Date;
    progressPercentage: number;
    duration?: string;
    statistics: MigrationStatistics;
    recentErrors: MigrationError[];
    currentOperation?: string;
    estimatedTimeRemaining: string;
}

export interface MigrationStatistics {
    totalTables: number;
    tablesProcessed: number;
    totalRecords: number;
    recordsProcessed: number;
    errorCount: number;
    progressPercentage: number;
}

export interface MigrationError {
    timestamp: Date;
    level: string;
    component: string;
    message: string;
    details?: string;
    tableName?: string;
    recordNumber?: number;
}

export interface MigrationProgress {
    migrationId: string;
    progressPercentage: number;
    currentOperation: string;
    statistics: MigrationStatistics;
    estimatedTimeRemaining: string;
    startTime: Date;
    elapsedTime: string;
    status: string;
}

export interface MigrationOptions {
    clearExistingData: boolean;
    createMissingTables: boolean;
    validateAgainstLegacy: boolean;
    removeAuthentication: boolean;
    includeTables: string[];
    excludeTables: string[];
    seedingOptions: SeedingOptions;
    validationOptions: ValidationOptions;
    authRemovalOptions: AuthRemovalOptions;
    maxConcurrentOperations: number;
    operationTimeoutMinutes: number;
}

export interface SeedingOptions {
    clearExistingData: boolean;
    createMissingTables: boolean;
    batchSize: number;
    continueOnError: boolean;
    validateBeforeInsert: boolean;
    useTransactions: boolean;
    commandTimeoutMinutes: number;
}

export interface ValidationOptions {
    compareQueryResults: boolean;
    comparePerformance: boolean;
    generateDetailedReports: boolean;
    maxDiscrepanciesToReport: number;
    performanceThresholdPercent: number;
    queryTimeoutMinutes: number;
    legacyConnectionString: string;
    ignoreMinorDifferences: boolean;
}

export interface AuthRemovalOptions {
    createBackup: boolean;
    backupDirectory: string;
    removeFromApi: boolean;
    removeFromFrontend: boolean;
    updateDocumentation: boolean;
    filesToExclude: string[];
}

export interface MigrationReport {
    migrationId: string;
    generatedAt: Date;
    status: string;
    duration: string;
    summary: MigrationSummary;
    seedingReport?: SeedingReport;
    validationReport?: ValidationReport;
    authRemovalReport?: AuthRemovalReport;
    errors: MigrationError[];
    recommendations: string[];
}

export interface MigrationSummary {
    totalTables: number;
    tablesProcessed: number;
    totalRecords: number;
    recordsProcessed: number;
    errorCount: number;
    success: boolean;
    overallProgressPercentage: number;
}

export interface SeedingReport {
    tablesProcessed: number;
    tablesCreated: number;
    recordsInserted: number;
    recordsSkipped: number;
    duration: string;
    success: boolean;
    tableReports: TableSeedingReport[];
}

export interface TableSeedingReport {
    tableName: string;
    success: boolean;
    recordsProcessed: number;
    recordsInserted: number;
    recordsSkipped: number;
    duration: string;
    errors: string[];
    tableCreated: boolean;
}

export interface ValidationReport {
    queriesValidated: number;
    queriesMatched: number;
    queriesFailed: number;
    matchPercentage: number;
    duration: string;
    success: boolean;
    queryReports: QueryComparisonReport[];
    summary: ValidationSummary;
}

export interface QueryComparisonReport {
    queryName: string;
    dataMatches: boolean;
    currentRowCount: number;
    legacyRowCount: number;
    discrepancyCount: number;
    currentExecutionTime: string;
    legacyExecutionTime: string;
    performanceRatio: number;
    error?: string;
}

export interface ValidationSummary {
    matchPercentage: number;
    totalDiscrepancies: number;
    averageCurrentExecutionTime: string;
    averageLegacyExecutionTime: string;
    criticalIssues: string[];
}

export interface AuthRemovalReport {
    success: boolean;
    removedComponentsCount: number;
    modifiedFilesCount: number;
    backupFilesCount: number;
    backupLocation: string;
    duration: string;
    errors: string[];
}

export interface MigrationStatisticsSummary {
    totalMigrations: number;
    successfulMigrations: number;
    failedMigrations: number;
    cancelledMigrations: number;
    averageDuration: string;
    totalRecordsProcessed: number;
    totalTablesProcessed: number;
    periodDays: number;
    successRate: number;
}

@Injectable({
    providedIn: 'root'
})
export class MigrationService {
    private readonly apiUrl = `${environment.apiUrl}/api/migration`;
    private currentStatusSubject = new BehaviorSubject<MigrationStatus | null>(null);
    private progressSubject = new BehaviorSubject<MigrationProgress | null>(null);
    private isPollingSubject = new BehaviorSubject<boolean>(false);

    public currentStatus$ = this.currentStatusSubject.asObservable();
    public progress$ = this.progressSubject.asObservable();
    public isPolling$ = this.isPollingSubject.asObservable();

    constructor(private http: HttpClient) { }

    startMigration(options: MigrationOptions): Observable<MigrationStatus> {
        return this.http.post<MigrationStatus>(`${this.apiUrl}/start`, { options });
    }

    getMigrationStatus(): Observable<MigrationStatus> {
        return this.http.get<MigrationStatus>(`${this.apiUrl}/status`);
    }

    getMigrationProgress(): Observable<MigrationProgress> {
        return this.http.get<MigrationProgress>(`${this.apiUrl}/progress`);
    }

    cancelMigration(): Observable<any> {
        return this.http.post(`${this.apiUrl}/cancel`, {});
    }

    pauseMigration(): Observable<any> {
        return this.http.post(`${this.apiUrl}/pause`, {});
    }

    resumeMigration(): Observable<any> {
        return this.http.post(`${this.apiUrl}/resume`, {});
    }

    getMigrationResult(migrationId: string): Observable<MigrationReport> {
        return this.http.get<MigrationReport>(`${this.apiUrl}/${migrationId}`);
    }

    getMigrationHistory(limit: number = 10): Observable<MigrationStatus[]> {
        const params = new HttpParams().set('limit', limit.toString());
        return this.http.get<MigrationStatus[]>(`${this.apiUrl}/history`, { params });
    }

    isMigrationRunning(): Observable<boolean> {
        return this.http.get<boolean>(`${this.apiUrl}/running`);
    }

    getMigrationStatistics(days: number = 30): Observable<MigrationStatisticsSummary> {
        const params = new HttpParams().set('days', days.toString());
        return this.http.get<MigrationStatisticsSummary>(`${this.apiUrl}/statistics`, { params });
    }

    downloadMigrationReport(migrationId: string, format: 'json' | 'csv' = 'json'): Observable<Blob> {
        const params = new HttpParams().set('format', format);
        return this.http.get(`${this.apiUrl}/${migrationId}/report`, {
            params,
            responseType: 'blob'
        });
    }

    downloadMigrationLogs(migrationId: string, level: string = 'all'): Observable<Blob> {
        const params = new HttpParams().set('level', level);
        return this.http.get(`${this.apiUrl}/${migrationId}/logs`, {
            params,
            responseType: 'blob'
        });
    }

    // Real-time monitoring methods
    startPolling(): void {
        if (this.isPollingSubject.value) {
            return; // Already polling
        }

        this.isPollingSubject.next(true);

        // Poll every 2 seconds while migration is running
        interval(2000)
            .pipe(
                takeWhile(() => this.isPollingSubject.value),
                switchMap(() => this.getMigrationStatus())
            )
            .subscribe({
                next: (status) => {
                    this.currentStatusSubject.next(status);

                    // If migration is not running, stop polling
                    if (status.status === 'Completed' || status.status === 'Failed' || status.status === 'Cancelled') {
                        this.stopPolling();
                    }
                },
                error: (error) => {
                    console.error('Error polling migration status:', error);
                    // Continue polling even on error
                }
            });

        // Poll progress every 1 second for more frequent updates
        interval(1000)
            .pipe(
                takeWhile(() => this.isPollingSubject.value),
                switchMap(() => this.getMigrationProgress())
            )
            .subscribe({
                next: (progress) => {
                    this.progressSubject.next(progress);
                },
                error: (error) => {
                    // Progress endpoint might not be available if no migration is running
                    // This is expected, so we don't log it as an error
                }
            });
    }

    stopPolling(): void {
        this.isPollingSubject.next(false);
    }

    // Utility methods
    getDefaultMigrationOptions(): MigrationOptions {
        return {
            clearExistingData: true,
            createMissingTables: true,
            validateAgainstLegacy: true,
            removeAuthentication: false,
            includeTables: [],
            excludeTables: [],
            maxConcurrentOperations: 4,
            operationTimeoutMinutes: 30,
            seedingOptions: {
                clearExistingData: true,
                createMissingTables: true,
                batchSize: 1000,
                continueOnError: true,
                validateBeforeInsert: true,
                useTransactions: true,
                commandTimeoutMinutes: 5
            },
            validationOptions: {
                compareQueryResults: true,
                comparePerformance: true,
                generateDetailedReports: true,
                maxDiscrepanciesToReport: 100,
                performanceThresholdPercent: 20.0,
                queryTimeoutMinutes: 2,
                legacyConnectionString: '',
                ignoreMinorDifferences: true
            },
            authRemovalOptions: {
                createBackup: true,
                backupDirectory: 'auth-backup',
                removeFromApi: true,
                removeFromFrontend: true,
                updateDocumentation: true,
                filesToExclude: []
            }
        };
    }

    formatDuration(duration: string): string {
        // Convert duration string to human readable format
        const match = duration.match(/(\d+):(\d+):(\d+)/);
        if (match) {
            const hours = parseInt(match[1]);
            const minutes = parseInt(match[2]);
            const seconds = parseInt(match[3]);

            if (hours > 0) {
                return `${hours}h ${minutes}m ${seconds}s`;
            } else if (minutes > 0) {
                return `${minutes}m ${seconds}s`;
            } else {
                return `${seconds}s`;
            }
        }
        return duration;
    }

    getStatusColor(status: string): string {
        switch (status.toLowerCase()) {
            case 'completed': return 'success';
            case 'failed': return 'danger';
            case 'cancelled': return 'warning';
            case 'inprogress': return 'primary';
            case 'paused': return 'info';
            default: return 'secondary';
        }
    }

    getErrorLevelColor(level: string): string {
        switch (level.toLowerCase()) {
            case 'error': return 'danger';
            case 'warning': return 'warning';
            case 'info': return 'info';
            case 'critical': return 'danger';
            default: return 'secondary';
        }
    }
}