import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';

import { InspectFilterTestEntryComponent } from './inspect-filter-test-entry.component';
import { SampleService } from '../../../../shared/services/sample.service';
import { TestService } from '../../../../shared/services/test.service';
import { ValidationService } from '../../../../shared/services/validation.service';
import { ParticleAnalysisCardComponent } from '../../../../shared/components/particle-analysis-card/particle-analysis-card.component';
import { SharedModule } from '../../../../shared/shared.module';
import {
    ParticleAnalysisData,
    ParticleTypeDefinition,
    ParticleSubTypeCategory
} from '../../../../shared/models/particle-analysis.model';

describe('InspectFilterTestEntryComponent', () => {
    let component: InspectFilterTestEntryComponent;
    let fixture: ComponentFixture<InspectFilterTestEntryComponent>;
    let mockSampleService: jasmine.SpyObj<SampleService>;
    let mockTestService: jasmine.SpyObj<TestService>;
    let mockValidationService: jasmine.SpyObj<ValidationService>;
    let mockRouter: jasmine.SpyObj<Router>;
    let mockSnackBar: jasmine.SpyObj<MatSnackBar>;
    let mockDialog: jasmine.SpyObj<MatDialog>;
    let mockActivatedRoute: any;

    const mockSample = {
        id: 1,
        tagNumber: 'TEST-001',
        component: 'Engine',
        location: 'Main',
        lubeType: 'Oil',
        sampleDate: new Date(),
        qualityClass: 'A'
    };

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
                { particleSubTypeCategoryId: 1, value: 2, description: 'Moderate', active: true, sortOrder: 2 }
            ]
        }
    ];

    beforeEach(async () => {
        const sampleServiceSpy = jasmine.createSpyObj('SampleService', ['getSample', 'clearError'], {
            selectedSample: jasmine.createSpy().and.returnValue(mockSample),
            isLoading: jasmine.createSpy().and.returnValue(false),
            error: jasmine.createSpy().and.returnValue(null)
        });

        const testServiceSpy = jasmine.createSpyObj('TestService', [
            'getParticleTypes',
            'getSubTypeCategories',
            'getInspectFilterResults',
            'saveInspectFilterResults',
            'deleteInspectFilterResults',
            'getTestResultsHistory',
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
            params: of({ sampleId: '1' })
        };

        await TestBed.configureTestingModule({
            imports: [
                InspectFilterTestEntryComponent,
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
                { provide: MatSnackBar, useValue: snackBarSpy },
                { provide: MatDialog, useValue: dialogSpy },
                { provide: ActivatedRoute, useValue: mockActivatedRoute }
            ]
        }).compileComponents();

        mockSampleService = TestBed.inject(SampleService) as jasmine.SpyObj<SampleService>;
        mockTestService = TestBed.inject(TestService) as jasmine.SpyObj<TestService>;
        mockValidationService = TestBed.inject(ValidationService) as jasmine.SpyObj<ValidationService>;
        mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
        mockSnackBar = TestBed.inject(MatSnackBar) as jasmine.SpyObj<MatSnackBar>;
        mockDialog = TestBed.inject(MatDialog) as jasmine.SpyObj<MatDialog>;

        fixture = TestBed.createComponent(InspectFilterTestEntryComponent);
        component = fixture.componentInstance;
    });

    describe('Component Initialization', () => {
        it('should create', () => {
            expect(component).toBeTruthy();
        });

        it('should initialize form on component creation', () => {
            // Setup mock responses
            mockSampleService.getSample.and.returnValue(of(mockSample));
            mockTestService.getParticleTypes.and.returnValue(of(mockParticleTypes));
            mockTestService.getSubTypeCategories.and.returnValue(of(mockSubTypeCategories));
            mockTestService.getInspectFilterResults.and.returnValue(of(null));
            mockTestService.getTestResultsHistory.and.returnValue(of([]));

            fixture.detectChanges();

            expect(component.inspectFilterForm).toBeDefined();
            expect(component.inspectFilterForm.get('narrative')).toBeTruthy();
        });

        it('should load particle analysis data on init', () => {
            mockTestService.getParticleTypes.and.returnValue(of(mockParticleTypes));
            mockTestService.getSubTypeCategories.and.returnValue(of(mockSubTypeCategories));
            mockSampleService.getSample.and.returnValue(of(mockSample));
            mockTestService.getInspectFilterResults.and.returnValue(of(null));
            mockTestService.getTestResultsHistory.and.returnValue(of([]));

            fixture.detectChanges();

            expect(mockTestService.getParticleTypes).toHaveBeenCalled();
            expect(mockTestService.getSubTypeCategories).toHaveBeenCalled();
            expect(component.particleTypes()).toEqual(mockParticleTypes);
            expect(component.subTypeCategories()).toEqual(mockSubTypeCategories);
        });
    });

    describe('Particle Analysis Card Integration', () => {
        beforeEach(() => {
            mockSampleService.getSample.and.returnValue(of(mockSample));
            mockTestService.getParticleTypes.and.returnValue(of(mockParticleTypes));
            mockTestService.getSubTypeCategories.and.returnValue(of(mockSubTypeCategories));
            mockTestService.getInspectFilterResults.and.returnValue(of(null));
            mockTestService.getTestResultsHistory.and.returnValue(of([]));
            fixture.detectChanges();
        });

        it('should create particle analysis configuration', () => {
            const config = component.particleAnalysisConfig();

            expect(config.testId).toBe(120);
            expect(config.readonly).toBe(false);
            expect(config.showImages).toBe(true);
            expect(config.viewFilter).toBe('all');
            expect(config.enableSeverityCalculation).toBe(true);
        });

        it('should handle particle data changes', () => {
            const mockParticleData: ParticleAnalysisData = {
                analyses: [{
                    particleTypeId: 1,
                    subTypeValues: { 1: 2 },
                    comments: 'Test comment',
                    severity: 2,
                    status: 'active'
                }],
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
            expect(component.overallSeverity()).toBe(2);
        });

        it('should handle particle severity changes', () => {
            spyOn(component, 'onParticleSeverityChange');

            component.onParticleSeverityChange(3);

            expect(component.onParticleSeverityChange).toHaveBeenCalledWith(3);
        });

        it('should handle particle validation changes', () => {
            const mockValidation = {
                isValid: false,
                errors: { 1: ['Test error'] },
                warnings: {}
            };

            spyOn(console, 'warn');
            component.onParticleValidationChange(mockValidation);

            expect(console.warn).toHaveBeenCalledWith('Particle analysis validation errors:', mockValidation.errors);
        });
    });

    describe('Form Submission', () => {
        beforeEach(() => {
            mockSampleService.getSample.and.returnValue(of(mockSample));
            mockTestService.getParticleTypes.and.returnValue(of(mockParticleTypes));
            mockTestService.getSubTypeCategories.and.returnValue(of(mockSubTypeCategories));
            mockTestService.getInspectFilterResults.and.returnValue(of(null));
            mockTestService.getTestResultsHistory.and.returnValue(of([]));
            fixture.detectChanges();
        });

        it('should save form data with particle analysis', () => {
            const mockParticleData: ParticleAnalysisData = {
                analyses: [{
                    particleTypeId: 1,
                    subTypeValues: { 1: 2 },
                    comments: 'Test comment',
                    severity: 2,
                    status: 'active'
                }],
                overallSeverity: 2,
                isValid: true,
                summary: {
                    totalParticles: 1,
                    criticalParticles: 0,
                    recommendations: []
                }
            };

            const mockResponse = { recordsSaved: 1 };
            mockTestService.saveInspectFilterResults.and.returnValue(of(mockResponse));

            component.onParticleDataChange(mockParticleData);
            component.inspectFilterForm.patchValue({ narrative: 'Test narrative' });

            component.onSave();

            expect(mockTestService.saveInspectFilterResults).toHaveBeenCalledWith(jasmine.objectContaining({
                sampleId: 1,
                testId: 120,
                narrative: 'Test narrative',
                particleAnalyses: jasmine.arrayContaining([
                    jasmine.objectContaining({
                        particleTypeDefinitionId: 1,
                        comments: 'Test comment',
                        status: 'E'
                    })
                ])
            }));
        });

        it('should show error message when save fails', () => {
            const mockError = { message: 'Save failed' };
            mockTestService.saveInspectFilterResults.and.returnValue(throwError(() => mockError));

            component.onSave();

            expect(mockSnackBar.open).toHaveBeenCalledWith(
                'Failed to save results: Save failed',
                'Close',
                jasmine.objectContaining({ panelClass: ['error-snackbar'] })
            );
        });
    });

    describe('Data Loading and Population', () => {
        it('should populate form with existing results', () => {
            const mockExistingResult = {
                sampleId: 1,
                testId: 120,
                particleAnalyses: [{
                    particleTypeDefinitionId: 1,
                    subTypeValues: { 1: 2 },
                    comments: 'Existing comment',
                    status: 'E'
                }],
                narrative: 'Existing narrative',
                major: null,
                minor: null,
                trace: null,
                overallSeverity: 2,
                mediaReady: true
            };

            mockSampleService.getSample.and.returnValue(of(mockSample));
            mockTestService.getParticleTypes.and.returnValue(of(mockParticleTypes));
            mockTestService.getSubTypeCategories.and.returnValue(of(mockSubTypeCategories));
            mockTestService.getInspectFilterResults.and.returnValue(of(mockExistingResult));
            mockTestService.getTestResultsHistory.and.returnValue(of([]));

            fixture.detectChanges();

            expect(component.inspectFilterForm.get('narrative')?.value).toBe('Existing narrative');
            expect(component.particleAnalysisData()?.analyses[0].comments).toBe('Existing comment');
        });

        it('should handle data loading errors gracefully', () => {
            const mockError = { message: 'Load failed' };
            mockSampleService.getSample.and.returnValue(of(mockSample));
            mockTestService.getParticleTypes.and.returnValue(throwError(() => mockError));
            mockTestService.getSubTypeCategories.and.returnValue(of(mockSubTypeCategories));

            fixture.detectChanges();

            expect(component.particleDataError()).toBe('Failed to load particle analysis data: Load failed');
        });
    });

    describe('Form Actions', () => {
        beforeEach(() => {
            mockSampleService.getSample.and.returnValue(of(mockSample));
            mockTestService.getParticleTypes.and.returnValue(of(mockParticleTypes));
            mockTestService.getSubTypeCategories.and.returnValue(of(mockSubTypeCategories));
            mockTestService.getInspectFilterResults.and.returnValue(of(null));
            mockTestService.getTestResultsHistory.and.returnValue(of([]));
            fixture.detectChanges();
        });

        it('should clear form data', () => {
            const mockDialogRef = {
                afterClosed: () => of(true)
            };
            mockDialog.open.and.returnValue(mockDialogRef as any);

            component.inspectFilterForm.patchValue({ narrative: 'Test narrative' });
            component.onClear();

            expect(component.inspectFilterForm.get('narrative')?.value).toBe('');
            expect(component.particleAnalysisData()).toBeNull();
        });

        it('should navigate back on cancel', () => {
            component.onCancel();

            expect(mockRouter.navigate).toHaveBeenCalledWith(['/samples']);
        });
    });

    describe('Utility Methods', () => {
        beforeEach(() => {
            mockSampleService.getSample.and.returnValue(of(mockSample));
            mockTestService.getParticleTypes.and.returnValue(of(mockParticleTypes));
            mockTestService.getSubTypeCategories.and.returnValue(of(mockSubTypeCategories));
            mockTestService.getInspectFilterResults.and.returnValue(of(null));
            mockTestService.getTestResultsHistory.and.returnValue(of([]));
            fixture.detectChanges();
        });

        it('should calculate media ready status', () => {
            const mockParticleData: ParticleAnalysisData = {
                analyses: [],
                overallSeverity: 2,
                isValid: true,
                summary: { totalParticles: 0, criticalParticles: 0, recommendations: [] }
            };

            component.onParticleDataChange(mockParticleData);
            component.inspectFilterForm.patchValue({ narrative: 'Test narrative' });

            expect(component.mediaReady()).toBe(true);
        });

        it('should check for existing results', () => {
            const mockParticleData: ParticleAnalysisData = {
                analyses: [{
                    particleTypeId: 1,
                    subTypeValues: { 1: 2 },
                    comments: 'Test comment',
                    severity: 2,
                    status: 'active'
                }],
                overallSeverity: 2,
                isValid: true,
                summary: { totalParticles: 1, criticalParticles: 0, recommendations: [] }
            };

            component.onParticleDataChange(mockParticleData);

            expect(component.hasExistingResults()).toBe(true);
        });

        it('should get status text correctly', () => {
            expect(component.getStatusText('C')).toBe('Complete');
            expect(component.getStatusText('E')).toBe('In Progress');
            expect(component.getStatusText('X')).toBe('Pending');
            expect(component.getStatusText('Unknown')).toBe('Unknown');
        });
    });
});