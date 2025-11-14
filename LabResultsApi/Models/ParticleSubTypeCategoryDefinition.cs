using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("ParticleSubTypeCategoryDefinition")]
public class ParticleSubTypeCategoryDefinition
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }
    
    [Column("Description")]
    [MaxLength(50)]
    public string Description { get; set; } = string.Empty;
    
    [Column("Active")]
    [MaxLength(1)]
    public string? Active { get; set; }
    
    [Column("SortOrder")]
    public int? SortOrder { get; set; }
}