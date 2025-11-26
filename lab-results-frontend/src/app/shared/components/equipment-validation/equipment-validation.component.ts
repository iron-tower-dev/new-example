import { Component, Input, signal, computed } from '@angular/core';
import { EquipmentValidationResult } from '../../models/equipment.model';

@Component({
    selector: 'app-equipment-validation',
    standalone: false,
    template: `
        @if (validationResult() && shouldShowWarning()) {
            <div class="validation-container" 
                 [class.error]="validationResult()!.isOverdue"
                 [class.warning]="validationResult()!.isDueSoon && !validationResult()!.isOverdue">
                
                <mat-icon class="validation-icon">
                    {{ getValidationIcon() }}
                </mat-icon>
                
                <div class="validation-content">
                    <div class="validation-message">
                        {{ validationResult()!.message }}
                    </div>
                    
                    @if (validationResult()!.dueDate && showDueDate) {
                        <div class="due-date">
                            Due: {{ validationResult()!.dueDate | date:'MM/dd/yyyy' }}
                        </div>
                    }
                    
                    @if (validationResult()!.daysUntilDue !== undefined && showDaysRemaining) {
                        <div class="days-remaining">
                            @if (validationResult()!.daysUntilDue < 0) {
                                {{ Math.abs(validationResult()!.daysUntilDue) }} days overdue
                            } @else if (validationResult()!.daysUntilDue === 0) {
                                Due today
                            } @else {
                                {{ validationResult()!.daysUntilDue }} days remaining
                            }
                        </div>
                    }
                </div>
                
                @if (showDismiss) {
                    <button mat-icon-button 
                            class="dismiss-button"
                            (click)="onDismiss()"
                            [attr.aria-label]="'Dismiss validation message'">
                        <mat-icon>close</mat-icon>
                    </button>
                }
            </div>
        }
    `,
    styles: [`
        .validation-container {
            display: flex;
            align-items: flex-start;
            gap: 12px;
            padding: 12px 16px;
            border-radius: 8px;
            margin: 8px 0;
            border-left: 4px solid;
            background-color: var(--validation-bg);
            border-left-color: var(--validation-border);
            color: var(--validation-text);
        }

        .validation-container.warning {
            --validation-bg: #fff3cd;
            --validation-border: #ffc107;
            --validation-text: #856404;
        }

        .validation-container.error {
            --validation-bg: #f8d7da;
            --validation-border: #dc3545;
            --validation-text: #721c24;
        }

        .validation-icon {
            flex-shrink: 0;
            margin-top: 2px;
            color: var(--validation-border);
        }

        .validation-content {
            flex: 1;
            min-width: 0;
        }

        .validation-message {
            font-weight: 500;
            margin-bottom: 4px;
        }

        .due-date,
        .days-remaining {
            font-size: 0.875rem;
            opacity: 0.8;
            margin-top: 2px;
        }

        .dismiss-button {
            flex-shrink: 0;
            width: 32px;
            height: 32px;
            margin: -4px -4px -4px 0;
        }

        .dismiss-button mat-icon {
            font-size: 18px;
            width: 18px;
            height: 18px;
        }

        /* Compact mode */
        .validation-container.compact {
            padding: 8px 12px;
            gap: 8px;
        }

        .validation-container.compact .validation-message {
            font-size: 0.875rem;
            margin-bottom: 0;
        }

        .validation-container.compact .due-date,
        .validation-container.compact .days-remaining {
            font-size: 0.75rem;
        }
    `]
})
export class EquipmentValidationComponent {
    // Expose Math for template
    Math = Math;

    // Inputs
    @Input() set validation(value: EquipmentValidationResult | null) {
        this._validationResult.set(value);
    }

    @Input() showDueDate: boolean = true;
    @Input() showDaysRemaining: boolean = true;
    @Input() showDismiss: boolean = false;
    @Input() compact: boolean = false;
    @Input() hideValid: boolean = true; // Hide when validation is valid
    @Input() hideWarnings: boolean = false; // Hide warning-level validations
    @Input() hideErrors: boolean = false; // Hide error-level validations

    // Signals
    private readonly _validationResult = signal<EquipmentValidationResult | null>(null);
    private readonly _isDismissed = signal(false);

    // Public readonly signals
    readonly validationResult = this._validationResult.asReadonly();
    readonly isDismissed = this._isDismissed.asReadonly();

    // Computed signals
    readonly shouldShowWarning = computed(() => {
        const result = this._validationResult();
        const dismissed = this._isDismissed();

        if (!result || dismissed) return false;

        // Hide if valid and hideValid is true
        if (result.isValid && !result.isDueSoon && this.hideValid) return false;

        // Hide warnings if hideWarnings is true
        if (result.isDueSoon && !result.isOverdue && this.hideWarnings) return false;

        // Hide errors if hideErrors is true
        if (result.isOverdue && this.hideErrors) return false;

        return true;
    });

    readonly validationLevel = computed(() => {
        const result = this._validationResult();
        if (!result) return 'none';

        if (result.isOverdue) return 'error';
        if (result.isDueSoon) return 'warning';
        if (!result.isValid) return 'error';

        return 'success';
    });

    getValidationIcon(): string {
        const result = this._validationResult();
        if (!result) return 'info';

        if (result.isOverdue) return 'error';
        if (result.isDueSoon) return 'warning';
        if (!result.isValid) return 'error';

        return 'check_circle';
    }

    onDismiss(): void {
        this._isDismissed.set(true);
    }

    // Public methods
    reset(): void {
        this._isDismissed.set(false);
    }

    show(): void {
        this._isDismissed.set(false);
    }

    hide(): void {
        this._isDismissed.set(true);
    }
}