import {
    Component,
    Input,
    Output,
    EventEmitter,
    OnInit,
    OnDestroy,
    inject,
    signal,
    computed,
    effect
} from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { EquipmentService } from '../../services/equipment.service';
import {
    EquipmentSelection,
    EquipmentSelectionEvent,
    EquipmentValidationResult
} from '../../models/equipment.model';

@Component({
    selector: 'app-equipment-selection',
    standalone: false,
    template: `
        <mat-form-field appearance="outline" class="equipment-selection">
            <mat-label>{{ label }}</mat-label>
            <mat-select 
                [formControl]="equipmentControl"
                [placeholder]="placeholder"
                (selectionChange)="onEquipmentSelected($event.value)">
                
                @if (isLoading()) {
                    <mat-option disabled>
                        <mat-spinner diameter="20"></mat-spinner>
                        Loading equipment...
                    </mat-option>
                } @else {
                    @if (equipmentOptions().length === 0) {
                        <mat-option disabled>No equipment available</mat-option>
                    } @else {
                        @for (equipment of equipmentOptions(); track equipment.id) {
                            <mat-option 
                                [value]="equipment.id"
                                [class]="getEquipmentOptionClass(equipment)">
                                {{ equipment.displayText }}
                                @if (equipment.isOverdue) {
                                    <mat-icon class="overdue-icon">warning</mat-icon>
                                } @else if (equipment.isDueSoon) {
                                    <mat-icon class="due-soon-icon">schedule</mat-icon>
                                }
                            </mat-option>
                        }
                    }
                }
            </mat-select>
            
            @if (equipmentControl.hasError('required')) {
                <mat-error>{{ label }} is required</mat-error>
            }
            
            @if (validationResult() && !validationResult()!.isValid) {
                <mat-error>{{ validationResult()!.message }}</mat-error>
            }
            
            <mat-hint>
                @if (selectedEquipment() && selectedEquipment()!.calibrationValue) {
                    Calibration: {{ selectedEquipment()!.calibrationValue }}
                } @else if (showCalibrationHint) {
                    Select equipment to view calibration value
                }
            </mat-hint>
        </mat-form-field>

        @if (selectedEquipment() && (selectedEquipment()!.isDueSoon || selectedEquipment()!.isOverdue)) {
            <div class="equipment-warning" 
                 [class.overdue]="selectedEquipment()!.isOverdue"
                 [class.due-soon]="selectedEquipment()!.isDueSoon">
                <mat-icon>
                    {{ selectedEquipment()!.isOverdue ? 'error' : 'warning' }}
                </mat-icon>
                <span>
                    @if (selectedEquipment()!.isOverdue) {
                        Equipment is overdue for calibration
                    } @else {
                        Equipment calibration due soon
                    }
                    @if (selectedEquipment()!.dueDate) {
                        ({{ selectedEquipment()!.dueDate | date:'MM/dd/yyyy' }})
                    }
                </span>
            </div>
        }
    `,
    styles: [`
        .equipment-selection {
            width: 100%;
            min-width: 200px;
        }

        .equipment-warning {
            display: flex;
            align-items: center;
            gap: 8px;
            padding: 8px 12px;
            border-radius: 4px;
            margin-top: 4px;
            font-size: 0.875rem;
        }

        .equipment-warning.due-soon {
            background-color: #fff3cd;
            color: #856404;
            border: 1px solid #ffeaa7;
        }

        .equipment-warning.overdue {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }

        .overdue-option {
            color: #d32f2f !important;
        }

        .due-soon-option {
            color: #f57c00 !important;
        }

        .overdue-icon,
        .due-soon-icon {
            font-size: 16px;
            width: 16px;
            height: 16px;
            margin-left: 8px;
        }

        .overdue-icon {
            color: #d32f2f;
        }

        .due-soon-icon {
            color: #f57c00;
        }

        mat-spinner {
            margin-right: 8px;
        }
    `]
})
export class EquipmentSelectionComponent implements OnInit, OnDestroy {
    private readonly equipmentService = inject(EquipmentService);
    private readonly destroy$ = new Subject<void>();

    // Inputs
    @Input() equipmentType!: string;
    @Input() testId?: number;
    @Input() lubeType?: string;
    @Input() label: string = 'Equipment';
    @Input() placeholder: string = 'Select equipment';
    @Input() required: boolean = false;
    @Input() disabled: boolean = false;
    @Input() showCalibrationHint: boolean = true;
    @Input() fieldKey?: string; // Unique key for this field to track selections

    // Outputs
    @Output() equipmentSelected = new EventEmitter<EquipmentSelectionEvent>();
    @Output() equipmentCleared = new EventEmitter<void>();
    @Output() validationChanged = new EventEmitter<EquipmentValidationResult>();

    // Form control
    equipmentControl = new FormControl<number | null>(null);

