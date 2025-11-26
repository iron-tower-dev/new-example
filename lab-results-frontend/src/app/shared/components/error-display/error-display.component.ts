import { Component, Input, Output, EventEmitter, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatChipsModule } from '@angular/material/chips';
import { ValidationError } from '../../services/validation.service';

export interface ErrorDisplayConfig {
    showDetails?: boolean;
    showDismiss?: boolean;
    showRetry?: boolean;
    collapsible?: boolean;
    maxErrors?: number;
}

@Component({
    selector: 'app-error-display',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatIconModule,
        MatButtonModule,
        MatExpansionModule,
        MatChipsModule
    ],
    template: `
        <div class="error-display-container">
            <!-- Validation Errors -->
            @if (validationErrors().length > 0) {
                <mat-card class="error-card validation-errors">
                    <mat-card-header>
                        <div class="error-header">
                            <mat-icon class="error-icon">error</mat-icon>
                            <div class="error-title">
                                <h3>Validation Errors ({{ validationErrors().length }})</h3>
                                <p>Please correct the following errors before proceeding</p>
                            </div>
                            @if (config.showDismiss) {
                                <button mat-icon-button (click)="onDismiss.emit('errors')" class="dismiss-button">
                                    <mat-icon>close</mat-icon>
                                </button>
                            }
                        </div>
                    </mat-card-header>
                    
                    <mat-card-content>
                        @if (config.collapsible) {
                            <mat-expansion-panel>
                                <mat-expansion-panel-header>
                                    <mat-panel-title>View Error Details</mat-panel-title>
                                </mat-expansion-panel-header>
                                <div class="error-list">
                                    @for (error of displayedValidationErrors(); track error.field) {
                                        <div class="error-item validation-error">
                                            <mat-icon class="item-icon">warning</mat-icon>
                                            <div class="error-content">
                                                <strong>{{ getFieldDisplayName(error.field) }}</strong>
                                                <span class="error-message">{{ error.message }}</span>
                                            </div>
                                        </div>
                                    }
                                </div>
                            </mat-expansion-panel>
                        } @else {
                            <div class="error-list">
                                @for (error of displayedValidationErrors(); track error.field) {
                                    <div class="error-item validation-error">
                                        <mat-icon class="item-icon">warning</mat-icon>
                                        <div class="error-content">
                                            <strong>{{ getFieldDisplayName(error.field) }}</strong>
                                            <span class="error-message">{{ error.message }}</span>
                                        </div>
                                    </div>
                                }
                            </div>
                        }
                        
                        @if (hasMoreValidationErrors()) {
                            <div class="more-errors">
                                <button mat-button color="primary" (click)="showAllValidationErrors.set(!showAllValidationErrors())">
                                    @if (showAllValidationErrors()) {
                                        Show Less
                                    } @else {
                                        Show {{ validationErrors().length - (config.maxErrors || 5) }} More Errors
                                    }
                                </button>
                            </div>
                        }
                    </mat-card-content>
                </mat-card>
            }

            <!-- Validation Warnings -->
            @if (validationWarnings().length > 0) {
                <mat-card class="warning-card validation-warnings">
                    <mat-card-header>
                        <div class="warning-header">
                            <mat-icon class="warning-icon">warning</mat-icon>
                            <div class="warning-title">
                                <h3>Validation Warnings ({{ validationWarnings().length }})</h3>
                                <p>Please review the following warnings</p>
                            </div>
                            @if (config.showDismiss) {
                                <button mat-icon-button (click)="onDismiss.emit('warnings')" class="dismiss-button">
                                    <mat-icon>close</mat-icon>
                                </button>
                            }
                        </div>
                    </mat-card-header>
                    
                    <mat-card-content>
                        <div class="warning-list">
                            @for (warning of displayedValidationWarnings(); track warning.field) {
                                <div class="warning-item validation-warning">
                                    <mat-icon class="item-icon">info</mat-icon>
                                    <div class="warning-content">
                                        <strong>{{ getFieldDisplayName(warning.field) }}</strong>
                                        <span class="warning-message">{{ warning.message }}</span>
                                    </div>
                                </div>
                            }
                        </div>
                        
                        @if (hasMoreValidationWarnings()) {
                            <div class="more-warnings">
                                <button mat-button color="primary" (click)="showAllValidationWarnings.set(!showAllValidationWarnings())">
                                    @if (showAllValidationWarnings()) {
                                        Show Less
                                    } @else {
                                        Show {{ validationWarnings().length - (config.maxErrors || 5) }} More Warnings
                                    }
                                </button>
                            </div>
                        }
                    </mat-card-content>
                </mat-card>
            }

            <!-- General Error Message -->
            @if (errorMessage()) {
                <mat-card class="error-card general-error">
                    <mat-card-header>
                        <div class="error-header">
                            <mat-icon class="error-icon">error_outline</mat-icon>
                            <div class="error-title">
                                <h3>Error</h3>
                            </div>
                            @if (config.showDismiss) {
                                <button mat-icon-button (click)="onDismiss.emit('general')" class="dismiss-button">
                                    <mat-icon>close</mat-icon>
                                </button>
                            }
                        </div>
                    </mat-card-header>
                    
                    <mat-card-content>
                        <p class="error-message">{{ errorMessage() }}</p>
                        
                        @if (errorDetails() && errorDetails()!.length > 0) {
                            <div class="error-details">
                                <h4>Details:</h4>
                                <ul>
                                    @for (detail of errorDetails(); track detail) {
                                        <li>{{ detail }}</li>
                                    }
                                </ul>
                            </div>
                        }
                        
                        @if (config.showRetry) {
                            <div class="error-actions">
                                <button mat-raised-button color="primary" (click)="onRetry.emit()">
                                    <mat-icon>refresh</mat-icon>
                                    Retry
                                </button>
                            </div>
                        }
                    </mat-card-content>
                </mat-card>
            }

            <!-- Error Summary Chips -->
            @if (showSummary() && (validationErrors().length > 0 || validationWarnings().length > 0)) {
                <div class="error-summary">
                    @if (validationErrors().length > 0) {
                        <mat-chip class="error-chip">
                            <mat-icon matChipAvatar>error</mat-icon>
                            {{ validationErrors().length }} Error{{ validationErrors().length > 1 ? 's' : '' }}
                        </mat-chip>
                    }
                    @if (validationWarnings().length > 0) {
                        <mat-chip class="warning-chip">
                            <mat-icon matChipAvatar>warning</mat-icon>
                            {{ validationWarnings().length }} Warning{{ validationWarnings().length > 1 ? 's' : '' }}
                        </mat-chip>
                    }
                </div>
            }
        </div>
    `,
    styles: [`
        .error-display-container {
            display: flex;
            flex-direction: column;
            gap: 16px;
            margin-bottom: 16px;
        }

        .error-card {
            border-left: 4px solid #f44336;
            background-color: #ffebee;
        }

        .warning-card {
            border-left: 4px solid #ff9800;
            background-color: #fff3e0;
        }

        .error-header, .warning-header {
            display: flex;
            align-items: flex-start;
            gap: 12px;
            width: 100%;
        }

        .error-icon {
            color: #f44336;
            margin-top: 2px;
        }

        .warning-icon {
            color: #ff9800;
            margin-top: 2px;
        }

        .error-title, .warning-title {
            flex: 1;
        }

        .error-title h3, .warning-title h3 {
            margin: 0 0 4px 0;
            font-size: 1.1em;
            font-weight: 500;
        }

        .error-title p, .warning-title p {
            margin: 0;
            color: #666;
            font-size: 0.9em;
        }

        .dismiss-button {
            margin-top: -8px;
        }

        .error-list, .warning-list {
            display: flex;
            flex-direction: column;
            gap: 12px;
        }

        .error-item, .warning-item {
            display: flex;
            align-items: flex-start;
            gap: 8px;
            padding: 8px;
            border-radius: 4px;
            background-color: rgba(255, 255, 255, 0.7);
        }

        .validation-error .item-icon {
            color: #f44336;
            font-size: 18px;
        }

        .validation-warning .item-icon {
            color: #ff9800;
            font-size: 18px;
        }

        .error-content, .warning-content {
            display: flex;
            flex-direction: column;
            gap: 2px;
            flex: 1;
        }

        .error-content strong, .warning-content strong {
            font-size: 0.9em;
            color: #333;
        }

        .error-message, .warning-message {
            font-size: 0.85em;
            color: #666;
        }

        .more-errors, .more-warnings {
            text-align: center;
            margin-top: 12px;
            padding-top: 12px;
            border-top: 1px solid #e0e0e0;
        }

        .error-details {
            margin-top: 12px;
            padding: 12px;
            background-color: rgba(255, 255, 255, 0.5);
            border-radius: 4px;
        }

        .error-details h4 {
            margin: 0 0 8px 0;
            font-size: 0.9em;
            font-weight: 500;
        }

        .error-details ul {
            margin: 0;
            padding-left: 20px;
        }

        .error-details li {
            font-size: 0.85em;
            color: #666;
            margin-bottom: 4px;
        }

        .error-actions {
            margin-top: 16px;
            display: flex;
            gap: 8px;
        }

        .error-summary {
            display: flex;
            gap: 8px;
            flex-wrap: wrap;
        }

        .error-chip {
            background-color: #ffebee !important;
            color: #f44336 !important;
        }

        .warning-chip {
            background-color: #fff3e0 !important;
            color: #ff9800 !important;
        }

        @media (max-width: 768px) {
            .error-header, .warning-header {
                flex-direction: column;
                align-items: flex-start;
            }

            .dismiss-button {
                align-self: flex-end;
                margin-top: -40px;
            }
        }
    `]
})
export class ErrorDisplayComponent {
    @Input() validationErrors = signal<ValidationError[]>([]);
    @Input() validationWarnings = signal<ValidationError[]>([]);
    @Input() errorMessage = signal<string | null>(null);
    @Input() errorDetails = signal<string[] | null>(null);
    @Input() showSummary = signal<boolean>(false);
    @Input() config: ErrorDisplayConfig = {
        showDetails: true,
        showDismiss: true,
        showRetry: false,
        collapsible: false,
        maxErrors: 5
    };

