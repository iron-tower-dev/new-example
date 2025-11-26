import { Injectable, inject } from '@angular/core';
import { ValidationService } from './validation.service';

@Injectable({
    providedIn: 'root'
})
export class AppInitializationService {
    private validationService = inject(ValidationService);

    /**
     * Initialize application services and configurations
     */
    async initialize(): Promise<void> {
        try {
            // Initialize validation rules
            this.initializeValidationRules();

            // Add other initialization tasks here
            console.log('Application initialization completed');
        } catch (error) {
            console.error('Error during application initialization:', error);
            throw error;
        }
    }

    /**
     * Initialize validation rules for all test types
     */
    private initializeValidationRules(): void {
        // Initialize default test validation rules
        this.validationService.initializeDefaultTestRules();

        console.log('Validation rules initialized');
    }
}

// Factory function for APP_INITIALIZER
export function initializeApp(appInitService: AppInitializationService) {
    return () => appInitService.initialize();
}