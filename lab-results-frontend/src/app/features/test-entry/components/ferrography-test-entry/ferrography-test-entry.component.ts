import { Component, OnInit, OnDestroy, inject, signal, computed, effect } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Subject, takeUntil, forkJoin } from 'rxjs';

import { SampleService } from '../../../../shared/services/sample.service';
import { TestService } from '../../../../shared/services/test.service';
import { ValidationService } from '../../../../shared/services/validation.service';
import { Sample } from '../../../../shared/models/sample.model';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { SharedModule } from '../../../../shared/shared.module';
import { ParticleAnalysisCardComponent } from '../../../../shared/components/particle-analysis-card/particle-analysis-card.component';
import {
    ParticleAnalysisData,
    ParticleAnalysisConfig,
    ParticleAnalysisValidation
} from '../../../../shared/models/particle-analysis.model';

import {
    calculateOverallSeverity as calculateParticleSeverity,
    getSeverityDescription,
    getSeverityColorClass,
    getSeverityRecommendations
} from '../../../../shared/utils/severity-mapping.util';

// Interfaces for Ferrography Analysis
interface ParticleTypeDefinition {
    id: number;
    type: string;
    description: string;
    image1: string;
    image2: string;
    active: boolean;
    sortOrder: number;
}

interface ParticleSubTypeCategory {
    id: number;
    description: string;
    active: boolean;
    sortOrder: number;
    subTypes: ParticleSubTypeDefinition[];
}

interface ParticleSubTypeDefinition {
    particleSubTypeCategoryId: number;
    value: number;
    description: string;
    active: boolean;
    sortOrder: number;
}

interface ParticleType {
    id: number;
    name: string;
    description: string;
    heat?: string;
    concentration?: string;
    sizeAvg?: string;
    sizeMax?: string;
    color?: string;
    texture?: string;
    composition?: string;
    severity?: number;
    comment?: string;
    appendComment: boolean;
}

interface ParticleAnalysis {
    sampleId: number;
    testId: number;
    particleTypeDefinitionId: number;
    status: string;
    comments: string;
    subTypeValues: { [categoryId: number]: number | null };
}

interface FerrographyResult {
    sampleId: number;
    testId: number;
    particleAnalyses: ParticleAnalysis[];
    dilutionFactor: string;
    overallSeverity: number;
    status: string;
}

