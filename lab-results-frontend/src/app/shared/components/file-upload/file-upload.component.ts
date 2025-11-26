import { Component, Input, Output, EventEmitter, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { FileUploadService, FileUploadRequest, FileInfo, UploadProgress } from '../../services/file-upload.service';
import { FilePreviewDialogComponent } from '../file-preview-dialog/file-preview-dialog.component';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatCardModule,
    MatListModule,
    MatDialogModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatChipsModule
  ],
  template: `
    <div class="file-upload-container">
      <!-- Upload Area -->
      <div class="upload-area" 
           [class.drag-over]="isDragOver()"
           (dragover)="onDragOver($event)"
           (dragleave)="onDragLeave($event)"
           (drop)="onDrop($event)"
           (click)="fileInput.click()">
        
        <input #fileInput 
               type="file" 
               [multiple]="allowMultiple"
               [accept]="acceptedTypes()"
               (change)="onFileSelected($event)"
               style="display: none;">
        
        <div class="upload-content">
          <mat-icon class="upload-icon">cloud_upload</mat-icon>
          <h3>Drop files here or click to browse</h3>
          <p class="upload-hint">
            @if (supportedExtensions().length > 0) {
              Supported formats: {{ supportedExtensions().join(', ') }}
            }
            @if (maxFileSize > 0) {
              <br>Maximum file size: {{ formatFileSize(maxFileSize) }}
            }
          </p>
        </div>
      </div>

      <!-- Supported Extensions Chips -->
      @if (supportedExtensions().length > 0) {
        <div class="extensions-chips">
          <span class="extensions-label">Supported formats:</span>
          @for (ext of supportedExtensions(); track ext) {
            <mat-chip>{{ ext }}</mat-chip>
          }
        </div>
      }

      <!-- Upload Progress -->
      @if (uploadProgress().length > 0) {
        <mat-card class="progress-card">
          <mat-card-header>
            <mat-card-title>Upload Progress</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            @for (progress of uploadProgress(); track progress.fileId) {
              <div class="progress-item">
                <div class="progress-info">
                  <span class="file-name">{{ progress.fileName }}</span>
                  <span class="progress-status" [class]="'status-' + progress.status">
                    @switch (progress.status) {
                      @case ('uploading') {
                        <mat-icon>cloud_upload</mat-icon>
                        Uploading...
                      }
                      @case ('completed') {
                        <mat-icon>check_circle</mat-icon>
                        Completed
                      }
                      @case ('error') {
                        <mat-icon>error</mat-icon>
                        Error
                      }
                    }
                  </span>
                </div>
                @if (progress.status === 'uploading') {
                  <mat-progress-bar mode="determinate" [value]="progress.progress"></mat-progress-bar>
                }
                @if (progress.status === 'error' && progress.error) {
                  <div class="error-message">{{ progress.error }}</div>
                }
              </div>
            }
          </mat-card-content>
          <mat-card-actions>
            <button mat-button (click)="clearProgress()">Clear</button>
          </mat-card-actions>
        </mat-card>
      }

      <!-- File List -->
      @if (files().length > 0) {
        <mat-card class="files-card">
          <mat-card-header>
            <mat-card-title>Uploaded Files</mat-card-title>
            <mat-card-subtitle>{{ files().length }} file(s)</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <mat-list>
              @for (file of files(); track file.id) {
                <mat-list-item>
                  <mat-icon matListItemIcon>
                    @if (isImageFile(file.contentType)) {
                      image
                    } @else if (isPdfFile(file.contentType)) {
                      picture_as_pdf
                    } @else {
                      description
                    }
                  </mat-icon>
                  
                  <div matListItemTitle>{{ file.originalFileName }}</div>
                  <div matListItemLine>
                    {{ formatFileSize(file.fileSize) }} • 
                    {{ file.uploadDate | date:'short' }} • 
                    {{ file.uploadedBy }}
                  </div>
                  
                  <div matListItemMeta class="file-actions">
                    @if (canPreviewFile(file.contentType)) {
                      <button mat-icon-button 
                              matTooltip="Preview"
                              (click)="previewFile(file)">
                        <mat-icon>visibility</mat-icon>
                      </button>
                    }
                    
                    <button mat-icon-button 
                            matTooltip="Download"
                            (click)="downloadFile(file)">
                      <mat-icon>download</mat-icon>
                    </button>
                    
                    @if (allowDelete) {
                      <button mat-icon-button 
                              matTooltip="Delete"
                              color="warn"
                              (click)="deleteFile(file)">
                        <mat-icon>delete</mat-icon>
                      </button>
                    }
                  </div>
                </mat-list-item>
              }
            </mat-list>
          </mat-card-content>
        </mat-card>
      }

      <!-- Error Display -->
      @if (fileUploadService.hasError()) {
        <mat-card class="error-card">
          <mat-card-content>
            <div class="error-content">
              <mat-icon color="warn">error</mat-icon>
              <span>{{ fileUploadService.error() }}</span>
              <button mat-icon-button (click)="clearError()">
                <mat-icon>close</mat-icon>
              </button>
            </div>
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .file-upload-container {
      width: 100%;
      max-width: 800px;
      margin: 0 auto;
    }

    .upload-area {
      border: 2px dashed #ccc;
      border-radius: 8px;
      padding: 40px 20px;
      text-align: center;
      cursor: pointer;
      transition: all 0.3s ease;
      background-color: #fafafa;
      margin-bottom: 20px;
    }

    .upload-area:hover,
    .upload-area.drag-over {
      border-color: #2196f3;
      background-color: #e3f2fd;
    }

    .upload-content {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 10px;
    }

    .upload-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #666;
    }

    .upload-hint {
      color: #666;
      font-size: 14px;
      margin: 0;
    }

    .extensions-chips {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 20px;
      flex-wrap: wrap;
    }

    .extensions-label {
      font-weight: 500;
      color: #666;
    }

    .progress-card,
    .files-card,
    .error-card {
      margin-bottom: 20px;
    }

    .progress-item {
      margin-bottom: 16px;
    }

    .progress-info {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;
    }

    .file-name {
      font-weight: 500;
      flex: 1;
      text-overflow: ellipsis;
      overflow: hidden;
      white-space: nowrap;
    }

    .progress-status {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 14px;
    }

    .status-uploading {
      color: #2196f3;
    }

    .status-completed {
      color: #4caf50;
    }

    .status-error {
      color: #f44336;
    }

    .error-message {
      color: #f44336;
      font-size: 12px;
      margin-top: 4px;
    }

    .file-actions {
      display: flex;
      gap: 4px;
    }

    .error-card {
      background-color: #ffebee;
    }

    .error-content {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .error-content span {
      flex: 1;
    }

    mat-list-item {
      border-bottom: 1px solid #eee;
    }

    mat-list-item:last-child {
      border-bottom: none;
    }
  `]
})
export class FileUploadComponent {
  @Input() sampleId!: number;
  @Input() testId!: number;
  @Input() trialNumber!: number;
  @Input() allowMultiple = true;
  @Input() allowDelete = true;
  @Input() maxFileSize = 50 * 1024 * 1024; // 50MB
  @Input() uploadedBy = 'System';

