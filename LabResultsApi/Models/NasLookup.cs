using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Table("NAS_lookup")]
public class NasLookup
{
    [Key]
    public int Id { get; set; }
    public int? Channel { get; set; }
    public int? ValLo { get; set; }
    public int? ValHi { get; set; }
    public int? NAS { get; set; }
}