import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-auth-debug',
    standalone: true,
    imports: [CommonModule, MatCardModule, MatButtonModule],
    template: `
    <mat-card style="margin: 20px; padding: 20px;">
      <mat-card-header>
        <mat-card-title>🔍 Authentication Debug</mat-card-title>
      </mat-card-header>
      
      <mat-card-content>
        <h3>Current Authentication State:</h3>
        <ul>
          <li><strong>Is Authenticated:</strong> {{ authService.isAuthenticated() ? '✅ YES' : '❌ NO' }}</li>
          <li><strong>Current User:</strong> {{ authService.currentUser()?.fullName || 'None' }}</li>
          <li><strong>User Role:</strong> {{ authService.currentUser()?.role || 'None' }}</li>
          <li><strong>Employee ID:</strong> {{ authService.currentUser()?.employeeId || 'None' }}</li>
          <li><strong>Token Present:</strong> {{ authService.getToken() ? '✅ YES' : '❌ NO' }}</li>
          <li><strong>Loading:</strong> {{ authService.isLoading() ? 'YES' : 'NO' }}</li>
          <li><strong>Error:</strong> {{ authService.error() || 'None' }}</li>
        </ul>

        <h3>LocalStorage Debug:</h3>
        <ul>
          <li><strong>Token in localStorage:</strong> {{ getLocalStorageToken() ? '✅ YES' : '❌ NO' }}</li>
          <li><strong>User in localStorage:</strong> {{ getLocalStorageUser() ? '✅ YES' : '❌ NO' }}</li>
        </ul>

        <h3>Raw Data:</h3>
        <pre style="background: #f5f5f5; padding: 10px; border-radius: 4px; font-size: 12px;">
Token: {{ getLocalStorageToken() || 'null' }}
User: {{ getLocalStorageUser() || 'null' }}
        </pre>
      </mat-card-content>

      <mat-card-actions>
        <button mat-raised-button color="primary" (click)="testLogin()">Test Login</button>
        <button mat-button (click)="clearAuth()">Clear Auth Data</button>
        <button mat-button (click)="refresh()">Refresh</button>
      </mat-card-actions>
    </mat-card>
  `
})
export class AuthDebugComponent {
    authService = inject(AuthService);

    getLocalStorageToken(): string | null {
        return localStorage.getItem('lab_results_token');
    }

    getLocalStorageUser(): string | null {
        return localStorage.getItem('lab_results_user');
    }

    testLogin() {
        this.authService.login({
            employeeId: 'ADMIN',
            password: 'admin123'
        }).subscribe({
            next: (response) => {
                console.log('Login successful:', response);
                this.refresh();
            },
            error: (error) => {
                console.error('Login failed:', error);
            }
        });
    }

    clearAuth() {
        localStorage.removeItem('lab_results_token');
        localStorage.removeItem('lab_results_user');
        this.refresh();
    }

    refresh() {
        window.location.reload();
    }
}