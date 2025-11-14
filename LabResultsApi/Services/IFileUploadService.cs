using LabResultsApi.DTOs;

namespace LabResultsApi.Services;

public interface IFileUploadService
{
    /// <summary>
    /// Upload a file and associate it with a sample and trial
    /// </summary>
    Task<FileUploadResponseDto> UploadFileAsync(IFormFile file, FileUploadRequestDto request, string uploadedBy);

    /// <summary>
    /// Get files associated with a specific sample and test
    /// </summary>
    Task<FileListDto> GetFilesAsync(int sampleId, int testId, int? trialNumber = null);

    /// <summary>
    /// Get file information by ID
    /// </summary>
    Task<FileUploadDto?> GetFileAsync(int fileId);

    /// <summary>
    /// Download file content by ID
    /// </summary>
    Task<(byte[] content, string fileName, string contentType)?> DownloadFileAsync(int fileId);

    /// <summary>
    /// Delete a file by ID
    /// </summary>
    Task<bool> DeleteFileAsync(int fileId, string deletedBy);

    /// <summary>
    /// Get file preview information
    /// </summary>
    Task<List<FilePreviewDto>> GetFilePreviewsAsync(int sampleId, int testId);

    /// <summary>
    /// Validate file before upload
    /// </summary>
    Task<(bool isValid, List<string> errors)> ValidateFileAsync(IFormFile file, FileUploadRequestDto request);

    /// <summary>
    /// Get supported file extensions for a test type
    /// </summary>
    Task<List<string>> GetSupportedExtensionsAsync(int testId);
}