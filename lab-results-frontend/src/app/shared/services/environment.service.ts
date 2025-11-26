import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class EnvironmentService {
    private readonly config = environment;

    get apiUrl(): string {
        return this.config.apiUrl;
    }

    get production(): boolean {
        return this.config.production;
    }

    get apiTimeout(): number {
        return this.config.apiTimeout;
    }

    get enableLogging(): boolean {
        return this.config.enableLogging;
    }

    /**
     * Get full API endpoint URL
     */
    getApiEndpoint(path: string): string {
        // Remove leading slash if present
        const cleanPath = path.startsWith('/') ? path.substring(1) : path;
        return `${this.apiUrl}/${cleanPath}`;
    }

    /**
     * Check if we're running in development mode
     */
    isDevelopment(): boolean {
        return !this.production;
    }
}