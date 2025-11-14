using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("Test")]
public class Test
{
    [Key]
    [Column("testID")]
    public int TestId { get; set; }
    
    [Column("testName")]
    public string TestName { get; set; } = string.Empty;
    
    [Column("testDescription")]
    public string? TestDescription { get; set; }
    
    [Column("active")]
    public bool Active { get; set; }
}