import { Component, Input, inject, signal, computed } from '@angular/core';
import { SampleService } from '../../../../shared/services/sample.service';
import { Sample, SampleHistory } from '../../../../shared/models/sample.model';
import { SharedModule } from '../../../../shared/shared.module';

@Component({
  selector: 'app-sample-info-display',
  standalone: true,
  imports: [SharedModule],
  template: `
    <div class="sample-info-container">
      @if (sample(); as sampleData) {
        <div class="sample-header">
          <mat-card>
            <mat-card-header>
              <div mat-card-avatar class="sample-avatar">
                <mat-icon>science</mat-icon>
              </div>
              <mat-card-title>{{ sampleData.tagNumber }}</mat-card-title>
              <mat-card-subtitle>{{ sampleData.component }} - {{ sampleData.location }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <div class="sample-grid">
                <div class="info-item">
                  <mat-icon class="info-icon">category</mat-icon>
                  <div class="info-content">
                    <div class="info-label">Lube Type</div>
                    <div class="info-value">{{ sampleData.lubeType }}</div>
                  </div>
                </div>

                @if (sampleData.qualityClass) {
                  <div class="info-item">
                    <mat-icon class="info-icon">star</mat-icon>
                    <div class="info-content">
                      <div class="info-label">Quality Class</div>
                      <div class="info-value">{{ sampleData.qualityClass }}</div>
                    </div>
                  </div>
                }

                <div class="info-item">
                  <mat-icon class="info-icon">schedule</mat-icon>
                  <div class="info-content">
                    <div class="info-label">Sample Date</div>
                    <div class="info-value">{{ sampleData.sampleDate | date:'medium' }}</div>
                  </div>
                </div>

                <div class="info-item">
                  <mat-icon class="info-icon">info</mat-icon>
                  <div class="info-content">
                    <div class="info-label">Status</div>
                    <div class="info-value">
                      <span class="status-badge" [class]="'status-' + sampleData.status.toLowerCase()">
                        {{ sampleData.status }}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </mat-card-content>
          </mat-card>
        </div>

        @if (showHistory && testId) {
          <div class="history-section">
            <mat-card>
              <mat-card-header>
                <mat-card-title>
                  <mat-icon>history</mat-icon>
                  Last 12 Results for {{ testName || 'Test' }}
                </mat-card-title>
                <div class="spacer"></div>
                <button mat-icon-button (click)="toggleHistorySize()" [title]="isHistoryExpanded() ? 'Minimize' : 'Expand'">
                  <mat-icon>{{ isHistoryExpanded() ? 'fullscreen_exit' : 'fullscreen' }}</mat-icon>
                </button>
              </mat-card-header>
              <mat-card-content>
                @if (isLoadingHistory()) {
                  <div class="loading-container">
                    <mat-spinner diameter="40"></mat-spinner>
                    <p>Loading history...</p>
                  </div>
                } @else if (historyError()) {
                  <div class="error-container">
                    <mat-icon color="warn">error</mat-icon>
                    <p>{{ historyError() }}</p>
                    <button mat-button color="primary" (click)="loadHistory()">Retry</button>
                  </div>
                } @else if (sampleHistory().length > 0) {
                  <div class="history-table-container" [class.expanded]="isHistoryExpanded()">
                    <table mat-table [dataSource]="sampleHistory()" class="history-table">
                      <ng-container matColumnDef="sampleId">
                        <th mat-header-cell *matHeaderCellDef>Sample ID</th>
                        <td mat-cell *matCellDef="let item">{{ item.sampleId }}</td>
                      </ng-container>

                      <ng-container matColumnDef="sampleDate">
                        <th mat-header-cell *matHeaderCellDef>Sample Date</th>
                        <td mat-cell *matCellDef="let item">{{ item.sampleDate | date:'shortDate' }}</td>
                      </ng-container>

                      <ng-container matColumnDef="status">
                        <th mat-header-cell *matHeaderCellDef>Status</th>
                        <td mat-cell *matCellDef="let item">
                          <span class="status-badge" [class]="'status-' + item.status.toLowerCase()">
                            {{ item.status }}
                          </span>
                        </td>
                      </ng-container>

                      <ng-container matColumnDef="entryDate">
                        <th mat-header-cell *matHeaderCellDef>Entry Date</th>
                        <td mat-cell *matCellDef="let item">
                          {{ item.entryDate ? (item.entryDate | date:'shortDate') : '-' }}
                        </td>
                      </ng-container>

                      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
                      <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
                    </table>
                  </div>
                } @else {
                  <div class="no-data-container">
                    <mat-icon>history</mat-icon>
                    <p>No historical data available</p>
                  </div>
                }
              </mat-card-content>
            </mat-card>
          </div>
        }
      } @else {
        <div class="no-sample-container">
          <mat-card>
            <mat-card-content>
              <div class="no-sample-message">
                <mat-icon>info</mat-icon>
                <p>No sample selected</p>
              </div>
            </mat-card-content>
          </mat-card>
        </div>
      }
    </div>
  `,
  styles: [`
    .sample-info-container {
      width: 100%;
    }

    .sample-header {
      margin-bottom: 16px;
    }

    .sample-avatar {
      background-color: #1976d2;
      color: white;
    }

    .sample-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 16px;
      margin-top: 16px;
    }

    .info-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 8px;
      border-radius: 8px;
      background-color: #f8f9fa;
    }

    .info-icon {
      color: #666;
      font-size: 20px;
    }

    .info-content {
      flex: 1;
    }

    .info-label {
      font-size: 0.9em;
      color: #666;
      margin-bottom: 2px;
    }

    .info-value {
      font-weight: 500;
    }

    .status-badge {
      padding: 4px 8px;
      border-radius: 12px;
      font-size: 0.8em;
      font-weight: 500;
      text-transform: uppercase;
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

    .history-section {
      margin-top: 16px;
    }

    .spacer {
      flex: 1;
    }

    .history-table-container {
      max-height: 300px;
      overflow-y: auto;
      transition: max-height 0.3s ease;
    }

    .history-table-container.expanded {
      max-height: 80vh;
    }

    .history-table {
      width: 100%;
    }

    .loading-container,
    .error-container,
    .no-data-container,
    .no-sample-message {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 32px;
      text-align: center;
      color: #666;
    }

    .loading-container mat-icon,
    .error-container mat-icon,
    .no-data-container mat-icon,
    .no-sample-message mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 16px;
    }

    .error-container mat-icon {
      color: #f44336;
    }

    .no-data-container mat-icon,
    .no-sample-message mat-icon {
      color: #ccc;
    }
  `]
})
export class SampleInfoDisplayComponent {
  @Input() showHistory: boolean = true;
  @Input() testId: number | null = null;
  @Input() testName: string | null = null;

