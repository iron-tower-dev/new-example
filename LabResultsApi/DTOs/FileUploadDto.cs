namespace LabResultsApi.DTOs;

public class FileUploadDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public int SampleId { get; set; }
    public int TestId { get; set; }
    public int TrialNumber { get; set; }
    public DateTime UploadDate { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class FileUploadRequestDto
{
    public int SampleId { get; set; }
    public int TestId { get; set; }
    public int TrialNumber { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class FileUploadResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public FileUploadDto? FileInfo { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class FileListDto
{
    public List<FileUploadDto> Files { get; set; } = new();
    public int TotalCount { get; set; }
}

public class FilePreviewDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool CanPreview { get; set; }
}