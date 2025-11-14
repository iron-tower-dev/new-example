using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("Comments")]
public class Comment
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }
    
    [Column("area")]
    [MaxLength(4)]
    public string Area { get; set; } = string.Empty;
    
    [Column("type")]
    [MaxLength(5)]
    public string? Type { get; set; }
    
    [Column("remark")]
    [MaxLength(80)]
    public string Remark { get; set; } = string.Empty;
}