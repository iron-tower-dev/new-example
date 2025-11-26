import { Component, OnInit, OnDestroy, inject, signal, computed, effect } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';

import { SampleService } from '../../../../shared/services/sample.service';
import { TestService } from '../../../../shared/services/test.service';
import { EmissionSpectroscopyService } from '../../../../shared/services/emission-spectroscopy.service';
import { FileUploadService } from '../../../../shared/services/file-upload.service';
import { ValidationService } from '../../../../shared/services/validation.service';
import { Sample } from '../../../../shared/models/sample.model';
import {
    TestTemplate,
    EmissionSpectroscopyTrialData,
    EmissionSpectroscopyCreateRequest,
    EmissionSpectroscopyUpdateRequest,
    EmissionSpectroscopyData
} from '../../../../shared/models/test.model';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { FileUploadComponent } from '../../../../shared/components/file-upload/file-upload.component';
import { SharedModule } from '../../../../shared/shared.module';

@Component({
    selector: 'app-emission-spectroscopy-test-entry',
    standalone: true,
    imports: [SharedModule, FileUploadComponent],
    template: `
        <div class="emission-spectroscopy-container">
            <!-- Header Section -->
            <mat-card class="header-card">
                <mat-card-header>
                    <mat-card-title>Emission Spectroscopy Test Entry</mat-card-title>
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
            @if (emissionSpectroscopyForm && !isLoading()) {
                <form [formGroup]="emissionSpectroscopyForm" (ngSubmit)="onSave()">
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
                            <mat-card-subtitle>Enter element concentrations for each trial (ppm)</mat-card-subtitle>
                        </mat-card-header>
                        <mat-card-content>
                            <div class="trials-container" formArrayName="trials">
                                @for (trial of trialsArray.controls; track $index) {
                                    <div class="trial-row" [formGroupName]="$index">
                                        <div class="trial-header">
                                            <h4>Trial {{ $index + 1 }}</h4>
                                            <div class="trial-controls">
                                                @if ($index === 0) {
                                                    <mat-checkbox 
                                                        [formControlName]="'scheduleFerrography'"
                                                        (change)="onFerrographyScheduleChange($index)">
                                                        Schedule Ferrography
                                                    </mat-checkbox>
                                                }
                                                <mat-checkbox 
                                                    [formControlName]="'isComplete'"
                                                    (change)="onTrialCompleteChange($index)">
                                                    Complete
                                                </mat-checkbox>
                                            </div>
                                        </div>
                                        
                                        <!-- File Upload Section -->
                                        <div class="file-upload-section">
                                            <app-file-upload
                                                [testId]="testId"
                                                [sampleId]="selectedSample()?.id || 0"
                                                [trialNumber]="$index + 1"
                                                (fileUploaded)="onFileUploaded($index, $event)">
                                            </app-file-upload>
                                        </div>
                                        
                                        <!-- Element Fields Grid -->
                                        <div class="elements-grid">
                                            <!-- Row 1: Na, Cr, Sn, Si, Mo -->
                                            <mat-form-field appearance="outline">
                                                <mat-label>Na (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="na"
                                                    placeholder="Sodium">
                                                <mat-hint>Sodium concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Cr (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="cr"
                                                    placeholder="Chromium">
                                                <mat-hint>Chromium concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Sn (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="sn"
                                                    placeholder="Tin">
                                                <mat-hint>Tin concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Si (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="si"
                                                    placeholder="Silicon">
                                                <mat-hint>Silicon concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Mo (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="mo"
                                                    placeholder="Molybdenum">
                                                <mat-hint>Molybdenum concentration</mat-hint>
                                            </mat-form-field>

                                            <!-- Row 2: Ca, Al, Ba, Mg, Ni -->
                                            <mat-form-field appearance="outline">
                                                <mat-label>Ca (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="ca"
                                                    placeholder="Calcium">
                                                <mat-hint>Calcium concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Al (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="al"
                                                    placeholder="Aluminum">
                                                <mat-hint>Aluminum concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Ba (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="ba"
                                                    placeholder="Barium">
                                                <mat-hint>Barium concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Mg (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="mg"
                                                    placeholder="Magnesium">
                                                <mat-hint>Magnesium concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Ni (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="ni"
                                                    placeholder="Nickel">
                                                <mat-hint>Nickel concentration</mat-hint>
                                            </mat-form-field>

                                            <!-- Row 3: Mn, Zn, P, Ag, Pb -->
                                            <mat-form-field appearance="outline">
                                                <mat-label>Mn (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="mn"
                                                    placeholder="Manganese">
                                                <mat-hint>Manganese concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Zn (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="zn"
                                                    placeholder="Zinc">
                                                <mat-hint>Zinc concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>P (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="p"
                                                    placeholder="Phosphorus">
                                                <mat-hint>Phosphorus concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Ag (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="ag"
                                                    placeholder="Silver">
                                                <mat-hint>Silver concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Pb (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="pb"
                                                    placeholder="Lead">
                                                <mat-hint>Lead concentration</mat-hint>
                                            </mat-form-field>

                                            <!-- Row 4: H, B, Cu, Fe -->
                                            <mat-form-field appearance="outline">
                                                <mat-label>H (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="h"
                                                    placeholder="Hydrogen">
                                                <mat-hint>Hydrogen concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>B (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="b"
                                                    placeholder="Boron">
                                                <mat-hint>Boron concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Cu (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="cu"
                                                    placeholder="Copper">
                                                <mat-hint>Copper concentration</mat-hint>
                                            </mat-form-field>

                                            <mat-form-field appearance="outline">
                                                <mat-label>Fe (ppm)</mat-label>
                                                <input 
                                                    matInput 
                                                    type="number" 
                                                    step="0.1"
                                                    min="0"
                                                    formControlName="fe"
                                                    placeholder="Iron">
                                                <mat-hint>Iron concentration</mat-hint>
                                            </mat-form-field>
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
                            [disabled]="!emissionSpectroscopyForm.valid || isLoading()">
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
                        <mat-card-title>Last 12 Results for Emission Spectroscopy Test</mat-card-title>
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

                                <ng-container matColumnDef="elements">
                                    <th mat-header-cell *matHeaderCellDef>Key Elements</th>
                                    <td mat-cell *matCellDef="let result">
                                        @for (trial of result.trials; track trial.trialNumber) {
                                            @if (trial.values.fe || trial.values.cu || trial.values.cr) {
                                                <span class="element-result">
                                                    T{{ trial.trialNumber }}: 
                                                    @if (trial.values.fe) { Fe:{{ trial.values.fe }} }
                                                    @if (trial.values.cu) { Cu:{{ trial.values.cu }} }
                                                    @if (trial.values.cr) { Cr:{{ trial.values.cr }} }
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
        .emission-spectroscopy-container {
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
            gap: 30px;
        }

        .trial-row {
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            padding: 20px;
            background-color: #fafafa;
        }

        .trial-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }

        .trial-header h4 {
            margin: 0;
            color: #333;
        }

        .trial-controls {
            display: flex;
            gap: 20px;
            align-items: center;
        }

        .file-upload-section {
            margin-bottom: 20px;
            padding: 15px;
            background-color: #f5f5f5;
            border-radius: 4px;
        }

        .elements-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 16px;
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

        .element-result {
            display: inline-block;
            margin-right: 8px;
            padding: 2px 6px;
            background-color: #e8f5e8;
            border-radius: 4px;
            font-size: 0.9em;
        }

        .status-c { color: #4caf50; font-weight: 500; }
        .status-e { color: #ff9800; font-weight: 500; }
        .status-x { color: #757575; font-weight: 500; }

        @media (max-width: 1200px) {
            .elements-grid {
                grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
            }
        }

        @media (max-width: 768px) {
            .emission-spectroscopy-container {
                padding: 10px;
            }
            
            .elements-grid {
                grid-template-columns: 1fr 1fr;
            }
            
            .trial-controls {
                flex-direction: column;
                gap: 10px;
                align-items: flex-start;
            }
            
            .action-buttons {
                flex-direction: column;
            }
            
            .action-buttons button {
                width: 100%;
            }
        }

        @media (max-width: 480px) {
            .elements-grid {
                grid-template-columns: 1fr;
            }
        }
    `]
})
export class EmissionSpectroscopyTestEntryComponent implements OnInit, OnDestroy {
    private fb = inject(FormBuilder);
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private snackBar = inject(MatSnackBar);
    private dialog = inject(MatDialog);
    private sampleService = inject(SampleService);
    private testService = inject(TestService);
    private emissionSpectroscopyService = inject(EmissionSpectroscopyService);
    private fileUploadService = inject(FileUploadService);
    private validationService = inject(ValidationService);
    private destroy$ = new Subject<void>();

