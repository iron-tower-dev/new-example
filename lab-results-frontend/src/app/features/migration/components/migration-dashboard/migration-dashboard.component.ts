import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { CommonModule } from '@angular/common';
import { MigrationService, MigrationStatus, MigrationProgress } from '../../services/migration.service';
import { MigrationProgressComponent } from '../migration-progress/migration-progress.component';
import { MigrationHistoryComponent } from '../migration-history/migration-history.component';
import { MigrationConfigComponent } from '../migration-config/migration-config.component';

@Component({
    selector: 'app-migration-dashboard',
    standalone: true,
    imports: [
        CommonModule,
        MigrationProgressComponent,
        MigrationHistoryComponent,
        MigrationConfigComponent
    ],
    templateUrl: './migration-dashboard.component.html',
    styleUrls: ['./migration-dashboard.component.scss']
})
export class MigrationDashboardComponent implements OnInit, OnDestroy {
    private destroy$ = new Subject<void>();

    currentStatus: MigrationStatus | null = null;
    currentProgress: MigrationProgress | null = null;
    isPolling = false;
    activeTab = 'dashboard';

    constructor(
        public migrationService: MigrationService,
        private router: Router
    ) { }

    ngOnInit(): void {
        // Subscribe to current status and progress
        this.migrationService.currentStatus$
            .pipe(takeUntil(this.destroy$))
            .subscribe(status => {
                this.currentStatus = status;
            });

        this.migrationService.progress$
            .pipe(takeUntil(this.destroy$))
            .subscribe(progress => {
                this.currentProgress = progress;
            });

        this.migrationService.isPolling$
            .pipe(takeUntil(this.destroy$))
            .subscribe(isPolling => {
                this.isPolling = isPolling;
            });

        // Load initial status
        this.loadInitialStatus();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        this.migrationService.stopPolling();
    }

    private loadInitialStatus(): void {
        this.migrationService.getMigrationStatus().subscribe({
            next: (status) => {
                this.currentStatus = status;

                // Start polling if migration is in progress
                if (status.status === 'InProgress' || status.status === 'Paused') {
                    this.migrationService.startPolling();
                }
            },
            error: (error) => {
                console.error('Error loading migration status:', error);
            }
        });
    }

    setActiveTab(tab: string): void {
        this.activeTab = tab;
    }

    startPolling(): void {
        this.migrationService.startPolling();
    }

    stopPolling(): void {
        this.migrationService.stopPolling();
    }

    getStatusColor(status: string): string {
        return this.migrationService.getStatusColor(status);
    }

    formatDuration(duration: string): string {
        return this.migrationService.formatDuration(duration);
    }
}