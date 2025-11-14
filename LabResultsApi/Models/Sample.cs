using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("UsedLubeSamples")]
public class Sample
{
    [Key]
    public int Id { get; set; }
    
    [Column("tagNumber")]
    public string TagNumber { get; set; } = string.Empty;
    
    [Column("component")]
    public string Component { get; set; } = string.Empty;
    
    [Column("location")]
    public string Location { get; set; } = string.Empty;
    
    [Column("lubeType")]
    public string LubeType { get; set; } = string.Empty;
    
    [Column("sampleDate")]
    public DateTime SampleDate { get; set; }
    
    [Column("status")]
    public string Status { get; set; } = string.Empty;
    
    [Column("qualityClass")]
    public string? QualityClass { get; set; }
}