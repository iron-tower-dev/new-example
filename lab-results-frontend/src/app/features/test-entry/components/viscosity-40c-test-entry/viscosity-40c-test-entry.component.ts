import { Component, OnInit, OnDestroy, inject, signal, computed, effect } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';

import { SampleService } from '../../../../shared/services/sample.service';
import { TestService } from '../../../../shared/services/test.service';
import { EquipmentService } from '../../../../shared/services/equipment.service';
import { ValidationService } from '../../../../shared/services/validation.service';
import { Sample } from '../../../../shared/models/sample.model';
import {
    TestTemplate,
    SaveTestResultRequest,
    TestCalculationRequest
} from '../../../../shared/models/test.model';
import { Equipment } from '../../../../shared/models/equipment.model';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { SharedModule } from '../../../../shared/shared.module';

@Component({
    selector: 'app-viscosity-40c-test-entry',
    standalone: true,
    imports: [SharedModule],
    template: `
        <div class="viscosity-test-container">
            <!-- Header Section -->
            <mat-card class="header-card">
                <mat-card-header>
                    <mat-card-title>Viscosity @ 40°C Test Entry</mat-card-title>
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
            @if (viscosityForm && !isLoading()) {
                <form [formGroup]="viscosityForm" (ngSubmit)="onSave()">
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
                            <mat-card-title>Trial Data Entry</mat-card-title>
                            <mat-card-subtitle>Enter equipment selections and stopwatch times for each trial</mat-card-subtitle>
                        </mat-card-header>
                        <mat-card-content>
                            <div class="trials-container" formArrayName="trials">
                                @for (trial of trialsArray.controls; track $index) {
                                    <div class="trial-row" [formGroupName]="$index">
                                        <div class="trial-header">
                                            <h4>Trial {{ $index + 1 }}</h4>
                                            <mat-checkbox 
                                                [formControlName]="'isComplete'"
                                                (change)="onTrialCompleteChange($index)">
                                                Complete
                                            </mat-checkbox>
                                        </div>
                                        
                                        <div class="trial-fields">
                                            <mat-form-field appearance="outline">
                                                <mat-label>Thermometer</mat-label>
                                                <mat-select 
                                                    formControlName="thermometerId"
                                                    (selectionChange)="onEquipmentChange($index)">
                                                    @for (thermometer of thermometers(); track thermometer.id) {
                                                        <mat-option [value]="thermometer.id">
                                                            {{ thermometer.equipName }} - {{ thermometer.equipType }}
                                                        </mat-option>
                                                    }
                                                </mat-select>
                                                <mat-hint>Select thermometer for temperature measurement</mat-hint>
                                                @if (getTrialControl($index, 'thermometerId')?.errors?.['required']) {
                                                    <mat-error>Thermometer selection is required</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Stopwatch</mat-label>
                                                <mat-select 
                                                    formControlName="stopwatchId"
                                                    (selectionChange)="onEquipmentChange($index)">
                                                    @for (stopwatch of stopwatches(); track stopwatch.id) {
                                                        <mat-option [value]="stopwatch.id">
                                                            {{ stopwatch.equipName }} - {{ stopwatch.equipType }}
                                                        </mat-option>
                                                    }
                                                </mat-select>
                                                <mat-hint>Select stopwatch for timing</mat-hint>
                                                @if (getTrialControl($index, 'stopwatchId')?.errors?.['required']) {
                                                    <mat-error>Stopwatch selection is required</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Tube ID</mat-label>
                                                <mat-select 
                                                    formControlName="tubeId"
                                                    (selectionChange)="onTubeChange($index)">
                                                    @for (tube of tubes(); track tube.id) {
                                                        <mat-option [value]="tube.id">
                                                            {{ tube.equipName }} - Cal: {{ tube.calibrationValue }}
                                                        </mat-option>
                                                    }
                                                </mat-select>
                                                <mat-hint>Select viscometer tube</mat-hint>
                                                @if (getTrialControl($index, 'tubeId')?.errors?.['required']) {
                                                    <mat-error>Tube ID selection is required</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Stopwatch Time (seconds)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.01"
                                                    min="0.01"
                                                    formControlName="stopwatchTime"
                                                    (input)="onTrialValueChange($index)"
                                                    placeholder="Enter stopwatch time">
                                                <mat-hint>Time in seconds</mat-hint>
                                                @if (getTrialControl($index, 'stopwatchTime')?.errors?.['required']) {
                                                    <mat-error>Stopwatch time is required</mat-error>
                                                }
                                                @if (getTrialControl($index, 'stopwatchTime')?.errors?.['min']) {
                                                    <mat-error>Stopwatch time must be greater than 0</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Tube Calibration</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    formControlName="tubeCalibration"
                                                    readonly
                                                    placeholder="Auto-filled from tube selection">
                                                <mat-hint>Calibration value from selected tube</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Viscosity Result (cSt)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    formControlName="viscosityResult"
                                                    readonly
                                                    placeholder="Calculated automatically">
                                                <mat-hint>Calculated: Stopwatch Time × Tube Calibration</mat-hint>
                                            </mat-form-field>
                                        </div>

                                        <!-- Repeatability Validation for Q/QAG samples -->
                                        @if (selectedSample()?.qualityClass === 'Q' || selectedSample()?.qualityClass === 'QAG') {
                                            <div class="repeatability-section">
                                                <mat-card class="repeatability-card">
                                                    <mat-card-content>
                                                        <h5>Repeatability Check (Q/QAG Sample)</h5>
                                                        @if (getRepeatabilityStatus($index); as status) {
                                                            <div class="repeatability-status" [class]="'status-' + status.type">
                                                                <mat-icon>{{ status.icon }}</mat-icon>
                                                                <span>{{ status.message }}</span>
                                                            </div>
                                                        }
                                                    </mat-card-content>
                                                </mat-card>
                                            </div>
                                        }
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
                            [disabled]="!viscosityForm.valid || isLoading()">
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
                        <mat-card-title>Last 12 Results for Viscosity @ 40°C Test</mat-card-title>
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

                                <ng-container matColumnDef="results">
                                    <th mat-header-cell *matHeaderCellDef>Results (cSt)</th>
                                    <td mat-cell *matCellDef="let result">
                                        @for (trial of result.trials; track trial.trialNumber) {
                                            @if (trial.calculatedResult) {
                                                <span class="trial-result">
                                                    T{{ trial.trialNumber }}: {{ trial.calculatedResult | number:'1.2-2' }}
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
        .viscosity-test-container {
            padding: 20px;
            max-width: 1200px;
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

        .trial-fields {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 16px;
        }

        .repeatability-section {
            margin-top: 16px;
        }

        .repeatability-card {
            background-color: #f5f5f5;
        }

        .repeatability-status {
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .status-pass {
            color: #4caf50;
        }

        .status-fail {
            color: #f44336;
        }

        .status-warning {
            color: #ff9800;
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
            .viscosity-test-container {
                padding: 10px;
            }
            
            .trial-fields {
                grid-template-columns: 1fr;
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
export class Viscosity40cTestEntryComponent implements OnInit, OnDestroy {
    private fb = inject(FormBuilder);
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private snackBar = inject(MatSnackBar);
    private dialog = inject(MatDialog);
    private sampleService = inject(SampleService);
    private testService = inject(TestService);
    private equipmentService = inject(EquipmentService);
    private validationService = inject(ValidationService);
    private destroy$ = new Subject<void>();

    // Signals from services
    readonly selectedSample = this.sampleService.selectedSample;
    readonly testTemplate = this.testService.testTemplate;
    readonly testResult = this.testService.testResult;
    readonly testResultHistory = this.testService.testResultHistory;
    readonly thermometers = this.equipmentService.thermometers;
    readonly stopwatches = this.equipmentService.stopwatches;
    readonly tubes = this.equipmentService.tubes;
    readonly isLoading = computed(() =>
        this.sampleService.isLoading() || this.testService.isLoading() || this.equipmentService.isLoading()
    );
    readonly error = computed(() =>
        this.sampleService.error() || this.testService.error() || this.equipmentService.error()
    );
    readonly hasError = computed(() => this.error() !== null);

    // Component state
    viscosityForm!: FormGroup;
    testId = 5; // Viscosity @ 40°C test ID - this should come from route or configuration
    historyDisplayedColumns = ['sampleId', 'entryDate', 'status', 'results'];

    // Form getters
    get trialsArray(): FormArray {
        return this.viscosityForm.get('trials') as FormArray;
    }

    constructor() {
        // Effect to handle sample changes
        effect(() => {
            const sample = this.selectedSample();
            if (sample && this.viscosityForm) {
                this.loadExistingResults(sample.id);
                this.loadTestHistory(sample.id);
            }
        });

        // Effect to handle template changes
        effect(() => {
            const template = this.testTemplate();
            if (template && !this.viscosityForm) {
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

        // Load test template and equipment
        this.testService.getTestTemplate(this.testId).subscribe();
        this.loadEquipment();
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

    private loadEquipment(): void {
        this.equipmentService.getThermometers().subscribe();
        this.equipmentService.getStopwatches().subscribe();
        this.equipmentService.getTubes().subscribe();
    }

    private loadExistingResults(sampleId: number): void {
        this.testService.getTestResults(this.testId, sampleId).subscribe({
            next: (result) => {
                if (result && this.viscosityForm) {
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
        this.viscosityForm = this.fb.group({
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
            thermometerId: [null, [Validators.required]],
            stopwatchId: [null, [Validators.required]],
            tubeId: [null, [Validators.required]],
            stopwatchTime: [null, [Validators.required, Validators.min(0.01)]],
            tubeCalibration: [{ value: null, disabled: true }],
            viscosityResult: [{ value: null, disabled: true }],
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
                thermometerId: [trial.values.thermometerId || null, [Validators.required]],
                stopwatchId: [trial.values.stopwatchId || null, [Validators.required]],
                tubeId: [trial.values.tubeId || null, [Validators.required]],
                stopwatchTime: [trial.values.stopwatchTime || null, [Validators.required, Validators.min(0.01)]],
                tubeCalibration: [{ value: trial.values.tubeCalibration || null, disabled: true }],
                viscosityResult: [{ value: trial.calculatedResult || null, disabled: true }],
                isComplete: [trial.isComplete || false]
            });
            this.trialsArray.push(trialGroup);
        });

        // Set comments
        this.viscosityForm.patchValue({
            comments: result.comments || ''
        });
    }

    onEquipmentChange(trialIndex: number): void {
        // Equipment selection changed - no immediate calculation needed
        // Calculation happens when stopwatch time is entered
    }

    onTubeChange(trialIndex: number): void {
        const trialGroup = this.trialsArray.at(trialIndex);
        const tubeId = trialGroup.get('tubeId')?.value;

        if (tubeId) {
            // Find the selected tube and set its calibration value
            const selectedTube = this.tubes().find(tube => tube.id === tubeId);
            if (selectedTube) {
                trialGroup.get('tubeCalibration')?.setValue(selectedTube.calibrationValue);
                // Recalculate if stopwatch time is already entered
                this.onTrialValueChange(trialIndex);
            }
        } else {
            trialGroup.get('tubeCalibration')?.setValue(null);
            trialGroup.get('viscosityResult')?.setValue(null);
        }
    }

    onTrialValueChange(trialIndex: number): void {
        const trialGroup = this.trialsArray.at(trialIndex);
        const stopwatchTime = trialGroup.get('stopwatchTime')?.value;
        const tubeCalibration = trialGroup.get('tubeCalibration')?.value;

        if (stopwatchTime && tubeCalibration && stopwatchTime > 0 && tubeCalibration > 0) {
            // Calculate Viscosity: Stopwatch Time * Tube Calibration
            let viscosityResult = stopwatchTime * tubeCalibration;

            // Round to 2 decimal places
            viscosityResult = Math.round(viscosityResult * 100) / 100;

            trialGroup.get('viscosityResult')?.setValue(viscosityResult);
        } else {
            trialGroup.get('viscosityResult')?.setValue(null);
        }
    }

    onTrialCompleteChange(trialIndex: number): void {
        const trialGroup = this.trialsArray.at(trialIndex);
        const isComplete = trialGroup.get('isComplete')?.value;

        if (isComplete) {
            // Validate that required fields are filled
            const thermometerId = trialGroup.get('thermometerId')?.value;
            const stopwatchId = trialGroup.get('stopwatchId')?.value;
            const tubeId = trialGroup.get('tubeId')?.value;
            const stopwatchTime = trialGroup.get('stopwatchTime')?.value;

            if (!thermometerId || !stopwatchId || !tubeId || !stopwatchTime) {
                trialGroup.get('isComplete')?.setValue(false);
                this.snackBar.open('Please fill in all required fields before marking trial as complete', 'Close', {
                    duration: 3000
                });
            }
        }
    }

    getTrialControl(trialIndex: number, controlName: string): AbstractControl | null {
        return this.trialsArray.at(trialIndex)?.get(controlName) || null;
    }

    getRepeatabilityStatus(trialIndex: number): { type: string; icon: string; message: string } | null {
        // For Q/QAG samples, check repeatability between trials
        const sample = this.selectedSample();
        if (!sample || (sample.qualityClass !== 'Q' && sample.qualityClass !== 'QAG')) {
            return null;
        }

        // Get completed trials with results
        const completedTrials = this.trialsArray.controls
            .map((control, index) => ({
                index,
                result: control.get('viscosityResult')?.value,
                isComplete: control.get('isComplete')?.value
            }))
            .filter(trial => trial.isComplete && trial.result);

        if (completedTrials.length < 2) {
            return {
                type: 'warning',
                icon: 'info',
                message: 'Need at least 2 completed trials for repeatability check'
            };
        }

        // Calculate repeatability (typically within 5% for viscosity)
        const results = completedTrials.map(trial => trial.result);
        const average = results.reduce((sum, val) => sum + val, 0) / results.length;
        const maxDeviation = Math.max(...results.map(val => Math.abs(val - average) / average * 100));

        if (maxDeviation <= 5) {
            return {
                type: 'pass',
                icon: 'check_circle',
                message: `Repeatability: ${maxDeviation.toFixed(1)}% (≤5% - PASS)`
            };
        } else {
            return {
                type: 'fail',
                icon: 'error',
                message: `Repeatability: ${maxDeviation.toFixed(1)}% (>5% - FAIL)`
            };
        }
    }

    onSave(): void {
        if (!this.viscosityForm.valid) {
            this.markFormGroupTouched(this.viscosityForm);
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

        const formValue = this.viscosityForm.value;
        const request: SaveTestResultRequest = {
            sampleId: sample.id,
            testId: this.testId,
            entryId: 'USER', // This should come from authentication
            comments: formValue.comments,
            trials: formValue.trials.map((trial: any, index: number) => ({
                trialNumber: index + 1,
                values: {
                    thermometerId: trial.thermometerId,
                    stopwatchId: trial.stopwatchId,
                    tubeId: trial.tubeId,
                    stopwatchTime: trial.stopwatchTime,
                    tubeCalibration: trial.tubeCalibration
                },
                calculatedResult: trial.viscosityResult,
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
                this.viscosityForm.reset();
                this.initializeForm();
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
                        this.viscosityForm.reset();
                        this.initializeForm();
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
        this.equipmentService.clearError();
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