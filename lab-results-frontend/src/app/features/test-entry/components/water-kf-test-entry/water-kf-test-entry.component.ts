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
    WaterKfTrialData,
    SaveTestResultRequest
} from '../../../../shared/models/test.model';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { SharedModule } from '../../../../shared/shared.module';
import { FileSelectionComponent } from '../../../../shared/components/file-selection/file-selection.component';
import { FileUploadService, FileUploadRequest } from '../../../../shared/services/file-upload.service';

@Component({
    selector: 'app-water-kf-test-entry',
    standalone: true,
    imports: [SharedModule, FileSelectionComponent],
    template: `
        <div class="water-kf-test-container">
            <!-- Header Section -->
            <mat-card class="header-card">
                <mat-card-header>
                    <mat-card-title>Water by Karl Fischer Test Entry</mat-card-title>
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
            @if (waterKfForm && !isLoading()) {
                <form [formGroup]="waterKfForm" (ngSubmit)="onSave()">
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
                            <mat-card-subtitle>Enter water content results and upload instrument data files</mat-card-subtitle>
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
                                                <mat-label>Water Content (%)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.001"
                                                    min="0"
                                                    formControlName="waterContent"
                                                    placeholder="Enter water content percentage">
                                                <mat-hint>Water content as percentage (0.000%)</mat-hint>
                                                @if (getTrialControl($index, 'waterContent')?.errors?.['required']) {
                                                    <mat-error>Water content is required</mat-error>
                                                }
                                                @if (getTrialControl($index, 'waterContent')?.errors?.['min']) {
                                                    <mat-error>Water content must be 0 or greater</mat-error>
                                                }
                                                @if (getTrialControl($index, 'waterContent')?.errors?.['pattern']) {
                                                    <mat-error>Please enter a valid number</mat-error>
                                                }
                                            </mat-form-field>

                                            <div class="file-upload-section">
                                                <app-file-selection
                                                    [sampleId]="selectedSample()?.id || 0"
                                                    [testId]="testId"
                                                    [showPreview]="true"
                                                    (fileSelected)="onFileSelected($event, $index)"
                                                    (existingFileSelected)="onExistingFileSelected($event, $index)">
                                                </app-file-selection>
                                                
                                                @if (getTrialControl($index, 'selectedFile')?.value) {
                                                    <div class="selected-file-info">
                                                        <mat-icon>attach_file</mat-icon>
                                                        <span>{{ getTrialControl($index, 'selectedFile')?.value.name }}</span>
                                                        <button mat-icon-button 
                                                                color="warn"
                                                                matTooltip="Remove file"
                                                                (click)="onRemoveFile($index)">
                                                            <mat-icon>close</mat-icon>
                                                        </button>
                                                    </div>
                                                }
                                            </div>
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
                            [disabled]="!waterKfForm.valid || isLoading()">
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
                        <mat-card-title>Last 12 Results for Water-KF Test</mat-card-title>
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
                                    <th mat-header-cell *matHeaderCellDef>Water Content (%)</th>
                                    <td mat-cell *matCellDef="let result">
                                        @for (trial of result.trials; track trial.trialNumber) {
                                            @if (trial.values.waterContent) {
                                                <span class="trial-result">
                                                    T{{ trial.trialNumber }}: {{ trial.values.waterContent | number:'1.3-3' }}%
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
        .water-kf-test-container {
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
            grid-template-columns: 1fr 2fr;
            gap: 16px;
            align-items: start;
        }

        .file-upload-section {
            display: flex;
            flex-direction: column;
            gap: 12px;
        }

        .file-upload-buttons {
            display: flex;
            gap: 8px;
            flex-wrap: wrap;
        }

        .file-upload-buttons button {
            min-width: 120px;
        }

        .selected-file-info {
            display: flex;
            align-items: center;
            gap: 8px;
            padding: 8px 12px;
            background-color: #e3f2fd;
            border-radius: 4px;
            font-size: 14px;
            margin-top: 8px;
        }

        .selected-file-info mat-icon {
            color: #1976d2;
        }

        .file-preview-section {
            margin-top: 16px;
            grid-column: 1 / -1;
        }

        .file-preview-card {
            background-color: #f8f9fa;
            border: 1px solid #dee2e6;
        }

        .file-info {
            margin-bottom: 16px;
        }

        .file-info p {
            margin: 4px 0;
            font-size: 0.9em;
        }

        .file-content-preview {
            margin-top: 16px;
        }

        .file-content-preview h5 {
            margin: 0 0 8px 0;
            color: #666;
        }

        .file-content-preview pre {
            background-color: #f1f3f4;
            padding: 12px;
            border-radius: 4px;
            font-size: 0.85em;
            max-height: 200px;
            overflow-y: auto;
            white-space: pre-wrap;
            word-wrap: break-word;
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
            .water-kf-test-container {
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

            .file-upload-buttons {
                flex-direction: column;
            }
        }
    `]
})
export class WaterKfTestEntryComponent implements OnInit, OnDestroy {
    private fb = inject(FormBuilder);
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private snackBar = inject(MatSnackBar);
    private dialog = inject(MatDialog);
    private sampleService = inject(SampleService);
    private testService = inject(TestService);
    private validationService = inject(ValidationService);
    private fileUploadService = inject(FileUploadService);
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
    waterKfForm!: FormGroup;
    testId = 2; // Water-KF test ID - this should come from route or configuration
    historyDisplayedColumns = ['sampleId', 'entryDate', 'status', 'results'];
    showFilePreview: boolean[] = [false, false, false, false];
    filePreviewContent: string[] = ['', '', '', ''];

