import { Component, OnInit, OnDestroy, inject, signal, computed, effect } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';

import { SampleService } from '../../../../shared/services/sample.service';
import { TestService } from '../../../../shared/services/test.service';
import { ValidationService } from '../../../../shared/services/validation.service';
import { Sample } from '../../../../shared/models/sample.model';
import {
    TestTemplate,
    TbnTrialData,
    SaveTestResultRequest
} from '../../../../shared/models/test.model';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { SharedModule } from '../../../../shared/shared.module';

@Component({
    selector: 'app-tbn-test-entry',
    standalone: true,
    imports: [SharedModule],
    template: `
        <div class="tbn-test-container">
            <!-- Header Section -->
            <mat-card class="header-card">
                <mat-card-header>
                    <mat-card-title>TBN by Auto Titration Test Entry</mat-card-title>
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
            @if (tbnForm && !isLoading()) {
                <form [formGroup]="tbnForm" (ngSubmit)="onSave()">
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
                            <mat-card-subtitle>Enter TBN results from auto titration analysis</mat-card-subtitle>
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
                                                <mat-label>TBN Result (mg KOH/g)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.01"
                                                    min="0"
                                                    formControlName="tbnResult"
                                                    placeholder="Enter TBN result">
                                                <mat-hint>Total Base Number in mg KOH/g</mat-hint>
                                                @if (getTrialControl($index, 'tbnResult')?.errors?.['required']) {
                                                    <mat-error>TBN result is required</mat-error>
                                                }
                                                @if (getTrialControl($index, 'tbnResult')?.errors?.['min']) {
                                                    <mat-error>TBN result must be 0 or greater</mat-error>
                                                }
                                                @if (getTrialControl($index, 'tbnResult')?.errors?.['pattern']) {
                                                    <mat-error>Please enter a valid number</mat-error>
                                                }
                                            </mat-form-field>

                                            <!-- Trial Selection Buttons -->
                                            <div class="trial-selection">
                                                <label>Select for Operations:</label>
                                                <mat-checkbox 
                                                    [checked]="isTrialSelected($index)"
                                                    (change)="onTrialSelectionChange($index, $event.checked)">
                                                    Select Trial {{ $index + 1 }}
                                                </mat-checkbox>
                                            </div>
                                        </div>
                                    </div>
                                }
                            </div>

                            <!-- Selected Trials Summary -->
                            @if (selectedTrials().length > 0) {
                                <div class="selected-trials-summary">
                                    <h5>Selected Trials for Operations:</h5>
                                    <div class="selected-trials-list">
                                        @for (trialIndex of selectedTrials(); track trialIndex) {
                                            <span class="selected-trial-chip">
                                                Trial {{ trialIndex + 1 }}
                                                <button 
                                                    mat-icon-button 
                                                    (click)="onTrialSelectionChange(trialIndex, false)"
                                                    aria-label="Remove trial selection">
                                                    <mat-icon>close</mat-icon>
                                                </button>
                                            </span>
                                        }
                                    </div>
                                </div>
                            }
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
                            [disabled]="!tbnForm.valid || isLoading()">
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
                            [disabled]="isLoading() || !hasExistingResults() || selectedTrials().length === 0">
                            <mat-icon>delete</mat-icon>
                            Delete Selected
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
                        <mat-card-title>Last 12 Results for TBN Test</mat-card-title>
                        <div class="history-controls">
                            <button 
                                mat-icon-button 
                                [class.active]="isHistoryResizable()"
                                (click)="toggleHistoryResize()"
                                matTooltip="Toggle resizable view">
                                <mat-icon>{{ isHistoryResizable() ? 'fullscreen_exit' : 'fullscreen' }}</mat-icon>
                            </button>
                            <button 
                                mat-icon-button 
                                [class.active]="isHistorySingleScreen()"
                                (click)="toggleHistorySingleScreen()"
                                matTooltip="Toggle single screen mode">
                                <mat-icon>{{ isHistorySingleScreen() ? 'view_module' : 'view_stream' }}</mat-icon>
                            </button>
                        </div>
                    </mat-card-header>
                    <mat-card-content [class.resizable]="isHistoryResizable()" [class.single-screen]="isHistorySingleScreen()">
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
                                    <th mat-header-cell *matHeaderCellDef>TBN Results (mg KOH/g)</th>
                                    <td mat-cell *matCellDef="let result">
                                        @for (trial of result.trials; track trial.trialNumber) {
                                            @if (trial.values.tbnResult) {
                                                <span class="trial-result">
                                                    T{{ trial.trialNumber }}: {{ trial.values.tbnResult | number:'1.2-2' }}
                                                </span>
                                            }
                                        }
                                    </td>
                                </ng-container>

                                <ng-container matColumnDef="actions">
                                    <th mat-header-cell *matHeaderCellDef>Actions</th>
                                    <td mat-cell *matCellDef="let result">
                                        <button 
                                            mat-icon-button 
                                            color="primary"
                                            (click)="viewHistoryDetails(result)"
                                            matTooltip="View details">
                                            <mat-icon>visibility</mat-icon>
                                        </button>
                                    </td>
                                </ng-container>

                                <tr mat-header-row *matHeaderRowDef="historyDisplayedColumns"></tr>
                                <tr mat-row *matRowDef="let row; columns: historyDisplayedColumns;"></tr>
                            </table>
                        </div>
                        
                        @if (isHistoryResizable()) {
                            <div class="resize-handle" 
                                 (mousedown)="startResize($event)"
                                 title="Drag to resize">
                            </div>
                        }
                    </mat-card-content>
                </mat-card>
            }
        </div>
    `,
    styles: [`
        .tbn-test-container {
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
            grid-template-columns: 1fr auto;
            gap: 16px;
            align-items: start;
        }

        .trial-selection {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 8px;
            padding: 16px;
            border: 1px solid #ddd;
            border-radius: 4px;
            background-color: #f9f9f9;
        }

        .trial-selection label {
            font-size: 0.9em;
            font-weight: 500;
            color: #666;
        }

        .selected-trials-summary {
            margin-top: 20px;
            padding: 16px;
            background-color: #e3f2fd;
            border-radius: 8px;
            border-left: 4px solid #2196f3;
        }

        .selected-trials-summary h5 {
            margin: 0 0 12px 0;
            color: #1976d2;
        }

        .selected-trials-list {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
        }

        .selected-trial-chip {
            display: inline-flex;
            align-items: center;
            padding: 4px 8px;
            background-color: #2196f3;
            color: white;
            border-radius: 16px;
            font-size: 0.9em;
            gap: 4px;
        }

        .selected-trial-chip button {
            width: 20px;
            height: 20px;
            line-height: 1;
        }

        .selected-trial-chip mat-icon {
            font-size: 16px;
            width: 16px;
            height: 16px;
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

        .history-card mat-card-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .history-controls {
            display: flex;
            gap: 8px;
        }

        .history-controls button.active {
            background-color: #e3f2fd;
            color: #1976d2;
        }

        .history-table-container {
            overflow-x: auto;
            position: relative;
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

        /* Resizable history section */
        .resizable {
            position: relative;
            resize: vertical;
            overflow: auto;
            min-height: 200px;
            max-height: 600px;
        }

        .resize-handle {
            position: absolute;
            bottom: 0;
            right: 0;
            width: 20px;
            height: 20px;
            background: linear-gradient(-45deg, transparent 0%, transparent 40%, #ccc 40%, #ccc 60%, transparent 60%);
            cursor: nw-resize;
        }

        /* Single screen mode */
        .single-screen {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            z-index: 1000;
            background: white;
            overflow: auto;
        }

        @media (max-width: 768px) {
            .tbn-test-container {
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

            .selected-trials-list {
                flex-direction: column;
            }
        }
    `]
})
export class TbnTestEntryComponent implements OnInit, OnDestroy {
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
    tbnForm!: FormGroup;
    testId = 3; // TBN test ID - this should come from route or configuration
    historyDisplayedColumns = ['sampleId', 'entryDate', 'status', 'results', 'actions'];