  @Output() fileUploaded = new EventEmitter<FileInfo>();
  @Output() fileDeleted = new EventEmitter<number>();
  @Output() uploadError = new EventEmitter<string>();

  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  protected fileUploadService = inject(FileUploadService);

  // Local signals
  private _isDragOver = signal(false);
  private _supportedExtensions = signal<string[]>([]);

  // Public signals
  readonly isDragOver = this._isDragOver.asReadonly();
  readonly supportedExtensions = this._supportedExtensions.asReadonly();
  readonly files = this.fileUploadService.files;
  readonly uploadProgress = this.fileUploadService.uploadProgress;

  // Computed signals
  readonly acceptedTypes = computed(() => {
    const extensions = this._supportedExtensions();
    return extensions.length > 0 ? extensions.join(',') : '*';
  });

  constructor() {
    // Load supported extensions when component initializes
    effect(() => {
      if (this.testId) {
        this.loadSupportedExtensions();
      }
    });

    // Load existing files when inputs change
    effect(() => {
      if (this.sampleId && this.testId && this.trialNumber) {
        this.loadFiles();
      }
    });
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this._isDragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this._isDragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this._isDragOver.set(false);

    const files = Array.from(event.dataTransfer?.files || []);
    if (files.length > 0) {
      this.handleFiles(files);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files || []);
    if (files.length > 0) {
      this.handleFiles(files);
    }
    // Clear the input so the same file can be selected again
    input.value = '';
  }

  private handleFiles(files: File[]): void {
    if (!this.allowMultiple && files.length > 1) {
      this.snackBar.open('Only one file can be uploaded at a time', 'Close', { duration: 3000 });
      return;
    }

    const request: FileUploadRequest = {
      sampleId: this.sampleId,
      testId: this.testId,
      trialNumber: this.trialNumber,
      uploadedBy: this.uploadedBy
    };

    if (files.length === 1) {
      this.uploadSingleFile(files[0], request);
    } else {
      this.uploadMultipleFiles(files, request);
    }
  }

