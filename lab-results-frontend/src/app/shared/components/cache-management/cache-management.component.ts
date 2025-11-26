import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { Subject, takeUntil, interval, startWith } from 'rxjs';
import { LookupService, CacheStatusDto, CacheInfo } from '../../services/lookup.service';

@Component({
    selector: 'app-cache-management',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatSnackBarModule,
        MatTooltipModule,
        MatChipsModule
    ],
    template: `
    <div class="cache-management-container">
      <mat-card class="cache-overview-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>storage</mat-icon>
            Cache Management
          </mat-card-title>
          <mat-card-subtitle>
            Monitor and manage lookup data caches
          </mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <div class="cache-actions">
            <button mat-raised-button 
                    color="primary" 
                    (click)="refreshAllCaches()"
                    [disabled]="isRefreshing()">
              @if (isRefreshing()) {
                <mat-spinner diameter="20"></mat-spinner>
              } @else {
                <mat-icon>refresh</mat-icon>
              }
              Refresh All Caches
            </button>
            
            <button mat-stroked-button 
                    (click)="loadCacheStatus()"
                    [disabled]="isLoading()">
              @if (isLoading()) {
                <mat-spinner diameter="20"></mat-spinner>
              } @else {
                <mat-icon>info</mat-icon>
              }
              Check Status
            </button>
          </div>

          @if (cacheStatus(); as status) {
            <div class="cache-status-grid">
              @for (entry of getCacheEntries(status); track entry.key) {
                <mat-card class="cache-entry-card">
                  <mat-card-header>
                    <mat-card-title class="cache-entry-title">
                      {{ getCacheDisplayName(entry.key) }}
                    </mat-card-title>
                    <div class="cache-status-chip">
                      <mat-chip [color]="entry.value.isLoaded ? 'primary' : 'warn'" selected>
                        {{ entry.value.isLoaded ? 'Loaded' : 'Not Loaded' }}
                      </mat-chip>
                    </div>
                  </mat-card-header>
                  
                  <mat-card-content>
                    <div class="cache-details">
                      <div class="cache-detail">
                        <span class="detail-label">Items:</span>
                        <span class="detail-value">{{ entry.value.itemCount }}</span>
                      </div>
                      
                      @if (entry.value.lastRefreshed) {
                        <div class="cache-detail">
                          <span class="detail-label">Last Refreshed:</span>
                          <span class="detail-value">{{ formatDate(entry.value.lastRefreshed) }}</span>
                        </div>
                      }
                      
                      @if (entry.value.expiresIn) {
                        <div class="cache-detail">
                          <span class="detail-label">Expires In:</span>
                          <span class="detail-value">{{ entry.value.expiresIn }}</span>
                        </div>
                      }
                    </div>
                  </mat-card-content>
                  
                  <mat-card-actions>
                    <button mat-button 
                            (click)="refreshSpecificCache(entry.key)"
                            [disabled]="isRefreshing()">
                      <mat-icon>refresh</mat-icon>
                      Refresh
                    </button>
                  </mat-card-actions>
                </mat-card>
              }
            </div>
          } @else if (hasError()) {
            <div class="error-display">
              <mat-icon color="warn">error</mat-icon>
              <span>{{ error() }}</span>
              <button mat-icon-button (click)="clearError()" matTooltip="Clear error">
                <mat-icon>close</mat-icon>
              </button>
            </div>
          } @else {
            <div class="no-data">
              <mat-icon>info</mat-icon>
              <span>Click "Check Status" to load cache information</span>
            </div>
          }
        </mat-card-content>
      </mat-card>

      <!-- Auto-refresh toggle -->
      <mat-card class="auto-refresh-card">
        <mat-card-content>
          <div class="auto-refresh-controls">
            <span>Auto-refresh every 30 seconds:</span>
            <button mat-icon-button 
                    [color]="autoRefreshEnabled() ? 'primary' : 'basic'"
                    (click)="toggleAutoRefresh()"
                    [matTooltip]="autoRefreshEnabled() ? 'Disable auto-refresh' : 'Enable auto-refresh'">
              <mat-icon>{{ autoRefreshEnabled() ? 'pause' : 'play_arrow' }}</mat-icon>
            </button>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
    styles: [`
    .cache-management-container {
      padding: 16px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .cache-overview-card {
      margin-bottom: 16px;
    }

    .cache-actions {
      display: flex;
      gap: 16px;
      margin-bottom: 24px;
      flex-wrap: wrap;
    }

    .cache-status-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 16px;
      margin-top: 16px;
    }

    .cache-entry-card {
      border: 1px solid #e0e0e0;
    }

    .cache-entry-title {
      font-size: 1.1em;
      font-weight: 500;
    }

    .cache-status-chip {
      margin-left: auto;
    }

    .cache-details {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .cache-detail {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .detail-label {
      font-weight: 500;
      color: rgba(0, 0, 0, 0.7);
    }

    .detail-value {
      color: rgba(0, 0, 0, 0.87);
    }

    .error-display {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 16px;
      background-color: #ffebee;
      border-radius: 4px;
      color: #d32f2f;
    }

    .no-data {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 16px;
      color: rgba(0, 0, 0, 0.6);
      justify-content: center;
    }

    .auto-refresh-card {
      margin-top: 16px;
    }

    .auto-refresh-controls {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    mat-card-header {
      display: flex;
      align-items: center;
    }

    mat-card-title {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    mat-spinner {
      margin-right: 8px;
    }
  `]
})
export class CacheManagementComponent implements OnInit, OnDestroy {
    private lookupService = inject(LookupService);
    private snackBar = inject(MatSnackBar);
    private destroy$ = new Subject<void>();

    // Component state signals
    private _isLoading = signal(false);
    private _isRefreshing = signal(false);
    private _autoRefreshEnabled = signal(false);
    private _cacheStatus = signal<CacheStatusDto | null>(null);

    // Public readonly signals
    readonly isLoading = this._isLoading.asReadonly();
    readonly isRefreshing = this._isRefreshing.asReadonly();
    readonly autoRefreshEnabled = this._autoRefreshEnabled.asReadonly();
    readonly cacheStatus = this._cacheStatus.asReadonly();
    readonly error = this.lookupService.error;
    readonly hasError = this.lookupService.hasError;

    ngOnInit(): void {
        this.loadCacheStatus();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadCacheStatus(): void {
        this._isLoading.set(true);

        this.lookupService.getCacheStatus()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (status) => {
                    this._cacheStatus.set(status);
                    this._isLoading.set(false);
                },
                error: (error) => {
                    console.error('Error loading cache status:', error);
                    this._isLoading.set(false);
                    this.snackBar.open('Error loading cache status', 'Close', { duration: 3000 });
                }
            });
    }

    refreshAllCaches(): void {
        this._isRefreshing.set(true);

        this.lookupService.refreshAllCaches()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this._isRefreshing.set(false);
                    this.snackBar.open(result.message, 'Close', { duration: 3000 });
                    this.loadCacheStatus(); // Reload status after refresh
                },
                error: (error) => {
                    console.error('Error refreshing caches:', error);
                    this._isRefreshing.set(false);
                    this.snackBar.open('Error refreshing caches', 'Close', { duration: 3000 });
                }
            });
    }

    refreshSpecificCache(cacheKey: string): void {
        this._isRefreshing.set(true);

        let refreshObservable;

        switch (cacheKey.toLowerCase()) {
            case 'nas':
                // NAS cache refresh is handled by the existing method
                this.snackBar.open('NAS cache refresh not implemented individually', 'Close', { duration: 3000 });
                this._isRefreshing.set(false);
                return;
            case 'nlgi':
                // NLGI cache refresh is handled by the existing method
                this.snackBar.open('NLGI cache refresh not implemented individually', 'Close', { duration: 3000 });
                this._isRefreshing.set(false);
                return;
            case 'particletypes':
                refreshObservable = this.lookupService.refreshParticleTypeCache();
                break;
            case 'commentareas':
                refreshObservable = this.lookupService.refreshCommentCache();
                break;
            default:
                if (cacheKey.toLowerCase().includes('equipment')) {
                    refreshObservable = this.lookupService.refreshEquipmentCache();
                } else {
                    this.snackBar.open('Unknown cache type', 'Close', { duration: 3000 });
                    this._isRefreshing.set(false);
                    return;
                }
        }

        refreshObservable.pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (result) => {
                    this._isRefreshing.set(false);
                    this.snackBar.open(result.message, 'Close', { duration: 3000 });
                    this.loadCacheStatus(); // Reload status after refresh
                },
                error: (error) => {
                    console.error(`Error refreshing ${cacheKey} cache:`, error);
                    this._isRefreshing.set(false);
                    this.snackBar.open(`Error refreshing ${cacheKey} cache`, 'Close', { duration: 3000 });
                }
            });
    }

    toggleAutoRefresh(): void {
        const newState = !this._autoRefreshEnabled();
        this._autoRefreshEnabled.set(newState);

        if (newState) {
            // Start auto-refresh
            interval(30000) // 30 seconds
                .pipe(
                    startWith(0),
                    takeUntil(this.destroy$)
                )
                .subscribe(() => {
                    if (this._autoRefreshEnabled()) {
                        this.loadCacheStatus();
                    }
                });

            this.snackBar.open('Auto-refresh enabled', 'Close', { duration: 2000 });
        } else {
            this.snackBar.open('Auto-refresh disabled', 'Close', { duration: 2000 });
        }
    }

    clearError(): void {
        this.lookupService.clearError();
    }

    getCacheEntries(status: CacheStatusDto): Array<{ key: string; value: CacheInfo }> {
        return Object.entries(status.cacheEntries).map(([key, value]) => ({ key, value }));
    }

    getCacheDisplayName(cacheKey: string): string {
        const displayNames: { [key: string]: string } = {
            'NAS': 'NAS Lookup',
            'NLGI': 'NLGI Lookup',
            'ParticleTypes': 'Particle Types',
            'CommentAreas': 'Comment Areas',
            'Equipment': 'Equipment'
        };

        return displayNames[cacheKey] || cacheKey;
    }

    formatDate(date: Date): string {
        return new Date(date).toLocaleString();
    }
}