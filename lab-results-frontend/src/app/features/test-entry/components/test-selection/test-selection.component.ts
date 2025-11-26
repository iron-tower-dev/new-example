import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../../shared/services/auth.service';
import { TestWorkflowService, TestType, SampleForTest } from '../../../../shared/services/test-workflow.service';

@Component({
  selector: 'app-test-selection',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatSnackBarModule
  ],
  template: `
    <div class="test-selection-container">
      <!-- Step 1: Test Selection -->
      @if (!workflowService.selectedTest()) {
        <mat-card class="selection-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>assignment</mat-icon>
              Select Test Type
            </mat-card-title>
            <mat-card-subtitle>Choose a test you are qualified to perform</mat-card-subtitle>
          </mat-card-header>
          
          <mat-card-content>
            <div class="test-grid">
              @for (test of availableTests; track test.id) {
                <div class="test-item" 
                     [class.disabled]="!canAccessTest(test)"
                     (click)="selectTest(test)">
                  <div class="test-header">
                    <h3>{{ test.name }}</h3>
                    @if (test.requiredQualification) {
                      <mat-icon class="qualification-icon" 
                               [class.qualified]="canAccessTest(test)"
                               [class.restricted]="!canAccessTest(test)">
                        {{ canAccessTest(test) ? 'verified' : 'lock' }}
                      </mat-icon>
                    }
                  </div>
                  <p>{{ test.description }}</p>
                  @if (test.requiredQualification) {
                    <mat-chip class="qualification-chip" 
                             [class.qualified]="canAccessTest(test)"
                             [class.restricted]="!canAccessTest(test)">
                      {{ test.requiredQualification }}
                    </mat-chip>
                  }
                </div>
              }
            </div>
          </mat-card-content>
        </mat-card>
      }

      <!-- Step 2: Sample Selection -->
      @if (workflowService.selectedTest() && !workflowService.selectedSample()) {
        <mat-card class="selection-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>science</mat-icon>
              Select Sample for {{ workflowService.selectedTest()?.name }}
            </mat-card-title>
            <mat-card-subtitle>Choose a sample to perform the test on</mat-card-subtitle>
          </mat-card-header>
          
          <mat-card-content>
            @if (workflowService.isLoading()) {
              <div class="loading-container">
                <mat-spinner diameter="40"></mat-spinner>
                <p>Loading available samples...</p>
              </div>
            } @else {
              @if (workflowService.availableSamples().length > 0) {
                <mat-selection-list (selectionChange)="onSampleSelection($event)">
                  @for (sample of workflowService.availableSamples(); track sample.id) {
                    <mat-list-option [value]="sample" class="sample-option">
                      <div matListItemTitle class="sample-title">
                        <span class="sample-id">{{ sample.id }}</span>
                        <span class="tag-number">{{ sample.tagNumber }}</span>
                        @if (sample.hasPartialData) {
                          <mat-chip class="partial-data-chip">Partial Data</mat-chip>
                        }
                      </div>
                      <div matListItemLine class="sample-details">
                        <span><strong>Customer:</strong> {{ sample.customerName }}</span>
                        <span><strong>Component:</strong> {{ sample.component }}</span>
                      </div>
                      <div matListItemLine class="sample-details">
                        <span><strong>Location:</strong> {{ sample.location }}</span>
                        <span><strong>Sample Date:</strong> {{ sample.sampleDate | date:'short' }}</span>
                      </div>
                      <div matListItemLine class="sample-status">
                        <span><strong>Status:</strong> {{ sample.status }}</span>
                        <span><strong>Lube Type:</strong> {{ sample.lubeType }}</span>
                      </div>
                    </mat-list-option>
                  }
                </mat-selection-list>
              } @else {
                <div class="no-samples">
                  <mat-icon>info</mat-icon>
                  <h3>No Samples Available</h3>
                  <p>There are no samples available for {{ workflowService.selectedTest()?.name }} testing at this time.</p>
                </div>
              }
            }
          </mat-card-content>
          
          <mat-card-actions>
            <button mat-button (click)="goBack()">
              <mat-icon>arrow_back</mat-icon>
              Back to Test Selection
            </button>
          </mat-card-actions>
        </mat-card>
      }

      <!-- Step 3: Confirmation and Navigation -->
      @if (workflowService.selectedTest() && workflowService.selectedSample()) {
        <mat-card class="selection-card confirmation-card">
          <mat-card-header>
            <mat-card-title>
              <mat-icon>check_circle</mat-icon>
              Ready to Begin Test
            </mat-card-title>
          </mat-card-header>
          
          <mat-card-content>
            <div class="confirmation-details">
              <div class="detail-section">
                <h3>Selected Test:</h3>
                <p><strong>{{ workflowService.selectedTest()?.name }}</strong></p>
                <p>{{ workflowService.selectedTest()?.description }}</p>
              </div>
              
              <div class="detail-section">
                <h3>Selected Sample:</h3>
                <div class="sample-summary">
                  <p><strong>Sample ID:</strong> {{ workflowService.selectedSample()?.id }}</p>
                  <p><strong>Tag Number:</strong> {{ workflowService.selectedSample()?.tagNumber }}</p>
                  <p><strong>Customer:</strong> {{ workflowService.selectedSample()?.customerName }}</p>
                  <p><strong>Component:</strong> {{ workflowService.selectedSample()?.component }}</p>
                  @if (workflowService.selectedSample()?.hasPartialData) {
                    <div class="partial-data-warning">
                      <mat-icon>warning</mat-icon>
                      <span>This sample has partially saved test data that will be loaded.</span>
                    </div>
                  }
                </div>
              </div>
            </div>
          </mat-card-content>
          
          <mat-card-actions>
            <button mat-button (click)="goBackToSamples()">
              <mat-icon>arrow_back</mat-icon>
              Change Sample
            </button>
            <button mat-raised-button color="primary" (click)="proceedToTest()">
              <mat-icon>play_arrow</mat-icon>
              Begin Test Entry
            </button>
          </mat-card-actions>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .test-selection-container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .selection-card {
      margin-bottom: 24px;
    }

    .test-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 16px;
      margin-top: 16px;
    }

    .test-item {
      padding: 16px;
      border: 2px solid #e0e0e0;
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.2s ease;
    }

    .test-item:hover:not(.disabled) {
      border-color: #1976d2;
      background-color: #f5f5f5;
    }

    .test-item.disabled {
      opacity: 0.5;
      cursor: not-allowed;
      background-color: #fafafa;
    }

    .test-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;
    }

    .test-header h3 {
      margin: 0;
      color: #333;
    }

    .qualification-icon.qualified {
      color: #4caf50;
    }

    .qualification-icon.restricted {
      color: #f44336;
    }

    .qualification-chip {
      font-size: 0.75rem;
      height: 20px;
    }

    .qualification-chip.qualified {
      background-color: #e8f5e8;
      color: #2e7d32;
    }

    .qualification-chip.restricted {
      background-color: #ffebee;
      color: #c62828;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 40px;
    }

    .loading-container p {
      margin-top: 16px;
      color: #666;
    }

    .sample-option {
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      margin-bottom: 8px;
      padding: 8px;
    }

    .sample-title {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .sample-id {
      font-weight: bold;
      color: #1976d2;
    }

    .tag-number {
      font-weight: 500;
    }

    .partial-data-chip {
      background-color: #fff3e0;
      color: #f57c00;
      font-size: 0.75rem;
      height: 20px;
    }

    .sample-details {
      display: flex;
      justify-content: space-between;
      margin: 4px 0;
      font-size: 0.9rem;
    }

    .sample-status {
      display: flex;
      justify-content: space-between;
      margin: 4px 0;
      font-size: 0.9rem;
      color: #666;
    }

    .no-samples {
      text-align: center;
      padding: 40px;
      color: #666;
    }

    .no-samples mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 16px;
    }

    .confirmation-card {
      background-color: #f8f9fa;
      border: 2px solid #4caf50;
    }

    .confirmation-details {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 24px;
      margin: 16px 0;
    }

    .detail-section h3 {
      color: #1976d2;
      margin-bottom: 8px;
    }

    .sample-summary p {
      margin: 4px 0;
    }

    .partial-data-warning {
      display: flex;
      align-items: center;
      gap: 8px;
      background-color: #fff3e0;
      padding: 8px;
      border-radius: 4px;
      margin-top: 8px;
      color: #f57c00;
    }

    @media (max-width: 768px) {
      .test-grid {
        grid-template-columns: 1fr;
      }
      
      .confirmation-details {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class TestSelectionComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  readonly workflowService = inject(TestWorkflowService);

  availableTests: TestType[] = [];

  ngOnInit() {
    this.workflowService.getAvailableTests().subscribe({
      next: (tests) => {
        this.availableTests = tests;
      },
      error: (error) => {
        console.error('Error loading available tests:', error);
        this.snackBar.open('Failed to load available tests', 'Close', { duration: 3000 });
      }
    });
    this.workflowService.clearSelection(); // Start fresh
  }

  canAccessTest(test: TestType): boolean {
    if (!test.testStandId || !test.requiredQualification) {
      return true; // No restrictions
    }

    return this.authService.hasQualification(test.testStandId, test.requiredQualification);
  }

  selectTest(test: TestType): void {
    if (!this.canAccessTest(test)) {
      this.snackBar.open(
        `You need ${test.requiredQualification} qualification for ${test.name}`,
        'Close',
        { duration: 3000 }
      );
      return;
    }

    this.workflowService.selectTest(test);
  }

  onSampleSelection(event: any): void {
    // Handle mat-selection-list selectionChange event
    if (event.options && event.options.length > 0) {
      const selectedOption = event.options[0];
      if (selectedOption && selectedOption.value) {
        const selectedSample = selectedOption.value as SampleForTest;
        this.workflowService.selectSample(selectedSample);
      }
    }
  }

  goBack(): void {
    this.workflowService.clearSelection();
  }

  goBackToSamples(): void {
    this.workflowService.selectSample(null as any);
  }

  proceedToTest(): void {
    const test = this.workflowService.selectedTest();
    const sample = this.workflowService.selectedSample();

    if (test && sample) {
      const testRoute = this.workflowService.getTestRoute(test.name);
      this.router.navigate(['/tests', testRoute, sample.id]);
    }
  }
}