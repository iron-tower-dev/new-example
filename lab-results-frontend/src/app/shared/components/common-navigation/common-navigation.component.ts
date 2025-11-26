import { Component, Input, Output, EventEmitter, inject, signal, computed } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ConfirmationDialogComponent, ConfirmationDialogData } from '../confirmation-dialog/confirmation-dialog.component';
import { SharedModule } from '../../shared.module';

export interface NavigationAction {
  type: 'save' | 'clear' | 'delete' | 'custom';
  label: string;
  icon: string;
  color?: 'primary' | 'accent' | 'warn';
  disabled?: boolean;
  shortcut?: string;
}

export interface NavigationState {
  hasUnsavedChanges: boolean;
  isLoading: boolean;
  canSave: boolean;
  canDelete: boolean;
  selectedItems?: any[];
}

@Component({
  selector: 'app-common-navigation',
  standalone: true,
  imports: [SharedModule],
  template: `
    <div class="navigation-container">
      <mat-card class="navigation-card">
        <mat-card-content>
          <div class="navigation-content">
            <!-- Primary Actions -->
            <div class="primary-actions">
              <button 
                mat-raised-button 
                color="primary"
                [disabled]="!navigationState().canSave || navigationState().isLoading"
                (click)="onSave()"
                [title]="'Save' + (saveShortcut ? ' (' + saveShortcut + ')' : '')">
                @if (navigationState().isLoading) {
                  <mat-spinner diameter="20"></mat-spinner>
                } @else {
                  <mat-icon>save</mat-icon>
                }
                Save
              </button>

              <button 
                mat-button
                [disabled]="navigationState().isLoading"
                (click)="onClear()"
                [title]="'Clear' + (clearShortcut ? ' (' + clearShortcut + ')' : '')">
                <mat-icon>clear_all</mat-icon>
                Clear
              </button>

              @if (showDelete) {
                <button 
                  mat-button 
                  color="warn"
                  [disabled]="!navigationState().canDelete || navigationState().isLoading"
                  (click)="onDelete()"
                  [title]="'Delete' + (deleteShortcut ? ' (' + deleteShortcut + ')' : '')">
                  <mat-icon>delete</mat-icon>
                  Delete
                </button>
              }
            </div>

            <!-- Custom Actions -->
            @if (customActions.length > 0) {
              <div class="custom-actions">
                @for (action of customActions; track action.label) {
                  <button 
                    mat-button
                    [color]="action.color || 'primary'"
                    [disabled]="action.disabled || navigationState().isLoading"
                    (click)="onCustomAction(action)"
                    [title]="action.label + (action.shortcut ? ' (' + action.shortcut + ')' : '')">
                    <mat-icon>{{ action.icon }}</mat-icon>
                    {{ action.label }}
                  </button>
                }
              </div>
            }

            <!-- Status Information -->
            <div class="status-info">
              @if (navigationState().hasUnsavedChanges) {
                <div class="unsaved-indicator">
                  <mat-icon color="warn">warning</mat-icon>
                  <span>Unsaved changes</span>
                </div>
              }

              @if (navigationState().selectedItems && navigationState().selectedItems!.length > 0) {
                <div class="selection-info">
                  <mat-icon>check_circle</mat-icon>
                  <span>{{ navigationState().selectedItems!.length }} selected</span>
                </div>
              }
            </div>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .navigation-container {
      width: 100%;
      margin-bottom: 16px;
    }

    .navigation-card {
      background-color: #f8f9fa;
      border: 1px solid #e9ecef;
    }

    .navigation-content {
      display: flex;
      align-items: center;
      gap: 16px;
      flex-wrap: wrap;
    }

    .primary-actions,
    .custom-actions {
      display: flex;
      gap: 8px;
      align-items: center;
    }

    .status-info {
      margin-left: auto;
      display: flex;
      gap: 16px;
      align-items: center;
    }

    .unsaved-indicator,
    .selection-info {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 0.9em;
      padding: 4px 8px;
      border-radius: 4px;
    }

    .unsaved-indicator {
      background-color: #fff3e0;
      color: #f57c00;
    }

    .selection-info {
      background-color: #e8f5e8;
      color: #2e7d32;
    }

    mat-spinner {
      margin-right: 8px;
    }

    @media (max-width: 768px) {
      .navigation-content {
        flex-direction: column;
        align-items: stretch;
      }

      .primary-actions,
      .custom-actions {
        justify-content: center;
      }

      .status-info {
        margin-left: 0;
        justify-content: center;
      }
    }

    @media (max-width: 480px) {
      .primary-actions,
      .custom-actions {
        flex-direction: column;
        width: 100%;
      }

      .primary-actions button,
      .custom-actions button {
        width: 100%;
      }
    }
  `]
})
export class CommonNavigationComponent {
  @Input() showDelete: boolean = true;
  @Input() customActions: NavigationAction[] = [];
  @Input() saveShortcut: string = 'Ctrl+S';
  @Input() clearShortcut: string = 'Ctrl+R';
  @Input() deleteShortcut: string = 'Delete';

