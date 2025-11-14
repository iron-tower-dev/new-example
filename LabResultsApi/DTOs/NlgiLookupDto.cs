namespace LabResultsApi.DTOs;

public class NlgiLookupDto
{
    public int LowerValue { get; set; }
    public int UpperValue { get; set; }
    public string NLGIValue { get; set; } = string.Empty;
}