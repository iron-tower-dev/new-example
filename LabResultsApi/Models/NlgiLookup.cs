using System.ComponentModel.DataAnnotations;

namespace LabResultsApi.Models;

public class NlgiLookup
{
    [Key]
    public int ID { get; set; }
    public int? LowerValue { get; set; }
    public int? UpperValue { get; set; }
    public string? NLGIValue { get; set; }
}
