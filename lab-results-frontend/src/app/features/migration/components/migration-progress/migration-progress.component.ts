import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { CommonModule } from '@angular/common';
import { MigrationService, MigrationStatus, MigrationProgress, MigrationError } from '../../services/migration.service';

@Component({
    selector: 'app-migration-progress',
    standalone: true,
    imports: [
        CommonModule
    ],
    templateUrl: './migration-progress.component.html',
    styleUrls: ['./migration-progress.component.scss']
})
export class MigrationProgressComponent implements OnInit, OnDestroy {
    @Input() status: MigrationStatus | null = null;
    @Input() progress: MigrationProgress | null = null;

    private destroy$ = new Subject<void>();

    showErrorDetails = false;
    selectedError: MigrationError | null = null;

    constructor(private migrationService: MigrationService) { }

    ngOnInit(): void {
        // Subscribe to real-time updates if not provided via inputs
        if (!this.status) {
            this.migrationService.currentStatus$
                .pipe(takeUntil(this.destroy$))
                .subscribe(status => {
                    this.status = status;
                });
        }

        if (!this.progress) {
            this.migrationService.progress$
                .pipe(takeUntil(this.destroy$))
                .subscribe(progress => {
                    this.progress = progress;
                });
        }
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    cancelMigration(): void {
        if (confirm('Are you sure you want to cancel the current migration?')) {
            this.migrationService.cancelMigration().subscribe({
                next: () => {
                    console.log('Migration cancellation requested');
                },
                error: (error) => {
                    console.error('Error cancelling migration:', error);
                }
            });
        }
    }

    pauseMigration(): void {
        this.migrationService.pauseMigration().subscribe({
            next: () => {
                console.log('Migration pause requested');
            },
            error: (error) => {
                console.error('Error pausing migration:', error);
            }
        });
    }

    resumeMigration(): void {
        this.migrationService.resumeMigration().subscribe({
            next: () => {
                console.log('Migration resume requested');
            },
            error: (error) => {
                console.error('Error resuming migration:', error);
            }
        });
    }

    showError(error: MigrationError): void {
        this.selectedError = error;
        this.showErrorDetails = true;
    }

    closeErrorDetails(): void {
        this.showErrorDetails = false;
        this.selectedError = null;
    }

    downloadReport(): void {
        if (this.status?.migrationId) {
            this.migrationService.downloadMigrationReport(this.status.migrationId, 'json').subscribe({
                next: (blob) => {
                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = `migration-report-${this.status!.migrationId}.json`;
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    window.URL.revokeObjectURL(url);
                },
                error: (error) => {
                    console.error('Error downloading report:', error);
                }
            });
        }
    }

    downloadLogs(): void {
        if (this.status?.migrationId) {
            this.migrationService.downloadMigrationLogs(this.status.migrationId, 'all').subscribe({
                next: (blob) => {
                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = `migration-logs-${this.status!.migrationId}.txt`;
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    window.URL.revokeObjectURL(url);
                },
                error: (error) => {
                    console.error('Error downloading logs:', error);
                }
            });
        }
    }

    getStatusColor(status: string): string {
        return this.migrationService.getStatusColor(status);
    }

    getErrorLevelColor(level: string): string {
        return this.migrationService.getErrorLevelColor(level);
    }

    formatDuration(duration: string): string {
        return this.migrationService.formatDuration(duration);
    }

    getProgressBarClass(): string {
        if (!this.status) return 'bg-secondary';

        switch (this.status.status.toLowerCase()) {
            case 'completed': return 'bg-success';
            case 'failed': return 'bg-danger';
            case 'cancelled': return 'bg-warning';
            case 'inprogress': return 'bg-primary';
            case 'paused': return 'bg-info';
            default: return 'bg-secondary';
        }
    }

    canPause(): boolean {
        return this.status?.status === 'InProgress';
    }

    canResume(): boolean {
        return this.status?.status === 'Paused';
    }

    canCancel(): boolean {
        return this.status?.status === 'InProgress' || this.status?.status === 'Paused';
    }

    isCompleted(): boolean {
        return this.status?.status === 'Completed' ||
            this.status?.status === 'Failed' ||
            this.status?.status === 'Cancelled';
    }
}