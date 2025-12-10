using LabResultsApi.DTOs;
using LabResultsApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LabResultsApi.Services;

public class FileUploadService : IFileUploadService
{
    private readonly LabDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileUploadService> _logger;

    // Supported file extensions by test type
    private readonly Dictionary<string, List<string>> _supportedExtensions = new()
    {
        { "default", new() { ".dat", ".txt", ".csv", ".pdf" } },
        { "EmissionSpectroscopy", new() { ".dat", ".txt", ".csv" } },
        { "RBOT", new() { ".dat", ".txt" } },
        { "TFOUT", new() { ".dat", ".txt" } },
        { "ParticleCount", new() { ".dat", ".txt", ".csv" } },
        { "FTIR", new() { ".dat", ".txt", ".csv" } },
        { "Ferrography", new() { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".pdf" } },
        { "InspectFilter", new() { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".pdf" } }
    };

    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB
    private const int MaxFilesPerTrial = 10;

    public FileUploadService(
        LabDbContext context,
        IConfiguration configuration,
        ILogger<FileUploadService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<FileUploadResponseDto> UploadFileAsync(IFormFile file, FileUploadRequestDto request, string uploadedBy)
    {
        try
        {
            // Validate the file
            var (isValid, errors) = await ValidateFileAsync(file, request);
            if (!isValid)
            {
                return new FileUploadResponseDto
                {
                    Success = false,
                    Message = "File validation failed",
                    Errors = errors
                };
            }

            // Create upload directory if it doesn't exist
            var uploadPath = GetUploadPath(request.SampleId, request.TestId);
            Directory.CreateDirectory(uploadPath);

            // Generate unique filename
            var fileName = GenerateUniqueFileName(file.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save file information to database
            var fileInfo = new FileUploadDto
            {
                FileName = fileName,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FilePath = filePath,
                SampleId = request.SampleId,
                TestId = request.TestId,
                TrialNumber = request.TrialNumber,
                UploadDate = DateTime.UtcNow,
                UploadedBy = uploadedBy,
                Status = "Active"
            };

            var fileId = await SaveFileInfoAsync(fileInfo);
            fileInfo.Id = fileId;

            _logger.LogInformation("File uploaded successfully: {FileName} for Sample {SampleId}, Test {TestId}, Trial {TrialNumber}",
                file.FileName, request.SampleId, request.TestId, request.TrialNumber);

            return new FileUploadResponseDto
            {
                Success = true,
                Message = "File uploaded successfully",
                FileInfo = fileInfo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} for Sample {SampleId}", file.FileName, request.SampleId);
            return new FileUploadResponseDto
            {
                Success = false,
                Message = "An error occurred while uploading the file",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<FileListDto> GetFilesAsync(int sampleId, int testId, int? trialNumber = null)
    {
        try
        {
            var sql = @"
                SELECT Id, FileName, OriginalFileName, ContentType, FileSize, FilePath,
                       SampleId, TestId, TrialNumber, UploadDate, UploadedBy, Status
                FROM FileUploads 
                WHERE SampleId = {0} AND TestId = {1} AND Status = 'Active'";

            var parameters = new List<object> { sampleId, testId };

            if (trialNumber.HasValue)
            {
                sql += " AND TrialNumber = {2}";
                parameters.Add(trialNumber.Value);
            }

            sql += " ORDER BY UploadDate DESC";

            var files = await _context.Database.SqlQueryRaw<FileUploadDto>(sql, parameters.ToArray()).ToListAsync();

            return new FileListDto
            {
                Files = files,
                TotalCount = files.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving files for Sample {SampleId}, Test {TestId}", sampleId, testId);
            return new FileListDto();
        }
    }

    public async Task<FileUploadDto?> GetFileAsync(int fileId)
    {
        try
        {
            var sql = @"
                SELECT Id, FileName, OriginalFileName, ContentType, FileSize, FilePath,
                       SampleId, TestId, TrialNumber, UploadDate, UploadedBy, Status
                FROM FileUploads 
                WHERE Id = {0} AND Status = 'Active'";

            var files = await _context.Database.SqlQueryRaw<FileUploadDto>(sql, fileId).ToListAsync();
            return files.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file {FileId}", fileId);
            return null;
        }
    }

    public async Task<(byte[] content, string fileName, string contentType)?> DownloadFileAsync(int fileId)
    {
        try
        {
            var fileInfo = await GetFileAsync(fileId);
            if (fileInfo == null || !File.Exists(fileInfo.FilePath))
            {
                return null;
            }

            var content = await File.ReadAllBytesAsync(fileInfo.FilePath);
            return (content, fileInfo.OriginalFileName, fileInfo.ContentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileId}", fileId);
            return null;
        }
    }

    public async Task<bool> DeleteFileAsync(int fileId, string deletedBy)
    {
        try
        {
            var fileInfo = await GetFileAsync(fileId);
            if (fileInfo == null)
            {
                return false;
            }

            // Mark as deleted in database
            var sql = @"
                UPDATE FileUploads 
                SET Status = 'Deleted', DeletedBy = {1}, DeletedDate = {2}
                WHERE Id = {0}";

            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, fileId, deletedBy, DateTime.UtcNow);

            // Optionally delete physical file (or move to archive)
            if (File.Exists(fileInfo.FilePath))
            {
                File.Delete(fileInfo.FilePath);
            }

            _logger.LogInformation("File deleted: {FileName} (ID: {FileId}) by {DeletedBy}", 
                fileInfo.OriginalFileName, fileId, deletedBy);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", fileId);
            return false;
        }
    }

    public async Task<List<FilePreviewDto>> GetFilePreviewsAsync(int sampleId, int testId)
    {
        try
        {
            var sql = @"
                SELECT Id, FileName, ContentType, FileSize, UploadDate, Status
                FROM FileUploads 
                WHERE SampleId = {0} AND TestId = {1} AND Status = 'Active'
                ORDER BY UploadDate DESC";

            var files = await _context.Database.SqlQueryRaw<FilePreviewDto>(sql, sampleId, testId).ToListAsync();

            // Set CanPreview based on content type
            foreach (var file in files)
            {
                file.CanPreview = CanPreviewFile(file.ContentType);
            }

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file previews for Sample {SampleId}, Test {TestId}", sampleId, testId);
            return new List<FilePreviewDto>();
        }
    }

    public async Task<(bool isValid, List<string> errors)> ValidateFileAsync(IFormFile file, FileUploadRequestDto request)
    {
        var errors = new List<string>();

        // Check if file is provided
        if (file == null || file.Length == 0)
        {
            errors.Add("No file provided or file is empty");
            return (false, errors);
        }

        // Check file size
        if (file.Length > MaxFileSize)
        {
            errors.Add($"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB");
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var supportedExtensions = await GetSupportedExtensionsAsync(request.TestId);
        
        if (!supportedExtensions.Contains(extension))
        {
            errors.Add($"File extension '{extension}' is not supported. Supported extensions: {string.Join(", ", supportedExtensions)}");
        }

        // Check filename for security
        if (!IsValidFileName(file.FileName))
        {
            errors.Add("Invalid filename. Filename contains invalid characters");
        }

        // Check if sample exists
        var sampleCount = await _context.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) FROM UsedLubeSamples WHERE ID = {0}", request.SampleId)
            .FirstAsync();
        if (sampleCount == 0)
        {
            errors.Add($"Sample {request.SampleId} does not exist");
        }

        // Check maximum files per trial
        var existingFiles = await GetFilesAsync(request.SampleId, request.TestId, request.TrialNumber);
        if (existingFiles.TotalCount >= MaxFilesPerTrial)
        {
            errors.Add($"Maximum number of files ({MaxFilesPerTrial}) already uploaded for this trial");
        }

        return (errors.Count == 0, errors);
    }

    public async Task<List<string>> GetSupportedExtensionsAsync(int testId)
    {
        try
        {
            // Get test name to determine supported extensions
            var testName = await GetTestNameAsync(testId);
            
            if (_supportedExtensions.ContainsKey(testName))
            {
                return _supportedExtensions[testName];
            }

            return _supportedExtensions["default"];
        }
        catch
        {
            return _supportedExtensions["default"];
        }
    }

    private async Task<string> GetTestNameAsync(int testId)
    {
        try
        {
            var sql = "SELECT testName FROM Test WHERE testID = {0}";
            var result = await _context.Database.SqlQueryRaw<string>(sql, testId).FirstOrDefaultAsync();
            return result ?? "default";
        }
        catch
        {
            return "default";
        }
    }

    private string GetUploadPath(int sampleId, int testId)
    {
        var baseUploadPath = _configuration["FileUpload:BasePath"] ?? "uploads";
        return Path.Combine(baseUploadPath, "samples", sampleId.ToString(), "tests", testId.ToString());
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var guid = Guid.NewGuid().ToString("N")[..8];
        
        return $"{nameWithoutExtension}_{timestamp}_{guid}{extension}";
    }

    private bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        // Check for invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.Any(c => invalidChars.Contains(c)))
            return false;

        // Check for reserved names
        var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
        if (reservedNames.Contains(nameWithoutExtension))
            return false;

        return true;
    }

    private bool CanPreviewFile(string contentType)
    {
        var previewableTypes = new[]
        {
            "text/plain",
            "text/csv",
            "application/pdf",
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/tiff"
        };

        return previewableTypes.Contains(contentType.ToLowerInvariant());
    }

    private async Task<int> SaveFileInfoAsync(FileUploadDto fileInfo)
    {
        var sql = @"
            INSERT INTO FileUploads 
            (FileName, OriginalFileName, ContentType, FileSize, FilePath, SampleId, TestId, TrialNumber, UploadDate, UploadedBy, Status)
            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10});
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var result = await _context.Database.SqlQueryRaw<int>(sql,
            fileInfo.FileName,
            fileInfo.OriginalFileName,
            fileInfo.ContentType,
            fileInfo.FileSize,
            fileInfo.FilePath,
            fileInfo.SampleId,
            fileInfo.TestId,
            fileInfo.TrialNumber,
            fileInfo.UploadDate,
            fileInfo.UploadedBy,
            fileInfo.Status).FirstAsync();

        return result;
    }
}