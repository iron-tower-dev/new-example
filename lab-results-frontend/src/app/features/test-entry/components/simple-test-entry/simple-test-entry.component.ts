import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';

interface Sample {
    id: string;
    tagNumber: string;
    customerName: string;
    component: string;
    sampleDate: Date;
}

@Component({
    selector: 'app-simple-test-entry',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ReactiveFormsModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatButtonModule,
        MatIconModule,
        MatSnackBarModule
    ],
    template: `
    <div class="test-entry-container">
      <mat-card>
        <mat-card-header>
          <mat-card-title>
            <mat-icon>assignment</mat-icon>
            {{ getTestTitle() }} Test Entry
          </mat-card-title>
        </mat-card-header>
        
        <mat-card-content>
          <form [formGroup]="testForm" (ngSubmit)="onSubmit()">
            <!-- Sample Selection -->
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Select Sample</mat-label>
              <mat-select formControlName="sampleId" required>
                <mat-option value="">-- Select a Sample --</mat-option>
                @for (sample of availableSamples; track sample.id) {
                  <mat-option [value]="sample.id">
                    {{ sample.id }} - {{ sample.tagNumber }} ({{ sample.customerName }})
                  </mat-option>
                }
              </mat-select>
            </mat-form-field>

            <!-- Sample Info Display -->
            @if (selectedSample) {
              <div class="sample-info">
                <h3>Sample Information</h3>
                <p><strong>Sample ID:</strong> {{ selectedSample.id }}</p>
                <p><strong>Tag Number:</strong> {{ selectedSample.tagNumber }}</p>
                <p><strong>Customer:</strong> {{ selectedSample.customerName }}</p>
                <p><strong>Component:</strong> {{ selectedSample.component }}</p>
                <p><strong>Sample Date:</strong> {{ selectedSample.sampleDate | date:'short' }}</p>
              </div>
            }

            <!-- Test-Specific Fields -->
            @if (selectedSample) {
              <div class="test-fields">
                <h3>Test Data Entry</h3>
                
                @switch (testType) {
                  @case ('tan') {
                    <div class="field-row">
                      <mat-form-field appearance="outline">
                        <mat-label>Sample Weight (g)</mat-label>
                        <input matInput type="number" formControlName="sampleWeight" step="0.01">
                      </mat-form-field>
                      
                      <mat-form-field appearance="outline">
                        <mat-label>Final Buret (mL)</mat-label>
                        <input matInput type="number" formControlName="finalBuret" step="0.01">
                      </mat-form-field>
                    </div>
                    
                    @if (calculatedResult) {
                      <div class="result-display">
                        <strong>TAN Result: {{ calculatedResult }} mg KOH/g</strong>
                      </div>
                    }
                  }
                  
                  @case ('water-kf') {
                    <div class="field-row">
                      <mat-form-field appearance="outline">
                        <mat-label>Water Content (%)</mat-label>
                        <input matInput type="number" formControlName="waterContent" step="0.001">
                      </mat-form-field>
                    </div>
                  }
                  
                  @case ('tbn') {
                    <div class="field-row">
                      <mat-form-field appearance="outline">
                        <mat-label>TBN Result (mg KOH/g)</mat-label>
                        <input matInput type="number" formControlName="tbnResult" step="0.01">
                      </mat-form-field>
                    </div>
                  }
                  
                  @default {
                    <div class="field-row">
                      <mat-form-field appearance="outline">
                        <mat-label>Test Result</mat-label>
                        <input matInput type="number" formControlName="testResult" step="0.01">
                      </mat-form-field>
                    </div>
                  }
                }
                
                <!-- Comments -->
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Comments (Optional)</mat-label>
                  <textarea matInput formControlName="comments" rows="3"></textarea>
                </mat-form-field>
              </div>
            }
          </form>
        </mat-card-content>
        
        <mat-card-actions>
          <button mat-button type="button" (click)="goBack()">
            <mat-icon>arrow_back</mat-icon>
            Back
          </button>
          
          @if (selectedSample) {
            <button mat-button type="button" (click)="clearForm()">
              <mat-icon>clear</mat-icon>
              Clear
            </button>
            
            <button mat-raised-button color="primary" 
                    [disabled]="testForm.invalid" 
                    (click)="onSubmit()">
              <mat-icon>save</mat-icon>
              Save Results
            </button>
          }
        </mat-card-actions>
      </mat-card>
    </div>
  `,
    styles: [`
    .test-entry-container {
      padding: 24px;
      max-width: 800px;
      margin: 0 auto;
    }

    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    .sample-info {
      background-color: #f5f5f5;
      padding: 16px;
      border-radius: 8px;
      margin: 16px 0;
    }

    .sample-info h3 {
      margin-top: 0;
      color: #1976d2;
    }

    .sample-info p {
      margin: 4px 0;
    }

    .test-fields {
      margin-top: 24px;
    }

    .test-fields h3 {
      color: #1976d2;
      margin-bottom: 16px;
    }

    .field-row {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
    }

    .field-row mat-form-field {
      flex: 1;
    }

    .result-display {
      background-color: #e8f5e8;
      padding: 12px;
      border-radius: 4px;
      margin: 16px 0;
      text-align: center;
      color: #2e7d32;
    }

    @media (max-width: 600px) {
      .field-row {
        flex-direction: column;
        gap: 0;
      }
    }
  `]
})
export class SimpleTestEntryComponent implements OnInit {
    private fb = inject(FormBuilder);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private snackBar = inject(MatSnackBar);