    // Signals from services
    readonly selectedSample = this.sampleService.selectedSample;
    readonly testTemplate = this.testService.testTemplate;
    readonly testResult = this.testService.testResult;
    readonly testResultHistory = this.testService.testResultHistory;
    readonly emissionSpectroscopyData = this.emissionSpectroscopyService.emissionSpectroscopyData;
    readonly isLoading = computed(() =>
        this.sampleService.isLoading() ||
        this.testService.isLoading() ||
        this.emissionSpectroscopyService.isLoading()
    );
    readonly error = computed(() =>
        this.sampleService.error() ||
        this.testService.error() ||
        this.emissionSpectroscopyService.error()
    );
    readonly hasError = computed(() => this.error() !== null);

    // Component state
    emissionSpectroscopyForm!: FormGroup;
    testId = 40; // Emission Spectroscopy test ID - this should come from route or configuration
    historyDisplayedColumns = ['sampleId', 'entryDate', 'status', 'elements'];

    // Element field names for form creation
    private readonly elementFields = [
        'na', 'cr', 'sn', 'si', 'mo', 'ca', 'al', 'ba', 'mg', 'ni',
        'mn', 'zn', 'p', 'ag', 'pb', 'h', 'b', 'cu', 'fe'
    ];

    // Form getters
    get trialsArray(): FormArray {
        return this.emissionSpectroscopyForm.get('trials') as FormArray;
    }