  private sampleService = inject(SampleService);

  // Signals from service
  readonly sample = this.sampleService.selectedSample;
  readonly sampleHistory = this.sampleService.sampleHistory;
  readonly isLoadingHistory = this.sampleService.isLoading;

  // Local signals
  private _historyError = signal<string | null>(null);
  private _isHistoryExpanded = signal(false);

  readonly historyError = this._historyError.asReadonly();
  readonly isHistoryExpanded = this._isHistoryExpanded.asReadonly();

  // Table configuration
  displayedColumns: string[] = ['sampleId', 'sampleDate', 'status', 'entryDate'];

  ngOnInit(): void {
    // Load history when sample or test changes
    if (this.sample() && this.testId && this.showHistory) {
      this.loadHistory();
    }
  }

  loadHistory(): void {
    const sample = this.sample();
    if (sample && this.testId) {
      this._historyError.set(null);
      this.sampleService.getSampleHistory(sample.id, this.testId).subscribe({
        error: (error) => {
          this._historyError.set(`Failed to load history: ${error.message}`);
        }
      });
    }
  }

  toggleHistorySize(): void {
    this._isHistoryExpanded.update(expanded => !expanded);
  }

  // Public method to refresh history
  refreshHistory(): void {
    this.loadHistory();
  }
}