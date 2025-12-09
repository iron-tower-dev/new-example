using LabResultsApi.Models;

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

    public static SampleDto ToDto(Sample entity)
    {
        return new SampleDto
        {
            Id = entity.Id,
            TagNumber = entity.TagNumber,
            Component = entity.Component,
            Location = entity.Location,
            LubeType = entity.LubeType,
            WoNumber = entity.WoNumber,
            TrackingNumber = entity.TrackingNumber,
            WarehouseId = entity.WarehouseId,
            BatchNumber = entity.BatchNumber,
            ClassItem = entity.ClassItem,
            SampleDate = entity.SampleDate,
            ReceivedOn = entity.ReceivedOn,
            SampledBy = entity.SampledBy,
            Status = entity.Status,
            CmptSelectFlag = entity.CmptSelectFlag,
            NewUsedFlag = entity.NewUsedFlag,
            EntryId = entity.EntryId,
            ValidateId = entity.ValidateId,
            TestPricesId = entity.TestPricesId,
            PricingPackageId = entity.PricingPackageId,
            Evaluation = entity.Evaluation,
            SiteId = entity.SiteId,
            ResultsReviewDate = entity.ResultsReviewDate,
            ResultsAvailDate = entity.ResultsAvailDate,
            ResultsReviewId = entity.ResultsReviewId,
            StoreSource = entity.StoreSource,
            Schedule = entity.Schedule,
            ReturnedDate = entity.ReturnedDate,
            QualityClass = entity.QualityClass
        };
    }

    public static Sample ToEntity(SampleDto dto)
    {
        return new Sample
        {
            Id = dto.Id,
            TagNumber = dto.TagNumber,
            Component = dto.Component,
            Location = dto.Location,
            LubeType = dto.LubeType,
            WoNumber = dto.WoNumber,
            TrackingNumber = dto.TrackingNumber,
            WarehouseId = dto.WarehouseId,
            BatchNumber = dto.BatchNumber,
            ClassItem = dto.ClassItem,
            SampleDate = dto.SampleDate,
            ReceivedOn = dto.ReceivedOn,
            SampledBy = dto.SampledBy,
            Status = dto.Status,
            CmptSelectFlag = dto.CmptSelectFlag,
            NewUsedFlag = dto.NewUsedFlag,
            EntryId = dto.EntryId,
            ValidateId = dto.ValidateId,
            TestPricesId = dto.TestPricesId,
            PricingPackageId = dto.PricingPackageId,
            Evaluation = dto.Evaluation,
            SiteId = dto.SiteId,
            ResultsReviewDate = dto.ResultsReviewDate,
            ResultsAvailDate = dto.ResultsAvailDate,
            ResultsReviewId = dto.ResultsReviewId,
            StoreSource = dto.StoreSource,
            Schedule = dto.Schedule,
            ReturnedDate = dto.ReturnedDate,
            QualityClass = dto.QualityClass
        };
    }
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