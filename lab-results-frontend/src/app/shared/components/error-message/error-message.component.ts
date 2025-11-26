import { Component, Input } from '@angular/core';
import { SharedModule } from '../../shared.module';

@Component({
    selector: 'app-error-message',
    standalone: true,
    imports: [SharedModule],
    template: `
    @if (message) {
      <mat-card class="error-card">
        <mat-card-content>
          <div class="error-content">
            <mat-icon color="warn">error</mat-icon>
            <span class="error-text">{{ message }}</span>
          </div>
        </mat-card-content>
      </mat-card>
    }
  `,
    styles: [`
    .error-card {
      background-color: #ffebee;
      border-left: 4px solid #f44336;
      margin: 16px 0;
    }
    
    .error-content {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .error-text {
      color: #c62828;
      font-weight: 500;
    }
  `]
})
export class ErrorMessageComponent {
    @Input() message?: string;
}