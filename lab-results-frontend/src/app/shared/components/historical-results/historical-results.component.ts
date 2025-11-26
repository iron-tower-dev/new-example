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
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatPaginatorModule } from '@angular/material/paginator';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { HistoricalResultsService, HistoricalResultSummary, HistoricalResultsFilter } from '../../services/historical-results.service';

export interface HistoricalResultsConfig {
    sampleId: number;
    testId: number;
    testName: string;
    showExtended?: boolean;
    initialCount?: number;
    resizable?: boolean;
    singleScreenMode?: boolean;
}

@Component({
    selector: 'app-historical-results',
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
        MatFormFieldModule,
        MatSelectModule,
        MatDatepickerModule,
        MatInputModule,
        MatNativeDateModule,
        MatPaginatorModule,
        ReactiveFormsModule
    ],
    template: `
        <mat-card [class.single-screen]="isSingleScreenMode()" [class.resizable]="config?.resizable">
            <mat-card-header>
                <mat-card-title>
                    <mat-toolbar color="primary" class="historical-toolbar">
                        <span>Last {{ displayCount() }} results for {{ config?.testName || 'Test' }}</span>
                        <span class="spacer"></span>
                        
                        @if (config?.resizable) {
                            <button mat-icon-button 
                                    (click)="toggleSingleScreen()"
                                    [matTooltip]="isSingleScreenMode() ? 'Exit single screen mode' : 'Enter single screen mode'">
                                <mat-icon>{{ isSingleScreenMode() ? 'fullscreen_exit' : 'fullscreen' }}</mat-icon>
                            </button>
                        }
                        
                        <button mat-icon-button [matMenuTriggerFor]="optionsMenu" matTooltip="Options">
                            <mat-icon>more_vert</mat-icon>
                        </button>
                        
                        <mat-menu #optionsMenu="matMenu">
                            <button mat-menu-item (click)="showExtendedHistory()">
                                <mat-icon>history</mat-icon>
                                <span>Extended History</span>
                            </button>
                            <button mat-menu-item (click)="refreshData()">
                                <mat-icon>refresh</mat-icon>
                                <span>Refresh</span>
                            </button>
                            <button mat-menu-item (click)="exportData()">
                                <mat-icon>download</mat-icon>
                                <span>Export</span>
                            </button>
                        </mat-menu>
                    </mat-toolbar>
                </mat-card-title>
            </mat-card-header>

            <mat-card-content>
                @if (historicalService.isLoading()) {
                    <div class="loading-container">
                        <mat-spinner diameter="40"></mat-spinner>
                        <p>Loading historical results...</p>
                    </div>
                } @else if (historicalService.hasError()) {
                    <div class="error-container">
                        <mat-icon color="warn">error</mat-icon>
                        <p>{{ historicalService.error() }}</p>
                        <button mat-button color="primary" (click)="refreshData()">
                            <mat-icon>refresh</mat-icon>
                            Retry
                        </button>
                    </div>
                } @else {
                    @if (showExtended()) {
                        <!-- Extended History View -->
                        <div class="extended-history">
                            <form [formGroup]="filterForm" class="filter-form">
                                <mat-form-field appearance="outline">
                                    <mat-label>From Date</mat-label>
                                    <input matInput [matDatepicker]="fromPicker" formControlName="fromDate">
                                    <mat-datepicker-toggle matIconSuffix [for]="fromPicker"></mat-datepicker-toggle>
                                    <mat-datepicker #fromPicker></mat-datepicker>
                                </mat-form-field>

                                <mat-form-field appearance="outline">
                                    <mat-label>To Date</mat-label>
                                    <input matInput [matDatepicker]="toPicker" formControlName="toDate">
                                    <mat-datepicker-toggle matIconSuffix [for]="toPicker"></mat-datepicker-toggle>
                                    <mat-datepicker #toPicker></mat-datepicker>
                                </mat-form-field>

                                <mat-form-field appearance="outline">
                                    <mat-label>Status</mat-label>
                                    <mat-select formControlName="status">
                                        <mat-option value="">All</mat-option>
                                        <mat-option value="Complete">Complete</mat-option>
                                        <mat-option value="In Progress">In Progress</mat-option>
                                        <mat-option value="Pending">Pending</mat-option>
                                    </mat-select>
                                </mat-form-field>

                                <button mat-raised-button color="primary" (click)="applyFilters()">
                                    <mat-icon>search</mat-icon>
                                    Apply Filters
                                </button>

                                <button mat-button (click)="clearFilters()">
                                    <mat-icon>clear</mat-icon>
                                    Clear
                                </button>

                                <button mat-button (click)="showBasicHistory()">
                                    <mat-icon>arrow_back</mat-icon>
                                    Back to Basic View
                                </button>
                            </form>

                            @if (historicalService.hasExtendedResults()) {
                                <div class="results-info">
                                    <p>Showing {{ historicalService.extendedResults()?.results?.length || 0 }} of {{ historicalService.extendedResults()?.totalCount || 0 }} results</p>
                                </div>

                                <mat-table [dataSource]="extendedDataSource()" class="historical-table">
                                    <ng-container matColumnDef="sampleId">
                                        <mat-header-cell *matHeaderCellDef>Sample ID</mat-header-cell>
                                        <mat-cell *matCellDef="let result">{{ result.sampleId }}</mat-cell>
                                    </ng-container>

                                    <ng-container matColumnDef="sampleDate">
                                        <mat-header-cell *matHeaderCellDef>Sample Date</mat-header-cell>
                                        <mat-cell *matCellDef="let result">{{ result.entryDate | date:'short' }}</mat-cell>
                                    </ng-container>

                                    <ng-container matColumnDef="status">
                                        <mat-header-cell *matHeaderCellDef>Status</mat-header-cell>
                                        <mat-cell *matCellDef="let result">
                                            <span [class]="'status-' + result.status.toLowerCase().replace(' ', '-')">
                                                {{ result.status }}
                                            </span>
                                        </mat-cell>
                                    </ng-container>

                                    <ng-container matColumnDef="entryDate">
                                        <mat-header-cell *matHeaderCellDef>Entry Date</mat-header-cell>
                                        <mat-cell *matCellDef="let result">{{ result.entryDate | date:'short' }}</mat-cell>
                                    </ng-container>

                                    <ng-container matColumnDef="actions">
                                        <mat-header-cell *matHeaderCellDef>Actions</mat-header-cell>
                                        <mat-cell *matCellDef="let result">
                                            <button mat-icon-button 
                                                    (click)="viewResultDetails(result)"
                                                    matTooltip="View Details">
                                                <mat-icon>visibility</mat-icon>
                                            </button>
                                        </mat-cell>
                                    </ng-container>

                                    <mat-header-row *matHeaderRowDef="extendedDisplayedColumns"></mat-header-row>
                                    <mat-row *matRowDef="let row; columns: extendedDisplayedColumns;"></mat-row>
                                </mat-table>

                                <mat-paginator 
                                    [length]="historicalService.extendedResults()?.totalCount || 0"
                                    [pageSize]="currentPageSize()"
                                    [pageIndex]="currentPageIndex()"
                                    [pageSizeOptions]="[10, 25, 50, 100]"
                                    (page)="onPageChange($event)"
                                    showFirstLastButtons>
                                </mat-paginator>
                            }
                        </div>
                    } @else {
                        <!-- Basic History View -->
                        @if (historicalService.hasHistoricalSummary()) {
                            <mat-table [dataSource]="basicDataSource()" class="historical-table">
                                <ng-container matColumnDef="sampleId">
                                    <mat-header-cell *matHeaderCellDef>Sample ID</mat-header-cell>
                                    <mat-cell *matCellDef="let summary">{{ summary.sampleId }}</mat-cell>
                                </ng-container>

                                <ng-container matColumnDef="tagNumber">
                                    <mat-header-cell *matHeaderCellDef>Tag Number</mat-header-cell>
                                    <mat-cell *matCellDef="let summary">{{ summary.tagNumber }}</mat-cell>
                                </ng-container>

                                <ng-container matColumnDef="sampleDate">
                                    <mat-header-cell *matHeaderCellDef>Sample Date</mat-header-cell>
                                    <mat-cell *matCellDef="let summary">{{ summary.sampleDate | date:'short' }}</mat-cell>
                                </ng-container>

                                <ng-container matColumnDef="status">
                                    <mat-header-cell *matHeaderCellDef>Status</mat-header-cell>
                                    <mat-cell *matCellDef="let summary">
                                        <span [class]="'status-' + summary.status.toLowerCase().replace(' ', '-')">
                                            {{ summary.status }}
                                        </span>
                                    </mat-cell>
                                </ng-container>

                                <ng-container matColumnDef="actions">
                                    <mat-header-cell *matHeaderCellDef>Actions</mat-header-cell>
                                    <mat-cell *matCellDef="let summary">
                                        <button mat-icon-button 
                                                (click)="viewSummaryDetails(summary)"
                                                matTooltip="View Details">
                                            <mat-icon>visibility</mat-icon>
                                        </button>
                                    </mat-cell>
                                </ng-container>

                                <mat-header-row *matHeaderRowDef="basicDisplayedColumns"></mat-header-row>
                                <mat-row *matRowDef="let row; columns: basicDisplayedColumns;" 
                                         (click)="viewSummaryDetails(row)"
                                         class="clickable-row"></mat-row>
                            </mat-table>
                        } @else {
                            <div class="no-data">
                                <mat-icon>history</mat-icon>
                                <p>No historical results found</p>
                            </div>
                        }
                    }
                }
            </mat-card-content>
        </mat-card>
    `,
    styles: [`
        .historical-toolbar {
            height: 48px;
            min-height: 48px;
        }

        .spacer {
            flex: 1 1 auto;
        }

        .single-screen {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            z-index: 1000;
            margin: 0;
            border-radius: 0;
        }

        .resizable {
            resize: both;
            overflow: auto;
            min-width: 400px;
            min-height: 300px;
        }

        .loading-container, .error-container, .no-data {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 2rem;
            text-align: center;
        }

        .error-container mat-icon {
            font-size: 48px;
            width: 48px;
            height: 48px;
            margin-bottom: 1rem;
        }

        .filter-form {
            display: flex;
            gap: 1rem;
            align-items: center;
            flex-wrap: wrap;
            margin-bottom: 1rem;
            padding: 1rem;
            background: #f5f5f5;
            border-radius: 4px;
        }

        .results-info {
            margin-bottom: 1rem;
            font-size: 0.9rem;
            color: #666;
        }

        .historical-table {
            width: 100%;
        }

        .clickable-row {
            cursor: pointer;
        }

        .clickable-row:hover {
            background-color: #f5f5f5;
        }

        .status-complete {
            color: #4caf50;
            font-weight: 500;
        }

        .status-in-progress {
            color: #ff9800;
            font-weight: 500;
        }

        .status-pending {
            color: #f44336;
            font-weight: 500;
        }

        .extended-history {
            width: 100%;
        }

        .no-data mat-icon {
            font-size: 64px;
            width: 64px;
            height: 64px;
            margin-bottom: 1rem;
            color: #ccc;
        }
    `]
})
export class HistoricalResultsComponent implements OnInit, OnDestroy {
    @Input() config: HistoricalResultsConfig | null = null;
    @Output() resultSelected = new EventEmitter<any>();
    @Output() singleScreenToggled = new EventEmitter<boolean>();

