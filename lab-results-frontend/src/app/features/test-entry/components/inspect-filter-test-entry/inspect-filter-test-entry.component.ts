import { Component, OnInit, OnDestroy, inject, signal, computed, effect } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, forkJoin } from 'rxjs';

import { SampleService } from '../../../../shared/services/sample.service';
import { TestService } from '../../../../shared/services/test.service';
import { ValidationService } from '../../../../shared/services/validation.service';
import { Sample } from '../../../../shared/models/sample.model';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { ParticleAnalysisCardComponent } from '../../../../shared/components/particle-analysis-card/particle-analysis-card.component';
import { SharedModule } from '../../../../shared/shared.module';

import {
    ParticleAnalysisData,
    ParticleAnalysis,
    ParticleTypeDefinition,
    ParticleSubTypeCategory,
    ParticleAnalysisConfig,
    ParticleAnalysisValidation
} from '../../../../shared/models/particle-analysis.model';

interface InspectFilterResult {
    sampleId: number;
    testId: number;
    particleAnalyses: any[];
    narrative: string;
    major: number | null;
    minor: number | null;
    trace: number | null;
    overallSeverity: number;
    mediaReady: boolean;
}

@Component({
    selector: 'app-inspect-filter-test-entry',
    standalone: true,
    imports: [SharedModule, ParticleAnalysisCardComponent],
    template: `
        <div class="inspect-filter-container">
            <!-- Header Section -->
            <mat-card class="header-card">
                <mat-card-header>
                    <mat-card-title>Inspect Filter Test Entry</mat-card-title>
                    <mat-card-subtitle>
                        @if (selectedSample(); as sample) {
                            Sample: {{ sample.tagNumber }} - {{ sample.component }} ({{ sample.location }})
                        }
                    </mat-card-subtitle>
                </mat-card-header>
            </mat-card>

            <!-- Loading Spinner -->
            @if (isLoading()) {
                <div class="loading-container">
                    <mat-spinner></mat-spinner>
                    <p>Loading particle analysis data...</p>
                </div>
            }

            <!-- Error Display -->
            @if (hasError()) {
                <mat-card class="error-card">
                    <mat-card-content>
                        <div class="error-content">
                            <mat-icon color="warn">error</mat-icon>
                            <span>{{ error() }}</span>
                            <button mat-button color="primary" (click)="clearError()">Dismiss</button>
                        </div>
                    </mat-card-content>
                </mat-card>
            }

            <!-- Main Form -->
            @if (inspectFilterForm && !isLoading()) {
                <form [formGroup]="inspectFilterForm" (ngSubmit)="onSave()">
                    <!-- Sample Information Display -->
                    @if (selectedSample(); as sample) {
                        <mat-card class="sample-info-card">
                            <mat-card-content>
                                <div class="sample-info-grid">
                                    <div class="info-item">
                                        <label>Tag Number:</label>
                                        <span>{{ sample.tagNumber }}</span>
                                    </div>
                                    <div class="info-item">
                                        <label>Component:</label>
                                        <span>{{ sample.component }}</span>
                                    </div>
                                    <div class="info-item">
                                        <label>Location:</label>
                                        <span>{{ sample.location }}</span>
                                    </div>
                                    <div class="info-item">
                                        <label>Lube Type:</label>
                                        <span>{{ sample.lubeType }}</span>
                                    </div>
                                    <div class="info-item">
                                        <label>Sample Date:</label>
                                        <span>{{ sample.sampleDate | date:'short' }}</span>
                                    </div>
                                    <div class="info-item">
                                        <label>Quality Class:</label>
                                        <span>{{ sample.qualityClass || 'N/A' }}</span>
                                    </div>
                                </div>
                            </mat-card-content>
                        </mat-card>
                    }

                    <!-- View Filter Controls -->
                    <mat-card class="filter-controls-card">
                        <mat-card-content>
                            <div class="filter-controls">
                                <mat-radio-group [(ngModel)]="viewFilter" (change)="onViewFilterChange()" [ngModelOptions]="{standalone: true}">
                                    <mat-radio-button value="all">All Records</mat-radio-button>
                                    <mat-radio-button value="review">Review Only</mat-radio-button>
                                </mat-radio-group>
                                
                                <div class="severity-info">
                                    <span class="severity-label">Overall Severity:</span>
                                    <span class="severity-value" [class]="'severity-' + overallSeverity()">
                                        {{ overallSeverity() }}
                                    </span>
                                    @if (mediaReady()) {
                                        <span class="media-ready-badge">Media Ready</span>
                                    }
                                </div>
                            </div>
                        </mat-card-content>
                    </mat-card>

                    <!-- Particle Analysis Card -->
                    <app-particle-analysis-card
                        [particleTypes]="particleTypes()"
                        [subTypeCategories]="subTypeCategories()"
                        [initialData]="particleAnalysisData()"
                        [config]="particleAnalysisConfig()"
                        (particleDataChange)="onParticleDataChange($event)"
                        (severityChange)="onParticleSeverityChange($event)"
                        (validationChange)="onParticleValidationChange($event)">
                    </app-particle-analysis-card>

                    <!-- Narrative Section -->
                    <mat-card class="narrative-card">
                        <mat-card-header>
                            <mat-card-title>Test Narrative</mat-card-title>
                            <mat-card-subtitle>Overall assessment and findings</mat-card-subtitle>
                        </mat-card-header>
                        <mat-card-content>
                            <mat-form-field appearance="outline" class="full-width">
                                <mat-label>Narrative</mat-label>
                                <textarea 
                                    matInput 
                                    formControlName="narrative"
                                    rows="4"
                                    placeholder="Enter overall test findings, conclusions, and recommendations...">
                                </textarea>
                                <mat-hint>Provide a comprehensive summary of the particle analysis results</mat-hint>
                            </mat-form-field>
                        </mat-card-content>
                    </mat-card>

                    <!-- Action Buttons -->
                    <div class="action-buttons">
                        <button 
                            mat-raised-button 
                            color="primary" 
                            type="submit"
                            [disabled]="!inspectFilterForm.valid || isLoading()">
                            <mat-icon>save</mat-icon>
                            Save Results
                        </button>
                        
                        <button 
                            mat-raised-button 
                            color="accent" 
                            type="button"
                            (click)="onClear()"
                            [disabled]="isLoading()">
                            <mat-icon>clear</mat-icon>
                            Clear Form
                        </button>
                        
                        <button 
                            mat-raised-button 
                            color="warn" 
                            type="button"
                            (click)="onDelete()"
                            [disabled]="isLoading() || !hasExistingResults()">
                            <mat-icon>delete</mat-icon>
                            Delete Results
                        </button>
                        
                        <button 
                            mat-stroked-button 
                            type="button"
                            (click)="onCancel()">
                            <mat-icon>arrow_back</mat-icon>
                            Back to Sample Selection
                        </button>
                    </div>
                </form>
            }

            <!-- Historical Results Section -->
            @if (testResultHistory().length > 0) {
                <mat-card class="history-card">
                    <mat-card-header>
                        <mat-card-title>Last 12 Results for Inspect Filter Test</mat-card-title>
                    </mat-card-header>
                    <mat-card-content>
                        <div class="history-table-container">
                            <table mat-table [dataSource]="testResultHistory()" class="history-table">
                                <ng-container matColumnDef="sampleId">
                                    <th mat-header-cell *matHeaderCellDef>Sample ID</th>
                                    <td mat-cell *matCellDef="let result">{{ result.sampleId }}</td>
                                </ng-container>

                                <ng-container matColumnDef="entryDate">
                                    <th mat-header-cell *matHeaderCellDef>Entry Date</th>
                                    <td mat-cell *matCellDef="let result">{{ result.entryDate | date:'short' }}</td>
                                </ng-container>

                                <ng-container matColumnDef="status">
                                    <th mat-header-cell *matHeaderCellDef>Status</th>
                                    <td mat-cell *matCellDef="let result">
                                        <span [class]="'status-' + result.status.toLowerCase()">
                                            {{ getStatusText(result.status) }}
                                        </span>
                                    </td>
                                </ng-container>

                                <ng-container matColumnDef="severity">
                                    <th mat-header-cell *matHeaderCellDef>Overall Severity</th>
                                    <td mat-cell *matCellDef="let result">
                                        <span [class]="'severity-' + (result.overallSeverity || 0)">
                                            {{ result.overallSeverity || 0 }}
                                        </span>
                                    </td>
                                </ng-container>

                                <tr mat-header-row *matHeaderRowDef="historyDisplayedColumns"></tr>
                                <tr mat-row *matRowDef="let row; columns: historyDisplayedColumns;"></tr>
                            </table>
                        </div>
                    </mat-card-content>
                </mat-card>
            }
        </div>
    `,
    styles: [`
        .inspect-filter-container {
            padding: 20px;
            max-width: 1400px;
            margin: 0 auto;
        }

        .header-card, .sample-info-card, .filter-controls-card, .narrative-card, .history-card {
            margin-bottom: 20px;
        }

        .loading-container {
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 40px;
        }

        .error-card {
            background-color: #ffebee;
            border-left: 4px solid #f44336;
        }

        .error-content {
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .sample-info-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
        }

        .info-item {
            display: flex;
            flex-direction: column;
        }

        .info-item label {
            font-weight: 500;
            color: #666;
            font-size: 0.9em;
        }

        .info-item span {
            font-weight: 400;
            margin-top: 2px;
        }

        .filter-controls {
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 20px;
        }

        .severity-info {
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .severity-label {
            font-weight: 500;
            color: #666;
        }

        .severity-value {
            font-weight: bold;
            font-size: 1.2em;
            padding: 4px 8px;
            border-radius: 4px;
        }

        .severity-0 { background-color: #e8f5e8; color: #2e7d32; }
        .severity-1 { background-color: #fff3e0; color: #f57c00; }
        .severity-2 { background-color: #fff3e0; color: #f57c00; }
        .severity-3 { background-color: #ffebee; color: #d32f2f; }
        .severity-4 { background-color: #ffebee; color: #d32f2f; }

        .media-ready-badge {
            background-color: #4caf50;
            color: white;
            padding: 4px 12px;
            border-radius: 16px;
            font-size: 0.875rem;
            font-weight: 500;
        }

        .full-width {
            width: 100%;
        }

        .action-buttons {
            display: flex;
            gap: 12px;
            flex-wrap: wrap;
            margin-top: 20px;
        }

        .action-buttons button {
            min-width: 140px;
        }

        .history-table-container {
            overflow-x: auto;
        }

        .history-table {
            width: 100%;
        }

        .status-c { color: #4caf50; font-weight: 500; }
        .status-e { color: #ff9800; font-weight: 500; }
        .status-x { color: #757575; font-weight: 500; }

        @media (max-width: 768px) {
            .inspect-filter-container {
                padding: 10px;
            }
            
            .action-buttons {
                flex-direction: column;
            }
            
            .action-buttons button {
                width: 100%;
            }
            
            .filter-controls {
                flex-direction: column;
                align-items: flex-start;
            }
        }
    `]
})
export class InspectFilterTestEntryComponent implements OnInit, OnDestroy {
    private fb = inject(FormBuilder);
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private snackBar = inject(MatSnackBar);
    private dialog = inject(MatDialog);
    private sampleService = inject(SampleService);
    private testService = inject(TestService);
    private validationService = inject(ValidationService);
    private destroy$ = new Subject<void>();

