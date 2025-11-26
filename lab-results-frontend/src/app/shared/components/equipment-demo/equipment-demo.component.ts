import { Component, OnInit, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EquipmentSelectionEvent, EquipmentValidationResult } from '../../models/equipment.model';
import { LookupService } from '../../services/lookup.service';
import { LookupDisplayComponent } from '../lookup-display/lookup-display.component';
import { SharedModule } from '../../shared.module';

@Component({
    selector: 'app-equipment-demo',
    standalone: true,
    imports: [SharedModule, LookupDisplayComponent],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="equipment-demo-container">
            <mat-card>
                <mat-card-header>
                    <mat-card-title>Equipment Selection Demo</mat-card-title>
                    <mat-card-subtitle>Example of how to use equipment selection components in test forms</mat-card-subtitle>
                </mat-card-header>
                
                <mat-card-content>
                    <form [formGroup]="demoForm">
                        <!-- Viscosity Test Equipment with Lookup Integration -->
                        <div class="test-section">
                            <h3>Viscosity Test Equipment (Using Lookup Service)</h3>
                            <div class="equipment-row">
                                <app-lookup-display
                                    lookupType="equipment"
                                    equipmentType="THERMOMETER"
                                    [testId]="4"
                                    equipmentLabel="Thermometer MTE#"
                                    (equipmentSelected)="onLookupEquipmentSelected('thermometer', $event)">
                                </app-lookup-display>

                                <app-lookup-display
                                    lookupType="equipment"
                                    equipmentType="TIMER"
                                    [testId]="4"
                                    equipmentLabel="Stop Watch MTE#"
                                    (equipmentSelected)="onLookupEquipmentSelected('timer', $event)">
                                </app-lookup-display>

                                <app-lookup-display
                                    lookupType="equipment"
                                    equipmentType="VISCOMETER"
                                    [testId]="4"
                                    equipmentLabel="Tube ID"
                                    (equipmentSelected)="onLookupEquipmentSelected('tube', $event)">
                                </app-lookup-display>
                            </div>

                            <!-- Display calibration values -->
                            @if (selectedLookupEquipment()['tube']?.calibrationValue) {
                                <div class="calibration-info">
                                    <mat-icon>info</mat-icon>
                                    <span>Tube Calibration Value: {{ selectedLookupEquipment()['tube'].calibrationValue }}</span>
                                </div>
                            }
                        </div>

                        <!-- Traditional Equipment Selection (for comparison) -->
                        <div class="test-section">
                            <h3>Flash Point Test Equipment (Traditional Method)</h3>
                            <div class="equipment-row">
                                <app-equipment-selection
                                    equipmentType="BAROMETER"
                                    label="Barometer MTE#"
                                    placeholder="Select barometer"
                                    [required]="true"
                                    [testId]="6"
                                    fieldKey="flashpoint_barometer"
                                    (equipmentSelected)="onEquipmentSelected('barometer', $event)"
                                    (validationChanged)="onValidationChanged('barometer', $event)">
                                </app-equipment-selection>

                                <app-equipment-selection
                                    equipmentType="THERMOMETER"
                                    label="Thermometer MTE#"
                                    placeholder="Select thermometer"
                                    [required]="true"
                                    [testId]="6"
                                    fieldKey="flashpoint_thermometer"
                                    (equipmentSelected)="onEquipmentSelected('flashpoint_thermometer', $event)"
                                    (validationChanged)="onValidationChanged('flashpoint_thermometer', $event)">
                                </app-equipment-selection>
                            </div>
                        </div>



                        <!-- Validation Results Display -->
                        @if (hasValidationIssues()) {
                            <div class="validation-section">
                                <h4>Equipment Validation Status</h4>
                                @for (validation of validationResults() | keyvalue; track validation.key) {
                                    <app-equipment-validation
                                        [validation]="validation.value"
                                        [showDueDate]="true"
                                        [showDaysRemaining]="true"
                                        [compact]="false">
                                    </app-equipment-validation>
                                }
                            </div>
                        }

                        <!-- Test Input Fields -->
                        <div class="test-section">
                            <h3>Test Data Entry</h3>
                            <div class="input-row">
                                <mat-form-field appearance="outline">
                                    <mat-label>Stop Watch Time (seconds)</mat-label>
                                    <input matInput 
                                           type="number" 
                                           formControlName="stopWatchTime"
                                           (input)="calculateResult()"
                                           placeholder="Enter time">
                                    <mat-hint>Time in seconds</mat-hint>
                                </mat-form-field>

                                <mat-form-field appearance="outline">
                                    <mat-label>Calculated cSt</mat-label>
                                    <input matInput 
                                           type="number" 
                                           formControlName="calculatedResult"
                                           readonly
                                           placeholder="Calculated automatically">
                                    <mat-hint>Stop watch time × Tube calibration value</mat-hint>
                                </mat-form-field>
                            </div>
                        </div>

                        <!-- Action Buttons -->
                        <div class="action-buttons">
                            <button mat-raised-button 
                                    color="primary" 
                                    [disabled]="!demoForm.valid || !allEquipmentValid()"
                                    (click)="onSave()">
                                <mat-icon>save</mat-icon>
                                Save Test Results
                            </button>

                            <button mat-stroked-button 
                                    (click)="onClear()">
                                <mat-icon>clear</mat-icon>
                                Clear All
                            </button>

                            <button mat-stroked-button 
                                    (click)="refreshEquipment()">
                                <mat-icon>refresh</mat-icon>
                                Refresh Equipment
                            </button>
                        </div>
                    </form>
                </mat-card-content>
            </mat-card>

            <!-- Debug Information -->
            <mat-card class="debug-card">
                <mat-card-header>
                    <mat-card-title>Debug Information</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                    <div class="debug-section">
                        <h4>Selected Equipment (Traditional):</h4>
                        <pre>{{ selectedEquipment() | json }}</pre>
                    </div>
                    
                    <div class="debug-section">
                        <h4>Selected Equipment (Lookup Service):</h4>
                        <pre>{{ selectedLookupEquipment() | json }}</pre>
                    </div>
                    
                    <div class="debug-section">
                        <h4>Validation Results:</h4>
                        <pre>{{ validationResults() | json }}</pre>
                    </div>
                    
                    <div class="debug-section">
                        <h4>Form Status:</h4>
                        <p>Valid: {{ demoForm.valid }}</p>
                        <p>All Equipment Valid: {{ allEquipmentValid() }}</p>
                        <pre>{{ demoForm.value | json }}</pre>
                    </div>
                </mat-card-content>
            </mat-card>
        </div>
    `,
    styles: [`
        .equipment-demo-container {
            padding: 20px;
            max-width: 1200px;
            margin: 0 auto;
        }

        .test-section {
            margin: 24px 0;
            padding: 16px;
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            background-color: #fafafa;
        }

        .test-section h3 {
            margin: 0 0 16px 0;
            color: #333;
        }

        .equipment-row,
        .input-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 16px;
            margin-bottom: 16px;
        }

        .calibration-info {
            display: flex;
            align-items: center;
            gap: 8px;
            padding: 8px 12px;
            background-color: #e3f2fd;
            border-radius: 4px;
            color: #1976d2;
            font-size: 0.9em;
        }

        .validation-section {
            margin: 16px 0;
            padding: 16px;
            background-color: #fff;
            border-radius: 8px;
            border: 1px solid #e0e0e0;
        }

        .validation-section h4 {
            margin: 0 0 12px 0;
            color: #333;
        }

        .validation-container {
            display: flex;
            align-items: flex-start;
            gap: 12px;
            padding: 12px 16px;
            border-radius: 8px;
            margin: 8px 0;
            border-left: 4px solid;
        }

        .validation-container.warning {
            background-color: #fff3cd;
            border-left-color: #ffc107;
            color: #856404;
        }

        .validation-icon {
            flex-shrink: 0;
            margin-top: 2px;
            color: #ffc107;
        }

        .validation-content {
            flex: 1;
            min-width: 0;
        }

        .validation-message {
            font-weight: 500;
            margin-bottom: 4px;
        }

        .due-date,
        .days-remaining {
            font-size: 0.875rem;
            opacity: 0.8;
            margin-top: 2px;
        }

        .action-buttons {
            display: flex;
            gap: 12px;
            flex-wrap: wrap;
            margin-top: 24px;
        }

        .debug-card {
            margin-top: 20px;
            background-color: #f5f5f5;
        }

        .debug-section {
            margin-bottom: 16px;
        }

        .debug-section h4 {
            margin: 0 0 8px 0;
            color: #666;
        }

        .debug-section pre {
            background-color: #fff;
            padding: 8px;
            border-radius: 4px;
            border: 1px solid #ddd;
            font-size: 0.8em;
            overflow-x: auto;
        }

        @media (max-width: 768px) {
            .equipment-demo-container {
                padding: 10px;
            }
            
            .equipment-row,
            .input-row {
                grid-template-columns: 1fr;
            }
            
            .action-buttons {
                flex-direction: column;
            }
        }
    `]
})
export class EquipmentDemoComponent implements OnInit {
    private fb = inject(FormBuilder);
    private lookupService = inject(LookupService);

    // Form
    demoForm!: FormGroup;

    // Signals for component state
    private readonly _selectedEquipment = signal<{ [key: string]: EquipmentSelectionEvent }>({});
    private readonly _validationResults = signal<{ [key: string]: EquipmentValidationResult }>({});
    private readonly _selectedLookupEquipment = signal<{ [key: string]: { equipmentId: number; calibrationValue?: number } }>({});

    // Public readonly signals
    readonly selectedEquipment = this._selectedEquipment.asReadonly();
    readonly validationResults = this._validationResults.asReadonly();
    readonly selectedLookupEquipment = this._selectedLookupEquipment.asReadonly();

    // Computed signals
    readonly hasValidationIssues = () => {
        const validations = this._validationResults();
        return Object.values(validations).some(v => !v.isValid || v.isDueSoon || v.isOverdue);
    };

    readonly allEquipmentValid = () => {
        const validations = this._validationResults();
        const equipmentKeys = Object.keys(this._selectedEquipment());

        // Check if all selected equipment is valid
        return equipmentKeys.every(key => {
            const validation = validations[key];
            return validation && validation.isValid && !validation.isOverdue;
        });
    };

    ngOnInit(): void {
        this.initializeForm();
    }

    private initializeForm(): void {
        this.demoForm = this.fb.group({
            stopWatchTime: [null, [Validators.required, Validators.min(0.01)]],
            calculatedResult: [{ value: null, disabled: true }]
        });
    }

    onEquipmentSelected(equipmentKey: string, event: EquipmentSelectionEvent): void {
        console.log(`Equipment selected for ${equipmentKey}:`, event);

        this._selectedEquipment.update(selected => ({
            ...selected,
            [equipmentKey]: event
        }));

        // Recalculate if this affects calculations
        if (equipmentKey === 'tube') {
            this.calculateResult();
        }
    }

    onValidationChanged(equipmentKey: string, validation: EquipmentValidationResult): void {
        console.log(`Validation changed for ${equipmentKey}:`, validation);

        this._validationResults.update(validations => ({
            ...validations,
            [equipmentKey]: validation
        }));
    }

    calculateResult(): void {
        const stopWatchTime = this.demoForm.get('stopWatchTime')?.value;
        const tubeEquipment = this._selectedEquipment()['tube'];

        if (stopWatchTime && tubeEquipment?.calibrationValue) {
            const result = stopWatchTime * tubeEquipment.calibrationValue;
            this.demoForm.get('calculatedResult')?.setValue(result);
        } else {
            this.demoForm.get('calculatedResult')?.setValue(null);
        }
    }

    onSave(): void {
        if (!this.demoForm.valid) {
            console.log('Form is not valid');
            return;
        }

        if (!this.allEquipmentValid()) {
            console.log('Not all equipment is valid');
            return;
        }

        const formData = {
            ...this.demoForm.value,
            selectedEquipment: this._selectedEquipment(),
            validationResults: this._validationResults()
        };

        console.log('Saving test results:', formData);
        // Here you would call your test service to save the results
    }

    onClear(): void {
        this.demoForm.reset();
        this._selectedEquipment.set({});
        this._validationResults.set({});
    }

    onLookupEquipmentSelected(equipmentKey: string, event: { equipmentId: number; calibrationValue?: number }): void {
        console.log(`Lookup equipment selected for ${equipmentKey}:`, event);

        this._selectedLookupEquipment.update(selected => ({
            ...selected,
            [equipmentKey]: event
        }));

        // Recalculate if this affects calculations
        if (equipmentKey === 'tube') {
            this.calculateLookupResult();
        }
    }

    calculateLookupResult(): void {
        const stopWatchTime = this.demoForm.get('stopWatchTime')?.value;
        const tubeEquipment = this._selectedLookupEquipment()['tube'];

        if (stopWatchTime && tubeEquipment?.calibrationValue) {
            const result = stopWatchTime * tubeEquipment.calibrationValue;
            this.demoForm.get('calculatedResult')?.setValue(result);
        } else {
            this.demoForm.get('calculatedResult')?.setValue(null);
        }
    }

    refreshEquipment(): void {
        // Use the lookup service to refresh equipment cache
        console.log('Refreshing equipment data...');
        this.lookupService.refreshEquipmentCache().subscribe({
            next: (result) => {
                console.log('Equipment cache refreshed:', result.message);
            },
            error: (error) => {
                console.error('Error refreshing equipment cache:', error);
            }
        });
    }
}