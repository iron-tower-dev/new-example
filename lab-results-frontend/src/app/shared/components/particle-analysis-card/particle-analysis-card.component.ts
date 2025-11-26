import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, computed, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, FormControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';

import {
    ParticleAnalysisData,
    ParticleAnalysis,
    ParticleTypeDefinition,
    ParticleSubTypeCategory,
    ParticleAnalysisConfig,
    ParticleAnalysisValidation
} from '../../models/particle-analysis.model';

/**
 * Reusable particle analysis card component for ferrography and inspect filter tests
 * 
 * This component provides a comprehensive interface for particle type analysis,
 * including sub-type categorization, severity assessment, and data validation.
 */
@Component({
    selector: 'app-particle-analysis-card',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatExpansionModule,
        MatButtonModule,
        MatIconModule,
        MatTooltipModule
    ],
    templateUrl: './particle-analysis-card.component.html',
    styleUrls: ['./particle-analysis-card.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ParticleAnalysisCardComponent implements OnInit, OnDestroy {
    private destroy$ = new Subject<void>();
    private fb = new FormBuilder();

    // Input properties
    @Input() particleTypes: ParticleTypeDefinition[] = [];
    @Input() subTypeCategories: ParticleSubTypeCategory[] = [];
    @Input() initialData: ParticleAnalysisData | null = null;
    @Input() readonly: boolean = false;
    @Input() showImages: boolean = true;
    @Input() config: ParticleAnalysisConfig | null = null;

    // Output events
    @Output() particleDataChange = new EventEmitter<ParticleAnalysisData>();
    @Output() severityChange = new EventEmitter<number>();
    @Output() validationChange = new EventEmitter<ParticleAnalysisValidation>();

    // Component state
    particleAnalysisForm: FormGroup;
    isLoading = signal<boolean>(false);
    errorMessage = signal<string | null>(null);

    // Computed properties
    filteredParticleTypes = computed(() => this.filterParticleTypes());
    overallSeverity = computed(() => this.calculateOverallSeverity());
    validationResult = computed(() => this.validateParticleData());

    constructor() {
        this.particleAnalysisForm = this.fb.group({
            analyses: this.fb.array([])
        });
    }

    ngOnInit(): void {
        this.initializeComponent();
        this.setupFormChangeHandling();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    /**
     * Initialize the component with provided data
     */
    private initializeComponent(): void {
        if (this.initialData) {
            this.populateFormWithData(this.initialData);
        } else {
            this.initializeEmptyForm();
        }
    }

    /**
     * Set up form change handling with debouncing
     */
    private setupFormChangeHandling(): void {
        if (!this.readonly) {
            this.particleAnalysisForm.valueChanges
                .pipe(
                    debounceTime(300),
                    distinctUntilChanged(),
                    takeUntil(this.destroy$)
                )
                .subscribe(() => {
                    this.onFormChange();
                });
        }
    }

    /**
     * Handle form changes and emit events
     */
    private onFormChange(): void {
        const particleData = this.getParticleAnalysisData();
        const severity = this.overallSeverity();
        const validation = this.validationResult();

        this.particleDataChange.emit(particleData);
        this.severityChange.emit(severity);
        this.validationChange.emit(validation);
    }

    /**
     * Get the analyses FormArray
     */
    get analysesFormArray(): FormArray {
        return this.particleAnalysisForm.get('analyses') as FormArray;
    }

    /**
     * Filter particle types based on configuration
     */
    private filterParticleTypes(): ParticleTypeDefinition[] {
        if (!this.config?.viewFilter || this.config.viewFilter === 'all') {
            return this.particleTypes.filter(pt => pt.active);
        }

        // Additional filtering logic can be added here
        return this.particleTypes.filter(pt => pt.active);
    }

    /**
     * Calculate overall severity from all particle analyses
     */
    private calculateOverallSeverity(): number {
        if (!this.config?.enableSeverityCalculation) {
            return 0;
        }

        const analyses = this.analysesFormArray.value as ParticleAnalysis[];
        if (!analyses || analyses.length === 0) {
            return 0;
        }

        const severities = analyses
            .map(analysis => analysis.severity || 0)
            .filter(severity => severity > 0);

        if (severities.length === 0) {
            return 0;
        }

        // Calculate weighted average or maximum severity
        return Math.max(...severities);
    }

    /**
     * Validate particle analysis data
     */
    private validateParticleData(): ParticleAnalysisValidation {
        const errors: { [particleTypeId: number]: string[] } = {};
        const warnings: { [particleTypeId: number]: string[] } = {};
        let isValid = true;

        const analyses = this.analysesFormArray.value as ParticleAnalysis[];

        analyses.forEach(analysis => {
            const particleErrors: string[] = [];
            const particleWarnings: string[] = [];

            // Validate required fields
            if (analysis.severity > 0 && !analysis.comments?.trim()) {
                particleWarnings.push('Comments recommended for particles with severity > 0');
            }

            // Validate sub-type values consistency
            const hasSubTypeValues = Object.values(analysis.subTypeValues || {})
                .some(value => value !== null && value !== undefined);

            if (hasSubTypeValues && analysis.severity === 0) {
                particleWarnings.push('Consider setting severity when sub-type values are present');
            }

            if (particleErrors.length > 0) {
                errors[analysis.particleTypeId] = particleErrors;
                isValid = false;
            }

            if (particleWarnings.length > 0) {
                warnings[analysis.particleTypeId] = particleWarnings;
            }
        });

        return { isValid, errors, warnings };
    }

    /**
     * Initialize empty form with particle types
     */
    private initializeEmptyForm(): void {
        const analysesArray = this.fb.array<FormGroup>([]);

        this.filteredParticleTypes().forEach(particleType => {
            const analysisGroup = this.createParticleAnalysisGroup(particleType);
            analysesArray.push(analysisGroup);
        });

        this.particleAnalysisForm.setControl('analyses', analysesArray);
    }

    /**
     * Populate form with existing data
     */
    private populateFormWithData(data: ParticleAnalysisData): void {
        const analysesArray = this.fb.array<FormGroup>([]);

        this.filteredParticleTypes().forEach(particleType => {
            const existingAnalysis = data.analyses.find((a: ParticleAnalysis) => a.particleTypeId === particleType.id);
            const analysisGroup = this.createParticleAnalysisGroup(particleType, existingAnalysis);
            analysesArray.push(analysisGroup);
        });

        this.particleAnalysisForm.setControl('analyses', analysesArray);
    }

    /**
     * Create form group for a particle analysis
     */
    private createParticleAnalysisGroup(
        particleType: ParticleTypeDefinition,
        existingData?: ParticleAnalysis
    ): FormGroup {
        const subTypeControls: { [key: string]: FormControl } = {};

        // Create form controls for each sub-type category
        this.subTypeCategories.forEach(category => {
            const controlName = `category_${category.id}`;
            const existingValue = existingData?.subTypeValues?.[category.id] || null;
            subTypeControls[controlName] = new FormControl(existingValue);
        });

        return this.fb.group({
            particleTypeId: [particleType.id],
            subTypeValues: this.fb.group(subTypeControls),
            comments: [existingData?.comments || ''],
            severity: [existingData?.severity || 0],
            status: [existingData?.status || 'active']
        });
    }

    /**
     * Get current particle analysis data from form
     */
    private getParticleAnalysisData(): ParticleAnalysisData {
        const formValue = this.particleAnalysisForm.value;
        const analyses: ParticleAnalysis[] = formValue.analyses.map((analysis: any) => ({
            particleTypeId: analysis.particleTypeId,
            subTypeValues: this.transformSubTypeValues(analysis.subTypeValues),
            comments: analysis.comments || '',
            severity: analysis.severity || 0,
            status: analysis.status || 'active'
        }));

        const overallSeverity = this.overallSeverity();
        const validation = this.validationResult();

        return {
            analyses,
            overallSeverity,
            isValid: validation.isValid,
            summary: {
                totalParticles: analyses.length,
                criticalParticles: analyses.filter(a => a.severity >= 3).length,
                recommendations: this.generateRecommendations(analyses)
            }
        };
    }

    /**
     * Transform sub-type values from form format to data format
     */
    private transformSubTypeValues(subTypeValues: any): { [categoryId: number]: number | null } {
        const result: { [categoryId: number]: number | null } = {};

        Object.keys(subTypeValues).forEach(key => {
            if (key.startsWith('category_')) {
                const categoryId = parseInt(key.replace('category_', ''), 10);
                result[categoryId] = subTypeValues[key];
            }
        });

        return result;
    }

    /**
     * Generate recommendations based on particle analysis
     */
    private generateRecommendations(analyses: ParticleAnalysis[]): string[] {
        const recommendations: string[] = [];

        const criticalParticles = analyses.filter(a => a.severity >= 3);
        if (criticalParticles.length > 0) {
            recommendations.push('Critical wear particles detected - investigate root cause');
        }

        const moderateParticles = analyses.filter(a => a.severity === 2);
        if (moderateParticles.length > 2) {
            recommendations.push('Multiple moderate severity particles - monitor closely');
        }

        return recommendations;
    }

    /**
     * Track by function for particle type iteration
     */
    trackByParticleId(index: number, particle: ParticleTypeDefinition): number {
        return particle.id;
    }

    /**
     * Get particle type by ID
     */
    getParticleTypeById(id: number): ParticleTypeDefinition | undefined {
        return this.particleTypes.find(pt => pt.id === id);
    }

    /**
     * Get sub-type category options for a specific category
     */
    getCategoryOptions(categoryDescription: string) {
        const category = this.subTypeCategories.find(cat =>
            cat.description.toLowerCase() === categoryDescription.toLowerCase()
        );
        return category?.subTypes || [];
    }

    /**
     * Reset form to initial state
     */
    resetForm(): void {
        if (this.initialData) {
            this.populateFormWithData(this.initialData);
        } else {
            this.initializeEmptyForm();
        }
    }

    /**
     * Clear all form data
     */
    clearForm(): void {
        this.initializeEmptyForm();
    }

    /**
     * Get warning keys for template iteration
     */
    getWarningKeys(): string[] {
        const warnings = this.validationResult().warnings;
        return warnings ? Object.keys(warnings) : [];
    }
}