    // Signals
    private readonly _equipmentOptions = signal<EquipmentSelection[]>([]);
    private readonly _selectedEquipment = signal<EquipmentSelection | null>(null);
    private readonly _validationResult = signal<EquipmentValidationResult | null>(null);
    private readonly _isLoading = signal(false);

    // Public readonly signals
    readonly equipmentOptions = this._equipmentOptions.asReadonly();
    readonly selectedEquipment = this._selectedEquipment.asReadonly();
    readonly validationResult = this._validationResult.asReadonly();
    readonly isLoading = this._isLoading.asReadonly();

    // Computed signals
    readonly hasValidEquipment = computed(() => {
        const selected = this._selectedEquipment();
        const validation = this._validationResult();
        return selected !== null && (validation?.isValid ?? true);
    });

    constructor() {
        // Set up form validation
        effect(() => {
            if (this.required) {
                this.equipmentControl.setValidators([Validators.required]);
            } else {
                this.equipmentControl.clearValidators();
            }
            this.equipmentControl.updateValueAndValidity();
        });

        // Watch for equipment type changes
        effect(() => {
            if (this.equipmentType) {
                this.loadEquipment();
            }
        });

        // Manage disabled state through FormControl
        effect(() => {
            const shouldDisable = this.isLoading() || this.disabled;
            if (shouldDisable && this.equipmentControl.enabled) {
                this.equipmentControl.disable();
            } else if (!shouldDisable && this.equipmentControl.disabled) {
                this.equipmentControl.enable();
            }
        });
    }

    ngOnInit(): void {
        // Watch for form control changes
        this.equipmentControl.valueChanges
            .pipe(
                debounceTime(300),
                distinctUntilChanged(),
                takeUntil(this.destroy$)
            )
            .subscribe(value => {
                if (value === null) {
                    this.clearSelection();
                }
            });

        // Load initial equipment if type is provided
        if (this.equipmentType) {
            this.loadEquipment();
        }
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private loadEquipment(): void {
        if (!this.equipmentType) return;

        this._isLoading.set(true);

        this.equipmentService.getEquipmentByType(this.equipmentType, this.testId, this.lubeType)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (equipment) => {
                    this._equipmentOptions.set(equipment);
                    this._isLoading.set(false);
                },
                error: (error) => {
                    console.error('Failed to load equipment:', error);
                    this._equipmentOptions.set([]);
                    this._isLoading.set(false);
                }
            });
    }

    onEquipmentSelected(equipmentId: number): void {
        const equipment = this._equipmentOptions().find(e => e.id === equipmentId);
        if (!equipment) return;

        this._selectedEquipment.set(equipment);

        // Store selection in service if fieldKey is provided
        if (this.fieldKey) {
            this.equipmentService.selectEquipment(this.fieldKey, equipment);
        }

        // Validate the selected equipment
        this.validateSelectedEquipment(equipmentId);

        // Emit selection event
        this.equipmentSelected.emit({
            equipmentId: equipment.id,
            equipmentType: equipment.equipType,
            calibrationValue: equipment.calibrationValue,
            isValid: !equipment.isOverdue, // Overdue equipment is not valid
            validationMessage: equipment.isOverdue ? 'Equipment is overdue for calibration' : undefined
        });
    }

    private validateSelectedEquipment(equipmentId: number): void {
        this.equipmentService.validateEquipment(equipmentId)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this._validationResult.set(result);
                    this.validationChanged.emit(result);
                },
                error: (error) => {
                    console.error('Failed to validate equipment:', error);
                    const errorResult: EquipmentValidationResult = {
                        isValid: false,
                        isDueSoon: false,
                        isOverdue: false,
                        message: 'Validation failed',
                        daysUntilDue: 0
                    };
                    this._validationResult.set(errorResult);
                    this.validationChanged.emit(errorResult);
                }
            });
    }

    private clearSelection(): void {
        this._selectedEquipment.set(null);
        this._validationResult.set(null);

        if (this.fieldKey) {
            this.equipmentService.clearEquipmentSelection(this.fieldKey);
        }

        this.equipmentCleared.emit();
    }

    getEquipmentOptionClass(equipment: EquipmentSelection): string {
        if (equipment.isOverdue) {
            return 'overdue-option';
        } else if (equipment.isDueSoon) {
            return 'due-soon-option';
        }
        return '';
    }

    // Public methods for external control
    selectEquipmentById(equipmentId: number): void {
        this.equipmentControl.setValue(equipmentId);
    }

    clearEquipment(): void {
        this.equipmentControl.setValue(null);
    }

    refreshEquipment(): void {
        this.loadEquipment();
    }

    getSelectedEquipmentId(): number | null {
        return this.equipmentControl.value;
    }

    getCalibrationValue(): number | undefined {
        return this._selectedEquipment()?.calibrationValue;
    }
}