import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { LookupService, EquipmentSelectionDto, CommentDto } from '../../services/lookup.service';

export type LookupType = 'nas' | 'nlgi' | 'equipment' | 'particle-type' | 'comment';

@Component({
  selector: 'app-lookup-display',
  standalone: true,
  imports: [
    CommonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule
  ],
  template: `
    <div class="lookup-display-container">
      @if (lookupType === 'nas') {
        <div class="nas-lookup">
          <div class="lookup-header">
            <span class="lookup-label">NAS Value:</span>
            @if (isLoading()) {
              <mat-spinner diameter="20"></mat-spinner>
            } @else {
              <button mat-icon-button 
                      (click)="refreshLookup()" 
                      matTooltip="Refresh NAS lookup"
                      [disabled]="isLoading()">
                <mat-icon>refresh</mat-icon>
              </button>
            }
          </div>
          <div class="lookup-result">
            @if (nasResult(); as result) {
              <span class="nas-value" [class.error]="!result.isValid">
                {{ result.isValid ? result.highestNAS : 'Error' }}
              </span>
              @if (result.errorMessage) {
                <div class="error-message">{{ result.errorMessage }}</div>
              }
            } @else {
              <span class="no-result">-</span>
            }
          </div>
        </div>
      }

      @if (lookupType === 'nlgi') {
        <div class="nlgi-lookup">
          <div class="lookup-header">
            <span class="lookup-label">NLGI Grade:</span>
            @if (isLoading()) {
              <mat-spinner diameter="20"></mat-spinner>
            } @else {
              <button mat-icon-button 
                      (click)="refreshLookup()" 
                      matTooltip="Refresh NLGI lookup"
                      [disabled]="isLoading()">
                <mat-icon>refresh</mat-icon>
              </button>
            }
          </div>
          <div class="lookup-result">
            @if (nlgiResult(); as result) {
              <span class="nlgi-value">{{ result }}</span>
            } @else {
              <span class="no-result">-</span>
            }
          </div>
        </div>
      }

      @if (lookupType === 'equipment') {
        <div class="equipment-lookup">
          <mat-form-field appearance="outline" class="equipment-select">
            <mat-label>{{ equipmentLabel || 'Select Equipment' }}</mat-label>
            <mat-select [value]="selectedEquipmentId()" 
                       (selectionChange)="onEquipmentSelectionChange($event.value)"
                       [disabled]="isLoading()">
              @if (isLoading()) {
                <mat-option disabled>
                  <mat-spinner diameter="20"></mat-spinner>
                  Loading equipment...
                </mat-option>
              } @else {
                @for (equipment of equipmentOptions(); track equipment.id) {
                  <mat-option [value]="equipment.id" 
                             [class.overdue]="equipment.isOverdue"
                             [class.due-soon]="equipment.isDueSoon">
                    {{ equipment.displayText }}
                    @if (equipment.calibrationValue) {
                      <span class="calibration-value">({{ equipment.calibrationValue }})</span>
                    }
                  </mat-option>
                }
              }
            </mat-select>
            @if (hasError()) {
              <mat-error>{{ error() }}</mat-error>
            }
          </mat-form-field>
          @if (selectedEquipmentCalibration(); as calibration) {
            <div class="calibration-info">
              <span class="calibration-label">Calibration:</span>
              <span class="calibration-value">{{ calibration }}</span>
            </div>
          }
        </div>
      }

      @if (lookupType === 'comment') {
        <div class="comment-lookup">
          <mat-form-field appearance="outline" class="comment-select">
            <mat-label>{{ commentLabel || 'Select Comment' }}</mat-label>
            <mat-select [value]="selectedCommentId()" 
                       (selectionChange)="onCommentSelectionChange($event.value)"
                       [disabled]="isLoading()">
              @if (isLoading()) {
                <mat-option disabled>
                  <mat-spinner diameter="20"></mat-spinner>
                  Loading comments...
                </mat-option>
              } @else {
                @for (comment of commentOptions(); track comment.id) {
                  <mat-option [value]="comment.id">
                    {{ comment.remark }}
                  </mat-option>
                }
              }
            </mat-select>
            @if (hasError()) {
              <mat-error>{{ error() }}</mat-error>
            }
          </mat-form-field>
        </div>
      }

      @if (hasError() && lookupType !== 'equipment' && lookupType !== 'comment') {
        <div class="error-display">
          <mat-icon color="warn">error</mat-icon>
          <span>{{ error() }}</span>
          <button mat-icon-button (click)="clearError()" matTooltip="Clear error">
            <mat-icon>close</mat-icon>
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .lookup-display-container {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .lookup-header {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .lookup-label {
      font-weight: 500;
      color: rgba(0, 0, 0, 0.87);
    }

    .lookup-result {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .nas-value, .nlgi-value {
      font-weight: 600;
      font-size: 1.1em;
      color: #2e7d32;
    }

    .nas-value.error {
      color: #d32f2f;
    }

    .no-result {
      color: rgba(0, 0, 0, 0.54);
      font-style: italic;
    }

    .equipment-select {
      min-width: 250px;
    }

    .comment-select {
      min-width: 300px;
    }

    .calibration-info {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 0.9em;
      color: rgba(0, 0, 0, 0.7);
    }

    .calibration-value {
      font-weight: 500;
    }

    .error-display {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px;
      background-color: #ffebee;
      border-radius: 4px;
      color: #d32f2f;
      font-size: 0.9em;
    }

    .error-message {
      color: #d32f2f;
      font-size: 0.8em;
      margin-top: 4px;
    }

    ::ng-deep .mat-mdc-option.overdue {
      background-color: #ffebee !important;
      color: #d32f2f !important;
    }

    ::ng-deep .mat-mdc-option.due-soon {
      background-color: #fff3e0 !important;
      color: #f57c00 !important;
    }

    ::ng-deep .calibration-value {
      font-size: 0.8em;
      color: rgba(0, 0, 0, 0.6);
      margin-left: 8px;
    }
  `]
})
export class LookupDisplayComponent implements OnInit, OnDestroy {
  private lookupService = inject(LookupService);
  private destroy$ = new Subject<void>();

