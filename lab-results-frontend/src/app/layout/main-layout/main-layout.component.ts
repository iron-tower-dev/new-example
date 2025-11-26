import { Component, signal, inject } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { RouterOutlet, Router } from '@angular/router';
import { UserInfoComponent } from '../../shared/components/user-info/user-info.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [SharedModule, RouterOutlet, UserInfoComponent],
  template: `
    <mat-sidenav-container class="sidenav-container">
      <mat-sidenav #drawer class="sidenav" fixedInViewport
                   [attr.role]="isHandset() ? 'dialog' : 'navigation'"
                   [mode]="isHandset() ? 'over' : 'side'"
                   [opened]="!isHandset()">
        <mat-toolbar>Menu</mat-toolbar>
        <mat-nav-list>
          <a mat-list-item routerLink="/samples">
            <mat-icon>science</mat-icon>
            <span>Sample Management</span>
          </a>

          <mat-list-item (click)="navigateToTest('/tests', 'Laboratory Tests')">
            <mat-icon>assignment</mat-icon>
            <span>Laboratory Tests</span>
          </mat-list-item>
        </mat-nav-list>
      </mat-sidenav>
      
      <mat-sidenav-content>
        <mat-toolbar color="primary">
          @if (isHandset()) {
            <button
              type="button"
              aria-label="Toggle sidenav"
              mat-icon-button
              (click)="drawer.toggle()">
              <mat-icon aria-label="Side nav toggle icon">menu</mat-icon>
            </button>
          }
          <span>Laboratory Test Results Entry System</span>
          <span class="spacer"></span>
          <app-user-info></app-user-info>
        </mat-toolbar>
        
        <div class="content">
          <router-outlet></router-outlet>
        </div>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .sidenav-container {
      height: 100%;
    }

    .sidenav {
      width: 200px;
    }

    .sidenav .mat-toolbar {
      background: inherit;
    }

    .mat-toolbar.mat-primary {
      position: sticky;
      top: 0;
      z-index: 1;
    }

    .spacer {
      flex: 1 1 auto;
    }

    .content {
      padding: 20px;
      min-height: calc(100vh - 64px);
    }

    .category-section {
      padding: 8px 0;
    }

    .category-title {
      padding: 12px 16px 8px 16px;
      margin: 0;
      font-size: 12px;
      font-weight: 600;
      color: rgba(0, 0, 0, 0.54);
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .test-link {
      padding-left: 24px;
    }

    .test-nav-link {
      display: flex;
      align-items: center;
      width: 100%;
      padding: 12px 16px;
      text-decoration: none;
      color: inherit;
      gap: 12px;
    }

    .test-nav-link:hover {
      background-color: rgba(0, 0, 0, 0.04);
    }

    .active-link {
      background-color: rgba(63, 81, 181, 0.08);
      color: #3f51b5;
    }

    .active-link mat-icon {
      color: #3f51b5;
    }

    @media (max-width: 768px) {
      .content {
        padding: 16px;
      }
    }
  `]
})
export class MainLayoutComponent {
  isHandset = signal(false);
  private router = inject(Router);

  constructor() {
    // Simple responsive detection - in a real app you might use BreakpointObserver
    this.checkScreenSize();
    window.addEventListener('resize', () => this.checkScreenSize());
  }

  private checkScreenSize(): void {
    this.isHandset.set(window.innerWidth < 768);
  }

  navigateToTest(route: string, testName: string): void {
    console.log(`[${new Date().toLocaleTimeString()}] Clicked on ${testName} test`);
    console.log('Current URL before:', window.location.href);
    console.log('Navigating to:', route);

    this.router.navigate([route]).then(success => {
      console.log('Navigation success:', success);
      setTimeout(() => {
        console.log('Current URL after:', window.location.href);
      }, 100);
    }).catch(error => {
      console.error('Navigation error:', error);
    });
  }
}