    testType: string = '';
    testForm: FormGroup;
    selectedSample: Sample | null = null;
    calculatedResult: number | null = null;

    // Mock sample data
    availableSamples: Sample[] = [
        {
            id: 'S001',
            tagNumber: 'ENG-001',
            customerName: 'Acme Corp',
            component: 'Engine Oil',
            sampleDate: new Date('2024-11-10')
        },
        {
            id: 'S002',
            tagNumber: 'HYD-002',
            customerName: 'Beta Industries',
            component: 'Hydraulic Fluid',
            sampleDate: new Date('2024-11-11')
        },
        {
            id: 'S003',
            tagNumber: 'GR-003',
            customerName: 'Gamma LLC',
            component: 'Gear Oil',
            sampleDate: new Date('2024-11-12')
        }
    ];

    constructor() {
        this.testForm = this.fb.group({
            sampleId: ['', Validators.required],
            sampleWeight: [''],
            finalBuret: [''],
            waterContent: [''],
            tbnResult: [''],
            testResult: [''],
            comments: ['']
        });

        // Watch for sample selection changes
        this.testForm.get('sampleId')?.valueChanges.subscribe(sampleId => {
            this.selectedSample = this.availableSamples.find(s => s.id === sampleId) || null;
        });

        // Watch for TAN calculation
        this.testForm.get('sampleWeight')?.valueChanges.subscribe(() => this.calculateTAN());
        this.testForm.get('finalBuret')?.valueChanges.subscribe(() => this.calculateTAN());
    }

    ngOnInit() {
        this.route.url.subscribe(segments => {
            if (segments.length > 0) {
                this.testType = segments[0].path;
            }
        });
    }

    getTestTitle(): string {
        const titles: { [key: string]: string } = {
            'tan': 'TAN (Total Acid Number)',
            'water-kf': 'Water-KF (Karl Fischer)',
            'tbn': 'TBN (Total Base Number)',
            'viscosity-40c': 'Viscosity @ 40°C',
            'viscosity-100c': 'Viscosity @ 100°C',
            'flash-point': 'Flash Point'
        };
        return titles[this.testType] || 'Test';
    }

    calculateTAN(): void {
        if (this.testType === 'tan') {
            const sampleWeight = this.testForm.get('sampleWeight')?.value;
            const finalBuret = this.testForm.get('finalBuret')?.value;

            if (sampleWeight && finalBuret && sampleWeight > 0) {
                this.calculatedResult = Math.round(((finalBuret * 5.61) / sampleWeight) * 100) / 100;
            } else {
                this.calculatedResult = null;
            }
        }
    }

    onSubmit(): void {
        if (this.testForm.valid && this.selectedSample) {
            const formData = {
                ...this.testForm.value,
                testType: this.testType,
                calculatedResult: this.calculatedResult
            };

            console.log('Saving test data:', formData);

            this.snackBar.open('Test results saved successfully!', 'Close', {
                duration: 3000
            });

            // In real implementation, this would call an API to save the data
        }
    }

    clearForm(): void {
        this.testForm.reset();
        this.selectedSample = null;
        this.calculatedResult = null;
    }

    goBack(): void {
        this.router.navigate(['/tests']);
    }
}