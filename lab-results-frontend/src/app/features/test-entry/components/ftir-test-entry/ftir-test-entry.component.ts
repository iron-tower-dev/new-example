import { Component, OnInit, OnDestroy, inject, signal, computed, effect } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
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
    SaveTestResultRequest
} from '../../../../shared/models/test.model';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { SharedModule } from '../../../../shared/shared.module';

interface FtirData {
    deltaArea: number | null;
    antiOxidant: number | null;
    oxidation: number | null;
    h2o: number | null;
    antiWear: number | null;
    soot: number | null;
    fuelDilution: number | null;
    mixture: number | null;
    weakAcid: number | null;
}

@Component({
    selector: 'app-ftir-test-entry',
    standalone: true,
    imports: [SharedModule],
    template: `
        <div class="ftir-test-container">
            <!-- Header Section -->
            <mat-card class="header-card">
                <mat-card-header>
                    <mat-card-title>FT-IR Spectroscopy Test Entry</mat-card-title>
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
                    <p>Loading FT-IR test data...</p>
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
            @if (ftirForm && !isLoading()) {
                <form [formGroup]="ftirForm" (ngSubmit)="onSave()">
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

                    <!-- FT-IR Spectroscopy Fields -->
                    <mat-card class="ftir-fields-card">
                        <mat-card-header>
                            <mat-card-title>FT-IR Spectroscopy Readings</mat-card-title>
                            <mat-card-subtitle>Fourier Transform Infrared Spectroscopy Analysis</mat-card-subtitle>
                        </mat-card-header>
                        <mat-card-content>
                            <div class="ftir-fields-grid">
                                <!-- Row 1 -->
                                <mat-form-field appearance="outline">
                                    <mat-label>Delta Area (Contamination)</mat-label>
                                    <input 
                                        matInput 
                                        type="number" 
                                        step="0.01"
                                        formControlName="deltaArea"
                                        placeholder="Enter delta area value">
                                    <mat-hint>Contamination level indicator</mat-hint>
                                    @if (ftirForm.get('deltaArea')?.errors?.['min']) {
                                        <mat-error>Value cannot be negative</mat-error>
                                    }
                                </mat-form-field>

                                <mat-form-field appearance="outline">
                                    <mat-label>Anti-oxidant</mat-label>
                                    <input 
                                        matInput 
                                        type="number" 
                                        step="0.01"
                                        formControlName="antiOxidant"
                                        placeholder="Enter anti-oxidant value">
                                    <mat-hint>Anti-oxidant additive level</mat-hint>
                                    @if (ftirForm.get('antiOxidant')?.errors?.['min']) {
                                        <mat-error>Value cannot be negative</mat-error>
                                    }
                                </mat-form-field>

                                <mat-form-field appearance="outline">
                                    <mat-label>Oxidation</mat-label>
                                    <input 
                                        matInput 
                                        type="number" 
                                        step="0.01"
                                        formControlName="oxidation"
                                        placeholder="Enter oxidation value">
                                    <mat-hint>Oxidation level</mat-hint>
                                    @if (ftirForm.get('oxidation')?.errors?.['min']) {
                                        <mat-error>Value cannot be negative</mat-error>
                                    }
                                </mat-form-field>

                                <!-- Row 2 -->
                                <mat-form-field appearance="outline">
                                    <mat-label>H₂O (Water Content)</mat-label>
                                    <input 
                                        matInput 
                                        type="number" 
                                        step="0.01"
                                        formControlName="h2o"
                                        placeholder="Enter water content">
                                    <mat-hint>Water contamination level</mat-hint>
                                    @if (ftirForm.get('h2o')?.errors?.['min']) {
                                        <mat-error>Value cannot be negative</mat-error>
                                    }
                                </mat-form-field>

                                <mat-form-field appearance="outline">
                                    <mat-label>Anti-wear (ZDDP)</mat-label>
                                    <input 
                                        matInput 
                                        type="number" 
                                        step="0.01"
                                        formControlName="antiWear"
                                        placeholder="Enter anti-wear value">
                                    <mat-hint>ZDDP anti-wear additive level</mat-hint>
                                    @if (ftirForm.get('antiWear')?.errors?.['min']) {
                                        <mat-error>Value cannot be negative</mat-error>
                                    }
                                </mat-form-field>

                                <mat-form-field appearance="outline">
                                    <mat-label>Soot</mat-label>
                                    <input 
                                        matInput 
                                        type="number" 
                                        step="0.01"
                                        formControlName="soot"
                                        placeholder="Enter soot value">
                                    <mat-hint>Soot contamination level</mat-hint>
                                    @if (ftirForm.get('soot')?.errors?.['min']) {
                                        <mat-error>Value cannot be negative</mat-error>
                                    }
                                </mat-form-field>

                                <!-- Row 3 -->
                                <mat-form-field appearance="outline">
                                    <mat-label>Fuel Dilution</mat-label>
                                    <input 
                                        matInput 
                                        type="number" 
                                        step="0.01"
                                        formControlName="fuelDilution"
                                        placeholder="Enter dilution value">
                                    <mat-hint>Fuel dilution level</mat-hint>
                                    @if (ftirForm.get('fuelDilution')?.errors?.['min']) {
                                        <mat-error>Value cannot be negative</mat-error>
                                    }
                                </mat-form-field>

                                <mat-form-field appearance="outline">
                                    <mat-label>Mixture</mat-label>
                                    <input 
                                        matInput 
                                        type="number" 
                                        step="0.01"
                                        formControlName="mixture"
                                        placeholder="Enter mixture value">
                                    <mat-hint>Oil mixture indicator</mat-hint>
                                    @if (ftirForm.get('mixture')?.errors?.['min']) {
                                        <mat-error>Value cannot be negative</mat-error>
                                    }
                                </mat-form-field>

                                <mat-form-field appearance="outline">
                                    <mat-label>Weak Acid (NLGI)</mat-label>
                                    <input 
                                        matInput 
                                        type="number" 
                                        step="0.01"
                                        formControlName="weakAcid"
                                        placeholder="Enter weak acid value">
                                    <mat-hint>NLGI weak acid level</mat-hint>
                                    @if (ftirForm.get('weakAcid')?.errors?.['min']) {
                                        <mat-error>Value cannot be negative</mat-error>
                                    }
                                </mat-form-field>
                            </div>

                            <!-- Macro Selection (if applicable) -->
                            @if (selectedSample(); as sample) {
                                <div class="macro-info">
                                    <mat-icon>info</mat-icon>
                                    <span>Macro: {{ macroType() || 'UNKNOWN' }}</span>
                                </div>
                            }
                        </mat-card-content>
                    </mat-card>

                    <!-- File Upload Section -->
                    <mat-card class="file-upload-card">
                        <mat-card-header>
                            <mat-card-title>Import Data from File</mat-card-title>
                            <mat-card-subtitle>Upload FT-IR results file (optional)</mat-card-subtitle>
                        </mat-card-header>
                        <mat-card-content>
                            <div class="file-upload-area">
                                <input 
                                    type="file" 
                                    #fileInput
                                    (change)="onFileSelected($event)"
                                    accept=".csv,.txt,.xlsx"
                                    style="display: none">
                                <button 
                                    mat-raised-button 
                                    color="accent"
                                    type="button"
                                    (click)="fileInput.click()">
                                    <mat-icon>upload_file</mat-icon>
                                    Upload File
                                </button>
                                @if (uploadedFileName()) {
                                    <span class="file-name">{{ uploadedFileName() }}</span>
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
                            [disabled]="!ftirForm.valid || isLoading()">
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
                        <mat-card-title>Last 12 Results for FT-IR Test</mat-card-title>
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
                                    <th mat-header-cell *matHeaderCellDef>Key Results</th>
                                    <td mat-cell *matCellDef="let result">
                                        @if (result.values) {
                                            <div class="result-summary">
                                                @if (result.values.oxidation) {
                                                    <span class="result-item">Ox: {{ result.values.oxidation | number:'1.2-2' }}</span>
                                                }
                                                @if (result.values.soot) {
                                                    <span class="result-item">Soot: {{ result.values.soot | number:'1.2-2' }}</span>
                                                }
                                                @if (result.values.h2o) {
                                                    <span class="result-item">H₂O: {{ result.values.h2o | number:'1.2-2' }}</span>
                                                }
                                            </div>
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
        .ftir-test-container {
            padding: 20px;
            max-width: 1400px;
            margin: 0 auto;
        }

        .header-card, .sample-info-card, .ftir-fields-card, .file-upload-card, .comments-card, .history-card {
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

        .ftir-fields-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 16px;
            margin-bottom: 16px;
        }

        .macro-info {
            display: flex;
            align-items: center;
            gap: 8px;
            padding: 12px;
            background-color: #e3f2fd;
            border-radius: 4px;
            margin-top: 16px;
        }

        .macro-info mat-icon {
            color: #1976d2;
        }

        .file-upload-area {
            display: flex;
            align-items: center;
            gap: 16px;
        }

        .file-name {
            color: #666;
            font-style: italic;
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

        .result-summary {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
        }

        .result-item {
            display: inline-block;
            padding: 2px 6px;
            background-color: #e3f2fd;
            border-radius: 4px;
            font-size: 0.9em;
        }

        .status-c { color: #4caf50; font-weight: 500; }
        .status-e { color: #ff9800; font-weight: 500; }
        .status-x { color: #757575; font-weight: 500; }

        @media (max-width: 768px) {
            .ftir-test-container {
                padding: 10px;
            }
            
            .ftir-fields-grid {
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
export class FtirTestEntryComponent implements OnInit, OnDestroy {
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
    ftirForm!: FormGroup;
    testId = 70; // FT-IR test ID
    historyDisplayedColumns = ['sampleId', 'entryDate', 'status', 'results'];
    macroType = signal<string | null>(null);
    uploadedFileName = signal<string | null>(null);

    constructor() {
        // Effect to handle sample changes
        effect(() => {
            const sample = this.selectedSample();
            if (sample && this.ftirForm) {
                this.loadExistingResults(sample.id);
                this.loadTestHistory(sample.id);
                this.loadMacroType(sample.lubeType);
            }
        });

        // Effect to handle template changes
        effect(() => {
            const template = this.testTemplate();
            if (template && !this.ftirForm) {
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
                if (result && this.ftirForm) {
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

    private loadMacroType(lubeType: string | undefined): void {
        // In legacy system, macro is determined from vwTestScheduleDefinitionBySample or ByMaterial
        // For now, set a placeholder - this should be fetched from API
        if (lubeType) {
            // TODO: Fetch actual macro from TestSchedulingService
            this.macroType.set('STANDARD');
        }
    }

    private initializeForm(): void {
        this.ftirForm = this.fb.group({
            deltaArea: [null, [Validators.min(0)]],
            antiOxidant: [null, [Validators.min(0)]],
            oxidation: [null, [Validators.min(0)]],
            h2o: [null, [Validators.min(0)]],
            antiWear: [null, [Validators.min(0)]],
            soot: [null, [Validators.min(0)]],
            fuelDilution: [null, [Validators.min(0)]],
            mixture: [null, [Validators.min(0)]],
            weakAcid: [null, [Validators.min(0)]],
            comments: ['']
        });
    }

    private populateFormWithResults(result: any): void {
        if (result.values) {
            this.ftirForm.patchValue({
                deltaArea: result.values.contam || null,
                antiOxidant: result.values.anti_oxidant || null,
                oxidation: result.values.oxidation || null,
                h2o: result.values.h2o || null,
                antiWear: result.values.zddp || null,
                soot: result.values.soot || null,
                fuelDilution: result.values.fuel_dilution || null,
                mixture: result.values.mixture || null,
                weakAcid: result.values.nlgi || null,
                comments: result.comments || ''
            });
        }
    }

    onFileSelected(event: any): void {
        const file = event.target.files[0];
        if (file) {
            this.uploadedFileName.set(file.name);
            // TODO: Implement file parsing logic
            // This should call FileUploadService to parse the file
            // and populate the form fields
            this.snackBar.open('File upload functionality will be implemented', 'Close', {
                duration: 3000
            });
        }
    }

    onSave(): void {
        if (!this.ftirForm.valid) {
            this.markFormGroupTouched(this.ftirForm);
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

        const formValue = this.ftirForm.value;
        const request: SaveTestResultRequest = {
            sampleId: sample.id,
            testId: this.testId,
            entryId: 'USER', // This should come from authentication
            comments: formValue.comments,
            trials: [{
                trialNumber: 1,
                values: {
                    contam: formValue.deltaArea,
                    anti_oxidant: formValue.antiOxidant,
                    oxidation: formValue.oxidation,
                    h2o: formValue.h2o,
                    zddp: formValue.antiWear,
                    soot: formValue.soot,
                    fuel_dilution: formValue.fuelDilution,
                    mixture: formValue.mixture,
                    nlgi: formValue.weakAcid
                },
                isComplete: true
            }]
        };

        this.testService.saveTestResults(this.testId, request).subscribe({
            next: (response) => {
                this.snackBar.open('Successfully saved FT-IR results', 'Close', {
                    duration: 3000,
                    panelClass: ['success-snackbar']
                });
                this.loadTestHistory(sample.id);
                
                // Auto-schedule follow-on tests if applicable
                this.scheduleFollowOnTests(sample.id);
            },
            error: (error) => {
                this.snackBar.open(`Failed to save results: ${error.message}`, 'Close', {
                    duration: 5000,
                    panelClass: ['error-snackbar']
                });
            }
        });
    }

    private scheduleFollowOnTests(sampleId: number): void {
        // TODO: Implement AutoAddRemoveTests logic from legacy system
        // This would analyze the FT-IR results and schedule additional tests if needed
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
                this.ftirForm.reset();
                this.uploadedFileName.set(null);
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
                message: 'Are you sure you want to delete all FT-IR test results for this sample? This action cannot be undone.',
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
                        this.ftirForm.reset();
                        this.uploadedFileName.set(null);
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
            }
        });
    }
}
