import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../models/auth.model';
import { AuthDebugComponent } from '../auth-debug/auth-debug.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatSnackBarModule,
    AuthDebugComponent
  ],
  template: `
    <div class="login-container">
      <mat-card class="login-card">
        <mat-card-header>
          <mat-card-title>Laboratory Test Results System</mat-card-title>
          <mat-card-subtitle>Please sign in to continue</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="login-form">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Employee ID</mat-label>
              <input 
                matInput 
                formControlName="employeeId" 
                placeholder="Enter your employee ID"
                maxlength="5"
                autocomplete="username">
              <mat-icon matSuffix>person</mat-icon>
              @if (loginForm.get('employeeId')?.hasError('required') && loginForm.get('employeeId')?.touched) {
                <mat-error>Employee ID is required</mat-error>
              }
              @if (loginForm.get('employeeId')?.hasError('maxlength')) {
                <mat-error>Employee ID must be 5 characters or less</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Password</mat-label>
              <input 
                matInput 
                [type]="hidePassword() ? 'password' : 'text'"
                formControlName="password" 
                placeholder="Enter your password"
                maxlength="8"
                autocomplete="current-password">
              <button 
                mat-icon-button 
                matSuffix 
                type="button"
                (click)="togglePasswordVisibility()"
                [attr.aria-label]="'Hide password'"
                [attr.aria-pressed]="hidePassword()">
                <mat-icon>{{hidePassword() ? 'visibility_off' : 'visibility'}}</mat-icon>
              </button>
              @if (loginForm.get('password')?.hasError('required') && loginForm.get('password')?.touched) {
                <mat-error>Password is required</mat-error>
              }
              @if (loginForm.get('password')?.hasError('maxlength')) {
                <mat-error>Password must be 8 characters or less</mat-error>
              }
            </mat-form-field>

            @if (authService.error()) {
              <div class="error-message">
                <mat-icon color="warn">error</mat-icon>
                <span>{{ authService.error() }}</span>
              </div>
            }
          </form>
        </mat-card-content>

        <mat-card-actions align="end">
          <button 
            mat-raised-button 
            color="primary" 
            type="submit"
            (click)="onSubmit()"
            [disabled]="loginForm.invalid || authService.isLoading()"
            class="login-button">
            @if (authService.isLoading()) {
              <mat-spinner diameter="20"></mat-spinner>
              <span>Signing in...</span>
            } @else {
              <span>Sign In</span>
            }
          </button>
        </mat-card-actions>
      </mat-card>
      
      <!-- Debug Component -->
      <app-auth-debug></app-auth-debug>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }

    .login-card {
      width: 100%;
      max-width: 400px;
      padding: 20px;
    }

    .login-form {
      display: flex;
      flex-direction: column;
      gap: 16px;
      margin-top: 20px;
    }

    .full-width {
      width: 100%;
    }

    .login-button {
      min-width: 120px;
      height: 40px;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 8px;
      color: #f44336;
      font-size: 14px;
      margin-top: 8px;
    }

    mat-card-header {
      text-align: center;
      margin-bottom: 20px;
    }

    mat-card-title {
      font-size: 24px;
      font-weight: 500;
      color: #333;
    }

    mat-card-subtitle {
      font-size: 14px;
      color: #666;
      margin-top: 8px;
    }
  `]
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly authService = inject(AuthService);
  readonly hidePassword = signal(true);

  loginForm: FormGroup;

  constructor() {
    this.loginForm = this.fb.group({
      employeeId: ['', [Validators.required, Validators.maxLength(5)]],
      password: ['', [Validators.required, Validators.maxLength(8)]]
    });

    // Redirect if already authenticated
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
    }
  }

  togglePasswordVisibility(): void {
    this.hidePassword.update(value => !value);
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      const credentials: LoginRequest = {
        employeeId: this.loginForm.value.employeeId.trim().toUpperCase(),
        password: this.loginForm.value.password
      };

      this.authService.login(credentials).subscribe({
        next: (response) => {
          this.snackBar.open(
            `Welcome, ${response.fullName}!`,
            'Close',
            { duration: 3000 }
          );
          this.router.navigate(['/']);
        },
        error: (error) => {
          let errorMessage = 'Login failed. Please check your credentials.';

          if (error.status === 401) {
            errorMessage = 'Invalid employee ID or password.';
          } else if (error.status === 0) {
            errorMessage = 'Unable to connect to server. Please try again.';
          }

          this.snackBar.open(errorMessage, 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      });
    } else {
      // Mark all fields as touched to show validation errors
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
    }
  }
}