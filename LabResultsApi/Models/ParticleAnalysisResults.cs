using System.ComponentModel.DataAnnotations.Schema;

namespace LabResultsApi.Models;

/// <summary>
/// Represents Filter Residue test results (Test ID 180)
/// Combines particle analysis with calculation fields
/// </summary>
public class FilterResidueResult
{
    public int SampleId { get; set; }
    public short TestId { get; set; }
    public string? Narrative { get; set; }
    
    // Legacy fields (for old format compatibility)
    public int? Major { get; set; }
    public int? Minor { get; set; }
    public int? Trace { get; set; }
    
    // Calculation fields (stored in TestReadings)
    public double? SampleSize { get; set; }        // value1
    public double? ResidueWeight { get; set; }     // value3
    public double? FinalWeight { get; set; }       // value2 (calculated)
    
    // Overall severity from particle analysis
    public int OverallSeverity { get; set; }
    
    // Particle analysis data (separate tables)
    public List<ParticleType>? ParticleTypes { get; set; }
    public List<ParticleSubType>? ParticleSubTypes { get; set; }
    
    // Metadata
    public string? EntryId { get; set; }
    public DateTime? EntryDate { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// Represents Debris Identification test results (Test ID 240)
/// Combines particle analysis with volume selection
/// </summary>
public class DebrisIdentificationResult
{
    public int SampleId { get; set; }
    public short TestId { get; set; }
    public string? Narrative { get; set; }
    
    // Legacy fields (for old format compatibility)
    public int? Major { get; set; }
    public int? Minor { get; set; }
    public int? Trace { get; set; }
    
    // Volume of oil used selection (stored in TestReadings.ID3)
    public string? VolumeOfOilUsed { get; set; }
    public string? CustomVolume { get; set; }  // When "custom" is selected
    
    // Overall severity from particle analysis
    public int OverallSeverity { get; set; }
    
    // Particle analysis data (separate tables)
    public List<ParticleType>? ParticleTypes { get; set; }
    public List<ParticleSubType>? ParticleSubTypes { get; set; }
    
    // Metadata
    public string? EntryId { get; set; }
    public DateTime? EntryDate { get; set; }
    public string? Status { get; set; }
}
