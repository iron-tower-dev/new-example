import { Component, Input, Output, EventEmitter, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { FileUploadService, FilePreview } from '../../services/file-upload.service';

@Component({
    selector: 'app-file-selection',
    standalone: true,
    imports: [
        CommonModule,
        MatButtonModule,
        MatIconModule,
        MatListModule,
        MatTooltipModule,
        MatSnackBarModule
    ],
    template: `
    <div class="file-selection-container">
      <!-- File Selection Button -->
      <div class="selection-header">
        <button mat-raised-button 
                color="primary"
                (click)="fileInput.click()"
                [disabled]="isLoading()">
          <mat-icon>attach_file</mat-icon>
          Find File
        </button>
        
        <input #fileInput 
               type="file" 
               [accept]="acceptedTypes()"
               (change)="onFileSelected($event)"
               style="display: none;">
        
        @if (selectedFile()) {
          <span class="selected-file">
            Selected: {{ selectedFile()!.name }}
            <button mat-icon-button 
                    matTooltip="Clear selection"
                    (click)="clearSelection()">
              <mat-icon>close</mat-icon>
            </button>
          </span>
        }
      </div>

      <!-- File Preview List -->
      @if (showPreview && filePreviews().length > 0) {
        <div class="preview-section">
          <h4>Available Files</h4>
          <mat-list class="preview-list">
            @for (preview of filePreviews(); track preview.id) {
              <mat-list-item (click)="selectExistingFile(preview)">
                <mat-icon matListItemIcon>
                  @if (isImageFile(preview.contentType)) {
                    image
                  } @else if (isPdfFile(preview.contentType)) {
                    picture_as_pdf
                  } @else {
                    description
                  }
                </mat-icon>
                
                <div matListItemTitle>{{ preview.fileName }}</div>
                <div matListItemLine>
                  {{ formatFileSize(preview.fileSize) }} • 
                  {{ preview.uploadDate | date:'short' }}
                </div>
                
                <div matListItemMeta>
                  @if (preview.canPreview) {
                    <mat-icon matTooltip="Can be previewed">visibility</mat-icon>
                  }
                </div>
              </mat-list-item>
            }
          </mat-list>
        </div>
      }

      <!-- Supported Extensions Info -->
      @if (supportedExtensions().length > 0) {
        <div class="extensions-info">
          <small>
            Supported formats: {{ supportedExtensions().join(', ') }}
          </small>
        </div>
      }
    </div>
  `,
    styles: [`
    .file-selection-container {
      width: 100%;
    }

    .selection-header {
      display: flex;
      align-items: center;
      gap: 16px;
      margin-bottom: 16px;
    }

    .selected-file {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 12px;
      background-color: #e3f2fd;
      border-radius: 4px;
      font-size: 14px;
    }

    .preview-section {
      margin-top: 16px;
    }

    .preview-section h4 {
      margin: 0 0 8px 0;
      font-size: 14px;
      font-weight: 500;
      color: #666;
    }

    .preview-list {
      max-height: 200px;
      overflow-y: auto;
      border: 1px solid #ddd;
      border-radius: 4px;
    }

    .preview-list mat-list-item {
      cursor: pointer;
      border-bottom: 1px solid #eee;
    }

    .preview-list mat-list-item:hover {
      background-color: #f5f5f5;
    }

    .preview-list mat-list-item:last-child {
      border-bottom: none;
    }

    .extensions-info {
      margin-top: 8px;
      color: #666;
    }
  `]
})
export class FileSelectionComponent {
    @Input() sampleId!: number;
    @Input() testId!: number;
    @Input() showPreview = true;
    @Input() placeholder = 'No file selected';

    @Output() fileSelected = new EventEmitter<File>();
    @Output() existingFileSelected = new EventEmitter<FilePreview>();

    private snackBar = inject(MatSnackBar);
    private fileUploadService = inject(FileUploadService);

    // Local signals
    private _selectedFile = signal<File | null>(null);
    private _supportedExtensions = signal<string[]>([]);
    private _isLoading = signal(false);

    // Public signals
    readonly selectedFile = this._selectedFile.asReadonly();
    readonly supportedExtensions = this._supportedExtensions.asReadonly();
    readonly isLoading = this._isLoading.asReadonly();
    readonly filePreviews = this.fileUploadService.filePreviews;

    // Computed signals
    readonly acceptedTypes = computed(() => {
        const extensions = this._supportedExtensions();
        return extensions.length > 0 ? extensions.join(',') : '*';
    });

    ngOnInit(): void {
        this.loadSupportedExtensions();
        if (this.showPreview) {
            this.loadFilePreviews();
        }
    }

    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        const file = input.files?.[0];

        if (file) {
            // Validate file extension
            const extension = this.getFileExtension(file.name);
            const supportedExtensions = this._supportedExtensions();

            if (supportedExtensions.length > 0 && !supportedExtensions.includes(extension.toLowerCase())) {
                this.snackBar.open(
                    `File type not supported. Supported formats: ${supportedExtensions.join(', ')}`,
                    'Close',
                    { duration: 5000 }
                );
                return;
            }

            this._selectedFile.set(file);
            this.fileSelected.emit(file);
        }

        // Clear the input so the same file can be selected again
        input.value = '';
    }

    selectExistingFile(preview: FilePreview): void {
        this.existingFileSelected.emit(preview);
    }

    clearSelection(): void {
        this._selectedFile.set(null);
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

    private getFileExtension(fileName: string): string {
        return fileName.substring(fileName.lastIndexOf('.'));
    }

    private loadSupportedExtensions(): void {
        if (!this.testId) return;

        this._isLoading.set(true);
        this.fileUploadService.getSupportedExtensions(this.testId).subscribe({
            next: (result) => {
                this._supportedExtensions.set(result.supportedExtensions);
                this._isLoading.set(false);
            },
            error: (error) => {
                console.error('Failed to load supported extensions:', error);
                this._isLoading.set(false);
            }
        });
    }

    private loadFilePreviews(): void {
        if (!this.sampleId || !this.testId) return;

        this._isLoading.set(true);
        this.fileUploadService.getFilePreviews(this.sampleId, this.testId).subscribe({
            next: () => {
                this._isLoading.set(false);
            },
            error: (error) => {
                console.error('Failed to load file previews:', error);
                this._isLoading.set(false);
            }
        });
    }
}