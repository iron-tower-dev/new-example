using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("Test")]
public class Test
{
    [Column("ID")]
    public short? Id { get; set; }
    
    [Column("name")]
    [MaxLength(40)]
    public string? Name { get; set; }
    
    [Column("testStandID")]
    public short? TestStandId { get; set; }
    
    [Column("sampleVolumeRequired")]
    public short? SampleVolumeRequired { get; set; }
    
    [Column("exclude")]
    [MaxLength(1)]
    public string? Exclude { get; set; }
    
    [Column("abbrev")]
    [MaxLength(12)]
    public string? Abbrev { get; set; }
    
    [Column("displayGroupId")]
    public short? DisplayGroupId { get; set; }
    
    [Column("groupname")]
    [MaxLength(30)]
    public string? GroupName { get; set; }
    
    [Column("Lab")]
    public bool? Lab { get; set; }
    
    [Column("Schedule")]
    public bool? Schedule { get; set; }
    
    [Column("ShortAbbrev")]
    [MaxLength(6)]
    public string? ShortAbbrev { get; set; }
}
