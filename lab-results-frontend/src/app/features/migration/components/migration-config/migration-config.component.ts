import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MigrationService, MigrationOptions } from '../../services/migration.service';

@Component({
    selector: 'app-migration-config',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule
    ],
    templateUrl: './migration-config.component.html',
    styleUrls: ['./migration-config.component.scss']
})
export class MigrationConfigComponent implements OnInit {
    migrationForm: FormGroup;
    isStarting = false;
    error: string | null = null;
    success: string | null = null;

    // Available tables (this would typically come from an API)
    availableTables = [
        'AllResults', 'Comments', 'EmSpectro', 'FTIR', 'Ferrogram',
        'InspectFilter', 'LubeTechList', 'ParticleCount', 'Test',
        'TestReadings', 'TestSchedule', 'Equipment', 'Sample'
    ];

    constructor(
        private fb: FormBuilder,
        private migrationService: MigrationService
    ) {
        this.migrationForm = this.createForm();
    }

    ngOnInit(): void {
        this.loadDefaultConfiguration();
    }

    private createForm(): FormGroup {
        return this.fb.group({
            // General Options
            clearExistingData: [true],
            createMissingTables: [true],
            validateAgainstLegacy: [true],
            removeAuthentication: [false],
            maxConcurrentOperations: [4, [Validators.min(1), Validators.max(10)]],
            operationTimeoutMinutes: [30, [Validators.min(5), Validators.max(120)]],

            // Table Selection
            includeTables: this.fb.array([]),
            excludeTables: this.fb.array([]),

            // Seeding Options
            seedingOptions: this.fb.group({
                clearExistingData: [true],
                createMissingTables: [true],
                batchSize: [1000, [Validators.min(100), Validators.max(10000)]],
                continueOnError: [true],
                validateBeforeInsert: [true],
                useTransactions: [true],
                commandTimeoutMinutes: [5, [Validators.min(1), Validators.max(30)]]
            }),

            // Validation Options
            validationOptions: this.fb.group({
                compareQueryResults: [true],
                comparePerformance: [true],
                generateDetailedReports: [true],
                maxDiscrepanciesToReport: [100, [Validators.min(10), Validators.max(1000)]],
                performanceThresholdPercent: [20.0, [Validators.min(0), Validators.max(100)]],
                queryTimeoutMinutes: [2, [Validators.min(1), Validators.max(10)]],
                legacyConnectionString: [''],
                ignoreMinorDifferences: [true]
            }),

            // Auth Removal Options
            authRemovalOptions: this.fb.group({
                createBackup: [true],
                backupDirectory: ['auth-backup', Validators.required],
                removeFromApi: [true],
                removeFromFrontend: [true],
                updateDocumentation: [true],
                filesToExclude: this.fb.array([])
            })
        });
    }

    private loadDefaultConfiguration(): void {
        const defaultOptions = this.migrationService.getDefaultMigrationOptions();
        this.migrationForm.patchValue(defaultOptions);
    }

    get includeTablesArray(): FormArray {
        return this.migrationForm.get('includeTables') as FormArray;
    }

    get excludeTablesArray(): FormArray {
        return this.migrationForm.get('excludeTables') as FormArray;
    }

    get filesToExcludeArray(): FormArray {
        return this.migrationForm.get('authRemovalOptions.filesToExclude') as FormArray;
    }

    addIncludeTable(): void {
        this.includeTablesArray.push(this.fb.control('', Validators.required));
    }

    removeIncludeTable(index: number): void {
        this.includeTablesArray.removeAt(index);
    }

    addExcludeTable(): void {
        this.excludeTablesArray.push(this.fb.control('', Validators.required));
    }

    removeExcludeTable(index: number): void {
        this.excludeTablesArray.removeAt(index);
    }

    addFileToExclude(): void {
        this.filesToExcludeArray.push(this.fb.control('', Validators.required));
    }

    removeFileToExclude(index: number): void {
        this.filesToExcludeArray.removeAt(index);
    }

    onPresetChange(preset: string): void {
        switch (preset) {
            case 'full':
                this.migrationForm.patchValue({
                    clearExistingData: true,
                    createMissingTables: true,
                    validateAgainstLegacy: true,
                    removeAuthentication: false
                });
                break;

            case 'seeding-only':
                this.migrationForm.patchValue({
                    clearExistingData: true,
                    createMissingTables: true,
                    validateAgainstLegacy: false,
                    removeAuthentication: false
                });
                break;

            case 'validation-only':
                this.migrationForm.patchValue({
                    clearExistingData: false,
                    createMissingTables: false,
                    validateAgainstLegacy: true,
                    removeAuthentication: false
                });
                break;

            case 'auth-removal':
                this.migrationForm.patchValue({
                    clearExistingData: false,
                    createMissingTables: false,
                    validateAgainstLegacy: false,
                    removeAuthentication: true
                });
                break;
        }
    }

    onSubmit(): void {
        if (this.migrationForm.valid) {
            this.startMigration();
        } else {
            this.markFormGroupTouched(this.migrationForm);
            this.error = 'Please fix the validation errors before starting the migration.';
        }
    }

    private startMigration(): void {
        this.isStarting = true;
        this.error = null;
        this.success = null;

        const options: MigrationOptions = this.migrationForm.value;

        this.migrationService.startMigration(options).subscribe({
            next: (result) => {
                this.success = `Migration started successfully! Migration ID: ${result.migrationId}`;
                this.isStarting = false;

                // Start polling for updates
                this.migrationService.startPolling();
            },
            error: (error) => {
                this.error = `Failed to start migration: ${error.error?.message || error.message}`;
                this.isStarting = false;
                console.error('Error starting migration:', error);
            }
        });
    }

    private markFormGroupTouched(formGroup: FormGroup): void {
        Object.keys(formGroup.controls).forEach(key => {
            const control = formGroup.get(key);
            if (control instanceof FormGroup) {
                this.markFormGroupTouched(control);
            } else if (control instanceof FormArray) {
                control.controls.forEach(arrayControl => {
                    if (arrayControl instanceof FormGroup) {
                        this.markFormGroupTouched(arrayControl);
                    } else {
                        arrayControl.markAsTouched();
                    }
                });
            } else {
                control?.markAsTouched();
            }
        });
    }

    resetForm(): void {
        this.migrationForm.reset();
        this.loadDefaultConfiguration();
        this.error = null;
        this.success = null;
    }

    exportConfiguration(): void {
        const config = this.migrationForm.value;
        const blob = new Blob([JSON.stringify(config, null, 2)], { type: 'application/json' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'migration-config.json';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
    }

    importConfiguration(event: any): void {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (e) => {
                try {
                    const config = JSON.parse(e.target?.result as string);
                    this.migrationForm.patchValue(config);
                    this.success = 'Configuration imported successfully!';
                    this.error = null;
                } catch (error) {
                    this.error = 'Invalid configuration file format.';
                    this.success = null;
                }
            };
            reader.readAsText(file);
        }
    }

    isFieldInvalid(fieldName: string): boolean {
        const field = this.migrationForm.get(fieldName);
        return !!(field && field.invalid && (field.dirty || field.touched));
    }

    getFieldError(fieldName: string): string {
        const field = this.migrationForm.get(fieldName);
        if (field?.errors) {
            if (field.errors['required']) return 'This field is required';
            if (field.errors['min']) return `Minimum value is ${field.errors['min'].min}`;
            if (field.errors['max']) return `Maximum value is ${field.errors['max'].max}`;
        }
        return '';
    }
}