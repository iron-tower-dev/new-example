import { Injectable, inject } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

export interface ApiErrorResponse {
    message: string;
    errors?: { [key: string]: string[] };
    statusCode: number;
    timestamp: string;
    path: string;
}

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
    private snackBar = inject(MatSnackBar);
    private router = inject(Router);

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            catchError((error: HttpErrorResponse) => {
                this.handleError(error);
                return throwError(() => error);
            })
        );
    }

    private handleError(error: HttpErrorResponse): void {
        let errorMessage = 'An unexpected error occurred';
        let errorDetails: string[] = [];

        switch (error.status) {
            case 400: // Bad Request
                errorMessage = 'Invalid request data';
                if (error.error?.errors) {
                    errorDetails = this.extractValidationErrors(error.error.errors);
                } else if (error.error?.message) {
                    errorMessage = error.error.message;
                }
                this.showValidationErrors(errorMessage, errorDetails);
                break;

            case 401: // Unauthorized
                errorMessage = 'Authentication required';
                this.showErrorSnackBar(errorMessage);
                // Only redirect if not already on login page
                if (!this.router.url.includes('/login')) {
                    this.router.navigate(['/login']);
                }
                break;

            case 403: // Forbidden
                errorMessage = 'Access denied';
                this.showErrorSnackBar(errorMessage);
                break;

            case 404: // Not Found
                errorMessage = 'Resource not found';
                this.showErrorSnackBar(errorMessage);
                break;

            case 409: // Conflict
                errorMessage = 'Data conflict occurred';
                if (error.error?.message) {
                    errorMessage = error.error.message;
                }
                this.showErrorSnackBar(errorMessage);
                break;

            case 422: // Unprocessable Entity
                errorMessage = 'Data validation failed';
                if (error.error?.errors) {
                    errorDetails = this.extractValidationErrors(error.error.errors);
                }
                this.showValidationErrors(errorMessage, errorDetails);
                break;

            case 500: // Internal Server Error
                errorMessage = 'Server error occurred';
                this.showServerError(error);
                break;

            case 503: // Service Unavailable
                errorMessage = 'Service temporarily unavailable';
                this.showErrorSnackBar(errorMessage);
                break;

            case 0: // Network Error
                errorMessage = 'Network connection error';
                this.showErrorSnackBar(errorMessage);
                break;

            default:
                if (error.error?.message) {
                    errorMessage = error.error.message;
                } else {
                    errorMessage = `HTTP Error ${error.status}: ${error.statusText}`;
                }
                this.showErrorSnackBar(errorMessage);
                break;
        }

        // Log error for debugging
        this.logError(error, errorMessage, errorDetails);
    }

    private extractValidationErrors(errors: { [key: string]: string[] }): string[] {
        const errorMessages: string[] = [];

        Object.keys(errors).forEach(field => {
            const fieldErrors = errors[field];
            if (Array.isArray(fieldErrors)) {
                fieldErrors.forEach(errorMsg => {
                    errorMessages.push(`${field}: ${errorMsg}`);
                });
            }
        });

        return errorMessages;
    }

    private showValidationErrors(message: string, details: string[]): void {
        if (details.length > 0) {
            // Show detailed validation errors
            const detailsText = details.join('\n');
            this.snackBar.open(`${message}\n${detailsText}`, 'Close', {
                duration: 8000,
                panelClass: ['error-snackbar', 'validation-error-snackbar'],
                verticalPosition: 'top'
            });
        } else {
            this.showErrorSnackBar(message);
        }
    }

    private showServerError(error: HttpErrorResponse): void {
        const errorId = this.generateErrorId();
        const message = `Server error occurred (ID: ${errorId})`;

        this.snackBar.open(message, 'Close', {
            duration: 10000,
            panelClass: ['error-snackbar', 'server-error-snackbar'],
            verticalPosition: 'top'
        });

        // Log detailed server error
        console.error(`Server Error [${errorId}]:`, {
            status: error.status,
            statusText: error.statusText,
            url: error.url,
            error: error.error,
            timestamp: new Date().toISOString()
        });
    }

    private showErrorSnackBar(message: string): void {
        this.snackBar.open(message, 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar'],
            verticalPosition: 'top'
        });
    }

    private logError(error: HttpErrorResponse, message: string, details: string[]): void {
        const errorLog = {
            timestamp: new Date().toISOString(),
            message,
            details,
            httpError: {
                status: error.status,
                statusText: error.statusText,
                url: error.url,
                headers: error.headers
            },
            userAgent: navigator.userAgent,
            url: window.location.href
        };

        console.error('HTTP Error:', errorLog);

        // In production, you might want to send this to a logging service
        if (error.status >= 500) {
            this.sendErrorToLoggingService(errorLog);
        }
    }

    private generateErrorId(): string {
        return Math.random().toString(36).substr(2, 9).toUpperCase();
    }

    private sendErrorToLoggingService(errorLog: any): void {
        // Implement logging service integration here
        // For example, send to Application Insights, Sentry, etc.
        console.log('Would send to logging service:', errorLog);
    }
}