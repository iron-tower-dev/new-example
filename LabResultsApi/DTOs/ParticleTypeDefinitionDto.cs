namespace LabResultsApi.DTOs;

public class ParticleTypeDefinitionDto
{
    public int Id { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public string? Image1 { get; set; }
    public string? Image2 { get; set; }
    public bool Active { get; set; }
    public int SortOrder { get; set; }
}