import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { HistoricalResultsService, HistoricalResultSummary } from '../../services/historical-results.service';

@Component({
    selector: 'app-historical-results-panel',
    standalone: true,
    imports: [
        CommonModule,
        MatTableModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatCardModule,
        MatToolbarModule,
        MatTooltipModule,
        MatMenuModule,
        MatDividerModule
    ],
    template: `
        <div class="historical-panel" [class.resizable]="resizable" [class.collapsed]="isCollapsed()">
            <div class="panel-header">
                <div class="panel-title">
                    <mat-icon>history</mat-icon>
                    <span>Last {{ displayCount() }} results for {{ testName || 'Test' }}</span>
                </div>
                <div class="panel-actions">
                    <button mat-icon-button 
                            (click)="toggleCollapse()"
                            [matTooltip]="isCollapsed() ? 'Expand panel' : 'Collapse panel'">
                        <mat-icon>{{ isCollapsed() ? 'expand_more' : 'expand_less' }}</mat-icon>
                    </button>
                    
                    @if (resizable) {
                        <button mat-icon-button 
                                (click)="toggleSingleScreen()"
                                [matTooltip]="isSingleScreenMode() ? 'Exit single screen' : 'Single screen mode'">
                            <mat-icon>{{ isSingleScreenMode() ? 'fullscreen_exit' : 'fullscreen' }}</mat-icon>
                        </button>
                    }
                    
                    <button mat-icon-button [matMenuTriggerFor]="panelMenu" matTooltip="Options">
                        <mat-icon>more_vert</mat-icon>
                    </button>
                    
                    <mat-menu #panelMenu="matMenu">
                        <button mat-menu-item (click)="openExtendedHistory()">
                            <mat-icon>open_in_new</mat-icon>
                            <span>Extended History</span>
                        </button>
                        <button mat-menu-item (click)="refreshData()">
                            <mat-icon>refresh</mat-icon>
                            <span>Refresh</span>
                        </button>
                        <mat-divider></mat-divider>
                        <button mat-menu-item (click)="changeDisplayCount(6)">
                            <span>Show 6 results</span>
                        </button>
                        <button mat-menu-item (click)="changeDisplayCount(12)">
                            <span>Show 12 results</span>
                        </button>
                        <button mat-menu-item (click)="changeDisplayCount(24)">
                            <span>Show 24 results</span>
                        </button>
                    </mat-menu>
                </div>
            </div>

            @if (!isCollapsed()) {
                <div class="panel-content">
                    @if (historicalService.isLoading()) {
                        <div class="loading-container">
                            <mat-spinner diameter="30"></mat-spinner>
                            <span>Loading...</span>
                        </div>
                    } @else if (historicalService.hasError()) {
                        <div class="error-container">
                            <mat-icon color="warn">error</mat-icon>
                            <span>{{ historicalService.error() }}</span>
                            <button mat-icon-button (click)="refreshData()" matTooltip="Retry">
                                <mat-icon>refresh</mat-icon>
                            </button>
                        </div>
                    } @else if (historicalService.hasHistoricalSummary()) {
                        <div class="results-table">
                            <table class="compact-table">
                                <thead>
                                    <tr>
                                        <th>Sample ID</th>
                                        <th>Date</th>
                                        <th>Status</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @for (summary of historicalService.historicalSummary(); track summary.sampleId) {
                                        <tr class="result-row" 
                                            [class.selected]="selectedSampleId() === summary.sampleId"
                                            (click)="selectResult(summary)">
                                            <td>{{ summary.sampleId }}</td>
                                            <td>{{ summary.sampleDate | date:'M/d/yy' }}</td>
                                            <td>
                                                <span [class]="'status-badge status-' + summary.status.toLowerCase().replace(' ', '-')">
                                                    {{ getStatusAbbreviation(summary.status) }}
                                                </span>
                                            </td>
                                            <td>
                                                <button mat-icon-button 
                                                        (click)="viewDetails(summary, $event)"
                                                        matTooltip="View Details"
                                                        class="action-button">
                                                    <mat-icon>visibility</mat-icon>
                                                </button>
                                            </td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>
                    } @else {
                        <div class="no-data">
                            <mat-icon>history</mat-icon>
                            <span>No historical results</span>
                        </div>
                    }
                </div>
            }
        </div>
    `,
    styles: [`
        .historical-panel {
            border: 1px solid #e0e0e0;
            border-radius: 4px;
            background: white;
            min-height: 60px;
            transition: all 0.3s ease;
        }

        .historical-panel.resizable {
            resize: vertical;
            overflow: auto;
            min-height: 200px;
            max-height: 600px;
        }

        .historical-panel.collapsed {
            height: 60px;
            overflow: hidden;
        }

        .panel-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 16px;
            background: #f5f5f5;
            border-bottom: 1px solid #e0e0e0;
            min-height: 44px;
        }

        .panel-title {
            display: flex;
            align-items: center;
            gap: 8px;
            font-weight: 500;
            color: #333;
        }

        .panel-title mat-icon {
            font-size: 20px;
            width: 20px;
            height: 20px;
        }

        .panel-actions {
            display: flex;
            align-items: center;
            gap: 4px;
        }

        .panel-content {
            padding: 8px;
            max-height: calc(100% - 60px);
            overflow: auto;
        }

        .loading-container, .error-container, .no-data {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            padding: 16px;
            color: #666;
            font-size: 0.9rem;
        }

        .error-container {
            color: #f44336;
        }

        .results-table {
            width: 100%;
        }

        .compact-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 0.85rem;
        }

        .compact-table th {
            background: #f8f9fa;
            padding: 6px 8px;
            text-align: left;
            font-weight: 500;
            border-bottom: 1px solid #e0e0e0;
            color: #555;
        }

        .compact-table td {
            padding: 6px 8px;
            border-bottom: 1px solid #f0f0f0;
        }

        .result-row {
            cursor: pointer;
            transition: background-color 0.2s ease;
        }

        .result-row:hover {
            background-color: #f5f5f5;
        }

        .result-row.selected {
            background-color: #e3f2fd;
        }

        .status-badge {
            padding: 2px 6px;
            border-radius: 12px;
            font-size: 0.75rem;
            font-weight: 500;
            text-transform: uppercase;
        }

        .status-complete {
            background: #e8f5e8;
            color: #2e7d32;
        }

        .status-in-progress {
            background: #fff3e0;
            color: #f57c00;
        }

        .status-pending {
            background: #ffebee;
            color: #c62828;
        }

        .action-button {
            width: 24px;
            height: 24px;
            line-height: 24px;
        }

        .action-button mat-icon {
            font-size: 16px;
            width: 16px;
            height: 16px;
        }

        .no-data mat-icon {
            font-size: 24px;
            width: 24px;
            height: 24px;
            color: #ccc;
        }
    `]
})
export class HistoricalResultsPanelComponent implements OnInit, OnDestroy {
    @Input() sampleId: number | null = null;
    @Input() testId: number | null = null;
    @Input() testName: string | null = null;
    @Input() resizable: boolean = true;
    @Input() initialCount: number = 12;
    @Input() autoLoad: boolean = true;

