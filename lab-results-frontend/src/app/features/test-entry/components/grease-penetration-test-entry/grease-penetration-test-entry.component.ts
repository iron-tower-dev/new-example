import { Component, OnInit, OnDestroy, inject, signal, computed, effect } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';

import { SampleService } from '../../../../shared/services/sample.service';
import { TestService } from '../../../../shared/services/test.service';
import { ValidationService } from '../../../../shared/services/validation.service';
import { Sample } from '../../../../shared/models/sample.model';
import {
    TestTemplate,
    SaveTestResultRequest,
    TestCalculationRequest
} from '../../../../shared/models/test.model';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { SharedModule } from '../../../../shared/shared.module';

@Component({
    selector: 'app-grease-penetration-test-entry',
    standalone: true,
    imports: [SharedModule],
    template: `
        <div class="grease-penetration-test-container">
            <!-- Header Section -->
            <mat-card class="header-card">
                <mat-card-header>
                    <mat-card-title>Grease Penetration Worked Test Entry</mat-card-title>
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
            @if (greasePenetrationForm && !isLoading()) {
                <form [formGroup]="greasePenetrationForm" (ngSubmit)="onSave()">
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
                            <mat-card-subtitle>Enter penetration values for each trial</mat-card-subtitle>
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
                                                <mat-label>1st Penetration</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="1"
                                                    min="0"
                                                    formControlName="pen1"
                                                    (input)="onTrialValueChange($index)"
                                                    placeholder="Enter 1st penetration value">
                                                <mat-hint>First penetration reading</mat-hint>
                                                @if (getTrialControl($index, 'pen1')?.errors?.['required']) {
                                                    <mat-error>1st penetration is required</mat-error>
                                                }
                                                @if (getTrialControl($index, 'pen1')?.errors?.['min']) {
                                                    <mat-error>Penetration value must be 0 or greater</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>2nd Penetration</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="1"
                                                    min="0"
                                                    formControlName="pen2"
                                                    (input)="onTrialValueChange($index)"
                                                    placeholder="Enter 2nd penetration value">
                                                <mat-hint>Second penetration reading</mat-hint>
                                                @if (getTrialControl($index, 'pen2')?.errors?.['required']) {
                                                    <mat-error>2nd penetration is required</mat-error>
                                                }
                                                @if (getTrialControl($index, 'pen2')?.errors?.['min']) {
                                                    <mat-error>Penetration value must be 0 or greater</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>3rd Penetration</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="1"
                                                    min="0"
                                                    formControlName="pen3"
                                                    (input)="onTrialValueChange($index)"
                                                    placeholder="Enter 3rd penetration value">
                                                <mat-hint>Third penetration reading</mat-hint>
                                                @if (getTrialControl($index, 'pen3')?.errors?.['required']) {
                                                    <mat-error>3rd penetration is required</mat-error>
                                                }
                                                @if (getTrialControl($index, 'pen3')?.errors?.['min']) {
                                                    <mat-error>Penetration value must be 0 or greater</mat-error>
                                                }
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Calculated Result</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    formControlName="calculatedResult"
                                                    readonly
                                                    placeholder="Calculated automatically">
                                                <mat-hint>Calculated: ((Average of 3 penetrations) × 3.75) + 24</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>NLGI Grade</mat-label>
                                                <input 
                                                    matInput 
                                                    type="text" 
                                                    formControlName="nlgiGrade"
                                                    readonly
                                                    placeholder="Looked up automatically">
                                                <mat-hint>NLGI grade based on calculated result</mat-hint>
                                            </mat-form-field>
                                        </div>

                                        <!-- NLGI Lookup Status -->
                                        @if (getNLGILookupStatus($index); as status) {
                                            <div class="nlgi-lookup-section">
                                                <mat-card class="nlgi-lookup-card">
                                                    <mat-card-content>
                                                        <div class="nlgi-lookup-status" [class]="'status-' + status.type">
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
                            [disabled]="!greasePenetrationForm.valid || isLoading()">
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
                        <mat-card-title>Last 12 Results for Grease Penetration Test</mat-card-title>
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
                                    <th mat-header-cell *matHeaderCellDef>Results</th>
                                    <td mat-cell *matCellDef="let result">
                                        @for (trial of result.trials; track trial.trialNumber) {
                                            @if (trial.calculatedResult) {
                                                <span class="trial-result">
                                                    T{{ trial.trialNumber }}: {{ trial.calculatedResult | number:'1.0-0' }}
                                                    @if (trial.values?.nlgiGrade) {
                                                        ({{ trial.values.nlgiGrade }})
                                                    }
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
        .grease-penetration-test-container {
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

        .nlgi-lookup-section {
            margin-top: 16px;
        }

        .nlgi-lookup-card {
            background-color: #f5f5f5;
        }

        .nlgi-lookup-status {
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

        .status-info {
            color: #2196f3;
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
            .grease-penetration-test-container {
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
export class GreasePenetrationTestEntryComponent implements OnInit, OnDestroy {
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
    greasePenetrationForm!: FormGroup;
    testId = 130; // Grease Penetration test ID - this should come from route or configuration
    historyDisplayedColumns = ['sampleId', 'entryDate', 'status', 'results'];

    // Form getters
    get trialsArray(): FormArray {
        return this.greasePenetrationForm.get('trials') as FormArray;
    }

    constructor() {
        // Effect to handle sample changes
        effect(() => {
            const sample = this.selectedSample();
            if (sample && this.greasePenetrationForm) {
                this.loadExistingResults(sample.id);
                this.loadTestHistory(sample.id);
            }
        });

        // Effect to handle template changes
        effect(() => {
            const template = this.testTemplate();
            if (template && !this.greasePenetrationForm) {
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
                if (result && this.greasePenetrationForm) {
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
        this.greasePenetrationForm = this.fb.group({
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
            pen1: [null, [Validators.required, Validators.min(0)]],
            pen2: [null, [Validators.required, Validators.min(0)]],
            pen3: [null, [Validators.required, Validators.min(0)]],
            calculatedResult: [{ value: null, disabled: true }],
            nlgiGrade: [{ value: null, disabled: true }],
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
                pen1: [trial.values.pen1 || null, [Validators.required, Validators.min(0)]],
                pen2: [trial.values.pen2 || null, [Validators.required, Validators.min(0)]],
                pen3: [trial.values.pen3 || null, [Validators.required, Validators.min(0)]],
                calculatedResult: [{ value: trial.calculatedResult || null, disabled: true }],
                nlgiGrade: [{ value: trial.values.nlgiGrade || null, disabled: true }],
                isComplete: [trial.isComplete || false]
            });
            this.trialsArray.push(trialGroup);
        });

        // Set comments
        this.greasePenetrationForm.patchValue({
            comments: result.comments || ''
        });
    }

    onTrialValueChange(trialIndex: number): void {
        const trialGroup = this.trialsArray.at(trialIndex);
        const pen1 = trialGroup.get('pen1')?.value;
        const pen2 = trialGroup.get('pen2')?.value;
        const pen3 = trialGroup.get('pen3')?.value;

        if (pen1 !== null && pen2 !== null && pen3 !== null &&
            pen1 >= 0 && pen2 >= 0 && pen3 >= 0) {

            // Calculate grease penetration: ((Average of 3 penetrations) * 3.75) + 24
            const average = (pen1 + pen2 + pen3) / 3;
            let result = (average * 3.75) + 24;

            // Round to 0 decimal places
            result = Math.round(result);

            trialGroup.get('calculatedResult')?.setValue(result);

            // Perform NLGI lookup
            this.performNLGILookup(trialIndex, result);
        } else {
            trialGroup.get('calculatedResult')?.setValue(null);
            trialGroup.get('nlgiGrade')?.setValue(null);
        }
    }

    private performNLGILookup(trialIndex: number, penetrationValue: number): void {
        this.testService.getNLGIForPenetration(penetrationValue).subscribe({
            next: (response) => {
                const trialGroup = this.trialsArray.at(trialIndex);
                trialGroup.get('nlgiGrade')?.setValue(response.nlgi);
            },
            error: (error) => {
                console.warn('NLGI lookup failed:', error);
                const trialGroup = this.trialsArray.at(trialIndex);
                trialGroup.get('nlgiGrade')?.setValue('Not Found');
            }
        });
    }

    getNLGILookupStatus(trialIndex: number): { type: string; icon: string; message: string } | null {
        const trialGroup = this.trialsArray.at(trialIndex);
        const calculatedResult = trialGroup.get('calculatedResult')?.value;
        const nlgiGrade = trialGroup.get('nlgiGrade')?.value;

        if (calculatedResult === null) {
            return null;
        }

        if (nlgiGrade && nlgiGrade !== 'Not Found') {
            return {
                type: 'success',
                icon: 'check_circle',
                message: `NLGI Grade: ${nlgiGrade} (Penetration: ${calculatedResult})`
            };
        } else if (nlgiGrade === 'Not Found') {
            return {
                type: 'error',
                icon: 'error',
                message: `No NLGI grade found for penetration value ${calculatedResult}`
            };
        } else {
            return {
                type: 'info',
                icon: 'info',
                message: 'Looking up NLGI grade...'
            };
        }
    }

    onTrialCompleteChange(trialIndex: number): void {
        // Additional logic when trial completion status changes
        const trialGroup = this.trialsArray.at(trialIndex);
        const isComplete = trialGroup.get('isComplete')?.value;

        if (isComplete) {
            // Validate that required fields are filled
            const pen1 = trialGroup.get('pen1')?.value;
            const pen2 = trialGroup.get('pen2')?.value;
            const pen3 = trialGroup.get('pen3')?.value;

            if (pen1 === null || pen2 === null || pen3 === null) {
                trialGroup.get('isComplete')?.setValue(false);
                this.snackBar.open('Please fill in all penetration values before marking trial as complete', 'Close', {
                    duration: 3000
                });
            }
        }
    }

    getTrialControl(trialIndex: number, controlName: string): AbstractControl | null {
        return this.trialsArray.at(trialIndex)?.get(controlName) || null;
    }

    onSave(): void {
        if (!this.greasePenetrationForm.valid) {
            this.markFormGroupTouched(this.greasePenetrationForm);
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

        const formValue = this.greasePenetrationForm.value;
        const request: SaveTestResultRequest = {
            sampleId: sample.id,
            testId: this.testId,
            entryId: 'USER', // This should come from authentication
            comments: formValue.comments,
            trials: formValue.trials.map((trial: any, index: number) => ({
                trialNumber: index + 1,
                values: {
                    pen1: trial.pen1,
                    pen2: trial.pen2,
                    pen3: trial.pen3,
                    nlgiGrade: trial.nlgiGrade
                },
                calculatedResult: trial.calculatedResult,
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
                this.greasePenetrationForm.reset();
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
                        this.greasePenetrationForm.reset();
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