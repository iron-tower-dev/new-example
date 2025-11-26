import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-unauthorized',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule
    ],
    template: `
    <div class="unauthorized-container">
      <mat-card class="unauthorized-card">
        <mat-card-header>
          <mat-icon mat-card-avatar color="warn">block</mat-icon>
          <mat-card-title>Access Denied</mat-card-title>
          <mat-card-subtitle>Insufficient Permissions</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          @if (testStandId && requiredLevel) {
            <div class="permission-details">
              <p>You do not have the required qualification to access this test.</p>
              
              <div class="details-grid">
                <div class="detail-item">
                  <strong>Test ID:</strong>
                  <span>{{ testStandId }}</span>
                </div>
                <div class="detail-item">
                  <strong>Required Level:</strong>
                  <span class="required-level">{{ requiredLevel }}</span>
                </div>
                @if (userLevel) {
                  <div class="detail-item">
                    <strong>Your Level:</strong>
                    <span class="user-level">{{ userLevel }}</span>
                  </div>
                } @else {
                  <div class="detail-item">
                    <strong>Your Level:</strong>
                    <span class="no-qualification">No qualification for this test</span>
                  </div>
                }
              </div>

              <div class="qualification-info">
                <h4>Qualification Hierarchy:</h4>
                <ul>
                  <li><strong>MicrE</strong> - Microscopy Expert (highest level)</li>
                  <li><strong>Q/QAG</strong> - Qualified/Quality Assurance Group</li>
                  <li><strong>TRAIN</strong> - Training level (basic access)</li>
                </ul>
              </div>
            </div>
          } @else {
            <p>You do not have permission to access this resource.</p>
            <p>Please contact your supervisor or system administrator if you believe this is an error.</p>
          }

          @if (authService.currentUser()) {
            <div class="user-info">
              <p><strong>Current User:</strong> {{ authService.currentUser()?.fullName }} ({{ authService.currentUser()?.employeeId }})</p>
              <p><strong>Role:</strong> {{ authService.currentUser()?.role }}</p>
            </div>
          }
        </mat-card-content>

        <mat-card-actions align="end">
          <button mat-button (click)="goBack()">
            <mat-icon>arrow_back</mat-icon>
            Go Back
          </button>
          <button mat-raised-button color="primary" (click)="goHome()">
            <mat-icon>home</mat-icon>
            Home
          </button>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
    styles: [`
    .unauthorized-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background-color: #f5f5f5;
      padding: 20px;
    }

    .unauthorized-card {
      width: 100%;
      max-width: 600px;
      padding: 20px;
    }

    .permission-details {
      margin: 20px 0;
    }

    .details-grid {
      display: grid;
      grid-template-columns: auto 1fr;
      gap: 12px 20px;
      margin: 20px 0;
      padding: 16px;
      background-color: #f9f9f9;
      border-radius: 8px;
    }

    .detail-item {
      display: contents;
    }

    .detail-item strong {
      color: #333;
      font-weight: 600;
    }

    .required-level {
      color: #f44336;
      font-weight: 500;
    }

    .user-level {
      color: #2196f3;
      font-weight: 500;
    }

    .no-qualification {
      color: #ff9800;
      font-style: italic;
    }

    .qualification-info {
      margin: 24px 0;
      padding: 16px;
      background-color: #e3f2fd;
      border-radius: 8px;
      border-left: 4px solid #2196f3;
    }

    .qualification-info h4 {
      margin: 0 0 12px 0;
      color: #1976d2;
      font-size: 16px;
    }

    .qualification-info ul {
      margin: 0;
      padding-left: 20px;
    }

    .qualification-info li {
      margin-bottom: 8px;
      color: #333;
    }

    .user-info {
      margin-top: 24px;
      padding: 16px;
      background-color: #f0f0f0;
      border-radius: 8px;
      font-size: 14px;
    }

    .user-info p {
      margin: 4px 0;
    }

    mat-card-actions {
      margin-top: 24px;
    }

    mat-card-actions button {
      margin-left: 8px;
    }

    mat-icon[mat-card-avatar] {
      font-size: 40px;
      width: 40px;
      height: 40px;
    }
  `]
})
export class UnauthorizedComponent {
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    readonly authService = inject(AuthService);

    testStandId: string | null = null;
    requiredLevel: string | null = null;
    userLevel: string | null = null;

    constructor() {
        // Get query parameters to show specific permission details
        this.route.queryParams.subscribe(params => {
            this.testStandId = params['test'] || null;
            this.requiredLevel = params['required'] || null;
            this.userLevel = params['userLevel'] || null;
        });
    }

    goBack(): void {
        window.history.back();
    }

    goHome(): void {
        this.router.navigate(['/']);
    }
}