import { Component, Inject, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { HistoricalResultsService, HistoricalResultSummary } from '../../services/historical-results.service';

export interface HistoricalResultDetailsDialogData {
    summary: HistoricalResultSummary;
    sampleId: number;
    testId: number;
    testName: string;
}

@Component({
    selector: 'app-historical-result-details-dialog',
    standalone: true,
    imports: [
        CommonModule,
        MatDialogModule,
        MatButtonModule,
        MatIconModule,
        MatCardModule,
        MatTableModule,
        MatProgressSpinnerModule,
        MatDividerModule,
        MatChipsModule
    ],
    template: `
        <div class="dialog-header">
            <h2 mat-dialog-title>
                <mat-icon>history</mat-icon>
                Historical Result Details
            </h2>
            <button mat-icon-button mat-dialog-close>
                <mat-icon>close</mat-icon>
            </button>
        </div>

        <mat-dialog-content class="dialog-content">
            @if (historicalService.isLoading()) {
                <div class="loading-container">
                    <mat-spinner diameter="40"></mat-spinner>
                    <p>Loading historical result details...</p>
                </div>
            } @else if (historicalService.hasError()) {
                <div class="error-container">
                    <mat-icon color="warn">error</mat-icon>
                    <p>{{ historicalService.error() }}</p>
                    <button mat-button color="primary" (click)="retryLoad()">
                        <mat-icon>refresh</mat-icon>
                        Retry
                    </button>
                </div>
            } @else if (historicalService.selectedHistoricalResult()) {
                <div class="result-details">
                    <!-- Sample Information -->
                    <mat-card class="info-card">
                        <mat-card-header>
                            <mat-card-title>Sample Information</mat-card-title>
                        </mat-card-header>
                        <mat-card-content>
                            <div class="info-grid">
                                <div class="info-item">
                                    <label>Sample ID:</label>
                                    <span>{{ data.summary.sampleId }}</span>
                                </div>
                                <div class="info-item">
                                    <label>Tag Number:</label>
                                    <span>{{ data.summary.tagNumber }}</span>
                                </div>
                                <div class="info-item">
                                    <label>Test Name:</label>
                                    <span>{{ data.testName }}</span>
                                </div>
                                <div class="info-item">
                                    <label>Sample Date:</label>
                                    <span>{{ data.summary.sampleDate | date:'medium' }}</span>
                                </div>
                                <div class="info-item">
                                    <label>Entry Date:</label>
                                    <span>{{ data.summary.entryDate | date:'medium' }}</span>
                                </div>
                                <div class="info-item">
                                    <label>Status:</label>
                                    <mat-chip [class]="'status-chip status-' + data.summary.status.toLowerCase().replace(' ', '-')">
                                        {{ data.summary.status }}
                                    </mat-chip>
                                </div>
                            </div>
                        </mat-card-content>
                    </mat-card>

                    <mat-divider></mat-divider>

                    <!-- Test Results -->
                    <mat-card class="results-card">
                        <mat-card-header>
                            <mat-card-title>Test Results</mat-card-title>
                        </mat-card-header>
                        <mat-card-content>
                            @if (resultDetails(); as details) {
                                @if (details.trials && details.trials.length > 0) {
                                    <div class="trials-container">
                                        @for (trial of details.trials; track trial.trialNumber) {
                                            <div class="trial-section">
                                                <h4>Trial {{ trial.trialNumber }}</h4>
                                                
                                                @if (trial.calculatedResult !== null && trial.calculatedResult !== undefined) {
                                                    <div class="calculated-result">
                                                        <label>Calculated Result:</label>
                                                        <span class="result-value">{{ trial.calculatedResult | number:'1.2-3' }}</span>
                                                    </div>
                                                }

                                                @if (trial.values && hasTrialValues(trial.values)) {
                                                    <div class="trial-values">
                                                        <h5>Input Values:</h5>
                                                        <div class="values-grid">
                                                            @for (valueEntry of getTrialValueEntries(trial.values); track valueEntry.key) {
                                                                @if (valueEntry.value !== null && valueEntry.value !== undefined) {
                                                                    <div class="value-item">
                                                                        <label>{{ formatFieldName(valueEntry.key) }}:</label>
                                                                        <span>{{ formatFieldValue(valueEntry.value) }}</span>
                                                                    </div>
                                                                }
                                                            }
                                                        </div>
                                                    </div>
                                                }

                                                <div class="trial-status">
                                                    <mat-chip [class]="trial.isComplete ? 'complete-chip' : 'incomplete-chip'">
                                                        {{ trial.isComplete ? 'Complete' : 'Incomplete' }}
                                                    </mat-chip>
                                                </div>
                                            </div>
                                            
                                            @if (!$last) {
                                                <mat-divider></mat-divider>
                                            }
                                        }
                                    </div>
                                } @else {
                                    <div class="no-trials">
                                        <mat-icon>info</mat-icon>
                                        <p>No trial data available for this result</p>
                                    </div>
                                }

                                @if (details.comments) {
                                    <mat-divider></mat-divider>
                                    <div class="comments-section">
                                        <h4>Comments</h4>
                                        <p class="comments-text">{{ details.comments }}</p>
                                    </div>
                                }
                            } @else {
                                <div class="no-details">
                                    <mat-icon>info</mat-icon>
                                    <p>No detailed result data available</p>
                                </div>
                            }
                        </mat-card-content>
                    </mat-card>
                </div>
            }
        </mat-dialog-content>

        <mat-dialog-actions align="end">
            <button mat-button mat-dialog-close>Close</button>
            @if (historicalService.selectedHistoricalResult()) {
                <button mat-raised-button color="primary" (click)="exportResult()">
                    <mat-icon>download</mat-icon>
                    Export
                </button>
            }
        </mat-dialog-actions>
    `,
    styles: [`
        .dialog-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 0 24px;
        }

        .dialog-header h2 {
            display: flex;
            align-items: center;
            gap: 8px;
            margin: 0;
        }

        .dialog-content {
            min-width: 600px;
            max-width: 800px;
            max-height: 70vh;
            padding: 0 24px 24px 24px;
        }

        .loading-container, .error-container, .no-details, .no-trials {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 2rem;
            text-align: center;
        }

        .error-container mat-icon, .no-details mat-icon, .no-trials mat-icon {
            font-size: 48px;
            width: 48px;
            height: 48px;
            margin-bottom: 1rem;
        }

        .result-details {
            display: flex;
            flex-direction: column;
            gap: 16px;
        }

        .info-card, .results-card {
            margin: 0;
        }

        .info-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 16px;
        }

        .info-item {
            display: flex;
            flex-direction: column;
            gap: 4px;
        }

        .info-item label {
            font-weight: 500;
            color: #666;
            font-size: 0.9rem;
        }

        .info-item span {
            font-size: 1rem;
        }

        .status-chip {
            width: fit-content;
        }

        .status-complete {
            background: #e8f5e8;
            color: #2e7d32;
        }

        .status-in-progress {
            background: #fff3e0;
            color: #f57c00;
        }

        .status-pending {
            background: #ffebee;
            color: #c62828;
        }

        .trials-container {
            display: flex;
            flex-direction: column;
            gap: 16px;
        }

        .trial-section {
            padding: 16px;
            border: 1px solid #e0e0e0;
            border-radius: 4px;
            background: #fafafa;
        }

        .trial-section h4 {
            margin: 0 0 12px 0;
            color: #333;
        }

        .trial-section h5 {
            margin: 12px 0 8px 0;
            color: #555;
            font-size: 0.9rem;
        }

        .calculated-result {
            display: flex;
            align-items: center;
            gap: 8px;
            margin-bottom: 12px;
        }

        .calculated-result label {
            font-weight: 500;
            color: #666;
        }

        .result-value {
            font-size: 1.1rem;
            font-weight: 600;
            color: #1976d2;
        }

        .values-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 12px;
        }

        .value-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 4px 0;
        }

        .value-item label {
            font-weight: 500;
            color: #666;
            font-size: 0.9rem;
        }

        .trial-status {
            margin-top: 12px;
        }

        .complete-chip {
            background: #e8f5e8;
            color: #2e7d32;
        }

        .incomplete-chip {
            background: #ffebee;
            color: #c62828;
        }

        .comments-section {
            margin-top: 16px;
        }

        .comments-section h4 {
            margin: 0 0 8px 0;
            color: #333;
        }

        .comments-text {
            background: #f5f5f5;
            padding: 12px;
            border-radius: 4px;
            margin: 0;
            font-style: italic;
        }
    `]
})
export class HistoricalResultDetailsDialogComponent {
    protected historicalService = inject(HistoricalResultsService);

    // Computed signal for result details
    readonly resultDetails = computed(() => this.historicalService.selectedHistoricalResult());

    constructor(
        private dialogRef: MatDialogRef<HistoricalResultDetailsDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: HistoricalResultDetailsDialogData
    ) {
        // Load the detailed result when dialog opens
        this.loadResultDetails();
    }

    private loadResultDetails(): void {
        this.historicalService.getHistoricalResultDetails(
            this.data.sampleId,
            this.data.testId,
            this.data.summary.sampleId
        ).subscribe();
    }

    retryLoad(): void {
        this.historicalService.clearError();
        this.loadResultDetails();
    }

    exportResult(): void {
        // TODO: Implement export functionality
        const result = this.resultDetails();
        if (result) {
            console.log('Exporting result:', result);
            // Could implement CSV, JSON, or PDF export here
        }
    }

    hasTrialValues(values: any): boolean {
        if (!values || typeof values !== 'object') return false;
        return Object.values(values).some(value => value !== null && value !== undefined && value !== '');
    }

    getTrialValueEntries(values: any): Array<{ key: string, value: any }> {
        if (!values || typeof values !== 'object') return [];
        return Object.entries(values).map(([key, value]) => ({ key, value }));
    }

    formatFieldName(fieldName: string): string {
        // Convert camelCase to readable format
        return fieldName
            .replace(/([A-Z])/g, ' $1')
            .replace(/^./, str => str.toUpperCase())
            .trim();
    }

    formatFieldValue(value: any): string {
        if (value === null || value === undefined) return 'N/A';
        if (typeof value === 'number') {
            return value.toLocaleString(undefined, { maximumFractionDigits: 3 });
        }
        return String(value);
    }
}