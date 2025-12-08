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

interface FilterResidueResult {
    sampleId: number;
    testId: number;
    particleAnalyses: any[];
    narrative: string;
    major: number | null;
    minor: number | null;
    trace: number | null;
    overallSeverity: number;
    sampleSize: number | null;
    residueWeight: number | null;
    finalWeight: number | null;
}

@Component({
    selector: 'app-filter-residue-test-entry',
    standalone: true,
    imports: [SharedModule, ParticleAnalysisCardComponent],
    template: `
        <div class="filter-residue-container">
            <!-- Header Section -->
            <mat-card class="header-card">
                <mat-card-header>
                    <mat-card-title>Filter Residue Test Entry</mat-card-title>
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
            @if (filterResidueForm && !isLoading()) {
                <form [formGroup]="filterResidueForm" (ngSubmit)="onSave()">
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
                                </div>
                            </div>
                        </mat-card-content>
                    </mat-card>

                    <!-- Calculation Fields Card -->
                    <mat-card class="calculation-card">
                        <mat-card-header>
                            <mat-card-title>Filter Residue Calculations</mat-card-title>
                            <mat-card-subtitle>Enter sample measurements</mat-card-subtitle>
                        </mat-card-header>
                        <mat-card-content>
                            <div class="calculation-fields">
                                <mat-form-field appearance="outline">
                                    <mat-label>Sample Size (g)</mat-label>
                                    <input 
                                        matInput 
                                        type="number" 
                                        formControlName="sampleSize"
                                        step="0.01"
                                        min="0"
                                        (input)="calculateFinalWeight()">
                                    <mat-hint>Enter the sample size in grams</mat-hint>
                                    @if (filterResidueForm.get('sampleSize')?.hasError('min')) {
                                        <mat-error>Sample size must be positive</mat-error>
                                    }
                                </mat-form-field>

                                <mat-form-field appearance="outline">
                                    <mat-label>Residue Weight (g)</mat-label>
                                    <input 
                                        matInput 
                                        type="number" 
                                        formControlName="residueWeight"
                                        step="0.001"
                                        min="0"
                                        (input)="calculateFinalWeight()">
                                    <mat-hint>Enter the residue weight in grams</mat-hint>
                                    @if (filterResidueForm.get('residueWeight')?.hasError('min')) {
                                        <mat-error>Residue weight must be positive</mat-error>
                                    }
                                </mat-form-field>

                                <mat-form-field appearance="outline">
                                    <mat-label>Final Weight (%)</mat-label>
                                    <input 
                                        matInput 
                                        type="number" 
                                        formControlName="finalWeight"
                                        step="0.1"
                                        readonly>
                                    <mat-hint>Automatically calculated: (100 / Sample Size) × Residue Weight</mat-hint>
                                </mat-form-field>
                            </div>

                            @if (finalWeight() !== null && finalWeight()! > 0) {
                                <div class="result-display">
                                    <mat-icon>calculate</mat-icon>
                                    <strong>Calculated Result: {{ finalWeight() }}%</strong>
                                </div>
                            }
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
                                <mat-hint>Provide a comprehensive summary of the filter residue analysis</mat-hint>
                            </mat-form-field>
                        </mat-card-content>
                    </mat-card>

                    <!-- Action Buttons -->
                    <div class="action-buttons">
                        <button 
                            mat-raised-button 
                            color="primary" 
                            type="submit"
                            [disabled]="!filterResidueForm.valid || isLoading()">
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
                            <mat-icon>close</mat-icon>
                            Cancel
                        </button>
                    </div>

                    <!-- Historical Results Section -->
                    @if (testResultHistory() && testResultHistory().length > 0) {
                        <mat-card class="history-card">
                            <mat-card-header>
                                <mat-card-title>Historical Results</mat-card-title>
                                <mat-card-subtitle>Last {{ testResultHistory().length }} test results</mat-card-subtitle>
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
                                                    {{ result.status }}
                                                </span>
                                            </td>
                                        </ng-container>

                                        <ng-container matColumnDef="severity">
                                            <th mat-header-cell *matHeaderCellDef>Severity</th>
                                            <td mat-cell *matCellDef="let result">
                                                <span [class]="'severity-' + result.severity">
                                                    {{ result.severity }}
                                                </span>
                                            </td>
                                        </ng-container>

                                        <ng-container matColumnDef="finalWeight">
                                            <th mat-header-cell *matHeaderCellDef>Final Weight (%)</th>
                                            <td mat-cell *matCellDef="let result">{{ result.finalWeight | number:'1.1-1' }}</td>
                                        </ng-container>

                                        <tr mat-header-row *matHeaderRowDef="historyDisplayedColumns"></tr>
                                        <tr mat-row *matRowDef="let row; columns: historyDisplayedColumns;"></tr>
                                    </table>
                                </div>
                            </mat-card-content>
                        </mat-card>
                    }
                </form>
            }
        </div>
    `,
    styles: [`
        .filter-residue-container {
            padding: 20px;
            max-width: 1400px;
            margin: 0 auto;
        }

        .header-card, .sample-info-card, .filter-controls-card, .calculation-card, .narrative-card, .history-card {
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

        .calculation-fields {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 16px;
            margin-bottom: 16px;
        }

        .result-display {
            background-color: #e3f2fd;
            padding: 16px;
            border-radius: 8px;
            display: flex;
            align-items: center;
            gap: 12px;
            color: #1565c0;
            margin-top: 12px;
        }

        .result-display mat-icon {
            color: #1565c0;
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
            .filter-residue-container {
                padding: 10px;
            }
            
            .calculation-fields {
                grid-template-columns: 1fr;
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
export class FilterResidueTestEntryComponent implements OnInit, OnDestroy {
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
    readonly finalWeight = computed(() => {
        if (!this.filterResidueForm) return null;
        return this.filterResidueForm.get('finalWeight')?.value;
    });

    // Component state
    filterResidueForm!: FormGroup;
    testId = 180; // Filter Residue test ID
    viewFilter: 'all' | 'review' = 'all';
    historyDisplayedColumns = ['sampleId', 'entryDate', 'status', 'severity', 'finalWeight'];
    private currentParticleData: ParticleAnalysisData | null = null;
    private hasResults = false;

    constructor() {
        // Effect to handle sample changes
        effect(() => {
            const sample = this.selectedSample();
            if (sample && this.filterResidueForm) {
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
        this.testService.getFilterResidueResults(sampleId, this.testId).subscribe({
            next: (result) => {
                if (result && this.filterResidueForm) {
                    this.populateFormWithResults(result);
                    this.hasResults = true;
                }
            },
            error: () => {
                // No existing results - this is normal for new entries
                this.hasResults = false;
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
        this.filterResidueForm = this.fb.group({
            sampleSize: [null, [Validators.min(0)]],
            residueWeight: [null, [Validators.min(0)]],
            finalWeight: [{ value: null, disabled: true }],
            narrative: ['']
        });

        // Watch for changes to calculate final weight
        this.filterResidueForm.get('sampleSize')?.valueChanges
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => this.calculateFinalWeight());

        this.filterResidueForm.get('residueWeight')?.valueChanges
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => this.calculateFinalWeight());
    }

    private populateFormWithResults(result: any): void {
        // Set form values
        this.filterResidueForm.patchValue({
            sampleSize: result.sampleSize || null,
            residueWeight: result.residueWeight || null,
            finalWeight: result.finalWeight || null,
            narrative: result.narrative || ''
        });

        // Transform particle analyses to ParticleAnalysisData format
        if (result.particleAnalyses && result.particleAnalyses.length > 0) {
            const analyses: ParticleAnalysis[] = result.particleAnalyses.map((analysis: any) => ({
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
    }

    /**
     * Calculate final weight based on sample size and residue weight
     * Formula: (100 / sampleSize) * residueWeight
     * Rounded to 1 decimal place
     */
    calculateFinalWeight(): void {
        const sampleSize = this.filterResidueForm.get('sampleSize')?.value;
        const residueWeight = this.filterResidueForm.get('residueWeight')?.value;

        if (sampleSize && residueWeight && sampleSize > 0) {
            const result = (100 / sampleSize) * residueWeight;
            const rounded = Math.round(result * 10) / 10;
            this.filterResidueForm.get('finalWeight')?.setValue(rounded, { emitEvent: false });
        } else {
            this.filterResidueForm.get('finalWeight')?.setValue(null, { emitEvent: false });
        }
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
     * Handle severity changes from particle analysis
     */
    onParticleSeverityChange(severity: number): void {
        // Severity is managed by the particle analysis card
    }

    /**
     * Handle validation changes from particle analysis
     */
    onParticleValidationChange(validation: ParticleAnalysisValidation): void {
        // Update form validity based on particle analysis validation
    }

    /**
     * Handle view filter changes
     */
    onViewFilterChange(): void {
        // Re-render particle analysis with new filter
        this.particleAnalysisData.set({ ...this.particleAnalysisData()! });
    }

    /**
     * Calculate overall severity from particle data
     */
    private calculateOverallSeverity(): number {
        const data = this.particleAnalysisData();
        if (!data || !data.analyses || data.analyses.length === 0) {
            return 0;
        }
        return data.overallSeverity || 0;
    }

    /**
     * Get severity from subtype values
     */
    private getSeverityFromSubTypeValues(subTypeValues: any): number {
        // Logic to determine severity from subtype values
        // This should match the business logic for severity calculation
        if (!subTypeValues) return 0;
        
        const values = Object.values(subTypeValues) as number[];
        const maxValue = Math.max(...values, 0);
        
        if (maxValue >= 4) return 4;
        if (maxValue >= 3) return 3;
        if (maxValue >= 2) return 2;
        if (maxValue >= 1) return 1;
        return 0;
    }

    /**
     * Save test results
     */
    onSave(): void {
        if (!this.filterResidueForm.valid) {
            this.snackBar.open('Please correct form errors before saving', 'Close', {
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

        const formValue = this.filterResidueForm.getRawValue();
        const result: FilterResidueResult = {
            sampleId: sample.id,
            testId: this.testId,
            particleAnalyses: this.currentParticleData?.analyses || [],
            narrative: formValue.narrative,
            major: null, // Legacy fields - not used in new format
            minor: null,
            trace: null,
            overallSeverity: this.calculateOverallSeverity(),
            sampleSize: formValue.sampleSize,
            residueWeight: formValue.residueWeight,
            finalWeight: formValue.finalWeight
        };

        this.testService.saveFilterResidueResults(result).subscribe({
            next: (response) => {
                this.snackBar.open('Filter residue results saved successfully!', 'Close', {
                    duration: 3000,
                    panelClass: ['success-snackbar']
                });
                this.hasResults = true;
                this.filterResidueForm.markAsPristine();
            },
            error: (error) => {
                this.snackBar.open(`Failed to save results: ${error.message}`, 'Close', {
                    duration: 5000,
                    panelClass: ['error-snackbar']
                });
            }
        });
    }

    /**
     * Clear the form
     */
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
                this.filterResidueForm.reset();
                this.particleAnalysisData.set(null);
                this.currentParticleData = null;
                this.snackBar.open('Form cleared', 'Close', { duration: 2000 });
            }
        });
    }

    /**
     * Delete existing results
     */
    onDelete(): void {
        const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
            data: {
                title: 'Delete Results',
                message: 'Are you sure you want to delete these test results? This action cannot be undone.',
                confirmText: 'Delete',
                cancelText: 'Cancel'
            }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                const sample = this.selectedSample();
                if (sample) {
                    this.testService.deleteFilterResidueResults(sample.id, this.testId).subscribe({
                        next: () => {
                            this.snackBar.open('Filter residue results deleted successfully', 'Close', {
                                duration: 3000,
                                panelClass: ['success-snackbar']
                            });
                            this.filterResidueForm.reset();
                            this.particleAnalysisData.set(null);
                            this.currentParticleData = null;
                            this.hasResults = false;
                        },
                        error: (error) => {
                            this.snackBar.open(`Failed to delete results: ${error.message}`, 'Close', {
                                duration: 5000,
                                panelClass: ['error-snackbar']
                            });
                        }
                    });
                }
            }
        });
    }

    /**
     * Cancel and return to test list
     */
    onCancel(): void {
        if (this.filterResidueForm.dirty) {
            const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
                data: {
                    title: 'Unsaved Changes',
                    message: 'You have unsaved changes. Are you sure you want to cancel?',
                    confirmText: 'Yes, Cancel',
                    cancelText: 'No, Stay'
                }
            });

            dialogRef.afterClosed().subscribe(result => {
                if (result) {
                    this.router.navigate(['/tests']);
                }
            });
        } else {
            this.router.navigate(['/tests']);
        }
    }

    /**
     * Clear error message
     */
    clearError(): void {
        this.particleDataError.set(null);
    }

    /**
     * Check if existing results exist
     */
    hasExistingResults(): boolean {
        return this.hasResults;
    }
}