    // Signals from services
    readonly selectedSample = this.sampleService.selectedSample;
    readonly testResultHistory = this.testService.testResultHistory;
    readonly isLoading = computed(() =>
        this.sampleService.isLoading() || this.testService.isLoading() || this.loadingParticleData()
    );
    readonly error = computed(() =>
        this.sampleService.error() || this.testService.error() || this.particleDataError()
    );
    readonly hasError = computed(() => this.error() !== null);

    // Component state signals
    private loadingParticleData = signal(false);
    private particleDataError = signal<string | null>(null);
    readonly particleTypes = signal<ParticleTypeDefinition[]>([]);
    readonly subTypeCategories = signal<ParticleSubTypeCategory[]>([]);
    readonly particleAnalysisData = signal<ParticleAnalysisData | null>(null);
    readonly particleAnalysisConfig = computed(() => this.createParticleAnalysisConfig());
    readonly overallSeverity = computed(() => this.calculateOverallSeverity());
    readonly mediaReady = computed(() => this.calculateMediaReady());

    // Component state
    inspectFilterForm!: FormGroup;
    testId = 120; // Inspect Filter test ID - matches the seeded database
    viewFilter: 'all' | 'review' = 'all';
    historyDisplayedColumns = ['sampleId', 'entryDate', 'status', 'severity'];
    private currentParticleData: ParticleAnalysisData | null = null;

