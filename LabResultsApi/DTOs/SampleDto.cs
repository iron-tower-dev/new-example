namespace LabResultsApi.DTOs;

public class SampleDto
{
    public int Id { get; set; }
    public string? TagNumber { get; set; }
    public string? Component { get; set; }
    public string? Location { get; set; }
    public string? LubeType { get; set; }
    public string? WoNumber { get; set; }
    public string? TrackingNumber { get; set; }
    public string? WarehouseId { get; set; }
    public string? BatchNumber { get; set; }
    public string? ClassItem { get; set; }
    public DateTime? SampleDate { get; set; }
    public DateTime? ReceivedOn { get; set; }
    public string? SampledBy { get; set; }
    public short? Status { get; set; }
    public byte? CmptSelectFlag { get; set; }
    public byte? NewUsedFlag { get; set; }
    public string? EntryId { get; set; }
    public string? ValidateId { get; set; }
    public short? TestPricesId { get; set; }
    public short? PricingPackageId { get; set; }
    public byte? Evaluation { get; set; }
    public int? SiteId { get; set; }
    public DateTime? ResultsReviewDate { get; set; }
    public DateTime? ResultsAvailDate { get; set; }
    public string? ResultsReviewId { get; set; }
    public string? StoreSource { get; set; }
    public string? Schedule { get; set; }
    public DateTime? ReturnedDate { get; set; }
    
    // From Lube_Sampling_Point JOIN
    public string? QualityClass { get; set; }
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