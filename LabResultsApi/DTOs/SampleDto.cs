namespace LabResultsApi.DTOs;

public class SampleDto
{
    public int Id { get; set; }
    public string TagNumber { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string LubeType { get; set; } = string.Empty;
    public string? QualityClass { get; set; }
    public DateTime SampleDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SampleHistoryDto
{
    public int SampleId { get; set; }
    public string TagNumber { get; set; } = string.Empty;
    public DateTime SampleDate { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? EntryDate { get; set; }
}

public class SampleFilterDto
{
    public string? TagNumber { get; set; }
    public string? Component { get; set; }
    public string? Location { get; set; }
    public string? LubeType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Status { get; set; }
}

public class ExtendedHistoryResultDto
{
    public List<SampleHistoryDto> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class HistoricalResultSummaryDto
{
    public int SampleId { get; set; }
    public string TagNumber { get; set; } = string.Empty;
    public DateTime SampleDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? EntryDate { get; set; }
    public string TestName { get; set; } = string.Empty;
}