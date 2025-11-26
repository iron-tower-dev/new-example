import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { AuthService } from '../../../../shared/services/auth.service';
import { UserInfo } from '../../../../shared/models/auth.model';
// import { SampleSelectionDialogComponent } from '../../../../shared/components/sample-selection-dialog/sample-selection-dialog.component';

interface TestCategory {
  name: string;
  description: string;
  icon: string;
  tests: TestInfo[];
  color: string;
}

interface TestInfo {
  name: string;
  route: string;
  description: string;
  testStandId?: number;
  requiredLevel?: string;
}

interface UserStats {
  testsCompleted: number;
  qualifications: string[];
  pendingReviews: number;
}

@Component({
  selector: 'app-test-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatGridListModule,
    MatChipsModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <h1>Laboratory Test Dashboard</h1>
        <p>Select a test category to view available tests</p>
        <div class="debug-info">
          <p><strong>DEBUG:</strong> Dashboard component loaded successfully!</p>
          <p><strong>Auth Status:</strong> {{ authService.isAuthenticated() ? 'Authenticated' : 'Not Authenticated' }}</p>
        </div>
        <div class="user-info" *ngIf="currentUser">
          <span>Welcome, {{ currentUser.fullName }} ({{ currentUser.role }})</span>
        </div>
      </div>

      <div class="test-categories">
        <mat-grid-list cols="3" rowHeight="300px" gutterSize="16px">
          <mat-grid-tile *ngFor="let category of testCategories">
            <mat-card class="category-card" [ngClass]="category.color">
              <mat-card-header>
                <mat-icon mat-card-avatar>{{ category.icon }}</mat-icon>
                <mat-card-title>{{ category.name }}</mat-card-title>
                <mat-card-subtitle>{{ category.description }}</mat-card-subtitle>
              </mat-card-header>
              
              <mat-card-content>
                <div class="test-list">
                  <mat-chip-listbox>
                    <mat-chip-option 
                      *ngFor="let test of category.tests" 
                      [disabled]="!canAccessTest(test)"
                      (click)="navigateToTest(test)"
                      class="test-chip"
                      [class.restricted]="!canAccessTest(test)">
                      {{ test.name }}
                      <mat-icon *ngIf="test.requiredLevel" matChipTrailingIcon>
                        {{ canAccessTest(test) ? 'verified' : 'lock' }}
                      </mat-icon>
                    </mat-chip-option>
                  </mat-chip-listbox>
                </div>
              </mat-card-content>
              
              <mat-card-actions>
                <button mat-button color="primary" (click)="showCategoryDetails(category)">
                  View All Tests
                </button>
              </mat-card-actions>
            </mat-card>
          </mat-grid-tile>
        </mat-grid-list>
      </div>

      <div class="quick-stats" *ngIf="userStats">
        <mat-card>
          <mat-card-header>
            <mat-card-title>Your Test Statistics</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="stats-grid">
              <div class="stat-item">
                <span class="stat-number">{{ userStats.testsCompleted }}</span>
                <span class="stat-label">Tests Completed Today</span>
              </div>
              <div class="stat-item">
                <span class="stat-number">{{ userStats.qualifications.length }}</span>
                <span class="stat-label">Active Qualifications</span>
              </div>
              <div class="stat-item">
                <span class="stat-number">{{ userStats.pendingReviews }}</span>
                <span class="stat-label">Pending Reviews</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <div class="help-section">
        <mat-card>
          <mat-card-header>
            <mat-card-title>Need Help?</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>To perform a test, you'll need to:</p>
            <ol>
              <li>Select a sample from the Sample Management section</li>
              <li>Choose the appropriate test from the categories above</li>
              <li>Ensure you have the required qualifications for the test</li>
            </ol>
            <button mat-raised-button color="primary" (click)="navigateToSamples()">
              Go to Sample Management
            </button>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .dashboard-header {
      text-align: center;
      margin-bottom: 32px;
    }

    .dashboard-header h1 {
      color: #1976d2;
      margin-bottom: 8px;
    }

    .user-info {
      margin-top: 8px;
      color: #666;
      font-size: 0.9rem;
    }

    .debug-info {
      background: #e3f2fd;
      padding: 12px;
      border-radius: 4px;
      margin: 16px 0;
      border-left: 4px solid #2196f3;
    }

    .debug-info p {
      margin: 4px 0;
      font-size: 0.9rem;
    }

    .test-categories {
      margin-bottom: 32px;
    }

    .category-card {
      width: 100%;
      height: 100%;
      display: flex;
      flex-direction: column;
      transition: transform 0.2s ease-in-out;
    }

    .category-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 16px rgba(0,0,0,0.1);
    }

    .category-card.basic-tests {
      border-left: 4px solid #4caf50;
    }

    .category-card.viscosity-tests {
      border-left: 4px solid #ff9800;
    }

    .category-card.complex-tests {
      border-left: 4px solid #f44336;
    }

    .category-card.grease-tests {
      border-left: 4px solid #9c27b0;
    }

    .category-card.particle-tests {
      border-left: 4px solid #2196f3;
    }

    .category-card.specialized-tests {
      border-left: 4px solid #607d8b;
    }

    .test-list {
      flex: 1;
      overflow-y: auto;
      max-height: 120px;
    }

    .test-chip {
      margin: 4px;
      cursor: pointer;
    }

    .test-chip.restricted {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .test-chip:not(.restricted):hover {
      background-color: #e3f2fd;
    }

    .quick-stats, .help-section {
      margin-top: 32px;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 24px;
      text-align: center;
    }

    .stat-item {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .stat-number {
      font-size: 2rem;
      font-weight: bold;
      color: #1976d2;
    }

    .stat-label {
      font-size: 0.875rem;
      color: #666;
      margin-top: 4px;
    }

    .help-section ol {
      margin: 16px 0;
      padding-left: 20px;
    }

    .help-section li {
      margin-bottom: 8px;
    }

    @media (max-width: 768px) {
      .test-categories mat-grid-list {
        cols: 1;
      }
      
      .stats-grid {
        grid-template-columns: 1fr;
        gap: 16px;
      }
    }

    @media (max-width: 1024px) {
      .test-categories mat-grid-list {
        cols: 2;
      }
    }
  `]
})
export class TestDashboardComponent implements OnInit {
  currentUser: UserInfo | null = null;
  userStats: UserStats | null = null;

  testCategories: TestCategory[] = [
    {
      name: 'Basic Tests',
      description: 'Fundamental oil analysis tests',
      icon: 'science',
      color: 'basic-tests',
      tests: [
        { name: 'TAN', route: 'tan', description: 'Total Acid Number', testStandId: 1, requiredLevel: 'TRAIN' },
        { name: 'Water-KF', route: 'water-kf', description: 'Water Content by Karl Fischer', testStandId: 2, requiredLevel: 'TRAIN' },
        { name: 'TBN', route: 'tbn', description: 'Total Base Number', testStandId: 3, requiredLevel: 'TRAIN' }
      ]
    },
    {
      name: 'Viscosity Tests',
      description: 'Temperature-dependent viscosity measurements',
      icon: 'opacity',
      color: 'viscosity-tests',
      tests: [
        { name: 'Viscosity @ 40°C', route: 'viscosity-40c', description: 'Kinematic viscosity at 40°C', testStandId: 4, requiredLevel: 'TRAIN' },
        { name: 'Viscosity @ 100°C', route: 'viscosity-100c', description: 'Kinematic viscosity at 100°C', testStandId: 5, requiredLevel: 'TRAIN' },
        { name: 'Flash Point', route: 'flash-point', description: 'Flash point determination', testStandId: 6, requiredLevel: 'Q/QAG' }
      ]
    },
    {
      name: 'Complex Tests',
      description: 'Advanced analytical procedures',
      icon: 'analytics',
      color: 'complex-tests',
      tests: [
        { name: 'Emission Spectroscopy', route: 'emission-spectroscopy', description: 'Elemental analysis', testStandId: 7, requiredLevel: 'Q/QAG' },
        { name: 'Particle Count', route: 'particle-count', description: 'ISO particle counting', testStandId: 8, requiredLevel: 'TRAIN' }
      ]
    },
    {
      name: 'Grease Tests',
      description: 'Specialized grease analysis',
      icon: 'build',
      color: 'grease-tests',
      tests: [
        { name: 'Grease Penetration', route: 'grease-penetration', description: 'NLGI consistency measurement', testStandId: 9, requiredLevel: 'TRAIN' },
        { name: 'Grease Dropping Point', route: 'grease-dropping-point', description: 'Thermal stability test', testStandId: 10, requiredLevel: 'Q/QAG' }
      ]
    },
    {
      name: 'Particle Analysis',
      description: 'Microscopic and wear particle analysis',
      icon: 'search',
      color: 'particle-tests',
      tests: [
        { name: 'Inspect Filter', route: 'inspect-filter', description: 'Visual filter inspection', testStandId: 11, requiredLevel: 'TRAIN' },
        { name: 'Ferrography', route: 'ferrography', description: 'Wear particle analysis', testStandId: 12, requiredLevel: 'MicrE' }
      ]
    },
    {
      name: 'Specialized Tests',
      description: 'Advanced and specialized procedures',
      icon: 'precision_manufacturing',
      color: 'specialized-tests',
      tests: [
        { name: 'RBOT', route: 'rbot', description: 'Rotating Bomb Oxidation Test', testStandId: 13, requiredLevel: 'Q/QAG' },
        { name: 'TFOUT', route: 'tfout', description: 'Thin Film Oxygen Uptake Test', testStandId: 14, requiredLevel: 'Q/QAG' },
        { name: 'Rust', route: 'rust', description: 'Rust and corrosion test', testStandId: 15, requiredLevel: 'TRAIN' },
        { name: 'Deleterious', route: 'deleterious', description: 'Deleterious materials test', testStandId: 16, requiredLevel: 'TRAIN' },
        { name: 'D-inch', route: 'd-inch', description: 'D-inch particle analysis', testStandId: 17, requiredLevel: 'MicrE' },
        { name: 'Oil Content', route: 'oil-content', description: 'Oil content determination', testStandId: 18, requiredLevel: 'TRAIN' },
        { name: 'Varnish Potential Rating', route: 'varnish-potential-rating', description: 'VPR analysis', testStandId: 19, requiredLevel: 'MicrE' }
      ]
    }
  ];

  constructor(
    public authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) { }

  ngOnInit() {
    console.log('TestDashboardComponent initialized');
    this.currentUser = this.authService.currentUser();
    console.log('Current user:', this.currentUser);
    this.loadUserStats();
  }

  canAccessTest(test: TestInfo): boolean {
    if (!test.testStandId || !test.requiredLevel) {
      return true; // No restrictions
    }

    return this.authService.hasQualification(test.testStandId, test.requiredLevel);
  }

  navigateToTest(test: TestInfo) {
    if (!this.canAccessTest(test)) {
      this.snackBar.open(
        `You need ${test.requiredLevel} qualification for ${test.name}`,
        'Close',
        { duration: 3000 }
      );
      return;
    }

    // Navigate to test selection workflow instead
    this.router.navigate(['/tests']);
  }

  navigateToSamples() {
    this.router.navigate(['/samples']);
  }

  showCategoryDetails(category: TestCategory) {
    const availableTests = category.tests.filter(test => this.canAccessTest(test));
    const restrictedTests = category.tests.filter(test => !this.canAccessTest(test));

    let message = `${category.name}:\n`;
    message += `Available tests: ${availableTests.length}\n`;
    if (restrictedTests.length > 0) {
      message += `Restricted tests: ${restrictedTests.length}`;
    }

    this.snackBar.open(message, 'Close', { duration: 4000 });
  }

  private loadUserStats() {
    if (!this.currentUser) return;

    // Mock user statistics - in real implementation, this would come from an API
    const qualificationNames = this.currentUser.qualifications
      .map(q => q.testStand || 'Unknown')
      .filter(name => name !== 'Unknown');

    this.userStats = {
      testsCompleted: Math.floor(Math.random() * 20) + 5, // Mock data
      qualifications: qualificationNames,
      pendingReviews: this.authService.isReviewer() ? Math.floor(Math.random() * 10) : 0
    };
  }
}