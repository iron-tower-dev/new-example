using LabResultsApi.Models;

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

    public static EmissionSpectroscopyDto ToDto(EmissionSpectroscopy entity)
    {
        return new EmissionSpectroscopyDto
        {
            Id = entity.Id ?? 0,
            TestId = entity.TestId,
            TrialNum = entity.TrialNum,
            Na = entity.Na,
            Cr = entity.Cr,
            Sn = entity.Sn,
            Si = entity.Si,
            Mo = entity.Mo,
            Ca = entity.Ca,
            Al = entity.Al,
            Ba = entity.Ba,
            Mg = entity.Mg,
            Ni = entity.Ni,
            Mn = entity.Mn,
            Zn = entity.Zn,
            P = entity.P,
            Ag = entity.Ag,
            Pb = entity.Pb,
            H = entity.H,
            B = entity.B,
            Cu = entity.Cu,
            Fe = entity.Fe,
            TrialDate = entity.TrialDate,
            Status = null,
            ScheduleFerrography = false
        };
    }

    public static List<EmissionSpectroscopyDto> ToDtoList(List<EmissionSpectroscopy> entities)
    {
        return entities.Select(ToDto).ToList();
    }
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

    public EmissionSpectroscopy ToEntity()
    {
        return new EmissionSpectroscopy
        {
            Id = this.Id,
            TestId = this.TestId,
            TrialNum = this.TrialNum,
            Na = this.Na,
            Cr = this.Cr,
            Sn = this.Sn,
            Si = this.Si,
            Mo = this.Mo,
            Ca = this.Ca,
            Al = this.Al,
            Ba = this.Ba,
            Mg = this.Mg,
            Ni = this.Ni,
            Mn = this.Mn,
            Zn = this.Zn,
            P = this.P,
            Ag = this.Ag,
            Pb = this.Pb,
            H = this.H,
            B = this.B,
            Cu = this.Cu,
            Fe = this.Fe
        };
    }
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

    public void UpdateEntity(EmissionSpectroscopy entity)
    {
        entity.Na = this.Na;
        entity.Cr = this.Cr;
        entity.Sn = this.Sn;
        entity.Si = this.Si;
        entity.Mo = this.Mo;
        entity.Ca = this.Ca;
        entity.Al = this.Al;
        entity.Ba = this.Ba;
        entity.Mg = this.Mg;
        entity.Ni = this.Ni;
        entity.Mn = this.Mn;
        entity.Zn = this.Zn;
        entity.P = this.P;
        entity.Ag = this.Ag;
        entity.Pb = this.Pb;
        entity.H = this.H;
        entity.B = this.B;
        entity.Cu = this.Cu;
        entity.Fe = this.Fe;
    }
}
