namespace LabResultsApi.DTOs
{
    public class SaveInspectFilterRequest
    {
        public int SampleId { get; set; }
        public int TestId { get; set; }
        public string? EntryId { get; set; }
        public string? Comments { get; set; }
        public List<ParticleTypeResult> ParticleTypes { get; set; } = new();
    }

    public class SaveFerrographyRequest
    {
        public int SampleId { get; set; }
        public int TestId { get; set; }
        public string? EntryId { get; set; }
        public string? Comments { get; set; }
        public string? DilutionFactor { get; set; }
        public List<ParticleTypeResult> ParticleTypes { get; set; } = new();
    }
}