    // Trial selection state
    private _selectedTrials = signal<number[]>([]);
    readonly selectedTrials = this._selectedTrials.asReadonly();

    // History view state
    private _isHistoryResizable = signal(false);
    private _isHistorySingleScreen = signal(false);
    readonly isHistoryResizable = this._isHistoryResizable.asReadonly();
    readonly isHistorySingleScreen = this._isHistorySingleScreen.asReadonly();

    // Form getters
    get trialsArray(): FormArray {
        return this.tbnForm.get('trials') as FormArray;
    }

    constructor() {
        // Effect to handle sample changes
        effect(() => {
            const sample = this.selectedSample();
            if (sample && this.tbnForm) {
                this.loadExistingResults(sample.id);
                this.loadTestHistory(sample.id);
            }
        });

        // Effect to handle template changes
        effect(() => {
            const template = this.testTemplate();
            if (template && !this.tbnForm) {
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
                if (result && this.tbnForm) {
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
        this.tbnForm = this.fb.group({
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
            tbnResult: [null, [Validators.required, Validators.min(0), Validators.pattern(/^\d+(\.\d{1,2})?$/)]],
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
                tbnResult: [trial.values.tbnResult || null, [Validators.required, Validators.min(0), Validators.pattern(/^\d+(\.\d{1,2})?$/)]],
                isComplete: [trial.isComplete || false]
            });
            this.trialsArray.push(trialGroup);
        });

        // Set comments
        this.tbnForm.patchValue({
            comments: result.comments || ''
        });
    }

    onTrialCompleteChange(trialIndex: number): void {
        const trialGroup = this.trialsArray.at(trialIndex);
        const isComplete = trialGroup.get('isComplete')?.value;

        if (isComplete) {
            // Validate that required fields are filled
            const tbnResult = trialGroup.get('tbnResult')?.value;

            if (!tbnResult) {
                trialGroup.get('isComplete')?.setValue(false);
                this.snackBar.open('Please fill in TBN result before marking trial as complete', 'Close', {
                    duration: 3000
                });
            }
        }
    }

    onTrialSelectionChange(trialIndex: number, selected: boolean): void {
        const currentSelected = this._selectedTrials();

        if (selected) {
            if (!currentSelected.includes(trialIndex)) {
                this._selectedTrials.set([...currentSelected, trialIndex]);
            }
        } else {
            this._selectedTrials.set(currentSelected.filter(index => index !== trialIndex));
        }
    }

    isTrialSelected(trialIndex: number): boolean {
        return this._selectedTrials().includes(trialIndex);
    }

    getTrialControl(trialIndex: number, controlName: string): AbstractControl | null {
        return this.trialsArray.at(trialIndex)?.get(controlName) || null;
    }

    onSave(): void {
        if (!this.tbnForm.valid) {
            this.markFormGroupTouched(this.tbnForm);
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

        const formValue = this.tbnForm.value;
        const request: SaveTestResultRequest = {
            sampleId: sample.id,
            testId: this.testId,
            entryId: 'USER', // This should come from authentication
            comments: formValue.comments,
            trials: formValue.trials.map((trial: any, index: number) => ({
                trialNumber: index + 1,
                values: {
                    tbnResult: trial.tbnResult
                },
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
                // Clear trial selections after successful save
                this._selectedTrials.set([]);
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
                this.tbnForm.reset();
                this.initializeForm();
                this._selectedTrials.set([]);
                this.snackBar.open('Form cleared', 'Close', { duration: 2000 });
            }
        });
    }

    onDelete(): void {
        const sample = this.selectedSample();
        const selectedTrials = this._selectedTrials();

        if (!sample || selectedTrials.length === 0) {
            this.snackBar.open('Please select trials to delete', 'Close', {
                duration: 3000,
                panelClass: ['error-snackbar']
            });
            return;
        }

        const trialText = selectedTrials.length === 1
            ? `Trial ${selectedTrials[0] + 1}`
            : `${selectedTrials.length} trials`;

        const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
            data: {
                title: 'Delete Selected Trials',
                message: `Are you sure you want to delete ${trialText}? This action cannot be undone.`,
                confirmText: 'Delete',
                cancelText: 'Cancel'
            }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result && sample) {
                // For this implementation, we'll delete all results and re-save the non-selected ones
                // In a real implementation, you might want a more granular delete API
                this.testService.deleteTestResults(this.testId, sample.id).subscribe({
                    next: (response) => {
                        // Clear the selected trials from the form
                        selectedTrials.forEach(trialIndex => {
                            const trialGroup = this.trialsArray.at(trialIndex);
                            trialGroup.reset();
                        });

                        this._selectedTrials.set([]);

                        this.snackBar.open(`Successfully deleted selected trials`, 'Close', {
                            duration: 3000,
                            panelClass: ['success-snackbar']
                        });

                        this.loadTestHistory(sample.id);
                    },
                    error: (error) => {
                        this.snackBar.open(`Failed to delete trials: ${error.message}`, 'Close', {
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

    // History view controls
    toggleHistoryResize(): void {
        this._isHistoryResizable.set(!this._isHistoryResizable());
    }

    toggleHistorySingleScreen(): void {
        this._isHistorySingleScreen.set(!this._isHistorySingleScreen());
    }

    startResize(event: MouseEvent): void {
        // Implement resize functionality if needed
        event.preventDefault();
    }

    viewHistoryDetails(result: any): void {
        // Implement history details view
        this.snackBar.open(`Viewing details for Sample ${result.sampleId}`, 'Close', {
            duration: 2000
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