    protected historicalService = inject(HistoricalResultsService);
    private fb = inject(FormBuilder);

    // Signals for component state
    private _isSingleScreenMode = signal(false);
    private _showExtended = signal(false);
    private _displayCount = signal(12);
    private _currentPageSize = signal(25);
    private _currentPageIndex = signal(0);

    // Public readonly signals
    readonly isSingleScreenMode = this._isSingleScreenMode.asReadonly();
    readonly showExtended = this._showExtended.asReadonly();
    readonly displayCount = this._displayCount.asReadonly();
    readonly currentPageSize = this._currentPageSize.asReadonly();
    readonly currentPageIndex = this._currentPageIndex.asReadonly();

    // Computed signals for data sources
    readonly basicDataSource = computed(() => this.historicalService.historicalSummary());
    readonly extendedDataSource = computed(() => this.historicalService.extendedResults()?.results || []);

    // Table columns
    readonly basicDisplayedColumns = ['sampleId', 'tagNumber', 'sampleDate', 'status', 'actions'];
    readonly extendedDisplayedColumns = ['sampleId', 'sampleDate', 'status', 'entryDate', 'actions'];

    // Filter form
    filterForm: FormGroup;

    constructor() {
        this.filterForm = this.fb.group({
            fromDate: [null],
            toDate: [null],
            status: ['']
        });

        // Effect to load data when config changes
        effect(() => {
            const config = this.config;
            if (config) {
                this._displayCount.set(config.initialCount || 12);
                this._isSingleScreenMode.set(config.singleScreenMode || false);
                this.loadHistoricalData();
            }
        });
    }

