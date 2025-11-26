import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';

import { ValidationService, ValidationError } from '../../services/validation.service';
import { NotificationService } from '../../services/notification.service';
import { ErrorDisplayComponent } from '../error-display/error-display.component';

@Component({
    selector: 'app-validation-demo',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatSelectModule,
        ErrorDisplayComponent
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    template: `
        <div class="validation-demo-container">
            <mat-card>
                <mat-card-header>
                    <mat-card-title>Enhanced Validation System Demo</mat-card-title>
                    <mat-card-subtitle>Demonstrates comprehensive validation and error handling</mat-card-subtitle>
                </mat-card-header>
                
                <mat-card-content>
                    <!-- Error Display Component -->
                    <app-error-display
                        [validationErrors]="validationErrors"
                        [validationWarnings]="validationWarnings"
                        [errorMessage]="generalError"
                        [showSummary]="showSummary"
                        [config]="errorDisplayConfig"
                        (onDismiss)="onDismissError($event)"
                        (onRetry)="onRetry()">
                    </app-error-display>

                    <!-- Demo Form -->
                    <form [formGroup]="demoForm" (ngSubmit)="onSubmit()">
                        <div class="form-row">
                            <mat-form-field appearance="outline">
                                <mat-label>Test Type</mat-label>
                                <mat-select formControlName="testType" (selectionChange)="onTestTypeChange()">
                                    <mat-option value="1">TAN by Color Indication</mat-option>
                                    <mat-option value="2">Viscosity @ 40°C</mat-option>
                                    <mat-option value="4">Flash Point</mat-option>
                                    <mat-option value="8">Particle Count</mat-option>
                                </mat-select>
                                @if (demoForm.get('testType')?.errors?.['required']) {
                                    <mat-error>Test type is required</mat-error>
                                }
                            </mat-form-field>
                        </div>

                        <div class="form-row">
                            <mat-form-field appearance="outline">
                                <mat-label>Sample Weight (g)</mat-label>
                                <input 
                                    matInput 
                                    type="number" 
                                    step="0.01"
                                    formControlName="sampleWeight"
                                    (blur)="validateField('sampleWeight')"
                                    placeholder="Enter sample weight">
                                <mat-hint>Weight of the sample in grams</mat-hint>
                                @if (demoForm.get('sampleWeight')?.errors?.['required']) {
                                    <mat-error>Sample weight is required</mat-error>
                                }
                                @if (demoForm.get('sampleWeight')?.errors?.['sampleWeightLow']) {
                                    <mat-error>{{ validationService.getErrorMessage(demoForm.get('sampleWeight')?.errors!) }}</mat-error>
                                }
                            </mat-form-field>

                            <mat-form-field appearance="outline">
                                <mat-label>Test Value</mat-label>
                                <input 
                                    matInput 
                                    type="number" 
                                    step="0.01"
                                    formControlName="testValue"
                                    (blur)="validateField('testValue')"
                                    placeholder="Enter test value">
                                <mat-hint>Test-specific value based on selected test type</mat-hint>
                                @if (demoForm.get('testValue')?.errors?.['required']) {
                                    <mat-error>Test value is required</mat-error>
                                }
                                @if (demoForm.get('testValue')?.errors && !demoForm.get('testValue')?.errors?.['required']) {
                                    <mat-error>{{ validationService.getErrorMessage(demoForm.get('testValue')?.errors!) }}</mat-error>
                                }
                            </mat-form-field>
                        </div>

                        <div class="form-row">
                            <mat-form-field appearance="outline">
                                <mat-label>Equipment 1</mat-label>
                                <mat-select formControlName="equipment1" (selectionChange)="validateEquipment()">
                                    <mat-option value="1">Thermometer A</mat-option>
                                    <mat-option value="2">Thermometer B</mat-option>
                                    <mat-option value="3">Barometer A</mat-option>
                                </mat-select>
                            </mat-form-field>

                            <mat-form-field appearance="outline">
                                <mat-label>Equipment 2</mat-label>
                                <mat-select formControlName="equipment2" (selectionChange)="validateEquipment()">
                                    <mat-option value="1">Thermometer A</mat-option>
                                    <mat-option value="2">Thermometer B</mat-option>
                                    <mat-option value="3">Barometer A</mat-option>
                                </mat-select>
                            </mat-form-field>
                        </div>

                        <div class="form-actions">
                            <button 
                                mat-raised-button 
                                color="primary" 
                                type="submit"
                                [disabled]="!demoForm.valid">
                                Validate Form
                            </button>
                            
                            <button 
                                mat-raised-button 
                                color="accent" 
                                type="button"
                                (click)="clearForm()">
                                Clear Form
                            </button>
                            
                            <button 
                                mat-raised-button 
                                color="warn" 
                                type="button"
                                (click)="simulateServerError()">
                                Simulate Server Error
                            </button>
                        </div>
                    </form>

                    <!-- Validation Status -->
                    <div class="validation-status">
                        <h4>Validation Status:</h4>
                        <p><strong>Form Valid:</strong> {{ demoForm.valid ? 'Yes' : 'No' }}</p>
                        <p><strong>Errors:</strong> {{ validationErrors().length }}</p>
                        <p><strong>Warnings:</strong> {{ validationWarnings().length }}</p>
                        <p><strong>Has Validation Issues:</strong> {{ validationService.hasAnyValidationIssues() ? 'Yes' : 'No' }}</p>
                    </div>
                </mat-card-content>
            </mat-card>
        </div>
    `,
    styles: [`
        .validation-demo-container {
            padding: 20px;
            max-width: 800px;
            margin: 0 auto;
        }

        .form-row {
            display: flex;
            gap: 16px;
            margin-bottom: 16px;
        }

        .form-row mat-form-field {
            flex: 1;
        }

        .form-actions {
            display: flex;
            gap: 12px;
            margin-top: 24px;
            flex-wrap: wrap;
        }

        .validation-status {
            margin-top: 24px;
            padding: 16px;
            background-color: #f5f5f5;
            border-radius: 4px;
        }

        .validation-status h4 {
            margin-top: 0;
            color: #333;
        }

        .validation-status p {
            margin: 8px 0;
            font-size: 0.9em;
        }

        @media (max-width: 768px) {
            .form-row {
                flex-direction: column;
            }
            
            .form-actions {
                flex-direction: column;
            }
        }
    `]
})
export class ValidationDemoComponent {
    private fb = inject(FormBuilder);
    readonly validationService = inject(ValidationService);
    private notificationService = inject(NotificationService);

