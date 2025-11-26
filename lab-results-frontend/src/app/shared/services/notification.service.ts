import { Injectable, inject, signal, computed } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

export interface NotificationMessage {
    id: string;
    message: string;
    type: 'success' | 'error' | 'warning' | 'info';
    timestamp: Date;
    duration?: number;
    persistent?: boolean;
    details?: string[];
}

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    private snackBar = inject(MatSnackBar);

    // Notification state management
    private _notifications = signal<NotificationMessage[]>([]);
    private _activeNotifications = signal<NotificationMessage[]>([]);

    readonly notifications = this._notifications.asReadonly();
    readonly activeNotifications = this._activeNotifications.asReadonly();
    readonly hasActiveNotifications = computed(() => this._activeNotifications().length > 0);
    readonly errorCount = computed(() =>
        this._activeNotifications().filter(n => n.type === 'error').length
    );
    readonly warningCount = computed(() =>
        this._activeNotifications().filter(n => n.type === 'warning').length
    );

    /**
     * Show success notification
     */
    showSuccess(message: string, duration: number = 3000): void {
        this.showNotification({
            message,
            type: 'success',
            duration
        });
    }

    /**
     * Show error notification
     */
    showError(message: string, details?: string[], persistent: boolean = false): void {
        this.showNotification({
            message,
            type: 'error',
            details,
            persistent,
            duration: persistent ? undefined : 8000
        });
    }

    /**
     * Show warning notification
     */
    showWarning(message: string, details?: string[], duration: number = 5000): void {
        this.showNotification({
            message,
            type: 'warning',
            details,
            duration
        });
    }

    /**
     * Show info notification
     */
    showInfo(message: string, duration: number = 4000): void {
        this.showNotification({
            message,
            type: 'info',
            duration
        });
    }

    /**
     * Show update available notification
     */
    showUpdateAvailable(onUpdate: () => void): void {
        const config: MatSnackBarConfig = {
            duration: undefined, // Persistent
            verticalPosition: 'top',
            horizontalPosition: 'right',
            panelClass: ['update-available-snackbar', 'persistent-snackbar']
        };

        const snackBarRef = this.snackBar.open(
            'A new version of the application is available!',
            'Update Now',
            config
        );

        snackBarRef.onAction().subscribe(() => {
            onUpdate();
        });
    }

    /**
     * Show validation errors
     */
    showValidationErrors(errors: { field: string; message: string }[]): void {
        const message = `Validation failed (${errors.length} error${errors.length > 1 ? 's' : ''})`;
        const details = errors.map(error => `${error.field}: ${error.message}`);

        this.showError(message, details, false);
    }

    /**
     * Show validation warnings
     */
    showValidationWarnings(warnings: { field: string; message: string }[]): void {
        const message = `Validation warnings (${warnings.length} warning${warnings.length > 1 ? 's' : ''})`;
        const details = warnings.map(warning => `${warning.field}: ${warning.message}`);

        this.showWarning(message, details);
    }

    /**
     * Show generic notification
     */
    private showNotification(notification: Partial<NotificationMessage>): void {
        const fullNotification: NotificationMessage = {
            id: this.generateId(),
            message: notification.message || '',
            type: notification.type || 'info',
            timestamp: new Date(),
            duration: notification.duration,
            persistent: notification.persistent || false,
            details: notification.details
        };

        // Add to notifications history
        this._notifications.update(notifications => [fullNotification, ...notifications]);

        // Add to active notifications
        this._activeNotifications.update(active => [...active, fullNotification]);

        // Show snackbar
        this.showSnackBar(fullNotification);

        // Auto-remove if not persistent
        if (!fullNotification.persistent && fullNotification.duration) {
            setTimeout(() => {
                this.removeNotification(fullNotification.id);
            }, fullNotification.duration);
        }
    }

    /**
     * Remove notification
     */
    removeNotification(id: string): void {
        this._activeNotifications.update(active =>
            active.filter(notification => notification.id !== id)
        );
    }

    /**
     * Clear all notifications
     */
    clearAllNotifications(): void {
        this._activeNotifications.set([]);
        this.snackBar.dismiss();
    }

    /**
     * Clear notifications by type
     */
    clearNotificationsByType(type: NotificationMessage['type']): void {
        this._activeNotifications.update(active =>
            active.filter(notification => notification.type !== type)
        );
    }

    /**
     * Get notifications by type
     */
    getNotificationsByType(type: NotificationMessage['type']): NotificationMessage[] {
        return this._activeNotifications().filter(notification => notification.type === type);
    }

    /**
     * Show snackbar with appropriate styling
     */
    private showSnackBar(notification: NotificationMessage): void {
        const config: MatSnackBarConfig = {
            duration: notification.persistent ? undefined : (notification.duration || 4000),
            verticalPosition: 'top',
            horizontalPosition: 'right',
            panelClass: this.getSnackBarClasses(notification)
        };

        let displayMessage = notification.message;
        if (notification.details && notification.details.length > 0) {
            // For detailed messages, show first few details
            const maxDetails = 3;
            const visibleDetails = notification.details.slice(0, maxDetails);
            const moreCount = notification.details.length - maxDetails;

            displayMessage += '\n' + visibleDetails.join('\n');
            if (moreCount > 0) {
                displayMessage += `\n... and ${moreCount} more`;
            }
        }

        const snackBarRef = this.snackBar.open(
            displayMessage,
            notification.persistent ? 'Dismiss' : 'Close',
            config
        );

        // Handle snackbar dismissal
        snackBarRef.afterDismissed().subscribe(() => {
            this.removeNotification(notification.id);
        });

        // For persistent notifications, handle manual dismissal
        if (notification.persistent) {
            snackBarRef.onAction().subscribe(() => {
                this.removeNotification(notification.id);
            });
        }
    }

    /**
     * Get CSS classes for snackbar based on notification type
     */
    private getSnackBarClasses(notification: NotificationMessage): string[] {
        const baseClasses = ['notification-snackbar'];

        switch (notification.type) {
            case 'success':
                baseClasses.push('success-snackbar');
                break;
            case 'error':
                baseClasses.push('error-snackbar');
                if (notification.details && notification.details.length > 0) {
                    baseClasses.push('detailed-error-snackbar');
                }
                break;
            case 'warning':
                baseClasses.push('warning-snackbar');
                break;
            case 'info':
                baseClasses.push('info-snackbar');
                break;
        }

        if (notification.persistent) {
            baseClasses.push('persistent-snackbar');
        }

        return baseClasses;
    }

    /**
     * Generate unique ID for notifications
     */
    private generateId(): string {
        return `notification_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    }

    /**
     * Get notification history (last 50 notifications)
     */
    getNotificationHistory(): NotificationMessage[] {
        return this._notifications().slice(0, 50);
    }

    /**
     * Clear notification history
     */
    clearNotificationHistory(): void {
        this._notifications.set([]);
    }
}