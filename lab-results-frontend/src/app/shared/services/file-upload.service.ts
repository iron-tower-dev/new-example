import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpEventType, HttpRequest } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { EnvironmentService } from './environment.service';
import { map, tap, catchError } from 'rxjs/operators';

export interface FileUploadRequest {
    sampleId: number;
    testId: number;
    trialNumber: number;
    description?: string;
    uploadedBy?: string;
}

export interface FileUploadResponse {
    success: boolean;
    message: string;
    fileInfo?: FileInfo;
    errors: string[];
}

export interface FileInfo {
    id: number;
    fileName: string;
    originalFileName: string;
    contentType: string;
    fileSize: number;
    filePath: string;
    sampleId: number;
    testId: number;
    trialNumber: number;
    uploadDate: Date;
    uploadedBy: string;
    status: string;
}

export interface FileList {
    files: FileInfo[];
    totalCount: number;
}

export interface FilePreview {
    id: number;
    fileName: string;
    contentType: string;
    fileSize: number;
    uploadDate: Date;
    status: string;
    canPreview: boolean;
}

export interface FileValidationResult {
    isValid: boolean;
    errors: string[];
}

export interface SupportedExtensions {
    testId: number;
    supportedExtensions: string[];
}

export interface UploadProgress {
    fileId?: string;
    fileName: string;
    progress: number;
    status: 'uploading' | 'completed' | 'error';
    error?: string;
}

@Injectable({
    providedIn: 'root'
})
export class FileUploadService {
    private http = inject(HttpClient);
    private environment = inject(EnvironmentService);
    private readonly baseUrl = this.environment.getApiEndpoint('files');

    // Signals for reactive state management
    private _files = signal<FileInfo[]>([]);
    private _filePreviews = signal<FilePreview[]>([]);
    private _uploadProgress = signal<UploadProgress[]>([]);
    private _isLoading = signal(false);
    private _error = signal<string | null>(null);
    private _supportedExtensions = signal<string[]>([]);

    // Public readonly signals
    readonly files = this._files.asReadonly();
    readonly filePreviews = this._filePreviews.asReadonly();
    readonly uploadProgress = this._uploadProgress.asReadonly();
    readonly isLoading = this._isLoading.asReadonly();
    readonly error = this._error.asReadonly();
    readonly supportedExtensions = this._supportedExtensions.asReadonly();

    // Computed signals
    readonly hasFiles = computed(() => this._files().length > 0);
    readonly hasError = computed(() => this._error() !== null);
    readonly isUploading = computed(() =>
        this._uploadProgress().some(p => p.status === 'uploading')
    );
    readonly completedUploads = computed(() =>
        this._uploadProgress().filter(p => p.status === 'completed').length
    );
    readonly failedUploads = computed(() =>
        this._uploadProgress().filter(p => p.status === 'error').length
    );

