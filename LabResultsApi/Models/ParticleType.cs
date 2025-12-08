using System.ComponentModel.DataAnnotations;
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
    [MaxLength(20)]
    public string? Status { get; set; }
    
    [Column("Comments")]
    [MaxLength(500)]
    public string? Comments { get; set; }
}
