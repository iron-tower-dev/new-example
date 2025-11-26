import { TestBed } from '@angular/core/testing';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ValidationService, ValidationError, TestSpecificValidationRules } from './validation.service';

describe('ValidationService', () => {
    let service: ValidationService;

    beforeEach(() => {
        TestBed.configureTestingModule({});
        service = TestBed.inject(ValidationService);
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('Static Validators', () => {
        describe('numericValidator', () => {
            it('should return null for valid numeric values', () => {
                const control = new FormControl('123.45');
                const validator = ValidationService.numericValidator();
                expect(validator(control)).toBeNull();
            });

            it('should return error for non-numeric values', () => {
                const control = new FormControl('abc');
                const validator = ValidationService.numericValidator();
                const result = validator(control);
                expect(result).toEqual({ numeric: { message: 'Value must be a valid number' } });
            });

            it('should return null for empty values', () => {
                const control = new FormControl('');
                const validator = ValidationService.numericValidator();
                expect(validator(control)).toBeNull();
            });
        });

        describe('positiveNumberValidator', () => {
            it('should return null for positive numbers', () => {
                const control = new FormControl('5.5');
                const validator = ValidationService.positiveNumberValidator();
                expect(validator(control)).toBeNull();
            });

            it('should return error for zero', () => {
                const control = new FormControl('0');
                const validator = ValidationService.positiveNumberValidator();
                const result = validator(control);
                expect(result).toEqual({ positiveNumber: { message: 'Value must be greater than zero' } });
            });

            it('should return error for negative numbers', () => {
                const control = new FormControl('-5');
                const validator = ValidationService.positiveNumberValidator();
                const result = validator(control);
                expect(result).toEqual({ positiveNumber: { message: 'Value must be greater than zero' } });
            });
        });

        describe('rangeValidator', () => {
            it('should return null for values within range', () => {
                const control = new FormControl('5');
                const validator = ValidationService.rangeValidator(1, 10);
                expect(validator(control)).toBeNull();
            });

            it('should return error for values below range', () => {
                const control = new FormControl('0');
                const validator = ValidationService.rangeValidator(1, 10);
                const result = validator(control);
                expect(result).toEqual({
                    range: {
                        message: 'Value must be between 1 and 10',
                        min: 1,
                        max: 10,
                        actual: 0
                    }
                });
            });

            it('should return error for values above range', () => {
                const control = new FormControl('15');
                const validator = ValidationService.rangeValidator(1, 10);
                const result = validator(control);
                expect(result).toEqual({
                    range: {
                        message: 'Value must be between 1 and 10',
                        min: 1,
                        max: 10,
                        actual: 15
                    }
                });
            });
        });

        describe('tanValidator', () => {
            it('should return null for valid TAN values', () => {
                const control = new FormControl('2.5');
                const validator = ValidationService.tanValidator();
                expect(validator(control)).toBeNull();
            });

            it('should return error for values below minimum', () => {
                const control = new FormControl('0.005');
                const validator = ValidationService.tanValidator();
                const result = validator(control);
                expect(result).toEqual({
                    tanMinimum: {
                        message: 'TAN value cannot be less than 0.01 mg KOH/g',
                        minimum: 0.01,
                        actual: 0.005
                    }
                });
            });

            it('should return warning for unusually high values', () => {
                const control = new FormControl('75');
                const validator = ValidationService.tanValidator();
                const result = validator(control);
                expect(result).toEqual({
                    tanMaximum: {
                        message: 'TAN value seems unusually high (>50 mg KOH/g). Please verify.',
                        maximum: 50,
                        actual: 75,
                        severity: 'warning'
                    }
                });
            });
        });

        describe('viscosityValidator', () => {
            it('should return null for valid viscosity values at 40C', () => {
                const control = new FormControl('100');
                const validator = ValidationService.viscosityValidator('40C');
                expect(validator(control)).toBeNull();
            });

            it('should return error for zero viscosity', () => {
                const control = new FormControl('0');
                const validator = ValidationService.viscosityValidator('40C');
                const result = validator(control);
                expect(result).toEqual({
                    positiveNumber: {
                        message: 'Viscosity must be greater than zero'
                    }
                });
            });

            it('should return warning for values outside expected range', () => {
                const control = new FormControl('15000');
                const validator = ValidationService.viscosityValidator('40C');
                const result = validator(control);
                expect(result).toEqual({
                    viscosityRange: {
                        message: 'cSt @ 40°C should be between 1 and 10000',
                        min: 1,
                        max: 10000,
                        actual: 15000,
                        severity: 'warning'
                    }
                });
            });
        });
    });

    describe('Validation State Management', () => {
        it('should add validation errors', () => {
            const error: ValidationError = {
                field: 'testField',
                message: 'Test error',
                code: 'test'
            };

            service.addValidationError(error);
            expect(service.validationErrors()).toContain(error);
            expect(service.hasValidationErrors()).toBe(true);
        });

        it('should add validation warnings', () => {
            const warning: ValidationError = {
                field: 'testField',
                message: 'Test warning',
                code: 'test',
                severity: 'warning'
            };

            service.addValidationWarning(warning);
            expect(service.validationWarnings()).toContain(warning);
            expect(service.hasValidationWarnings()).toBe(true);
        });

        it('should remove validation errors for specific field', () => {
            const error: ValidationError = {
                field: 'testField',
                message: 'Test error',
                code: 'test'
            };

            service.addValidationError(error);
            expect(service.validationErrors()).toContain(error);

            service.removeValidationError('testField');
            expect(service.validationErrors()).not.toContain(error);
        });

        it('should clear all validation errors', () => {
            const error: ValidationError = {
                field: 'testField',
                message: 'Test error',
                code: 'test'
            };

            service.addValidationError(error);
            expect(service.hasValidationErrors()).toBe(true);

            service.clearValidationErrors();
            expect(service.hasValidationErrors()).toBe(false);
        });
    });

    describe('Test-Specific Validation Rules', () => {
        it('should register and retrieve test validation rules', () => {
            const rules: TestSpecificValidationRules = {
                testId: 1,
                testName: 'Test TAN',
                fieldRules: {
                    'sampleWeight': [
                        { type: 'required', message: 'Sample weight is required' },
                        { type: 'positive', message: 'Sample weight must be positive' }
                    ]
                }
            };

            service.registerTestValidationRules(rules);
            const retrieved = service.getTestValidationRules(1);
            expect(retrieved).toEqual(rules);
        });

        it('should create validators for test fields', () => {
            const rules: TestSpecificValidationRules = {
                testId: 1,
                testName: 'Test TAN',
                fieldRules: {
                    'sampleWeight': [
                        { type: 'required', message: 'Sample weight is required' },
                        { type: 'numeric' },
                        { type: 'positive' }
                    ]
                }
            };

            service.registerTestValidationRules(rules);
            const validators = service.createTestFieldValidators(1, 'sampleWeight');
            expect(validators.length).toBe(3);
        });
    });

    describe('Form Validation', () => {
        it('should validate form and return results', () => {
            const form = new FormGroup({
                sampleWeight: new FormControl('', [Validators.required]),
                finalBuret: new FormControl('abc', [ValidationService.numericValidator()])
            });

            const result = service.validateForm(form);
            expect(result.isValid).toBe(false);
            expect(result.errors.length).toBe(2);
        });

        // validateTrialData method has been removed - validation is now handled through form validators
    });

    describe('Error Message Handling', () => {
        it('should return appropriate error messages', () => {
            expect(service.getErrorMessage({ required: true })).toBe('This field is required');
            expect(service.getErrorMessage({ numeric: { message: 'Must be numeric' } })).toBe('Must be numeric');
            expect(service.getErrorMessage({ positiveNumber: { message: 'Must be positive' } })).toBe('Must be positive');
        });

        it('should determine error severity', () => {
            expect(service.getErrorSeverity({ required: true })).toBe('error');
            expect(service.getErrorSeverity({ tanMaximum: { severity: 'warning' } })).toBe('warning');
            expect(service.getErrorSeverity({ viscosityRange: true })).toBe('warning');
        });
    });

    describe('Initialization', () => {
        it('should initialize default test rules', () => {
            service.initializeDefaultTestRules();

            const tanRules = service.getTestValidationRules(1);
            expect(tanRules).toBeDefined();
            expect(tanRules?.testName).toBe('TAN by Color Indication');
            expect(tanRules?.fieldRules['sampleWeight']).toBeDefined();
            expect(tanRules?.fieldRules['finalBuret']).toBeDefined();
        });
    });
});