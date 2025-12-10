import { Component, Inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { SampleService } from '../../services/sample.service';
import { Sample } from '../../models/sample.model';

interface SampleSelectionData {
    testRoute: string;
    testName: string;
    testId: number;
}

@Component({
    selector: 'app-sample-selection-dialog',
    standalone: true,
    imports: [
        CommonModule,
        MatDialogModule,
        MatButtonModule,
        MatListModule,
        MatIconModule,
        MatProgressSpinnerModule
    ],
    template: `
    <h2 mat-dialog-title>Select Sample for {{ data.testName }}</h2>
    
    <mat-dialog-content>
      @if (isLoading()) {
        <div class="loading-container">
          <mat-spinner diameter="40"></mat-spinner>
          <p>Loading samples...</p>
        </div>
      } @else if (error()) {
        <div class="error-container">
          <mat-icon color="warn">error</mat-icon>
          <p>{{ error() }}</p>
          <button mat-button color="primary" (click)="loadSamples()">Retry</button>
        </div>
      } @else {
        <p>Choose a sample to perform the {{ data.testName }} test:</p>
        
        <mat-selection-list #sampleList (selectionChange)="onSampleSelection($event)">
          @for (sample of availableSamples(); track sample.id) {
            <mat-list-option [value]="sample.id">
              <mat-icon matListItemIcon>science</mat-icon>
              <div matListItemTitle>{{ sample.tagNumber }} - {{ sample.component }}</div>
              <div matListItemLine>{{ sample.location }} | {{ sample.lubeType }}</div>
              <div matListItemLine>Sample Date: {{ sample.sampleDate | date:'short' }}</div>
            </mat-list-option>
          }
        </mat-selection-list>
        
        @if (availableSamples().length === 0) {
          <div class="no-samples">
            <mat-icon>info</mat-icon>
            <p>No samples available for testing. Please add samples first.</p>
          </div>
        }
      }
    </mat-dialog-content>
    
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-button (click)="goToSamples()">Manage Samples</button>
      <button 
        mat-raised-button 
        color="primary" 
        [disabled]="!selectedSampleId"
        (click)="onProceed()">
        Proceed to Test
      </button>
    </mat-dialog-actions>
  `,
    styles: [`
    mat-dialog-content {
      min-width: 400px;
      max-height: 400px;
    }
    
    .no-samples {
      text-align: center;
      padding: 24px;
      color: #666;
    }
    
    .no-samples mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 16px;
    }
    
    .loading-container,
    .error-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 40px;
      text-align: center;
    }
    
    .loading-container mat-spinner {
      margin-bottom: 16px;
    }
    
    .error-container mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 16px;
    }
  `]
})
export class SampleSelectionDialogComponent implements OnInit {
    private sampleService = SampleService;
    
    selectedSampleId: number | null = null;
    
    // Signals for state management
    readonly isLoading = signal(false);
    readonly error = signal<string | null>(null);
    readonly availableSamples = signal<Sample[]>([]);

    constructor(
        private dialogRef: MatDialogRef<SampleSelectionDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: SampleSelectionData,
        private router: Router,
        sampleService: SampleService
    ) {
        this.sampleService = sampleService;
    }
    
    ngOnInit(): void {
        this.loadSamples();
    }
    
    private loadSamples(): void {
        if (!this.data.testId) {
            this.error.set('Test ID not provided');
            return;
        }
        
        this.isLoading.set(true);
        this.error.set(null);
        
        this.sampleService.getSamplesByTest(this.data.testId).subscribe({
            next: (samples) => {
                this.availableSamples.set(samples);
                this.isLoading.set(false);
            },
            error: (err) => {
                console.error('Error loading samples:', err);
                this.error.set('Failed to load samples. Please try again.');
                this.isLoading.set(false);
            }
        });
    }

    onSampleSelection(event: any) {
        this.selectedSampleId = Number(event.option.value);
    }

    onCancel() {
        this.dialogRef.close();
    }

    goToSamples() {
        this.dialogRef.close();
        this.router.navigate(['/samples']);
    }

    onProceed() {
        if (this.selectedSampleId) {
            this.dialogRef.close();
            this.router.navigate(['/tests', this.data.testRoute, this.selectedSampleId]);
        }
    }
}