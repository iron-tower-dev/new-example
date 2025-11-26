import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../../shared/services/auth.service';
import { TestService } from '../../../../shared/services/test.service';

interface TestInfo {
  id: number;
  name: string;
  description: string;
  route: string;
  testStandId?: number;
  requiredLevel?: string;
  category: string;
}

@Component({
  selector: 'app-test-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="test-list-container">
      <div class="header">
        <h1>Available Laboratory Tests</h1>
        <p>Select a test you are qualified to perform</p>
      </div>

      @if (isLoading) {
        <div class="loading-container">
          <mat-spinner></mat-spinner>
          <p>Loading qualified tests...</p>
        </div>
      } @else {
        <div class="test-grid">
          @for (test of qualifiedTests; track test.id) {
          <mat-card class="test-card" (click)="selectTest(test)">
            <mat-card-header>
              <mat-card-title>{{ test.name }}</mat-card-title>
              <mat-card-subtitle>{{ test.description }}</mat-card-subtitle>
            </mat-card-header>
            
            <mat-card-content>
              <div class="test-info">
                <mat-chip class="category-chip">{{ test.category }}</mat-chip>
                @if (test.requiredLevel) {
                  <mat-chip class="qualification-chip">{{ test.requiredLevel }}</mat-chip>
                }
              </div>
            </mat-card-content>
            
            <mat-card-actions>
              <button mat-raised-button color="primary">
                <mat-icon>play_arrow</mat-icon>
                Start Test
              </button>
            </mat-card-actions>
          </mat-card>
          }
        </div>

        @if (qualifiedTests.length === 0) {
        <div class="no-tests">
          <mat-icon>info</mat-icon>
          <h3>No Tests Available</h3>
          <p>You are not currently qualified to perform any tests.</p>
        </div>
      }
    }
    </div>
  `,
  styles: [`
    .test-list-container {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }

    .header {
      text-align: center;
      margin-bottom: 32px;
    }

    .header h1 {
      font-size: 32px;
      color: #1976d2;
      margin-bottom: 8px;
    }

    .header p {
      font-size: 18px;
      color: #666;
    }

    .test-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 24px;
    }

    .test-card {
      cursor: pointer;
      transition: all 0.2s ease;
      height: 200px;
      display: flex;
      flex-direction: column;
    }

    .test-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 16px rgba(0,0,0,0.15);
    }

    .test-info {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
      margin-top: 8px;
    }

    .category-chip {
      background-color: #e3f2fd;
      color: #1976d2;
    }

    .qualification-chip {
      background-color: #e8f5e8;
      color: #2e7d32;
    }

    mat-card-content {
      flex: 1;
    }

    mat-card-actions {
      justify-content: flex-end;
    }

    .no-tests {
      text-align: center;
      padding: 60px 20px;
      color: #666;
    }

    .no-tests mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
    }

    .loading-container {
      text-align: center;
      padding: 60px 20px;
      color: #666;
    }

    .loading-container mat-spinner {
      margin: 0 auto 16px;
    }

    @media (max-width: 768px) {
      .test-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class TestListComponent implements OnInit {
  private authService = inject(AuthService);
  private testService = inject(TestService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  qualifiedTests: TestInfo[] = [];
  isLoading = false;

  ngOnInit() {
    this.loadQualifiedTests();
  }

  private loadQualifiedTests() {
    this.isLoading = true;
    this.testService.getQualifiedTests().subscribe({
      next: (tests) => {
        this.qualifiedTests = tests.map(test => ({
          id: test.testId,
          name: test.testName,
          description: test.testDescription || '',
          route: this.getTestRoute(test.testName),
          category: this.getTestCategory(test.testName)
        }));
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading qualified tests:', error);
        this.snackBar.open('Failed to load qualified tests', 'Close', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  private getTestRoute(testName: string): string {
    // Map test names to routes
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

  private getTestCategory(testName: string): string {
    // Categorize tests based on their names
    if (testName.includes('TAN') || testName.includes('TBN') || testName.includes('Water') || testName.includes('FT-IR')) {
      return 'Chemical';
    } else if (testName.includes('Viscosity') || testName.includes('Flash Point')) {
      return 'Physical';
    } else if (testName.includes('Grease')) {
      return 'Grease';
    } else if (testName.includes('Particle') || testName.includes('Filter') || testName.includes('Ferrography') || testName.includes('Debris')) {
      return 'Particle Analysis';
    } else if (testName.includes('Emission') || testName.includes('Spectroscopy')) {
      return 'Spectroscopy';
    } else {
      return 'Specialized';
    }
  }

  selectTest(test: TestInfo) {
    this.router.navigate(['/tests', test.route]);
  }
}