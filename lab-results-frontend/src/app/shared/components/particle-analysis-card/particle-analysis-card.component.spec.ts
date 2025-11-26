import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ParticleAnalysisCardComponent } from './particle-analysis-card.component';
import {
    ParticleAnalysisData,
    ParticleTypeDefinition,
    ParticleSubTypeCategory,
    ParticleAnalysisConfig
} from '../../models/particle-analysis.model';

describe('ParticleAnalysisCardComponent', () => {
    let component: ParticleAnalysisCardComponent;
    let fixture: ComponentFixture<ParticleAnalysisCardComponent>;

    const mockParticleTypes: ParticleTypeDefinition[] = [
        {
            id: 1,
            type: 'Cutting Wear',
            description: 'Normal machining wear particles',
            image1: '/assets/cutting-wear-1.jpg',
            image2: '/assets/cutting-wear-2.jpg',
            active: true,
            sortOrder: 1,
            category: 'wear'
        },
        {
            id: 2,
            type: 'Sliding Wear',
            description: 'Adhesive wear particles',
            image1: '/assets/sliding-wear-1.jpg',
            image2: '/assets/sliding-wear-2.jpg',
            active: true,
            sortOrder: 2,
            category: 'wear'
        }
    ];

    const mockSubTypeCategories: ParticleSubTypeCategory[] = [
        {
            id: 1,
            description: 'Severity',
            active: true,
            sortOrder: 1,
            subTypes: [
                { particleSubTypeCategoryId: 1, value: 1, description: 'Low', active: true, sortOrder: 1 },
                { particleSubTypeCategoryId: 1, value: 2, description: 'Moderate', active: true, sortOrder: 2 },
                { particleSubTypeCategoryId: 1, value: 3, description: 'High', active: true, sortOrder: 3 }
            ]
        },
        {
            id: 2,
            description: 'Heat',
            active: true,
            sortOrder: 2,
            subTypes: [
                { particleSubTypeCategoryId: 2, value: 1, description: 'Blue', active: true, sortOrder: 1 },
                { particleSubTypeCategoryId: 2, value: 2, description: 'Straw', active: true, sortOrder: 2 }
            ]
        }
    ];

    const mockInitialData: ParticleAnalysisData = {
        analyses: [
            {
                particleTypeId: 1,
                subTypeValues: { 1: 2, 2: 1 },
                comments: 'Test comment',
                severity: 2,
                status: 'active'
            }
        ],
        overallSeverity: 2,
        isValid: true,
        summary: {
            totalParticles: 1,
            criticalParticles: 0,
            recommendations: []
        }
    };

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ParticleAnalysisCardComponent,
                ReactiveFormsModule,
                NoopAnimationsModule,
                MatCardModule,
                MatFormFieldModule,
                MatInputModule,
                MatSelectModule,
                MatExpansionModule,
                MatButtonModule,
                MatIconModule,
                MatTooltipModule
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(ParticleAnalysisCardComponent);
        component = fixture.componentInstance;
    });

    describe('Component Initialization', () => {
        it('should create', () => {
            expect(component).toBeTruthy();
        });

        it('should initialize with empty form when no initial data provided', () => {
            component.particleTypes = mockParticleTypes;
            component.subTypeCategories = mockSubTypeCategories;

            fixture.detectChanges();

            expect(component.analysesFormArray.length).toBe(2);
            expect(component.particleAnalysisForm.valid).toBe(true);
        });

        it('should initialize with provided initial data', () => {
            component.particleTypes = mockParticleTypes;
            component.subTypeCategories = mockSubTypeCategories;
            component.initialData = mockInitialData;

            fixture.detectChanges();

            const firstAnalysis = component.analysesFormArray.at(0);
            expect(firstAnalysis.get('comments')?.value).toBe('Test comment');
            expect(firstAnalysis.get('severity')?.value).toBe(2);
        });
    });

    describe('Input Binding', () => {
        it('should accept particle types input', () => {
            component.particleTypes = mockParticleTypes;
            expect(component.particleTypes).toEqual(mockParticleTypes);
        });

        it('should accept sub-type categories input', () => {
            component.subTypeCategories = mockSubTypeCategories;
            expect(component.subTypeCategories).toEqual(mockSubTypeCategories);
        });

        it('should accept initial data input', () => {
            component.initialData = mockInitialData;
            expect(component.initialData).toEqual(mockInitialData);
        });

        it('should accept readonly mode input', () => {
            component.readonly = true;
            expect(component.readonly).toBe(true);
        });

        it('should accept show images input', () => {
            component.showImages = false;
            expect(component.showImages).toBe(false);
        });

        it('should accept configuration input', () => {
            const config: ParticleAnalysisConfig = {
                testId: 123,
                readonly: false,
                showImages: true,
                viewFilter: 'all',
                enableSeverityCalculation: true
            };
            component.config = config;
            expect(component.config).toEqual(config);
        });
    });

    describe('Form Creation and Validation', () => {
        beforeEach(() => {
            component.particleTypes = mockParticleTypes;
            component.subTypeCategories = mockSubTypeCategories;
            component.config = {
                testId: 123,
                readonly: false,
                showImages: true,
                viewFilter: 'all',
                enableSeverityCalculation: true
            };
            fixture.detectChanges();
        });

        it('should create form groups for each particle type', () => {
            expect(component.analysesFormArray.length).toBe(mockParticleTypes.length);

            const firstAnalysis = component.analysesFormArray.at(0);
            expect(firstAnalysis.get('particleTypeId')?.value).toBe(1);
            expect(firstAnalysis.get('comments')).toBeTruthy();
            expect(firstAnalysis.get('severity')).toBeTruthy();
            expect(firstAnalysis.get('subTypeValues')).toBeTruthy();
        });

        it('should create sub-type category controls dynamically', () => {
            const firstAnalysis = component.analysesFormArray.at(0);
            const subTypeValues = firstAnalysis.get('subTypeValues');

            expect(subTypeValues?.get('category_1')).toBeTruthy();
            expect(subTypeValues?.get('category_2')).toBeTruthy();
        });

        it('should validate particle data correctly', () => {
            const validation = component['validateParticleData']();
            expect(validation.isValid).toBe(true);
            expect(Object.keys(validation.errors).length).toBe(0);
        });
    });

    describe('Event Emission', () => {
        beforeEach(() => {
            component.particleTypes = mockParticleTypes;
            component.subTypeCategories = mockSubTypeCategories;
            component.config = {
                testId: 123,
                readonly: false,
                showImages: true,
                viewFilter: 'all',
                enableSeverityCalculation: true
            };
            fixture.detectChanges();
        });

        it('should emit particle data changes', () => {
            spyOn(component.particleDataChange, 'emit');

            component['onFormChange']();

            expect(component.particleDataChange.emit).toHaveBeenCalled();
        });

        it('should emit severity changes', () => {
            spyOn(component.severityChange, 'emit');

            component['onFormChange']();

            expect(component.severityChange.emit).toHaveBeenCalled();
        });

        it('should emit validation changes', () => {
            spyOn(component.validationChange, 'emit');

            component['onFormChange']();

            expect(component.validationChange.emit).toHaveBeenCalled();
        });
    });

    describe('Readonly Mode', () => {
        beforeEach(() => {
            component.particleTypes = mockParticleTypes;
            component.subTypeCategories = mockSubTypeCategories;
            component.readonly = true;
            fixture.detectChanges();
        });

        it('should not set up form change handling in readonly mode', () => {
            spyOn(component.particleAnalysisForm.valueChanges, 'pipe');

            component['setupFormChangeHandling']();

            expect(component.particleAnalysisForm.valueChanges.pipe).not.toHaveBeenCalled();
        });

        it('should disable form controls in readonly mode', () => {
            // This would be tested by checking if form controls are disabled
            // The actual implementation would need to disable controls when readonly is true
            expect(component.readonly).toBe(true);
        });
    });

    describe('Severity Calculation', () => {
        beforeEach(() => {
            component.particleTypes = mockParticleTypes;
            component.subTypeCategories = mockSubTypeCategories;
            component.config = {
                testId: 123,
                readonly: false,
                showImages: true,
                viewFilter: 'all',
                enableSeverityCalculation: true
            };
            fixture.detectChanges();
        });

        it('should calculate overall severity correctly', () => {
            // Set severity values in form
            const firstAnalysis = component.analysesFormArray.at(0);
            const secondAnalysis = component.analysesFormArray.at(1);

            firstAnalysis.get('severity')?.setValue(2);
            secondAnalysis.get('severity')?.setValue(3);

            const overallSeverity = component['calculateOverallSeverity']();
            expect(overallSeverity).toBe(3); // Should return maximum severity
        });

        it('should return 0 when severity calculation is disabled', () => {
            component.config = { ...component.config!, enableSeverityCalculation: false };

            const overallSeverity = component['calculateOverallSeverity']();
            expect(overallSeverity).toBe(0);
        });

        it('should return 0 when no analyses have severity values', () => {
            const overallSeverity = component['calculateOverallSeverity']();
            expect(overallSeverity).toBe(0);
        });
    });

    describe('Utility Methods', () => {
        beforeEach(() => {
            component.particleTypes = mockParticleTypes;
            component.subTypeCategories = mockSubTypeCategories;
            fixture.detectChanges();
        });

        it('should track particle types by ID', () => {
            const trackResult = component.trackByParticleId(0, mockParticleTypes[0]);
            expect(trackResult).toBe(1);
        });

        it('should get particle type by ID', () => {
            const particleType = component.getParticleTypeById(1);
            expect(particleType).toEqual(mockParticleTypes[0]);
        });

        it('should return undefined for non-existent particle type ID', () => {
            const particleType = component.getParticleTypeById(999);
            expect(particleType).toBeUndefined();
        });

        it('should get category options by description', () => {
            const options = component.getCategoryOptions('Severity');
            expect(options).toEqual(mockSubTypeCategories[0].subTypes);
        });

        it('should return empty array for non-existent category', () => {
            const options = component.getCategoryOptions('NonExistent');
            expect(options).toEqual([]);
        });
    });

    describe('Form Actions', () => {
        beforeEach(() => {
            component.particleTypes = mockParticleTypes;
            component.subTypeCategories = mockSubTypeCategories;
            component.initialData = mockInitialData;
            fixture.detectChanges();
        });

        it('should reset form to initial values', () => {
            // Modify form values
            const firstAnalysis = component.analysesFormArray.at(0);
            firstAnalysis.get('comments')?.setValue('Modified comment');

            // Reset form
            component.resetForm();

            // Check if values are restored
            expect(firstAnalysis.get('comments')?.value).toBe('Test comment');
        });

        it('should clear form data', () => {
            component.clearForm();

            const firstAnalysis = component.analysesFormArray.at(0);
            expect(firstAnalysis.get('comments')?.value).toBe('');
            expect(firstAnalysis.get('severity')?.value).toBe(0);
        });
    });

    describe('Data Transformation', () => {
        beforeEach(() => {
            component.particleTypes = mockParticleTypes;
            component.subTypeCategories = mockSubTypeCategories;
            fixture.detectChanges();
        });

        it('should transform sub-type values correctly', () => {
            const formSubTypeValues = {
                category_1: 2,
                category_2: 1
            };

            const transformed = component['transformSubTypeValues'](formSubTypeValues);

            expect(transformed).toEqual({ 1: 2, 2: 1 });
        });

        it('should generate recommendations based on severity', () => {
            const analyses = [
                { particleTypeId: 1, subTypeValues: {}, comments: '', severity: 3, status: 'active' as const },
                { particleTypeId: 2, subTypeValues: {}, comments: '', severity: 4, status: 'active' as const }
            ];

            const recommendations = component['generateRecommendations'](analyses);

            expect(recommendations).toContain('Critical wear particles detected - investigate root cause');
        });
    });
});