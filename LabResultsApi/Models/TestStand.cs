using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("TestStand")]
public class TestStand
{
    [Key]
    public short Id { get; set; }
    
    [MaxLength(50)]
    public string? Name { get; set; }
}