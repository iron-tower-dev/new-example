using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("TestStandMapping")]
public class TestStandMapping
{
    [Key]
    public short TestStandId { get; set; }
    
    [Key]
    public int TestId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public TestStand? TestStand { get; set; }
    public Test? Test { get; set; }
}