using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("ParticleTypeDefinition")]
public class ParticleTypeDefinition
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }
    
    [Column("Type")]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;
    
    [Column("Description")]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Column("Image1")]
    [MaxLength(50)]
    public string Image1 { get; set; } = string.Empty;
    
    [Column("Image2")]
    [MaxLength(50)]
    public string Image2 { get; set; } = string.Empty;
    
    [Column("Active")]
    [MaxLength(1)]
    public string? Active { get; set; }
    
    [Column("SortOrder")]
    public int? SortOrder { get; set; }
}