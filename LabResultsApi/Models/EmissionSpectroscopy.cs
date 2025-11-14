using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Keyless]
[Table("EmSpectro")]
public class EmissionSpectroscopy
{
    [Column("ID")]
    public int? Id { get; set; }
    
    [Column("testID")]
    public int TestId { get; set; }
    
    [Column("trialNum")]
    public int TrialNum { get; set; }
    
    [Column("Na")]
    public double? Na { get; set; }
    
    [Column("Cr")]
    public double? Cr { get; set; }
    
    [Column("Sn")]
    public double? Sn { get; set; }
    
    [Column("Si")]
    public double? Si { get; set; }
    
    [Column("Mo")]
    public double? Mo { get; set; }
    
    [Column("Ca")]
    public double? Ca { get; set; }
    
    [Column("Al")]
    public double? Al { get; set; }
    
    [Column("Ba")]
    public double? Ba { get; set; }
    
    [Column("Mg")]
    public double? Mg { get; set; }
    
    [Column("Ni")]
    public double? Ni { get; set; }
    
    [Column("Mn")]
    public double? Mn { get; set; }
    
    [Column("Zn")]
    public double? Zn { get; set; }
    
    [Column("P")]
    public double? P { get; set; }
    
    [Column("Ag")]
    public double? Ag { get; set; }
    
    [Column("Pb")]
    public double? Pb { get; set; }
    
    [Column("H")]
    public double? H { get; set; }
    
    [Column("B")]
    public double? B { get; set; }
    
    [Column("Cu")]
    public double? Cu { get; set; }
    
    [Column("Fe")]
    public double? Fe { get; set; }
    
    [Column("trialDate")]
    public DateTime? TrialDate { get; set; }
    
    [Column("Sb")]
    public double? Sb { get; set; }
    
    public string? Status { get; set; }
}