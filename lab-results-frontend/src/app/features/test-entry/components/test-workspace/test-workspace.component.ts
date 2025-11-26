import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { SampleService } from '../../../../shared/services/sample.service';
import { TestService } from '../../../../shared/services/test.service';
import { Sample } from '../../../../shared/models/sample.model';

interface TestInfo {
  id: number;
  name: string;
  route: string;
}



interface HistoricalResult {
  date: Date;
  result: string;
  technician: string;
  status: string;
}

@Component({
  selector: 'app-test-workspace',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatSidenavModule,
    MatCardModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatDividerModule,
    MatSnackBarModule
  ],
  template: `
    <div class="workspace-container">
      <mat-sidenav-container class="sidenav-container">
        <!-- Sidebar -->
        <mat-sidenav mode="side" opened class="sidebar">
          <div class="sidebar-content">
            <!-- Test Selector -->
            <div class="test-selector">
              <h3>Selected Test</h3>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Test Type</mat-label>
                <mat-select [value]="currentTestRoute()" (selectionChange)="onTestChange($event.value)">
                  @for (test of availableTests; track test.id) {
                    <mat-option [value]="test.route">{{ test.name }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>
            </div>

            <mat-divider></mat-divider>

            <!-- Sample List -->
            <div class="sample-list">
              <h3>Samples for {{ getCurrentTestName() }}</h3>
              <div class="sample-count">{{ availableSamples().length }} samples available</div>
              
              <div class="sample-buttons">
                @for (sample of availableSamples(); track sample.id) {
                  <button 
                    class="sample-button" 
                    [class.selected]="selectedSample()?.id === sample.id"
                    (click)="onSampleClick(sample)">
                    {{ sample.id }}
                  </button>
                }
              </div>

              @if (availableSamples().length === 0) {
                <div class="no-samples">
                  <mat-icon>info</mat-icon>
                  <p>No samples available for this test</p>
                </div>
              }
            </div>
          </div>
        </mat-sidenav>

        <!-- Main Content -->
        <mat-sidenav-content class="main-content">
          @if (selectedSample()) {
            <div class="content-layout">
              <!-- Test Entry Form -->
              <div class="form-section">
                <mat-card>
                  <mat-card-header>
                    <mat-card-title>{{ getCurrentTestName() }} - {{ selectedSample()?.id }}</mat-card-title>
                    <mat-card-subtitle>{{ selectedSample()?.component }} • {{ selectedSample()?.tagNumber }}</mat-card-subtitle>
                  </mat-card-header>
                  
                  <mat-card-content>
                    <form [formGroup]="testForm" (ngSubmit)="onSubmit()">
                      @switch (currentTestRoute()) {
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
                          
                          @if (calculatedResult()) {
                            <div class="result-display">
                              <strong>TAN Result: {{ calculatedResult() }} mg KOH/g</strong>
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
                        
                        @case ('viscosity-40c') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Viscosity @ 40°C (cSt)</mat-label>
                              <input matInput type="number" formControlName="viscosity40" step="0.01">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('viscosity-100c') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Viscosity @ 100°C (cSt)</mat-label>
                              <input matInput type="number" formControlName="viscosity100" step="0.01">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('flash-point') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Flash Point (°C)</mat-label>
                              <input matInput type="number" formControlName="flashPoint" step="1">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('particle-count') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Particle Count (ISO 4406)</mat-label>
                              <input matInput type="text" formControlName="particleCount" placeholder="e.g., 18/16/13">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('grease-dropping-point') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Dropping Point (°C)</mat-label>
                              <input matInput type="number" formControlName="droppingPoint" step="1">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('grease-penetration') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Penetration (0.1 mm)</mat-label>
                              <input matInput type="number" formControlName="penetration" step="1">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('emission-spectroscopy') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Iron (Fe) ppm</mat-label>
                              <input matInput type="number" formControlName="ironPpm" step="0.1">
                            </mat-form-field>
                            <mat-form-field appearance="outline">
                              <mat-label>Copper (Cu) ppm</mat-label>
                              <input matInput type="number" formControlName="copperPpm" step="0.1">
                            </mat-form-field>
                          </div>
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Aluminum (Al) ppm</mat-label>
                              <input matInput type="number" formControlName="aluminumPpm" step="0.1">
                            </mat-form-field>
                            <mat-form-field appearance="outline">
                              <mat-label>Chromium (Cr) ppm</mat-label>
                              <input matInput type="number" formControlName="chromiumPpm" step="0.1">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('ft-ir') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>FT-IR Result</mat-label>
                              <input matInput type="text" formControlName="ftirResult">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('inspect-filter') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Filter Condition</mat-label>
                              <input matInput type="text" formControlName="filterCondition">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('ferrography') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Wear Particles</mat-label>
                              <input matInput type="text" formControlName="wearParticles">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('rbot') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>RBOT Result (minutes)</mat-label>
                              <input matInput type="number" formControlName="rbotResult" step="1">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('tfout') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>TFOUT Result (minutes)</mat-label>
                              <input matInput type="number" formControlName="tfoutResult" step="1">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('rust') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Rust Rating</mat-label>
                              <input matInput type="text" formControlName="rustRating">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('deleterious') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Deleterious Materials</mat-label>
                              <input matInput type="text" formControlName="deleteriousMaterials">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('d-inch') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>D-inch Result</mat-label>
                              <input matInput type="number" formControlName="dinchResult" step="0.1">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('oil-content') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Oil Content (%)</mat-label>
                              <input matInput type="number" formControlName="oilContent" step="0.1">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('varnish-potential-rating') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>VPR Result</mat-label>
                              <input matInput type="number" formControlName="vprResult" step="0.1">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('filter-residue') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Filter Residue (%)</mat-label>
                              <input matInput type="number" formControlName="filterResidue" step="0.01">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('rheometer') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Rheometer Result</mat-label>
                              <input matInput type="number" formControlName="rheometerResult" step="0.1">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('misc-tests') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Test Result</mat-label>
                              <input matInput type="text" formControlName="miscResult">
                            </mat-form-field>
                          </div>
                        }
                        
                        @case ('debris-identification') {
                          <div class="field-row">
                            <mat-form-field appearance="outline">
                              <mat-label>Debris Type</mat-label>
                              <input matInput type="text" formControlName="debrisType">
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
                    </form>
                  </mat-card-content>
                  
                  <mat-card-actions>
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
                  </mat-card-actions>
                </mat-card>
              </div>

              <!-- Historical Results -->
              <div class="history-section">
                <mat-card>
                  <mat-card-header>
                    <mat-card-title>Historical Results</mat-card-title>
                    <mat-card-subtitle>Last 10 results for {{ selectedSample()?.id }}</mat-card-subtitle>
                  </mat-card-header>
                  
                  <mat-card-content>
                    @if (historicalResults().length > 0) {
                      <div class="history-list">
                        @for (result of historicalResults(); track $index) {
                          <div class="history-item">
                            <div class="history-date">{{ result.date | date:'short' }}</div>
                            <div class="history-result">{{ result.result }}</div>
                            <div class="history-tech">{{ result.technician }}</div>
                            <div class="history-status">{{ result.status }}</div>
                          </div>
                        }
                      </div>
                    } @else {
                      <div class="no-history">
                        <mat-icon>history</mat-icon>
                        <p>No historical results available</p>
                      </div>
                    }
                  </mat-card-content>
                </mat-card>
              </div>
            </div>
          } @else {
            <div class="no-selection">
              <mat-icon>arrow_back</mat-icon>
              <h3>Select a Sample</h3>
              <p>Choose a sample from the sidebar to begin test entry</p>
            </div>
          }
        </mat-sidenav-content>
      </mat-sidenav-container>
    </div>
  `,
  styles: [`
    .workspace-container {
      height: 100vh;
      display: flex;
      flex-direction: column;
    }

    .sidenav-container {
      flex: 1;
    }

    .sidebar {
      width: 350px;
      border-right: 1px solid #e0e0e0;
    }

    .sidebar-content {
      padding: 16px;
      height: 100%;
      overflow-y: auto;
    }

    .test-selector {
      margin-bottom: 16px;
    }

    .test-selector h3 {
      margin: 0 0 12px 0;
      color: #1976d2;
    }

    .sample-list {
      margin-top: 16px;
    }

    .sample-list h3 {
      margin: 0 0 8px 0;
      color: #1976d2;
    }

    .sample-count {
      font-size: 0.875rem;
      color: #666;
      margin-bottom: 12px;
    }

    .sample-buttons {
      display: flex;
      flex-direction: column;
      gap: 8px;
      margin-top: 16px;
    }

    .sample-button {
      background: #f5f5f5;
      border: 1px solid #ddd;
      border-radius: 6px;
      padding: 12px 16px;
      font-size: 1.1em;
      font-weight: 500;
      color: #333;
      cursor: pointer;
      transition: all 0.2s ease;
      text-align: left;
      width: 100%;
    }

    .sample-button:hover {
      background: #e3f2fd;
      border-color: #1976d2;
      color: #1976d2;
    }

    .sample-button.selected {
      background: #1976d2;
      border-color: #1976d2;
      color: white;
    }

    .sample-button.selected:hover {
      background: #1565c0;
      border-color: #1565c0;
    }



    .no-samples, .no-history {
      text-align: center;
      padding: 24px;
      color: #666;
    }

    .no-samples mat-icon, .no-history mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
      margin-bottom: 8px;
    }

    .main-content {
      background-color: #fafafa;
    }

    .content-layout {
      display: grid;
      grid-template-columns: 1fr 400px;
      gap: 24px;
      padding: 24px;
      height: 100%;
    }

    .form-section {
      min-height: fit-content;
    }

    .history-section {
      max-height: calc(100vh - 48px);
      overflow-y: auto;
    }

    .field-row {
      display: flex;
      gap: 16px;
      margin-bottom: 16px;
    }

    .field-row mat-form-field {
      flex: 1;
    }

    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    .result-display {
      background-color: #e8f5e8;
      padding: 12px;
      border-radius: 4px;
      margin: 16px 0;
      text-align: center;
      color: #2e7d32;
    }

    .history-list {
      max-height: 400px;
      overflow-y: auto;
    }

    .history-item {
      display: grid;
      grid-template-columns: 1fr 1fr 1fr 80px;
      gap: 8px;
      padding: 8px 0;
      border-bottom: 1px solid #e0e0e0;
      font-size: 0.875rem;
    }

    .history-date {
      font-weight: 500;
    }

    .history-result {
      color: #1976d2;
      font-weight: 500;
    }

    .history-tech {
      color: #666;
    }

    .history-status {
      font-size: 0.75rem;
      color: #666;
    }

    .no-selection {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      color: #666;
    }

    .no-selection mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
    }

    @media (max-width: 1200px) {
      .content-layout {
        grid-template-columns: 1fr;
        grid-template-rows: auto 1fr;
      }
      
      .history-section {
        max-height: 300px;
      }
    }

    @media (max-width: 768px) {
      .sidebar {
        width: 280px;
      }
      
      .field-row {
        flex-direction: column;
        gap: 0;
      }
    }
  `]
})
export class TestWorkspaceComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private snackBar = inject(MatSnackBar);
  private sampleService = inject(SampleService);
  private testService = inject(TestService);

  // Signals
  currentTestRoute = signal<string>('');
  selectedSample = signal<Sample | null>(null);
  availableSamples = signal<Sample[]>([]);
  historicalResults = signal<HistoricalResult[]>([]);
  calculatedResult = signal<number | null>(null);

  testForm: FormGroup;

  availableTests: TestInfo[] = [];

  constructor() {
    this.testForm = this.fb.group({
      sampleWeight: [''],
      finalBuret: [''],
      waterContent: [''],
      tbnResult: [''],
      viscosity40: [''],
      viscosity100: [''],
      flashPoint: [''],
      particleCount: [''],
      droppingPoint: [''],
      penetration: [''],
      ironPpm: [''],
      copperPpm: [''],
      aluminumPpm: [''],
      chromiumPpm: [''],
      ftirResult: [''],
      filterCondition: [''],
      wearParticles: [''],
      rbotResult: [''],
      tfoutResult: [''],
      rustRating: [''],
      deleteriousMaterials: [''],
      dinchResult: [''],
      oilContent: [''],
      vprResult: [''],
      filterResidue: [''],
      rheometerResult: [''],
      miscResult: [''],
      debrisType: [''],
      testResult: [''],
      comments: ['']
    });

    // Watch for TAN calculation
    this.testForm.get('sampleWeight')?.valueChanges.subscribe(() => this.calculateTAN());
    this.testForm.get('finalBuret')?.valueChanges.subscribe(() => this.calculateTAN());
  }

  ngOnInit() {
    // Load available tests first
    this.loadAvailableTests();

    this.route.params.subscribe(params => {
      const testRoute = params['testType'];
      const sampleId = params['sampleId'];

      if (testRoute) {
        this.currentTestRoute.set(testRoute);
        this.loadSamplesForTest(testRoute);

        // If sampleId is provided, auto-select the sample after samples are loaded
        if (sampleId) {
          // Wait a bit for samples to load, then select the sample
          setTimeout(() => {
            this.autoSelectSample(+sampleId);
          }, 500);
        }
      }
    });
  }

  private autoSelectSample(sampleId: number) {
    const samples = this.availableSamples();
    const sample = samples.find(s => s.id === sampleId);
    if (sample) {
      this.selectedSample.set(sample);
      this.loadHistoricalResults(sample.id);
    }
  }

  getCurrentTestName(): string {
    const test = this.availableTests.find(t => t.route === this.currentTestRoute());
    return test?.name || 'Unknown Test';
  }

  onTestChange(newTestRoute: string) {
    this.router.navigate(['/tests', newTestRoute]);
  }

  onSampleClick(sample: Sample) {
    const currentTestRoute = this.currentTestRoute();
    // Navigate to specific test entry component based on test type
    this.navigateToTestEntry(currentTestRoute, sample.id);
  }

  private navigateToTestEntry(testRoute: string, sampleId: number) {
    // Map test routes to their dedicated components
    const dedicatedComponents: { [key: string]: string } = {
      'tan': 'tan',
      'water-kf': 'water-kf',
      'tbn': 'tbn',
      'viscosity-40c': 'viscosity-40c',
      'viscosity-100c': 'viscosity-100c',
      'flash-point': 'flash-point',
      'emission-spectroscopy': 'emission-spectroscopy',
      'particle-count': 'particle-count',
      'grease-penetration': 'grease-penetration',
      'grease-dropping-point': 'grease-dropping-point',
      'inspect-filter': 'inspect-filter',
      'ferrography': 'ferrography',
      'rbot': 'rbot',
      'tfout': 'tfout',
      'rust': 'rust',
      'deleterious': 'deleterious',
      'd-inch': 'd-inch',
      'oil-content': 'oil-content',
      'varnish-potential-rating': 'varnish-potential-rating'
    };

    if (dedicatedComponents[testRoute]) {
      // Navigate to dedicated test entry component
      this.router.navigate(['/tests', dedicatedComponents[testRoute], sampleId]);
    } else {
      // Navigate to generic test workspace with sample
      this.router.navigate(['/tests', testRoute, sampleId]);
    }
  }

  private loadSamplesForTest(testRoute: string) {


    // Get the test ID from the route
    const testId = this.getTestIdFromRoute(testRoute);
    if (!testId) {
      console.warn(`No test ID found for route: ${testRoute}`);
      this.availableSamples.set([]);
      return;
    }

    // Load samples from API
    this.sampleService.getSamplesByTest(testId).subscribe({
      next: (samples) => {
        this.availableSamples.set(samples);
        this.selectedSample.set(null);
        this.historicalResults.set([]);
      },
      error: (error) => {
        console.error('Error loading samples:', error);
        this.snackBar.open('Failed to load samples', 'Close', { duration: 3000 });
        this.availableSamples.set([]);
      }
    });
  }

  private getTestIdFromRoute(testRoute: string): number | null {
    // Map test routes to test IDs based on our database mapping
    const routeToIdMap: { [key: string]: number } = {
      'tan': 10,                          // TAN by Color Indication
      'water-kf': 20,                     // Water-KF
      'tbn': 110,                         // TBN by Auto Titration
      'viscosity-40c': 50,                // Viscosity @ 40°C
      'emission-spectroscopy': 30,        // Emission Spectroscopy - Standard
      'emission-spectroscopy-large': 40,  // Emission Spectroscopy - Large
      'flash-point': 80,                  // Flash Point
      'particle-count': 160,              // Particle Count
      'viscosity-100c': 60,               // Viscosity @ 100°C
      'ft-ir': 70,                        // FT-IR
      'inspect-filter': 120,              // Inspect Filter
      'grease-penetration': 130,          // Grease Penetration Worked
      'grease-dropping-point': 140,       // Grease Dropping Point
      'rbot': 170,                        // RBOT
      'filter-residue': 180,              // Filter Residue
      'ferrography': 210,                 // Ferrography
      'rust': 220,                        // Rust
      'tfout': 230,                       // TFOUT
      'debris-identification': 240,       // Debris Identification
      'deleterious': 250,                 // Deleterious
      'rheometer': 270,                   // Rheometer
      'd-inch': 284,                      // D-inch
      'oil-content': 285,                 // Oil Content
      'varnish-potential-rating': 286     // Varnish Potential Rating
    };

    return routeToIdMap[testRoute] || null;
  }

  private loadAvailableTests() {
    this.testService.getQualifiedTests().subscribe({
      next: (tests) => {
        this.availableTests = tests.map(test => ({
          id: test.testId,
          name: test.testName,
          route: this.getTestRouteFromName(test.testName)
        }));
      },
      error: (error) => {
        console.error('Error loading available tests:', error);
        this.snackBar.open('Failed to load available tests', 'Close', { duration: 3000 });
      }
    });
  }

  private getTestRouteFromName(testName: string): string {
    // Map test names to routes (same as in test-list component)
    const routeMap: { [key: string]: string } = {
      'TAN by Color Indication': 'tan',
      'Water-KF': 'water-kf',
      'TBN by Auto Titration': 'tbn',
      'Viscosity @ 40°C': 'viscosity-40c',
      'Viscosity @ 100°C': 'viscosity-100c',
      'Flash Point': 'flash-point',
      'Emission Spectroscopy': 'emission-spectroscopy',
      'Particle Count': 'particle-count',
      'Grease Penetration Worked': 'grease-penetration',
      'Grease Dropping Point': 'grease-dropping-point',
      'Inspect Filter': 'inspect-filter',
      'Ferrography': 'ferrography',
      'RBOT': 'rbot',
      'TFOUT': 'tfout',
      'Rust': 'rust',
      'Deleterious': 'deleterious',
      'D-inch': 'd-inch',
      'Oil Content': 'oil-content',
      'Varnish Potential Rating': 'varnish-potential-rating',
      'FT-IR': 'ft-ir',
      'Filter Residue': 'filter-residue',
      'Rheometer': 'rheometer',
      'Misc. Tests': 'misc-tests',
      'Debris Identification': 'debris-identification'
    };

    return routeMap[testName] || testName.toLowerCase().replace(/[^a-z0-9]/g, '-');
  }

  private loadHistoricalResults(sampleId: number) {
    // Mock historical data
    const mockHistory: HistoricalResult[] = [
      {
        date: new Date('2024-11-01'),
        result: '2.45 mg KOH/g',
        technician: 'J.Smith',
        status: 'Complete'
      },
      {
        date: new Date('2024-10-15'),
        result: '2.38 mg KOH/g',
        technician: 'M.Johnson',
        status: 'Complete'
      },
      {
        date: new Date('2024-09-30'),
        result: '2.52 mg KOH/g',
        technician: 'K.Wilson',
        status: 'Complete'
      }
    ];

    this.historicalResults.set(mockHistory);
  }

  calculateTAN(): void {
    if (this.currentTestRoute() === 'tan') {
      const sampleWeight = this.testForm.get('sampleWeight')?.value;
      const finalBuret = this.testForm.get('finalBuret')?.value;

      if (sampleWeight && finalBuret && sampleWeight > 0) {
        const result = Math.round(((finalBuret * 5.61) / sampleWeight) * 100) / 100;
        this.calculatedResult.set(result);
      } else {
        this.calculatedResult.set(null);
      }
    }
  }

  onSubmit(): void {
    if (this.testForm.valid && this.selectedSample()) {
      const formData = {
        ...this.testForm.value,
        testType: this.currentTestRoute(),
        sampleId: this.selectedSample()?.id,
        calculatedResult: this.calculatedResult()
      };

      console.log('Saving test data:', formData);

      this.snackBar.open('Test results saved successfully!', 'Close', {
        duration: 3000
      });
    }
  }

  clearForm(): void {
    this.testForm.reset();
    this.calculatedResult.set(null);
  }
}