  // Input properties
  @Input() lookupType: LookupType = 'nas';
  @Input() equipmentType?: string;
  @Input() testId?: number;
  @Input() equipmentLabel?: string;
  @Input() commentArea?: string;
  @Input() commentType?: string;
  @Input() commentLabel?: string;
  @Input() particleCounts?: { [channel: number]: number };
  @Input() penetrationValue?: number;
  @Input() autoTrigger = true;

  // Output events
  @Output() nasResultEvent = new EventEmitter<{ isValid: boolean; highestNAS: number; channelNASValues: { [channel: number]: number }; errorMessage?: string }>();
  @Output() nlgiResultEvent = new EventEmitter<string>();
  @Output() equipmentSelected = new EventEmitter<{ equipmentId: number; calibrationValue?: number }>();
  @Output() commentSelected = new EventEmitter<{ commentId: number; remark: string }>();
  @Output() lookupError = new EventEmitter<string>();

  // Signals for component state
  private _nasResult = signal<{ isValid: boolean; highestNAS: number; channelNASValues: { [channel: number]: number }; errorMessage?: string } | null>(null);
  private _nlgiResult = signal<string | null>(null);
  private _equipmentOptions = signal<EquipmentSelectionDto[]>([]);
  private _commentOptions = signal<CommentDto[]>([]);
  private _selectedEquipmentId = signal<number | null>(null);
  private _selectedCommentId = signal<number | null>(null);
  private _selectedEquipmentCalibration = signal<number | null>(null);

  // Public readonly signals
  readonly nasResult = this._nasResult.asReadonly();
  readonly nlgiResult = this._nlgiResult.asReadonly();
  readonly equipmentOptions = this._equipmentOptions.asReadonly();
  readonly commentOptions = this._commentOptions.asReadonly();
  readonly selectedEquipmentId = this._selectedEquipmentId.asReadonly();
  readonly selectedCommentId = this._selectedCommentId.asReadonly();
  readonly selectedEquipmentCalibration = this._selectedEquipmentCalibration.asReadonly();
  readonly isLoading = this.lookupService.isLoading;
  readonly error = this.lookupService.error;
  readonly hasError = this.lookupService.hasError;

  // Effects for automatic lookup triggers
  private particleCountsEffect = effect(() => {
    if (this.autoTrigger && this.lookupType === 'nas' && this.particleCounts) {
      this.triggerNASLookup();
    }
  });

