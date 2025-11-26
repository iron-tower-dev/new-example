import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog } from '@angular/material/dialog';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

import { HistoricalResultsComponent, HistoricalResultsConfig } from '../historical-results/historical-results.component';
import { HistoricalResultsPanelComponent } from '../historical-results-panel/historical-results-panel.component';
import { HistoricalResultDetailsDialogComponent, HistoricalResultDetailsDialogData } from '../historical-result-details-dialog/historical-result-details-dialog.component';
import { HistoricalResultSummary } from '../../services/historical-results.service';

@Component({
    selector: 'app-historical-results-demo',
    standalone: true,
    imports: [
        CommonModule,
        MatButtonModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        ReactiveFormsModule,
        HistoricalResultsComponent,
        HistoricalResultsPanelComponent
    ],
    template: `
        <div class="demo-container">
            <h2>Historical Results Components Demo</h2>
            
            <!-- Configuration Form -->
            <mat-card class="config-card">
                <mat-card-header>
                    <mat-card-title>Configuration</mat-card-title>
                </mat-card-header>
                <mat-card-content>
                    <form [formGroup]="configForm" class="config-form">
                        <mat-form-field appearance="outline">
                            <mat-label>Sample ID</mat-label>
                            <input matInput type="number" formControlName="sampleId" placeholder="Enter sample ID">
                        </mat-form-field>

                        <mat-form-field appearance="outline">
                            <mat-label>Test ID</mat-label>
                            <input matInput type="number" formControlName="testId" placeholder="Enter test ID">
                        </mat-form-field>

                        <mat-form-field appearance="outline">
                            <mat-label>Test Name</mat-label>
                            <mat-select formControlName="testName">
                                <mat-option value="TAN by Color Indication">TAN by Color Indication</mat-option>
                                <mat-option value="Water-KF">Water-KF</mat-option>
                                <mat-option value="TBN by Auto Titration">TBN by Auto Titration</mat-option>
                                <mat-option value="Viscosity @ 40°C">Viscosity @ 40°C</mat-option>
                                <mat-option value="Viscosity @ 100°C">Viscosity @ 100°C</mat-option>
                                <mat-option value="Flash Point">Flash Point</mat-option>
                            </mat-select>
                        </mat-form-field>

                        <button mat-raised-button color="primary" 
                                (click)="updateConfiguration()"
                                [disabled]="configForm.invalid">
                            Update Configuration
                        </button>
                    </form>
                </mat-card-content>
            </mat-card>

            <!-- Demo Sections -->
            <div class="demo-sections">
                <!-- Historical Results Panel Demo -->
                <mat-card class="demo-section">
                    <mat-card-header>
                        <mat-card-title>Historical Results Panel (Compact)</mat-card-title>
                        <mat-card-subtitle>Suitable for embedding in test entry forms</mat-card-subtitle>
                    </mat-card-header>
                    <mat-card-content>
                        @if (currentConfig()) {
                            <app-historical-results-panel
                                [sampleId]="currentConfig()!.sampleId"
                                [testId]="currentConfig()!.testId"
                                [testName]="currentConfig()!.testName"
                                [resizable]="true"
                                [initialCount]="12"
                                [autoLoad]="true"
                                (resultSelected)="onPanelResultSelected($event)"
                                (detailsRequested)="onPanelDetailsRequested($event)"
                                (extendedHistoryRequested)="onExtendedHistoryRequested($event)"
                                (singleScreenToggled)="onSingleScreenToggled($event)">
                            </app-historical-results-panel>
                        } @else {
                            <p>Please configure sample ID, test ID, and test name above to see the historical results panel.</p>
                        }
                    </mat-card-content>
                </mat-card>

                <!-- Full Historical Results Demo -->
                <mat-card class="demo-section">
                    <mat-card-header>
                        <mat-card-title>Full Historical Results Component</mat-card-title>
                        <mat-card-subtitle>Complete historical results view with filtering and pagination</mat-card-subtitle>
                    </mat-card-header>
                    <mat-card-content>
                        @if (currentConfig()) {
                            <app-historical-results
                                [config]="currentConfig()"
                                (resultSelected)="onFullResultSelected($event)"
                                (singleScreenToggled)="onSingleScreenToggled($event)">
                            </app-historical-results>
                        } @else {
                            <p>Please configure sample ID, test ID, and test name above to see the full historical results component.</p>
                        }
                    </mat-card-content>
                </mat-card>
            </div>

            <!-- Event Log -->
            <mat-card class="event-log">
                <mat-card-header>
                    <mat-card-title>Event Log</mat-card-title>
                    <mat-card-subtitle>Shows events fired by the components</mat-card-subtitle>
                </mat-card-header>
                <mat-card-content>
                    <div class="log-container">
                        @if (eventLog().length === 0) {
                            <p>No events logged yet. Interact with the components above to see events.</p>
                        } @else {
                            @for (event of eventLog(); track $index) {
                                <div class="log-entry">
                                    <span class="timestamp">{{ event.timestamp | date:'HH:mm:ss' }}</span>
                                    <span class="event-type">{{ event.type }}</span>
                                    <span class="event-data">{{ event.data }}</span>
                                </div>
                            }
                        }
                    </div>
                    <button mat-button (click)="clearEventLog()">Clear Log</button>
                </mat-card-content>
            </mat-card>
        </div>
    `,
    styles: [`
        .demo-container {
            padding: 20px;
            max-width: 1200px;
            margin: 0 auto;
        }

        .config-card {
            margin-bottom: 20px;
        }

        .config-form {
            display: flex;
            gap: 16px;
            align-items: center;
            flex-wrap: wrap;
        }

        .demo-sections {
            display: flex;
            flex-direction: column;
            gap: 20px;
        }

        .demo-section {
            margin-bottom: 20px;
        }

        .event-log {
            margin-top: 20px;
        }

        .log-container {
            max-height: 300px;
            overflow-y: auto;
            border: 1px solid #e0e0e0;
            border-radius: 4px;
            padding: 8px;
            background: #fafafa;
            margin-bottom: 16px;
        }

        .log-entry {
            display: flex;
            gap: 12px;
            padding: 4px 0;
            border-bottom: 1px solid #f0f0f0;
            font-family: monospace;
            font-size: 0.85rem;
        }

        .log-entry:last-child {
            border-bottom: none;
        }

        .timestamp {
            color: #666;
            min-width: 60px;
        }

        .event-type {
            color: #1976d2;
            font-weight: 500;
            min-width: 120px;
        }

        .event-data {
            color: #333;
            flex: 1;
        }
    `]
})
export class HistoricalResultsDemoComponent {
    private dialog = inject(MatDialog);
    private fb = inject(FormBuilder);

