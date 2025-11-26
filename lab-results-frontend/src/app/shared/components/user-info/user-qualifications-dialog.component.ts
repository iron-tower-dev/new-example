import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { UserInfo, UserQualification } from '../../models/auth.model';

interface DialogData {
    user: UserInfo;
    qualifications: UserQualification[];
}

@Component({
    selector: 'app-user-qualifications-dialog',
    standalone: true,
    imports: [
        CommonModule,
        MatDialogModule,
        MatButtonModule,
        MatIconModule,
        MatTableModule,
        MatChipsModule
    ],
    template: `
    <h2 mat-dialog-title>
      <mat-icon>verified_user</mat-icon>
      Test Qualifications - {{ data.user.fullName }}
    </h2>

    <mat-dialog-content>
      @if (data.qualifications.length > 0) {
        <div class="qualifications-table">
          <table mat-table [dataSource]="data.qualifications" class="full-width">
            <ng-container matColumnDef="testStand">
              <th mat-header-cell *matHeaderCellDef>Test</th>
              <td mat-cell *matCellDef="let qualification">
                <div class="test-info">
                  <div class="test-name">{{ qualification.testStand }}</div>
                  @if (qualification.testStandId) {
                    <div class="test-id">ID: {{ qualification.testStandId }}</div>
                  }
                </div>
              </td>
            </ng-container>

            <ng-container matColumnDef="qualificationLevel">
              <th mat-header-cell *matHeaderCellDef>Qualification Level</th>
              <td mat-cell *matCellDef="let qualification">
                <mat-chip 
                  [color]="getQualificationColor(qualification.qualificationLevel)"
                  class="qualification-chip">
                  {{ qualification.qualificationLevel }}
                </mat-chip>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        </div>

        <div class="qualification-legend">
          <h4>Qualification Levels:</h4>
          <div class="legend-items">
            <div class="legend-item">
              <mat-chip color="warn">MicrE</mat-chip>
              <span>Microscopy Expert - Highest level, can perform all tests</span>
            </div>
            <div class="legend-item">
              <mat-chip color="primary">Q/QAG</mat-chip>
              <span>Qualified/Quality Assurance Group - Can perform most tests</span>
            </div>
            <div class="legend-item">
              <mat-chip color="accent">TRAIN</mat-chip>
              <span>Training - Basic level, limited test access</span>
            </div>
          </div>
        </div>
      } @else {
        <div class="no-qualifications">
          <mat-icon>info</mat-icon>
          <p>No test qualifications found for this user.</p>
        </div>
      }
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="close()">Close</button>
    </mat-dialog-actions>
  `,
    styles: [`
    .qualifications-table {
      margin: 20px 0;
    }

    .full-width {
      width: 100%;
    }

    .test-info {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .test-name {
      font-weight: 500;
      font-size: 14px;
    }

    .test-id {
      font-size: 12px;
      color: #666;
    }

    .qualification-chip {
      font-size: 12px;
      height: 28px;
    }

    .qualification-legend {
      margin-top: 24px;
      padding: 16px;
      background-color: #f5f5f5;
      border-radius: 8px;
    }

    .qualification-legend h4 {
      margin: 0 0 12px 0;
      font-size: 14px;
      font-weight: 600;
    }

    .legend-items {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 12px;
      font-size: 13px;
    }

    .legend-item mat-chip {
      min-width: 60px;
      font-size: 11px;
      height: 24px;
    }

    .no-qualifications {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 40px 20px;
      text-align: center;
      color: #666;
    }

    .no-qualifications mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 16px;
      color: #ccc;
    }

    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 0;
    }
  `]
})
export class UserQualificationsDialogComponent {
    displayedColumns: string[] = ['testStand', 'qualificationLevel'];

    constructor(
        public dialogRef: MatDialogRef<UserQualificationsDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: DialogData
    ) { }

    getQualificationColor(level?: string): 'primary' | 'accent' | 'warn' {
        switch (level) {
            case 'MicrE':
                return 'warn';
            case 'Q/QAG':
                return 'primary';
            case 'TRAIN':
                return 'accent';
            default:
                return 'primary';
        }
    }

    close(): void {
        this.dialogRef.close();
    }
}