import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MigrationService, MigrationStatus, MigrationStatisticsSummary } from '../../services/migration.service';

@Component({
    selector: 'app-migration-history',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule
    ],
    templateUrl: './migration-history.component.html',
    styleUrls: ['./migration-history.component.scss']
})
export class MigrationHistoryComponent implements OnInit {
    migrationHistory: MigrationStatus[] = [];
    statistics: MigrationStatisticsSummary | null = null;
    loading = false;
    error: string | null = null;

    // Pagination
    currentPage = 1;
    pageSize = 10;
    totalItems = 0;

    // Filters
    statusFilter = 'all';
    dateFilter = 30; // days

    // Make Math available in template
    Math = Math;

    constructor(private migrationService: MigrationService) { }

    ngOnInit(): void {
        this.loadMigrationHistory();
        this.loadStatistics();
    }

    loadMigrationHistory(): void {
        this.loading = true;
        this.error = null;

        this.migrationService.getMigrationHistory(50).subscribe({
            next: (history) => {
                this.migrationHistory = this.applyFilters(history);
                this.totalItems = this.migrationHistory.length;
                this.loading = false;
            },
            error: (error) => {
                this.error = 'Failed to load migration history';
                this.loading = false;
                console.error('Error loading migration history:', error);
            }
        });
    }

    loadStatistics(): void {
        this.migrationService.getMigrationStatistics(this.dateFilter).subscribe({
            next: (stats) => {
                this.statistics = stats;
            },
            error: (error) => {
                console.error('Error loading statistics:', error);
            }
        });
    }

    applyFilters(history: MigrationStatus[]): MigrationStatus[] {
        let filtered = [...history];

        // Status filter
        if (this.statusFilter !== 'all') {
            filtered = filtered.filter(m => m.status.toLowerCase() === this.statusFilter.toLowerCase());
        }

        // Date filter
        const cutoffDate = new Date();
        cutoffDate.setDate(cutoffDate.getDate() - this.dateFilter);
        filtered = filtered.filter(m => new Date(m.startTime) >= cutoffDate);

        return filtered.sort((a, b) => new Date(b.startTime).getTime() - new Date(a.startTime).getTime());
    }

    onStatusFilterChange(): void {
        this.currentPage = 1;
        this.migrationHistory = this.applyFilters(this.migrationHistory);
        this.totalItems = this.migrationHistory.length;
    }

    onDateFilterChange(): void {
        this.currentPage = 1;
        this.loadMigrationHistory();
        this.loadStatistics();
    }

    getPaginatedHistory(): MigrationStatus[] {
        const startIndex = (this.currentPage - 1) * this.pageSize;
        const endIndex = startIndex + this.pageSize;
        return this.migrationHistory.slice(startIndex, endIndex);
    }

    getTotalPages(): number {
        return Math.ceil(this.totalItems / this.pageSize);
    }

    goToPage(page: number): void {
        if (page >= 1 && page <= this.getTotalPages()) {
            this.currentPage = page;
        }
    }

    downloadReport(migrationId: string): void {
        this.migrationService.downloadMigrationReport(migrationId, 'json').subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `migration-report-${migrationId}.json`;
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

    downloadLogs(migrationId: string): void {
        this.migrationService.downloadMigrationLogs(migrationId, 'all').subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `migration-logs-${migrationId}.txt`;
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

    getStatusColor(status: string): string {
        return this.migrationService.getStatusColor(status);
    }

    formatDuration(duration: string): string {
        return this.migrationService.formatDuration(duration);
    }

    refresh(): void {
        this.loadMigrationHistory();
        this.loadStatistics();
    }
}