    // Form getters
    get trialsArray(): FormArray {
        return this.waterKfForm.get('trials') as FormArray;
    }

    constructor() {
        // Effect to handle sample changes
        effect(() => {
            const sample = this.selectedSample();
            if (sample && this.waterKfForm) {
                this.loadExistingResults(sample.id);
                this.loadTestHistory(sample.id);
            }
        });

        // Effect to handle template changes
        effect(() => {
            const template = this.testTemplate();
            if (template && !this.waterKfForm) {
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
                if (result && this.waterKfForm) {
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
        this.waterKfForm = this.fb.group({
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
            waterContent: [null, [Validators.required, Validators.min(0), Validators.pattern(/^\d+(\.\d{1,3})?$/)]],
            selectedFile: [null],
            uploadedFileId: [null],
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
                waterContent: [trial.values.waterContent || null, [Validators.required, Validators.min(0), Validators.pattern(/^\d+(\.\d{1,3})?$/)]],
                selectedFile: [null], // Files cannot be restored from server
                uploadedFileId: [trial.values.uploadedFileId || null],
                isComplete: [trial.isComplete || false]
            });
            this.trialsArray.push(trialGroup);
        });

        // Set comments
        this.waterKfForm.patchValue({
            comments: result.comments || ''
        });
    }

    onTrialCompleteChange(trialIndex: number): void {
        const trialGroup = this.trialsArray.at(trialIndex);
        const isComplete = trialGroup.get('isComplete')?.value;

        if (isComplete) {
            // Validate that required fields are filled
            const waterContent = trialGroup.get('waterContent')?.value;

            if (!waterContent) {
                trialGroup.get('isComplete')?.setValue(false);
                this.snackBar.open('Please fill in water content before marking trial as complete', 'Close', {
                    duration: 3000
                });
            }
        }
    }

    getTrialControl(trialIndex: number, controlName: string): AbstractControl | null {
        return this.trialsArray.at(trialIndex)?.get(controlName) || null;
    }

    onFileSelected(file: File, trialIndex: number): void {
        // Set file to form control for later upload
        this.getTrialControl(trialIndex, 'selectedFile')?.setValue(file);

        this.snackBar.open(`File "${file.name}" selected successfully`, 'Close', {
            duration: 2000,
            panelClass: ['success-snackbar']
        });
    }

    onExistingFileSelected(filePreview: any, trialIndex: number): void {
        // Set the existing file ID for reference
        this.getTrialControl(trialIndex, 'uploadedFileId')?.setValue(filePreview.id);

        this.snackBar.open(`Existing file "${filePreview.fileName}" selected`, 'Close', {
            duration: 2000,
            panelClass: ['success-snackbar']
        });
    }



    onRemoveFile(trialIndex: number): void {
        this.getTrialControl(trialIndex, 'selectedFile')?.setValue(null);
        this.getTrialControl(trialIndex, 'uploadedFileId')?.setValue(null);
        this.showFilePreview[trialIndex] = false;
        this.filePreviewContent[trialIndex] = '';

        this.snackBar.open('File removed', 'Close', {
            duration: 2000
        });
    }

    getSelectedFileName(trialIndex: number): string {
        const file = this.getTrialControl(trialIndex, 'selectedFile')?.value;
        return file ? file.name : '';
    }

    getFileSize(trialIndex: number): string {
        const file = this.getTrialControl(trialIndex, 'selectedFile')?.value;
        if (!file) return '';

        return this.fileUploadService.formatFileSize(file.size);
    }

    getFileType(trialIndex: number): string {
        const file = this.getTrialControl(trialIndex, 'selectedFile')?.value;
        return file ? file.type || 'Unknown' : '';
    }

    getFileLastModified(trialIndex: number): Date | null {
        const file = this.getTrialControl(trialIndex, 'selectedFile')?.value;
        return file ? new Date(file.lastModified) : null;
    }

    onSave(): void {
        if (!this.waterKfForm.valid) {
            this.markFormGroupTouched(this.waterKfForm);
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

        // First upload any new files, then save the test results
        this.uploadFilesAndSave(sample);
    }

    private async uploadFilesAndSave(sample: any): Promise<void> {
        const formValue = this.waterKfForm.value;
        const uploadPromises: Promise<any>[] = [];

        // Upload files for each trial that has a selected file
        formValue.trials.forEach((trial: any, index: number) => {
            if (trial.selectedFile) {
                const uploadRequest: FileUploadRequest = {
                    sampleId: sample.id,
                    testId: this.testId,
                    trialNumber: index + 1,
                    uploadedBy: 'USER' // This should come from authentication
                };

                const uploadPromise = this.fileUploadService.uploadFile(trial.selectedFile, uploadRequest).toPromise()
                    .then(response => {
                        if (response && response.success && response.fileInfo) {
                            // Update the form with the uploaded file ID
                            this.getTrialControl(index, 'uploadedFileId')?.setValue(response.fileInfo.id);
                            return response.fileInfo.id;
                        }
                        throw new Error(response?.message || 'Upload failed');
                    });

                uploadPromises.push(uploadPromise);
            }
        });

        try {
            // Wait for all file uploads to complete
            if (uploadPromises.length > 0) {
                await Promise.all(uploadPromises);
                this.snackBar.open('Files uploaded successfully', 'Close', { duration: 2000 });
            }

            // Now save the test results with file references
            const updatedFormValue = this.waterKfForm.value;
            const request: SaveTestResultRequest = {
                sampleId: sample.id,
                testId: this.testId,
                entryId: 'USER', // This should come from authentication
                comments: updatedFormValue.comments,
                trials: updatedFormValue.trials.map((trial: any, index: number) => ({
                    trialNumber: index + 1,
                    values: {
                        waterContent: trial.waterContent,
                        uploadedFileId: trial.uploadedFileId
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

                    // Clear selected files since they're now uploaded
                    formValue.trials.forEach((trial: any, index: number) => {
                        this.getTrialControl(index, 'selectedFile')?.setValue(null);
                    });
                },
                error: (error) => {
                    this.snackBar.open(`Failed to save results: ${error.message}`, 'Close', {
                        duration: 5000,
                        panelClass: ['error-snackbar']
                    });
                }
            });

        } catch (error: any) {
            this.snackBar.open(`File upload failed: ${error.message}`, 'Close', {
                duration: 5000,
                panelClass: ['error-snackbar']
            });
        }
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
                this.waterKfForm.reset();
                this.initializeForm();
                this.showFilePreview = [false, false, false, false];
                this.filePreviewContent = ['', '', '', ''];
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
                        this.waterKfForm.reset();
                        this.initializeForm();
                        this.showFilePreview = [false, false, false, false];
                        this.filePreviewContent = ['', '', '', ''];
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