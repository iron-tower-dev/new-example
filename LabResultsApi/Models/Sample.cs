using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LabResultsApi.Models;

[Keyless]
[Table("UsedLubeSamples")]
public class Sample
{
    [Column("ID")]
    public int Id { get; set; }
    
    [Column("tagNumber")]
    [MaxLength(22)]
    public string? TagNumber { get; set; }
    
    [Column("component")]
    [MaxLength(3)]
    public string? Component { get; set; }
    
    [Column("location")]
    [MaxLength(3)]
    public string? Location { get; set; }
    
    [Column("lubeType")]
    [MaxLength(30)]
    public string? LubeType { get; set; }
    
    [Column("woNumber")]
    [MaxLength(16)]
    public string? WoNumber { get; set; }
    
    [Column("trackingNumber")]
    [MaxLength(12)]
    public string? TrackingNumber { get; set; }
    
    [Column("warehouseId")]
    [MaxLength(10)]
    public string? WarehouseId { get; set; }
    
    [Column("batchNumber")]
    [MaxLength(30)]
    public string? BatchNumber { get; set; }
    
    [Column("classItem")]
    [MaxLength(10)]
    public string? ClassItem { get; set; }
    
    [Column("sampleDate")]
    public DateTime? SampleDate { get; set; }
    
    [Column("receivedOn")]
    public DateTime? ReceivedOn { get; set; }
    
    [Column("sampledBy")]
    [MaxLength(50)]
    public string? SampledBy { get; set; }
    
    [Column("status")]
    public short? Status { get; set; }
    
    [Column("cmptSelectFlag")]
    public byte? CmptSelectFlag { get; set; }
    
    [Column("newUsedFlag")]
    public byte? NewUsedFlag { get; set; }
    
    [Column("entryId")]
    [MaxLength(5)]
    public string? EntryId { get; set; }
    
    [Column("validateId")]
    [MaxLength(5)]
    public string? ValidateId { get; set; }
    
    [Column("testPricesId")]
    public short? TestPricesId { get; set; }
    
    [Column("pricingPackageId")]
    public short? PricingPackageId { get; set; }
    
    [Column("evaluation")]
    public byte? Evaluation { get; set; }
    
    [Column("siteId")]
    public int? SiteId { get; set; }
    
    [Column("results_review_date")]
    public DateTime? ResultsReviewDate { get; set; }
    
    [Column("results_avail_date")]
    public DateTime? ResultsAvailDate { get; set; }
    
    [Column("results_reviewId")]
    [MaxLength(5)]
    public string? ResultsReviewId { get; set; }
    
    [Column("storeSource")]
    [MaxLength(100)]
    public string? StoreSource { get; set; }
    
    [Column("schedule")]
    [MaxLength(1)]
    public string? Schedule { get; set; }
    
    [Column("returnedDate")]
    public DateTime? ReturnedDate { get; set; }
}
