using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("InspectFilter")]
public class InspectFilter
{
    [Column("ID")]
    public int? Id { get; set; }
    
    [Column("testID")]
    public short? TestId { get; set; }
    
    [Column("narrative")]
    [MaxLength(4000)]
    public string? Narrative { get; set; }
    
    [Column("major")]
    public int? Major { get; set; }
    
    [Column("minor")]
    public int? Minor { get; set; }
    
    [Column("trace")]
    public int? Trace { get; set; }
}