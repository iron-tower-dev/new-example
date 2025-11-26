import { Component, OnInit, OnDestroy, inject, signal, computed, effect } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';

import { SampleService } from '../../../../shared/services/sample.service';
import { TestService } from '../../../../shared/services/test.service';
import { ValidationService } from '../../../../shared/services/validation.service';
import { FileUploadService } from '../../../../shared/services/file-upload.service';
import { LookupService, NasLookupRequest, NasLookupResult } from '../../../../shared/services/lookup.service';
import { Sample } from '../../../../shared/models/sample.model';
import {
    TestTemplate,
    SaveTestResultRequest,
    TestCalculationRequest
} from '../../../../shared/models/test.model';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { FileUploadComponent } from '../../../../shared/components/file-upload/file-upload.component';
import { LookupDisplayComponent } from '../../../../shared/components/lookup-display/lookup-display.component';
import { SharedModule } from '../../../../shared/shared.module';

interface ParticleCountTrialData {
    count5to10: number | null;
    count10to15: number | null;
    count15to25: number | null;
    count25to50: number | null;
    count50to100: number | null;
    countOver100: number | null;
    calculatedNAS: number | null;
    isComplete: boolean;
}



@Component({
    selector: 'app-particle-count-test-entry',
    standalone: true,
    imports: [SharedModule, FileUploadComponent, LookupDisplayComponent],
    template: `
        <div class="particle-count-test-container">
            <!-- Header Section -->
            <mat-card class="header-card">
                <mat-card-header>
                    <mat-card-title>Particle Count Test Entry</mat-card-title>
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
                    <p>Loading test data...</p>
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
            @if (particleCountForm && !isLoading()) {
                <form [formGroup]="particleCountForm" (ngSubmit)="onSave()">
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

                    <!-- Trial Entry Section -->
                    <mat-card class="trials-card">
                        <mat-card-header>
                            <mat-card-title>Particle Count Data Entry</mat-card-title>
                            <mat-card-subtitle>Enter particle counts for each size range - NAS values will be calculated automatically</mat-card-subtitle>
                        </mat-card-header>
                        <mat-card-content>
                            <div class="trials-container" formArrayName="trials">
                                @for (trial of trialsArray.controls; track $index) {
                                    <div class="trial-row" [formGroupName]="$index">
                                        <div class="trial-header">
                                            <h4>Trial {{ $index + 1 }}</h4>
                                            <div class="trial-actions">
                                                <app-file-upload
                                                    [sampleId]="selectedSample()?.id || 0"
                                                    [testId]="testId"
                                                    [trialNumber]="$index + 1"
                                                    (fileUploaded)="onFileUploaded($index, $event)">
                                                </app-file-upload>
                                                <mat-checkbox 
                                                    [formControlName]="'isComplete'"
                                                    (change)="onTrialCompleteChange($index)">
                                                    Complete
                                                </mat-checkbox>
                                            </div>
                                        </div>
                                        
                                        <div class="particle-size-grid">
                                            <mat-form-field appearance="outline">
                                                <mat-label>5-10 μm</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    min="0"
                                                    step="1"
                                                    formControlName="count5to10"
                                                    (input)="onParticleCountChange($index)"
                                                    placeholder="Enter count">
                                                <mat-hint>Particles 5-10 micrometers</mat-hint>
                                                @if (getTrialControl($index, 'count5to10')?.errors?.['min']) {
                                                    <mat-error>Count must be 0 or greater</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>10-15 μm</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    min="0"
                                                    step="1"
                                                    formControlName="count10to15"
                                                    (input)="onParticleCountChange($index)"
                                                    placeholder="Enter count">
                                                <mat-hint>Particles 10-15 micrometers</mat-hint>
                                                @if (getTrialControl($index, 'count10to15')?.errors?.['min']) {
                                                    <mat-error>Count must be 0 or greater</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>15-25 μm</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    min="0"
                                                    step="1"
                                                    formControlName="count15to25"
                                                    (input)="onParticleCountChange($index)"
                                                    placeholder="Enter count">
                                                <mat-hint>Particles 15-25 micrometers</mat-hint>
                                                @if (getTrialControl($index, 'count15to25')?.errors?.['min']) {
                                                    <mat-error>Count must be 0 or greater</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>25-50 μm</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    min="0"
                                                    step="1"
                                                    formControlName="count25to50"
                                                    (input)="onParticleCountChange($index)"
                                                    placeholder="Enter count">
                                                <mat-hint>Particles 25-50 micrometers</mat-hint>
                                                @if (getTrialControl($index, 'count25to50')?.errors?.['min']) {
                                                    <mat-error>Count must be 0 or greater</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>50-100 μm</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    min="0"
                                                    step="1"
                                                    formControlName="count50to100"
                                                    (input)="onParticleCountChange($index)"
                                                    placeholder="Enter count">
                                                <mat-hint>Particles 50-100 micrometers</mat-hint>
                                                @if (getTrialControl($index, 'count50to100')?.errors?.['min']) {
                                                    <mat-error>Count must be 0 or greater</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>>100 μm</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    min="0"
                                                    step="1"
                                                    formControlName="countOver100"
                                                    (input)="onParticleCountChange($index)"
                                                    placeholder="Enter count">
                                                <mat-hint>Particles over 100 micrometers</mat-hint>
                                                @if (getTrialControl($index, 'countOver100')?.errors?.['min']) {
                                                    <mat-error>Count must be 0 or greater</mat-error>
                                                }
                                            </mat-form-field>
                                        </div>

                                        <!-- NAS Result Display with Lookup Integration -->
                                        <div class="nas-result-section">
                                            <mat-form-field appearance="outline" class="nas-result-field">
                                                <mat-label>Calculated NAS</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    formControlName="calculatedNAS"
                                                    readonly
                                                    placeholder="Calculated automatically">
                                                <mat-hint>Highest NAS value from all particle size ranges</mat-hint>
                                            </mat-form-field>
                                            
                                            <app-lookup-display
                                                lookupType="nas"
                                                [particleCounts]="getParticleCountsForTrial($index)"
                                                [autoTrigger]="true"
                                                (nasResultEvent)="onNasLookupResult($index, $event)"
                                                (lookupError)="onNasLookupError($index, $event)">
                                            </app-lookup-display>
                                        </div>
                                    </div>
                                }
                            </div>
                        </mat-card-content>
                    </mat-card>

                    <!-- Comments Section -->
                    <mat-card class="comments-card">
                        <mat-card-header>
                            <mat-card-title>Comments</mat-card-title>
                        </mat-card-header>
                        <mat-card-content>
                            <mat-form-field appearance="outline" class="full-width">
                                <mat-label>Test Comments</mat-label>
                                <textarea 
                                    matInput 
                                    formControlName="comments"
                                    rows="3"
                                    placeholder="Enter any comments about this test...">
                                </textarea>
                            </mat-form-field>
                        </mat-card-content>
                    </mat-card>

                    <!-- Action Buttons -->
                    <div class="action-buttons">
                        <button 
                            mat-raised-button 
                            color="primary" 
                            type="submit"
                            [disabled]="!particleCountForm.valid || isLoading()">
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
                        <mat-card-title>Last 12 Results for Particle Count Test</mat-card-title>
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

                                <ng-container matColumnDef="nasResults">
                                    <th mat-header-cell *matHeaderCellDef>NAS Results</th>
                                    <td mat-cell *matCellDef="let result">
                                        @for (trial of result.trials; track trial.trialNumber) {
                                            @if (trial.calculatedResult) {
                                                <span class="trial-result">
                                                    T{{ trial.trialNumber }}: {{ trial.calculatedResult }}
                                                </span>
                                            }
                                        }
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
        .particle-count-test-container {
            padding: 20px;
            max-width: 1400px;
            margin: 0 auto;
        }

        .header-card, .sample-info-card, .trials-card, .comments-card, .history-card {
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

        .trials-container {
            display: flex;
            flex-direction: column;
            gap: 20px;
        }

        .trial-row {
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            padding: 16px;
            background-color: #fafafa;
        }

        .trial-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 16px;
        }

        .trial-header h4 {
            margin: 0;
            color: #333;
        }

        .trial-actions {
            display: flex;
            align-items: center;
            gap: 16px;
        }

        .particle-size-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 16px;
            margin-bottom: 16px;
        }

        .nas-result-section {
            display: flex;
            align-items: center;
            gap: 16px;
            padding: 16px;
            background-color: #f5f5f5;
            border-radius: 8px;
            border-left: 4px solid #2196f3;
        }

        .nas-result-field {
            flex: 0 0 200px;
        }

        .nas-calculation-status {
            display: flex;
            align-items: center;
            gap: 8px;
            color: #666;
        }

        .nas-error {
            display: flex;
            align-items: center;
            gap: 8px;
            color: #f44336;
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

        .trial-result {
            display: inline-block;
            margin-right: 8px;
            padding: 2px 6px;
            background-color: #e3f2fd;
            border-radius: 4px;
            font-size: 0.9em;
        }

        .status-c { color: #4caf50; font-weight: 500; }
        .status-e { color: #ff9800; font-weight: 500; }
        .status-x { color: #757575; font-weight: 500; }

        @media (max-width: 768px) {
            .particle-count-test-container {
                padding: 10px;
            }
            
            .particle-size-grid {
                grid-template-columns: 1fr;
            }
            
            .nas-result-section {
                flex-direction: column;
                align-items: flex-start;
            }
            
            .action-buttons {
                flex-direction: column;
            }
            
            .action-buttons button {
                width: 100%;
            }
        }
    `]
})
export class ParticleCountTestEntryComponent implements OnInit, OnDestroy {
    private fb = inject(FormBuilder);
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private snackBar = inject(MatSnackBar);
    private dialog = inject(MatDialog);
    private sampleService = inject(SampleService);
    private testService = inject(TestService);
    private validationService = inject(ValidationService);
    private fileUploadService = inject(FileUploadService);
    private lookupService = inject(LookupService);
    private destroy$ = new Subject<void>();

