using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("ParticleType")]
public class ParticleType
{
    [Column("SampleID")]
    public int SampleId { get; set; }
    
    [Column("testID")]
    public short TestId { get; set; }
    
    [Column("ParticleTypeDefinitionID")]
    public int ParticleTypeDefinitionId { get; set; }
    
    [Column("Status")]
    public string? Status { get; set; }
    
    [Column("Comments")]
    public string? Comments { get; set; }
}