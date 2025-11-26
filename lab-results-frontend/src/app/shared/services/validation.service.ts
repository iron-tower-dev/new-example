import { Injectable, signal, computed } from '@angular/core';
import { AbstractControl, ValidationErrors, ValidatorFn, FormGroup, FormArray } from '@angular/forms';

export interface ValidationError {
    field: string;
    message: string;
    code: string;
    severity?: 'error' | 'warning' | 'info';
}

export interface ValidationResult {
    isValid: boolean;
    errors: ValidationError[];
    warnings: ValidationError[];
}

export interface TestSpecificValidationRules {
    testId: number;
    testName: string;
    fieldRules: { [fieldName: string]: ValidationRule[] };
    crossFieldRules?: CrossFieldValidationRule[];
}

export interface ValidationRule {
    type: 'required' | 'numeric' | 'positive' | 'range' | 'decimal' | 'custom';
    params?: any;
    message?: string;
    severity?: 'error' | 'warning';
}

export interface CrossFieldValidationRule {
    fields: string[];
    validator: ValidatorFn;
    message: string;
    severity?: 'error' | 'warning';
}

@Injectable({
    providedIn: 'root'
})
export class ValidationService {
    // Global validation errors and warnings signals
    private _validationErrors = signal<ValidationError[]>([]);
    private _validationWarnings = signal<ValidationError[]>([]);
    private _testSpecificRules = signal<Map<number, TestSpecificValidationRules>>(new Map());

    readonly validationErrors = this._validationErrors.asReadonly();
    readonly validationWarnings = this._validationWarnings.asReadonly();
    readonly hasValidationErrors = computed(() => this._validationErrors().length > 0);
    readonly hasValidationWarnings = computed(() => this._validationWarnings().length > 0);
    readonly hasAnyValidationIssues = computed(() =>
        this.hasValidationErrors() || this.hasValidationWarnings()
    );