    // Signals for component state
    private _currentConfig = signal<HistoricalResultsConfig | null>(null);
    private _eventLog = signal<Array<{ timestamp: Date, type: string, data: string }>>([]);

    // Public readonly signals
    readonly currentConfig = this._currentConfig.asReadonly();
    readonly eventLog = this._eventLog.asReadonly();

    // Configuration form
    configForm: FormGroup;

    constructor() {
        this.configForm = this.fb.group({
            sampleId: [null, [Validators.required, Validators.min(1)]],
            testId: [null, [Validators.required, Validators.min(1)]],
            testName: ['', Validators.required]
        });
    }

    updateConfiguration(): void {
        if (this.configForm.valid) {
            const formValue = this.configForm.value;
            const config: HistoricalResultsConfig = {
                sampleId: formValue.sampleId,
                testId: formValue.testId,
                testName: formValue.testName,
                showExtended: false,
                initialCount: 12,
                resizable: true,
                singleScreenMode: false
            };

            this._currentConfig.set(config);
            this.logEvent('Configuration Updated', `Sample: ${config.sampleId}, Test: ${config.testId}, Name: ${config.testName}`);
        }
    }

    onPanelResultSelected(summary: HistoricalResultSummary): void {
        this.logEvent('Panel Result Selected', `Sample ID: ${summary.sampleId}, Tag: ${summary.tagNumber}, Date: ${summary.sampleDate.toLocaleDateString()}`);
    }

    onPanelDetailsRequested(summary: HistoricalResultSummary): void {
        this.logEvent('Panel Details Requested', `Sample ID: ${summary.sampleId}`);
        this.openDetailsDialog(summary);
    }

    onExtendedHistoryRequested(data: { sampleId: number, testId: number, testName: string }): void {
        this.logEvent('Extended History Requested', `Sample: ${data.sampleId}, Test: ${data.testId}`);

        // Update config to show extended view
        const currentConfig = this._currentConfig();
        if (currentConfig) {
            this._currentConfig.set({
                ...currentConfig,
                showExtended: true
            });
        }
    }

    onFullResultSelected(result: any): void {
        this.logEvent('Full Result Selected', `Sample ID: ${result?.sampleId || 'Unknown'}`);
    }

    onSingleScreenToggled(enabled: boolean): void {
        this.logEvent('Single Screen Toggled', enabled ? 'Enabled' : 'Disabled');
    }

    private openDetailsDialog(summary: HistoricalResultSummary): void {
        const config = this._currentConfig();
        if (!config) return;

        const dialogData: HistoricalResultDetailsDialogData = {
            summary,
            sampleId: config.sampleId,
            testId: config.testId,
            testName: config.testName
        };

        const dialogRef = this.dialog.open(HistoricalResultDetailsDialogComponent, {
            width: '800px',
            maxWidth: '90vw',
            maxHeight: '90vh',
            data: dialogData
        });

        dialogRef.afterClosed().subscribe(result => {
            this.logEvent('Details Dialog Closed', result ? 'With result' : 'Without result');
        });
    }

    private logEvent(type: string, data: string): void {
        const currentLog = this._eventLog();
        const newEntry = {
            timestamp: new Date(),
            type,
            data
        };

        // Keep only last 50 entries
        const updatedLog = [newEntry, ...currentLog].slice(0, 50);
        this._eventLog.set(updatedLog);
    }

    clearEventLog(): void {
        this._eventLog.set([]);
    }
}