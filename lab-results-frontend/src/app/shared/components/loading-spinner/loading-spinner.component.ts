import { Component, Input } from '@angular/core';
import { SharedModule } from '../../shared.module';

@Component({
    selector: 'app-loading-spinner',
    standalone: true,
    imports: [SharedModule],
    template: `
    <div class="loading-container" [class.overlay]="overlay">
      <mat-spinner [diameter]="diameter"></mat-spinner>
      @if (message) {
        <p class="loading-message">{{ message }}</p>
      }
    </div>
  `,
    styles: [`
    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 20px;
    }
    
    .loading-container.overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background-color: rgba(255, 255, 255, 0.8);
      z-index: 1000;
    }
    
    .loading-message {
      margin-top: 16px;
      text-align: center;
      color: #666;
    }
  `]
})
export class LoadingSpinnerComponent {
    @Input() diameter: number = 40;
    @Input() message?: string;
    @Input() overlay: boolean = false;
}