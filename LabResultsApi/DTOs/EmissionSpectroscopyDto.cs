namespace LabResultsApi.DTOs;

public class EmissionSpectroscopyDto
{
    public int Id { get; set; }
    public int TestId { get; set; }
    public int TrialNum { get; set; }
    public double? Na { get; set; }
    public double? Cr { get; set; }
    public double? Sn { get; set; }
    public double? Si { get; set; }
    public double? Mo { get; set; }
    public double? Ca { get; set; }
    public double? Al { get; set; }
    public double? Ba { get; set; }
    public double? Mg { get; set; }
    public double? Ni { get; set; }
    public double? Mn { get; set; }
    public double? Zn { get; set; }
    public double? P { get; set; }
    public double? Ag { get; set; }
    public double? Pb { get; set; }
    public double? H { get; set; }
    public double? B { get; set; }
    public double? Cu { get; set; }
    public double? Fe { get; set; }
    public DateTime? TrialDate { get; set; }
    public string? Status { get; set; }
    public bool ScheduleFerrography { get; set; } // For Ferrography scheduling option
}

public class EmissionSpectroscopyCreateDto
{
    public int Id { get; set; }
    public int TestId { get; set; }
    public int TrialNum { get; set; }
    public double? Na { get; set; }
    public double? Cr { get; set; }
    public double? Sn { get; set; }
    public double? Si { get; set; }
    public double? Mo { get; set; }
    public double? Ca { get; set; }
    public double? Al { get; set; }
    public double? Ba { get; set; }
    public double? Mg { get; set; }
    public double? Ni { get; set; }
    public double? Mn { get; set; }
    public double? Zn { get; set; }
    public double? P { get; set; }
    public double? Ag { get; set; }
    public double? Pb { get; set; }
    public double? H { get; set; }
    public double? B { get; set; }
    public double? Cu { get; set; }
    public double? Fe { get; set; }
    public string? Status { get; set; }
    public bool ScheduleFerrography { get; set; }
}

public class EmissionSpectroscopyUpdateDto
{
    public double? Na { get; set; }
    public double? Cr { get; set; }
    public double? Sn { get; set; }
    public double? Si { get; set; }
    public double? Mo { get; set; }
    public double? Ca { get; set; }
    public double? Al { get; set; }
    public double? Ba { get; set; }
    public double? Mg { get; set; }
    public double? Ni { get; set; }
    public double? Mn { get; set; }
    public double? Zn { get; set; }
    public double? P { get; set; }
    public double? Ag { get; set; }
    public double? Pb { get; set; }
    public double? H { get; set; }
    public double? B { get; set; }
    public double? Cu { get; set; }
    public double? Fe { get; set; }
    public string? Status { get; set; }
    public bool ScheduleFerrography { get; set; }
}