    /**
     * Upload a file with progress tracking
     */
    uploadFile(file: File, request: FileUploadRequest): Observable<FileUploadResponse> {
        this._error.set(null);

        const formData = new FormData();
        formData.append('file', file);
        formData.append('sampleId', request.sampleId.toString());
        formData.append('testId', request.testId.toString());
        formData.append('trialNumber', request.trialNumber.toString());
        if (request.description) {
            formData.append('description', request.description);
        }
        if (request.uploadedBy) {
            formData.append('uploadedBy', request.uploadedBy);
        }

        // Create upload progress entry
        const fileId = this.generateFileId();
        const progressEntry: UploadProgress = {
            fileId,
            fileName: file.name,
            progress: 0,
            status: 'uploading'
        };

        this._uploadProgress.update(progress => [...progress, progressEntry]);

        const httpRequest = new HttpRequest('POST', `${this.baseUrl}/upload`, formData, {
            reportProgress: true
        });

        return this.http.request<FileUploadResponse>(httpRequest).pipe(
            map(event => {
                if (event.type === HttpEventType.UploadProgress && event.total) {
                    const progress = Math.round(100 * event.loaded / event.total);
                    this.updateUploadProgress(fileId, progress, 'uploading');
                } else if (event.type === HttpEventType.Response) {
                    const response = event.body as FileUploadResponse;
                    if (response.success) {
                        this.updateUploadProgress(fileId, 100, 'completed');
                        // Add to files list if successful
                        if (response.fileInfo) {
                            this._files.update(files => [...files, this.mapFileDates(response.fileInfo!)]);
                        }
                    } else {
                        this.updateUploadProgress(fileId, 0, 'error', response.message);
                    }
                    return response;
                }
                return null as any;
            }),
            tap(response => {
                if (response && !response.success) {
                    this._error.set(response.message);
                }
            }),
            catchError(error => {
                this.updateUploadProgress(fileId, 0, 'error', error.message);
                this._error.set(`Upload failed: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Upload multiple files
     */
    uploadMultipleFiles(files: File[], request: FileUploadRequest): Observable<FileUploadResponse[]> {
        const uploads = files.map(file => this.uploadFile(file, request));
        return new Observable(observer => {
            const results: FileUploadResponse[] = [];
            let completed = 0;

            uploads.forEach((upload, index) => {
                upload.subscribe({
                    next: (response) => {
                        if (response) {
                            results[index] = response;
                            completed++;
                            if (completed === files.length) {
                                observer.next(results);
                                observer.complete();
                            }
                        }
                    },
                    error: (error) => {
                        results[index] = {
                            success: false,
                            message: error.message,
                            errors: [error.message]
                        };
                        completed++;
                        if (completed === files.length) {
                            observer.next(results);
                            observer.complete();
                        }
                    }
                });
            });
        });
    }

    /**
     * Get files for a specific sample and test
     */
    getFiles(sampleId: number, testId: number, trialNumber?: number): Observable<FileList> {
        this._isLoading.set(true);
        this._error.set(null);

        const url = trialNumber !== undefined
            ? `${this.baseUrl}/sample/${sampleId}/test/${testId}/trial/${trialNumber}`
            : `${this.baseUrl}/sample/${sampleId}/test/${testId}`;

        return this.http.get<FileList>(url).pipe(
            map(fileList => ({
                ...fileList,
                files: fileList.files.map(this.mapFileDates)
            })),
            tap(fileList => {
                this._files.set(fileList.files);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load files: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get file information by ID
     */
    getFile(fileId: number): Observable<FileInfo> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<FileInfo>(`${this.baseUrl}/${fileId}`).pipe(
            map(this.mapFileDates),
            tap(() => this._isLoading.set(false)),
            catchError(error => {
                this._error.set(`Failed to load file: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Download a file
     */
    downloadFile(fileId: number): Observable<Blob> {
        return this.http.get(`${this.baseUrl}/${fileId}/download`, {
            responseType: 'blob'
        }).pipe(
            catchError(error => {
                this._error.set(`Failed to download file: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Delete a file
     */
    deleteFile(fileId: number, deletedBy?: string): Observable<{ success: boolean; message: string }> {
        const params: Record<string, string> = {};
        if (deletedBy) {
            params['deletedBy'] = deletedBy;
        }

        return this.http.delete<{ success: boolean; message: string }>(`${this.baseUrl}/${fileId}`, { params }).pipe(
            tap(response => {
                if (response.success) {
                    // Remove from files list
                    this._files.update(files => files.filter(f => f.id !== fileId));
                } else {
                    this._error.set(response.message);
                }
            }),
            catchError(error => {
                this._error.set(`Failed to delete file: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Get file previews for a sample and test
     */
    getFilePreviews(sampleId: number, testId: number): Observable<FilePreview[]> {
        this._isLoading.set(true);
        this._error.set(null);

        return this.http.get<FilePreview[]>(`${this.baseUrl}/sample/${sampleId}/test/${testId}/preview`).pipe(
            map(previews => previews.map(this.mapPreviewDates)),
            tap(previews => {
                this._filePreviews.set(previews);
                this._isLoading.set(false);
            }),
            catchError(error => {
                this._error.set(`Failed to load file previews: ${error.message}`);
                this._isLoading.set(false);
                throw error;
            })
        );
    }

    /**
     * Get supported file extensions for a test type
     */
    getSupportedExtensions(testId: number): Observable<SupportedExtensions> {
        return this.http.get<SupportedExtensions>(`${this.baseUrl}/test/${testId}/supported-extensions`).pipe(
            tap(result => {
                this._supportedExtensions.set(result.supportedExtensions);
            }),
            catchError(error => {
                this._error.set(`Failed to load supported extensions: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Validate a file before upload
     */
    validateFile(file: File, request: FileUploadRequest): Observable<FileValidationResult> {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('sampleId', request.sampleId.toString());
        formData.append('testId', request.testId.toString());
        formData.append('trialNumber', request.trialNumber.toString());

        return this.http.post<FileValidationResult>(`${this.baseUrl}/validate`, formData).pipe(
            catchError(error => {
                this._error.set(`File validation failed: ${error.message}`);
                throw error;
            })
        );
    }

    /**
     * Clear upload progress
     */
    clearUploadProgress(): void {
        this._uploadProgress.set([]);
    }

    /**
     * Clear error state
     */
    clearError(): void {
        this._error.set(null);
    }

    /**
     * Clear all data
     */
    clearData(): void {
        this._files.set([]);
        this._filePreviews.set([]);
        this._uploadProgress.set([]);
        this._supportedExtensions.set([]);
        this._error.set(null);
    }

    /**
     * Check if file type is supported
     */
    isFileTypeSupported(fileName: string, supportedExtensions: string[]): boolean {
        const extension = this.getFileExtension(fileName);
        return supportedExtensions.includes(extension.toLowerCase());
    }

    /**
     * Get file extension from filename
     */
    getFileExtension(fileName: string): string {
        return fileName.substring(fileName.lastIndexOf('.'));
    }

    /**
     * Format file size for display
     */
    formatFileSize(bytes: number): string {
        if (bytes === 0) return '0 Bytes';

        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));

        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    /**
     * Check if file can be previewed based on content type
     */
    canPreviewFile(contentType: string): boolean {
        const previewableTypes = [
            'text/plain',
            'text/csv',
            'application/pdf',
            'image/jpeg',
            'image/png',
            'image/gif',
            'image/bmp',
            'image/tiff'
        ];
        return previewableTypes.includes(contentType.toLowerCase());
    }

    private generateFileId(): string {
        return Math.random().toString(36).substring(2, 15);
    }

    private updateUploadProgress(fileId: string, progress: number, status: UploadProgress['status'], error?: string): void {
        this._uploadProgress.update(progressList =>
            progressList.map(p =>
                p.fileId === fileId
                    ? { ...p, progress, status, error }
                    : p
            )
        );
    }

    private mapFileDates(file: any): FileInfo {
        return {
            ...file,
            uploadDate: new Date(file.uploadDate)
        };
    }

    private mapPreviewDates(preview: any): FilePreview {
        return {
            ...preview,
            uploadDate: new Date(preview.uploadDate)
        };
    }
}