import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';

interface SampleSelectionData {
    testRoute: string;
    testName: string;
}

interface Sample {
    id: string;
    customerName: string;
    sampleType: string;
    receivedDate: Date;
    status: string;
}

@Component({
    selector: 'app-sample-selection-dialog',
    standalone: true,
    imports: [
        CommonModule,
        MatDialogModule,
        MatButtonModule,
        MatListModule,
        MatIconModule
    ],
    template: `
    <h2 mat-dialog-title>Select Sample for {{ data.testName }}</h2>
    
    <mat-dialog-content>
      <p>Choose a sample to perform the {{ data.testName }} test:</p>
      
      <mat-selection-list #sampleList (selectionChange)="onSampleSelection($event)">
        <mat-list-option *ngFor="let sample of availableSamples" [value]="sample.id">
          <mat-icon matListItemIcon>science</mat-icon>
          <div matListItemTitle>{{ sample.customerName }}</div>
          <div matListItemLine>{{ sample.sampleType }} - Received: {{ sample.receivedDate | date:'short' }}</div>
          <div matListItemLine>Status: {{ sample.status }}</div>
        </mat-list-option>
      </mat-selection-list>
      
      <div *ngIf="availableSamples.length === 0" class="no-samples">
        <mat-icon>info</mat-icon>
        <p>No samples available for testing. Please add samples first.</p>
      </div>
    </mat-dialog-content>
    
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-button (click)="goToSamples()">Manage Samples</button>
      <button 
        mat-raised-button 
        color="primary" 
        [disabled]="!selectedSampleId"
        (click)="onProceed()">
        Proceed to Test
      </button>
    </mat-dialog-actions>
  `,
    styles: [`
    mat-dialog-content {
      min-width: 400px;
      max-height: 400px;
    }
    
    .no-samples {
      text-align: center;
      padding: 24px;
      color: #666;
    }
    
    .no-samples mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 16px;
    }
  `]
})
export class SampleSelectionDialogComponent {
    selectedSampleId: string | null = null;

    // Mock sample data - in real implementation, this would come from a service
    availableSamples: Sample[] = [
        {
            id: 'S001',
            customerName: 'Acme Corp - Engine Oil',
            sampleType: 'Lubricating Oil',
            receivedDate: new Date('2024-11-10'),
            status: 'Ready for Testing'
        },
        {
            id: 'S002',
            customerName: 'Beta Industries - Hydraulic Fluid',
            sampleType: 'Hydraulic Oil',
            receivedDate: new Date('2024-11-11'),
            status: 'Ready for Testing'
        },
        {
            id: 'S003',
            customerName: 'Gamma LLC - Gear Oil',
            sampleType: 'Gear Oil',
            receivedDate: new Date('2024-11-12'),
            status: 'Ready for Testing'
        }
    ];

    constructor(
        private dialogRef: MatDialogRef<SampleSelectionDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: SampleSelectionData,
        private router: Router
    ) { }

    onSampleSelection(event: any) {
        this.selectedSampleId = event.option.value;
    }

    onCancel() {
        this.dialogRef.close();
    }

    goToSamples() {
        this.dialogRef.close();
        this.router.navigate(['/samples']);
    }

    onProceed() {
        if (this.selectedSampleId) {
            this.dialogRef.close();
            this.router.navigate(['/tests', this.data.testRoute, this.selectedSampleId]);
        }
    }
}