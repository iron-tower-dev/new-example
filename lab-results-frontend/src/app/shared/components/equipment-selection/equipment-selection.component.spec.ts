import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { EquipmentSelectionComponent } from './equipment-selection.component';
import { EquipmentService } from '../../services/equipment.service';
import { EquipmentSelection, EquipmentValidationResult } from '../../models/equipment.model';

describe('EquipmentSelectionComponent', () => {
    let component: EquipmentSelectionComponent;
    let fixture: ComponentFixture<EquipmentSelectionComponent>;
    let mockEquipmentService: jasmine.SpyObj<EquipmentService>;

    const mockEquipment: EquipmentSelection[] = [
        {
            id: 1,
            equipName: 'Digital Thermometer #1',
            equipType: 'Thermometer',
            calibrationValue: 1.0,
            dueDate: new Date('2024-12-31'),
            isDueSoon: false,
            isOverdue: false,
            displayText: 'Digital Thermometer #1 (Cal: 1.0)'
        },
        {
            id: 2,
            equipName: 'Digital Thermometer #2',
            equipType: 'Thermometer',
            calibrationValue: 1.02,
            dueDate: new Date('2024-01-15'),
            isDueSoon: true,
            isOverdue: false,
            displayText: 'Digital Thermometer #2 (Cal: 1.02) - Due Soon'
        },
        {
            id: 3,
            equipName: 'Digital Thermometer #3',
            equipType: 'Thermometer',
            calibrationValue: 0.98,
            dueDate: new Date('2023-12-01'),
            isDueSoon: false,
            isOverdue: true,
            displayText: 'Digital Thermometer #3 (Cal: 0.98) - OVERDUE'
        }
    ];

    beforeEach(async () => {
        const equipmentServiceSpy = jasmine.createSpyObj('EquipmentService', [
            'getEquipmentByType',
            'validateEquipment',
            'selectEquipment',
            'clearEquipmentSelection'
        ]);

        await TestBed.configureTestingModule({
            declarations: [EquipmentSelectionComponent],
            imports: [
                ReactiveFormsModule,
                MatSelectModule,
                MatFormFieldModule,
                MatIconModule,
                MatProgressSpinnerModule,
                NoopAnimationsModule
            ],
            providers: [
                { provide: EquipmentService, useValue: equipmentServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(EquipmentSelectionComponent);
        component = fixture.componentInstance;
        mockEquipmentService = TestBed.inject(EquipmentService) as jasmine.SpyObj<EquipmentService>;
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    describe('Component Initialization', () => {
        it('should initialize with default values', () => {
            expect(component.label).toBe('Equipment');
            expect(component.placeholder).toBe('Select equipment');
            expect(component.required).toBe(false);
            expect(component.disabled).toBe(false);
            expect(component.showCalibrationHint).toBe(true);
            expect(component.equipmentOptions()).toEqual([]);
            expect(component.selectedEquipment()).toBeNull();
            expect(component.isLoading()).toBe(false);
        });

        it('should set up form control with required validator when required is true', () => {
            component.required = true;
            fixture.detectChanges(); // Trigger change detection to run effects

            expect(component.equipmentControl.hasError('required')).toBe(true);
        });
    });

    describe('Equipment Loading', () => {
        beforeEach(() => {
            component.equipmentType = 'Thermometer';
        });

        it('should load equipment when equipmentType is provided', () => {
            mockEquipmentService.getEquipmentByType.and.returnValue(of(mockEquipment));

            component.ngOnInit();

            expect(mockEquipmentService.getEquipmentByType).toHaveBeenCalledWith('Thermometer', undefined, undefined);
            expect(component.equipmentOptions()).toEqual(mockEquipment);
            expect(component.isLoading()).toBe(false);
        });

        it('should handle loading errors gracefully', () => {
            mockEquipmentService.getEquipmentByType.and.returnValue(throwError(() => new Error('Network error')));
            spyOn(console, 'error');

            component.ngOnInit();

            expect(component.equipmentOptions()).toEqual([]);
            expect(component.isLoading()).toBe(false);
            expect(console.error).toHaveBeenCalledWith('Failed to load equipment:', jasmine.any(Error));
        });

        it('should pass testId and lubeType to service when provided', () => {
            component.testId = 1;
            component.lubeType = 'Oil';
            mockEquipmentService.getEquipmentByType.and.returnValue(of(mockEquipment));

            component.ngOnInit();

            expect(mockEquipmentService.getEquipmentByType).toHaveBeenCalledWith('Thermometer', 1, 'Oil');
        });
    });

    describe('Equipment Selection', () => {
        beforeEach(() => {
            component.equipmentType = 'Thermometer';
            mockEquipmentService.getEquipmentByType.and.returnValue(of(mockEquipment));
            component.ngOnInit();
        });

        it('should select equipment and emit selection event', () => {
            const validationResult: EquipmentValidationResult = {
                isValid: true,
                isDueSoon: false,
                isOverdue: false,
                message: 'Equipment is valid',
                daysUntilDue: 365
            };

            mockEquipmentService.validateEquipment.and.returnValue(of(validationResult));
            spyOn(component.equipmentSelected, 'emit');

            component.onEquipmentSelected(1);

            expect(component.selectedEquipment()).toEqual(mockEquipment[0]);
            expect(component.equipmentSelected.emit).toHaveBeenCalledWith({
                equipmentId: 1,
                equipmentType: 'Thermometer',
                calibrationValue: 1.0,
                isValid: true,
                validationMessage: undefined
            });
        });

        it('should handle overdue equipment selection', () => {
            const validationResult: EquipmentValidationResult = {
                isValid: false,
                isDueSoon: false,
                isOverdue: true,
                message: 'Equipment is overdue for calibration',
                daysUntilDue: -30
            };

            mockEquipmentService.validateEquipment.and.returnValue(of(validationResult));
            spyOn(component.equipmentSelected, 'emit');

            component.onEquipmentSelected(3);

            expect(component.selectedEquipment()).toEqual(mockEquipment[2]);
            expect(component.equipmentSelected.emit).toHaveBeenCalledWith({
                equipmentId: 3,
                equipmentType: 'Thermometer',
                calibrationValue: 0.98,
                isValid: false,
                validationMessage: 'Equipment is overdue for calibration'
            });
        });

        it('should store selection in service when fieldKey is provided', () => {
            component.fieldKey = 'thermometer1';
            mockEquipmentService.validateEquipment.and.returnValue(of({
                isValid: true,
                isDueSoon: false,
                isOverdue: false,
                message: 'Valid',
                daysUntilDue: 365
            }));

            component.onEquipmentSelected(1);

            expect(mockEquipmentService.selectEquipment).toHaveBeenCalledWith('thermometer1', mockEquipment[0]);
        });

        it('should handle validation errors', () => {
            mockEquipmentService.validateEquipment.and.returnValue(throwError(() => new Error('Validation failed')));
            spyOn(console, 'error');
            spyOn(component.validationChanged, 'emit');

            component.onEquipmentSelected(1);

            expect(console.error).toHaveBeenCalledWith('Failed to validate equipment:', jasmine.any(Error));
            expect(component.validationChanged.emit).toHaveBeenCalledWith({
                isValid: false,
                isDueSoon: false,
                isOverdue: false,
                message: 'Validation failed',
                daysUntilDue: 0
            });
        });
    });

    describe('Equipment Clearing', () => {
        beforeEach(() => {
            component.equipmentType = 'Thermometer';
            component.fieldKey = 'thermometer1';
            mockEquipmentService.getEquipmentByType.and.returnValue(of(mockEquipment));
            component.ngOnInit();
        });

        it('should clear selection when form control value is set to null', async () => {
            spyOn(component.equipmentCleared, 'emit');

            component.equipmentControl.setValue(1);
            component.equipmentControl.setValue(null);

            // Wait for debounced value changes
            await new Promise(resolve => setTimeout(resolve, 350));

            expect(component.selectedEquipment()).toBeNull();
            expect(component.validationResult()).toBeNull();
            expect(mockEquipmentService.clearEquipmentSelection).toHaveBeenCalledWith('thermometer1');
            expect(component.equipmentCleared.emit).toHaveBeenCalled();
        });
    });

    describe('CSS Class Methods', () => {
        it('should return correct CSS class for overdue equipment', () => {
            const overdueEquipment = mockEquipment[2];
            expect(component.getEquipmentOptionClass(overdueEquipment)).toBe('overdue-option');
        });

        it('should return correct CSS class for due soon equipment', () => {
            const dueSoonEquipment = mockEquipment[1];
            expect(component.getEquipmentOptionClass(dueSoonEquipment)).toBe('due-soon-option');
        });

        it('should return empty string for valid equipment', () => {
            const validEquipment = mockEquipment[0];
            expect(component.getEquipmentOptionClass(validEquipment)).toBe('');
        });
    });

    describe('Public Methods', () => {
        beforeEach(() => {
            component.equipmentType = 'Thermometer';
            mockEquipmentService.getEquipmentByType.and.returnValue(of(mockEquipment));
            component.ngOnInit();
        });

        it('should select equipment by ID', () => {
            component.selectEquipmentById(1);
            expect(component.equipmentControl.value).toBe(1);
        });

        it('should clear equipment', () => {
            component.equipmentControl.setValue(1);
            component.clearEquipment();
            expect(component.equipmentControl.value).toBeNull();
        });

        it('should refresh equipment list', () => {
            mockEquipmentService.getEquipmentByType.calls.reset();
            mockEquipmentService.getEquipmentByType.and.returnValue(of(mockEquipment));

            component.refreshEquipment();

            expect(mockEquipmentService.getEquipmentByType).toHaveBeenCalled();
        });

        it('should get selected equipment ID', () => {
            component.equipmentControl.setValue(1);
            expect(component.getSelectedEquipmentId()).toBe(1);
        });

        it('should get calibration value', () => {
            mockEquipmentService.validateEquipment.and.returnValue(of({
                isValid: true,
                isDueSoon: false,
                isOverdue: false,
                message: 'Valid',
                daysUntilDue: 365
            }));

            component.onEquipmentSelected(1);
            expect(component.getCalibrationValue()).toBe(1.0);
        });
    });

    describe('Computed Signals', () => {
        beforeEach(() => {
            component.equipmentType = 'Thermometer';
            mockEquipmentService.getEquipmentByType.and.returnValue(of(mockEquipment));
            component.ngOnInit();
        });

        it('should compute hasValidEquipment correctly', () => {
            expect(component.hasValidEquipment()).toBe(false);

            mockEquipmentService.validateEquipment.and.returnValue(of({
                isValid: true,
                isDueSoon: false,
                isOverdue: false,
                message: 'Valid',
                daysUntilDue: 365
            }));

            component.onEquipmentSelected(1);
            expect(component.hasValidEquipment()).toBe(true);
        });

        it('should compute hasValidEquipment as false for invalid equipment', () => {
            mockEquipmentService.validateEquipment.and.returnValue(of({
                isValid: false,
                isDueSoon: false,
                isOverdue: true,
                message: 'Overdue',
                daysUntilDue: -30
            }));

            component.onEquipmentSelected(3);
            expect(component.hasValidEquipment()).toBe(false);
        });
    });
});