    @Output() resultSelected = new EventEmitter<HistoricalResultSummary>();
    @Output() detailsRequested = new EventEmitter<HistoricalResultSummary>();
    @Output() extendedHistoryRequested = new EventEmitter<{ sampleId: number, testId: number, testName: string }>();
    @Output() singleScreenToggled = new EventEmitter<boolean>();

    protected historicalService = inject(HistoricalResultsService);

    // Component state signals
    private _isCollapsed = signal(false);
    private _isSingleScreenMode = signal(false);
    private _displayCount = signal(12);
    private _selectedSampleId = signal<number | null>(null);

    // Public readonly signals
    readonly isCollapsed = this._isCollapsed.asReadonly();
    readonly isSingleScreenMode = this._isSingleScreenMode.asReadonly();
    readonly displayCount = this._displayCount.asReadonly();
    readonly selectedSampleId = this._selectedSampleId.asReadonly();

    constructor() {
        // Effect to load data when inputs change
        effect(() => {
            if (this.autoLoad && this.sampleId && this.testId) {
                this.loadData();
            }
        });
    }

    ngOnInit(): void {
        this._displayCount.set(this.initialCount);

        if (this.autoLoad && this.sampleId && this.testId) {
            this.loadData();
        }
    }

    ngOnDestroy(): void {
        // Don't clear all data as it might be used by other components
        // this.historicalService.clearData();
    }

    private loadData(): void {
        if (!this.sampleId || !this.testId) return;

        this.historicalService.getHistoricalResultsSummary(
            this.sampleId,
            this.testId,
            this._displayCount()
        ).subscribe();
    }

    toggleCollapse(): void {
        this._isCollapsed.set(!this._isCollapsed());
    }

    toggleSingleScreen(): void {
        const newMode = !this._isSingleScreenMode();
        this._isSingleScreenMode.set(newMode);
        this.singleScreenToggled.emit(newMode);
    }

    refreshData(): void {
        this.historicalService.clearError();
        this.loadData();
    }

    changeDisplayCount(count: number): void {
        this._displayCount.set(count);
        this.loadData();
    }

    selectResult(summary: HistoricalResultSummary): void {
        this._selectedSampleId.set(summary.sampleId);
        this.resultSelected.emit(summary);
    }

    viewDetails(summary: HistoricalResultSummary, event: Event): void {
        event.stopPropagation(); // Prevent row selection
        this.detailsRequested.emit(summary);
    }

    openExtendedHistory(): void {
        if (this.sampleId && this.testId) {
            this.extendedHistoryRequested.emit({
                sampleId: this.sampleId,
                testId: this.testId,
                testName: this.testName || 'Test'
            });
        }
    }

    getStatusAbbreviation(status: string): string {
        switch (status.toLowerCase()) {
            case 'complete':
                return 'C';
            case 'in progress':
                return 'IP';
            case 'pending':
                return 'P';
            default:
                return status.charAt(0).toUpperCase();
        }
    }

    // Public methods for external control
    loadHistoricalData(sampleId: number, testId: number, testName?: string): void {
        this.sampleId = sampleId;
        this.testId = testId;
        if (testName) {
            this.testName = testName;
        }
        this.loadData();
    }

    collapse(): void {
        this._isCollapsed.set(true);
    }

    expand(): void {
        this._isCollapsed.set(false);
    }

    clearSelection(): void {
        this._selectedSampleId.set(null);
    }
}