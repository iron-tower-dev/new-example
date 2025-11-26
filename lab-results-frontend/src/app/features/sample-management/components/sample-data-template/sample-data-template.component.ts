import { Component, Input, Output, EventEmitter, OnInit, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { SampleService } from '../../../../shared/services/sample.service';
import { Sample, SampleFilter } from '../../../../shared/models/sample.model';
import { SharedModule } from '../../../../shared/shared.module';
import { SampleSelectionComponent } from '../sample-selection/sample-selection.component';
import { SampleInfoDisplayComponent } from '../sample-info-display/sample-info-display.component';

@Component({
  selector: 'app-sample-data-template',
  standalone: true,
  imports: [SharedModule, SampleSelectionComponent, SampleInfoDisplayComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="sample-template-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>
            <mat-icon>search</mat-icon>
            Sample Selection & Information
          </mat-card-title>
          <div class="spacer"></div>
          @if (showFiltersSignal()) {
            <button mat-icon-button (click)="toggleFilters()" title="Hide Filters">
              <mat-icon>filter_list_off</mat-icon>
            </button>
          } @else {
            <button mat-icon-button (click)="toggleFilters()" title="Show Filters">
              <mat-icon>filter_list</mat-icon>
            </button>
          }
        </mat-card-header>

        <mat-card-content>
          <!-- Filter Section -->
          @if (showFiltersSignal()) {
            <div class="filter-section">
              <form [formGroup]="filterForm" class="filter-form">
                <div class="filter-grid">
                  <mat-form-field appearance="outline">
                    <mat-label>Tag Number</mat-label>
                    <input matInput formControlName="tagNumber" placeholder="Enter tag number">
                    <mat-icon matSuffix>label</mat-icon>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Component</mat-label>
                    <input matInput formControlName="component" placeholder="Enter component">
                    <mat-icon matSuffix>build</mat-icon>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Location</mat-label>
                    <input matInput formControlName="location" placeholder="Enter location">
                    <mat-icon matSuffix>place</mat-icon>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>Lube Type</mat-label>
                    <input matInput formControlName="lubeType" placeholder="Enter lube type">
                    <mat-icon matSuffix>local_gas_station</mat-icon>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>From Date</mat-label>
                    <input matInput [matDatepicker]="fromPicker" formControlName="fromDate">
                    <mat-datepicker-toggle matSuffix [for]="fromPicker"></mat-datepicker-toggle>
                    <mat-datepicker #fromPicker></mat-datepicker>
                  </mat-form-field>

                  <mat-form-field appearance="outline">
                    <mat-label>To Date</mat-label>
                    <input matInput [matDatepicker]="toPicker" formControlName="toDate">
                    <mat-datepicker-toggle matSuffix [for]="toPicker"></mat-datepicker-toggle>
                    <mat-datepicker #toPicker></mat-datepicker>
                  </mat-form-field>
                </div>

                <div class="filter-actions">
                  <button mat-raised-button color="primary" (click)="applyFilters()" [disabled]="isLoading()">
                    <mat-icon>search</mat-icon>
                    Apply Filters
                  </button>
                  <button mat-button (click)="clearFilters()">
                    <mat-icon>clear</mat-icon>
                    Clear
                  </button>
                </div>
              </form>
            </div>
          }

          <!-- Sample Selection -->
          <div class="selection-section">
            <app-sample-selection
              [testId]="testId"
              [required]="required"
              (sampleSelected)="onSampleSelected($event)">
            </app-sample-selection>
          </div>

          <!-- Sample Information Display -->
          @if (hasSelectedSample()) {
            <div class="info-section">
              <app-sample-info-display
                [showHistory]="showHistory"
                [testId]="testId"
                [testName]="testName">
              </app-sample-info-display>
            </div>
          }

          <!-- Error Display -->
          @if (hasError()) {
            <div class="error-section">
              <mat-card color="warn">
                <mat-card-content>
                  <div class="error-content">
                    <mat-icon color="warn">error</mat-icon>
                    <span>{{ error() }}</span>
                    <button mat-button color="primary" (click)="clearError()">
                      <mat-icon>close</mat-icon>
                      Dismiss
                    </button>
                  </div>
                </mat-card-content>
              </mat-card>
            </div>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .sample-template-container {
      width: 100%;
      max-width: 1200px;
      margin: 0 auto;
    }

    .spacer {
      flex: 1;
    }

    .filter-section {
      margin-bottom: 24px;
      padding: 16px;
      background-color: #f8f9fa;
      border-radius: 8px;
    }

    .filter-form {
      width: 100%;
    }

    .filter-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 16px;
      margin-bottom: 16px;
    }

    .filter-actions {
      display: flex;
      gap: 12px;
      justify-content: flex-end;
    }

    .selection-section {
      margin-bottom: 24px;
    }

    .info-section {
      margin-top: 24px;
    }

    .error-section {
      margin-top: 16px;
    }

    .error-content {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .error-content span {
      flex: 1;
    }

    @media (max-width: 768px) {
      .filter-grid {
        grid-template-columns: 1fr;
      }

      .filter-actions {
        flex-direction: column;
      }

      .filter-actions button {
        width: 100%;
      }
    }
  `]
})
export class SampleDataTemplateComponent implements OnInit {
  @Input() testId: number | null = null;
  @Input() testName: string | null = null;
  @Input() required: boolean = true;
  @Input() showHistory: boolean = true;
  @Input() showFilters: boolean = true;
  @Output() sampleSelected = new EventEmitter<Sample | null>();

  private fb = inject(FormBuilder);
  private sampleService = inject(SampleService);

  // Local signals
  private _showFilters = signal(false);

  readonly showFiltersSignal = this._showFilters.asReadonly();

  // Signals from service
  readonly selectedSample = this.sampleService.selectedSample;
  readonly isLoading = this.sampleService.isLoading;
  readonly error = this.sampleService.error;
  readonly hasSelectedSample = this.sampleService.hasSelectedSample;
  readonly hasError = this.sampleService.hasError;

  // Reactive form for filters
  filterForm: FormGroup;

  constructor() {
    this.filterForm = this.fb.group({
      tagNumber: [''],
      component: [''],
      location: [''],
      lubeType: [''],
      fromDate: [null],
      toDate: [null]
    });

    // Auto-apply filters when form changes (with debounce)
    // This would be implemented with proper debounce in a real application
  }

  ngOnInit(): void {
    // Initialize with filters hidden by default
    this._showFilters.set(this.showFilters);
  }

  onSampleSelected(sample: Sample | null): void {
    this.sampleSelected.emit(sample);
  }

  toggleFilters(): void {
    this._showFilters.update(show => !show);
  }

  applyFilters(): void {
    const filterValues = this.filterForm.value;
    const filter: SampleFilter = {
      tagNumber: filterValues.tagNumber || undefined,
      component: filterValues.component || undefined,
      location: filterValues.location || undefined,
      lubeType: filterValues.lubeType || undefined,
      fromDate: filterValues.fromDate || undefined,
      toDate: filterValues.toDate || undefined
    };

    // Apply filters
    if (this.testId) {
      // If testId is provided, still use test-specific endpoint but could add filters
      this.sampleService.getSamplesByTest(this.testId).subscribe();
    } else {
      this.sampleService.getSamples(filter).subscribe();
    }
  }

  clearFilters(): void {
    this.filterForm.reset();
    this.applyFilters();
  }

  clearError(): void {
    this.sampleService.clearError();
  }

  // Public methods for external control
  refreshSamples(): void {
    this.applyFilters();
  }

  clearSelection(): void {
    this.sampleService.selectSample(null);
  }

  clearAllData(): void {
    this.sampleService.clearData();
    this.filterForm.reset();
  }
}