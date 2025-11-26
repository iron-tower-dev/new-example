import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../services/auth.service';
import { UserQualificationsDialogComponent } from './user-qualifications-dialog.component';

@Component({
  selector: 'app-user-info',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatChipsModule,
    MatTooltipModule,
    MatDialogModule,
    MatDividerModule
  ],
  template: `
    @if (authService.isAuthenticated()) {
      <div class="user-info">
        <button 
          mat-button 
          [matMenuTriggerFor]="userMenu"
          class="user-button">
          <mat-icon>person</mat-icon>
          <span class="user-name">{{ authService.currentUser()?.fullName }}</span>
          <mat-icon>arrow_drop_down</mat-icon>
        </button>

        <mat-menu #userMenu="matMenu">
          <div class="user-menu-header">
            <div class="user-details">
              <div class="user-name">{{ authService.currentUser()?.fullName }}</div>
              <div class="user-id">ID: {{ authService.currentUser()?.employeeId }}</div>
              <mat-chip class="role-chip" [color]="getRoleColor()">
                {{ authService.currentUser()?.role }}
              </mat-chip>
            </div>
          </div>
          
          <mat-divider></mat-divider>
          
          @if (authService.isTechnician()) {
            <button mat-menu-item (click)="showQualifications()">
              <mat-icon>verified_user</mat-icon>
              <span>View Qualifications</span>
            </button>
          }
          
          <button mat-menu-item (click)="refreshUserInfo()">
            <mat-icon>refresh</mat-icon>
            <span>Refresh Info</span>
          </button>
          
          <mat-divider></mat-divider>
          
          <button mat-menu-item (click)="logout()" class="logout-item">
            <mat-icon>logout</mat-icon>
            <span>Sign Out</span>
          </button>
        </mat-menu>
      </div>
    }
  `,
  styles: [`
    .user-info {
      display: flex;
      align-items: center;
    }

    .user-button {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 16px;
      border-radius: 20px;
      background-color: rgba(255, 255, 255, 0.1);
      color: white;
      transition: background-color 0.2s;
    }

    .user-button:hover {
      background-color: rgba(255, 255, 255, 0.2);
    }

    .user-name {
      font-weight: 500;
      max-width: 150px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .user-menu-header {
      padding: 16px;
      background-color: #f5f5f5;
    }

    .user-details .user-name {
      font-weight: 600;
      font-size: 16px;
      color: #333;
      margin-bottom: 4px;
    }

    .user-id {
      font-size: 12px;
      color: #666;
      margin-bottom: 8px;
    }

    .role-chip {
      font-size: 11px;
      height: 24px;
    }

    .logout-item {
      color: #f44336;
    }

    .logout-item mat-icon {
      color: #f44336;
    }

    ::ng-deep .mat-mdc-menu-panel {
      min-width: 250px;
    }
  `]
})
export class UserInfoComponent {
  readonly authService = inject(AuthService);
  private readonly dialog = inject(MatDialog);

  getRoleColor(): 'primary' | 'accent' | 'warn' {
    const role = this.authService.currentUser()?.role;
    switch (role) {
      case 'Reviewer':
        return 'accent';
      case 'Technician':
        return 'primary';
      default:
        return 'primary';
    }
  }

  showQualifications(): void {
    this.dialog.open(UserQualificationsDialogComponent, {
      width: '600px',
      data: {
        user: this.authService.currentUser(),
        qualifications: this.authService.currentUser()?.qualifications || []
      }
    });
  }

  refreshUserInfo(): void {
    this.authService.getCurrentUser().subscribe({
      next: () => {
        // User info updated via the service
      },
      error: (error) => {
        console.error('Failed to refresh user info:', error);
      }
    });
  }

  logout(): void {
    this.authService.logout().catch(error => {
      console.error('Logout failed:', error);
      // Even if logout fails, the user should be logged out locally
    });
  }
}