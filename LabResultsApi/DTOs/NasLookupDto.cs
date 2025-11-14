namespace LabResultsApi.DTOs;

public class NasLookupDto
{
    public int Channel { get; set; }
    public int ValLo { get; set; }
    public int ValHi { get; set; }
    public int NAS { get; set; }
}

public class NasLookupRequest
{
    public Dictionary<int, int> ParticleCounts { get; set; } = new();
}

public class NasLookupResult
{
    public int HighestNAS { get; set; }
    public Dictionary<int, int> ChannelNASValues { get; set; } = new();
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ParticleCountDto
{
    public int SampleId { get; set; }
    public int TestId { get; set; }
    public int TrialNumber { get; set; }
    public int? Count5to10 { get; set; }
    public int? Count10to15 { get; set; }
    public int? Count15to25 { get; set; }
    public int? Count25to50 { get; set; }
    public int? Count50to100 { get; set; }
    public int? CountOver100 { get; set; }
    public int? CalculatedNAS { get; set; }
    public string? Status { get; set; }
    public DateTime? EntryDate { get; set; }
    public string? EntryId { get; set; }
}