    ngOnInit(): void {
        if (this.config) {
            this.loadHistoricalData();
        }
    }

    ngOnDestroy(): void {
        this.historicalService.clearData();
    }

    private loadHistoricalData(): void {
        if (!this.config) return;

        if (this._showExtended()) {
            this.loadExtendedHistory();
        } else {
            this.historicalService.getHistoricalResultsSummary(
                this.config.sampleId,
                this.config.testId,
                this._displayCount()
            ).subscribe();
        }
    }

    private loadExtendedHistory(): void {
        if (!this.config) return;

        const filter: HistoricalResultsFilter = {
            page: this._currentPageIndex() + 1,
            pageSize: this._currentPageSize(),
            ...this.filterForm.value
        };

        this.historicalService.getExtendedHistoricalResults(
            this.config.sampleId,
            this.config.testId,
            filter
        ).subscribe();
    }

    toggleSingleScreen(): void {
        const newMode = !this._isSingleScreenMode();
        this._isSingleScreenMode.set(newMode);
        this.singleScreenToggled.emit(newMode);
    }

    showExtendedHistory(): void {
        this._showExtended.set(true);
        this.loadExtendedHistory();
    }

    showBasicHistory(): void {
        this._showExtended.set(false);
        this.loadHistoricalData();
    }

    refreshData(): void {
        this.historicalService.clearError();
        this.loadHistoricalData();
    }

    exportData(): void {
        // TODO: Implement export functionality
        console.log('Export functionality to be implemented');
    }

    applyFilters(): void {
        this._currentPageIndex.set(0);
        this.loadExtendedHistory();
    }

    clearFilters(): void {
        this.filterForm.reset();
        this._currentPageIndex.set(0);
        this.loadExtendedHistory();
    }

    onPageChange(event: any): void {
        this._currentPageIndex.set(event.pageIndex);
        this._currentPageSize.set(event.pageSize);
        this.loadExtendedHistory();
    }

    viewSummaryDetails(summary: HistoricalResultSummary): void {
        if (!this.config) return;

        this.historicalService.getHistoricalResultDetails(
            this.config.sampleId,
            this.config.testId,
            summary.sampleId
        ).subscribe(result => {
            this.resultSelected.emit(result);
        });
    }

    viewResultDetails(result: any): void {
        this.historicalService.selectHistoricalResult(result);
        this.resultSelected.emit(result);
    }
}