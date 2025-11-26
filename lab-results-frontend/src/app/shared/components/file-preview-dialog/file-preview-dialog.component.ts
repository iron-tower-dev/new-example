import { Component, Inject, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { FileUploadService, FileInfo } from '../../services/file-upload.service';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

export interface FilePreviewDialogData {
    file: FileInfo;
}

@Component({
    selector: 'app-file-preview-dialog',
    standalone: true,
    imports: [
        CommonModule,
        MatDialogModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatSnackBarModule
    ],
    template: `
    <div class="file-preview-dialog">
      <div mat-dialog-title class="dialog-header">
        <div class="file-info">
          <mat-icon>{{ getFileIcon() }}</mat-icon>
          <div>
            <h2>{{ data.file.originalFileName }}</h2>
            <p class="file-details">
              {{ formatFileSize(data.file.fileSize) }} • 
              {{ data.file.uploadDate | date:'medium' }}
            </p>
          </div>
        </div>
        <button mat-icon-button 
                mat-dialog-close
                aria-label="Close">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div mat-dialog-content class="dialog-content">
        @if (isLoading()) {
          <div class="loading-container">
            <mat-spinner diameter="50"></mat-spinner>
            <p>Loading preview...</p>
          </div>
        } @else if (previewUrl()) {
          <div class="preview-container">
            @if (isImageFile()) {
              <img [src]="previewUrl()" 
                   [alt]="data.file.originalFileName"
                   class="image-preview">
            } @else if (isPdfFile()) {
              <iframe [src]="previewUrl()" 
                      class="pdf-preview"
                      frameborder="0">
              </iframe>
            } @else if (isTextFile()) {
              <pre class="text-preview">{{ textContent() }}</pre>
            } @else {
              <div class="no-preview">
                <mat-icon>description</mat-icon>
                <p>Preview not available for this file type</p>
                <button mat-raised-button 
                        color="primary"
                        (click)="downloadFile()">
                  <mat-icon>download</mat-icon>
                  Download File
                </button>
              </div>
            }
          </div>
        } @else {
          <div class="error-container">
            <mat-icon color="warn">error</mat-icon>
            <p>Failed to load file preview</p>
            <button mat-raised-button 
                    color="primary"
                    (click)="downloadFile()">
              <mat-icon>download</mat-icon>
              Download File
            </button>
          </div>
        }
      </div>

      <div mat-dialog-actions class="dialog-actions">
        <button mat-button (click)="downloadFile()">
          <mat-icon>download</mat-icon>
          Download
        </button>
        <button mat-button mat-dialog-close>Close</button>
      </div>
    </div>
  `,
    styles: [`
    .file-preview-dialog {
      width: 80vw;
      max-width: 1000px;
      height: 80vh;
      max-height: 800px;
      display: flex;
      flex-direction: column;
    }

    .dialog-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      padding: 16px 24px;
      border-bottom: 1px solid #e0e0e0;
    }

    .file-info {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .file-info mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
      color: #666;
    }

    .file-info h2 {
      margin: 0;
      font-size: 18px;
      font-weight: 500;
    }

    .file-details {
      margin: 4px 0 0 0;
      font-size: 14px;
      color: #666;
    }

    .dialog-content {
      flex: 1;
      padding: 0;
      overflow: hidden;
    }

    .preview-container {
      width: 100%;
      height: 100%;
      display: flex;
      justify-content: center;
      align-items: center;
      padding: 16px;
    }

    .image-preview {
      max-width: 100%;
      max-height: 100%;
      object-fit: contain;
      border-radius: 4px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .pdf-preview {
      width: 100%;
      height: 100%;
      border-radius: 4px;
    }

    .text-preview {
      width: 100%;
      height: 100%;
      overflow: auto;
      padding: 16px;
      margin: 0;
      font-family: 'Courier New', monospace;
      font-size: 14px;
      line-height: 1.4;
      background-color: #f5f5f5;
      border-radius: 4px;
    }

    .loading-container,
    .error-container,
    .no-preview {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      gap: 16px;
      color: #666;
    }

    .loading-container p,
    .error-container p,
    .no-preview p {
      margin: 0;
      font-size: 16px;
    }

    .error-container mat-icon,
    .no-preview mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
    }

    .dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid #e0e0e0;
      justify-content: flex-end;
      gap: 8px;
    }
  `]
})
export class FilePreviewDialogComponent {
    private fileUploadService = inject(FileUploadService);
    private snackBar = inject(MatSnackBar);
    private sanitizer = inject(DomSanitizer);
    private dialogRef = inject(MatDialogRef<FilePreviewDialogComponent>);

    // Signals
    private _isLoading = signal(true);
    private _previewUrl = signal<SafeResourceUrl | null>(null);
    private _textContent = signal<string>('');

    readonly isLoading = this._isLoading.asReadonly();
    readonly previewUrl = this._previewUrl.asReadonly();
    readonly textContent = this._textContent.asReadonly();

    constructor(@Inject(MAT_DIALOG_DATA) public data: FilePreviewDialogData) {
        this.loadPreview();
    }

    private loadPreview(): void {
        if (!this.canPreviewFile()) {
            this._isLoading.set(false);
            return;
        }

        this.fileUploadService.downloadFile(this.data.file.id).subscribe({
            next: (blob) => {
                if (this.isTextFile()) {
                    // Read text content
                    const reader = new FileReader();
                    reader.onload = () => {
                        this._textContent.set(reader.result as string);
                        this._isLoading.set(false);
                    };
                    reader.readAsText(blob);
                } else {
                    // Create object URL for images and PDFs
                    const url = URL.createObjectURL(blob);
                    const safeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
                    this._previewUrl.set(safeUrl);
                    this._isLoading.set(false);
                }
            },
            error: (error) => {
                console.error('Failed to load file for preview:', error);
                this._isLoading.set(false);
                this.snackBar.open('Failed to load file preview', 'Close', { duration: 3000 });
            }
        });
    }

    downloadFile(): void {
        this.fileUploadService.downloadFile(this.data.file.id).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = this.data.file.originalFileName;
                link.click();
                window.URL.revokeObjectURL(url);
            },
            error: (error) => {
                this.snackBar.open(`Download failed: ${error.message}`, 'Close', { duration: 3000 });
            }
        });
    }

    formatFileSize(bytes: number): string {
        return this.fileUploadService.formatFileSize(bytes);
    }

    getFileIcon(): string {
        if (this.isImageFile()) return 'image';
        if (this.isPdfFile()) return 'picture_as_pdf';
        if (this.isTextFile()) return 'description';
        return 'insert_drive_file';
    }

    isImageFile(): boolean {
        return this.data.file.contentType.startsWith('image/');
    }

    isPdfFile(): boolean {
        return this.data.file.contentType === 'application/pdf';
    }

    isTextFile(): boolean {
        return this.data.file.contentType.startsWith('text/') ||
            this.data.file.contentType === 'text/csv';
    }

    canPreviewFile(): boolean {
        return this.fileUploadService.canPreviewFile(this.data.file.contentType);
    }
}