    constructor() {
        // Effect to handle sample changes
        effect(() => {
            const sample = this.selectedSample();
            if (sample && this.emissionSpectroscopyForm) {
                this.loadExistingResults(sample.id);
                this.loadTestHistory(sample.id);
            }
        });

        // Effect to handle template changes
        effect(() => {
            const template = this.testTemplate();
            if (template && !this.emissionSpectroscopyForm) {
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
        this.emissionSpectroscopyService.getEmissionSpectroscopyData(sampleId, this.testId).subscribe({
            next: (data) => {
                if (data && data.length > 0 && this.emissionSpectroscopyForm) {
                    this.populateFormWithResults(data);
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
        this.emissionSpectroscopyForm = this.fb.group({
            trials: this.fb.array([]),
            comments: ['']
        });

        // Initialize 4 trials
        for (let i = 0; i < 4; i++) {
            this.addTrial(i);
        }
    }

    private addTrial(trialIndex: number): void {
        const trialControls: any = {
            isComplete: [false],
            scheduleFerrography: [false]
        };

        // Add all element fields
        this.elementFields.forEach(element => {
            trialControls[element] = [null, [Validators.min(0)]];
        });

        const trialGroup = this.fb.group(trialControls);
        this.trialsArray.push(trialGroup);
    }

    private populateFormWithResults(data: EmissionSpectroscopyData[]): void {
        // Clear existing trials
        while (this.trialsArray.length > 0) {
            this.trialsArray.removeAt(0);
        }

        // Group data by trial number
        const trialData = new Map<number, EmissionSpectroscopyData>();
        data.forEach(item => {
            trialData.set(item.trialNum, item);
        });

        // Create trials (ensure we have at least 4)
        const maxTrials = Math.max(4, Math.max(...data.map(d => d.trialNum)));
        for (let i = 1; i <= maxTrials; i++) {
            const trial = trialData.get(i);
            const trialControls: any = {
                isComplete: [trial?.status === 'C' || false],
                scheduleFerrography: [false] // This would need to be determined from the data
            };

            // Add element fields with values
            this.elementFields.forEach(element => {
                const value = trial ? (trial as any)[element] : null;
                trialControls[element] = [value, [Validators.min(0)]];
            });

            const trialGroup = this.fb.group(trialControls);
            this.trialsArray.push(trialGroup);
        }
    }

    onTrialCompleteChange(trialIndex: number): void {
        const trialGroup = this.trialsArray.at(trialIndex);
        const isComplete = trialGroup.get('isComplete')?.value;

        if (isComplete) {
            // Check if at least one element field has a value
            const hasData = this.elementFields.some(element => {
                const value = trialGroup.get(element)?.value;
                return value !== null && value !== undefined && value !== '';
            });

            if (!hasData) {
                trialGroup.get('isComplete')?.setValue(false);
                this.snackBar.open('Please enter at least one element value before marking trial as complete', 'Close', {
                    duration: 3000
                });
            }
        }
    }

    onFerrographyScheduleChange(trialIndex: number): void {
        // Only allow Ferrography scheduling on Trial 1
        if (trialIndex !== 0) {
            return;
        }

        const trialGroup = this.trialsArray.at(trialIndex);
        const scheduleFerrography = trialGroup.get('scheduleFerrography')?.value;

        if (scheduleFerrography) {
            this.snackBar.open('Ferrography will be scheduled when this trial is saved', 'Close', {
                duration: 3000,
                panelClass: ['info-snackbar']
            });
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
        if (!this.emissionSpectroscopyForm.valid) {
            this.markFormGroupTouched(this.emissionSpectroscopyForm);
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

        const formValue = this.emissionSpectroscopyForm.value;
        let savedCount = 0;

        // Save each trial that has data
        formValue.trials.forEach((trial: any, index: number) => {
            const hasData = this.elementFields.some(element => {
                const value = trial[element];
                return value !== null && value !== undefined && value !== '';
            });

            if (hasData || trial.isComplete) {
                const request: EmissionSpectroscopyCreateRequest = {
                    id: sample.id,
                    testId: this.testId,
                    trialNum: index + 1,
                    status: trial.isComplete ? 'C' : 'E',
                    scheduleFerrography: trial.scheduleFerrography && index === 0,
                    ...this.elementFields.reduce((acc, element) => {
                        acc[element] = trial[element];
                        return acc;
                    }, {} as any)
                };

                // Check if this trial already exists
                const existingTrial = this.emissionSpectroscopyData().find(d => d.trialNum === index + 1);

                if (existingTrial) {
                    // Update existing trial
                    const updateRequest: EmissionSpectroscopyUpdateRequest = {
                        status: request.status,
                        scheduleFerrography: request.scheduleFerrography,
                        ...this.elementFields.reduce((acc, element) => {
                            acc[element] = trial[element];
                            return acc;
                        }, {} as any)
                    };

                    this.emissionSpectroscopyService.updateEmissionSpectroscopyData(
                        sample.id, this.testId, index + 1, updateRequest
                    ).subscribe({
                        next: () => {
                            savedCount++;
                            this.checkSaveCompletion(savedCount, formValue.trials.length);
                        },
                        error: (error) => {
                            this.snackBar.open(`Failed to update trial ${index + 1}: ${error.message}`, 'Close', {
                                duration: 5000,
                                panelClass: ['error-snackbar']
                            });
                        }
                    });
                } else {
                    // Create new trial
                    this.emissionSpectroscopyService.createEmissionSpectroscopyData(request).subscribe({
                        next: () => {
                            savedCount++;
                            this.checkSaveCompletion(savedCount, formValue.trials.length);
                        },
                        error: (error) => {
                            this.snackBar.open(`Failed to save trial ${index + 1}: ${error.message}`, 'Close', {
                                duration: 5000,
                                panelClass: ['error-snackbar']
                            });
                        }
                    });
                }
            }
        });

        if (savedCount === 0) {
            this.snackBar.open('No data to save. Please enter at least one element value.', 'Close', {
                duration: 3000,
                panelClass: ['warning-snackbar']
            });
        }
    }

    private checkSaveCompletion(savedCount: number, totalTrials: number): void {
        if (savedCount === totalTrials) {
            this.snackBar.open(`Successfully saved ${savedCount} trial records`, 'Close', {
                duration: 3000,
                panelClass: ['success-snackbar']
            });

            const sample = this.selectedSample();
            if (sample) {
                this.loadTestHistory(sample.id);
            }
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
                this.emissionSpectroscopyForm.reset();
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
                message: 'Are you sure you want to delete all emission spectroscopy results for this sample? This action cannot be undone.',
                confirmText: 'Delete',
                cancelText: 'Cancel'
            }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result && sample) {
                const data = this.emissionSpectroscopyData();
                let deletedCount = 0;

                data.forEach(item => {
                    this.emissionSpectroscopyService.deleteEmissionSpectroscopyData(
                        sample.id, this.testId, item.trialNum
                    ).subscribe({
                        next: () => {
                            deletedCount++;
                            if (deletedCount === data.length) {
                                this.snackBar.open(`Successfully deleted ${deletedCount} records`, 'Close', {
                                    duration: 3000,
                                    panelClass: ['success-snackbar']
                                });
                                this.emissionSpectroscopyForm.reset();
                                this.initializeForm();
                                this.loadTestHistory(sample.id);
                            }
                        },
                        error: (error) => {
                            this.snackBar.open(`Failed to delete trial ${item.trialNum}: ${error.message}`, 'Close', {
                                duration: 5000,
                                panelClass: ['error-snackbar']
                            });
                        }
                    });
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
        this.emissionSpectroscopyService.clearError();
    }

    hasExistingResults(): boolean {
        return this.emissionSpectroscopyData().length > 0;
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