namespace LabResultsApi.DTOs
{
    public class ParticleAnalysisDto
    {
        public int SampleId { get; set; }
        public int TestId { get; set; }
        public List<ParticleTypeResult> ParticleTypes { get; set; } = new();
        public string? Comments { get; set; }
        public string EntryId { get; set; } = string.Empty;
        public int OverallSeverity { get; set; }
        public bool MediaReady { get; set; }
    }

    public class FerrographyResultDto
    {
        public int SampleId { get; set; }
        public int TestId { get; set; }
        public string DilutionFactor { get; set; } = string.Empty;
        public List<ParticleTypeResult> ParticleTypes { get; set; } = new();
        public string? Comments { get; set; }
        public string EntryId { get; set; } = string.Empty;
        public string Status { get; set; } = "X";
    }

    public class ParticleTypeResult
    {
        public int ParticleTypeDefinitionId { get; set; }
        public string ParticleTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public int Heat { get; set; }
        public int Concentration { get; set; }
        public int Size { get; set; }
        public int Color { get; set; }
        public int Texture { get; set; }
        public int Composition { get; set; }
        public int Severity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }

    public class InspectFilterResultDto
    {
        public int SampleId { get; set; }
        public int TestId { get; set; }
        public string? Narrative { get; set; }
        public string? Major { get; set; }
        public string? Minor { get; set; }
        public string? Trace { get; set; }
        public List<ParticleTypeResult> ParticleTypes { get; set; } = new();
        public int OverallSeverity { get; set; }
        public bool MediaReady { get; set; }
        public string EntryId { get; set; } = string.Empty;
    }

    public class ParticleAnalysisRequest
    {
        public int SampleId { get; set; }
        public int TestId { get; set; }
        public List<ParticleTypeResult> ParticleTypes { get; set; } = new();
        public string? Comments { get; set; }
        public string EntryId { get; set; } = string.Empty;
        public string? DilutionFactor { get; set; } // For Ferrography
        public string? Narrative { get; set; } // For Inspect Filter
        public string? Major { get; set; } // For Inspect Filter
        public string? Minor { get; set; } // For Inspect Filter
        public string? Trace { get; set; } // For Inspect Filter
    }
}