    // Form
    demoForm: FormGroup;

    // Validation state
    validationErrors = signal<ValidationError[]>([]);
    validationWarnings = signal<ValidationError[]>([]);
    generalError = signal<string | null>(null);
    showSummary = signal<boolean>(true);

    // Error display configuration
    errorDisplayConfig = {
        showDetails: true,
        showDismiss: true,
        showRetry: true,
        collapsible: false,
        maxErrors: 5
    };

    constructor() {
        this.demoForm = this.fb.group({
            testType: ['', Validators.required],
            sampleWeight: ['', [Validators.required, ValidationService.sampleWeightValidator()]],
            testValue: ['', Validators.required],
            equipment1: [''],
            equipment2: ['']
        });

        // Initialize validation rules
        this.validationService.initializeDefaultTestRules();
    }

    onTestTypeChange(): void {
        const testType = this.demoForm.get('testType')?.value;
        const testValueControl = this.demoForm.get('testValue');

        // Clear existing validators
        testValueControl?.clearValidators();

        // Add test-specific validators
        const validators = [Validators.required];

        switch (testType) {
            case '1': // TAN
                validators.push(ValidationService.tanValidator());
                break;
            case '2': // Viscosity
                validators.push(ValidationService.viscosityValidator('40C'));
                break;
            case '4': // Flash Point
                validators.push(ValidationService.flashPointValidator());
                break;
            case '8': // Particle Count
                validators.push(ValidationService.particleCountValidator());
                break;
        }

        testValueControl?.setValidators(validators);
        testValueControl?.updateValueAndValidity();

        this.clearValidationIssues();
    }