@Component({
    selector: 'app-ferrography-test-entry',
    standalone: true,
    imports: [SharedModule, ParticleAnalysisCardComponent],
    template: `
        <div class="ferrography-container">
            <!-- Header Section -->
            <mat-card class="header-card">
                <mat-card-header>
                    <mat-card-title>Ferrography Test Entry</mat-card-title>
                    <mat-card-subtitle>
                        @if (currentSample()) {
                            Sample: {{ currentSample()!.tagNumber }} - {{ currentSample()!.component }} ({{ currentSample()!.location }})
                        } @else {
                            No sample selected
                        }
                    </mat-card-subtitle>
                </mat-card-header>
                
                <!-- Sample Selection Buttons -->
                @if (availableSamples().length > 0) {
                    <mat-card-content>
                        <div class="sample-selection">
                            <label class="sample-selection-label">Available Samples:</label>
                            <div class="sample-buttons">
                                @for (sample of availableSamples(); track sample.id) {
                                    <button 
                                        mat-raised-button 
                                        [color]="currentSample()?.id === sample.id ? 'primary' : ''"
                                        (click)="selectSample(sample)"
                                        class="sample-button">
                                        Sample {{ sample.id }}
                                        <span class="sample-tag">{{ sample.tagNumber }}</span>
                                    </button>
                                }
                            </div>
                        </div>
                    </mat-card-content>
                }
            </mat-card>

            <!-- Loading Spinner -->
            @if (isLoading()) {
                <div class="loading-container">
                    <mat-spinner></mat-spinner>
                    <p>Loading Ferrography data...</p>
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
            @if (ferrographyForm && !loadingParticleData()) {
                <form [formGroup]="ferrographyForm" (ngSubmit)="onSave()">
                    <!-- Sample Information Display -->
                    <mat-card class="sample-info-card">
                        <mat-card-content>
                            @if (currentSample()) {
                                <div class="sample-info-grid">
                                    <div class="info-item">
                                        <label>Tag Number:</label>
                                        <span>{{ currentSample()!.tagNumber }}</span>
                                    </div>
                                    <div class="info-item">
                                        <label>Component:</label>
                                        <span>{{ currentSample()!.component }}</span>
                                    </div>
                                    <div class="info-item">
                                        <label>Location:</label>
                                        <span>{{ currentSample()!.location }}</span>
                                    </div>
                                    <div class="info-item">
                                        <label>Lube Type:</label>
                                        <span>{{ currentSample()!.lubeType }}</span>
                                    </div>
                                    <div class="info-item">
                                        <label>Sample Date:</label>
                                        <span>{{ currentSample()!.sampleDate | date:'short' }}</span>
                                    </div>
                                    @if (currentSample()!.qualityClass) {
                                        <div class="info-item">
                                            <label>Quality Class:</label>
                                            <span>{{ currentSample()!.qualityClass }}</span>
                                        </div>
                                    }
                                </div>
                            } @else {
                                <div class="sample-info-grid">
                                    <div class="info-item">
                                        <label>No sample selected</label>
                                        <span>Please select a sample from the list</span>
                                    </div>
                                </div>
                            }
                        </mat-card-content>
                    </mat-card>

                    <!-- Dilution Factor Section -->
                    <mat-card class="dilution-factor-card">
                        <mat-card-header>
                            <mat-card-title>Dilution Factor</mat-card-title>
                            <mat-card-subtitle>Select the dilution factor for this analysis</mat-card-subtitle>
                        </mat-card-header>
                        <mat-card-content>
                            <div class="dilution-controls">
                                <mat-form-field appearance="outline">
                                    <mat-label>Dilution Factor</mat-label>
                                    <mat-select formControlName="dilutionFactor" (selectionChange)="onDilutionFactorChange()">
                                        <mat-option value="">Select Dilution Factor</mat-option>
                                        <mat-option value="3:2">3:2</mat-option>
                                        <mat-option value="1:10">1:10</mat-option>
                                        <mat-option value="1:100">1:100</mat-option>
                                        <mat-option value="X/YYYY">X/YYYY (Custom)</mat-option>
                                    </mat-select>
                                    <mat-hint>Selecting a dilution factor will change test status from X to E</mat-hint>
                                </mat-form-field>

                                @if (ferrographyForm.get('dilutionFactor')?.value === 'X/YYYY') {
                                    <mat-form-field appearance="outline">
                                        <mat-label>Custom Dilution Factor</mat-label>
                                        <input matInput formControlName="customDilutionFactor" placeholder="Enter custom dilution factor">
                                        <mat-hint>Enter the custom dilution factor (e.g., 1:50, 2:25)</mat-hint>
                                    </mat-form-field>
                                }

                                <div class="status-info">
                                    <span class="status-label">Test Status:</span>
                                    <span class="status-value" [class]="'status-' + testStatus().toLowerCase()">
                                        {{ getStatusText(testStatus()) }}
                                    </span>
                                </div>

                                <button 
                                    mat-raised-button 
                                    color="accent" 
                                    type="button"
                                    (click)="onPartialSave()"
                                    [disabled]="!ferrographyForm.get('dilutionFactor')?.value || isLoading()">
                                    <mat-icon>save_alt</mat-icon>
                                    Partial Save
                                </button>
                            </div>
                        </mat-card-content>
                    </mat-card>

                    <!-- Overall Assessment Section -->
                    <mat-card class="overall-assessment-card">
                        <mat-card-header>
                            <mat-card-title>Overall Assessment</mat-card-title>
                        </mat-card-header>
                        <mat-card-content>
                            <div class="overall-controls">
                                <mat-form-field appearance="outline">
                                    <mat-label>Overall Severity</mat-label>
                                    <mat-select formControlName="overallSeverity">
                                        <mat-option [value]="null">Not Selected</mat-option>
                                        <mat-option [value]="1">1 - Normal</mat-option>
                                        <mat-option [value]="2">2 - Slight</mat-option>
                                        <mat-option [value]="3">3 - Moderate</mat-option>
                                        <mat-option [value]="4">4 - Severe</mat-option>
                                    </mat-select>
                                </mat-form-field>

                                <mat-form-field appearance="outline" class="full-width">
                                    <mat-label>Overall Comments ({{ remainingChars }} chars remaining)</mat-label>
                                    <textarea 
                                        matInput 
                                        formControlName="overallComments"
                                        maxlength="1000"
                                        rows="4">
                                    </textarea>
                                </mat-form-field>
                            </div>
                        </mat-card-content>
                    </mat-card>

                    <!-- Particle Analysis Card -->
                    @if (showParticleAnalysis()) {
                        <app-particle-analysis-card
                            [particleTypes]="particleTypeDefinitions"
                            [subTypeCategories]="subTypeCategories()"
                            [initialData]="particleAnalysisData()"
                            [readonly]="false"
                            [showImages]="true"
                            [config]="particleAnalysisConfig()"
                            (particleDataChange)="onParticleDataChange($event)"
                            (severityChange)="onParticleSeverityChange($event)"
                            (validationChange)="onParticleValidationChange($event)">
                        </app-particle-analysis-card>
                    }

                    <!-- Action Buttons -->
                    <div class="action-buttons">
                        <button 
                            mat-raised-button 
                            color="primary" 
                            type="submit"
                            [disabled]="!ferrographyForm.valid || isLoading()">
                            <mat-icon>save</mat-icon>
                            Save Complete Results
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
                        <mat-card-title>Last 12 Results for Ferrography Test</mat-card-title>
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

                                <ng-container matColumnDef="dilutionFactor">
                                    <th mat-header-cell *matHeaderCellDef>Dilution Factor</th>
                                    <td mat-cell *matCellDef="let result">{{ result.dilutionFactor || 'N/A' }}</td>
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
        .ferrography-container {
            padding: 20px;
            max-width: 1400px;
            margin: 0 auto;
        }

        .header-card, .sample-info-card, .dilution-factor-card, .overall-assessment-card, .particle-analysis-card, .history-card {
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

        .dilution-controls, .overall-controls {
            display: flex;
            align-items: center;
            gap: 20px;
            flex-wrap: wrap;
        }

        .dilution-controls mat-form-field, .overall-controls mat-form-field {
            min-width: 200px;
        }

        .status-info {
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .status-label {
            font-weight: 500;
            color: #666;
        }

        .status-value {
            font-weight: bold;
            font-size: 1.1em;
            padding: 4px 8px;
            border-radius: 4px;
        }

        .status-c { background-color: #e8f5e8; color: #2e7d32; }
        .status-e { background-color: #fff3e0; color: #f57c00; }
        .status-x { background-color: #f5f5f5; color: #757575; }

        .severity-info {
            margin-left: auto;
            display: flex;
            align-items: center;
            gap: 10px;
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

        .particles-section {
            display: flex;
            flex-direction: column;
            gap: 16px;
        }

        .particle-form {
            padding: 16px 0;
        }

        .form-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 16px;
            margin-bottom: 16px;
        }

        .severity-badge {
            margin-left: 12px;
            padding: 4px 8px;
            background-color: #ff9800;
            color: white;
            border-radius: 12px;
            font-size: 12px;
        }

        .full-width {
            width: 100%;
            margin: 12px 0;
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

        .error-text {
            color: #f44336;
            font-weight: 500;
        }



        @media (max-width: 768px) {
            .ferrography-container {
                padding: 10px;
            }
            
            .action-buttons {
                flex-direction: column;
            }
            
            .action-buttons button {
                width: 100%;
            }
            
            .dilution-controls, .overall-controls {
                flex-direction: column;
                align-items: flex-start;
            }
        }
    `]
})
export class FerrographyTestEntryComponent implements OnInit, OnDestroy {
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
    readonly loadingParticleData = signal(false);
    readonly particleDataError = signal<string | null>(null);
    readonly particleTypes = signal<ParticleType[]>([]);
    readonly subTypeCategories = signal<ParticleSubTypeCategory[]>([]);
    readonly testStatus = signal<string>('X');
    readonly particleAnalysisData = signal<ParticleAnalysisData | null>(null);
    readonly particleAnalysisConfig = signal<ParticleAnalysisConfig>({
        testId: 210,
        readonly: false,
        showImages: true,
        viewFilter: 'all',
        enableSeverityCalculation: true
    });
    readonly overallSeverity = computed(() => this.calculateOverallSeverity());
    readonly showParticleAnalysis = computed(() => {
        // Always show particle analysis if particle types are loaded
        return this.particleTypes().length > 0;
    });

