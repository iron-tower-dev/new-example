using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Keyless]
[Table("Lube_Sampling_Point")]
public class LubeSamplingPoint
{
    [Column("tagNumber")]
    [MaxLength(22)]
    public string? TagNumber { get; set; }
    
    [Column("component")]
    [MaxLength(3)]
    public string? Component { get; set; }
    
    [Column("location")]
    [MaxLength(3)]
    public string? Location { get; set; }
    
    [Column("lubeClassItemNumber")]
    [MaxLength(10)]
    public string? LubeClassItemNumber { get; set; }
    
    [Column("lubeQuantityRequired")]
    public double? LubeQuantityRequired { get; set; }
    
    [Column("lubeUnitsOfMeasure")]
    [MaxLength(3)]
    public string? LubeUnitsOfMeasure { get; set; }
    
    [Column("testCategory")]
    [MaxLength(1)]
    public string? TestCategory { get; set; }
    
    [Column("qualityClass")]
    [MaxLength(6)]
    public string? QualityClass { get; set; }
    
    [Column("pricingPackageId")]
    public short? PricingPackageId { get; set; }
    
    [Column("testPricesId")]
    public short? TestPricesId { get; set; }
    
    [Column("lastSampleDate")]
    public DateTime? LastSampleDate { get; set; }
    
    [Column("changeTaskNumber")]
    [MaxLength(6)]
    public string? ChangeTaskNumber { get; set; }
    
    [Column("changeIntervalType")]
    [MaxLength(1)]
    public string? ChangeIntervalType { get; set; }
    
    [Column("changeIntervalNumber")]
    public byte? ChangeIntervalNumber { get; set; }
    
    [Column("lastChangeDate")]
    public DateTime? LastChangeDate { get; set; }
    
    [Column("inProgram")]
    public bool? InProgram { get; set; }
    
    [Column("testsScheduled")]
    public bool? TestsScheduled { get; set; }
    
    [Column("applid")]
    public int? ApplId { get; set; }
    
    [Column("material_info")]
    [MaxLength(500)]
    public string? MaterialInfo { get; set; }
}
