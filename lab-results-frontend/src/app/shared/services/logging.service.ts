import { Injectable } from '@angular/core';

export enum LogLevel {
    DEBUG = 0,
    INFO = 1,
    WARN = 2,
    ERROR = 3
}

export interface LogEntry {
    timestamp: string;
    level: LogLevel;
    category: string;
    message: string;
    data?: any;
    userId?: string;
    sessionId?: string;
}

@Injectable({
    providedIn: 'root'
})
export class LoggingService {
    private readonly maxLogEntries = 1000;
    private logEntries: LogEntry[] = [];
    private currentLogLevel: LogLevel = LogLevel.INFO;
    private sessionId: string;

    constructor() {
        this.sessionId = this.generateSessionId();
        this.setLogLevel();
    }

    private generateSessionId(): string {
        return `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    }

    private setLogLevel(): void {
        // Set log level based on environment
        const isDevelopment = !window.location.hostname.includes('production');
        this.currentLogLevel = isDevelopment ? LogLevel.DEBUG : LogLevel.INFO;
    }

    private shouldLog(level: LogLevel): boolean {
        return level >= this.currentLogLevel;
    }

    private createLogEntry(level: LogLevel, category: string, message: string, data?: any, userId?: string): LogEntry {
        return {
            timestamp: new Date().toISOString(),
            level,
            category,
            message,
            data: this.sanitizeData(data),
            userId,
            sessionId: this.sessionId
        };
    }

    private sanitizeData(data: any): any {
        if (!data) return data;

        // Create a deep copy to avoid modifying original data
        const sanitized = JSON.parse(JSON.stringify(data));

        // Remove sensitive information
        this.removeSensitiveFields(sanitized);

        return sanitized;
    }

    private removeSensitiveFields(obj: any): void {
        if (typeof obj !== 'object' || obj === null) return;

        const sensitiveFields = ['token', 'password', 'authorization', 'bearer', 'secret', 'key'];

        for (const key in obj) {
            if (obj.hasOwnProperty(key)) {
                const lowerKey = key.toLowerCase();

                // Remove sensitive fields
                if (sensitiveFields.some(field => lowerKey.includes(field))) {
                    obj[key] = '[REDACTED]';
                } else if (typeof obj[key] === 'object') {
                    this.removeSensitiveFields(obj[key]);
                }
            }
        }
    }

    private addLogEntry(entry: LogEntry): void {
        this.logEntries.push(entry);

        // Keep only the most recent entries
        if (this.logEntries.length > this.maxLogEntries) {
            this.logEntries = this.logEntries.slice(-this.maxLogEntries);
        }
    }

    private formatConsoleMessage(entry: LogEntry): string {
        const timestamp = new Date(entry.timestamp).toLocaleTimeString();
        const userId = entry.userId ? ` [User: ${entry.userId}]` : '';
        return `[${timestamp}] [${entry.category}]${userId} ${entry.message}`;
    }

    debug(category: string, message: string, data?: any, userId?: string): void {
        if (!this.shouldLog(LogLevel.DEBUG)) return;

        const entry = this.createLogEntry(LogLevel.DEBUG, category, message, data, userId);
        this.addLogEntry(entry);
        console.debug(this.formatConsoleMessage(entry), entry.data || '');
    }

    info(category: string, message: string, data?: any, userId?: string): void {
        if (!this.shouldLog(LogLevel.INFO)) return;

        const entry = this.createLogEntry(LogLevel.INFO, category, message, data, userId);
        this.addLogEntry(entry);
        console.info(this.formatConsoleMessage(entry), entry.data || '');
    }

    warn(category: string, message: string, data?: any, userId?: string): void {
        if (!this.shouldLog(LogLevel.WARN)) return;

        const entry = this.createLogEntry(LogLevel.WARN, category, message, data, userId);
        this.addLogEntry(entry);
        console.warn(this.formatConsoleMessage(entry), entry.data || '');
    }

    error(category: string, message: string, error?: any, userId?: string): void {
        if (!this.shouldLog(LogLevel.ERROR)) return;

        const errorData = this.extractErrorInfo(error);
        const entry = this.createLogEntry(LogLevel.ERROR, category, message, errorData, userId);
        this.addLogEntry(entry);
        console.error(this.formatConsoleMessage(entry), errorData);
    }

    private extractErrorInfo(error: any): any {
        if (!error) return null;

        if (error instanceof Error) {
            return {
                name: error.name,
                message: error.message,
                stack: error.stack
            };
        }

        if (typeof error === 'object') {
            return {
                status: error.status,
                statusText: error.statusText,
                message: error.message || error.error?.message,
                url: error.url,
                timestamp: error.timestamp || new Date().toISOString()
            };
        }

        return { error: String(error) };
    }

    // Authentication-specific logging methods
    logAuthEvent(event: string, data?: any, userId?: string): void {
        this.info('AUTH', `Authentication event: ${event}`, data, userId);
    }

    logAuthError(event: string, error: any, userId?: string): void {
        this.error('AUTH', `Authentication error: ${event}`, error, userId);
    }

    logAuthStateChange(fromState: string, toState: string, reason?: string, userId?: string): void {
        this.info('AUTH', `State change: ${fromState} -> ${toState}`, { reason }, userId);
    }

    logLogoutEvent(reason: string, silent: boolean, userId?: string): void {
        this.info('AUTH', `Logout initiated`, { reason, silent }, userId);
    }

    logServerLogoutResult(success: boolean, error?: any, userId?: string): void {
        if (success) {
            this.info('AUTH', 'Server logout successful', undefined, userId);
        } else {
            this.warn('AUTH', 'Server logout failed', error, userId);
        }
    }

    // Get recent log entries for debugging
    getRecentLogs(count: number = 50, category?: string): LogEntry[] {
        let logs = this.logEntries;

        if (category) {
            logs = logs.filter(entry => entry.category === category);
        }

        return logs.slice(-count);
    }

    // Clear log entries
    clearLogs(): void {
        this.logEntries = [];
        this.info('LOGGING', 'Log entries cleared');
    }

    // Export logs for debugging
    exportLogs(): string {
        return JSON.stringify(this.logEntries, null, 2);
    }
}