    // Sample data signals
    readonly currentSample = signal<Sample | null>(null);
    readonly availableSamples = signal<Sample[]>([]);
    readonly loadingSamples = signal(false);

    // Component state
    ferrographyForm!: FormGroup;
    testId = 210; // Ferrography test ID - matches the seeded database
    historyDisplayedColumns = ['sampleId', 'entryDate', 'status', 'dilutionFactor', 'severity'];
    currentDate = new Date();
    particleTypeDefinitions: any[] = []; // Store particle type definitions for the particle analysis card



    get remainingChars(): number {
        const comments = this.ferrographyForm?.get('overallComments')?.value || '';
        return 1000 - comments.length;
    }

    constructor() {
        // Constructor - effects removed to avoid dependency issues
    }

    ngOnInit(): void {
        // Load available samples for Ferrography test
        this.loadAvailableSamples();

        // Get sample ID from route parameters
        this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
            const sampleId = +params['sampleId'];
            if (sampleId) {
                this.loadSample(sampleId);
            }
        });

        // Load particle analysis data and initialize form
        this.loadParticleAnalysisData();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadAvailableSamples(): void {
        this.loadingSamples.set(true);
        this.sampleService.getSamplesByTest(this.testId).subscribe({
            next: (samples) => {
                this.availableSamples.set(samples);
                this.loadingSamples.set(false);

                // If we have samples and no current sample, select the first one
                if (samples.length > 0 && !this.currentSample()) {
                    this.currentSample.set(samples[0]);
                }
            },
            error: (error) => {
                console.error('Error loading samples:', error);
                this.snackBar.open('Failed to load samples', 'Close', { duration: 3000 });
                this.loadingSamples.set(false);
            }
        });
    }

    private loadSample(sampleId: number): void {
        this.sampleService.getSample(sampleId).subscribe({
            next: (sample) => {
                this.currentSample.set(sample);
                this.loadExistingResults(sampleId);
                this.loadTestHistory(sampleId);
            },
            error: (error) => {
                console.error('Error loading sample:', error);
                this.snackBar.open('Failed to load sample details', 'Close', { duration: 3000 });
            }
        });
    }

    private loadParticleAnalysisData(): void {
        this.loadingParticleData.set(true);
        this.particleDataError.set(null);

        // Load both particle types and sub-type categories from API
        forkJoin({
            particleTypes: this.testService.getParticleTypes(),
            subTypeCategories: this.testService.getSubTypeCategories()
        }).subscribe({
            next: (data) => {
                // Convert API particle types to ParticleTypeDefinition format for the particle analysis card
                const particleTypeDefinitions = data.particleTypes.map(pt => ({
                    id: pt.id,
                    type: pt.type || '',
                    description: pt.description || '',
                    image1: pt.image1 || '',
                    image2: pt.image2 || '',
                    active: pt.active,
                    sortOrder: pt.sortOrder,
                    category: this.categorizeParticleType(pt.type || '') as 'wear' | 'oxide' | 'contaminant' | 'other'
                }));

                // Also convert to internal ParticleType format for legacy compatibility
                const particleTypes: ParticleType[] = data.particleTypes.map(pt => ({
                    id: pt.id,
                    name: pt.type || '',
                    description: pt.description || '',
                    appendComment: false
                }));

                this.particleTypes.set(particleTypes);
                this.particleTypeDefinitions = particleTypeDefinitions;
                this.subTypeCategories.set(data.subTypeCategories);
                this.initializeForm();
                this.loadingParticleData.set(false);
            },
            error: (error) => {
                this.particleDataError.set(`Failed to load particle analysis data: ${error.message}`);
                this.loadingParticleData.set(false);
                // Initialize with empty data as fallback
                this.particleTypes.set([]);
                this.subTypeCategories.set([]);
                this.initializeForm();
            }
        });
    }

    private loadExistingResults(sampleId: number): void {
        // TODO: Implement when ferrography results endpoint is available
    }

    private loadTestHistory(sampleId: number): void {
        // TODO: Implement when test history endpoint is available
    }

    private initializeForm(): void {
        this.ferrographyForm = this.fb.group({
            dilutionFactor: [''],
            customDilutionFactor: [''],
            overallSeverity: [null],
            overallComments: [''],
            particleAnalysis: [null] // New form control for particle analysis data
        });
    }

    private populateFormWithResults(result: FerrographyResult): void {
        // Set dilution factor and status
        this.ferrographyForm.patchValue({
            dilutionFactor: result.dilutionFactor || '',
            overallSeverity: result.overallSeverity
        });

        // Handle custom dilution factor
        if (result.dilutionFactor && !['3:2', '1:10', '1:100'].includes(result.dilutionFactor)) {
            this.ferrographyForm.patchValue({
                dilutionFactor: 'X/YYYY',
                customDilutionFactor: result.dilutionFactor
            });
        }

        this.testStatus.set(result.status || 'X');
    }

    // Get options from sub-type categories
    getHeatOptions() {
        const heatCategory = this.subTypeCategories().find(cat => cat.description === 'Heat');
        return heatCategory?.subTypes || [
            { value: 'NA', description: 'N/A' },
            { value: 'Blue', description: 'Blue' },
            { value: 'Straw', description: 'Straw' },
            { value: 'Purple', description: 'Purple' }
        ];
    }

    getConcentrationOptions() {
        const concentrationCategory = this.subTypeCategories().find(cat => cat.description === 'Concentration');
        return concentrationCategory?.subTypes || [
            { value: 'Few', description: 'Few' },
            { value: 'Moderate', description: 'Moderate' },
            { value: 'Many', description: 'Many' },
            { value: 'Heavy', description: 'Heavy' }
        ];
    }

    getSizeOptions() {
        const sizeCategory = this.subTypeCategories().find(cat => cat.description.includes('Size'));
        return sizeCategory?.subTypes || [
            { value: 'Fine', description: 'Fine (<5μm)' },
            { value: 'Small', description: 'Small (5-15μm)' },
            { value: 'Medium', description: 'Medium (15-40μm)' },
            { value: 'Large', description: 'Large (40-100μm)' }
        ];
    }

    getSeverityOptions() {
        const severityCategory = this.subTypeCategories().find(cat => cat.description === 'Severity');
        return severityCategory?.subTypes || [
            { value: 1, description: 'Low' },
            { value: 2, description: 'Medium' },
            { value: 3, description: 'High' },
            { value: 4, description: 'Critical' }
        ];
    }

    onDilutionFactorChange(): void {
        const dilutionFactor = this.ferrographyForm.get('dilutionFactor')?.value;

        // Update test status based on dilution factor selection
        if (dilutionFactor && dilutionFactor !== '') {
            this.testStatus.set('E');
        } else {
            this.testStatus.set('X');
        }

        // Clear custom dilution factor if not X/YYYY
        if (dilutionFactor !== 'X/YYYY') {
            this.ferrographyForm.get('customDilutionFactor')?.setValue('');
        }
    }

    private calculateOverallSeverity(): number {
        const formSeverity = this.ferrographyForm?.get('overallSeverity')?.value;
        if (formSeverity) return formSeverity;

        // Calculate from particle analysis data using severity mapping utility
        const particleData = this.particleAnalysisData();
        if (particleData && particleData.analyses.length > 0) {
            return calculateParticleSeverity(particleData);
        }

        // Fallback: Calculate from legacy particle severities
        let maxSeverity = 0;
        this.particleTypes().forEach(particle => {
            if (particle.severity && particle.severity > maxSeverity) {
                maxSeverity = particle.severity;
            }
        });

        return maxSeverity;
    }

    /**
     * Handle particle analysis data changes from the particle analysis card
     */
    onParticleDataChange(data: ParticleAnalysisData): void {
        this.particleAnalysisData.set(data);
        this.ferrographyForm.get('particleAnalysis')?.setValue(data);

        // Calculate severity using the mapping utility
        const calculatedSeverity = calculateParticleSeverity(data);

        // Update overall severity if not manually set
        const currentSeverity = this.ferrographyForm.get('overallSeverity')?.value;
        if (!currentSeverity || this.shouldUpdateSeverity(currentSeverity, calculatedSeverity)) {
            this.ferrographyForm.get('overallSeverity')?.setValue(calculatedSeverity);
        }
    }

    /**
     * Handle particle analysis severity changes
     */
    onParticleSeverityChange(severity: number): void {
        // Update overall severity if not manually overridden
        const currentSeverity = this.ferrographyForm.get('overallSeverity')?.value;
        if (!currentSeverity || this.shouldUpdateSeverity(currentSeverity, severity)) {
            this.ferrographyForm.get('overallSeverity')?.setValue(severity);
        }
    }

    /**
     * Handle particle analysis validation changes
     */
    onParticleValidationChange(validation: ParticleAnalysisValidation): void {
        // Update form validity based on particle analysis validation
        const particleAnalysisControl = this.ferrographyForm.get('particleAnalysis');
        if (particleAnalysisControl) {
            if (validation.isValid) {
                particleAnalysisControl.setErrors(null);
            } else {
                particleAnalysisControl.setErrors({ particleValidation: validation.errors });
            }
        }
    }

    /**
     * Determine if severity should be updated based on business rules
     */
    private shouldUpdateSeverity(currentSeverity: number, newSeverity: number): boolean {
        // Always update if new severity is higher
        if (newSeverity > currentSeverity) {
            return true;
        }

        // Update if current severity was calculated (not manually set)
        // This is a simplified check - in a real implementation, you might track
        // whether the severity was manually set by the user
        return newSeverity !== currentSeverity;
    }

    /**
     * Get severity description for display
     */
    getSeverityDescription(severity: number): string {
        return getSeverityDescription(severity);
    }

    /**
     * Get severity color class for styling
     */
    getSeverityColorClass(severity: number): string {
        return getSeverityColorClass(severity);
    }

    /**
     * Get severity recommendations
     */
    getSeverityRecommendations(): string[] {
        const particleData = this.particleAnalysisData();
        if (!particleData) return [];

        const severity = this.overallSeverity();
        return getSeverityRecommendations(severity, particleData.analyses);
    }

    onPartialSave(): void {
        const dilutionFactor = this.ferrographyForm.get('dilutionFactor')?.value;
        const customDilutionFactor = this.ferrographyForm.get('customDilutionFactor')?.value;

        if (!dilutionFactor) {
            this.snackBar.open('Please select a dilution factor', 'Close', {
                duration: 3000,
                panelClass: ['error-snackbar']
            });
            return;
        }

        // TODO: Implement actual save when API endpoint is available
        this.snackBar.open('Partial save successful - dilution factor saved (simulated)', 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar']
        });
        this.testStatus.set('E');
    }

    onSave(): void {
        if (!this.ferrographyForm.valid) {
            this.markFormGroupTouched(this.ferrographyForm);
            this.snackBar.open('Please correct the errors in the form', 'Close', {
                duration: 3000,
                panelClass: ['error-snackbar']
            });
            return;
        }

        const formValue = this.ferrographyForm.value;
        const dilutionFactor = formValue.dilutionFactor === 'X/YYYY' ? formValue.customDilutionFactor : formValue.dilutionFactor;
        const particleAnalyses: ParticleAnalysis[] = [];

        // Build particle analyses from particle types data
        this.particleTypes().forEach(particle => {
            // Only include particle analyses that have some data
            if (particle.heat || particle.concentration || particle.sizeAvg || particle.severity || particle.comment) {
                const subTypeValues: { [categoryId: number]: number | null } = {};

                // Map particle properties to sub-type values (this would need proper mapping based on your API structure)
                if (particle.severity) {
                    subTypeValues[1] = particle.severity; // Assuming category 1 is Severity
                }

                particleAnalyses.push({
                    sampleId: 6, // Mock sample ID
                    testId: this.testId,
                    particleTypeDefinitionId: particle.id,
                    status: 'E',
                    comments: particle.comment || '',
                    subTypeValues
                });
            }
        });

        // Get particle analysis data from the new particle analysis card
        const particleAnalysisData = this.particleAnalysisData();
        const finalParticleAnalyses = particleAnalysisData ?
            particleAnalysisData.analyses.map(analysis => ({
                sampleId: 6, // Mock sample ID
                testId: this.testId,
                particleTypeDefinitionId: analysis.particleTypeId,
                status: 'E',
                comments: analysis.comments || '',
                subTypeValues: analysis.subTypeValues
            })) : particleAnalyses;

        const request = {
            sampleId: 6, // Mock sample ID
            testId: this.testId,
            particleAnalyses: finalParticleAnalyses,
            dilutionFactor: dilutionFactor || '',
            overallSeverity: formValue.overallSeverity || (particleAnalysisData?.overallSeverity || 0),
            overallComments: formValue.overallComments,
            entryId: 'USER', // This should come from authentication
            particleAnalysisData: particleAnalysisData
        };

        // TODO: Implement actual save when API endpoint is available
        this.snackBar.open('Successfully saved ferrography results (simulated)', 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar']
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
                this.ferrographyForm.reset();
                this.initializeForm();
                this.testStatus.set('X');

                // Clear particle data
                const clearedParticles = this.particleTypes().map(p => ({
                    ...p,
                    heat: undefined,
                    concentration: undefined,
                    sizeAvg: undefined,
                    sizeMax: undefined,
                    color: undefined,
                    texture: undefined,
                    composition: undefined,
                    severity: undefined,
                    comment: undefined,
                    appendComment: false
                }));
                this.particleTypes.set(clearedParticles);

                this.snackBar.open('Form cleared', 'Close', { duration: 2000 });
            }
        });
    }

    onDelete(): void {

        const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
            data: {
                title: 'Delete Test Results',
                message: 'Are you sure you want to delete all Ferrography results for this sample? This action cannot be undone.',
                confirmText: 'Delete',
                cancelText: 'Cancel'
            }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                // TODO: Implement actual delete when API endpoint is available
                this.snackBar.open('Successfully deleted ferrography results (simulated)', 'Close', {
                    duration: 3000,
                    panelClass: ['success-snackbar']
                });
                this.ferrographyForm.reset();
                this.initializeForm();
                this.testStatus.set('X');
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
        // Check if dilution factor is set or any particle analysis has data
        const hasDilutionFactor = this.ferrographyForm?.get('dilutionFactor')?.value;
        const hasParticleData = this.particleTypes().some(particle =>
            particle.heat || particle.concentration || particle.sizeAvg || particle.severity || particle.comment
        );

        return hasDilutionFactor || hasParticleData;
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

    /**
     * Categorize particle type based on its name
     */
    private categorizeParticleType(typeName: string): 'wear' | 'oxide' | 'contaminant' | 'other' {
        const lowerName = typeName.toLowerCase();

        if (lowerName.includes('oxide') || lowerName.includes('rust')) {
            return 'oxide';
        } else if (lowerName.includes('fiber') || lowerName.includes('contaminant') ||
            lowerName.includes('crystalline') || lowerName.includes('amorphous') ||
            lowerName.includes('polymer') || lowerName.includes('corrosive')) {
            return 'contaminant';
        } else if (lowerName.includes('wear') || lowerName.includes('rubbing') ||
            lowerName.includes('abrasive') || lowerName.includes('severe') ||
            lowerName.includes('chunk') || lowerName.includes('sphere') ||
            lowerName.includes('metal') || lowerName.includes('rework')) {
            return 'wear';
        } else {
            return 'other';
        }
    }
}