  private uploadSingleFile(file: File, request: FileUploadRequest): void {
    // Validate file first
    this.fileUploadService.validateFile(file, request).subscribe({
      next: (validation) => {
        if (validation.isValid) {
          this.performUpload(file, request);
        } else {
          const errorMessage = validation.errors.join(', ');
          this.snackBar.open(`File validation failed: ${errorMessage}`, 'Close', { duration: 5000 });
          this.uploadError.emit(errorMessage);
        }
      },
      error: (error) => {
        this.snackBar.open(`Validation error: ${error.message}`, 'Close', { duration: 5000 });
        this.uploadError.emit(error.message);
      }
    });
  }

  private uploadMultipleFiles(files: File[], request: FileUploadRequest): void {
    this.fileUploadService.uploadMultipleFiles(files, request).subscribe({
      next: (responses) => {
        const successful = responses.filter(r => r.success).length;
        const failed = responses.filter(r => !r.success).length;

        if (successful > 0) {
          this.snackBar.open(`${successful} file(s) uploaded successfully`, 'Close', { duration: 3000 });
          responses.filter(r => r.success && r.fileInfo).forEach(r => {
            this.fileUploaded.emit(r.fileInfo!);
          });
        }

        if (failed > 0) {
          const errors = responses.filter(r => !r.success).map(r => r.message).join(', ');
          this.snackBar.open(`${failed} file(s) failed to upload: ${errors}`, 'Close', { duration: 5000 });
          this.uploadError.emit(errors);
        }
      },
      error: (error) => {
        this.snackBar.open(`Upload error: ${error.message}`, 'Close', { duration: 5000 });
        this.uploadError.emit(error.message);
      }
    });
  }

  private performUpload(file: File, request: FileUploadRequest): void {
    this.fileUploadService.uploadFile(file, request).subscribe({
      next: (response) => {
        if (response && response.success) {
          this.snackBar.open('File uploaded successfully', 'Close', { duration: 3000 });
          if (response.fileInfo) {
            this.fileUploaded.emit(response.fileInfo);
          }
        } else if (response && !response.success) {
          const errorMessage = response.errors.join(', ');
          this.snackBar.open(`Upload failed: ${errorMessage}`, 'Close', { duration: 5000 });
          this.uploadError.emit(errorMessage);
        }
      },
      error: (error) => {
        this.snackBar.open(`Upload error: ${error.message}`, 'Close', { duration: 5000 });
        this.uploadError.emit(error.message);
      }
    });
  }

  downloadFile(file: FileInfo): void {
    this.fileUploadService.downloadFile(file.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = file.originalFileName;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        this.snackBar.open(`Download failed: ${error.message}`, 'Close', { duration: 3000 });
      }
    });
  }

  deleteFile(file: FileInfo): void {
    if (confirm(`Are you sure you want to delete "${file.originalFileName}"?`)) {
      this.fileUploadService.deleteFile(file.id, this.uploadedBy).subscribe({
        next: (response) => {
          if (response.success) {
            this.snackBar.open('File deleted successfully', 'Close', { duration: 3000 });
            this.fileDeleted.emit(file.id);
          } else {
            this.snackBar.open(`Delete failed: ${response.message}`, 'Close', { duration: 3000 });
          }
        },
        error: (error) => {
          this.snackBar.open(`Delete error: ${error.message}`, 'Close', { duration: 3000 });
        }
      });
    }
  }

  previewFile(file: FileInfo): void {
    this.dialog.open(FilePreviewDialogComponent, {
      data: { file },
      maxWidth: '90vw',
      maxHeight: '90vh',
      panelClass: 'file-preview-dialog-panel'
    });
  }

  clearProgress(): void {
    this.fileUploadService.clearUploadProgress();
  }

  clearError(): void {
    this.fileUploadService.clearError();
  }

  formatFileSize(bytes: number): string {
    return this.fileUploadService.formatFileSize(bytes);
  }

  isImageFile(contentType: string): boolean {
    return contentType.startsWith('image/');
  }

  isPdfFile(contentType: string): boolean {
    return contentType === 'application/pdf';
  }

  canPreviewFile(contentType: string): boolean {
    return this.fileUploadService.canPreviewFile(contentType);
  }

  private loadSupportedExtensions(): void {
    this.fileUploadService.getSupportedExtensions(this.testId).subscribe({
      next: (result) => {
        this._supportedExtensions.set(result.supportedExtensions);
      },
      error: (error) => {
        console.error('Failed to load supported extensions:', error);
      }
    });
  }

  private loadFiles(): void {
    this.fileUploadService.getFiles(this.sampleId, this.testId, this.trialNumber).subscribe({
      next: (fileList) => {
        // Files are automatically updated in the service
      },
      error: (error) => {
        console.error('Failed to load files:', error);
      }
    });
  }
}