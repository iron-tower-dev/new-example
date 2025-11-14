using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

[Keyless]
[Table("TestReadings")]
public class TestReading
{
    [Column("sampleID")]
    public int SampleId { get; set; }
    
    [Column("testID")]
    public int TestId { get; set; }
    
    [Column("trialNumber")]
    public int TrialNumber { get; set; }
    
    [Column("value1")]
    public double? Value1 { get; set; }
    
    [Column("value2")]
    public double? Value2 { get; set; }
    
    [Column("value3")]
    public double? Value3 { get; set; }
    
    [Column("trialCalc")]
    public double? TrialCalc { get; set; }
    
    [Column("ID1")]
    public string? Id1 { get; set; }
    
    [Column("ID2")]
    public string? Id2 { get; set; }
    
    [Column("ID3")]
    public string? Id3 { get; set; }
    
    [Column("trialComplete")]
    public bool? TrialComplete { get; set; }
    
    [Column("status")]
    public string? Status { get; set; }
    
    [Column("schedType")]
    public string? SchedType { get; set; }
    
    [Column("entryID")]
    public string? EntryId { get; set; }
    
    [Column("validateID")]
    public string? ValidateId { get; set; }
    
    [Column("entryDate")]
    public DateTime? EntryDate { get; set; }
    
    [Column("valiDate")]
    public DateTime? ValidateDate { get; set; }
    
    [Column("MainComments")]
    public string? MainComments { get; set; }
}