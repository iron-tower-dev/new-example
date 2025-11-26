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
    selector: 'app-grease-dropping-point-test-entry',
    standalone: true,
    imports: [SharedModule],
    template: `
        <div class="grease-dropping-point-test-container">
            <!-- Header Section -->
            <mat-card class="header-card">
                <mat-card-header>
                    <mat-card-title>Grease Dropping Point Test Entry</mat-card-title>
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
            @if (greaseDroppingPointForm && !isLoading()) {
                <form [formGroup]="greaseDroppingPointForm" (ngSubmit)="onSave()">
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
                            <mat-card-subtitle>Enter thermometer selections and temperature readings for each trial</mat-card-subtitle>
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
                                                <mat-label>Thermometer 1</mat-label>
                                                <mat-select 
                                                    formControlName="thermometer1Id"
                                                    (selectionChange)="onThermometerChange($index)">
                                                    @for (thermometer of thermometers(); track thermometer.id) {
                                                        <mat-option [value]="thermometer.id">
                                                            {{ thermometer.equipName }} - {{ thermometer.equipType }}
                                                        </mat-option>
                                                    }
                                                </mat-select>
                                                <mat-hint>Select first thermometer</mat-hint>
                                                @if (getTrialControl($index, 'thermometer1Id')?.errors?.['required']) {
                                                    <mat-error>Thermometer 1 selection is required</mat-error>
                                                }
                                                @if (getTrialControl($index, 'thermometer1Id')?.errors?.['sameThermometer']) {
                                                    <mat-error>Cannot select the same thermometer twice</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Thermometer 2</mat-label>
                                                <mat-select 
                                                    formControlName="thermometer2Id"
                                                    (selectionChange)="onThermometerChange($index)">
                                                    @for (thermometer of thermometers(); track thermometer.id) {
                                                        <mat-option [value]="thermometer.id">
                                                            {{ thermometer.equipName }} - {{ thermometer.equipType }}
                                                        </mat-option>
                                                    }
                                                </mat-select>
                                                <mat-hint>Select second thermometer (must be different from first)</mat-hint>
                                                @if (getTrialControl($index, 'thermometer2Id')?.errors?.['required']) {
                                                    <mat-error>Thermometer 2 selection is required</mat-error>
                                                }
                                                @if (getTrialControl($index, 'thermometer2Id')?.errors?.['sameThermometer']) {
                                                    <mat-error>Cannot select the same thermometer twice</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Dropping Point (°F)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    formControlName="droppingPoint"
                                                    (input)="onTrialValueChange($index)"
                                                    placeholder="Enter dropping point temperature">
                                                <mat-hint>Temperature at which grease drops</mat-hint>
                                                @if (getTrialControl($index, 'droppingPoint')?.errors?.['required']) {
                                                    <mat-error>Dropping point is required</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Block Temperature (°F)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    formControlName="blockTemp"
                                                    (input)="onTrialValueChange($index)"
                                                    placeholder="Enter block temperature">
                                                <mat-hint>Temperature of the heating block</mat-hint>
                                                @if (getTrialControl($index, 'blockTemp')?.errors?.['required']) {
                                                    <mat-error>Block temperature is required</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Corrected Dropping Point (°F)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    formControlName="correctedResult"
                                                    readonly
                                                    placeholder="Calculated automatically">
                                                <mat-hint>Calculated: Dropping Point + ((Block Temp - Dropping Point) ÷ 3)</mat-hint>
                                            </mat-form-field>
                                        </div>

                                        <!-- Thermometer Validation Warning -->
                                        @if (getThermometerValidationStatus($index); as status) {
                                            <div class="thermometer-validation-section">
                                                <mat-card class="thermometer-validation-card">
                                                    <mat-card-content>
                                                        <div class="thermometer-validation-status" [class]="'status-' + status.type">
                                                            <mat-icon>{{ status.icon }}</mat-icon>
                                                            <span>{{ status.message }}</span>
                                                        </div>
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
                            [disabled]="!greaseDroppingPointForm.valid || isLoading()">
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
                        <mat-card-title>Last 12 Results for Grease Dropping Point Test</mat-card-title>
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
                                    <th mat-header-cell *matHeaderCellDef>Results (°F)</th>
                                    <td mat-cell *matCellDef="let result">
                                        @for (trial of result.trials; track trial.trialNumber) {
                                            @if (trial.calculatedResult) {
                                                <span class="trial-result">
                                                    T{{ trial.trialNumber }}: {{ trial.calculatedResult | number:'1.1-1' }}°F
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
        .grease-dropping-point-test-container {
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

        .thermometer-validation-section {
            margin-top: 16px;
        }

        .thermometer-validation-card {
            background-color: #f5f5f5;
        }

        .thermometer-validation-status {
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .status-success {
            color: #4caf50;
        }

        .status-error {
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
            .grease-dropping-point-test-container {
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
export class GreaseDroppingPointTestEntryComponent implements OnInit, OnDestroy {
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
    readonly isLoading = computed(() =>
        this.sampleService.isLoading() || this.testService.isLoading() || this.equipmentService.isLoading()
    );
    readonly error = computed(() =>
        this.sampleService.error() || this.testService.error() || this.equipmentService.error()
    );
    readonly hasError = computed(() => this.error() !== null);

    // Component state
    greaseDroppingPointForm!: FormGroup;
    testId = 131; // Grease Dropping Point test ID - this should come from route or configuration
    historyDisplayedColumns = ['sampleId', 'entryDate', 'status', 'results'];

    // Form getters
    get trialsArray(): FormArray {
        return this.greaseDroppingPointForm.get('trials') as FormArray;
    }

    constructor() {
        // Effect to handle sample changes
        effect(() => {
            const sample = this.selectedSample();
            if (sample && this.greaseDroppingPointForm) {
                this.loadExistingResults(sample.id);
                this.loadTestHistory(sample.id);
            }
        });

        // Effect to handle template changes
        effect(() => {
            const template = this.testTemplate();
            if (template && !this.greaseDroppingPointForm) {
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
    }

    private loadExistingResults(sampleId: number): void {
        this.testService.getTestResults(this.testId, sampleId).subscribe({
            next: (result) => {
                if (result && this.greaseDroppingPointForm) {
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
        this.greaseDroppingPointForm = this.fb.group({
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
            thermometer1Id: [null, [Validators.required]],
            thermometer2Id: [null, [Validators.required]],
            droppingPoint: [null, [Validators.required]],
            blockTemp: [null, [Validators.required]],
            correctedResult: [{ value: null, disabled: true }],
            isComplete: [false]
        }, { validators: this.thermometerValidator });

        this.trialsArray.push(trialGroup);
    }

    private thermometerValidator(group: AbstractControl): { [key: string]: any } | null {
        const thermometer1Id = group.get('thermometer1Id')?.value;
        const thermometer2Id = group.get('thermometer2Id')?.value;

        if (thermometer1Id && thermometer2Id && thermometer1Id === thermometer2Id) {
            // Set individual control errors
            group.get('thermometer1Id')?.setErrors({ sameThermometer: true });
            group.get('thermometer2Id')?.setErrors({ sameThermometer: true });
            return { sameThermometer: true };
        } else {
            // Clear individual control errors if they were set by this validator
            const therm1Control = group.get('thermometer1Id');
            const therm2Control = group.get('thermometer2Id');

            if (therm1Control?.errors?.['sameThermometer']) {
                delete therm1Control.errors['sameThermometer'];
                if (Object.keys(therm1Control.errors).length === 0) {
                    therm1Control.setErrors(null);
                }
            }

            if (therm2Control?.errors?.['sameThermometer']) {
                delete therm2Control.errors['sameThermometer'];
                if (Object.keys(therm2Control.errors).length === 0) {
                    therm2Control.setErrors(null);
                }
            }
        }

        return null;
    }

    private populateFormWithResults(result: any): void {
        // Clear existing trials
        while (this.trialsArray.length > 0) {
            this.trialsArray.removeAt(0);
        }

        // Add trials from result
        result.trials.forEach((trial: any) => {
            const trialGroup = this.fb.group({
                thermometer1Id: [trial.values.thermometer1Id || null, [Validators.required]],
                thermometer2Id: [trial.values.thermometer2Id || null, [Validators.required]],
                droppingPoint: [trial.values.droppingPoint || null, [Validators.required]],
                blockTemp: [trial.values.blockTemp || null, [Validators.required]],
                correctedResult: [{ value: trial.calculatedResult || null, disabled: true }],
                isComplete: [trial.isComplete || false]
            }, { validators: this.thermometerValidator });
            this.trialsArray.push(trialGroup);
        });

        // Set comments
        this.greaseDroppingPointForm.patchValue({
            comments: result.comments || ''
        });
    }

    onTrialValueChange(trialIndex: number): void {
        const trialGroup = this.trialsArray.at(trialIndex);
        const droppingPoint = trialGroup.get('droppingPoint')?.value;
        const blockTemp = trialGroup.get('blockTemp')?.value;

        if (droppingPoint !== null && blockTemp !== null) {
            // Calculate grease dropping point: Dropping Point + ((Block Temp - Dropping Point) / 3)
            let result = droppingPoint + ((blockTemp - droppingPoint) / 3);

            // Round to 1 decimal place
            result = Math.round(result * 10) / 10;

            trialGroup.get('correctedResult')?.setValue(result);
        } else {
            trialGroup.get('correctedResult')?.setValue(null);
        }
    }

    onThermometerChange(trialIndex: number): void {
        // Trigger validation when thermometer selection changes
        const trialGroup = this.trialsArray.at(trialIndex);
        trialGroup.updateValueAndValidity();
    }

    getThermometerValidationStatus(trialIndex: number): { type: string; icon: string; message: string } | null {
        const trialGroup = this.trialsArray.at(trialIndex);
        const thermometer1Id = trialGroup.get('thermometer1Id')?.value;
        const thermometer2Id = trialGroup.get('thermometer2Id')?.value;

        if (!thermometer1Id || !thermometer2Id) {
            return null;
        }

        if (thermometer1Id === thermometer2Id) {
            return {
                type: 'error',
                icon: 'error',
                message: 'Both thermometers cannot be the same. Please select different thermometers.'
            };
        } else {
            return {
                type: 'success',
                icon: 'check_circle',
                message: 'Thermometer selection is valid.'
            };
        }
    }

    onTrialCompleteChange(trialIndex: number): void {
        // Additional logic when trial completion status changes
        const trialGroup = this.trialsArray.at(trialIndex);
        const isComplete = trialGroup.get('isComplete')?.value;

        if (isComplete) {
            // Validate that required fields are filled
            const thermometer1Id = trialGroup.get('thermometer1Id')?.value;
            const thermometer2Id = trialGroup.get('thermometer2Id')?.value;
            const droppingPoint = trialGroup.get('droppingPoint')?.value;
            const blockTemp = trialGroup.get('blockTemp')?.value;

            if (!thermometer1Id || !thermometer2Id || droppingPoint === null || blockTemp === null) {
                trialGroup.get('isComplete')?.setValue(false);
                this.snackBar.open('Please fill in all required fields before marking trial as complete', 'Close', {
                    duration: 3000
                });
            } else if (thermometer1Id === thermometer2Id) {
                trialGroup.get('isComplete')?.setValue(false);
                this.snackBar.open('Please select different thermometers before marking trial as complete', 'Close', {
                    duration: 3000
                });
            }
        }
    }

    getTrialControl(trialIndex: number, controlName: string): AbstractControl | null {
        return this.trialsArray.at(trialIndex)?.get(controlName) || null;
    }

    onSave(): void {
        if (!this.greaseDroppingPointForm.valid) {
            this.markFormGroupTouched(this.greaseDroppingPointForm);
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

        const formValue = this.greaseDroppingPointForm.value;
        const request: SaveTestResultRequest = {
            sampleId: sample.id,
            testId: this.testId,
            entryId: 'USER', // This should come from authentication
            comments: formValue.comments,
            trials: formValue.trials.map((trial: any, index: number) => ({
                trialNumber: index + 1,
                values: {
                    thermometer1Id: trial.thermometer1Id,
                    thermometer2Id: trial.thermometer2Id,
                    droppingPoint: trial.droppingPoint,
                    blockTemp: trial.blockTemp
                },
                calculatedResult: trial.correctedResult,
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
                this.greaseDroppingPointForm.reset();
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
                        this.greaseDroppingPointForm.reset();
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