    validateField(fieldName: string): void {
        const control = this.demoForm.get(fieldName);
        if (control && control.errors) {
            const severity = this.validationService.getErrorSeverity(control.errors);
            const message = this.validationService.getErrorMessage(control.errors);

            const validationError: ValidationError = {
                field: fieldName,
                message,
                code: Object.keys(control.errors)[0],
                severity
            };

            if (severity === 'error') {
                this.validationErrors.update(errors => {
                    const filtered = errors.filter(e => e.field !== fieldName);
                    return [...filtered, validationError];
                });
            } else {
                this.validationWarnings.update(warnings => {
                    const filtered = warnings.filter(w => w.field !== fieldName);
                    return [...filtered, validationError];
                });
            }
        } else {
            // Remove any existing errors/warnings for this field
            this.validationErrors.update(errors => errors.filter(e => e.field !== fieldName));
            this.validationWarnings.update(warnings => warnings.filter(w => w.field !== fieldName));
        }
    }

    validateEquipment(): void {
        const equipment1 = this.demoForm.get('equipment1')?.value;
        const equipment2 = this.demoForm.get('equipment2')?.value;

        if (equipment1 && equipment2 && equipment1 === equipment2) {
            const validationError: ValidationError = {
                field: 'equipment',
                message: 'Different equipment must be selected',
                code: 'differentEquipment',
                severity: 'error'
            };

            this.validationErrors.update(errors => {
                const filtered = errors.filter(e => e.field !== 'equipment');
                return [...filtered, validationError];
            });
        } else {
            this.validationErrors.update(errors => errors.filter(e => e.field !== 'equipment'));
        }
    }

    onSubmit(): void {
        if (this.demoForm.valid) {
            // Perform comprehensive validation
            const testId = parseInt(this.demoForm.get('testType')?.value || '1');
            const validationResult = this.validationService.validateTestForm(this.demoForm, testId);

            if (validationResult.isValid) {
                this.notificationService.showSuccess('Form validation passed successfully!');
                this.clearValidationIssues();
            } else {
                this.validationErrors.set(validationResult.errors);
                this.validationWarnings.set(validationResult.warnings);

                this.notificationService.showValidationErrors(
                    validationResult.errors.map(e => ({ field: e.field, message: e.message }))
                );

                if (validationResult.warnings.length > 0) {
                    this.notificationService.showValidationWarnings(
                        validationResult.warnings.map(w => ({ field: w.field, message: w.message }))
                    );
                }
            }
        } else {
            this.notificationService.showError('Please correct the form errors before submitting');
        }
    }

    clearForm(): void {
        this.demoForm.reset();
        this.clearValidationIssues();
        this.notificationService.showInfo('Form cleared');
    }

    simulateServerError(): void {
        this.generalError.set('Simulated server error occurred');
        this.notificationService.showError('Server error occurred', ['Connection timeout', 'Database unavailable'], true);
    }

    onDismissError(type: 'errors' | 'warnings' | 'general'): void {
        switch (type) {
            case 'errors':
                this.validationErrors.set([]);
                break;
            case 'warnings':
                this.validationWarnings.set([]);
                break;
            case 'general':
                this.generalError.set(null);
                break;
        }
    }

    onRetry(): void {
        this.generalError.set(null);
        this.notificationService.showInfo('Retrying operation...');
    }

    private clearValidationIssues(): void {
        this.validationErrors.set([]);
        this.validationWarnings.set([]);
        this.generalError.set(null);
    }
}