    @Output() onDismiss = new EventEmitter<'errors' | 'warnings' | 'general'>();
    @Output() onRetry = new EventEmitter<void>();

    // Component state
    showAllValidationErrors = signal<boolean>(false);
    showAllValidationWarnings = signal<boolean>(false);

    // Computed properties
    displayedValidationErrors = computed(() => {
        const errors = this.validationErrors();
        const maxErrors = this.config.maxErrors || 5;

        if (this.showAllValidationErrors() || errors.length <= maxErrors) {
            return errors;
        }

        return errors.slice(0, maxErrors);
    });

    displayedValidationWarnings = computed(() => {
        const warnings = this.validationWarnings();
        const maxWarnings = this.config.maxErrors || 5;

        if (this.showAllValidationWarnings() || warnings.length <= maxWarnings) {
            return warnings;
        }

        return warnings.slice(0, maxWarnings);
    });

    hasMoreValidationErrors = computed(() => {
        return this.validationErrors().length > (this.config.maxErrors || 5) && !this.showAllValidationErrors();
    });

    hasMoreValidationWarnings = computed(() => {
        return this.validationWarnings().length > (this.config.maxErrors || 5) && !this.showAllValidationWarnings();
    });

    /**
     * Convert field path to user-friendly display name
     */
    getFieldDisplayName(fieldPath: string): string {
        // Handle array notation like "trials[0].sampleWeight"
        const cleanPath = fieldPath.replace(/\[\d+\]/g, '');

        // Split by dots and get the last part
        const parts = cleanPath.split('.');
        const fieldName = parts[parts.length - 1];

        // Convert camelCase to Title Case
        const displayName = fieldName
            .replace(/([A-Z])/g, ' $1')
            .replace(/^./, str => str.toUpperCase())
            .trim();

        // Add trial information if present
        const trialMatch = fieldPath.match(/\[(\d+)\]/);
        if (trialMatch) {
            const trialNumber = parseInt(trialMatch[1]) + 1;
            return `Trial ${trialNumber} - ${displayName}`;
        }

        return displayName;
    }
}