    /**
     * Numeric field validator
     */
    static numericValidator(): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.value) {
                return null; // Let required validator handle empty values
            }

            const value = control.value;
            if (isNaN(value) || isNaN(parseFloat(value))) {
                return { numeric: { message: 'Value must be a valid number' } };
            }

            return null;
        };
    }

    /**
     * Positive number validator
     */
    static positiveNumberValidator(): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.value) {
                return null;
            }

            const numValue = parseFloat(control.value);
            if (isNaN(numValue)) {
                return { numeric: { message: 'Value must be a valid number' } };
            }

            if (numValue <= 0) {
                return { positiveNumber: { message: 'Value must be greater than zero' } };
            }

            return null;
        };
    }

    /**
     * Range validator
     */
    static rangeValidator(min: number, max: number): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.value) {
                return null;
            }

            const numValue = parseFloat(control.value);
            if (isNaN(numValue)) {
                return { numeric: { message: 'Value must be a valid number' } };
            }

            if (numValue < min || numValue > max) {
                return {
                    range: {
                        message: `Value must be between ${min} and ${max}`,
                        min,
                        max,
                        actual: numValue
                    }
                };
            }

            return null;
        };
    }

    /**
     * Decimal places validator
     */
    static decimalPlacesValidator(maxDecimals: number): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.value) {
                return null;
            }

            const value = control.value.toString();
            const decimalIndex = value.indexOf('.');

            if (decimalIndex !== -1) {
                const decimalPlaces = value.length - decimalIndex - 1;
                if (decimalPlaces > maxDecimals) {
                    return {
                        decimalPlaces: {
                            message: `Maximum ${maxDecimals} decimal places allowed`,
                            maxDecimals,
                            actual: decimalPlaces
                        }
                    };
                }
            }

            return null;
        };
    }

    /**
     * Equipment selection validator (ensures different equipment selected)
     */
    static differentEquipmentValidator(otherControlName: string): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.parent || !control.value) {
                return null;
            }

            const otherControl = control.parent.get(otherControlName);
            if (!otherControl || !otherControl.value) {
                return null;
            }

            if (control.value === otherControl.value) {
                return {
                    differentEquipment: {
                        message: 'Must select different equipment',
                        conflictsWith: otherControlName
                    }
                };
            }

            return null;
        };
    }

    /**
     * TAN test specific validator
     */
    static tanValidator(): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.value) {
                return null;
            }

            const numValue = parseFloat(control.value);
            if (isNaN(numValue)) {
                return { numeric: { message: 'TAN value must be numeric' } };
            }

            if (numValue < 0.01) {
                return {
                    tanMinimum: {
                        message: 'TAN value cannot be less than 0.01 mg KOH/g',
                        minimum: 0.01,
                        actual: numValue
                    }
                };
            }

            if (numValue > 50) {
                return {
                    tanMaximum: {
                        message: 'TAN value seems unusually high (>50 mg KOH/g). Please verify.',
                        maximum: 50,
                        actual: numValue,
                        severity: 'warning'
                    }
                };
            }

            return null;
        };
    }

    /**
     * Viscosity test specific validator
     */
    static viscosityValidator(testType: '40C' | '100C'): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.value) {
                return null;
            }

            const numValue = parseFloat(control.value);
            if (isNaN(numValue)) {
                return { numeric: { message: 'Viscosity value must be numeric' } };
            }

            if (numValue <= 0) {
                return {
                    positiveNumber: {
                        message: 'Viscosity must be greater than zero'
                    }
                };
            }

            // Test-specific ranges
            const ranges = {
                '40C': { min: 1, max: 10000, unit: 'cSt @ 40°C' },
                '100C': { min: 1, max: 1000, unit: 'cSt @ 100°C' }
            };

            const range = ranges[testType];
            if (numValue < range.min || numValue > range.max) {
                return {
                    viscosityRange: {
                        message: `${range.unit} should be between ${range.min} and ${range.max}`,
                        min: range.min,
                        max: range.max,
                        actual: numValue,
                        severity: numValue > range.max * 2 ? 'error' : 'warning'
                    }
                };
            }

            return null;
        };
    }

    /**
     * Particle count validator
     */
    static particleCountValidator(): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.value) {
                return null;
            }

            const numValue = parseFloat(control.value);
            if (isNaN(numValue)) {
                return { numeric: { message: 'Particle count must be numeric' } };
            }

            if (numValue < 0) {
                return {
                    nonNegative: {
                        message: 'Particle count cannot be negative'
                    }
                };
            }

            if (numValue > 1000000) {
                return {
                    particleCountHigh: {
                        message: 'Particle count seems unusually high (>1,000,000). Please verify.',
                        maximum: 1000000,
                        actual: numValue,
                        severity: 'warning'
                    }
                };
            }

            return null;
        };
    }

    /**
     * Flash point validator
     */
    static flashPointValidator(): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.value) {
                return null;
            }

            const numValue = parseFloat(control.value);
            if (isNaN(numValue)) {
                return { numeric: { message: 'Flash point must be numeric' } };
            }

            if (numValue < -50 || numValue > 800) {
                return {
                    flashPointRange: {
                        message: 'Flash point should be between -50°F and 800°F',
                        min: -50,
                        max: 800,
                        actual: numValue,
                        severity: 'warning'
                    }
                };
            }

            return null;
        };
    }

    /**
     * Sample weight validator
     */
    static sampleWeightValidator(): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.value) {
                return null;
            }

            const numValue = parseFloat(control.value);
            if (isNaN(numValue)) {
                return { numeric: { message: 'Sample weight must be numeric' } };
            }

            if (numValue <= 0) {
                return {
                    positiveNumber: {
                        message: 'Sample weight must be greater than zero'
                    }
                };
            }

            if (numValue < 0.1) {
                return {
                    sampleWeightLow: {
                        message: 'Sample weight is very low (<0.1g). Results may be inaccurate.',
                        minimum: 0.1,
                        actual: numValue,
                        severity: 'warning'
                    }
                };
            }

            if (numValue > 100) {
                return {
                    sampleWeightHigh: {
                        message: 'Sample weight is very high (>100g). Please verify.',
                        maximum: 100,
                        actual: numValue,
                        severity: 'warning'
                    }
                };
            }

            return null;
        };
    }

    /**
     * Date range validator
     */
    static dateRangeValidator(startDateControlName: string, endDateControlName: string): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.parent) {
                return null;
            }

            const startControl = control.parent.get(startDateControlName);
            const endControl = control.parent.get(endDateControlName);

            if (!startControl?.value || !endControl?.value) {
                return null;
            }

            const startDate = new Date(startControl.value);
            const endDate = new Date(endControl.value);

            if (startDate > endDate) {
                return {
                    dateRange: {
                        message: 'End date must be after start date'
                    }
                };
            }

            return null;
        };
    }

    /**
     * Register test-specific validation rules
     */
    registerTestValidationRules(rules: TestSpecificValidationRules): void {
        this._testSpecificRules.update(rulesMap => {
            const newMap = new Map(rulesMap);
            newMap.set(rules.testId, rules);
            return newMap;
        });
    }

    /**
     * Get test-specific validation rules
     */
    getTestValidationRules(testId: number): TestSpecificValidationRules | undefined {
        return this._testSpecificRules().get(testId);
    }

    /**
     * Create validators for a specific test and field
     */
    createTestFieldValidators(testId: number, fieldName: string): ValidatorFn[] {
        const testRules = this.getTestValidationRules(testId);
        if (!testRules || !testRules.fieldRules[fieldName]) {
            return [];
        }

        const validators: ValidatorFn[] = [];
        const fieldRules = testRules.fieldRules[fieldName];

        fieldRules.forEach(rule => {
            switch (rule.type) {
                case 'required':
                    validators.push(this.createRequiredValidator(rule.message));
                    break;
                case 'numeric':
                    validators.push(ValidationService.numericValidator());
                    break;
                case 'positive':
                    validators.push(ValidationService.positiveNumberValidator());
                    break;
                case 'range':
                    if (rule.params?.min !== undefined && rule.params?.max !== undefined) {
                        validators.push(ValidationService.rangeValidator(rule.params.min, rule.params.max));
                    }
                    break;
                case 'decimal':
                    if (rule.params?.maxDecimals !== undefined) {
                        validators.push(ValidationService.decimalPlacesValidator(rule.params.maxDecimals));
                    }
                    break;
                case 'custom':
                    if (rule.params?.validator) {
                        validators.push(rule.params.validator);
                    }
                    break;
            }
        });

        return validators;
    }

    /**
     * Add validation error
     */
    addValidationError(error: ValidationError): void {
        const severity = error.severity || 'error';

        if (severity === 'error') {
            this._validationErrors.update(errors => {
                const filtered = errors.filter(e => e.field !== error.field);
                return [...filtered, error];
            });
        } else {
            this._validationWarnings.update(warnings => {
                const filtered = warnings.filter(w => w.field !== error.field);
                return [...filtered, error];
            });
        }
    }

    /**
     * Add validation warning
     */
    addValidationWarning(warning: ValidationError): void {
        this.addValidationError({ ...warning, severity: 'warning' });
    }

    /**
     * Remove validation error for a specific field
     */
    removeValidationError(field: string): void {
        this._validationErrors.update(errors =>
            errors.filter(e => e.field !== field)
        );
        this._validationWarnings.update(warnings =>
            warnings.filter(w => w.field !== field)
        );
    }

    /**
     * Clear all validation errors
     */
    clearValidationErrors(): void {
        this._validationErrors.set([]);
    }

    /**
     * Clear all validation warnings
     */
    clearValidationWarnings(): void {
        this._validationWarnings.set([]);
    }

    /**
     * Clear all validation issues
     */
    clearAllValidationIssues(): void {
        this.clearValidationErrors();
        this.clearValidationWarnings();
    }

    /**
     * Validate form and return result
     */
    validateForm(form: AbstractControl, testId?: number): ValidationResult {
        const errors: ValidationError[] = [];
        const warnings: ValidationError[] = [];

        // Collect all form errors and warnings
        this.collectFormErrors(form, '', errors, warnings);

        // Apply test-specific cross-field validation if testId provided
        if (testId) {
            const testRules = this.getTestValidationRules(testId);
            if (testRules?.crossFieldRules) {
                testRules.crossFieldRules.forEach(rule => {
                    const validationResult = rule.validator(form);
                    if (validationResult) {
                        const error: ValidationError = {
                            field: rule.fields.join(','),
                            message: rule.message,
                            code: 'crossField',
                            severity: rule.severity || 'error'
                        };

                        if (error.severity === 'error') {
                            errors.push(error);
                        } else {
                            warnings.push(error);
                        }
                    }
                });
            }
        }

        return {
            isValid: errors.length === 0,
            errors,
            warnings
        };
    }

    /**
     * Validate specific test form with test-specific rules
     */
    validateTestForm(form: FormGroup, testId: number): ValidationResult {
        return this.validateForm(form, testId);
    }

    /**
     * Validate trial data for specific test
     */
    validateTrialData(trialData: any, testId: number, trialNumber: number): ValidationResult {
        const errors: ValidationError[] = [];
        const warnings: ValidationError[] = [];

        const testRules = this.getTestValidationRules(testId);
        if (!testRules) {
            return { isValid: true, errors: [], warnings: [] };
        }

        // Validate each field in trial data
        Object.keys(trialData).forEach(fieldName => {
            const fieldRules = testRules.fieldRules[fieldName];
            if (fieldRules) {
                fieldRules.forEach(rule => {
                    const value = trialData[fieldName];
                    const validationError = this.validateFieldValue(value, rule, `trial${trialNumber}.${fieldName}`);

                    if (validationError) {
                        if (validationError.severity === 'error') {
                            errors.push(validationError);
                        } else {
                            warnings.push(validationError);
                        }
                    }
                });
            }
        });

        return {
            isValid: errors.length === 0,
            errors,
            warnings
        };
    }

    /**
     * Validate individual field value against rule
     */
    private validateFieldValue(value: any, rule: ValidationRule, fieldPath: string): ValidationError | null {
        let isValid = true;
        let message = rule.message || 'Invalid value';

        switch (rule.type) {
            case 'required':
                isValid = value !== null && value !== undefined && value !== '';
                message = rule.message || 'This field is required';
                break;
            case 'numeric':
                isValid = !isNaN(parseFloat(value)) && isFinite(value);
                message = rule.message || 'Must be a valid number';
                break;
            case 'positive':
                const numValue = parseFloat(value);
                isValid = !isNaN(numValue) && numValue > 0;
                message = rule.message || 'Must be a positive number';
                break;
            case 'range':
                const rangeValue = parseFloat(value);
                isValid = !isNaN(rangeValue) &&
                    rangeValue >= (rule.params?.min || -Infinity) &&
                    rangeValue <= (rule.params?.max || Infinity);
                message = rule.message || `Value must be between ${rule.params?.min} and ${rule.params?.max}`;
                break;
        }

        if (!isValid) {
            return {
                field: fieldPath,
                message,
                code: rule.type,
                severity: rule.severity || 'error'
            };
        }

        return null;
    }

    /**
     * Get user-friendly error message from validation errors
     */
    getErrorMessage(errors: ValidationErrors): string {
        if (errors['required']) {
            return 'This field is required';
        }

        if (errors['numeric']) {
            return errors['numeric'].message || 'Must be a valid number';
        }

        if (errors['positiveNumber']) {
            return errors['positiveNumber'].message || 'Must be a positive number';
        }

        if (errors['range']) {
            return errors['range'].message || 'Value is out of range';
        }

        if (errors['decimalPlaces']) {
            return errors['decimalPlaces'].message || 'Too many decimal places';
        }

        if (errors['differentEquipment']) {
            return errors['differentEquipment'].message || 'Must select different equipment';
        }

        if (errors['dateRange']) {
            return errors['dateRange'].message || 'Invalid date range';
        }

        // Test-specific error messages
        if (errors['tanMinimum']) {
            return errors['tanMinimum'].message || 'TAN value too low';
        }

        if (errors['tanMaximum']) {
            return errors['tanMaximum'].message || 'TAN value unusually high';
        }

        if (errors['viscosityRange']) {
            return errors['viscosityRange'].message || 'Viscosity value out of expected range';
        }

        if (errors['particleCountHigh']) {
            return errors['particleCountHigh'].message || 'Particle count unusually high';
        }

        if (errors['flashPointRange']) {
            return errors['flashPointRange'].message || 'Flash point out of expected range';
        }

        if (errors['sampleWeightLow']) {
            return errors['sampleWeightLow'].message || 'Sample weight very low';
        }

        if (errors['sampleWeightHigh']) {
            return errors['sampleWeightHigh'].message || 'Sample weight very high';
        }

        if (errors['nonNegative']) {
            return errors['nonNegative'].message || 'Value cannot be negative';
        }

        // Return first error message found
        const firstError = Object.values(errors)[0];
        if (typeof firstError === 'object' && firstError?.message) {
            return firstError.message;
        }

        return 'Invalid value';
    }

    /**
     * Get error severity from validation errors
     */
    getErrorSeverity(errors: ValidationErrors): 'error' | 'warning' | 'info' {
        // Check for warning-level errors
        const warningErrors = ['tanMaximum', 'viscosityRange', 'particleCountHigh', 'flashPointRange', 'sampleWeightLow', 'sampleWeightHigh'];

        for (const errorKey of Object.keys(errors)) {
            const error = errors[errorKey];
            if (typeof error === 'object' && error?.severity) {
                return error.severity;
            }
            if (warningErrors.includes(errorKey)) {
                return 'warning';
            }
        }

        return 'error';
    }

    /**
     * Create custom required validator with message
     */
    private createRequiredValidator(message?: string): ValidatorFn {
        return (control: AbstractControl): ValidationErrors | null => {
            if (!control.value || (typeof control.value === 'string' && control.value.trim().length === 0)) {
                return { required: { message: message || 'This field is required' } };
            }
            return null;
        };
    }

    /**
     * Recursively collect form errors and warnings
     */
    private collectFormErrors(control: AbstractControl, path: string, errors: ValidationError[], warnings?: ValidationError[]): void {
        if (control.errors) {
            Object.keys(control.errors).forEach(key => {
                const severity = this.getErrorSeverity(control.errors!);
                const validationError: ValidationError = {
                    field: path,
                    message: this.getErrorMessage(control.errors!),
                    code: key,
                    severity
                };

                if (severity === 'error') {
                    errors.push(validationError);
                } else if (warnings && (severity === 'warning' || severity === 'info')) {
                    warnings.push(validationError);
                }
            });
        }

        if (control instanceof FormGroup) {
            Object.keys(control.controls).forEach(key => {
                const childControl = control.get(key);
                if (childControl) {
                    const childPath = path ? `${path}.${key}` : key;
                    this.collectFormErrors(childControl, childPath, errors, warnings);
                }
            });
        }

        if (control instanceof FormArray) {
            control.controls.forEach((childControl, index) => {
                const childPath = `${path}[${index}]`;
                this.collectFormErrors(childControl, childPath, errors, warnings);
            });
        }
    }

    /**
     * Initialize default test validation rules
     */
    initializeDefaultTestRules(): void {
        // TAN Test Rules
        this.registerTestValidationRules({
            testId: 1,
            testName: 'TAN by Color Indication',
            fieldRules: {
                'sampleWeight': [
                    { type: 'required', message: 'Sample weight is required' },
                    { type: 'positive', message: 'Sample weight must be positive' },
                    { type: 'range', params: { min: 0.01, max: 100 }, message: 'Sample weight should be between 0.01g and 100g' }
                ],
                'finalBuret': [
                    { type: 'required', message: 'Final buret reading is required' },
                    { type: 'numeric', message: 'Final buret must be numeric' },
                    { type: 'range', params: { min: 0, max: 100 }, message: 'Final buret reading should be between 0 and 100 mL' }
                ],
                'tanResult': [
                    { type: 'custom', params: { validator: ValidationService.tanValidator() } }
                ]
            }
        });

        // Viscosity Test Rules
        this.registerTestValidationRules({
            testId: 2,
            testName: 'Viscosity @ 40°C',
            fieldRules: {
                'stopwatchTime': [
                    { type: 'required', message: 'Stopwatch time is required' },
                    { type: 'positive', message: 'Stopwatch time must be positive' }
                ],
                'tubeId': [
                    { type: 'required', message: 'Tube ID selection is required' }
                ],
                'viscosityResult': [
                    { type: 'custom', params: { validator: ValidationService.viscosityValidator('40C') } }
                ]
            }
        });

        // Add more test rules as needed...
    }
}

// Re-export form classes for convenience
export { FormGroup, FormControl, FormArray, FormBuilder, Validators } from '@angular/forms';