    // Signals from services
    readonly selectedSample = this.sampleService.selectedSample;
    readonly testTemplate = this.testService.testTemplate;
    readonly testResult = this.testService.testResult;
    readonly testResultHistory = this.testService.testResultHistory;
    readonly isLoading = computed(() =>
        this.sampleService.isLoading() || this.testService.isLoading()
    );
    readonly error = computed(() =>
        this.sampleService.error() || this.testService.error()
    );
    readonly hasError = computed(() => this.error() !== null);

    // Component state
    particleCountForm!: FormGroup;
    testId = 160; // Particle Count test ID - matches the seeded database
    historyDisplayedColumns = ['sampleId', 'entryDate', 'status', 'nasResults'];

    // NAS calculation state
    private nasCalculationStates = signal<{ [trialIndex: number]: { inProgress: boolean; error: string | null } }>({});

    // Form getters
    get trialsArray(): FormArray {
        return this.particleCountForm.get('trials') as FormArray;
    }

    constructor() {
        // Effect to handle sample changes
        effect(() => {
            const sample = this.selectedSample();
            if (sample && this.particleCountForm) {
                this.loadExistingResults(sample.id);
                this.loadTestHistory(sample.id);
            }
        });

        // Effect to handle template changes
        effect(() => {
            const template = this.testTemplate();
            if (template && !this.particleCountForm) {
                this.initializeForm();
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

        // Load test template
        this.testService.getTestTemplate(this.testId).subscribe();
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

    private loadExistingResults(sampleId: number): void {
        this.testService.getTestResults(this.testId, sampleId).subscribe({
            next: (result) => {
                if (result && this.particleCountForm) {
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
        this.particleCountForm = this.fb.group({
            trials: this.fb.array([]),
            comments: ['']
        });

        // Initialize 4 trials
        for (let i = 0; i < 4; i++) {
            this.addTrial();
        }
    }

    private addTrial(): void {
        const trialGroup = this.fb.group({
            count5to10: [null, [Validators.min(0)]],
            count10to15: [null, [Validators.min(0)]],
            count15to25: [null, [Validators.min(0)]],
            count25to50: [null, [Validators.min(0)]],
            count50to100: [null, [Validators.min(0)]],
            countOver100: [null, [Validators.min(0)]],
            calculatedNAS: [{ value: null, disabled: true }],
            isComplete: [false]
        });

        this.trialsArray.push(trialGroup);
    }

    private populateFormWithResults(result: any): void {
        // Clear existing trials
        while (this.trialsArray.length > 0) {
            this.trialsArray.removeAt(0);
        }

        // Add trials from result
        result.trials.forEach((trial: any) => {
            const trialGroup = this.fb.group({
                count5to10: [trial.values.count5to10 || null, [Validators.min(0)]],
                count10to15: [trial.values.count10to15 || null, [Validators.min(0)]],
                count15to25: [trial.values.count15to25 || null, [Validators.min(0)]],
                count25to50: [trial.values.count25to50 || null, [Validators.min(0)]],
                count50to100: [trial.values.count50to100 || null, [Validators.min(0)]],
                countOver100: [trial.values.countOver100 || null, [Validators.min(0)]],
                calculatedNAS: [{ value: trial.calculatedResult || null, disabled: true }],
                isComplete: [trial.isComplete || false]
            });
            this.trialsArray.push(trialGroup);
        });

        // Set comments
        this.particleCountForm.patchValue({
            comments: result.comments || ''
        });
    }

    onParticleCountChange(trialIndex: number): void {
        const trialGroup = this.trialsArray.at(trialIndex);

        // Get all particle count values
        const particleCounts: { [channel: number]: number } = {};
        const count5to10 = trialGroup.get('count5to10')?.value;
        const count10to15 = trialGroup.get('count10to15')?.value;
        const count15to25 = trialGroup.get('count15to25')?.value;
        const count25to50 = trialGroup.get('count25to50')?.value;
        const count50to100 = trialGroup.get('count50to100')?.value;
        const countOver100 = trialGroup.get('countOver100')?.value;

        // Map to channels (1-6 for the size ranges)
        if (count5to10 !== null && count5to10 >= 0) particleCounts[1] = count5to10;
        if (count10to15 !== null && count10to15 >= 0) particleCounts[2] = count10to15;
        if (count15to25 !== null && count15to25 >= 0) particleCounts[3] = count15to25;
        if (count25to50 !== null && count25to50 >= 0) particleCounts[4] = count25to50;
        if (count50to100 !== null && count50to100 >= 0) particleCounts[5] = count50to100;
        if (countOver100 !== null && countOver100 >= 0) particleCounts[6] = countOver100;

        // Only calculate if we have at least one valid particle count
        if (Object.keys(particleCounts).length > 0) {
            this.calculateNAS(trialIndex, particleCounts);
        } else {
            trialGroup.get('calculatedNAS')?.setValue(null);
            this.updateNasCalculationState(trialIndex, false, null);
        }
    }

    private calculateNAS(trialIndex: number, particleCounts: { [channel: number]: number }): void {
        // This method is now handled by the lookup display component
        // but we keep it for backward compatibility
        this.updateNasCalculationState(trialIndex, true, null);

        const request: NasLookupRequest = { particleCounts };

        // Use the new lookup service
        this.lookupService.calculateNAS(request).pipe(
            takeUntil(this.destroy$),
            debounceTime(300) // Debounce to avoid too many API calls
        ).subscribe({
            next: (result: NasLookupResult) => {
                if (result.isValid) {
                    const trialGroup = this.trialsArray.at(trialIndex);
                    trialGroup.get('calculatedNAS')?.setValue(result.highestNAS);
                    this.updateNasCalculationState(trialIndex, false, null);
                } else {
                    this.updateNasCalculationState(trialIndex, false, result.errorMessage || 'NAS calculation failed');
                }
            },
            error: (error) => {
                this.updateNasCalculationState(trialIndex, false, `Error calculating NAS: ${error.message}`);
            }
        });
    }

    private updateNasCalculationState(trialIndex: number, inProgress: boolean, error: string | null): void {
        const currentStates = this.nasCalculationStates();
        currentStates[trialIndex] = { inProgress, error };
        this.nasCalculationStates.set({ ...currentStates });
    }

    nasCalculationInProgress(trialIndex: number): boolean {
        return this.nasCalculationStates()[trialIndex]?.inProgress || false;
    }

    nasCalculationError(trialIndex: number): string | null {
        return this.nasCalculationStates()[trialIndex]?.error || null;
    }

    onTrialCompleteChange(trialIndex: number): void {
        const trialGroup = this.trialsArray.at(trialIndex);
        const isComplete = trialGroup.get('isComplete')?.value;

        if (isComplete) {
            // Validate that at least one particle count is filled
            const hasAnyCount = [
                'count5to10', 'count10to15', 'count15to25',
                'count25to50', 'count50to100', 'countOver100'
            ].some(field => {
                const value = trialGroup.get(field)?.value;
                return value !== null && value >= 0;
            });

            if (!hasAnyCount) {
                trialGroup.get('isComplete')?.setValue(false);
                this.snackBar.open('Please enter at least one particle count before marking trial as complete', 'Close', {
                    duration: 3000
                });
            }
        }
    }

    onFileUploaded(trialIndex: number, fileInfo: any): void {
        this.snackBar.open(`File uploaded successfully for Trial ${trialIndex + 1}`, 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar']
        });
    }

    getTrialControl(trialIndex: number, controlName: string): AbstractControl | null {
        return this.trialsArray.at(trialIndex)?.get(controlName) || null;
    }

    onSave(): void {
        if (!this.particleCountForm.valid) {
            this.markFormGroupTouched(this.particleCountForm);
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

        const formValue = this.particleCountForm.value;
        const request: SaveTestResultRequest = {
            sampleId: sample.id,
            testId: this.testId,
            entryId: 'USER', // This should come from authentication
            comments: formValue.comments,
            trials: formValue.trials.map((trial: any, index: number) => ({
                trialNumber: index + 1,
                values: {
                    count5to10: trial.count5to10,
                    count10to15: trial.count10to15,
                    count15to25: trial.count15to25,
                    count25to50: trial.count25to50,
                    count50to100: trial.count50to100,
                    countOver100: trial.countOver100
                },
                calculatedResult: trial.calculatedNAS,
                isComplete: trial.isComplete
            }))
        };

        this.testService.saveTestResults(this.testId, request).subscribe({
            next: (response) => {
                this.snackBar.open(`Successfully saved ${response.recordsSaved} trial records`, 'Close', {
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
                this.particleCountForm.reset();
                this.initializeForm();
                this.nasCalculationStates.set({});
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
                message: 'Are you sure you want to delete all test results for this sample? This action cannot be undone.',
                confirmText: 'Delete',
                cancelText: 'Cancel'
            }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result && sample) {
                this.testService.deleteTestResults(this.testId, sample.id).subscribe({
                    next: (response) => {
                        this.snackBar.open(`Successfully deleted ${response.recordsDeleted} records`, 'Close', {
                            duration: 3000,
                            panelClass: ['success-snackbar']
                        });
                        this.particleCountForm.reset();
                        this.initializeForm();
                        this.nasCalculationStates.set({});
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
    }

    hasExistingResults(): boolean {
        return this.testResult() !== null;
    }

    getStatusText(status: string): string {
        switch (status) {
            case 'C': return 'Complete';
            case 'E': return 'In Progress';
            case 'X': return 'Pending';
            default: return 'Unknown';
        }
    }

    // New methods for lookup display component integration
    getParticleCountsForTrial(trialIndex: number): { [channel: number]: number } | undefined {
        const trialGroup = this.trialsArray.at(trialIndex);
        if (!trialGroup) return undefined;

        const particleCounts: { [channel: number]: number } = {};
        const count5to10 = trialGroup.get('count5to10')?.value;
        const count10to15 = trialGroup.get('count10to15')?.value;
        const count15to25 = trialGroup.get('count15to25')?.value;
        const count25to50 = trialGroup.get('count25to50')?.value;
        const count50to100 = trialGroup.get('count50to100')?.value;
        const countOver100 = trialGroup.get('countOver100')?.value;

        // Map to channels (1-6 for the size ranges)
        if (count5to10 !== null && count5to10 >= 0) particleCounts[1] = count5to10;
        if (count10to15 !== null && count10to15 >= 0) particleCounts[2] = count10to15;
        if (count15to25 !== null && count15to25 >= 0) particleCounts[3] = count15to25;
        if (count25to50 !== null && count25to50 >= 0) particleCounts[4] = count25to50;
        if (count50to100 !== null && count50to100 >= 0) particleCounts[5] = count50to100;
        if (countOver100 !== null && countOver100 >= 0) particleCounts[6] = countOver100;

        return Object.keys(particleCounts).length > 0 ? particleCounts : undefined;
    }

    onNasLookupResult(trialIndex: number, result: NasLookupResult): void {
        if (result.isValid) {
            const trialGroup = this.trialsArray.at(trialIndex);
            trialGroup.get('calculatedNAS')?.setValue(result.highestNAS);
            this.updateNasCalculationState(trialIndex, false, null);
        } else {
            this.updateNasCalculationState(trialIndex, false, result.errorMessage || 'NAS calculation failed');
        }
    }

    onNasLookupError(trialIndex: number, error: string): void {
        this.updateNasCalculationState(trialIndex, false, error);
        this.snackBar.open(`NAS calculation error for Trial ${trialIndex + 1}: ${error}`, 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar']
        });
    }

    private markFormGroupTouched(formGroup: FormGroup): void {
        Object.keys(formGroup.controls).forEach(key => {
            const control = formGroup.get(key);
            control?.markAsTouched();

            if (control instanceof FormGroup) {
                this.markFormGroupTouched(control);
            } else if (control instanceof FormArray) {
                control.controls.forEach(arrayControl => {
                    if (arrayControl instanceof FormGroup) {
                        this.markFormGroupTouched(arrayControl);
                    }
                });
            }
        });
    }
}