  @Output() saveClicked = new EventEmitter<void>();
  @Output() clearClicked = new EventEmitter<void>();
  @Output() deleteClicked = new EventEmitter<void>();
  @Output() customActionClicked = new EventEmitter<NavigationAction>();

  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  // Navigation state signal
  private _navigationState = signal<NavigationState>({
    hasUnsavedChanges: false,
    isLoading: false,
    canSave: true,
    canDelete: true
  });

  readonly navigationState = this._navigationState.asReadonly();

  constructor() {
    // Set up keyboard shortcuts
    this.setupKeyboardShortcuts();
  }

  /**
   * Update the navigation state
   */
  updateState(state: Partial<NavigationState>): void {
    this._navigationState.update(current => ({ ...current, ...state }));
  }

  /**
   * Handle save action
   */
  onSave(): void {
    if (this.navigationState().canSave && !this.navigationState().isLoading) {
      this.saveClicked.emit();
    }
  }

  /**
   * Handle clear action with confirmation if there are unsaved changes
   */
  onClear(): void {
    if (this.navigationState().hasUnsavedChanges) {
      this.showConfirmationDialog({
        title: 'Clear Data',
        message: 'You have unsaved changes. Are you sure you want to clear all data?',
        confirmText: 'Clear',
        cancelText: 'Cancel'
      }).then(confirmed => {
        if (confirmed) {
          this.clearClicked.emit();
          this.updateState({ hasUnsavedChanges: false });
        }
      });
    } else {
      this.clearClicked.emit();
    }
  }

  /**
   * Handle delete action with confirmation
   */
  onDelete(): void {
    if (!this.navigationState().canDelete || this.navigationState().isLoading) {
      return;
    }

    const selectedCount = this.navigationState().selectedItems?.length || 0;
    const message = selectedCount > 0
      ? `Are you sure you want to delete ${selectedCount} selected item(s)?`
      : 'Are you sure you want to delete this item?';

    this.showConfirmationDialog({
      title: 'Confirm Delete',
      message: message,
      confirmText: 'Delete',
      cancelText: 'Cancel'
    }).then(confirmed => {
      if (confirmed) {
        this.deleteClicked.emit();
      }
    });
  }

  /**
   * Handle custom action
   */
  onCustomAction(action: NavigationAction): void {
    if (action.disabled || this.navigationState().isLoading) {
      return;
    }

    this.customActionClicked.emit(action);
  }

  /**
   * Show confirmation dialog
   */
  private showConfirmationDialog(data: ConfirmationDialogData): Promise<boolean> {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: data,
      disableClose: true
    });

    return dialogRef.afterClosed().toPromise().then(result => !!result);
  }

  /**
   * Set up keyboard shortcuts
   */
  private setupKeyboardShortcuts(): void {
    document.addEventListener('keydown', (event) => {
      // Save shortcut (Ctrl+S)
      if (event.ctrlKey && event.key === 's') {
        event.preventDefault();
        this.onSave();
      }

      // Clear shortcut (Ctrl+R)
      if (event.ctrlKey && event.key === 'r') {
        event.preventDefault();
        this.onClear();
      }

      // Delete shortcut (Delete key)
      if (event.key === 'Delete' && !this.isInputFocused()) {
        event.preventDefault();
        this.onDelete();
      }

      // Custom action shortcuts
      this.customActions.forEach(action => {
        if (action.shortcut && this.matchesShortcut(event, action.shortcut)) {
          event.preventDefault();
          this.onCustomAction(action);
        }
      });
    });
  }

  /**
   * Check if an input element is currently focused
   */
  private isInputFocused(): boolean {
    const activeElement = document.activeElement;
    return !!(activeElement?.tagName === 'INPUT' ||
      activeElement?.tagName === 'TEXTAREA' ||
      activeElement?.hasAttribute('contenteditable'));
  }

  /**
   * Check if keyboard event matches shortcut
   */
  private matchesShortcut(event: KeyboardEvent, shortcut: string): boolean {
    const parts = shortcut.toLowerCase().split('+');
    const key = parts[parts.length - 1];
    const modifiers = parts.slice(0, -1);

    if (event.key.toLowerCase() !== key) {
      return false;
    }

    return modifiers.every(modifier => {
      switch (modifier) {
        case 'ctrl': return event.ctrlKey;
        case 'alt': return event.altKey;
        case 'shift': return event.shiftKey;
        case 'meta': return event.metaKey;
        default: return false;
      }
    });
  }

  /**
   * Show success message
   */
  showSuccessMessage(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  /**
   * Show error message
   */
  showErrorMessage(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }

  /**
   * Show info message
   */
  showInfoMessage(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['info-snackbar']
    });
  }
}