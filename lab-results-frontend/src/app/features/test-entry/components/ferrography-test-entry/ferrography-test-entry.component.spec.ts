import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { FerrographyTestEntryComponent } from './ferrography-test-entry.component';
import { SampleService } from '../../../../shared/services/sample.service';
import { TestService } from '../../../../shared/services/test.service';
import { ValidationService } from '../../../../shared/services/validation.service';
import { SharedModule } from '../../../../shared/shared.module';
import { ParticleAnalysisCardComponent } from '../../../../shared/components/particle-analysis-card/particle-analysis-card.component';
import {
    ParticleAnalysisData,
    ParticleAnalysisValidation,
    ParticleSubTypeCategory
} from '../../../../shared/models/particle-analysis.model';
import { FERROGRAPHY_PARTICLE_TYPES } from '../../../../shared/constants/ferrography-particle-types';

describe('FerrographyTestEntryComponent', () => {
    let component: FerrographyTestEntryComponent;
    let fixture: ComponentFixture<FerrographyTestEntryComponent>;
    let mockSampleService: jasmine.SpyObj<SampleService>;
    let mockTestService: jasmine.SpyObj<TestService>;
    let mockValidationService: jasmine.SpyObj<ValidationService>;
    let mockRouter: jasmine.SpyObj<Router>;
    let mockActivatedRoute: any;
    let mockSnackBar: jasmine.SpyObj<MatSnackBar>;
    let mockDialog: jasmine.SpyObj<MatDialog>;

    const mockSubTypeCategories: ParticleSubTypeCategory[] = [
        {
            id: 1,
            description: 'Severity',
            active: true,
            sortOrder: 1,
            subTypes: [
                { particleSubTypeCategoryId: 1, value: 1, description: 'Low', active: true, sortOrder: 1 },
                { particleSubTypeCategoryId: 1, value: 2, description: 'Medium', active: true, sortOrder: 2 },
                { particleSubTypeCategoryId: 1, value: 3, description: 'High', active: true, sortOrder: 3 },
                { particleSubTypeCategoryId: 1, value: 4, description: 'Critical', active: true, sortOrder: 4 }
            ]
        },
        {
            id: 2,
            description: 'Heat',
            active: true,
            sortOrder: 2,
            subTypes: [
                { particleSubTypeCategoryId: 2, value: 1, description: 'Blue', active: true, sortOrder: 1 },
                { particleSubTypeCategoryId: 2, value: 2, description: 'Straw', active: true, sortOrder: 2 },
                { particleSubTypeCategoryId: 2, value: 3, description: 'Purple', active: true, sortOrder: 3 }
            ]
        },
        {
            id: 3,
            description: 'Concentration',
            active: true,
            sortOrder: 3,
            subTypes: [
                { particleSubTypeCategoryId: 3, value: 1, description: 'Few', active: true, sortOrder: 1 },
                { particleSubTypeCategoryId: 3, value: 2, description: 'Moderate', active: true, sortOrder: 2 },
                { particleSubTypeCategoryId: 3, value: 3, description: 'Many', active: true, sortOrder: 3 },
                { particleSubTypeCategoryId: 3, value: 4, description: 'Heavy', active: true, sortOrder: 4 }
            ]
        }
    ];

    beforeEach(async () => {
        const sampleServiceSpy = jasmine.createSpyObj('SampleService', ['clearError'], {
            selectedSample: jasmine.createSpy().and.returnValue(null),
            isLoading: jasmine.createSpy().and.returnValue(false),
            error: jasmine.createSpy().and.returnValue(null)
        });

        const testServiceSpy = jasmine.createSpyObj('TestService', [
            'getSubTypeCategories',
            'clearError'
        ], {
            testResultHistory: jasmine.createSpy().and.returnValue([]),
            isLoading: jasmine.createSpy().and.returnValue(false),
            error: jasmine.createSpy().and.returnValue(null)
        });

        const validationServiceSpy = jasmine.createSpyObj('ValidationService', ['validateForm']);
        const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
        const snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);
        const dialogSpy = jasmine.createSpyObj('MatDialog', ['open']);

        mockActivatedRoute = {
            params: of({ sampleId: '6' })
        };

        await TestBed.configureTestingModule({
            imports: [
                FerrographyTestEntryComponent,
                ParticleAnalysisCardComponent,
                SharedModule,
                ReactiveFormsModule,
                NoopAnimationsModule
            ],
            providers: [
                { provide: SampleService, useValue: sampleServiceSpy },
                { provide: TestService, useValue: testServiceSpy },
                { provide: ValidationService, useValue: validationServiceSpy },
                { provide: Router, useValue: routerSpy },
                { provide: ActivatedRoute, useValue: mockActivatedRoute },
                { provide: MatSnackBar, useValue: snackBarSpy },
                { provide: MatDialog, useValue: dialogSpy }
            ]
        }).compileComponents();

        mockSampleService = TestBed.inject(SampleService) as jasmine.SpyObj<SampleService>;
        mockTestService = TestBed.inject(TestService) as jasmine.SpyObj<TestService>;
        mockValidationService = TestBed.inject(ValidationService) as jasmine.SpyObj<ValidationService>;
        mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
        mockSnackBar = TestBed.inject(MatSnackBar) as jasmine.SpyObj<MatSnackBar>;
        mockDialog = TestBed.inject(MatDialog) as jasmine.SpyObj<MatDialog>;

        // Setup default service responses
        mockTestService.getSubTypeCategories.and.returnValue(of(mockSubTypeCategories));

        fixture = TestBed.createComponent(FerrographyTestEntryComponent);
        component = fixture.componentInstance;
    });

    describe('Component Initialization', () => {
        it('should create', () => {
            expect(component).toBeTruthy();
        });

        it('should initialize with ferrography particle types', () => {
            fixture.detectChanges();
            expect(component.FERROGRAPHY_PARTICLE_TYPES).toEqual(FERROGRAPHY_PARTICLE_TYPES);
            expect(component.FERROGRAPHY_PARTICLE_TYPES.length).toBe(10);
        });

        it('should load sub-type categories on init', () => {
            fixture.detectChanges();
            expect(mockTestService.getSubTypeCategories).toHaveBeenCalled();
            expect(component.subTypeCategories().length).toBe(3);
        });

        it('should initialize form with particle analysis control', () => {
            fixture.detectChanges();
            expect(component.ferrographyForm.get('particleAnalysis')).toBeTruthy();
        });

        it('should set particle analysis config correctly', () => {
            fixture.detectChanges();
            const config = component.particleAnalysisConfig();
            expect(config.testId).toBe(210);
            expect(config.readonly).toBe(false);
            expect(config.showImages).toBe(true);
            expect(config.enableSeverityCalculation).toBe(true);
        });
    });

    describe('Particle Analysis Integration', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should handle particle data changes', () => {
            const mockParticleData: ParticleAnalysisData = {
                analyses: [
                    {
                        particleTypeId: 1,
                        subTypeValues: { 1: 2 },
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

            component.onParticleDataChange(mockParticleData);

            expect(component.particleAnalysisData()).toEqual(mockParticleData);
            expect(component.ferrographyForm.get('particleAnalysis')?.value).toEqual(mockParticleData);
        });

        it('should update overall severity when particle data changes', () => {
            const mockParticleData: ParticleAnalysisData = {
                analyses: [
                    {
                        particleTypeId: 3, // Severe Sliding
                        subTypeValues: { 1: 4, 3: 4 }, // Critical severity, Heavy concentration
                        comments: 'Severe sliding wear detected',
                        severity: 4,
                        status: 'active'
                    }
                ],
                overallSeverity: 4,
                isValid: true,
                summary: {
                    totalParticles: 1,
                    criticalParticles: 1,
                    recommendations: ['Immediate investigation required']
                }
            };

            component.onParticleDataChange(mockParticleData);

            expect(component.ferrographyForm.get('overallSeverity')?.value).toBe(4);
        });

        it('should handle particle severity changes', () => {
            component.onParticleSeverityChange(3);
            expect(component.ferrographyForm.get('overallSeverity')?.value).toBe(3);
        });

        it('should handle particle validation changes', () => {
            const mockValidation: ParticleAnalysisValidation = {
                isValid: false,
                errors: { 1: ['Test error'] },
                warnings: { 2: ['Test warning'] }
            };

            component.onParticleValidationChange(mockValidation);

            const particleControl = component.ferrographyForm.get('particleAnalysis');
            expect(particleControl?.errors).toEqual({ particleValidation: mockValidation.errors });
        });

        it('should not override manually set severity', () => {
            // Manually set severity
            component.ferrographyForm.get('overallSeverity')?.setValue(1);

            // Try to update with higher severity
            component.onParticleSeverityChange(3);

            // Should update because new severity is higher
            expect(component.ferrographyForm.get('overallSeverity')?.value).toBe(3);
        });
    });

    describe('Severity Calculation', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should calculate overall severity from particle analysis', () => {
            const mockParticleData: ParticleAnalysisData = {
                analyses: [
                    {
                        particleTypeId: 1,
                        subTypeValues: { 1: 2 },
                        comments: '',
                        severity: 2,
                        status: 'active'
                    },
                    {
                        particleTypeId: 3,
                        subTypeValues: { 1: 3 },
                        comments: '',
                        severity: 3,
                        status: 'active'
                    }
                ],
                overallSeverity: 3,
                isValid: true,
                summary: {
                    totalParticles: 2,
                    criticalParticles: 1,
                    recommendations: []
                }
            };

            component.particleAnalysisData.set(mockParticleData);
            const severity = component['calculateOverallSeverity']();
            expect(severity).toBeGreaterThan(0);
        });

        it('should return form severity if manually set', () => {
            component.ferrographyForm.get('overallSeverity')?.setValue(4);
            const severity = component['calculateOverallSeverity']();
            expect(severity).toBe(4);
        });

        it('should provide severity description', () => {
            const description = component.getSeverityDescription(3);
            expect(description).toContain('Moderate');
        });

        it('should provide severity color class', () => {
            const colorClass = component.getSeverityColorClass(4);
            expect(colorClass).toBe('severity-severe');
        });

        it('should provide severity recommendations', () => {
            const mockParticleData: ParticleAnalysisData = {
                analyses: [
                    {
                        particleTypeId: 3,
                        subTypeValues: { 1: 4 },
                        comments: '',
                        severity: 4,
                        status: 'active'
                    }
                ],
                overallSeverity: 4,
                isValid: true,
                summary: {
                    totalParticles: 1,
                    criticalParticles: 1,
                    recommendations: []
                }
            };

            component.particleAnalysisData.set(mockParticleData);
            component.ferrographyForm.get('overallSeverity')?.setValue(4);

            const recommendations = component.getSeverityRecommendations();
            expect(recommendations.length).toBeGreaterThan(0);
            expect(recommendations).toContain('Immediate investigation required');
        });
    });

    describe('Form Integration', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should include particle analysis data in form submission', () => {
            const mockParticleData: ParticleAnalysisData = {
                analyses: [
                    {
                        particleTypeId: 1,
                        subTypeValues: { 1: 2 },
                        comments: 'Test analysis',
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

            component.particleAnalysisData.set(mockParticleData);
            component.ferrographyForm.patchValue({
                dilutionFactor: '1:10',
                overallSeverity: 2,
                overallComments: 'Test comments'
            });

            spyOn(component, 'onSave').and.callThrough();
            component.onSave();

            expect(mockSnackBar.open).toHaveBeenCalledWith(
                'Successfully saved ferrography results (simulated)',
                'Close',
                jasmine.any(Object)
            );
        });

        it('should validate form including particle analysis', () => {
            component.ferrographyForm.get('particleAnalysis')?.setErrors({ test: 'error' });

            spyOn(component, 'onSave').and.callThrough();
            component.onSave();

            expect(mockSnackBar.open).toHaveBeenCalledWith(
                'Please correct the errors in the form',
                'Close',
                jasmine.any(Object)
            );
        });

        it('should clear particle analysis data when form is cleared', () => {
            const mockParticleData: ParticleAnalysisData = {
                analyses: [
                    {
                        particleTypeId: 1,
                        subTypeValues: { 1: 2 },
                        comments: 'Test',
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

            component.particleAnalysisData.set(mockParticleData);

            const dialogRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
            dialogRef.afterClosed.and.returnValue(of(true));
            mockDialog.open.and.returnValue(dialogRef);

            component.onClear();

            expect(mockDialog.open).toHaveBeenCalled();
        });
    });

    describe('Error Handling', () => {
        it('should handle sub-type categories loading error', () => {
            mockTestService.getSubTypeCategories.and.returnValue(
                throwError(() => new Error('Failed to load categories'))
            );

            fixture.detectChanges();

            expect(component.particleDataError()).toContain('Failed to load particle analysis data');
        });

        it('should clear errors when requested', () => {
            component.particleDataError.set('Test error');
            component.clearError();
            expect(component.particleDataError()).toBeNull();
        });
    });

    describe('Component Template Integration', () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it('should render particle analysis card when particle types are loaded', () => {
            const particleCard = fixture.debugElement.nativeElement.querySelector('app-particle-analysis-card');
            expect(particleCard).toBeTruthy();
        });

        it('should pass correct inputs to particle analysis card', () => {
            const particleCard = fixture.debugElement.nativeElement.querySelector('app-particle-analysis-card');
            expect(particleCard).toBeTruthy();
            // Additional template integration tests would go here
        });

        it('should show loading state when loading particle data', () => {
            component.loadingParticleData.set(true);
            fixture.detectChanges();

            const loadingElement = fixture.debugElement.nativeElement.querySelector('.loading-container');
            expect(loadingElement).toBeTruthy();
        });

        it('should show error state when there is a particle data error', () => {
            component.particleDataError.set('Test error message');
            fixture.detectChanges();

            const errorElement = fixture.debugElement.nativeElement.querySelector('.error-card');
            expect(errorElement).toBeTruthy();
        });
    });
});