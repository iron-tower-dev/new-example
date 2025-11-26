import { Injectable, inject } from '@angular/core';
import { SwUpdate, VersionReadyEvent } from '@angular/service-worker';
import { filter, map } from 'rxjs/operators';
import { NotificationService } from './notification.service';

@Injectable({
    providedIn: 'root'
})
export class SwUpdateService {
    private swUpdate = inject(SwUpdate);
    private notificationService = inject(NotificationService);

    constructor() {
        if (this.swUpdate.isEnabled) {
            this.checkForUpdates();
            this.handleUpdateAvailable();
            this.handleUpdateActivated();
        }
    }

    private checkForUpdates(): void {
        // Check for updates every 6 hours
        setInterval(() => {
            this.swUpdate.checkForUpdate().then(() => {
                console.log('Checked for app updates');
            }).catch(err => {
                console.error('Error checking for updates:', err);
            });
        }, 6 * 60 * 60 * 1000);
    }

    private handleUpdateAvailable(): void {
        this.swUpdate.versionUpdates
            .pipe(
                filter((evt): evt is VersionReadyEvent => evt.type === 'VERSION_READY'),
                map(evt => ({
                    type: 'UPDATE_AVAILABLE',
                    current: evt.currentVersion,
                    available: evt.latestVersion,
                }))
            )
            .subscribe(() => {
                this.notificationService.showUpdateAvailable(() => {
                    this.activateUpdate();
                });
            });
    }

    private handleUpdateActivated(): void {
        this.swUpdate.versionUpdates
            .pipe(
                filter(evt => evt.type === 'VERSION_INSTALLATION_FAILED')
            )
            .subscribe(() => {
                this.notificationService.showError('Application update failed. Please refresh manually.');
            });
    }

    private activateUpdate(): void {
        this.swUpdate.activateUpdate().then(() => {
            document.location.reload();
        }).catch(err => {
            console.error('Error activating update:', err);
            this.notificationService.showError('Failed to activate update. Please refresh manually.');
        });
    }

    public forceUpdate(): void {
        if (this.swUpdate.isEnabled) {
            this.swUpdate.checkForUpdate().then(updateAvailable => {
                if (updateAvailable) {
                    this.activateUpdate();
                } else {
                    this.notificationService.showInfo('No updates available');
                }
            }).catch(err => {
                console.error('Error checking for updates:', err);
                this.notificationService.showError('Error checking for updates');
            });
        }
    }
}