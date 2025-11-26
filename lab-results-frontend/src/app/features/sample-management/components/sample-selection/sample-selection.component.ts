import { Component, Input, Output, EventEmitter, OnInit, inject, signal, computed, effect } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { SampleService } from '../../../../shared/services/sample.service';
import { Sample } from '../../../../shared/models/sample.model';
import { SharedModule } from '../../../../shared/shared.module';

@Component({
  selector: 'app-sample-selection',
  standalone: true,
  imports: [SharedModule],
  template: `
    <div class="sample-selection-container">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Select Sample</mat-label>
        <mat-select 
          [formControl]="sampleControl"
          (selectionChange)="onSampleChange($event.value)">
          
          @if (isLoading()) {
            <mat-option disabled>
              <mat-spinner diameter="20"></mat-spinner>
              Loading samples...
            </mat-option>
          } @else {
            @for (sample of samples(); track sample.id) {
              <mat-option [value]="sample">
                {{ sample.tagNumber }} - {{ sample.component }}
                <span class="sample-date">({{ sample.sampleDate | date:'shortDate' }})</span>
              </mat-option>
            } @empty {
              <mat-option disabled>No samples available</mat-option>
            }
          }
        </mat-select>
        
        @if (hasError()) {
          <mat-error>{{ error() }}</mat-error>
        }
      </mat-form-field>

      @if (selectedSample(); as sample) {
        <div class="selected-sample-info">
          <mat-card>
            <mat-card-header>
              <mat-card-title>{{ sample.tagNumber }}</mat-card-title>
              <mat-card-subtitle>{{ sample.component }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <div class="sample-details">
                <div class="detail-row">
                  <span class="label">Location:</span>
                  <span class="value">{{ sample.location }}</span>
                </div>
                <div class="detail-row">
                  <span class="label">Lube Type:</span>
                  <span class="value">{{ sample.lubeType }}</span>
                </div>
                @if (sample.qualityClass) {
                  <div class="detail-row">
                    <span class="label">Quality Class:</span>
                    <span class="value">{{ sample.qualityClass }}</span>
                  </div>
                }
                <div class="detail-row">
                  <span class="label">Sample Date:</span>
                  <span class="value">{{ sample.sampleDate | date:'medium' }}</span>
                </div>
                <div class="detail-row">
                  <span class="label">Status:</span>
                  <span class="value status" [class]="'status-' + sample.status.toLowerCase()">
                    {{ sample.status }}
                  </span>
                </div>
              </div>
            </mat-card-content>
          </mat-card>
        </div>
      }
    </div>
  `,
  styles: [`
    .sample-selection-container {
      width: 100%;
      max-width: 600px;
    }

    .full-width {
      width: 100%;
    }

    .sample-date {
      color: #666;
      font-size: 0.9em;
      margin-left: 8px;
    }

    .selected-sample-info {
      margin-top: 16px;
    }

    .sample-details {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .detail-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 4px 0;
      border-bottom: 1px solid #f0f0f0;
    }

    .detail-row:last-child {
      border-bottom: none;
    }

    .label {
      font-weight: 500;
      color: #666;
      min-width: 120px;
    }

    .value {
      flex: 1;
      text-align: right;
    }

    .status {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 0.9em;
      font-weight: 500;
    }

    .status-available {
      background-color: #e8f5e8;
      color: #2e7d32;
    }

    .status-pending {
      background-color: #fff3e0;
      color: #f57c00;
    }

    .status-complete {
      background-color: #e3f2fd;
      color: #1976d2;
    }

    .status-cancelled {
      background-color: #ffebee;
      color: #d32f2f;
    }

    mat-spinner {
      margin-right: 8px;
    }
  `]
})
export class SampleSelectionComponent implements OnInit {
  @Input() testId: number | null = null;
  @Input() required: boolean = true;
  @Output() sampleSelected = new EventEmitter<Sample | null>();

  private sampleService = inject(SampleService);

  // Form control for the dropdown
  sampleControl = new FormControl<Sample | null>(null);

  // Signals from service
  readonly samples = this.sampleService.samples;
  readonly selectedSample = this.sampleService.selectedSample;
  readonly isLoading = this.sampleService.isLoading;
  readonly error = this.sampleService.error;
  readonly hasError = this.sampleService.hasError;

  constructor() {
    // Set up validators based on required input
    effect(() => {
      if (this.required) {
        this.sampleControl.setValidators([Validators.required]);
      } else {
        this.sampleControl.clearValidators();
      }
      this.sampleControl.updateValueAndValidity();
    });

    // Sync form control with service state
    effect(() => {
      const selected = this.selectedSample();
      if (selected !== this.sampleControl.value) {
        this.sampleControl.setValue(selected, { emitEvent: false });
      }
    });

    // Manage disabled state through FormControl
    effect(() => {
      const shouldDisable = this.isLoading();
      if (shouldDisable && this.sampleControl.enabled) {
        this.sampleControl.disable();
      } else if (!shouldDisable && this.sampleControl.disabled) {
        this.sampleControl.enable();
      }
    });
  }

  ngOnInit(): void {
    this.loadSamples();
  }

  onSampleChange(sample: Sample | null): void {
    this.sampleService.selectSample(sample);
    this.sampleSelected.emit(sample);
  }

  private loadSamples(): void {
    if (this.testId) {
      this.sampleService.getSamplesByTest(this.testId).subscribe();
    } else {
      this.sampleService.getSamples().subscribe();
    }
  }

  // Public method to refresh samples
  refreshSamples(): void {
    this.loadSamples();
  }

  // Public method to clear selection
  clearSelection(): void {
    this.sampleService.selectSample(null);
    this.sampleControl.setValue(null);
  }
}