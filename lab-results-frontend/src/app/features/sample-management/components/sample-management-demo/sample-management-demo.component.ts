import { Component, OnInit, inject, signal, computed, effect } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SampleService } from '../../../../shared/services/sample.service';
import { ValidationService } from '../../../../shared/services/validation.service';
import { Sample } from '../../../../shared/models/sample.model';
import { NavigationAction, NavigationState } from '../../../../shared/components/common-navigation/common-navigation.component';
import { SharedModule } from '../../../../shared/shared.module';
import { CommonNavigationComponent } from '../../../../shared/components/common-navigation/common-navigation.component';
import { SampleDataTemplateComponent } from '../sample-data-template/sample-data-template.component';

@Component({
  selector: 'app-sample-management-demo',
  standalone: true,
  imports: [SharedModule, CommonNavigationComponent, SampleDataTemplateComponent],
  template: `
    <div class="demo-container">
      <h1>Sample Management Demo</h1>
      
      <!-- Common Navigation -->
      <app-common-navigation
        [showDelete]="true"
        [customActions]="customActions()"
        (saveClicked)="onSave()"
        (clearClicked)="onClear()"
        (deleteClicked)="onDelete()"
        (customActionClicked)="onCustomAction($event)">
      </app-common-navigation>

      <!-- Sample Data Template -->
      <app-sample-data-template
        [testId]="selectedTestId()"
        [testName]="selectedTestName()"
        [required]="true"
        [showHistory]="true"
        [showFilters]="true"
        (sampleSelected)="onSampleSelected($event)">
      </app-sample-data-template>

      <!-- Demo Form (simulating test entry) -->
      @if (selectedSample()) {
        <mat-card class="demo-form-card">
          <mat-card-header>
            <mat-card-title>Demo Test Entry Form</mat-card-title>
            <mat-card-subtitle>Sample: {{ selectedSample()?.tagNumber }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <form [formGroup]="demoForm" class="demo-form">
              <div class="form-grid">
                <mat-form-field appearance="outline">
                  <mat-label>Test Value 1</mat-label>
                  <input matInput type="number" formControlName="value1" placeholder="Enter value">
                  @if (demoForm.get('value1')?.errors && demoForm.get('value1')?.touched) {
                    <mat-error>{{ getFieldError('value1') }}</mat-error>
                  }
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Test Value 2</mat-label>
                  <input matInput type="number" formControlName="value2" placeholder="Enter value">
                  @if (demoForm.get('value2')?.errors && demoForm.get('value2')?.touched) {
                    <mat-error>{{ getFieldError('value2') }}</mat-error>
                  }
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Comments</mat-label>
                  <textarea matInput formControlName="comments" rows="3" placeholder="Enter comments"></textarea>
                </mat-form-field>

                <mat-form-field appearance="outline">
                  <mat-label>Test Date</mat-label>
                  <input matInput [matDatepicker]="testDatePicker" formControlName="testDate">
                  <mat-datepicker-toggle matSuffix [for]="testDatePicker"></mat-datepicker-toggle>
                  <mat-datepicker #testDatePicker></mat-datepicker>
                </mat-form-field>
              </div>

              <!-- Calculated Result -->
              @if (calculatedResult(); as result) {
                <div class="calculated-result">
                  <mat-card>
                    <mat-card-content>
                      <div class="result-display">
                        <mat-icon>calculate</mat-icon>
                        <span class="result-label">Calculated Result:</span>
                        <span class="result-value">{{ result | number:'1.2-2' }}</span>
                      </div>
                    </mat-card-content>
                  </mat-card>
                </div>
              }
            </form>
          </mat-card-content>
        </mat-card>
      }

      <!-- Test Selection -->
      <mat-card class="test-selection-card">
        <mat-card-header>
          <mat-card-title>Test Selection</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Select Test Type</mat-label>
            <mat-select [value]="selectedTestId()" (selectionChange)="onTestSelected($event.value)">
              @for (test of availableTests; track test.id) {
                <mat-option [value]="test.id">{{ test.name }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .demo-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 16px;
    }

    .demo-form-card,
    .test-selection-card {
      margin-top: 16px;
    }

    .demo-form {
      width: 100%;
    }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 16px;
      margin-bottom: 16px;
    }

    .full-width {
      width: 100%;
    }

    .calculated-result {
      margin-top: 16px;
    }

    .result-display {
      display: flex;
      align-items: center;
      gap: 12px;
      font-size: 1.1em;
    }

    .result-label {
      font-weight: 500;
    }

    .result-value {
      font-weight: bold;
      color: #1976d2;
      font-size: 1.2em;
    }

    @media (max-width: 768px) {
      .form-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class SampleManagementDemoComponent implements OnInit {
  private fb = inject(FormBuilder);
  private sampleService = inject(SampleService);
  private validationService = inject(ValidationService);

  // Signals
  private _selectedTestId = signal<number | null>(1);
  private _selectedTestName = signal<string | null>('TAN by Color Indication');
  private _hasUnsavedChanges = signal(false);

  readonly selectedTestId = this._selectedTestId.asReadonly();
  readonly selectedTestName = this._selectedTestName.asReadonly();
  readonly selectedSample = this.sampleService.selectedSample;

  // Demo form
  demoForm: FormGroup;

  // Available tests for demo
  availableTests = [
    { id: 1, name: 'TAN by Color Indication' },
    { id: 2, name: 'Water-KF' },
    { id: 3, name: 'TBN by Auto Titration' },
    { id: 4, name: 'Viscosity @ 40°C' },
    { id: 5, name: 'Emission Spectroscopy' }
  ];

  // Custom actions for navigation
  readonly customActions = computed(() => {
    const actions: NavigationAction[] = [
      {
        type: 'custom',
        label: 'Print',
        icon: 'print',
        color: 'primary'
      }
    ];

    if (this.selectedSample()) {
      actions.push({
        type: 'custom',
        label: 'History',
        icon: 'history',
        color: 'primary'
      });
    }

    return actions;
  });

  // Calculated result based on form values
  readonly calculatedResult = computed(() => {
    const value1 = this.demoForm?.get('value1')?.value;
    const value2 = this.demoForm?.get('value2')?.value;

    if (value1 && value2 && !isNaN(value1) && !isNaN(value2)) {
      // Simple calculation for demo (could be test-specific)
      return (parseFloat(value1) + parseFloat(value2)) / 2;
    }

    return null;
  });

  constructor() {
    this.demoForm = this.fb.group({
      value1: ['', [
        Validators.required,
        ValidationService.numericValidator(),
        ValidationService.positiveNumberValidator()
      ]],
      value2: ['', [
        Validators.required,
        ValidationService.numericValidator(),
        ValidationService.positiveNumberValidator()
      ]],
      comments: [''],
      testDate: [new Date(), Validators.required]
    });

    // Track form changes for unsaved changes indicator
    effect(() => {
      this.demoForm.valueChanges.subscribe(() => {
        this._hasUnsavedChanges.set(this.demoForm.dirty);
        this.updateNavigationState();
      });
    });
  }

  ngOnInit(): void {
    this.updateNavigationState();
  }

  onSampleSelected(sample: Sample | null): void {
    if (sample) {
      console.log('Sample selected:', sample);
      // Reset form when new sample is selected
      this.demoForm.reset({
        testDate: new Date()
      });
      this._hasUnsavedChanges.set(false);
    }
    this.updateNavigationState();
  }

  onTestSelected(testId: number): void {
    this._selectedTestId.set(testId);
    const test = this.availableTests.find(t => t.id === testId);
    this._selectedTestName.set(test?.name || null);

    // Clear current sample selection when test changes
    this.sampleService.selectSample(null);
    this.demoForm.reset({
      testDate: new Date()
    });
    this._hasUnsavedChanges.set(false);
    this.updateNavigationState();
  }

  onSave(): void {
    if (this.demoForm.valid) {
      console.log('Saving form data:', this.demoForm.value);
      console.log('Selected sample:', this.selectedSample());

      // Simulate save operation
      setTimeout(() => {
        this._hasUnsavedChanges.set(false);
        this.demoForm.markAsPristine();
        this.updateNavigationState();

        // Show success message (would be handled by navigation component)
        console.log('Data saved successfully!');
      }, 1000);
    } else {
      console.log('Form is invalid');
      this.demoForm.markAllAsTouched();
    }
  }

  onClear(): void {
    this.demoForm.reset({
      testDate: new Date()
    });
    this._hasUnsavedChanges.set(false);
    this.updateNavigationState();
    console.log('Form cleared');
  }

  onDelete(): void {
    console.log('Delete action triggered');
    // Simulate delete operation
    this.onClear();
  }

  onCustomAction(action: NavigationAction): void {
    console.log('Custom action triggered:', action);

    switch (action.label) {
      case 'Print':
        console.log('Printing report...');
        break;
      case 'History':
        console.log('Showing history for sample:', this.selectedSample());
        break;
    }
  }

  getFieldError(fieldName: string): string {
    const control = this.demoForm.get(fieldName);
    if (control?.errors) {
      return this.validationService.getErrorMessage(control.errors);
    }
    return '';
  }

  private updateNavigationState(): void {
    // This would typically be handled by the navigation component
    // but we're simulating it here for demo purposes
    const navigationState: NavigationState = {
      hasUnsavedChanges: this._hasUnsavedChanges(),
      isLoading: false,
      canSave: this.demoForm.valid && this.selectedSample() !== null,
      canDelete: this.selectedSample() !== null,
      selectedItems: this.selectedSample() ? [this.selectedSample()] : []
    };

    console.log('Navigation state updated:', navigationState);
  }
}