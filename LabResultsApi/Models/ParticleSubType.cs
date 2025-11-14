using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("ParticleSubType")]
public class ParticleSubType
{
    [Column("SampleID")]
    public int SampleId { get; set; }
    
    [Column("testID")]
    public short TestId { get; set; }
    
    [Column("ParticleTypeDefinitionID")]
    public int ParticleTypeDefinitionId { get; set; }
    
    [Column("ParticleSubTypeCategoryID")]
    public int ParticleSubTypeCategoryId { get; set; }
    
    [Column("Value")]
    public int? Value { get; set; }
}