  private penetrationValueEffect = effect(() => {
    if (this.autoTrigger && this.lookupType === 'nlgi' && this.penetrationValue) {
      this.triggerNLGILookup();
    }
  });

  ngOnInit(): void {
    this.initializeLookup();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeLookup(): void {
    switch (this.lookupType) {
      case 'equipment':
        this.loadEquipmentOptions();
        break;
      case 'comment':
        this.loadCommentOptions();
        break;
      case 'nas':
        if (this.autoTrigger && this.particleCounts) {
          this.triggerNASLookup();
        }
        break;
      case 'nlgi':
        if (this.autoTrigger && this.penetrationValue) {
          this.triggerNLGILookup();
        }
        break;
    }
  }

  private loadEquipmentOptions(): void {
    if (!this.equipmentType) return;

    this.lookupService.getCachedEquipmentByType(this.equipmentType, this.testId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (equipment) => {
          this._equipmentOptions.set(equipment);
        },
        error: (error) => {
          console.error('Error loading equipment options:', error);
          this.lookupError.emit(error.message || 'Error loading equipment');
        }
      });
  }

  private loadCommentOptions(): void {
    if (!this.commentArea) return;

    const request = this.commentType
      ? this.lookupService.getCommentsByAreaAndType(this.commentArea, this.commentType)
      : this.lookupService.getCommentsByArea(this.commentArea);

    request.pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (comments) => {
          this._commentOptions.set(comments);
        },
        error: (error) => {
          console.error('Error loading comment options:', error);
          this.lookupError.emit(error.message || 'Error loading comments');
        }
      });
  }

  private triggerNASLookup(): void {
    if (!this.particleCounts) return;

    this.lookupService.triggerNASLookup(this.particleCounts)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (highestNAS) => {
          const result = {
            isValid: true,
            highestNAS,
            channelNASValues: {} // This would need to be populated from the full result
          };
          this._nasResult.set(result);
          this.nasResultEvent.emit(result);
        },
        error: (error) => {
          const result = {
            isValid: false,
            highestNAS: 0,
            channelNASValues: {},
            errorMessage: error.message || 'Error calculating NAS'
          };
          this._nasResult.set(result);
          this.nasResultEvent.emit(result);
          this.lookupError.emit(error.message || 'Error calculating NAS');
        }
      });
  }

  private triggerNLGILookup(): void {
    if (!this.penetrationValue) return;

    this.lookupService.triggerNLGILookup(this.penetrationValue)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (nlgiValue) => {
          this._nlgiResult.set(nlgiValue);
          this.nlgiResultEvent.emit(nlgiValue);
        },
        error: (error) => {
          this._nlgiResult.set(null);
          this.lookupError.emit(error.message || 'Error retrieving NLGI value');
        }
      });
  }

  onEquipmentSelectionChange(equipmentId: number): void {
    this._selectedEquipmentId.set(equipmentId);

    // Get calibration value for selected equipment
    this.lookupService.triggerEquipmentCalibrationLookup(equipmentId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (calibrationValue) => {
          this._selectedEquipmentCalibration.set(calibrationValue);
          this.equipmentSelected.emit({ equipmentId, calibrationValue: calibrationValue || undefined });
        },
        error: (error) => {
          console.error('Error getting equipment calibration:', error);
          this.equipmentSelected.emit({ equipmentId });
        }
      });
  }

  onCommentSelectionChange(commentId: number): void {
    this._selectedCommentId.set(commentId);

    const selectedComment = this._commentOptions().find(c => c.id === commentId);
    if (selectedComment) {
      this.commentSelected.emit({ commentId, remark: selectedComment.remark });
    }
  }

  refreshLookup(): void {
    this.lookupService.clearError();

    switch (this.lookupType) {
      case 'nas':
        if (this.particleCounts) {
          this.triggerNASLookup();
        }
        break;
      case 'nlgi':
        if (this.penetrationValue) {
          this.triggerNLGILookup();
        }
        break;
      case 'equipment':
        this.lookupService.refreshEquipmentCache().subscribe(() => {
          this.loadEquipmentOptions();
        });
        break;
      case 'comment':
        this.lookupService.refreshCommentCache().subscribe(() => {
          this.loadCommentOptions();
        });
        break;
    }
  }

  clearError(): void {
    this.lookupService.clearError();
  }
}