    constructor() {
        // Effect to handle sample changes
        effect(() => {
            const sample = this.selectedSample();
            if (sample && this.inspectFilterForm) {
                this.loadExistingResults(sample.id);
                this.loadTestHistory(sample.id);
            }
        });
    }

    ngOnInit(): void {
        // Get sample ID from route parameters
        this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
            const sampleId = +params['sampleId'];
            if (sampleId) {
                this.loadSample(sampleId);
            }
        });

        // Load particle analysis data
        this.loadParticleAnalysisData();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadSample(sampleId: number): void {
        this.sampleService.getSample(sampleId).subscribe({
            next: () => {
                // Sample loaded successfully
            },
            error: (error) => {
                this.snackBar.open(`Failed to load sample: ${error.message}`, 'Close', {
                    duration: 5000,
                    panelClass: ['error-snackbar']
                });
            }
        });
    }

    private loadParticleAnalysisData(): void {
        this.loadingParticleData.set(true);
        this.particleDataError.set(null);

        forkJoin({
            particleTypes: this.testService.getParticleTypes(),
            subTypeCategories: this.testService.getSubTypeCategories()
        }).subscribe({
            next: (data) => {
                this.particleTypes.set(data.particleTypes);
                this.subTypeCategories.set(data.subTypeCategories);
                this.initializeForm();
                this.loadingParticleData.set(false);
            },
            error: (error) => {
                this.particleDataError.set(`Failed to load particle analysis data: ${error.message}`);
                this.loadingParticleData.set(false);
            }
        });
    }

    private loadExistingResults(sampleId: number): void {
        this.testService.getInspectFilterResults(sampleId, this.testId).subscribe({
            next: (result) => {
                if (result && this.inspectFilterForm) {
                    this.populateFormWithResults(result);
                }
            },
            error: () => {
                // No existing results - this is normal for new entries
            }
        });
    }

    private loadTestHistory(sampleId: number): void {
        this.testService.getTestResultsHistory(this.testId, sampleId, 12).subscribe({
            error: (error) => {
                console.warn('Failed to load test history:', error);
            }
        });
    }

    private initializeForm(): void {
        this.inspectFilterForm = this.fb.group({
            narrative: ['']
        });
    }

    private populateFormWithResults(result: InspectFilterResult): void {
        // Set narrative
        this.inspectFilterForm.patchValue({
            narrative: result.narrative || ''
        });

        // Transform particle analyses to ParticleAnalysisData format
        const analyses: ParticleAnalysis[] = result.particleAnalyses.map(analysis => ({
            particleTypeId: analysis.particleTypeDefinitionId,
            subTypeValues: analysis.subTypeValues,
            comments: analysis.comments,
            severity: this.getSeverityFromSubTypeValues(analysis.subTypeValues),
            status: analysis.status as 'active' | 'review' | 'complete'
        }));

        const particleData: ParticleAnalysisData = {
            analyses,
            overallSeverity: result.overallSeverity,
            isValid: true,
            summary: {
                totalParticles: analyses.length,
                criticalParticles: analyses.filter(a => a.severity >= 3).length,
                recommendations: []
            }
        };

        this.particleAnalysisData.set(particleData);
        this.currentParticleData = particleData;
    }

    /**
     * Create configuration for particle analysis card
     */
    private createParticleAnalysisConfig(): ParticleAnalysisConfig {
        return {
            testId: this.testId,
            readonly: false,
            showImages: true,
            viewFilter: this.viewFilter,
            enableSeverityCalculation: true
        };
    }

    /**
     * Handle particle data changes from the particle analysis card
     */
    onParticleDataChange(data: ParticleAnalysisData): void {
        this.currentParticleData = data;
        this.particleAnalysisData.set(data);
    }

    /**
     * Handle severity changes from the particle analysis card
     */
    onParticleSeverityChange(severity: number): void {
        // Severity is already included in the particle data change event
        // This can be used for additional UI updates if needed
    }

    /**
     * Handle validation changes from the particle analysis card
     */
    onParticleValidationChange(validation: ParticleAnalysisValidation): void {
        // Handle validation feedback if needed
        if (!validation.isValid) {
            console.warn('Particle analysis validation errors:', validation.errors);
        }
    }

    /**
     * Get severity value from sub-type values (legacy compatibility)
     */
    private getSeverityFromSubTypeValues(subTypeValues: { [categoryId: number]: number | null }): number {
        // Category 1 is typically severity in the legacy system
        return subTypeValues[1] || 0;
    }

    onViewFilterChange(): void {
        // View filter changed - the computed signal will handle the filtering
    }

    private calculateOverallSeverity(): number {
        const particleData = this.currentParticleData;
        if (!particleData) return 0;

        return particleData.overallSeverity;
    }

    private calculateMediaReady(): boolean {
        const overallSeverity = this.overallSeverity();
        const narrative = this.inspectFilterForm?.get('narrative')?.value;

        return overallSeverity > 0 && narrative && narrative.trim().length > 0;
    }

    onSave(): void {
        if (!this.inspectFilterForm.valid) {
            this.markFormGroupTouched(this.inspectFilterForm);
            this.snackBar.open('Please correct the errors in the form', 'Close', {
                duration: 3000,
                panelClass: ['error-snackbar']
            });
            return;
        }

        const sample = this.selectedSample();
        if (!sample) {
            this.snackBar.open('No sample selected', 'Close', {
                duration: 3000,
                panelClass: ['error-snackbar']
            });
            return;
        }

        const formValue = this.inspectFilterForm.value;
        const particleData = this.currentParticleData;

        // Transform particle analysis data to legacy format
        const particleAnalyses: any[] = [];
        if (particleData) {
            particleData.analyses.forEach(analysis => {
                // Only include particle analyses that have some data
                if (analysis.severity > 0 || analysis.comments.trim() ||
                    Object.values(analysis.subTypeValues).some(value => value !== null)) {
                    particleAnalyses.push({
                        sampleId: sample.id,
                        testId: this.testId,
                        particleTypeDefinitionId: analysis.particleTypeId,
                        status: analysis.status === 'active' ? 'E' : analysis.status === 'complete' ? 'C' : 'X',
                        comments: analysis.comments || '',
                        subTypeValues: analysis.subTypeValues
                    });
                }
            });
        }

        const request = {
            sampleId: sample.id,
            testId: this.testId,
            particleAnalyses,
            narrative: formValue.narrative || '',
            entryId: 'USER' // This should come from authentication
        };

        this.testService.saveInspectFilterResults(request).subscribe({
            next: (response) => {
                this.snackBar.open(`Successfully saved ${response.recordsSaved} records`, 'Close', {
                    duration: 3000,
                    panelClass: ['success-snackbar']
                });
                this.loadTestHistory(sample.id);
            },
            error: (error) => {
                this.snackBar.open(`Failed to save results: ${error.message}`, 'Close', {
                    duration: 5000,
                    panelClass: ['error-snackbar']
                });
            }
        });
    }

    onClear(): void {
        const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
            data: {
                title: 'Clear Form',
                message: 'Are you sure you want to clear all entered data? This action cannot be undone.',
                confirmText: 'Clear',
                cancelText: 'Cancel'
            }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.inspectFilterForm.reset();
                this.initializeForm();
                this.particleAnalysisData.set(null);
                this.currentParticleData = null;
                this.snackBar.open('Form cleared', 'Close', { duration: 2000 });
            }
        });
    }

    onDelete(): void {
        const sample = this.selectedSample();
        if (!sample) return;

        const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
            data: {
                title: 'Delete Test Results',
                message: 'Are you sure you want to delete all Inspect Filter results for this sample? This action cannot be undone.',
                confirmText: 'Delete',
                cancelText: 'Cancel'
            }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result && sample) {
                this.testService.deleteInspectFilterResults(sample.id, this.testId).subscribe({
                    next: (response) => {
                        this.snackBar.open(`Successfully deleted ${response.recordsDeleted} records`, 'Close', {
                            duration: 3000,
                            panelClass: ['success-snackbar']
                        });
                        this.inspectFilterForm.reset();
                        this.initializeForm();
                        this.particleAnalysisData.set(null);
                        this.currentParticleData = null;
                        this.loadTestHistory(sample.id);
                    },
                    error: (error) => {
                        this.snackBar.open(`Failed to delete results: ${error.message}`, 'Close', {
                            duration: 5000,
                            panelClass: ['error-snackbar']
                        });
                    }
                });
            }
        });
    }

    onCancel(): void {
        this.router.navigate(['/samples']);
    }

    clearError(): void {
        this.sampleService.clearError();
        this.testService.clearError();
        this.particleDataError.set(null);
    }

    hasExistingResults(): boolean {
        // Check if any particle analysis has data
        const particleData = this.currentParticleData;
        if (!particleData) return false;

        return particleData.analyses.some(analysis =>
            analysis.severity > 0 ||
            analysis.comments.trim().length > 0 ||
            Object.values(analysis.subTypeValues).some(value => value !== null)
        );
    }

    getStatusText(status: string): string {
        switch (status) {
            case 'C': return 'Complete';
            case 'E': return 'In Progress';
            case 'X': return 'Pending';
            default: return 'Unknown';
        }
    }

    private markFormGroupTouched(formGroup: FormGroup): void {
        Object.keys(formGroup.controls).forEach(key => {
            const control = formGroup.get(key);
            control?.markAsTouched();

            if (control instanceof FormGroup) {
                this.markFormGroupTouched(control);
            }
        });
    }
}