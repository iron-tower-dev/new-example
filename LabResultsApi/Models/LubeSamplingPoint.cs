using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("Lube_Sampling_Point")]
public class LubeSamplingPoint
{
    [Key]
    public int Id { get; set; }
    
    [Column("tagNumber")]
    [MaxLength(50)]
    public string? TagNumber { get; set; }
    
    [Column("component")]
    [MaxLength(10)]
    public string? Component { get; set; }
    
    [Column("location")]
    [MaxLength(10)]
    public string? Location { get; set; }
    
    [Column("lubeClassItemNumber")]
    public string? LubeClassItemNumber { get; set; }
    
    [Column("lubeQuantityRequired")]
    public decimal? LubeQuantityRequired { get; set; }
    
    [Column("lubeUnitsOfMeasure")]
    public string? LubeUnitsOfMeasure { get; set; }
    
    [Column("testCategory")]
    public string? TestCategory { get; set; }
    
    [Column("qualityClass")]
    [MaxLength(10)]
    public string? QualityClass { get; set; }
    
    [Column("pricingPackageId")]
    public int? PricingPackageId { get; set; }
    
    [Column("testPricesId")]
    public int? TestPricesId { get; set; }
    
    [Column("lastSampleDate")]
    public DateTime? LastSampleDate { get; set; }
    
    [Column("changeTaskNumber")]
    public string? ChangeTaskNumber { get; set; }
    
    [Column("changeIntervalType")]
    public string? ChangeIntervalType { get; set; }
    
    [Column("changeIntervalNumber")]
    public int? ChangeIntervalNumber { get; set; }
    
    [Column("lastChangeDate")]
    public DateTime? LastChangeDate { get; set; }
    
    [Column("inProgram")]
    public bool? InProgram { get; set; }
    
    [Column("testsScheduled")]
    public int? TestsScheduled { get; set; }
    
    [Column("applid")]
    public int? ApplId { get; set; }
    
    [Column("material_info")]
    public string? MaterialInfo { get; set; }
}
